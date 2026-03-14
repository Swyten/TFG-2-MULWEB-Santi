using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ADAPTADOR DE UI — Panel de nivel y barra de experiencia (XP).
/// Versión corregida: genera el sprite de la barra por código,
/// eliminando la dependencia del paquete 2D Sprite de Unity.
/// </summary>
public class PanelNivelUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private TextMeshProUGUI textoNivel;
    [SerializeField] private Image barraXP;
    [SerializeField] private TextMeshProUGUI textoBarraXP;

    // ── Ciclo de vida ─────────────────────────────────────────────────────────

    private void Awake()
    {
        // Genera y asigna un sprite cuadrado puro por código.
        // Esto elimina el efecto "burbuja" causado por UISprite (9-slice).
        if (barraXP != null)
        {
            barraXP.sprite    = CrearSpriteBlanco();
            barraXP.type      = Image.Type.Filled;
            barraXP.fillMethod = Image.FillMethod.Horizontal;
            barraXP.fillOrigin = (int)Image.OriginHorizontal.Left;
            barraXP.fillAmount = 1f;
        }
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>
    /// Recibe (xpActual, xpSiguienteNivel, nivel) y refresca toda la zona inferior.
    /// </summary>
    public void RefrescarPanel(int xpActual, int xpSiguienteNivel, int nivel)
    {
        if (textoNivel != null)
            textoNivel.text = $"Nivel {nivel}";

        if (barraXP != null)
            barraXP.fillAmount = (xpSiguienteNivel > 0)
                ? (float)xpActual / xpSiguienteNivel
                : 0f;

        if (textoBarraXP != null)
            textoBarraXP.text = $"{xpActual} / {xpSiguienteNivel} XP";
    }

    // ── Helpers privados ──────────────────────────────────────────────────────

    /// <summary>
    /// Crea un Sprite de 1×1 píxel blanco puro.
    /// Sin bordes, sin 9-slice → la Image Filled se rellena de forma limpia.
    /// </summary>
    private Sprite CrearSpriteBlanco()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f)
        );
    }
}
