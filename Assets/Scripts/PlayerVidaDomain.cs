using System;
using UnityEngine;

/// <summary>
/// CAPA DE DOMINIO — Gestión pura de la vida del jugador.
/// ► No depende de MonoBehaviour, ni de UI, ni de ningún framework externo.
/// ► Emite eventos tipados que los Adaptadores escuchan para actualizar la vista.
/// </summary>
public class PlayerVidaDomain
{
    // ── Eventos ──────────────────────────────────────────────────────────────

    /// <summary>Vida cambió. Parámetros: (vidaActual, vidaMaxima).</summary>
    public event Action<float, float> OnVidaCambiada;

    /// <summary>El jugador ha llegado a 0 de vida. Se emite una sola vez.</summary>
    public event Action OnJugadorMuerto;

    // ── Estado ───────────────────────────────────────────────────────────────
    private float _vidaActual;
    private float _vidaMaxima;
    private bool  _estaMuerto;

    public float VidaActual  => _vidaActual;
    public float VidaMaxima  => _vidaMaxima;
    public bool  EstaMuerto  => _estaMuerto;

    // ── Constructor ──────────────────────────────────────────────────────────
    public PlayerVidaDomain(float vidaMaxima)
    {
        _vidaMaxima = vidaMaxima;
        _vidaActual = vidaMaxima;
        _estaMuerto = false;
    }

    // ── Casos de uso ─────────────────────────────────────────────────────────

    public void RecibirDanio(float cantidad)
    {
        if (_estaMuerto) return;

        _vidaActual = Mathf.Max(0f, _vidaActual - cantidad);
        Debug.Log($"[Dominio·Vida] Daño: -{cantidad} | Vida: {_vidaActual}/{_vidaMaxima}");
        OnVidaCambiada?.Invoke(_vidaActual, _vidaMaxima);

        if (_vidaActual <= 0f)
        {
            _estaMuerto = true;
            Debug.Log("[Dominio·Vida] El jugador ha muerto.");
            OnJugadorMuerto?.Invoke();
        }
    }

    public void Curar(float cantidad)
    {
        if (_estaMuerto) return;

        _vidaActual = Mathf.Min(_vidaMaxima, _vidaActual + cantidad);
        Debug.Log($"[Dominio·Vida] Curación: +{cantidad} | Vida: {_vidaActual}/{_vidaMaxima}");
        OnVidaCambiada?.Invoke(_vidaActual, _vidaMaxima);
    }

    public void SetVidaMaxima(float nuevaMaxima)
    {
        _vidaMaxima = nuevaMaxima;
        _vidaActual = Mathf.Min(_vidaActual, _vidaMaxima);
        Debug.Log($"[Dominio·Vida] Nueva vida máxima: {_vidaMaxima}");
        OnVidaCambiada?.Invoke(_vidaActual, _vidaMaxima);
    }
}
