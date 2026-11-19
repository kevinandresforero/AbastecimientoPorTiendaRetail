namespace Abastecimiento.Dominio.Entidades.ArbolComercial;

/// <summary>
/// Nivel de la jerarquia comercial. <see cref="Profundidad"/> es independiente del Id para permitir
/// insertar niveles intermedios sin renumerar claves. El nivel hoja es el de mayor profundidad (derivado).
/// </summary>
public class NivelJerarquia
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int Profundidad { get; set; }
}
