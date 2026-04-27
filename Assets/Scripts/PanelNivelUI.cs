using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Adapter de UI: muestra el nivel y la barra de XP del jugador
// Genero el sprite de la barra por código para evitar el efecto "burbuja" del 9-slice de Unity
public class PanelNivelUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private TextMeshProUGUI textoNivel;
    [SerializeField] private Image barraXP;
    [SerializeField] private TextMeshProUGUI textoBarraXP;

    private void Awake()
    {
        // creo un sprite de 1×1 píxel blanco por código para que la barra Filled no tenga el bug del 9-slice
        if (barraXP != null)
        {
            barraXP.sprite    = CrearSpriteBlanco();
            barraXP.type      = Image.Type.Filled;
            barraXP.fillMethod = Image.FillMethod.Horizontal;
            barraXP.fillOrigin = (int)Image.OriginHorizontal.Left;
            barraXP.fillAmount = 1f;
        }
    }

    // recibo (xpActual, xpSiguienteNivel, nivel) desde el dominio y actualizo toda la zona inferior de la HUD
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

    // sprite de 1×1 blanco: sin bordes ni 9-slice, la barra Filled se rellena limpiamente
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
