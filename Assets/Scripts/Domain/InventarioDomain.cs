using System;
using System.Collections.Generic;

// Dominio del inventario: C# puro, sin Unity
// Gestiona qué armas tengo y cuál está equipada
// El adapter escucha los eventos y actualiza la UI y el WeaponAdapter
public class InventarioDomain
{
    public event Action                  OnAbierto;
    public event Action                  OnCerrado;
    public event Action<ArmaInventario>  OnArmaAgregada;
    public event Action<ArmaInventario>  OnArmaEquipada;
    public event Action                  OnArmaDesequipada;

    private bool _estaAbierto;
    public  bool EstaAbierto => _estaAbierto;

    private readonly List<ArmaInventario> _armas = new();
    public IReadOnlyList<ArmaInventario> Armas => _armas;

    private ArmaInventario _armaEquipada;
    public  ArmaInventario ArmaEquipada => _armaEquipada;

    public void Abrir()
    {
        if (_estaAbierto) return;
        _estaAbierto = true;
        OnAbierto?.Invoke();
    }

    public void Cerrar()
    {
        if (!_estaAbierto) return;
        _estaAbierto = false;
        OnCerrado?.Invoke();
    }

    // toggle para abrir y cerrar con la misma tecla (Tab)
    public void Toggle()
    {
        if (_estaAbierto) Cerrar();
        else Abrir();
    }

    public bool AgregarArma(ArmaInventario arma)
    {
        if (arma == null) return false;
        _armas.Add(arma);
        OnArmaAgregada?.Invoke(arma);
        return true;
    }

    // solo puedo equipar un arma que esté en mi inventario
    public bool EquiparArma(ArmaInventario arma)
    {
        if (!_armas.Contains(arma)) return false;
        _armaEquipada = arma;
        OnArmaEquipada?.Invoke(arma);
        return true;
    }

    public void DesequiparArma()
    {
        if (_armaEquipada == null) return;
        _armaEquipada = null;
        OnArmaDesequipada?.Invoke();
    }
}
