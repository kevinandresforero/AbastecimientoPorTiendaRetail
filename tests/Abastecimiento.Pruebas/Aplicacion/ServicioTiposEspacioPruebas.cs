using Abastecimiento.Aplicacion.Dtos.Tiendas;
using Abastecimiento.Aplicacion.Servicios;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Pruebas.Datos;

namespace Abastecimiento.Pruebas.Aplicacion;

public class ServicioTiposEspacioPruebas
{
    private readonly TiendasDePrueba datos = new();
    private readonly UnidadDeTrabajoFalsa unidadDeTrabajo = new();

    private ServicioTiposEspacio CrearServicio() => new(new TipoEspacioRepositorioFalso(datos.TiposEspacio), unidadDeTrabajo);

    [Fact]
    public async Task ListarDevuelveLosTiposDeLaSemilla()
    {
        var tipos = await CrearServicio().ListarAsync(CancellationToken.None);

        Assert.Equal(7, tipos.Count);
        Assert.Equal(6, tipos.Count(t => t.Categoria == CategoriaEspacio.Mobiliario));
        Assert.Equal("Perimetral", Assert.Single(tipos, t => t.Categoria == CategoriaEspacio.Perimetral).Nombre);
    }

    [Fact]
    public async Task CrearTipoNuevoAsignaIdYGuardaUnaVez()
    {
        var creado = await CrearServicio().CrearAsync(
            new CrearTipoEspacioSolicitud { Nombre = "  Vitrina  ", Categoria = CategoriaEspacio.Mobiliario }, CancellationToken.None);

        Assert.Equal(8, creado.Id);
        Assert.Equal("Vitrina", creado.Nombre);
        Assert.Equal(CategoriaEspacio.Mobiliario, creado.Categoria);
        Assert.Equal(1, unidadDeTrabajo.Guardados);
    }

    [Fact]
    public async Task NombreDuplicadoLanzaConflicto()
    {
        await Assert.ThrowsAsync<ConflictoExcepcion>(() => CrearServicio().CrearAsync(
            new CrearTipoEspacioSolicitud { Nombre = "Isla", Categoria = CategoriaEspacio.Mobiliario }, CancellationToken.None));

        Assert.Equal(0, unidadDeTrabajo.Guardados);
    }

    [Fact]
    public async Task CategoriaFueraDelEnumLanzaValidacion()
    {
        await Assert.ThrowsAsync<ValidacionDominioExcepcion>(() => CrearServicio().CrearAsync(
            new CrearTipoEspacioSolicitud { Nombre = "Repisa", Categoria = (CategoriaEspacio)99 }, CancellationToken.None));
    }
}
