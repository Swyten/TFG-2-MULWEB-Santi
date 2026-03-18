using System;

/// <summary>
/// CAPA DE DOMINIO — Gestión pura del sistema de arma.
///
/// ► C# puro: sin UnityEngine, sin MonoBehaviour. Totalmente testeable en aislamiento.
/// ► Toda la lógica de estado vive aquí: munición, cadencia de tiro y recarga.
/// ► Se comunica con el exterior exclusivamente mediante eventos tipados (Action).
///   Los adaptadores (efectos, UI, audio) se suscriben; el dominio nunca sabe quién escucha.
///
/// EVENTOS EMITIDOS:
///   OnDisparoRealizado  → (municionActual, municionMaxima)  disparo exitoso.
///   OnMunicionCambiada  → (municionActual, municionMaxima)  cualquier cambio de balas.
///   OnSinMunicion       → ()                                intento de disparo sin balas.
///   OnRecargaIniciada   → (tiempoRecargaTotal)              inicio de recarga.
///   OnRecargaCompletada → (municionActual, municionMaxima)  recarga terminada.
/// </summary>
public class WeaponDomain
{
    // ── Eventos del dominio ──────────────────────────────────────────────────

    /// <summary>
    /// Disparo exitoso. Parámetros: (municionActual, municionMaxima).
    /// El adaptador instancia la bala y reproduce el efecto de disparo.
    /// </summary>
    public event Action<int, int> OnDisparoRealizado;

    /// <summary>
    /// Cualquier cambio en la munición (disparo o recarga completada).
    /// Parámetros: (municionActual, municionMaxima).
    /// El adaptador de UI refresca el contador de balas.
    /// </summary>
    public event Action<int, int> OnMunicionCambiada;

    /// <summary>
    /// El jugador intenta disparar pero el cargador está vacío.
    /// El adaptador reproduce el sonido de "clic" seco.
    /// </summary>
    public event Action OnSinMunicion;

    /// <summary>
    /// Se inicia la secuencia de recarga.
    /// Parámetro: (tiempoRecargaTotal) en segundos, para animar una barra de progreso.
    /// </summary>
    public event Action<float> OnRecargaIniciada;

    /// <summary>
    /// La recarga ha concluido y el arma está lista.
    /// Parámetros: (municionActual, municionMaxima).
    /// </summary>
    public event Action<int, int> OnRecargaCompletada;

    // ── Estado privado ───────────────────────────────────────────────────────

    private int   _municionActual;
    private readonly int   _municionMaxima;
    private readonly float _cadencia;           // Segundos mínimos entre disparos
    private readonly float _duracionRecarga;    // Segundos que dura una recarga completa

    private float _cooldownDisparo;             // Tiempo restante hasta poder volver a disparar
    private float _timerRecarga;                // Tiempo restante para completar la recarga
    private bool  _estaRecargando;

    // ── Propiedades de solo lectura ──────────────────────────────────────────

    public int   MunicionActual  => _municionActual;
    public int   MunicionMaxima  => _municionMaxima;
    public bool  EstaRecargando  => _estaRecargando;
    public bool  MunicionLlena   => _municionActual == _municionMaxima;
    public float DuracionRecarga => _duracionRecarga;

    // ── Constructor ──────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un arma con la configuración indicada.
    /// </summary>
    /// <param name="municionMaxima">Capacidad total del cargador. Por defecto: 12.</param>
    /// <param name="cadencia">Segundos mínimos entre disparos (0.15 ≈ 6 disparos/seg). Por defecto: 0.15.</param>
    /// <param name="duracionRecarga">Segundos que dura la recarga completa. Por defecto: 1.5.</param>
    public WeaponDomain(int municionMaxima = 12, float cadencia = 0.15f, float duracionRecarga = 1.5f)
    {
        _municionMaxima  = municionMaxima;
        _municionActual  = municionMaxima;
        _cadencia        = cadencia;
        _duracionRecarga = duracionRecarga;
        _cooldownDisparo = 0f;   // Listo para disparar desde el primer frame
        _timerRecarga    = 0f;
        _estaRecargando  = false;
    }

    // ── API pública — Casos de uso ────────────────────────────────────────────

    /// <summary>
    /// Debe llamarse cada frame desde el adaptador, pasando Time.deltaTime.
    /// Avanza el cooldown de cadencia y el temporizador de recarga.
    /// El dominio no conoce Unity; recibe el delta como parámetro.
    /// </summary>
    /// <param name="deltaTime">Tiempo transcurrido desde el último frame (Time.deltaTime).</param>
    public void Tick(float deltaTime)
    {
        // Avanzar cooldown de cadencia de tiro
        if (_cooldownDisparo > 0f)
            _cooldownDisparo -= deltaTime;

        // Avanzar temporizador de recarga
        if (_estaRecargando)
        {
            _timerRecarga -= deltaTime;

            if (_timerRecarga <= 0f)
                FinalizarRecarga();
        }
    }

    /// <summary>
    /// Intenta realizar un disparo respetando cadencia, recarga y munición.
    /// </summary>
    /// <returns>True si el disparo fue exitoso.</returns>
    public bool IntentarDisparar()
    {
        // Bloqueado mientras recarga
        if (_estaRecargando)
            return false;

        // Bloqueado por cadencia de tiro
        if (_cooldownDisparo > 0f)
            return false;

        // Sin munición: emitir evento específico y salir
        if (_municionActual <= 0)
        {
            OnSinMunicion?.Invoke();
            return false;
        }

        // ── Disparo exitoso ──────────────────────────────────────────────────
        _municionActual--;
        _cooldownDisparo = _cadencia;

        OnDisparoRealizado?.Invoke(_municionActual, _municionMaxima);
        OnMunicionCambiada?.Invoke(_municionActual, _municionMaxima);

        // Recarga automática al vaciar el cargador
        if (_municionActual == 0)
            IniciarRecarga();

        return true;
    }

    /// <summary>
    /// Inicia la recarga manual si hay espacio en el cargador y no se está ya recargando.
    /// </summary>
    public void IniciarRecarga()
    {
        // No recargar si ya está recargando o el cargador está lleno
        if (_estaRecargando || MunicionLlena)
            return;

        _estaRecargando = true;
        _timerRecarga   = _duracionRecarga;

        OnRecargaIniciada?.Invoke(_duracionRecarga);
    }

    // ── Métodos privados ─────────────────────────────────────────────────────

    /// <summary>
    /// Completa la recarga: restaura la munición y notifica al exterior.
    /// Solo es llamado internamente desde Tick() cuando el timer llega a 0.
    /// </summary>
    private void FinalizarRecarga()
    {
        _estaRecargando = false;
        _timerRecarga   = 0f;
        _municionActual = _municionMaxima;

        OnRecargaCompletada?.Invoke(_municionActual, _municionMaxima);
        OnMunicionCambiada?.Invoke(_municionActual, _municionMaxima);
    }
}
