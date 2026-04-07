using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAdapter : MonoBehaviour
{
    [Header("Arma inicial (opcional)")]
    [Tooltip("Si se asigna, esta arma se equipa automáticamente al iniciar la partida.")]
    [SerializeField] private ArmaDefinicion armaInicial;

    [Header("Configuración por defecto")]
    [SerializeField] private int   municionMaxima  = 12;
    [SerializeField] private float cadencia        = 0.15f;
    [SerializeField] private float duracionRecarga = 1.5f;
    [SerializeField] private float bulletForce     = 20f;

    [Header("Referencias de Escena")]
    [SerializeField] private Camera     mainCamera;
    [SerializeField] private Transform  firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private ParticleSystem muzzleFlash;

    [Tooltip("Padre donde se instancia el modelo visual del arma equipada. " +
             "Normalmente es un Transform hijo de la cámara (p.ej. 'PortaArmas').")]
    [SerializeField] private Transform portaArmas;

    [Tooltip("Referencia al PlayerMovement para bloquear disparo cuando el inventario está abierto.")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   audioDisparo;
    [SerializeField] private AudioClip   audioSeco;
    [SerializeField] private AudioClip   audioRecarga;

    private WeaponDomain _dominio;

    // ── Ciclo de vida ─────────────────────────────────────────────────────────

    private void Awake()
    {
        _dominio = new WeaponDomain(municionMaxima, cadencia, duracionRecarga);
        SuscribirEventos();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                Debug.LogWarning("[WeaponAdapter] No se encontró ninguna cámara.");
        }
    }

    private void Start()
    {
        if (armaInicial != null)
            EquiparArma(armaInicial.CrearDatosDominio(), armaInicial);
    }

    private void Update()
    {
        if (_dominio == null) return;

        _dominio.Tick(Time.deltaTime);

        // No leer input si el inventario (u otro sistema) lo bloqueó
        if (playerMovement != null && playerMovement.InputBloqueado) return;
        LeerInput();
    }

    private void OnDestroy() => DesuscribirEventos();

    // ── API pública — Equipar arma desde el inventario ────────────────────────

    /// <summary>
    /// Equipa un arma: reinicia el dominio con sus stats y reemplaza el modelo visual.
    /// Llamado por InventarioAdapter cuando el jugador pulsa "Equipar".
    /// </summary>
    public void DesequiparArma()
    {
        if (armaInicial != null)
            EquiparArma(armaInicial.CrearDatosDominio(), armaInicial);
        else if (portaArmas != null)
            foreach (Transform hijo in portaArmas)
                Destroy(hijo.gameObject);
    }

    public void EquiparArma(ArmaInventario arma, ArmaDefinicion definicion)
    {
        // Reiniciar dominio con los stats del arma nueva
        DesuscribirEventos();
        _dominio = new WeaponDomain(arma.MunicionMaxima, arma.Cadencia, arma.DuracionRecarga);
        SuscribirEventos();

        // Actualizar referencias Unity
        bulletForce  = arma.FuerzaBala;
        bulletPrefab = definicion.bulletPrefab;

        // Actualizar audio si el arma define clips propios
        if (definicion.audioDisparo != null) audioDisparo = definicion.audioDisparo;
        if (definicion.audioRecarga != null) audioRecarga = definicion.audioRecarga;
        if (definicion.audioSeco    != null) audioSeco    = definicion.audioSeco;

        // Reemplazar modelo visual en la mano del jugador
        if (portaArmas != null)
        {
            foreach (Transform hijo in portaArmas)
                Destroy(hijo.gameObject);

            if (definicion.modeloEquipado != null)
                Instantiate(definicion.modeloEquipado, portaArmas);
        }

        Debug.Log($"[WeaponAdapter] Arma equipada: {arma.Nombre} | " +
                  $"Cargador: {arma.MunicionMaxima} | Cadencia: {arma.Cadencia}s");
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private void LeerInput()
    {
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            _dominio.IntentarDisparar();

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            _dominio.IniciarRecarga();
    }

    // ── Suscripciones ─────────────────────────────────────────────────────────

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

    // ── Callbacks del dominio ─────────────────────────────────────────────────

    private void AlDisparar(int municionActual, int municionMax)
    {
        if (bulletPrefab == null || firePoint == null || mainCamera == null)
        {
            Debug.LogWarning("[WeaponAdapter] bulletPrefab, firePoint o mainCamera no asignados.");
            return;
        }

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit)
            ? hit.point
            : ray.GetPoint(100f);

        Vector3 direction = (targetPoint - firePoint.position).normalized;
        GameObject bala   = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));

        if (bala.TryGetComponent(out Rigidbody rb))
            rb.AddForce(direction * bulletForce, ForceMode.Impulse);

        if (muzzleFlash != null) muzzleFlash.Play();
        ReproducirAudio(audioDisparo);

        Debug.Log($"[WeaponAdapter] ¡Disparo! Munición: {municionActual}/{municionMax}");
    }

    private void AlQuedarSinMunicion()
    {
        ReproducirAudio(audioSeco);
        Debug.Log("[WeaponAdapter] Sin munición.");
    }

    private void AlCambiarMunicion(int actual, int max) =>
        Debug.Log($"[WeaponAdapter] Munición → {actual}/{max}");

    private void AlIniciarRecarga(float tiempo)
    {
        ReproducirAudio(audioRecarga);
        Debug.Log($"[WeaponAdapter] Recargando... ({tiempo:F1}s)");
    }

    private void AlCompletarRecarga(int actual, int max) =>
        Debug.Log($"[WeaponAdapter] ¡Recarga completada! {actual}/{max}");

    private void ReproducirAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
