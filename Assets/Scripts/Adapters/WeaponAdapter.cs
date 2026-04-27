using UnityEngine;
using UnityEngine.InputSystem;

// Adapter del arma: hace de puente entre el dominio y Unity
// Lee input, llama a los métodos del dominio y reacciona a sus eventos
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

        // no leo input si el inventario (u otro sistema) lo bloqueó
        if (playerMovement != null && playerMovement.InputBloqueado) return;
        LeerInput();
    }

    private void OnDestroy() => DesuscribirEventos();

    // equipa un arma: reinicia el dominio con sus stats y reemplaza el modelo visual
    // se llama desde InventarioAdapter cuando el jugador pulsa "Equipar"
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
        // creo un dominio nuevo con los stats del arma nueva
        DesuscribirEventos();
        _dominio = new WeaponDomain(arma.MunicionMaxima, arma.Cadencia, arma.DuracionRecarga);
        SuscribirEventos();

        bulletForce  = arma.FuerzaBala;
        bulletPrefab = definicion.bulletPrefab;

        // actualizo los clips de audio solo si el arma define los suyos propios
        if (definicion.audioDisparo != null) audioDisparo = definicion.audioDisparo;
        if (definicion.audioRecarga != null) audioRecarga = definicion.audioRecarga;
        if (definicion.audioSeco    != null) audioSeco    = definicion.audioSeco;

        // destruyo el modelo anterior y pongo el del arma nueva en la mano del jugador
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

    private void LeerInput()
    {
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            _dominio.IntentarDisparar();

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            _dominio.IniciarRecarga();
    }

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

    private void AlDisparar(int municionActual, int municionMax)
    {
        if (bulletPrefab == null || firePoint == null || mainCamera == null)
        {
            Debug.LogWarning("[WeaponAdapter] bulletPrefab, firePoint o mainCamera no asignados.");
            return;
        }

        // la dirección de la bala apunta al centro de la pantalla, no al forward del firePoint
        // así el disparo siempre va donde miro, aunque el arma esté descentrada
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
