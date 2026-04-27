using UnityEngine;
using UnityEngine.SceneManagement;

// Se activa cuando el jugador muere: muestra el Game Over y recarga la escena
public class PlayerMuerteAdapter : MonoBehaviour
{
    [Header("UI Game Over")]
    [Tooltip("Panel de Game Over. Debe estar desactivado al inicio.")]
    [SerializeField] private GameObject panelGameOver;

    [Header("Reinicio")]
    [SerializeField] private float delayReinicio = 3f;
    [SerializeField] private string nombreEscena = ""; // si está vacío recarga la escena actual

    private void Awake()
    {
        if (panelGameOver == null)
            Debug.LogWarning("[PlayerMuerteAdapter] 'panelGameOver' no asignado — " +
                             "no se mostrará pantalla de Game Over, pero la escena sí se recargará.");
        else
            Debug.Log("[PlayerMuerteAdapter] Listo. Panel Game Over asignado.");
    }

    // el GameManager llama a este método cuando PlayerVidaDomain emite OnJugadorMuerto
    public void OnJugadorMuerto()
    {
        Debug.Log("[PlayerMuerteAdapter] ¡GAME OVER! Ejecutando secuencia de muerte...");

        if (panelGameOver != null)
            panelGameOver.SetActive(true);

        // deshabilito el movimiento y el arma para que el jugador no pueda seguir jugando
        if (TryGetComponent(out PlayerMovement movimiento))
            movimiento.enabled = false;

        foreach (var mb in GetComponentsInChildren<MonoBehaviour>())
            if (mb is WeaponAdapter arma) arma.enabled = false;

        Invoke(nameof(RecargarEscena), delayReinicio);
    }

    private void RecargarEscena()
    {
        // si no especifiqué nombre de escena, recargo la que está activa
        string escena = !string.IsNullOrEmpty(nombreEscena)
            ? nombreEscena
            : SceneManager.GetActiveScene().name;

        Debug.Log($"[PlayerMuerteAdapter] Recargando escena '{escena}'...");
        SceneManager.LoadScene(escena);
    }
}
