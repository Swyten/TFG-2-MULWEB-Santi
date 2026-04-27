using UnityEngine;

// ScriptableObject que define todos los datos de un arma
// Creo un asset nuevo con: clic derecho en Project → TFG → Definición de Arma
//
// Prefabs que necesita cada arma:
//   pickupPrefab   → el objeto 3D que aparece en el suelo cuando el enemigo muere
//                    debe tener ArmaPickupAdapter + SphereCollider (Is Trigger)
//   modeloEquipado → el modelo 3D que se ve en la mano del jugador al equiparla
//                    se instancia como hijo del portaArmas del WeaponAdapter
//   bulletPrefab   → el prefab de la bala que dispara
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

    // convierte este ScriptableObject en datos de dominio puros
    // lo llaman los adapters al recoger o equipar el arma
    public ArmaInventario CrearDatosDominio() =>
        new ArmaInventario(id, nombreArma, municionMaxima, cadencia, duracionRecarga, fuerzaBala);
}
