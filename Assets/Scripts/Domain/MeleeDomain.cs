using System;

// Dominio del ataque melee: C# puro, sin UnityEngine
// Solo guarda el daño, el cooldown y dispara el evento cuando puede atacar
public class MeleeDomain
{
    // el adapter escucha esto para hacer el OverlapSphere y reproducir efectos
    public event Action OnAtaqueMelee;

    public float Danio    { get; }
    public float Cooldown { get; }

    private float _timer;
    public bool PuedeAtacar => _timer <= 0f;

    public MeleeDomain(float danio, float cooldown)
    {
        Danio    = danio;
        Cooldown = cooldown;
        _timer   = 0f; // listo para atacar desde el primer frame
    }

    // descuento el timer cada frame; el adapter me lo llama con Time.deltaTime
    public void Tick(float deltaTime)
    {
        if (_timer > 0f)
            _timer -= deltaTime;
    }

    public void IntentarAtacar()
    {
        if (!PuedeAtacar) return;
        _timer = Cooldown; // reinicio el cooldown
        OnAtaqueMelee?.Invoke();
    }
}
