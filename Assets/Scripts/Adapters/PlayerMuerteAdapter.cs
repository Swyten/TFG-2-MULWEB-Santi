using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMuerteAdapter : MonoBehaviour
{
    [Header("UI Game Over")]
    [Tooltip("Panel de Game Over. Debe estar desactivado al inicio.")]
    [SerializeField] private GameObject panelGameOver;

    [Header("Reinicio")]
    [SerializeField] private float delayReinicio = 3f;
    [SerializeField] private string nombreEscena = "";

    private void Awake()
    {
        if (panelGameOver == null)
            Debug.LogWarning("[PlayerMuerteAdapter] 'panelGameOver' no asignado — " +
                             "no se mostrará pantalla de Game Over, pero la escena sí se recargará.");
        else
            Debug.Log("[PlayerMuerteAdapter] Listo. Panel Game Over asignado.");
    }

    public void OnJugadorMuerto()
    {
        Debug.Log("[PlayerMuerteAdapter] ¡GAME OVER! Ejecutando secuencia de muerte...");

        if (panelGameOver != null)
            panelGameOver.SetActive(true);

        if (TryGetComponent(out PlayerMovement movimiento))
            movimiento.enabled = false;

        foreach (var mb in GetComponentsInChildren<MonoBehaviour>())
            if (mb is WeaponAdapter arma) arma.enabled = false;

        Invoke(nameof(RecargarEscena), delayReinicio);
    }

    private void RecargarEscena()
    {
        string escena = !string.IsNullOrEmpty(nombreEscena)
            ? nombreEscena
            : SceneManager.GetActiveScene().name;

        Debug.Log($"[PlayerMuerteAdapter] Recargando escena '{escena}'...");
        SceneManager.LoadScene(escena);
    }
}
