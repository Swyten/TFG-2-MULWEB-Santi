using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia { get; private set; }

    [Header("Adaptadores de UI")]
    [Tooltip("El GameManager ya NO necesita referencia a BarraVidaUI aquí — " +
             "la barra se registra sola en Start(). Este campo queda para PanelNivelUI.")]
    [SerializeField] private PanelNivelUI panelNivelUI;

    [Header("Adaptador de Muerte del Jugador")]
    [SerializeField] private PlayerMuerteAdapter playerMuerteAdapter;

    [Header("Inventario")]
    [SerializeField] private InventarioAdapter inventarioAdapter;

    private PlayerVidaDomain        _domVida;
    private PlayerExperienciaDomain _domXP;

    // ── Ciclo de vida ─────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;

        InicializarDominio();
        CablearEventosFijos();   // Solo los eventos que no dependen de BarraVidaUI
        EmitirEstadoInicial();
    }

    private void Update()
    {
        if (Keyboard.current.xKey.wasPressedThisFrame) _domXP.GanarExperiencia(35);
        if (Keyboard.current.zKey.wasPressedThisFrame) _domVida.RecibirDanio(20f);
        if (Keyboard.current.cKey.wasPressedThisFrame) _domVida.Curar(15f);
    }

    private void OnDestroy()
    {
        if (_domXP != null)
        {
            if (panelNivelUI != null) _domXP.OnExperienciaCambiada -= panelNivelUI.RefrescarPanel;
            _domXP.OnNivelSubido -= OnJugadorSubioNivel;
        }
        if (_domVida != null && playerMuerteAdapter != null)
            _domVida.OnJugadorMuerto -= playerMuerteAdapter.OnJugadorMuerto;

        if (Instancia == this) Instancia = null;
    }

    // ── API pública para Adapters ─────────────────────────────────────────────

    public void DaniarJugador(float cantidad)  => _domVida.RecibirDanio(cantidad);
    public void DarExperiencia(int cantidad)   => _domXP.GanarExperiencia(cantidad);

    /// <summary>
    /// Llamado por ArmaPickupAdapter cuando el jugador recoge un arma del suelo.
    /// </summary>
    public void AgregarArmaAlInventario(ArmaDefinicion definicion)
    {
        if (inventarioAdapter != null)
            inventarioAdapter.AgregarArma(definicion);
        else
            Debug.LogWarning("[GameManager] inventarioAdapter no asignado — no se puede añadir el arma.");
    }

    /// <summary>
    /// Llamado por BarraVidaUI.Start() — garantiza suscripción después de Awake.
    /// </summary>
    public void RegistrarBarraVida(BarraVidaUI barra)
    {
        _domVida.OnVidaCambiada += barra.RefrescarBarra;
        // Forzar refresco inmediato para que la barra muestre el estado actual
        barra.RefrescarBarra(_domVida.VidaActual, _domVida.VidaMaxima);
        Debug.Log("[GameManager] BarraVidaUI registrada y refrescada.");
    }

    /// <summary>
    /// Llamado por BarraVidaUI.OnDestroy() para limpiar la suscripción.
    /// </summary>
    public void DesregistrarBarraVida(BarraVidaUI barra)
    {
        if (_domVida != null)
            _domVida.OnVidaCambiada -= barra.RefrescarBarra;
    }

    // ── Privados ──────────────────────────────────────────────────────────────

    private void InicializarDominio()
    {
        _domVida = new PlayerVidaDomain(vidaMaxima: 100f);
        _domXP   = new PlayerExperienciaDomain(vidaMaximaInicial: 100f);
        Debug.Log("[GameManager] Dominio inicializado.");
    }

    private void CablearEventosFijos()
    {
        // XP y niveles
        if (panelNivelUI != null)
            _domXP.OnExperienciaCambiada += panelNivelUI.RefrescarPanel;
        else
            Debug.LogWarning("[GameManager] panelNivelUI no asignado en el Inspector.");

        _domXP.OnNivelSubido += OnJugadorSubioNivel;

        // Muerte del jugador
        if (playerMuerteAdapter != null)
            _domVida.OnJugadorMuerto += playerMuerteAdapter.OnJugadorMuerto;
        else
            Debug.LogWarning("[GameManager] playerMuerteAdapter no asignado — muerte del jugador inactiva.");

        Debug.Log("[GameManager] Eventos fijos cableados.");
    }

    private void EmitirEstadoInicial()
    {
        // No forzamos Curar(0) aquí porque BarraVidaUI aún no se ha registrado
        // (se registra en Start, que ocurre después de este Awake).
        // El refresco inicial lo hace RegistrarBarraVida().
        _domXP.GanarExperiencia(0);
    }

    private void OnJugadorSubioNivel(int nivelNuevo, float nuevaVidaMaxima)
    {
        _domVida.SetVidaMaxima(nuevaVidaMaxima);
        Debug.Log($"[GameManager] Nivel {nivelNuevo} — nueva vida máxima: {nuevaVidaMaxima}");
    }
}
