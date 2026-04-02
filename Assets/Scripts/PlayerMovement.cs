using System.Collections;  // Necesario para IEnumerator (Corrutinas)
using UnityEngine;
using UnityEngine.InputSystem; // Nuevo Input System de Unity 6

/// <summary>
/// Controlador de movimiento en primera persona — Unity 6 / Nuevo Input System.
/// NO requiere un Input Action Asset (.inputactions) en el Editor.
///
/// CARACTERÍSTICAS COMPLETAS:
///   ✔ Movimiento WASD con CharacterController
///   ✔ Cámara FPS con ratón
///   ✔ Gravedad constante
///   ✔ Doble salto configurable
///   ✔ Dash con duración y cooldown (corrutina)
///   ✔ Gancho (Grappling Hook) con Raycast, vuelo y cancelación
///   ✔ Cable visual del gancho con LineRenderer + origen configurable
///
/// REQUISITOS:
///   - Paquete "Input System" instalado (Package Manager).
///   - CharacterController en el mismo GameObject.
///   - Cámara FPS como hijo del jugador, asignada en el Inspector.
///   - LineRenderer asignado en el Inspector (puede estar en un hijo vacío).
///   - GrappleOrigin: Transform hijo que marca el punto de salida del cable.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // =========================================================================
    // INSPECTOR — PARÁMETROS CONFIGURABLES
    // =========================================================================

    [Header("Movimiento")]
    [Tooltip("Velocidad de desplazamiento normal en m/s.")]
    public float velocidadMovimiento = 5f;

    // -------------------------------------------------------------------------
    [Header("Salto y Gravedad")]
    [Tooltip("Fuerza con la que el jugador salta.")]
    public float fuerzaSalto = 5f;

    [Tooltip("Número máximo de saltos antes de tocar suelo. 2 = doble salto.")]
    public int saltosMaximos = 2;

    [Tooltip("Intensidad de la gravedad (valor negativo).")]
    public float gravedad = -9.81f;

    // -------------------------------------------------------------------------
    [Header("Dash")]
    [Tooltip("Velocidad del impulso del Dash en m/s.")]
    public float dashVelocidad = 20f;

    [Tooltip("Duración del Dash en segundos.")]
    public float dashDuracion = 0.2f;

    [Tooltip("Tiempo de recarga del Dash en segundos.")]
    public float dashCooldown = 1.5f;

    // -------------------------------------------------------------------------
    [Header("Gancho (Grappling Hook)")]
    [Tooltip("Distancia máxima a la que puede engancharse el gancho.")]
    public float ganchoDistanciaMaxima = 30f;

    [Tooltip("Velocidad a la que el jugador vuela hacia el punto de enganche en m/s.")]
    public float ganchoVelocidad = 25f;

    [Tooltip("Distancia mínima al punto de enganche para soltar automáticamente.")]
    public float ganchoDistanciaLlegada = 1.5f;

    // -------------------------------------------------------------------------
    [Header("Cable del Gancho (LineRenderer)")]
    [Tooltip("LineRenderer que dibuja el cable del gancho. " +
             "Asígnalo desde el Inspector. Puede vivir en un GameObject hijo vacío. " +
             "Configura su Material y ancho (Width) a tu gusto en el Inspector.")]
    public LineRenderer grappleLine;

    [Tooltip("Origen del cable visual del gancho. " +
             "Coloca un GameObject vacío hijo de la cámara (p.ej. en la posición del arma) " +
             "y asígnalo aquí. El cable saldrá desde ese punto en lugar de desde el centro " +
             "de la cámara, lo que queda mucho mejor en primera persona. " +
             "Si se deja vacío, se usará la posición de la cámara como fallback.")]
    public Transform grappleOrigin;

    // -------------------------------------------------------------------------
    [Header("Cámara Primera Persona")]
    [Tooltip("Referencia a la cámara hija del jugador.")]
    public Camera camaraPrimeraPersona;

    [Tooltip("Sensibilidad del ratón.")]
    public float sensibilidadRaton = 0.15f;

    [Tooltip("Límite de rotación vertical hacia arriba (grados).")]
    public float limiteVerticalArriba = 80f;

    [Tooltip("Límite de rotación vertical hacia abajo (grados).")]
    public float limiteVerticalAbajo = 80f;

    // =========================================================================
    // INPUT ACTIONS — Creadas por código (sin Asset en el Editor)
    // =========================================================================

    private InputAction _accionMover;
    private InputAction _accionSaltar;
    private InputAction _accionMirar;
    // Dash   → Keyboard.current.leftShiftKey  (lectura directa)
    // Gancho → Mouse.current.rightButton      (lectura directa)

    // =========================================================================
    // VARIABLES PRIVADAS INTERNAS
    // =========================================================================

    /// <summary>
    /// Cuando es true, bloquea cámara, movimiento y dash (p. ej. al abrir el inventario).
    /// </summary>
    public bool InputBloqueado { get; set; }

    private CharacterController _characterController;

    // --- Movimiento general ---
    private float _velocidadVertical         = 0f;
    private float _rotacionVerticalAcumulada = 0f;

    // --- Doble salto ---
    private int _saltosRestantes = 0;

    // --- Dash ---
    private bool    _isDashing      = false;
    private bool    _dashEnCooldown = false;
    private Vector3 _dashDireccion  = Vector3.zero;

    // --- Gancho ---
    private bool    _isGrappling = false;
    private Vector3 _ganchoPoint = Vector3.zero;

    // =========================================================================
    // AWAKE — Inicialización y registro de InputActions
    // =========================================================================
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

        // Aviso si grappleOrigin no está asignado (no es error crítico, hay fallback)
        if (grappleOrigin == null)
            Debug.LogWarning("[PlayerMovement] grappleOrigin no asignado. " +
                             "Se usará la posición de la cámara como origen del cable. " +
                             "Para un mejor resultado FPS, crea un GameObject vacío hijo " +
                             "de la cámara y asígnalo aquí.");

        // --- Movimiento WASD + flechas (2D Vector Composite) ---
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

        // --- Salto (barra espaciadora) ---
        _accionSaltar = new InputAction(name: "Saltar", type: InputActionType.Button);
        _accionSaltar.AddBinding("<Keyboard>/space");

        // --- Mirada (delta del ratón) ---
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

    // =========================================================================
    // START — Estado inicial de componentes visuales
    // =========================================================================
    private void Start()
    {
        if (grappleLine != null)
        {
            grappleLine.positionCount = 2;    // El cable siempre tiene exactamente 2 puntos
            grappleLine.enabled       = false; // Invisible hasta que se active el gancho
        }
    }

    // =========================================================================
    // ONDESTROY — Liberación de recursos del Input System
    // =========================================================================
    private void OnDestroy()
    {
        _accionMover.Disable();
        _accionSaltar.Disable();
        _accionMirar.Disable();

        _accionMover.Dispose();
        _accionSaltar.Dispose();
        _accionMirar.Dispose();
    }

    // =========================================================================
    // UPDATE — Tick principal (lógica y estado)
    // =========================================================================
    private void Update()
    {
        if (InputBloqueado) return;

        ManejarCamara();      // Siempre activa
        ManejarGancho();      // Prioridad máxima de estado
        ManejarDash();        // Solo si no hay gancho
        ManejarMovimiento();  // Bloqueado según estado activo
    }

    // =========================================================================
    // LATE UPDATE — Actualización visual del cable (después de que todo se mueva)
    // =========================================================================
    private void LateUpdate()
    {
        ActualizarCableVisual();
    }

    // =========================================================================
    // CÁMARA FPS — Sin cambios
    // =========================================================================
    private void ManejarCamara()
    {
        if (camaraPrimeraPersona == null) return;

        Vector2 deltaRaton = _accionMirar.ReadValue<Vector2>();
        float mouseX = deltaRaton.x * sensibilidadRaton;
        float mouseY = deltaRaton.y * sensibilidadRaton;

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

    // =========================================================================
    // GANCHO — Detección, vuelo y cancelación (lógica sin cambios)
    // =========================================================================
    private void ManejarGancho()
    {
        if (!_isGrappling && Mouse.current != null
            && Mouse.current.rightButton.wasPressedThisFrame)
        {
            IntentarLanzarGancho();
        }

        if (_isGrappling && _accionSaltar.WasPressedThisFrame())
        {
            CancelarGancho(aplicarImpulso: true);
            return;
        }

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

    /// <summary>
    /// Lanza un Raycast desde la cámara. Si impacta, activa el gancho y enciende el cable.
    /// </summary>
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
            _isDashing         = false;
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

    /// <summary>
    /// Cancela el gancho y apaga el cable visual.
    /// </summary>
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

    // =========================================================================
    // CABLE VISUAL — Actualización frame a frame en LateUpdate
    //
    // Posición 0 (origen) → grappleOrigin.position
    //   Usamos el Transform configurable desde el Inspector para que el cable
    //   salga desde la mano/arma del jugador y se vea bien en primera persona.
    //   Si grappleOrigin no está asignado, se usa la cámara como fallback seguro.
    //
    // Posición 1 (destino) → _ganchoPoint
    //   Punto fijo en la geometría de la escena capturado al lanzar el gancho.
    // =========================================================================
    private void ActualizarCableVisual()
    {
        if (!_isGrappling || grappleLine == null) return;

        // Origen del cable: grappleOrigin si está asignado, cámara si no lo está.
        // El operador condicional ternario (?:) evita una NullReferenceException
        // si el desarrollador olvidó asignar grappleOrigin en el Inspector.
        Vector3 origenCable = (grappleOrigin != null)
            ? grappleOrigin.position
            : camaraPrimeraPersona.transform.position;

        // Posición 0 → origen del cable (mano/arma del jugador)
        grappleLine.SetPosition(0, origenCable);

        // Posición 1 → destino del cable (punto de enganche en la escena)
        grappleLine.SetPosition(1, _ganchoPoint);
    }

    // =========================================================================
    // DASH — Sin cambios
    // =========================================================================
    private void ManejarDash()
    {
        if (_isGrappling || _isDashing || _dashEnCooldown) return;

        if (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            Vector2 inputMover       = _accionMover.ReadValue<Vector2>();
            Vector3 direccionDeseada = (transform.right   * inputMover.x)
                                     + (transform.forward * inputMover.y);

            if (direccionDeseada.magnitude < 0.1f)
                direccionDeseada = transform.forward;

            _dashDireccion = direccionDeseada.normalized;
            StartCoroutine(CorrutinaDash());

            Debug.Log($"[PlayerMovement] Dash iniciado hacia: {_dashDireccion}");
        }
    }

    // =========================================================================
    // CORRUTINA DASH — Sin cambios
    // =========================================================================
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

    // =========================================================================
    // MOVIMIENTO — Sin cambios
    // =========================================================================
    private void ManejarMovimiento()
    {
        // PRIORIDAD 1: Gancho activo
        if (_isGrappling)
        {
            Vector3 direccionAlGancho = (_ganchoPoint - transform.position).normalized;
            _characterController.Move(direccionAlGancho * ganchoVelocidad * Time.deltaTime);
            return;
        }

        // PRIORIDAD 2: Dash activo
        if (_isDashing)
        {
            _characterController.Move(_dashDireccion * dashVelocidad * Time.deltaTime);
            return;
        }

        // MOVIMIENTO NORMAL (WASD + gravedad + doble salto)
        Vector2 inputMover = _accionMover.ReadValue<Vector2>();
        Vector3 direccion  = (transform.right   * inputMover.x)
                           + (transform.forward * inputMover.y);

        if (_characterController.isGrounded)
        {
            _saltosRestantes = saltosMaximos;
            if (_velocidadVertical < 0f)
                _velocidadVertical = -2f;
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
