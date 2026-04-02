using UnityEngine;

/// <summary>
/// ScriptableObject que define todos los datos de un arma.
/// Crea un asset: clic derecho en Project → TFG → Definición de Arma.
///
/// PREFABS NECESARIOS:
///   pickupPrefab     → El objeto 3D que aparece en el suelo al morir el enemigo.
///                      Debe tener ArmaPickupAdapter + SphereCollider (Is Trigger).
///   modeloEquipado   → El modelo 3D que se muestra en la mano del jugador al equipar.
///                      Se instancia como hijo del portaArmas del WeaponAdapter.
///   bulletPrefab     → Prefab de la bala (igual al de WeaponAdapter actual).
/// </summary>
[CreateAssetMenu(fileName = "NuevaArma", menuName = "TFG/Definición de Arma")]
public class ArmaDefinicion : ScriptableObject
{
    [Header("Identificación")]
    public string id;
    public string nombreArma;
    public Sprite iconoUI;

    [Header("Stats (alimentan WeaponDomain)")]
    public int   municionMaxima  = 12;
    public float cadencia        = 0.15f;
    public float duracionRecarga = 1.5f;
    public float fuerzaBala      = 20f;

    [Header("Prefabs")]
    [Tooltip("Objeto 3D en el suelo (debe tener ArmaPickupAdapter + SphereCollider Trigger).")]
    public GameObject pickupPrefab;

    [Tooltip("Modelo visual que se instancia en la mano del jugador al equipar el arma.")]
    public GameObject modeloEquipado;

    [Tooltip("Prefab de la bala que dispara esta arma.")]
    public GameObject bulletPrefab;

    [Header("Audio (opcional)")]
    public AudioClip audioDisparo;
    public AudioClip audioRecarga;
    public AudioClip audioSeco;

    /// <summary>
    /// Crea el objeto de dominio puro a partir de este ScriptableObject.
    /// Llamado por los Adapters al recoger/equipar el arma.
    /// </summary>
    public ArmaInventario CrearDatosDominio() =>
        new ArmaInventario(id, nombreArma, municionMaxima, cadencia, duracionRecarga, fuerzaBala);
}
