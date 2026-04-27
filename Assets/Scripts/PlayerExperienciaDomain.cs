using System;
using UnityEngine;

// Dominio de experiencia y nivel del jugador: C# puro, sin MonoBehaviour, sin UI
// Los adapters de UI se suscriben a los eventos para actualizarse de forma reactiva
// En el futuro los datos (nivel, XP, vidaMaxima) se enviarán a Firebase via n8n
public class PlayerExperienciaDomain
{
    // se dispara al ganar XP o al subir de nivel
    // parámetros: (experienciaActual, experienciaParaSiguienteNivel, nivelActual)
    public event Action<int, int, int> OnExperienciaCambiada;

    // se dispara solo cuando subo de nivel
    // parámetros: (nivelNuevo, nuevaVidaMaxima)
    // el GameManager escucha esto para actualizar PlayerVidaDomain
    public event Action<int, float> OnNivelSubido;

    private int   _nivel;
    private int   _experienciaActual;
    private int   _experienciaParaSiguienteNivel;
    private float _vidaMaxima;

    public int   Nivel                        => _nivel;
    public int   ExperienciaActual            => _experienciaActual;
    public int   ExperienciaParaSiguienteNivel => _experienciaParaSiguienteNivel;
    public float VidaMaxima                   => _vidaMaxima;

    public PlayerExperienciaDomain(float vidaMaximaInicial = 100f)
    {
        _nivel                        = 1;
        _experienciaActual            = 0;
        _experienciaParaSiguienteNivel = 100;
        _vidaMaxima                   = vidaMaximaInicial;
    }

    // le doy XP al jugador; uso un while en vez de if por si subo varios niveles de golpe
    public void GanarExperiencia(int cantidad)
    {
        _experienciaActual += cantidad;
        Debug.Log($"[Dominio·XP] Has ganado {cantidad} XP. " +
                  $"({_experienciaActual}/{_experienciaParaSiguienteNivel})");

        while (_experienciaActual >= _experienciaParaSiguienteNivel)
            SubirNivel();

        // notifico con el estado ya actualizado (post-subida si aplica)
        OnExperienciaCambiada?.Invoke(
            _experienciaActual,
            _experienciaParaSiguienteNivel,
            _nivel
        );
    }

    // conservo el exceso de XP, subo el nivel, mejoro stats y recalculo el umbral
    private void SubirNivel()
    {
        int sobrante = _experienciaActual - _experienciaParaSiguienteNivel;

        _nivel++;
        _vidaMaxima                   += 20f; // cada nivel da 20 de vida máxima más
        _experienciaParaSiguienteNivel  = Mathf.RoundToInt(_experienciaParaSiguienteNivel * 1.5f);
        _experienciaActual             = sobrante; // el sobrante pasa al nuevo nivel

        Debug.Log($"[Dominio·XP] ¡Nivel subido! Ahora eres nivel {_nivel}. " +
                  $"Vida máxima: {_vidaMaxima} | " +
                  $"XP para siguiente nivel: {_experienciaParaSiguienteNivel}");

        // el GameManager escucha esto y le dice a PlayerVidaDomain que actualice la vida máxima
        OnNivelSubido?.Invoke(_nivel, _vidaMaxima);
    }
}
