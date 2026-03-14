using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// GAME MANAGER — Punto de entrada y cableado de la arquitectura hexagonal.
///
/// Responsabilidades:
///   1. Instanciar los objetos de dominio (sin MonoBehaviour).
///   2. Suscribir los adaptadores de UI a los eventos del dominio.
///   3. Actuar como único punto de acceso al estado del juego.
///   4. [FUTURO] Conectar con Firebase vía servidor MCP / n8n para
///      persistir y leer (nivel, XP, vida) en la nube.
///
/// TECLAS DE PRUEBA (solo en desarrollo):
///   X  → Ganar 35 XP
///   Z  → Recibir 20 de daño
///   C  → Curar 15 HP
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Referencias a los Adaptadores de UI (asignar en el Inspector) ─────────
    [Header("Adaptadores de UI")]
    [Tooltip("Script BarraVidaUI adjunto al GameObject de la barra de HP.")]
    [SerializeField] private BarraVidaUI barraVidaUI;

    [Tooltip("Script PanelNivelUI adjunto al panel inferior de nivel/XP.")]
    [SerializeField] private PanelNivelUI panelNivelUI;

    // ── Objetos de Dominio (instanciados en código, no en el Inspector) ───────
    private PlayerVidaDomain        _domVida;
    private PlayerExperienciaDomain _domXP;

    // ── Ciclo de vida Unity ───────────────────────────────────────────────────

    private void Awake()
    {
        InicializarDominio();
        CablearEventos();
        EmitirEstadoInicial();
    }

    private void Update()
    {
        // ── Testing en tiempo de ejecución ──
        if (Keyboard.current.xKey.wasPressedThisFrame)
            _domXP.GanarExperiencia(35);

        if (Keyboard.current.zKey.wasPressedThisFrame)
            _domVida.RecibirDanio(20f);

        if (Keyboard.current.cKey.wasPressedThisFrame)
            _domVida.Curar(15f);
    }

    private void OnDestroy()
    {
        // Desuscribir para evitar memory leaks si el manager se destruye
        if (_domVida != null)
            _domVida.OnVidaCambiada -= barraVidaUI.RefrescarBarra;

        if (_domXP != null)
        {
            _domXP.OnExperienciaCambiada -= panelNivelUI.RefrescarPanel;
            _domXP.OnNivelSubido         -= OnJugadorSubioNivel;
        }
    }

    // ── Métodos privados de inicialización ───────────────────────────────────

    /// <summary>Crea las instancias de dominio puras (sin MonoBehaviour).</summary>
    private void InicializarDominio()
    {
        _domVida = new PlayerVidaDomain(vidaMaxima: 100f);
        _domXP   = new PlayerExperienciaDomain(vidaMaximaInicial: 100f);

        Debug.Log("[GameManager] Dominio inicializado.");
    }

    /// <summary>
    /// Conecta los eventos del dominio con los métodos públicos de los adaptadores.
    /// Este es el "puerto" de la arquitectura hexagonal: el dominio no sabe nada
    /// de quién escucha; los adaptadores no saben nada de cómo se calcula el estado.
    /// </summary>
    private void CablearEventos()
    {
        // Vida → Barra de HP
        _domVida.OnVidaCambiada      += barraVidaUI.RefrescarBarra;

        // XP → Panel de nivel
        _domXP.OnExperienciaCambiada += panelNivelUI.RefrescarPanel;

        // Subida de nivel → sincronizar vida máxima en el dominio de vida
        _domXP.OnNivelSubido         += OnJugadorSubioNivel;

        Debug.Log("[GameManager] Eventos cableados.");
    }

    /// <summary>
    /// Fuerza un disparo de eventos inicial para que la UI muestre
    /// el estado correcto desde el primer frame sin esperar input.
    /// </summary>
    private void EmitirEstadoInicial()
    {
        // Simula un cambio de 0 puntos para forzar el refresco visual
        _domVida.Curar(0f);
        _domXP.GanarExperiencia(0);
    }

    // ── Manejadores de eventos entre dominios ────────────────────────────────

    /// <summary>
    /// Cuando el dominio de XP notifica una subida de nivel,
    /// actualiza la vida máxima en el dominio de vida.
    /// Esta es la única comunicación entre dominios y pasa por el GameManager.
    /// </summary>
    private void OnJugadorSubioNivel(int nivelNuevo, float nuevaVidaMaxima)
    {
        _domVida.SetVidaMaxima(nuevaVidaMaxima);
        Debug.Log($"[GameManager] Nivel {nivelNuevo} alcanzado. Nueva vida máxima: {nuevaVidaMaxima}");

        // ── [FUTURO - FASE CLOUD] ─────────────────────────────────────────────
        // Aquí llamarás al servidor MCP / n8n para persistir en Firebase:
        //
        // await FirebaseMCPAdapter.GuardarProgreso(new JugadorDTO {
        //     Id        = "player_01",
        //     Nivel     = nivelNuevo,
        //     VidaMax   = nuevaVidaMaxima,
        //     Timestamp = DateTime.UtcNow
        // });
        // ─────────────────────────────────────────────────────────────────────
    }
}
