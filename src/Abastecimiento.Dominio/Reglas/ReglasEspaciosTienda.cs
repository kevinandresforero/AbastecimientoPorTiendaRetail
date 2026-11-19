using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Dominio.Excepciones;

namespace Abastecimiento.Dominio.Reglas;

/// <summary>Reglas sobre la configuracion de espacios fisicos de una tienda.</summary>
public static class ReglasEspaciosTienda
{
    /// <summary>Tope de tipos de mobiliario por tienda (los espacios perimetrales no cuentan).</summary>
    public const int MaximoMobiliarioPorTienda = 3;

    /// <summary>
    /// Valida los espacios de una tienda. Requiere que cada <see cref="EspacioTienda.TipoEspacio"/> este cargado.
    /// </summary>
    /// <exception cref="ValidacionDominioExcepcion">Si se viola alguna regla.</exception>
    public static void Validar(IReadOnlyCollection<EspacioTienda> espacios)
    {
        var errores = new List<string>();

        if (espacios.Count == 0)
        {
            errores.Add("La tienda debe tener al menos un espacio.");
        }

        var duplicados = espacios
            .GroupBy(e => e.TipoEspacioId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (duplicados.Count > 0)
        {
            errores.Add($"Tipos de espacio repetidos: {string.Join(", ", duplicados)}.");
        }

        if (espacios.Any(e => e.CantidadEspacios < 0))
        {
            errores.Add("La cantidad de espacios no puede ser negativa.");
        }

        var cantidadMobiliario = espacios.Count(e => e.TipoEspacio.Categoria == CategoriaEspacio.Mobiliario);
        if (cantidadMobiliario > MaximoMobiliarioPorTienda)
        {
            errores.Add(
                $"Una tienda admite maximo {MaximoMobiliarioPorTienda} tipos de mobiliario; se recibieron {cantidadMobiliario}.");
        }

        if (errores.Count > 0)
        {
            throw new ValidacionDominioExcepcion(errores);
        }
    }
}
