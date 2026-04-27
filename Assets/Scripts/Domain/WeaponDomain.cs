using System;

// Dominio del arma: C# puro, sin MonoBehaviour, sin UnityEngine
// Toda la lógica de estado está aquí: munición, cadencia y recarga
// Se comunica con el exterior solo mediante eventos tipados (Action)
// Los adaptadores se suscriben a esos eventos; el dominio no sabe quién escucha
//
// Eventos que emito:
//   OnDisparoRealizado  → (municionActual, municionMaxima)  cuando el disparo tiene éxito
//   OnMunicionCambiada  → (municionActual, municionMaxima)  cualquier cambio de balas
//   OnSinMunicion       → ()                                intento de disparo sin balas
//   OnRecargaIniciada   → (tiempoRecargaTotal)              al empezar a recargar
//   OnRecargaCompletada → (municionActual, municionMaxima)  cuando termina la recarga
public class WeaponDomain
{
    // el adapter instancia la bala y reproduce el efecto de disparo al recibir este evento
    public event Action<int, int> OnDisparoRealizado;

    // el adapter de UI refresca el contador de balas al recibir este evento
    public event Action<int, int> OnMunicionCambiada;

    // el adapter reproduce el "clic" seco cuando no hay balas
    public event Action OnSinMunicion;

    // le paso el tiempo total de recarga para que el adapter pueda animar una barra de progreso
    public event Action<float> OnRecargaIniciada;

    public event Action<int, int> OnRecargaCompletada;

    private int   _municionActual;
    private readonly int   _municionMaxima;
    private readonly float _cadencia;        // segundos mínimos entre disparos
    private readonly float _duracionRecarga; // segundos que dura una recarga completa

    private float _cooldownDisparo; // tiempo restante hasta poder disparar otra vez
    private float _timerRecarga;    // tiempo restante para completar la recarga
    private bool  _estaRecargando;

    public int   MunicionActual  => _municionActual;
    public int   MunicionMaxima  => _municionMaxima;
    public bool  EstaRecargando  => _estaRecargando;
    public bool  MunicionLlena   => _municionActual == _municionMaxima;
    public float DuracionRecarga => _duracionRecarga;

    public WeaponDomain(int municionMaxima = 12, float cadencia = 0.15f, float duracionRecarga = 1.5f)
    {
        _municionMaxima  = municionMaxima;
        _municionActual  = municionMaxima; // empieza con el cargador lleno
        _cadencia        = cadencia;
        _duracionRecarga = duracionRecarga;
        _cooldownDisparo = 0f; // listo para disparar desde el primer frame
        _timerRecarga    = 0f;
        _estaRecargando  = false;
    }

    // se llama cada frame desde el adapter pasándole Time.deltaTime
    // así el dominio no necesita saber nada de Unity
    public void Tick(float deltaTime)
    {
        if (_cooldownDisparo > 0f)
            _cooldownDisparo -= deltaTime;

        if (_estaRecargando)
        {
            _timerRecarga -= deltaTime;

            if (_timerRecarga <= 0f)
                FinalizarRecarga();
        }
    }

    // intenta disparar respetando cadencia, estado de recarga y munición
    public bool IntentarDisparar()
    {
        if (_estaRecargando) return false;
        if (_cooldownDisparo > 0f) return false;

        if (_municionActual <= 0)
        {
            OnSinMunicion?.Invoke();
            return false;
        }

        _municionActual--;
        _cooldownDisparo = _cadencia;

        OnDisparoRealizado?.Invoke(_municionActual, _municionMaxima);
        OnMunicionCambiada?.Invoke(_municionActual, _municionMaxima);

        // si vacío el cargador arranco la recarga automáticamente
        if (_municionActual == 0)
            IniciarRecarga();

        return true;
    }

    // recarga manual; no hago nada si ya estoy recargando o el cargador está lleno
    public void IniciarRecarga()
    {
        if (_estaRecargando || MunicionLlena) return;

        _estaRecargando = true;
        _timerRecarga   = _duracionRecarga;

        OnRecargaIniciada?.Invoke(_duracionRecarga);
    }

    // este método solo lo llama Tick() cuando el timer llega a 0
    private void FinalizarRecarga()
    {
        _estaRecargando = false;
        _timerRecarga   = 0f;
        _municionActual = _municionMaxima;

        OnRecargaCompletada?.Invoke(_municionActual, _municionMaxima);
        OnMunicionCambiada?.Invoke(_municionActual, _municionMaxima);
    }
}
