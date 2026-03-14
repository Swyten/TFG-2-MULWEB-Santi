using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ADAPTADOR DE UI — Barra de salud (HP).
/// ► Vive en la capa de Infraestructura/UI; nunca contiene lógica de negocio.
/// ► Se suscribe al evento OnVidaCambiada del dominio y refresca la Image rellena.
/// ► La barra NO muestra texto: solo el relleno rojo que varía entre 0 y 1.
///
/// Configuración en el Inspector:
///   • barraRelleno → la Image interior con Image Type: Filled / Fill Method: Horizontal
/// </summary>
public class BarraVidaUI : MonoBehaviour
{
    [Header("Referencia de UI")]
    [Tooltip("Image interior de la barra con Image Type: Filled.")]
    [SerializeField] private Image barraRelleno;

    // ── Ciclo de vida ────────────────────────────────────────────────────────

    private void OnDestroy()
    {
        // Buena práctica: desuscribirse al destruir el objeto para evitar memory leaks
        // (el GameManager es quien pasa la referencia; la desuscripción la gestiona él)
    }

    // ── API pública (llamada por el GameManager al suscribirse al evento) ─────

    /// <summary>
    /// Recibe (vidaActual, vidaMaxima) y actualiza el fillAmount de la barra.
    /// fillAmount ∈ [0, 1]: 1 = barra llena, 0 = barra vacía.
    /// </summary>
    public void RefrescarBarra(float vidaActual, float vidaMaxima)
    {
        if (barraRelleno == null)
        {
            Debug.LogWarning("[BarraVidaUI] barraRelleno no asignado en el Inspector.");
            return;
        }

        barraRelleno.fillAmount = (vidaMaxima > 0f) ? (vidaActual / vidaMaxima) : 0f;
    }
}
