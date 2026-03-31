using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ADAPTADOR DE UI — Barra de salud (HP).
/// Se auto-registra en el GameManager en Start() para garantizar
/// que la suscripción ocurre DESPUÉS de que GameManager.Awake() haya
/// inicializado el dominio, independientemente del orden de ejecución.
/// </summary>
public class BarraVidaUI : MonoBehaviour
{
    [Header("Referencia de UI")]
    [Tooltip("Image interior de la barra con Image Type: Filled / Fill Method: Horizontal.")]
    [SerializeField] private Image barraRelleno;

    private void Awake()
    {
        if (barraRelleno == null)
            Debug.LogError("[BarraVidaUI] 'barraRelleno' no asignado en el Inspector.");
    }

    private void Start()
    {
        // Start se ejecuta después de TODOS los Awake, así que GameManager
        // ya ha inicializado el dominio cuando llegamos aquí.
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.RegistrarBarraVida(this);
            Debug.Log("[BarraVidaUI] Registrada en GameManager correctamente.");
        }
        else
        {
            Debug.LogError("[BarraVidaUI] GameManager.Instancia es null en Start. " +
                           "Asegúrate de que GameManager está en la escena.");
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instancia != null)
            GameManager.Instancia.DesregistrarBarraVida(this);
    }

    /// <summary>Llamado por el dominio de vida cada vez que la vida cambia.</summary>
    public void RefrescarBarra(float vidaActual, float vidaMaxima)
    {
        if (barraRelleno == null) return;
        barraRelleno.fillAmount = (vidaMaxima > 0f) ? (vidaActual / vidaMaxima) : 0f;
        Debug.Log($"[BarraVidaUI] fill={barraRelleno.fillAmount:F2} ({vidaActual}/{vidaMaxima})");
    }
}
