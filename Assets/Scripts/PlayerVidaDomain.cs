using System;
using UnityEngine;

/// <summary>
/// CAPA DE DOMINIO — Gestión pura de la vida del jugador.
/// ► No depende de MonoBehaviour, ni de UI, ni de ningún framework externo.
/// ► Emite eventos tipados que los Adaptadores escuchan para actualizar la vista.
/// ► Preparado para ser instanciado y controlado desde un GameManager central,
///   o serializado/deserializado hacia Firebase vía n8n en fases posteriores.
/// </summary>
public class PlayerVidaDomain
{
    // ── Eventos del dominio ──────────────────────────────────────────────────
    /// <summary>
    /// Se dispara cada vez que la vida cambia (daño o curación).
    /// Parámetros: (vidaActual, vidaMaxima)
    /// Los adaptadores de UI se suscriben aquí para refrescar la barra de HP.
    /// </summary>
    public event Action<float, float> OnVidaCambiada;

    // ── Estado privado ───────────────────────────────────────────────────────
    private float _vidaActual;
    private float _vidaMaxima;

    // ── Propiedades de solo lectura (acceso externo seguro) ──────────────────
    public float VidaActual => _vidaActual;
    public float VidaMaxima => _vidaMaxima;

    // ── Constructor ──────────────────────────────────────────────────────────
    /// <param name="vidaMaxima">Vida máxima con la que arranca el jugador.</param>
    public PlayerVidaDomain(float vidaMaxima)
    {
        _vidaMaxima = vidaMaxima;
        _vidaActual = vidaMaxima;
    }

    // ── Casos de uso (Lógica de negocio) ─────────────────────────────────────

    /// <summary>
    /// Aplica una cantidad de daño al jugador.
    /// La vida nunca bajará por debajo de 0.
    /// </summary>
    public void RecibirDanio(float cantidad)
    {
        _vidaActual = Mathf.Max(0f, _vidaActual - cantidad);
        Debug.Log($"[Dominio·Vida] Daño: -{cantidad} | Vida: {_vidaActual}/{_vidaMaxima}");
        OnVidaCambiada?.Invoke(_vidaActual, _vidaMaxima);
    }

    /// <summary>
    /// Cura al jugador en una cantidad dada.
    /// La vida nunca superará la vida máxima.
    /// </summary>
    public void Curar(float cantidad)
    {
        _vidaActual = Mathf.Min(_vidaMaxima, _vidaActual + cantidad);
        Debug.Log($"[Dominio·Vida] Curación: +{cantidad} | Vida: {_vidaActual}/{_vidaMaxima}");
        OnVidaCambiada?.Invoke(_vidaActual, _vidaMaxima);
    }

    /// <summary>
    /// Actualiza la vida máxima del jugador (p.ej. al subir de nivel).
    /// Reajusta la vida actual si ahora excede el nuevo máximo.
    /// </summary>
    public void SetVidaMaxima(float nuevaMaxima)
    {
        _vidaMaxima = nuevaMaxima;
        _vidaActual = Mathf.Min(_vidaActual, _vidaMaxima);
        Debug.Log($"[Dominio·Vida] Nueva vida máxima: {_vidaMaxima}");
        OnVidaCambiada?.Invoke(_vidaActual, _vidaMaxima);
    }
}
