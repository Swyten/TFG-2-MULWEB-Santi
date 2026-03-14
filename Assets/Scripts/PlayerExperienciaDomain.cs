using System;
using UnityEngine;

/// <summary>
/// CAPA DE DOMINIO — Gestión pura de la experiencia y el nivel del jugador.
/// ► Sin dependencias de UI ni de Unity (salvo Mathf y Debug para logging).
/// ► Emite eventos tipados que los Adaptadores de UI consumen de forma reactiva.
/// ► Los datos (nivel, XP, vidaMaxima) son el contrato que se enviará a Firebase
///   mediante el servidor MCP y los flujos n8n en la fase de integración cloud.
/// </summary>
public class PlayerExperienciaDomain
{
    // ── Eventos del dominio ──────────────────────────────────────────────────

    /// <summary>
    /// Se dispara al ganar XP o al subir de nivel.
    /// Parámetros: (experienciaActual, experienciaParaSiguienteNivel, nivelActual)
    /// </summary>
    public event Action<int, int, int> OnExperienciaCambiada;

    /// <summary>
    /// Se dispara exclusivamente cuando el jugador sube de nivel.
    /// Parámetros: (nivelNuevo, nuevaVidaMaxima)
    /// El GameManager puede escuchar esto para actualizar PlayerVidaDomain.
    /// </summary>
    public event Action<int, float> OnNivelSubido;

    // ── Estado privado ───────────────────────────────────────────────────────
    private int   _nivel;
    private int   _experienciaActual;
    private int   _experienciaParaSiguienteNivel;
    private float _vidaMaxima;

    // ── Propiedades de solo lectura ──────────────────────────────────────────
    public int   Nivel                        => _nivel;
    public int   ExperienciaActual            => _experienciaActual;
    public int   ExperienciaParaSiguienteNivel => _experienciaParaSiguienteNivel;
    public float VidaMaxima                   => _vidaMaxima;

    // ── Constructor ──────────────────────────────────────────────────────────
    /// <param name="vidaMaximaInicial">Vida máxima de partida para calcular crecimientos.</param>
    public PlayerExperienciaDomain(float vidaMaximaInicial = 100f)
    {
        _nivel                        = 1;
        _experienciaActual            = 0;
        _experienciaParaSiguienteNivel = 100;
        _vidaMaxima                   = vidaMaximaInicial;
    }

    // ── Casos de uso ─────────────────────────────────────────────────────────

    /// <summary>
    /// Concede una cantidad de experiencia al jugador.
    /// Dispara SubirNivel() en bucle si la XP supera el umbral varias veces.
    /// </summary>
    public void GanarExperiencia(int cantidad)
    {
        _experienciaActual += cantidad;
        Debug.Log($"[Dominio·XP] Has ganado {cantidad} XP. " +
                  $"({_experienciaActual}/{_experienciaParaSiguienteNivel})");

        // Bucle para cubrir subidas de varios niveles de golpe
        while (_experienciaActual >= _experienciaParaSiguienteNivel)
        {
            SubirNivel();
        }

        // Notifica a los adaptadores con el estado actual (ya post-subida si aplica)
        OnExperienciaCambiada?.Invoke(
            _experienciaActual,
            _experienciaParaSiguienteNivel,
            _nivel
        );
    }

    // ── Métodos privados ──────────────────────────────────────────────────────

    /// <summary>
    /// Ejecuta la lógica de subida de nivel:
    /// conserva XP sobrante, incrementa nivel, mejora stats y recalcula umbral.
    /// </summary>
    private void SubirNivel()
    {
        int sobrante = _experienciaActual - _experienciaParaSiguienteNivel;

        _nivel++;
        _vidaMaxima                   += 20f;
        _experienciaParaSiguienteNivel  = Mathf.RoundToInt(_experienciaParaSiguienteNivel * 1.5f);
        _experienciaActual             = sobrante;

        Debug.Log($"[Dominio·XP] ¡Nivel subido! Ahora eres nivel {_nivel}. " +
                  $"Vida máxima: {_vidaMaxima} | " +
                  $"XP para siguiente nivel: {_experienciaParaSiguienteNivel}");

        // Notifica al GameManager (que actualizará PlayerVidaDomain)
        OnNivelSubido?.Invoke(_nivel, _vidaMaxima);
    }
}
