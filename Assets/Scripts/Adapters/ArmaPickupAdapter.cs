using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ADAPTADOR — Arma recogible en el mundo (loot drop).
///
/// CONFIGURACIÓN DEL PREFAB:
///   1. SphereCollider con "Is Trigger" activado (radio = radioRecogida).
///   2. Este componente ArmaPickupAdapter.
///   3. (Opcional) GameObject hijo "TextoRecogida" con Canvas world-space
///      para mostrar el hint "Pulsa E para recoger". Se activa/desactiva
///      automáticamente cuando el jugador entra/sale del rango.
///
/// CONFIGURACIÓN EN EL INSPECTOR:
///   • definicion       → ScriptableObject ArmaDefinicion del arma que representa.
///   • textoRecogida    → GameObject con el hint visual (puede dejarse vacío).
/// </summary>
[RequireComponent(typeof(Collider))]
public class ArmaPickupAdapter : MonoBehaviour
{
    [Header("Datos del Arma")]
    [Tooltip("ScriptableObject con los stats y prefabs del arma.")]
    [SerializeField] private ArmaDefinicion definicion;

    [Header("UI Hint (opcional)")]
    [Tooltip("GameObject con el texto 'Pulsa E para recoger'. Se activa al acercarse.")]
    [SerializeField] private GameObject textoRecogida;

    private bool _jugadorEnRango;

    // ── Ciclo de vida ─────────────────────────────────────────────────────────

    private void Awake()
    {
        if (definicion == null)
            Debug.LogError($"[ArmaPickupAdapter] '{gameObject.name}' no tiene 'definicion' asignada.");

        // Asegurar que el collider sea trigger
        if (TryGetComponent(out Collider col))
            col.isTrigger = true;

        if (textoRecogida != null)
            textoRecogida.SetActive(false);
    }

    private void Update()
    {
        if (!_jugadorEnRango || definicion == null) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Recoger();
    }

    // ── Trigger ───────────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _jugadorEnRango = true;
        if (textoRecogida != null) textoRecogida.SetActive(true);
        Debug.Log($"[ArmaPickupAdapter] Jugador en rango de '{definicion?.nombreArma}'.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _jugadorEnRango = false;
        if (textoRecogida != null) textoRecogida.SetActive(false);
    }

    // ── Recogida ──────────────────────────────────────────────────────────────

    private void Recoger()
    {
        if (GameManager.Instancia == null)
        {
            Debug.LogWarning("[ArmaPickupAdapter] GameManager.Instancia es null.");
            return;
        }

        GameManager.Instancia.AgregarArmaAlInventario(definicion);
        Debug.Log($"[ArmaPickupAdapter] '{definicion.nombreArma}' recogida.");
        Destroy(gameObject);
    }
}
