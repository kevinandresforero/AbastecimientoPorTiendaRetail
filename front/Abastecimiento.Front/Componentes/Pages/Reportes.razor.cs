using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Formularios;
using Abastecimiento.Front.Utilidades;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Pages;

/// <summary>Consolidado por tienda, marca de tienda, regional o nivel del arbol (R4b), con la fila Total general.</summary>
public partial class Reportes
{
    /// <summary>Nivel por defecto al agrupar por nivel: 3 = Marca de Producto.</summary>
    public const int ProfundidadPorDefecto = 3;

    private static readonly IReadOnlyList<Compartidos.Miga> Migas = [new("Inicio", ""), new("Reportes")];

    private static readonly IReadOnlyList<AgruparPor> OpcionesAgrupacion = Enum.GetValues<AgruparPor>();

    private readonly FormularioReporte formulario = new();
    private IReadOnlyList<CatalogoRespuesta> opcionesNivel = [];
    private ConsolidadoRespuesta? consolidado;
    private bool cargando = true;
    private ExcepcionApi? error;

    [Inject]
    private IClienteReportes ClienteReportes { get; set; } = default!;

    [Inject]
    private IClienteArbolComercial ClienteArbol { get; set; } = default!;

    private IQueryable<FilaReporte> FilasConsulta => consolidado is null
        ? Enumerable.Empty<FilaReporte>().AsQueryable()
        : consolidado.Grupos.Select(g => new FilaReporte(g, false))
            .Append(new FilaReporte(consolidado.Total, true))
            .AsQueryable();

    private bool MostrarEspaciosTotales => consolidado?.Grupos.Any(g => g.EspaciosTotales is not null) ?? false;

    private string TituloResultado => consolidado is null
        ? string.Empty
        : consolidado.Nivel is { } nivel
            ? $"Consolidado por {nivel.Nombre}"
            : $"Consolidado por {Textos.AgruparPor(consolidado.AgruparPor).ToLowerInvariant()}";

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        cargando = true;
        error = await LlamarApiAsync(async ct =>
        {
            var niveles = (await ClienteArbol.ObtenerNivelesAsync(ct)).OrderBy(n => n.Profundidad).ToList();
            opcionesNivel = niveles.Select(n => new CatalogoRespuesta(n.Id, $"{n.Profundidad}. {n.Nombre}")).ToList();
            formulario.NivelId = niveles.FirstOrDefault(n => n.Profundidad == ProfundidadPorDefecto)?.Id ?? niveles.FirstOrDefault()?.Id;
        });
        if (error is null)
        {
            await ConsultarAsync();
        }
        else
        {
            cargando = false;
        }
    }

    private Task CambiarAgrupacionAsync() => ConsultarAsync();

    private Task CambiarNivelAsync() =>
        formulario.AgruparPor == AgruparPor.Nivel ? ConsultarAsync() : Task.CompletedTask;

    private async Task ConsultarAsync()
    {
        if (formulario.AgruparPor == AgruparPor.Nivel && formulario.NivelId is null)
        {
            return;
        }

        cargando = true;
        error = await LlamarApiAsync(async ct =>
            consolidado = await ClienteReportes.ConsolidadoAsync(formulario.AgruparPor, formulario.NivelAEnviar, ct));
        cargando = false;
    }

    private static string? ClaseCelda(FilaReporte fila) => fila.EsTotal ? "celda-total" : null;

    /// <summary>Fila del reporte; <see cref="EsTotal"/> marca la fila Total general.</summary>
    private sealed record FilaReporte(GrupoConsolidadoRespuesta Grupo, bool EsTotal);
}
