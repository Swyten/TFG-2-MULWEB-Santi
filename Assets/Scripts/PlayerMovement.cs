using System.Collections;  // necesito IEnumerator para la corrutina del dash
using UnityEngine;
using UnityEngine.InputSystem; // nuevo Input System de Unity 6

// Controla el movimiento del jugador en primera persona
// No uso un archivo .inputactions, creo las acciones directamente por código en Awake
// Tiene WASD, cámara FPS, gravedad, doble salto, dash y gancho (grappling hook)
// El gancho tiene prioridad máxima, luego el dash, luego el movimiento normal
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento normal en m/s.")]
    public float velocidadMovimiento = 5f;

    [Header("Salto y Gravedad")]
    [Tooltip("Fuerza con la que el jugador salta.")]
    public float fuerzaSalto = 5f;

    [Tooltip("Número máximo de saltos antes de tocar suelo. 2 = doble salto.")]
    public int saltosMaximos = 2;

    [Tooltip("Intensidad de la gravedad (valor negativo).")]
    public float gravedad = -9.81f;

    [Header("Dash")]
    [Tooltip("Velocidad del impulso del Dash en m/s.")]
    public float dashVelocidad = 20f;

    [Tooltip("Duración del Dash en segundos.")]
    public float dashDuracion = 0.2f;

    [Tooltip("Tiempo de recarga del Dash en segundos.")]
    public float dashCooldown = 1.5f;

    [Header("Gancho (Grappling Hook)")]
    [Tooltip("Distancia máxima a la que puede engancharse el gancho.")]
    public float ganchoDistanciaMaxima = 30f;

    [Tooltip("Velocidad a la que el jugador vuela hacia el punto de enganche en m/s.")]
    public float ganchoVelocidad = 25f;

    [Tooltip("Distancia mínima al punto de enganche para soltar automáticamente.")]
    public float ganchoDistanciaLlegada = 1.5f;

    [Header("Cable del Gancho (LineRenderer)")]
    [Tooltip("LineRenderer que dibuja el cable del gancho. " +
             "Asígnalo desde el Inspector. Puede vivir en un GameObject hijo vacío. " +
             "Configura su Material y ancho (Width) a tu gusto en el Inspector.")]
    public LineRenderer grappleLine;

    // el cable sale desde este transform (la mano/arma) en vez del centro de la cámara
    // si no lo asigno, uso la cámara como fallback para no tener errores
    [Tooltip("Origen del cable visual del gancho. " +
             "Coloca un GameObject vacío hijo de la cámara (p.ej. en la posición del arma) " +
             "y asígnalo aquí. El cable saldrá desde ese punto en lugar de desde el centro " +
             "de la cámara, lo que queda mucho mejor en primera persona. " +
             "Si se deja vacío, se usará la posición de la cámara como fallback.")]
    public Transform grappleOrigin;

    [Header("Cámara Primera Persona")]
    [Tooltip("Referencia a la cámara hija del jugador.")]
    public Camera camaraPrimeraPersona;

    [Tooltip("Sensibilidad del ratón.")]
    public float sensibilidadRaton = 0.15f;

    [Tooltip("Límite de rotación vertical hacia arriba (grados).")]
    public float limiteVerticalArriba = 80f;

    [Tooltip("Límite de rotación vertical hacia abajo (grados).")]
    public float limiteVerticalAbajo = 80f;

    // las InputActions las creo aquí en código, sin asset en el editor
    private InputAction _accionMover;
    private InputAction _accionSaltar;
    private InputAction _accionMirar;
    // el dash (Shift izq) y el gancho (botón derecho) los leo directo con Keyboard/Mouse.current

    // cuando esto está a true bloqueo cámara, movimiento y dash (p.ej. al abrir el inventario)
    public bool InputBloqueado { get; set; }

    private CharacterController _characterController;

    private float _velocidadVertical         = 0f;
    private float _rotacionVerticalAcumulada = 0f;

    private int _saltosRestantes = 0;

    private bool    _isDashing      = false;
    private bool    _dashEnCooldown = false;
    private Vector3 _dashDireccion  = Vector3.zero;

    private bool    _isGrappling = false;
    private Vector3 _ganchoPoint = Vector3.zero;

    // aquí creo y registro todas las InputActions
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _saltosRestantes     = saltosMaximos;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        if (camaraPrimeraPersona == null)
            Debug.LogWarning("[PlayerMovement] ¡Asigna la cámara FPS en el Inspector!");

        if (grappleLine == null)
            Debug.LogWarning("[PlayerMovement] ¡Asigna el LineRenderer (grappleLine) en el Inspector!");

        if (grappleOrigin == null)
            Debug.LogWarning("[PlayerMovement] grappleOrigin no asignado. " +
                             "Se usará la posición de la cámara como origen del cable. " +
                             "Para un mejor resultado FPS, crea un GameObject vacío hijo " +
                             "de la cámara y asígnalo aquí.");

        // movimiento WASD + flechas usando un composite 2DVector
        _accionMover = new InputAction(
            name: "Mover",
            type: InputActionType.Value,
            expectedControlType: "Vector2"
        );
        var bindingWASD = _accionMover.AddCompositeBinding("2DVector");
        bindingWASD.With("Up",    "<Keyboard>/w");
        bindingWASD.With("Down",  "<Keyboard>/s");
        bindingWASD.With("Left",  "<Keyboard>/a");
        bindingWASD.With("Right", "<Keyboard>/d");

        var bindingFlechas = _accionMover.AddCompositeBinding("2DVector");
        bindingFlechas.With("Up",    "<Keyboard>/upArrow");
        bindingFlechas.With("Down",  "<Keyboard>/downArrow");
        bindingFlechas.With("Left",  "<Keyboard>/leftArrow");
        bindingFlechas.With("Right", "<Keyboard>/rightArrow");

        _accionSaltar = new InputAction(name: "Saltar", type: InputActionType.Button);
        _accionSaltar.AddBinding("<Keyboard>/space");

        // el delta del ratón me da cuánto se movió entre frames, perfecto para la cámara FPS
        _accionMirar = new InputAction(
            name: "Mirar",
            type: InputActionType.Value,
            expectedControlType: "Vector2"
        );
        _accionMirar.AddBinding("<Mouse>/delta");

        _accionMover.Enable();
        _accionSaltar.Enable();
        _accionMirar.Enable();
    }

    private void Start()
    {
        if (grappleLine != null)
        {
            grappleLine.positionCount = 2;     // el cable tiene siempre 2 puntos: origen y destino
            grappleLine.enabled       = false; // empieza invisible hasta que lance el gancho
        }
    }

    // limpio las InputActions para no dejar referencias sueltas al destruir el objeto
    private void OnDestroy()
    {
        _accionMover.Disable();
        _accionSaltar.Disable();
        _accionMirar.Disable();

        _accionMover.Dispose();
        _accionSaltar.Dispose();
        _accionMirar.Dispose();
    }

    // orden de prioridad: gancho > dash > movimiento normal
    private void Update()
    {
        if (InputBloqueado) return;

        ManejarCamara();      // siempre activa
        ManejarGancho();      // prioridad máxima de estado
        ManejarDash();        // solo si no hay gancho activo
        ManejarMovimiento();  // bloqueado según el estado activo
    }

    // actualizo el cable en LateUpdate para que se mueva después de que todo se haya movido
    private void LateUpdate()
    {
        ActualizarCableVisual();
    }

    private void ManejarCamara()
    {
        if (camaraPrimeraPersona == null) return;

        Vector2 deltaRaton = _accionMirar.ReadValue<Vector2>();
        float mouseX = deltaRaton.x * sensibilidadRaton;
        float mouseY = deltaRaton.y * sensibilidadRaton;

        // roto el cuerpo del jugador en horizontal y la cámara en vertical
        transform.Rotate(Vector3.up * mouseX);

        _rotacionVerticalAcumulada -= mouseY;
        _rotacionVerticalAcumulada  = Mathf.Clamp(
            _rotacionVerticalAcumulada,
            -limiteVerticalArriba,
            limiteVerticalAbajo
        );
        camaraPrimeraPersona.transform.localRotation =
            Quaternion.Euler(_rotacionVerticalAcumulada, 0f, 0f);
    }

    private void ManejarGancho()
    {
        // botón derecho → intento lanzar el gancho
        if (!_isGrappling && Mouse.current != null
            && Mouse.current.rightButton.wasPressedThisFrame)
        {
            IntentarLanzarGancho();
        }

        // si salto mientras el gancho está activo, lo cancelo con un pequeño impulso
        if (_isGrappling && _accionSaltar.WasPressedThisFrame())
        {
            CancelarGancho(aplicarImpulso: true);
            return;
        }

        // si llego al punto de enganche lo suelto automáticamente
        if (_isGrappling)
        {
            float distanciaAlPunto = Vector3.Distance(transform.position, _ganchoPoint);
            if (distanciaAlPunto < ganchoDistanciaLlegada)
            {
                CancelarGancho(aplicarImpulso: false);
                Debug.Log("[PlayerMovement] Gancho: llegamos al punto de enganche.");
            }
        }
    }

    // raycast desde el centro de la cámara hacia adelante; si impacta activo el gancho
    private void IntentarLanzarGancho()
    {
        Ray rayo = new Ray(
            camaraPrimeraPersona.transform.position,
            camaraPrimeraPersona.transform.forward
        );

        if (Physics.Raycast(rayo, out RaycastHit impacto, ganchoDistanciaMaxima))
        {
            _ganchoPoint       = impacto.point;
            _isGrappling       = true;
            _isDashing         = false; // el gancho cancela el dash si estaba activo
            _velocidadVertical = 0f;

            if (grappleLine != null)
                grappleLine.enabled = true;

            Debug.Log($"[PlayerMovement] Gancho enganchado en: {_ganchoPoint} " +
                      $"(objeto: {impacto.collider.name})");
        }
        else
        {
            Debug.Log("[PlayerMovement] Gancho: sin impacto.");
        }
    }

    // desactivo el gancho y apago el cable; con aplicarImpulso le doy un saltito al soltarlo
    private void CancelarGancho(bool aplicarImpulso)
    {
        _isGrappling = false;

        if (aplicarImpulso)
        {
            _velocidadVertical = Mathf.Sqrt(fuerzaSalto * -2f * gravedad);
            Debug.Log("[PlayerMovement] Gancho cancelado con impulso de salto.");
        }
        else
        {
            _velocidadVertical = 0f;
        }

        if (grappleLine != null)
            grappleLine.enabled = false;
    }

    // posición 0 del LineRenderer = origen del cable (mano/arma o cámara como fallback)
    // posición 1 = punto fijo en la geometría de la escena donde se enganchó
    private void ActualizarCableVisual()
    {
        if (!_isGrappling || grappleLine == null) return;

        // si no asigné grappleOrigin en el Inspector uso la cámara para evitar NullReferenceException
        Vector3 origenCable = (grappleOrigin != null)
            ? grappleOrigin.position
            : camaraPrimeraPersona.transform.position;

        grappleLine.SetPosition(0, origenCable);
        grappleLine.SetPosition(1, _ganchoPoint);
    }

    private void ManejarDash()
    {
        if (_isGrappling || _isDashing || _dashEnCooldown) return;

        if (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            Vector2 inputMover       = _accionMover.ReadValue<Vector2>();
            Vector3 direccionDeseada = (transform.right   * inputMover.x)
                                     + (transform.forward * inputMover.y);

            // si no me estoy moviendo, el dash va hacia adelante
            if (direccionDeseada.magnitude < 0.1f)
                direccionDeseada = transform.forward;

            _dashDireccion = direccionDeseada.normalized;
            StartCoroutine(CorrutinaDash());

            Debug.Log($"[PlayerMovement] Dash iniciado hacia: {_dashDireccion}");
        }
    }

    // el dash dura dashDuracion segundos y luego espera el cooldown antes de poder volver a usarlo
    private IEnumerator CorrutinaDash()
    {
        _isDashing         = true;
        _velocidadVertical = 0f;

        yield return new WaitForSeconds(dashDuracion);

        _isDashing      = false;
        _dashEnCooldown = true;

        Debug.Log("[PlayerMovement] Dash finalizado. Cooldown activo...");

        yield return new WaitForSeconds(dashCooldown);

        _dashEnCooldown = false;
        Debug.Log("[PlayerMovement] Dash disponible.");
    }

    private void ManejarMovimiento()
    {
        // prioridad 1: si el gancho está activo me muevo hacia el punto de enganche
        if (_isGrappling)
        {
            Vector3 direccionAlGancho = (_ganchoPoint - transform.position).normalized;
            _characterController.Move(direccionAlGancho * ganchoVelocidad * Time.deltaTime);
            return;
        }

        // prioridad 2: si el dash está activo me muevo en su dirección
        if (_isDashing)
        {
            _characterController.Move(_dashDireccion * dashVelocidad * Time.deltaTime);
            return;
        }

        // movimiento normal con gravedad y doble salto
        Vector2 inputMover = _accionMover.ReadValue<Vector2>();
        Vector3 direccion  = (transform.right   * inputMover.x)
                           + (transform.forward * inputMover.y);

        if (_characterController.isGrounded)
        {
            _saltosRestantes = saltosMaximos; // recargo los saltos al tocar el suelo
            if (_velocidadVertical < 0f)
                _velocidadVertical = -2f;     // valor negativo pequeño para quedarme pegado al suelo
        }

        if (_accionSaltar.WasPressedThisFrame() && _saltosRestantes > 0)
        {
            _velocidadVertical = 0f;
            _velocidadVertical = Mathf.Sqrt(fuerzaSalto * -2f * gravedad);
            _saltosRestantes--;
            Debug.Log($"[PlayerMovement] Salto. Restantes: {_saltosRestantes}");
        }

        _velocidadVertical += gravedad * Time.deltaTime;
        direccion.y         = _velocidadVertical;

        _characterController.Move(
            direccion * velocidadMovimiento * Time.deltaTime
            + Vector3.up * _velocidadVertical * Time.deltaTime
        );
    }
}
