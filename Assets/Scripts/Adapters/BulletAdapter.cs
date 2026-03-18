using UnityEngine;

/// <summary>
/// CAPA DE ADAPTADOR — Ciclo de vida del proyectil (bala).
///
/// Responsabilidades:
///   1. Destruir el GameObject transcurrido el tiempo de vida configurado en
///      el Inspector, evitando que balas "perdidas" se acumulen en la escena.
///   2. Detectar la primera colisión física y destruir el proyectil de inmediato,
///      dejando un punto de extensión claro para conectar un DamageDomain en el futuro.
///
/// PRINCIPIO DE DISEÑO:
///   Este adaptador NO contiene lógica de daño. Su única responsabilidad es
///   gestionar el ciclo de vida del GameObject. El cálculo y aplicación de daño
///   pertenecerá a un DamageDomain (C# puro) que se conectará desde aquí
///   mediante eventos, siguiendo el mismo patrón del resto de la arquitectura.
///
/// CONFIGURACIÓN EN EL INSPECTOR:
///   • lifeTime          → Segundos antes de que la bala se autodestruya (por defecto 3).
///   • efectoImpacto     → ParticleSystem instanciado al colisionar (opcional).
///   • audioImpacto      → AudioClip reproducido al colisionar (opcional).
///
/// REQUISITOS:
///   • El prefab debe tener un Rigidbody (obligatorio — [RequireComponent]).
///   • El prefab debe tener un Collider con "Is Trigger" DESACTIVADO para
///     que Unity llame a OnCollisionEnter (colisión física, no trigger).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BulletAdapter : MonoBehaviour
{
    // ── Parámetros del proyectil (configurables en el Inspector) ─────────────

    [Header("Ciclo de Vida")]
    [Tooltip("Segundos que la bala permanece en escena si no impacta con nada. " +
             "Evita que proyectiles 'perdidos' acumulen memoria.")]
    [SerializeField] private float lifeTime = 3f;

    // ── Efectos de impacto (opcionales) ──────────────────────────────────────

    [Header("Efectos de Impacto (opcionales)")]
    [Tooltip("Prefab de partículas instanciado en el punto de impacto (polvo, chispas, etc.).")]
    [SerializeField] private GameObject efectoImpactoPrefab;

    [Tooltip("AudioClip reproducido al colisionar. Se usa PlayClipAtPoint para no " +
             "perder el sonido al destruir el GameObject.")]
    [SerializeField] private AudioClip audioImpacto;

    // ── Referencias internas ─────────────────────────────────────────────────

    private bool _yaImpacto; // Guarda evita doble-destrucción en el mismo frame

    // ── Ciclo de vida Unity ───────────────────────────────────────────────────

    private void Start()
    {
        // Programar autodestrucción por tiempo de vida agotado.
        // Destroy(gameObject, delay) es la forma idiomática en Unity:
        // encola la destrucción sin bloquear el hilo principal.
        Destroy(gameObject, lifeTime);

        Debug.Log($"[BulletAdapter] Proyectil instanciado. Se destruirá en {lifeTime}s si no impacta.");
    }

    // ── Detección de colisión física ──────────────────────────────────────────

    /// <summary>
    /// Llamado por Unity al producirse una colisión física (Collider sin Is Trigger).
    /// Destruye el proyectil inmediatamente y ejecuta los efectos de impacto.
    ///
    /// NOTA ARQUITECTÓNICA:
    ///   Aquí NO se aplica daño directamente. En el futuro, este método
    ///   obtendrá una referencia al DamageDomain del objeto golpeado y llamará
    ///   a su método RecibirDanio(), manteniendo la separación de responsabilidades:
    ///
    ///   [FUTURO — conexión con DamageDomain]
    ///   if (collision.gameObject.TryGetComponent(out DamageAdapter dañable))
    ///       dañable.AplicarDanio(danio);
    /// </summary>
    /// <param name="collision">Datos de la colisión: punto de contacto, normal, objeto golpeado.</param>
    private void OnCollisionEnter(Collision collision)
    {
        // Guarda: si por algún motivo Unity llama dos veces en el mismo frame, salimos.
        if (_yaImpacto) return;
        _yaImpacto = true;

        // Punto y normal del primer contacto (para orientar los efectos)
        ContactPoint contacto = collision.GetContact(0);

        Debug.Log($"[BulletAdapter] Impacto con '{collision.gameObject.name}' " +
                  $"en posición {contacto.point}.");

        // Instanciar efecto de partículas en el punto de contacto
        if (efectoImpactoPrefab != null)
        {
            // La rotación Quaternion.LookRotation(normal) orienta el efecto
            // perpendicular a la superficie golpeada (polvo saliendo de la pared, etc.)
            Instantiate(
                efectoImpactoPrefab,
                contacto.point,
                Quaternion.LookRotation(contacto.normal)
            );
        }

        // Reproducir audio en el punto de impacto.
        // PlayClipAtPoint crea un AudioSource temporal en la escena: el sonido
        // se sigue reproduciendo aunque el proyectil ya haya sido destruido.
        if (audioImpacto != null)
            AudioSource.PlayClipAtPoint(audioImpacto, contacto.point);

        // ── [FUTURO — aplicar daño al objeto golpeado] ───────────────────────
        // if (collision.gameObject.TryGetComponent(out DamageAdapter dañable))
        //     dañable.AplicarDanio(danio);
        // ────────────────────────────────────────────────────────────────────

        // Destruir el proyectil inmediatamente al impactar.
        // Cancelamos también la destrucción por tiempo de vida programada en Start().
        Destroy(gameObject);
    }
}
