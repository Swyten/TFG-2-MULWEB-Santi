using UnityEngine;
using UnityEngine.InputSystem;

// Singleton que wirea el dominio con los adapters de UI y gestiona eventos globales
// Es el único sitio donde creo los objetos de dominio (PlayerVidaDomain y PlayerExperienciaDomain)
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

    private void Awake()
    {
        // patrón singleton: si ya existe una instancia destruyo este y salgo
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;

        InicializarDominio();
        CablearEventosFijos();   // solo los eventos que no dependen de BarraVidaUI
        EmitirEstadoInicial();
    }

    // teclas de prueba: X da 35 XP, Z quita 20 de vida
    private void Update()
    {
        if (Keyboard.current.xKey.wasPressedThisFrame) _domXP.GanarExperiencia(35);
        if (Keyboard.current.zKey.wasPressedThisFrame) _domVida.RecibirDanio(20f);
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

    // API que usan los adapters para dañar al jugador o darle XP sin acceder al dominio directamente
    public void DaniarJugador(float cantidad)  => _domVida.RecibirDanio(cantidad);
    public void DarExperiencia(int cantidad)   => _domXP.GanarExperiencia(cantidad);

    // llamado por ArmaPickupAdapter cuando el jugador recoge un arma del suelo
    public void AgregarArmaAlInventario(ArmaDefinicion definicion)
    {
        if (inventarioAdapter != null)
            inventarioAdapter.AgregarArma(definicion);
        else
            Debug.LogWarning("[GameManager] inventarioAdapter no asignado — no se puede añadir el arma.");
    }

    // llamado por BarraVidaUI.Start() para suscribirse después de que Awake haya inicializado el dominio
    // de esta forma evito problemas con el orden de ejecución de scripts
    public void RegistrarBarraVida(BarraVidaUI barra)
    {
        _domVida.OnVidaCambiada += barra.RefrescarBarra;
        // fuerzo un refresco inmediato para que la barra muestre el estado actual desde el primer frame
        barra.RefrescarBarra(_domVida.VidaActual, _domVida.VidaMaxima);
        Debug.Log("[GameManager] BarraVidaUI registrada y refrescada.");
    }

    // llamado por BarraVidaUI.OnDestroy() para limpiar la suscripción
    public void DesregistrarBarraVida(BarraVidaUI barra)
    {
        if (_domVida != null)
            _domVida.OnVidaCambiada -= barra.RefrescarBarra;
    }

    private void InicializarDominio()
    {
        _domVida = new PlayerVidaDomain(vidaMaxima: 100f);
        _domXP   = new PlayerExperienciaDomain(vidaMaximaInicial: 100f);
        Debug.Log("[GameManager] Dominio inicializado.");
    }

    private void CablearEventosFijos()
    {
        if (panelNivelUI != null)
            _domXP.OnExperienciaCambiada += panelNivelUI.RefrescarPanel;
        else
            Debug.LogWarning("[GameManager] panelNivelUI no asignado en el Inspector.");

        _domXP.OnNivelSubido += OnJugadorSubioNivel;

        if (playerMuerteAdapter != null)
            _domVida.OnJugadorMuerto += playerMuerteAdapter.OnJugadorMuerto;
        else
            Debug.LogWarning("[GameManager] playerMuerteAdapter no asignado — muerte del jugador inactiva.");

        Debug.Log("[GameManager] Eventos fijos cableados.");
    }

    private void EmitirEstadoInicial()
    {
        // no fuerzo Curar(0) aquí porque BarraVidaUI aún no se ha registrado
        // (se registra en su Start, que ocurre después de este Awake)
        // el refresco inicial lo hace RegistrarBarraVida()
        _domXP.GanarExperiencia(0);
    }

    private void OnJugadorSubioNivel(int nivelNuevo, float nuevaVidaMaxima)
    {
        _domVida.SetVidaMaxima(nuevaVidaMaxima);
        Debug.Log($"[GameManager] Nivel {nivelNuevo} — nueva vida máxima: {nuevaVidaMaxima}");
    }
}
