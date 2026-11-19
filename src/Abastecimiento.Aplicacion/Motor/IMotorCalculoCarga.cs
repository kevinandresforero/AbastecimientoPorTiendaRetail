using Abastecimiento.Dominio.Entidades.ArbolComercial;

namespace Abastecimiento.Aplicacion.Motor;

/// <summary>Motor de calculo de carga en cascada (Reglas 2 y 3). Puro: no accede a la base de datos.</summary>
public interface IMotorCalculoCarga
{
    /// <summary>Distribuye los espacios totales de una tienda por el arbol y calcula la carga de cada tipo de producto.</summary>
    /// <param name="espaciosTotales">Espacios fisicos totales de la tienda (Regla 1).</param>
    /// <param name="nodos">Arbol de la tienda con concepto y nivel cargados.</param>
    /// <param name="profundidadHoja">Profundidad del nivel hoja segun <c>NivelesJerarquia</c>.</param>
    ResultadoCalculo Calcular(int espaciosTotales, IReadOnlyList<NodoComercial> nodos, int profundidadHoja);
}
