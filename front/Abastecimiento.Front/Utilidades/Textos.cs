using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Utilidades;

/// <summary>Textos visibles de los valores enumerados del contrato.</summary>
public static class Textos
{
    /// <summary>Nombre visible de la categoria de espacio.</summary>
    public static string Categoria(CategoriaEspacio categoria) => categoria switch
    {
        CategoriaEspacio.Mobiliario => "Mobiliario",
        CategoriaEspacio.Perimetral => "Perimetral",
        _ => categoria.ToString()
    };

    /// <summary>Nombre visible del criterio de agrupacion del consolidado.</summary>
    public static string AgruparPor(AgruparPor agruparPor) => agruparPor switch
    {
        Contratos.AgruparPor.Tienda => "Tienda",
        Contratos.AgruparPor.MarcaTienda => "Marca de tienda",
        Contratos.AgruparPor.Regional => "Regional",
        Contratos.AgruparPor.Nivel => "Nivel del árbol",
        _ => agruparPor.ToString()
    };
}
