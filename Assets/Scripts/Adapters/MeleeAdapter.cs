using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ADAPTADOR — Ataque melee del jugador.
///
/// CONFIGURACIÓN EN EL INSPECTOR:
///   • danio          → Daño por golpe (defecto: 25).
///   • cooldown       → Segundos entre golpes (defecto: 0.8).
///   • rango          → Radio del OverlapSphere en metros (defecto: 1.5).
///   • capaMaskEnemigos → Layer de los enemigos para el hit detection.
///   • puntoAtaque    → Transform desde donde se lanza el OverlapSphere
///                      (normalmente un hijo de la cámara, frente al jugador).
/// </summary>
public class MeleeAdapter : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float danio    = 25f;
    [SerializeField] private float cooldown = 0.8f;
    [SerializeField] private float rango    = 1.5f;

    [Header("Referencias")]
    [Tooltip("Transform desde donde se mide el rango (p.ej. hijo de la cámara).")]
    [SerializeField] private Transform puntoAtaque;
    [SerializeField] private LayerMask capaMaskEnemigos;

    [Header("Efectos (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   clipGolpe;
    [SerializeField] private ParticleSystem efectoGolpe;

    private MeleeDomain _dominio;

    private void Awake()
    {
        _dominio = new MeleeDomain(danio, cooldown);
        _dominio.OnAtaqueMelee += AlGolpear;

        if (puntoAtaque == null)
            puntoAtaque = transform;
    }

    private void Update()
    {
        _dominio.Tick(Time.deltaTime);

        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
            _dominio.IntentarAtacar();
    }

    private void OnDestroy()
    {
        _dominio.OnAtaqueMelee -= AlGolpear;
    }

    private void AlGolpear()
    {
Collider[] hits = Physics.OverlapSphere(puntoAtaque.position, rango, capaMaskEnemigos);
        foreach (Collider col in hits)
        {
            EnemigoAdapter enemigo = col.GetComponentInParent<EnemigoAdapter>();
            if (enemigo != null)
            {
                enemigo.RecibirDanio(_dominio.Danio);
                Debug.Log($"[MeleeAdapter] Golpe a '{col.gameObject.name}' — daño: {_dominio.Danio}");
            }
        }

        if (efectoGolpe != null) efectoGolpe.Play();
        if (audioSource != null && clipGolpe != null) audioSource.PlayOneShot(clipGolpe);
    }

    // Visualización del rango en el editor
    private void OnDrawGizmosSelected()
    {
        if (puntoAtaque == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoAtaque.position, rango);
    }
}
