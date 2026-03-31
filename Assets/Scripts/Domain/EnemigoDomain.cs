using System;
using UnityEngine;

/// <summary>
/// CAPA DE DOMINIO — Gestión pura del estado de un enemigo.
/// ► Sin UnityEngine (salvo Mathf). Sin MonoBehaviour.
/// ► Emite eventos tipados que EnemyAdapter escucha para animar, morir, etc.
/// </summary>
public class EnemigoDomain
{
    // ── Eventos ──────────────────────────────────────────────────────────────
    /// <summary>Vida actual cambió (vidaActual, vidaMaxima).</summary>
    public event Action<float, float> OnVidaCambiada;

    /// <summary>El enemigo ha muerto. Parámetro: XP que otorga al jugador.</summary>
    public event Action<int> OnMuerto;

    /// <summary>El enemigo ataca al jugador. Parámetro: daño que inflige.</summary>
    public event Action<float> OnAtaque;

    // ── Estado ───────────────────────────────────────────────────────────────
    private float _vidaActual;
    private float _vidaMaxima;
    private float _danioAtaque;
    private float _cadenciaAtaque;
    private float _timerAtaque;
    private bool  _estaMuerto;
    private int   _xpAlMorir;

    // ── Propiedades ───────────────────────────────────────────────────────────
    public float VidaActual => _vidaActual;
    public float VidaMaxima => _vidaMaxima;
    public bool  EstaMuerto => _estaMuerto;

    // ── Constructor ───────────────────────────────────────────────────────────
    public EnemigoDomain(float vidaMaxima, float danioAtaque, float cadenciaAtaque, int xpAlMorir)
    {
        _vidaMaxima     = vidaMaxima;
        _vidaActual     = vidaMaxima;
        _danioAtaque    = danioAtaque;
        _cadenciaAtaque = cadenciaAtaque;
        _timerAtaque    = 0f;
        _estaMuerto     = false;
        _xpAlMorir      = xpAlMorir;
    }

    // ── Casos de uso ──────────────────────────────────────────────────────────

    public void TickAtaque(float deltaTime, bool enRango)
    {
        if (_estaMuerto) return;

        _timerAtaque += deltaTime;

        if (enRango && _timerAtaque >= _cadenciaAtaque)
        {
            _timerAtaque = 0f;
            OnAtaque?.Invoke(_danioAtaque);
            Debug.Log($"[EnemigoDomain] ¡Ataque! Daño: {_danioAtaque}");
        }
    }

    public void RecibirDanio(float cantidad)
    {
        if (_estaMuerto) return;

        _vidaActual = Mathf.Max(0f, _vidaActual - cantidad);
        Debug.Log($"[EnemigoDomain] Daño recibido: -{cantidad} | Vida: {_vidaActual}/{_vidaMaxima}");
        OnVidaCambiada?.Invoke(_vidaActual, _vidaMaxima);

        if (_vidaActual <= 0f)
            Morir();
    }

    // ── Privados ──────────────────────────────────────────────────────────────

    private void Morir()
    {
        _estaMuerto = true;
        Debug.Log($"[EnemigoDomain] Enemigo muerto. XP otorgada: {_xpAlMorir}");
        OnMuerto?.Invoke(_xpAlMorir);
    }
}
