using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// ADAPTADOR — Inventario del jugador.
///
/// CONFIGURACIÓN EN EL INSPECTOR:
///   • panelInventario   → GameObject raíz del panel de inventario en el Canvas.
///   • contenedorSlots   → Transform (Layout Group) donde se generan los slots de arma.
///   • prefabSlotArma    → Prefab de un slot. Debe tener:
///                           - Image (icono del arma) como hijo.
///                           - Text o TMP_Text con el nombre del arma como hijo.
///                           - Button con el texto "Equipar" como hijo.
///   • playerMovement    → PlayerMovement del jugador.
///   • weaponAdapter     → WeaponAdapter del jugador.
///
/// COMPORTAMIENTO:
///   • Tab → Abre / cierra el inventario.
///   • Al abrir  → Panel visible, cursor libre, input del jugador bloqueado.
///   • Al cerrar → Panel oculto, cursor bloqueado, input reactivo.
///   • Al recoger un arma → Se crea un slot en el contenedor.
///   • Al pulsar "Equipar" en un slot → Se equipa el arma en WeaponAdapter.
/// </summary>
public class InventarioAdapter : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelInventario;
    [SerializeField] private Transform  contenedorSlots;
    [Tooltip("Prefab del slot de arma (Image icono + Text nombre + Button equipar).")]
    [SerializeField] private GameObject prefabSlotArma;

    [Header("Referencias")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private WeaponAdapter  weaponAdapter;

    private InventarioDomain _dominio;

    // Mapa ArmaInventario → ArmaDefinicion para recuperar prefabs al equipar
    private readonly Dictionary<ArmaInventario, ArmaDefinicion> _mapaDefiniciones = new();

    // ── Ciclo de vida ─────────────────────────────────────────────────────────

    private void Awake()
    {
        _dominio = new InventarioDomain();
        _dominio.OnAbierto      += AlAbrir;
        _dominio.OnCerrado      += AlCerrar;
        _dominio.OnArmaAgregada += AlAgregarArma;
        _dominio.OnArmaEquipada += AlEquiparArma;

        if (panelInventario != null)
            panelInventario.SetActive(false);
        else
            Debug.LogWarning("[InventarioAdapter] 'panelInventario' no asignado.");
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            _dominio.Toggle();
    }

    private void OnDestroy()
    {
        _dominio.OnAbierto      -= AlAbrir;
        _dominio.OnCerrado      -= AlCerrar;
        _dominio.OnArmaAgregada -= AlAgregarArma;
        _dominio.OnArmaEquipada -= AlEquiparArma;
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>
    /// Llamado por GameManager.AgregarArmaAlInventario().
    /// Mapea la definición Unity → datos de dominio y los añade al inventario.
    /// </summary>
    public void AgregarArma(ArmaDefinicion definicion)
    {
        ArmaInventario datos = definicion.CrearDatosDominio();
        _mapaDefiniciones[datos] = definicion;
        _dominio.AgregarArma(datos);
    }

    // ── Callbacks del dominio ─────────────────────────────────────────────────

    private void AlAbrir()
    {
        if (panelInventario != null) panelInventario.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (playerMovement != null) playerMovement.InputBloqueado = true;

        Debug.Log("[InventarioAdapter] Inventario abierto.");
    }

    private void AlCerrar()
    {
        if (panelInventario != null) panelInventario.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        if (playerMovement != null) playerMovement.InputBloqueado = false;

        Debug.Log("[InventarioAdapter] Inventario cerrado.");
    }

    private void AlAgregarArma(ArmaInventario arma)
    {
        if (contenedorSlots == null || prefabSlotArma == null)
        {
            Debug.LogWarning("[InventarioAdapter] contenedorSlots o prefabSlotArma no asignados. " +
                             "El arma se añadió al dominio pero no hay UI de slot.");
            return;
        }

        GameObject slot = Instantiate(prefabSlotArma, contenedorSlots);
        ConfigurarSlot(slot, arma);
        Debug.Log($"[InventarioAdapter] Slot creado para: {arma.Nombre}");
    }

    private void AlEquiparArma(ArmaInventario arma)
    {
        Debug.Log($"[InventarioAdapter] Arma equipada en dominio: {arma.Nombre}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void ConfigurarSlot(GameObject slot, ArmaInventario arma)
    {
        // Icono — busca la primera Image del slot (excluyendo el fondo si hay varios)
        if (_mapaDefiniciones.TryGetValue(arma, out ArmaDefinicion def) && def.iconoUI != null)
        {
            Image[] imagenes = slot.GetComponentsInChildren<Image>();
            // La última Image suele ser el icono (la primera es el fondo del slot)
            if (imagenes.Length > 1)
                imagenes[imagenes.Length - 1].sprite = def.iconoUI;
        }

        // Nombre — busca el primer Text hijo
        Text textoNombre = slot.GetComponentInChildren<Text>();
        if (textoNombre != null)
            textoNombre.text = arma.Nombre;

        // Botón equipar — busca el Button hijo
        Button botonEquipar = slot.GetComponentInChildren<Button>();
        if (botonEquipar != null)
        {
            // Captura local para el closure del listener
            ArmaInventario armaCap = arma;
            botonEquipar.onClick.AddListener(() => EquiparDesdeSlot(armaCap));
        }
    }

    private void EquiparDesdeSlot(ArmaInventario arma)
    {
        if (!_mapaDefiniciones.TryGetValue(arma, out ArmaDefinicion definicion))
        {
            Debug.LogWarning($"[InventarioAdapter] Sin definición para: {arma.Nombre}");
            return;
        }

        _dominio.EquiparArma(arma);

        if (weaponAdapter != null)
            weaponAdapter.EquiparArma(arma, definicion);
        else
            Debug.LogWarning("[InventarioAdapter] weaponAdapter no asignado — arma equipada en dominio pero no en escena.");

        // Cerrar inventario al equipar
        _dominio.Cerrar();
    }
}
