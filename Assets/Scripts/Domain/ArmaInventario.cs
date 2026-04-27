// Datos puros de un arma dentro del inventario: C# puro, sin UnityEngine
// El adapter mapea ArmaDefinicion (ScriptableObject) → ArmaInventario al recoger el arma
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
