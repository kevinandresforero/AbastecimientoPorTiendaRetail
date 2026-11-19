using Abastecimiento.Front.Clientes;

namespace Abastecimiento.Front.Componentes.Pages;

/// <summary>Catalogos en pestañas: cada una lista y permite agregar elementos sin recargar la pagina.</summary>
public partial class Catalogos
{
    private static readonly IReadOnlyList<PestanaCatalogo> Pestanas =
    [
        new("marcas", "Marcas de tienda", "marca de tienda", TipoCatalogo.MarcasTienda, 50),
        new("regionales", "Regionales", "regional", TipoCatalogo.Regionales, 50),
        new("tipos-tienda", "Tipos de tienda", "tipo de tienda", TipoCatalogo.TiposTienda, 50),
        new("tipos-espacio", "Tipos de espacio", "tipo de espacio", null, 50)
    ];

    private static readonly IReadOnlyList<Compartidos.Miga> Migas = [new("Inicio", ""), new("Catálogos")];

    private PestanaCatalogo activa = Pestanas[0];

    private bool EsActiva(PestanaCatalogo pestana) => pestana.Clave == activa.Clave;

    private void Activar(PestanaCatalogo pestana) => activa = pestana;

    /// <summary>Pestaña; <see cref="Tipo"/> null = tipos de espacio.</summary>
    private sealed record PestanaCatalogo(string Clave, string Titulo, string Singular, TipoCatalogo? Tipo, int LongitudMaxima);
}
