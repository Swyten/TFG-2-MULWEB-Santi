using UnityEngine;
using UnityEngine.InputSystem; // nuevo Input System, ya no uso UnityEngine.Input

// Stats del jugador: nivel, XP y vida máxima
// Pongo este script en el GameObject "Player" de la escena
public class PlayerStats : MonoBehaviour
{
    [Header("Progresión")]
    public int nivel = 1;                           // empieza en nivel 1
    public int experienciaActual = 0;               // XP acumulada en el nivel actual
    public int experienciaParaSiguienteNivel = 100; // cuánta XP hace falta para subir

    [Header("Salud")]
    public int vidaMaxima = 100; // sube 20 cada vez que subo de nivel

    // solo para probar que el sistema funciona: pulso X y gano 35 XP
    // uso Keyboard.current del nuevo Input System en lugar del antiguo Input.GetKeyDown
    void Update()
    {
        if (Keyboard.current.xKey.wasPressedThisFrame)
            GanarExperiencia(35);
    }

    // le doy XP al jugador; si supera el umbral llamo a SubirNivel()
    public void GanarExperiencia(int cantidad)
    {
        experienciaActual += cantidad;
        Debug.Log($"Has ganado {cantidad} de experiencia. " +
                  $"(XP actual: {experienciaActual} / {experienciaParaSiguienteNivel})");

        // while en vez de if por si subo varios niveles de golpe con mucha XP
        while (experienciaActual >= experienciaParaSiguienteNivel)
            SubirNivel();
    }

    // sube el nivel, mejora stats y recalcula el umbral del siguiente nivel
    // guardo el exceso de XP para no perderla
    private void SubirNivel()
    {
        int experienciaSobrante = experienciaActual - experienciaParaSiguienteNivel;

        nivel++;
        vidaMaxima += 20;

        // cada nivel necesita 1.5× más XP que el anterior
        experienciaParaSiguienteNivel = Mathf.RoundToInt(experienciaParaSiguienteNivel * 1.5f);

        experienciaActual = experienciaSobrante; // aplico el sobrante al nuevo nivel

        Debug.Log($"¡Nivel subido! Ahora eres nivel {nivel}. " +
                  $"Vida máxima: {vidaMaxima} | " +
                  $"XP para el siguiente nivel: {experienciaParaSiguienteNivel}");
    }
}
