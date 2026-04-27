using System;
using UnityEngine;

// Dominio del enemigo: C# puro, sin MonoBehaviour
// Gestiona su vida y sus ataques; EnemyAdapter escucha los eventos para animar, morir, etc.
public class EnemigoDomain
{
    // vida actual cambió → (vidaActual, vidaMaxima)
    public event Action<float, float> OnVidaCambiada;

    // el enemigo ha muerto → parámetro: XP que otorga al jugador
    public event Action<int> OnMuerto;

    // el enemigo ataca → parámetro: daño que inflige
    public event Action<float> OnAtaque;

    private float _vidaActual;
    private float _vidaMaxima;
    private float _danioAtaque;
    private float _cadenciaAtaque;
    private float _timerAtaque;
    private bool  _estaMuerto;
    private int   _xpAlMorir;

    public float VidaActual => _vidaActual;
    public float VidaMaxima => _vidaMaxima;
    public bool  EstaMuerto => _estaMuerto;

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

    // se llama cada frame desde el adapter; acumulo tiempo y ataco cuando corresponde
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

        _vidaActual = Mathf.Max(0f, _vidaActual - cantidad); // no bajo de 0
        Debug.Log($"[EnemigoDomain] Daño recibido: -{cantidad} | Vida: {_vidaActual}/{_vidaMaxima}");
        OnVidaCambiada?.Invoke(_vidaActual, _vidaMaxima);

        if (_vidaActual <= 0f)
            Morir();
    }

    private void Morir()
    {
        _estaMuerto = true;
        Debug.Log($"[EnemigoDomain] Enemigo muerto. XP otorgada: {_xpAlMorir}");
        OnMuerto?.Invoke(_xpAlMorir);
    }
}
