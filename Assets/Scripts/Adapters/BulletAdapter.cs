using UnityEngine;

/// <summary>
/// CAPA DE ADAPTADOR — Ciclo de vida del proyectil (bala).
///
/// Fix: se fuerza CollisionDetectionMode.ContinuousDynamic en Awake para evitar
/// tunneling cuando la bala viaja a alta velocidad (impulso > 100).
///
/// CONFIGURACIÓN EN EL INSPECTOR:
///   • lifeTime              → Segundos antes de autodestrucción (default 3).
///   • danio                 → Daño que inflige al enemigo (default 25).
///   • efectoImpactoPrefab   → ParticleSystem de impacto (opcional).
///   • audioImpacto          → AudioClip de impacto (opcional).
///
/// REQUISITOS:
///   • El prefab debe tener Rigidbody ([RequireComponent]).
///   • Collider con "Is Trigger" DESACTIVADO (colisión física, no trigger).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BulletAdapter : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Ciclo de Vida")]
    [SerializeField] private float lifeTime = 3f;

    [Header("Daño")]
    [Tooltip("Daño que esta bala inflige a los enemigos al impactar.")]
    [SerializeField] private float danio = 25f;

    [Header("Efectos de Impacto (opcionales)")]
    [SerializeField] private GameObject efectoImpactoPrefab;
    [SerializeField] private AudioClip  audioImpacto;

    // ── Privados ──────────────────────────────────────────────────────────────
    private bool _yaImpacto;

    // ── Ciclo de vida Unity ───────────────────────────────────────────────────

    private void Awake()
    {
        // FIX TUNNELING: forzar detección de colisión continua en el Rigidbody.
        // Sin esto, balas rápidas (impulso > ~100) atraviesan colliders finos
        // porque en un frame están antes del objeto y en el siguiente ya pasaron.
        // ContinuousDynamic es la opción correcta para proyectiles rápidos contra
        // objetos estáticos Y dinámicos (como el enemigo con NavMeshAgent).
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
        Debug.Log($"[BulletAdapter] Proyectil instanciado. Daño: {danio} | LifeTime: {lifeTime}s");
    }

    // ── Colisión física ───────────────────────────────────────────────────────

    private void OnCollisionEnter(Collision collision)
    {
        if (_yaImpacto) return;
        _yaImpacto = true;

        ContactPoint contacto = collision.GetContact(0);

        Debug.Log($"[BulletAdapter] Impacto con '{collision.gameObject.name}' en {contacto.point}.");

        // ── Daño al enemigo ───────────────────────────────────────────────────
        // Busca EnemigoAdapter en el propio objeto o en su raíz (por si la bala
        // impacta con un hijo del enemigo, ej. un sub-collider de hitbox)
        EnemigoAdapter enemigo = collision.gameObject.GetComponent<EnemigoAdapter>();
        if (enemigo == null)
            enemigo = collision.gameObject.GetComponentInParent<EnemigoAdapter>();

        if (enemigo != null)
        {
            enemigo.RecibirDanio(danio);
            Debug.Log($"[BulletAdapter] ¡Impacto en enemigo '{collision.gameObject.name}'! Daño: {danio}");
        }

        // ── Efectos ───────────────────────────────────────────────────────────
        if (efectoImpactoPrefab != null)
            Instantiate(efectoImpactoPrefab, contacto.point, Quaternion.LookRotation(contacto.normal));

        if (audioImpacto != null)
            AudioSource.PlayClipAtPoint(audioImpacto, contacto.point);

        Destroy(gameObject);
    }
}
