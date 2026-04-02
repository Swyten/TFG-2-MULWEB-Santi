using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemigoAdapter : MonoBehaviour
{
    [Header("Stats del Enemigo")]
    [SerializeField] private float vidaMaxima          = 50f;
    [SerializeField] private float danioAtaque         = 10f;
    [SerializeField] private float cadenciaAtaque      = 1.5f;
    [SerializeField] private float rangoAtaque         = 1.8f;
    [SerializeField] private float velocidadMovimiento = 3.5f;
    [SerializeField] private int   xpAlMorir           = 50;

    [Header("Referencias de Escena")]
    [SerializeField] private Transform jugador;

    [Header("Loot (opcional)")]
    [Tooltip("Arma que puede dropear al morir. Déjalo vacío para que no droppee nada.")]
    [SerializeField] private ArmaDefinicion armaDropeable;

    [Range(0f, 1f)]
    [Tooltip("Probabilidad de drop: 0 = nunca, 1 = siempre.")]
    [SerializeField] private float probabilidadDrop = 0.5f;

    [Header("Efectos (opcional)")]
    [SerializeField] private ParticleSystem efectoMuerte;
    [SerializeField] private AudioSource    audioSource;
    [SerializeField] private AudioClip      clipMuerte;

    private EnemigoDomain _dominio;
    private NavMeshAgent  _agente;
    private bool          _destruyendose;

    private void Awake()
    {
        _agente       = GetComponent<NavMeshAgent>();
        _agente.speed = velocidadMovimiento;

        _dominio = new EnemigoDomain(vidaMaxima, danioAtaque, cadenciaAtaque, xpAlMorir);
        _dominio.OnAtaque       += AlAtacar;
        _dominio.OnVidaCambiada += AlCambiarVida;
        _dominio.OnMuerto       += AlMorir;

        if (jugador == null)
        {
            GameObject go = GameObject.FindWithTag("Player");
            if (go != null) jugador = go.transform;
            else Debug.LogWarning("[EnemigoAdapter] No se encontró GameObject con tag 'Player'.");
        }
    }

    private void Update()
    {
        // Guard completo: dominio, muerte, destrucción en curso o jugador inválido
        if (_dominio == null || _dominio.EstaMuerto || _destruyendose) return;

        // Comprobar que el jugador sigue vivo en escena (puede haberse destruido al morir)
        if (jugador == null || !jugador.gameObject.activeInHierarchy) return;

        // Comprobar que el agente sigue activo
        if (_agente == null || !_agente.isActiveAndEnabled || !_agente.isOnNavMesh) return;

        _agente.SetDestination(jugador.position);

        float distancia   = Vector3.Distance(transform.position, jugador.position);
        bool  enRango     = distancia <= rangoAtaque;
        _agente.isStopped = enRango;

        _dominio.TickAtaque(Time.deltaTime, enRango);
    }

    private void OnDestroy()
    {
        _destruyendose = true;
        if (_dominio == null) return;
        _dominio.OnAtaque       -= AlAtacar;
        _dominio.OnVidaCambiada -= AlCambiarVida;
        _dominio.OnMuerto       -= AlMorir;
    }

    public void RecibirDanio(float cantidad)
    {
        if (_dominio == null || _destruyendose) return;
        _dominio.RecibirDanio(cantidad);
    }

    private void AlAtacar(float danio)
    {
        if (GameManager.Instancia != null)
            GameManager.Instancia.DaniarJugador(danio);
    }

    private void AlCambiarVida(float actual, float maxima)
    {
        Debug.Log($"[EnemigoAdapter] Vida: {actual}/{maxima}");
    }

    private void AlMorir(int xp)
    {
        _destruyendose = true;

        if (GameManager.Instancia != null)
            GameManager.Instancia.DarExperiencia(xp);

        // Drop de arma con probabilidad configurable
        if (armaDropeable != null && armaDropeable.pickupPrefab != null
            && Random.value <= probabilidadDrop)
        {
            Vector3 posicionDrop = transform.position + Vector3.up * 0.5f;
            Instantiate(armaDropeable.pickupPrefab, posicionDrop, Quaternion.identity);
            Debug.Log($"[EnemigoAdapter] Drop: '{armaDropeable.nombreArma}'");
        }

        if (efectoMuerte != null)
        {
            efectoMuerte.transform.SetParent(null);
            efectoMuerte.Play();
        }

        if (audioSource != null && clipMuerte != null)
            audioSource.PlayOneShot(clipMuerte);

        if (_agente != null) _agente.enabled = false;
        if (TryGetComponent(out Collider col)) col.enabled = false;

        Destroy(gameObject, 0.1f);
    }
}
