using System;
using System.Collections.Generic;

/// <summary>
/// CAPA DE DOMINIO — Estado puro del inventario del jugador.
/// ► Sin dependencias de UnityEngine ni de UI.
/// ► Gestiona la colección de armas y cuál está equipada.
/// </summary>
public class InventarioDomain
{
    // ── Eventos ───────────────────────────────────────────────────────────────
    public event Action                  OnAbierto;
    public event Action                  OnCerrado;
    public event Action<ArmaInventario>  OnArmaAgregada;
    public event Action<ArmaInventario>  OnArmaEquipada;

    // ── Estado ────────────────────────────────────────────────────────────────
    private bool _estaAbierto;
    public  bool EstaAbierto => _estaAbierto;

    private readonly List<ArmaInventario> _armas = new();
    public IReadOnlyList<ArmaInventario> Armas => _armas;

    private ArmaInventario _armaEquipada;
    public  ArmaInventario ArmaEquipada => _armaEquipada;

    // ── Casos de uso: inventario ──────────────────────────────────────────────
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

    public void Toggle()
    {
        if (_estaAbierto) Cerrar();
        else Abrir();
    }

    // ── Casos de uso: armas ───────────────────────────────────────────────────
    public bool AgregarArma(ArmaInventario arma)
    {
        if (arma == null) return false;
        _armas.Add(arma);
        OnArmaAgregada?.Invoke(arma);
        return true;
    }

    public bool EquiparArma(ArmaInventario arma)
    {
        if (!_armas.Contains(arma)) return false;
        _armaEquipada = arma;
        OnArmaEquipada?.Invoke(arma);
        return true;
    }
}
