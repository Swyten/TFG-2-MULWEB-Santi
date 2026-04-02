/// <summary>
/// CAPA DE DOMINIO — Datos puros de un arma en el inventario.
/// ► C# puro: sin UnityEngine, sin MonoBehaviour.
/// ► El Adapter mapea ArmaDefinicion (ScriptableObject) → ArmaInventario al recoger el arma.
/// </summary>
public class ArmaInventario
{
    public string Id             { get; }
    public string Nombre         { get; }
    public int    MunicionMaxima { get; }
    public float  Cadencia       { get; }
    public float  DuracionRecarga{ get; }
    public float  FuerzaBala     { get; }

    public ArmaInventario(string id, string nombre, int municionMaxima,
                          float cadencia, float duracionRecarga, float fuerzaBala)
    {
        Id              = id;
        Nombre          = nombre;
        MunicionMaxima  = municionMaxima;
        Cadencia        = cadencia;
        DuracionRecarga = duracionRecarga;
        FuerzaBala      = fuerzaBala;
    }
}
