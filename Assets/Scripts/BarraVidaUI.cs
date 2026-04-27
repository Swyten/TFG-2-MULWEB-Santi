using UnityEngine;
using UnityEngine.UI;

// Adapter de UI: muestra la barra de vida del jugador
// Me registro en el GameManager desde Start() para garantizar que Awake() ya ha inicializado el dominio
// De esta forma no me preocupo por el orden de ejecución de scripts
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
        // Start se ejecuta después de TODOS los Awake, así que GameManager ya existe cuando llego aquí
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

    // el dominio de vida llama a este método cada vez que la vida cambia
    public void RefrescarBarra(float vidaActual, float vidaMaxima)
    {
        if (barraRelleno == null) return;
        barraRelleno.fillAmount = (vidaMaxima > 0f) ? (vidaActual / vidaMaxima) : 0f;
        Debug.Log($"[BarraVidaUI] fill={barraRelleno.fillAmount:F2} ({vidaActual}/{vidaMaxima})");
    }
}
