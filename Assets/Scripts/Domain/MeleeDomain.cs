using System;

/// <summary>
/// CAPA DE DOMINIO — Lógica pura del ataque cuerpo a cuerpo del jugador.
/// ► C# puro: sin UnityEngine, sin MonoBehaviour.
/// </summary>
public class MeleeDomain
{
    public event Action OnAtaqueMelee;

    public float Danio    { get; }
    public float Cooldown { get; }

    private float _timer;
    public bool PuedeAtacar => _timer <= 0f;

    public MeleeDomain(float danio, float cooldown)
    {
        Danio    = danio;
        Cooldown = cooldown;
        _timer   = 0f;
    }

    public void Tick(float deltaTime)
    {
        if (_timer > 0f)
            _timer -= deltaTime;
    }

    public void IntentarAtacar()
    {
        if (!PuedeAtacar) return;
        _timer = Cooldown;
        OnAtaqueMelee?.Invoke();
    }
}
