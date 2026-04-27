using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Adapter del inventario: une el dominio con la UI y con el sistema de armas
// Tab → abre/cierra el inventario
// Al abrir: panel visible, cursor libre, input del jugador bloqueado
// Al cerrar: panel oculto, cursor bloqueado, input reactivo
// Al recoger un arma: se crea un slot en el contenedor
// Al pulsar "Equipar": se llama a WeaponAdapter para equipar el arma
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

    // guardo la relación entre ArmaInventario y su ArmaDefinicion para recuperar prefabs al equipar
    private readonly Dictionary<ArmaInventario, ArmaDefinicion> _mapaDefiniciones = new();
    // guardo también el botón de cada slot para cambiar su texto entre "Equipar" y "Desequipar"
    private readonly Dictionary<ArmaInventario, Button> _mapaBotones = new();

    private void Awake()
    {
        _dominio = new InventarioDomain();
        _dominio.OnAbierto        += AlAbrir;
        _dominio.OnCerrado        += AlCerrar;
        _dominio.OnArmaAgregada   += AlAgregarArma;
        _dominio.OnArmaEquipada   += AlEquiparArma;
        _dominio.OnArmaDesequipada += AlDesequiparArma;

        if (panelInventario != null)
            panelInventario.SetActive(false); // empieza oculto
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
        _dominio.OnAbierto        -= AlAbrir;
        _dominio.OnCerrado        -= AlCerrar;
        _dominio.OnArmaAgregada   -= AlAgregarArma;
        _dominio.OnArmaEquipada   -= AlEquiparArma;
        _dominio.OnArmaDesequipada -= AlDesequiparArma;
    }

    // llamado por GameManager.AgregarArmaAlInventario()
    // convierto la definición Unity en datos de dominio y los añado al inventario
    public void AgregarArma(ArmaDefinicion definicion)
    {
        ArmaInventario datos = definicion.CrearDatosDominio();
        _mapaDefiniciones[datos] = definicion; // guardo la referencia para poder equiparla después
        _dominio.AgregarArma(datos);
    }

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

    // cuando equipo un arma cambio el texto de su botón a "Desequipar" y el del resto a "Equipar"
    private void AlEquiparArma(ArmaInventario arma)
    {
        foreach (var par in _mapaBotones)
        {
            TMP_Text textoBoton = par.Value.GetComponentInChildren<TMP_Text>();
            if (textoBoton != null)
                textoBoton.text = par.Key == arma ? "Desequipar" : "Equipar";
        }
        Debug.Log($"[InventarioAdapter] Arma equipada: {arma.Nombre}");
    }

    private void AlDesequiparArma()
    {
        foreach (Button boton in _mapaBotones.Values)
        {
            TMP_Text textoBoton = boton.GetComponentInChildren<TMP_Text>();
            if (textoBoton != null)
                textoBoton.text = "Equipar";
        }
        Debug.Log("[InventarioAdapter] Arma desequipada.");
    }

    private void ConfigurarSlot(GameObject slot, ArmaInventario arma)
    {
        // asigno el icono a la última Image del slot (la primera suele ser el fondo)
        if (_mapaDefiniciones.TryGetValue(arma, out ArmaDefinicion def) && def.iconoUI != null)
        {
            Image[] imagenes = slot.GetComponentsInChildren<Image>();
            if (imagenes.Length > 1)
                imagenes[imagenes.Length - 1].sprite = def.iconoUI;
        }

        // nombre del arma en el primer TMP_Text que encuentro
        TMP_Text textoNombre = slot.GetComponentInChildren<TMP_Text>();
        if (textoNombre != null)
            textoNombre.text = arma.Nombre;

        // botón de equipar/desequipar: guardo la referencia y le asigno el listener
        Button botonEquipar = slot.GetComponentInChildren<Button>();
        if (botonEquipar != null)
        {
            _mapaBotones[arma] = botonEquipar;
            ArmaInventario armaCap = arma; // capturo la variable para el lambda
            botonEquipar.onClick.AddListener(() => AlternarEquipado(armaCap));
        }
    }

    private void AlternarEquipado(ArmaInventario arma)
    {
        if (_dominio.ArmaEquipada == arma)
        {
            // si ya estaba equipada, la desequipo
            _dominio.DesequiparArma();
            if (weaponAdapter != null)
                weaponAdapter.DesequiparArma();
        }
        else
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
                Debug.LogWarning("[InventarioAdapter] weaponAdapter no asignado.");
        }

        _dominio.Cerrar(); // cierro el inventario después de equipar/desequipar
    }
}
