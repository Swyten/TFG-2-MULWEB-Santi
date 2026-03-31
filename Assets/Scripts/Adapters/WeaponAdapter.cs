using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAdapter : MonoBehaviour
{
    [Header("Configuración del Arma")]
    [SerializeField] private int   municionMaxima  = 12;
    [SerializeField] private float cadencia        = 0.15f;
    [SerializeField] private float duracionRecarga = 1.5f;
    [SerializeField] private float bulletForce     = 20f;

    [Header("Referencias de Escena")]
    [SerializeField] private Camera     mainCamera;
    [SerializeField] private Transform  firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private ParticleSystem muzzleFlash;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   audioDisparo;
    [SerializeField] private AudioClip   audioSeco;
    [SerializeField] private AudioClip   audioRecarga;

    private WeaponDomain _dominio;

    private void Awake()
    {
        _dominio = new WeaponDomain(
            municionMaxima:  municionMaxima,
            cadencia:        cadencia,
            duracionRecarga: duracionRecarga
        );

        SuscribirEventos();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                Debug.LogWarning("[WeaponAdapter] No se encontró ninguna cámara.");
        }

        Debug.Log($"[WeaponAdapter] Dominio de arma inicializado. " +
                  $"Cargador: {municionMaxima} | Cadencia: {cadencia}s | " +
                  $"Recarga: {duracionRecarga}s | Fuerza: {bulletForce}");
    }

    private void Update()
    {
        // Guard: si el dominio es null (puede pasar un frame tras desactivación) salir
        if (_dominio == null) return;

        _dominio.Tick(Time.deltaTime);
        LeerInput();
    }

    private void OnDestroy()
    {
        DesuscribirEventos();
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

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit)
            ? hit.point
            : ray.GetPoint(100f);

        Vector3 direction = (targetPoint - firePoint.position).normalized;

        GameObject bala = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));

        if (bala.TryGetComponent(out Rigidbody rb))
            rb.AddForce(direction * bulletForce, ForceMode.Impulse);

        if (muzzleFlash != null) muzzleFlash.Play();
        ReproducirAudio(audioDisparo);

        Debug.Log($"[WeaponAdapter] ¡Disparo! | Impulso: {bulletForce} | Munición: {municionActual}/{municionMax}");
    }

    private void AlQuedarSinMunicion()
    {
        ReproducirAudio(audioSeco);
        Debug.Log("[WeaponAdapter] Sin munición.");
    }

    private void AlCambiarMunicion(int actual, int max)
    {
        Debug.Log($"[WeaponAdapter] Munición → {actual}/{max}");
    }

    private void AlIniciarRecarga(float tiempo)
    {
        ReproducirAudio(audioRecarga);
        Debug.Log($"[WeaponAdapter] Recargando... ({tiempo:F1}s)");
    }

    private void AlCompletarRecarga(int actual, int max)
    {
        Debug.Log($"[WeaponAdapter] ¡Recarga completada! {actual}/{max}");
    }

    private void ReproducirAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
