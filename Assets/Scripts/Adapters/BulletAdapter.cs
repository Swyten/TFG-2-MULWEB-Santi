using UnityEngine;

// Controla el ciclo de vida del proyectil (bala)
// Necesita Rigidbody en el prefab (por RequireComponent)
// El collider debe tener "Is Trigger" DESACTIVADO para que use colisión física, no trigger
[RequireComponent(typeof(Rigidbody))]
public class BulletAdapter : MonoBehaviour
{
    [Header("Ciclo de Vida")]
    [SerializeField] private float lifeTime = 3f;

    [Header("Daño")]
    [Tooltip("Daño que esta bala inflige a los enemigos al impactar.")]
    [SerializeField] private float danio = 25f;

    [Header("Efectos de Impacto (opcionales)")]
    [SerializeField] private GameObject efectoImpactoPrefab;
    [SerializeField] private AudioClip  audioImpacto;

    private bool _yaImpacto; // flag para que no procese la colisión más de una vez

    private void Awake()
    {
        // sin esto las balas rápidas atraviesan los colliders porque en un frame están antes
        // del objeto y en el siguiente ya pasaron (tunneling)
        // ContinuousDynamic funciona contra objetos estáticos Y dinámicos (como el enemigo con NavMeshAgent)
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime); // se autodestruye si no impacta nada
        Debug.Log($"[BulletAdapter] Proyectil instanciado. Daño: {danio} | LifeTime: {lifeTime}s");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_yaImpacto) return; // evito que procese el impacto varias veces
        _yaImpacto = true;

        ContactPoint contacto = collision.GetContact(0);

        Debug.Log($"[BulletAdapter] Impacto con '{collision.gameObject.name}' en {contacto.point}.");

        // busco el EnemigoAdapter en el objeto o en su padre por si la bala impacta en un sub-collider del enemigo
        EnemigoAdapter enemigo = collision.gameObject.GetComponent<EnemigoAdapter>();
        if (enemigo == null)
            enemigo = collision.gameObject.GetComponentInParent<EnemigoAdapter>();

        if (enemigo != null)
        {
            enemigo.RecibirDanio(danio);
            Debug.Log($"[BulletAdapter] ¡Impacto en enemigo '{collision.gameObject.name}'! Daño: {danio}");
        }

        if (efectoImpactoPrefab != null)
            Instantiate(efectoImpactoPrefab, contacto.point, Quaternion.LookRotation(contacto.normal));

        if (audioImpacto != null)
            AudioSource.PlayClipAtPoint(audioImpacto, contacto.point);

        Destroy(gameObject);
    }
}
