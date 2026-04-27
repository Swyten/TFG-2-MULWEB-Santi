using UnityEngine;
using UnityEngine.InputSystem;

// Adapter del ataque melee del jugador
// Pulso C para atacar; uso un OverlapSphere desde el puntoAtaque para detectar enemigos
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

        // si no asigno puntoAtaque en el Inspector uso la posición del propio objeto
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

    // cuando el dominio confirma el ataque, hago el OverlapSphere y daño a los enemigos que encuentro
    private void AlGolpear()
    {
        Collider[] hits = Physics.OverlapSphere(puntoAtaque.position, rango, capaMaskEnemigos);
        foreach (Collider col in hits)
        {
            // busco en el padre por si impacto un sub-collider del enemigo
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

    // dibujo el rango de ataque en el editor para ver bien cuánto abarca
    private void OnDrawGizmosSelected()
    {
        if (puntoAtaque == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoAtaque.position, rango);
    }
}
