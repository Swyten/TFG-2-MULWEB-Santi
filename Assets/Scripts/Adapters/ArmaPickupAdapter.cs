using UnityEngine;
using UnityEngine.InputSystem;

// Controla el arma recogible en el suelo (loot drop)
// Necesita un SphereCollider con "Is Trigger" activado para detectar cuando me acerco
// Al entrar en el trigger aparece el hint "Pulsa E para recoger"; al salir desaparece
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

    private void Awake()
    {
        if (definicion == null)
            Debug.LogError($"[ArmaPickupAdapter] '{gameObject.name}' no tiene 'definicion' asignada.");

        // me aseguro de que el collider es trigger aunque alguien lo haya dejado sin marcar
        if (TryGetComponent(out Collider col))
            col.isTrigger = true;

        if (textoRecogida != null)
            textoRecogida.SetActive(false); // empieza oculto
    }

    private void Update()
    {
        if (!_jugadorEnRango || definicion == null) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Recoger();
    }

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

    private void Recoger()
    {
        if (GameManager.Instancia == null)
        {
            Debug.LogWarning("[ArmaPickupAdapter] GameManager.Instancia es null.");
            return;
        }

        // le digo al GameManager que añada el arma al inventario y destruyo el pickup
        GameManager.Instancia.AgregarArmaAlInventario(definicion);
        Debug.Log($"[ArmaPickupAdapter] '{definicion.nombreArma}' recogida.");
        Destroy(gameObject);
    }
}
