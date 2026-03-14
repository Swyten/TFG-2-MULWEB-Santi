using UnityEngine;
using UnityEngine.InputSystem; // ← Nuevo Input System (reemplaza a UnityEngine.Input)

/// <summary>
/// PlayerStats — Núcleo RPG del jugador.
/// Gestiona el nivel, la experiencia y la vida máxima.
/// Adjunta este script a tu GameObject "Player" en la escena.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  ESTADÍSTICAS PÚBLICAS
    // ─────────────────────────────────────────────

    [Header("Progresión")]
    /// <summary>Nivel actual del jugador. Empieza en 1.</summary>
    public int nivel = 1;

    /// <summary>Experiencia acumulada en el nivel actual.</summary>
    public int experienciaActual = 0;

    /// <summary>Experiencia necesaria para alcanzar el siguiente nivel.</summary>
    public int experienciaParaSiguienteNivel = 100;

    [Header("Salud")]
    /// <summary>Vida máxima del jugador. Aumenta al subir de nivel.</summary>
    public int vidaMaxima = 100;

    // ─────────────────────────────────────────────
    //  UNITY: UPDATE — TECLA DE PRUEBA
    // ─────────────────────────────────────────────

    /// <summary>
    /// Sólo para testing: pulsa X durante el juego para ganar 35 XP
    /// y comprobar que el sistema de niveles funciona correctamente.
    /// Usa el nuevo Input System en lugar del legacy UnityEngine.Input.
    /// </summary>
    void Update()
    {
        // CORRECCIÓN: Keyboard.current del nuevo Input System
        // en lugar del antiguo Input.GetKeyDown(KeyCode.X)
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            GanarExperiencia(35);
        }
    }

    // ─────────────────────────────────────────────
    //  MÉTODOS PÚBLICOS
    // ─────────────────────────────────────────────

    /// <summary>
    /// Otorga al jugador una cantidad de experiencia.
    /// Si supera el umbral del nivel actual, dispara SubirNivel().
    /// </summary>
    /// <param name="cantidad">Puntos de XP que se van a conceder.</param>
    public void GanarExperiencia(int cantidad)
    {
        experienciaActual += cantidad;
        Debug.Log($"Has ganado {cantidad} de experiencia. " +
                  $"(XP actual: {experienciaActual} / {experienciaParaSiguienteNivel})");

        // Bucle por si se suben varios niveles de golpe
        while (experienciaActual >= experienciaParaSiguienteNivel)
        {
            SubirNivel();
        }
    }

    // ─────────────────────────────────────────────
    //  MÉTODOS PRIVADOS
    // ─────────────────────────────────────────────

    /// <summary>
    /// Incrementa el nivel del jugador, mejora sus estadísticas
    /// y recalcula el umbral de XP para el siguiente nivel.
    /// La XP sobrante se conserva para no "desperdiciarla".
    /// </summary>
    private void SubirNivel()
    {
        // Conservar el exceso de XP por encima del umbral
        int experienciaSobrante = experienciaActual - experienciaParaSiguienteNivel;

        // Incrementar nivel
        nivel++;

        // Aumentar vida máxima como recompensa
        vidaMaxima += 20;

        // Nueva barra de XP: 1.5× la anterior (redondeado al entero más cercano)
        experienciaParaSiguienteNivel = Mathf.RoundToInt(experienciaParaSiguienteNivel * 1.5f);

        // Aplicar XP sobrante al nuevo nivel
        experienciaActual = experienciaSobrante;

        Debug.Log($"¡Nivel subido! Ahora eres nivel {nivel}. " +
                  $"Vida máxima: {vidaMaxima} | " +
                  $"XP para el siguiente nivel: {experienciaParaSiguienteNivel}");
    }
}
