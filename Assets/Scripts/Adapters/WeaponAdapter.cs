using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// CAPA DE ADAPTADOR — Puente entre WeaponDomain y el motor Unity.
///
/// Responsabilidades:
///   1. Instanciar WeaponDomain con los parámetros configurados en el Inspector.
///   2. Capturar el input del jugador (clic izquierdo = disparar, R = recargar)
///      y delegarlo al dominio. El adaptador NO decide si se puede disparar;
///      esa lógica vive exclusivamente en WeaponDomain.
///   3. Suscribirse a los eventos del dominio para ejecutar efectos en Unity:
///      instanciar balas, calcular trayectoria con Raycast desde el crosshair,
///      aplicar física al proyectil, reproducir partículas y audio.
///   4. Llamar a domain.Tick(Time.deltaTime) cada frame para que el dominio
///      avance sus temporizadores internos (cadencia y recarga).
///
/// SISTEMA DE APUNTADO (Raycast desde el centro de la cámara):
///   La bala no sale en línea recta desde el firePoint, sino que su dirección
///   se calcula lanzando un Raycast desde el centro exacto de la pantalla
///   (viewport 0.5, 0.5). Esto garantiza que el proyectil siempre apunte al
///   centro del crosshair, independientemente del offset visual del arma.
///
/// CONFIGURACIÓN EN EL INSPECTOR:
///   • mainCamera      → Cámara principal (normalmente la FPS camera del jugador).
///   • firePoint       → Transform vacío hijo del arma: marca el ORIGEN del proyectil.
///   • bulletPrefab    → Prefab de la bala (debe tener Rigidbody y BulletAdapter).
///   • muzzleFlash     → ParticleSystem del fogonazo (opcional).
///   • audioSource     → AudioSource adjunto al arma (opcional).
///   • audioDisparo    → AudioClip del disparo.
///   • audioSeco       → AudioClip del "clic" sin munición.
///   • audioRecarga    → AudioClip de la recarga.
///   • municionMaxima  → Capacidad del cargador (por defecto 12).
///   • cadencia        → Segundos entre disparos  (por defecto 0.15).
///   • duracionRecarga → Segundos de recarga       (por defecto 1.5).
///   • bulletForce     → Magnitud del impulso físico aplicado al proyectil (por defecto 20).
/// </summary>
public class WeaponAdapter : MonoBehaviour
{
    // ── Parámetros del arma (configurables en el Inspector) ──────────────────

    [Header("Configuración del Arma")]
    [Tooltip("Capacidad total del cargador.")]
    [SerializeField] private int   municionMaxima  = 12;

    [Tooltip("Segundos mínimos entre disparos (0.15 ≈ 6 disparos/seg).")]
    [SerializeField] private float cadencia        = 0.15f;

    [Tooltip("Segundos que dura la animación de recarga.")]
    [SerializeField] private float duracionRecarga = 1.5f;

    [Tooltip("Magnitud del impulso físico (ForceMode.Impulse) aplicado al Rigidbody del proyectil. " +
             "Valores recomendados: 10–40 según la masa del prefab de bala.")]
    [SerializeField] private float bulletForce     = 20f;

    // ── Referencias de escena (asignar en el Inspector) ──────────────────────

    [Header("Referencias de Escena")]
    [Tooltip("Cámara principal del jugador. Se usa para lanzar el Raycast desde el centro " +
             "exacto de la pantalla y calcular la dirección real del crosshair.")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Transform que marca el ORIGEN del proyectil (posición de salida de la bala). " +
             "Su forward ya no determina la dirección: eso lo hace el Raycast de cámara.")]
    [SerializeField] private Transform firePoint;

    [Tooltip("Prefab del proyectil. Debe tener un Rigidbody y BulletAdapter.")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("ParticleSystem del fogonazo de boca (opcional).")]
    [SerializeField] private ParticleSystem muzzleFlash;

    // ── Audio ────────────────────────────────────────────────────────────────

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   audioDisparo;
    [SerializeField] private AudioClip   audioSeco;      // Clic sin munición
    [SerializeField] private AudioClip   audioRecarga;

    // ── Dominio (instanciado en código, nunca desde el Inspector) ────────────

    private WeaponDomain _dominio;

    // ── Ciclo de vida Unity ───────────────────────────────────────────────────

    private void Awake()
    {
        // Instanciar el dominio puro con los parámetros del Inspector
        _dominio = new WeaponDomain(
            municionMaxima:  municionMaxima,
            cadencia:        cadencia,
            duracionRecarga: duracionRecarga
        );

        SuscribirEventos();

        // Fallback: si no se asigna cámara en el Inspector, usar Camera.main
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                Debug.LogWarning("[WeaponAdapter] No se encontró ninguna cámara. " +
                                 "Asigna mainCamera en el Inspector.");
        }

        Debug.Log("[WeaponAdapter] Dominio de arma inicializado. " +
                  $"Cargador: {municionMaxima} | Cadencia: {cadencia}s | " +
                  $"Recarga: {duracionRecarga}s | Fuerza: {bulletForce}");
    }

    private void Update()
    {
        // Paso 1: Tick del dominio (avanza cadencia y recarga)
        _dominio.Tick(Time.deltaTime);

        // Paso 2: Capturar input y delegar al dominio
        LeerInput();
    }

    private void OnDestroy()
    {
        // Desuscribir para evitar memory leaks
        DesuscribirEventos();
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Lee el input del frame actual y llama al método de dominio correspondiente.
    /// Este adaptador NO decide si el disparo es válido; eso lo hace WeaponDomain.
    /// </summary>
    private void LeerInput()
    {
        // Disparar: clic izquierdo mantenido = automático mientras la cadencia lo permita
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            _dominio.IntentarDisparar();

        // Recargar: tecla R (manual)
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            _dominio.IniciarRecarga();
    }

    // ── Suscripción / Desuscripción de eventos ────────────────────────────────

    private void SuscribirEventos()
    {
        _dominio.OnDisparoRealizado  += AlDisparar;
        _dominio.OnSinMunicion       += AlQuedarSinMunicion;
        _dominio.OnMunicionCambiada  += AlCambiarMunicion;
        _dominio.OnRecargaIniciada   += AlIniciarRecarga;
        _dominio.OnRecargaCompletada += AlCompletarRecarga;
    }

    private void DesuscribirEventos()
    {
        if (_dominio == null) return;

        _dominio.OnDisparoRealizado  -= AlDisparar;
        _dominio.OnSinMunicion       -= AlQuedarSinMunicion;
        _dominio.OnMunicionCambiada  -= AlCambiarMunicion;
        _dominio.OnRecargaIniciada   -= AlIniciarRecarga;
        _dominio.OnRecargaCompletada -= AlCompletarRecarga;
    }

    // ── Manejadores de eventos del dominio ────────────────────────────────────

    /// <summary>
    /// Llamado cuando el dominio confirma un disparo exitoso.
    ///
    /// LÓGICA DE APUNTADO POR RAYCAST:
    ///   1. Se lanza un rayo desde el centro exacto de la pantalla (crosshair).
    ///   2. Si el rayo impacta con geometría, el destino es hit.point.
    ///   3. Si no impacta (cielo abierto, etc.), el destino es un punto lejano
    ///      en la dirección del rayo para evitar que la bala apunte "a ningún lado".
    ///   4. La dirección final se calcula desde el firePoint hasta ese destino,
    ///      garantizando que la bala salga de la boca del cañón pero viaje
    ///      exactamente hacia donde apunta el crosshair.
    /// </summary>
    private void AlDisparar(int municionActual, int municionMaxima)
    {
        // Validar referencias mínimas antes de instanciar
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("[WeaponAdapter] bulletPrefab o firePoint no asignados en el Inspector.");
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogWarning("[WeaponAdapter] mainCamera no asignada. No se puede calcular la dirección del disparo.");
            return;
        }

        // ── Paso 1: Raycast desde el centro de la pantalla (crosshair) ───────
        // ViewportPointToRay(0.5, 0.5, 0) genera un rayo exactamente desde el
        // píxel central de la cámara, independientemente de la resolución.
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // ── Paso 2: Determinar el punto de destino ────────────────────────────
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // El rayo impactó con geometría: apuntar exactamente a ese punto
            targetPoint = hit.point;
            Debug.Log($"[WeaponAdapter] Raycast impactó '{hit.collider.name}' a {hit.distance:F1}m.");
        }
        else
        {
            // El rayo no impactó nada (cielo, vacío): usar un punto lejano en la dirección del rayo
            // GetPoint(100) devuelve la posición a 100 unidades de la cámara en esa dirección
            targetPoint = ray.GetPoint(100f);
        }

        // ── Paso 3: Calcular la dirección real del disparo ────────────────────
        // Desde la boca del cañón (firePoint) hacia el punto de destino del crosshair.
        // Esto corrige el offset visual entre la cámara y el arma.
        Vector3 direction = (targetPoint - firePoint.position).normalized;

        // ── Paso 4: Instanciar la bala orientada hacia la dirección calculada ─
        // Quaternion.LookRotation(direction) rota el proyectil para que su forward
        // coincida con la dirección de viaje, orientando también el BulletAdapter.
        GameObject bala = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        // ── Paso 5: Aplicar impulso físico en la dirección del crosshair ──────
        if (bala.TryGetComponent(out Rigidbody rb))
            rb.AddForce(direction * bulletForce, ForceMode.Impulse);
        else
            Debug.LogWarning($"[WeaponAdapter] El prefab '{bulletPrefab.name}' no tiene Rigidbody. " +
                              "La bala no recibirá impulso físico.");

        // ── Efectos de disparo ────────────────────────────────────────────────

        // Reproducir fogonazo de boca
        if (muzzleFlash != null)
            muzzleFlash.Play();

        // Reproducir audio de disparo
        ReproducirAudio(audioDisparo);

        Debug.Log($"[WeaponAdapter] ¡Disparo! Dirección: {direction} | " +
                  $"Impulso: {bulletForce} | Munición restante: {municionActual}/{municionMaxima}");
    }

    /// <summary>
    /// Llamado cuando el jugador intenta disparar con el cargador vacío.
    /// </summary>
    private void AlQuedarSinMunicion()
    {
        ReproducirAudio(audioSeco);
        Debug.Log("[WeaponAdapter] Sin munición. Recarga con R.");
    }

    /// <summary>
    /// Llamado en cualquier cambio de munición (disparo o recarga completada).
    /// Punto de extensión para el adaptador de UI de munición.
    /// </summary>
    private void AlCambiarMunicion(int municionActual, int municionMaxima)
    {
        // ── [FUTURO] Notificar al adaptador de UI de munición ────────────────
        // panelMunicionUI.RefrescarMunicion(municionActual, municionMaxima);
        // ────────────────────────────────────────────────────────────────────
        Debug.Log($"[WeaponAdapter] HUD Munición → {municionActual}/{municionMaxima}");
    }

    /// <summary>
    /// Llamado al inicio de la secuencia de recarga.
    /// </summary>
    private void AlIniciarRecarga(float tiempoRecarga)
    {
        ReproducirAudio(audioRecarga);

        // ── [FUTURO] Animar barra de recarga en UI ───────────────────────────
        // barraRecargaUI.IniciarAnimacion(tiempoRecarga);
        // ────────────────────────────────────────────────────────────────────
        Debug.Log($"[WeaponAdapter] Recargando... ({tiempoRecarga:F1}s)");
    }

    /// <summary>
    /// Llamado cuando la recarga se completa y el arma está lista para disparar.
    /// </summary>
    private void AlCompletarRecarga(int municionActual, int municionMaxima)
    {
        Debug.Log($"[WeaponAdapter] ¡Recarga completada! Cargador: {municionActual}/{municionMaxima}");
    }

    // ── Helpers privados ─────────────────────────────────────────────────────

    /// <summary>
    /// Reproduce un AudioClip si el AudioSource y el clip están asignados.
    /// Usa PlayOneShot para permitir disparos solapados sin cortar el audio anterior.
    /// </summary>
    private void ReproducirAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
