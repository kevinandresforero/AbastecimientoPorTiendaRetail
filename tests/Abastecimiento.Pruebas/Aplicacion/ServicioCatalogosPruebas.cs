using Abastecimiento.Aplicacion.Dtos.Catalogos;
using Abastecimiento.Aplicacion.Servicios;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Pruebas.Datos;

namespace Abastecimiento.Pruebas.Aplicacion;

/// <summary>Servicio generico de catalogos con una fabrica de repositorios en memoria (sin contenedor de DI).</summary>
public class ServicioCatalogosPruebas
{
    private readonly TiendasDePrueba datos = new();
    private readonly UnidadDeTrabajoFalsa unidadDeTrabajo = new();

    private ServicioCatalogos CrearServicio() => new(new CatalogoRepositorioFabricaFalsa(datos), unidadDeTrabajo);

    private static CrearCatalogoSolicitud Solicitud(string nombre) => new() { Nombre = nombre };

    [Fact]
    public async Task ListarUsaElRepositorioDelTipoPedido()
    {
        var regionales = await CrearServicio().ListarAsync<Regional>(CancellationToken.None);
        var marcas = await CrearServicio().ListarAsync<MarcaTienda>(CancellationToken.None);

        Assert.Equal(new[] { "ANTIOQUIA", "BOGOTA" }, regionales.Select(r => r.Nombre).ToArray());
        Assert.Equal(3, marcas.Count);
    }

    [Fact]
    public async Task CrearRecortaElNombreYGuardaUnaVez()
    {
        var creada = await CrearServicio().CrearAsync<Regional>(Solicitud("  CARIBE  "), CancellationToken.None);

        Assert.Equal("CARIBE", creada.Nombre);
        Assert.Equal(3, creada.Id);
        Assert.Contains(datos.Regionales, r => r.Nombre == "CARIBE");
        Assert.Equal(1, unidadDeTrabajo.Guardados);
    }

    [Fact]
    public async Task NombreDuplicadoLanzaConflicto()
    {
        await Assert.ThrowsAsync<ConflictoExcepcion>(() =>
            CrearServicio().CrearAsync<MarcaTienda>(Solicitud("YOI"), CancellationToken.None));

        Assert.Equal(0, unidadDeTrabajo.Guardados);
    }

    [Fact]
    public async Task NombreDeMasDe50CaracteresLanzaValidacion()
    {
        await Assert.ThrowsAsync<ValidacionDominioExcepcion>(() =>
            CrearServicio().CrearAsync<TipoTienda>(Solicitud(new string('X', 51)), CancellationToken.None));
    }

    [Fact]
    public async Task ResponsableVisualAdmiteHasta100Caracteres()
    {
        var servicio = CrearServicio();

        var creado = await servicio.CrearAsync<ResponsableVisual>(Solicitud(new string('A', 100)), CancellationToken.None);

        Assert.Equal(100, creado.Nombre.Length);
        await Assert.ThrowsAsync<ValidacionDominioExcepcion>(() =>
            servicio.CrearAsync<ResponsableVisual>(Solicitud(new string('B', 101)), CancellationToken.None));
    }
}
