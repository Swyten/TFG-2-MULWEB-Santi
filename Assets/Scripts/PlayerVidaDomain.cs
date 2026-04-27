using System;
using UnityEngine;

// Dominio de la vida del jugador: C# puro, sin MonoBehaviour, sin UI
// Los adapters escuchan los eventos y actualizan la vista; el dominio no sabe nada de ellos
public class PlayerVidaDomain
{
    // vida cambió → parámetros: (vidaActual, vidaMaxima)
    public event Action<float, float> OnVidaCambiada;

    // el jugador llegó a 0 de vida; se emite una sola vez
    public event Action OnJugadorMuerto;

    private float _vidaActual;
    private float _vidaMaxima;
    private bool  _estaMuerto;

    public float VidaActual  => _vidaActual;
    public float VidaMaxima  => _vidaMaxima;
    public bool  EstaMuerto  => _estaMuerto;

    public PlayerVidaDomain(float vidaMaxima)
    {
        _vidaMaxima = vidaMaxima;
        _vidaActual = vidaMaxima; // empieza con vida llena
        _estaMuerto = false;
    }

    public void RecibirDanio(float cantidad)
    {
        if (_estaMuerto) return; // si ya murió no proceso más daño

        _vidaActual = Mathf.Max(0f, _vidaActual - cantidad); // no bajo de 0
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

        _vidaActual = Mathf.Min(_vidaMaxima, _vidaActual + cantidad); // no supero la vida máxima
        Debug.Log($"[Dominio·Vida] Curación: +{cantidad} | Vida: {_vidaActual}/{_vidaMaxima}");
        OnVidaCambiada?.Invoke(_vidaActual, _vidaMaxima);
    }

    // se llama desde GameManager cuando el jugador sube de nivel para actualizar la vida máxima
    public void SetVidaMaxima(float nuevaMaxima)
    {
        _vidaMaxima = nuevaMaxima;
        _vidaActual = Mathf.Min(_vidaActual, _vidaMaxima); // por si la vida actual supera la nueva máxima
        Debug.Log($"[Dominio·Vida] Nueva vida máxima: {_vidaMaxima}");
        OnVidaCambiada?.Invoke(_vidaActual, _vidaMaxima);
    }
}
