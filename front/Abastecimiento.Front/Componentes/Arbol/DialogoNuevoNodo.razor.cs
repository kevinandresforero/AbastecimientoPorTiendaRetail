using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Formularios;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Arbol;

/// <summary>
/// Agregar un nodo bajo <see cref="Padre"/> (o como raiz): ofrece los conceptos del nivel siguiente y el porcentaje.
/// Las reglas (saltos de nivel, duplicados, suma de hermanos) las valida la API.
/// </summary>
public partial class DialogoNuevoNodo
{
    private static readonly IReadOnlyDictionary<string, string> AliasCampos = new Dictionary<string, string>
    {
        ["PorcentajeParticipacion"] = nameof(FormularioNodo.Porcentaje)
    };

    private readonly FormularioNodo formulario = new();
    private NivelRespuesta? nivelHijo;
    private IReadOnlyList<CatalogoRespuesta> opcionesConcepto = [];
    private bool cargando = true;
    private bool enviando;
    private bool inicializado;
    private ExcepcionApi? errorCarga;
    private ExcepcionApi? errorEnvio;

    /// <summary>Nodo padre; null agrega una raiz.</summary>
    [Parameter]
    public NodoArbolRespuesta? Padre { get; set; }

    /// <summary>Niveles de la jerarquia.</summary>
    [Parameter]
    public IReadOnlyList<NivelRespuesta> Niveles { get; set; } = [];

    /// <summary>Suma de los % de los futuros hermanos.</summary>
    [Parameter]
    public decimal SumaHermanos { get; set; }

    /// <summary>Se invoca con el nodo creado.</summary>
    [Parameter]
    public EventCallback<NodoArbolRespuesta> AlGuardar { get; set; }

    /// <summary>Se invoca al cancelar.</summary>
    [Parameter]
    public EventCallback AlCancelar { get; set; }

    [Inject]
    private IClienteArbolComercial ClienteArbol { get; set; } = default!;

    private string Titulo => Padre is null ? "Agregar nodo raíz" : $"Agregar nodo bajo {Padre.Nombre}";

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (inicializado)
        {
            return;
        }

        inicializado = true;
        var profundidad = (Padre?.Profundidad ?? 0) + 1;
        nivelHijo = Niveles.FirstOrDefault(n => n.Profundidad == profundidad);
        if (nivelHijo is not null)
        {
            await CargarConceptosAsync();
        }
    }

    private async Task CargarConceptosAsync()
    {
        cargando = true;
        errorCarga = await LlamarApiAsync(async ct =>
        {
            var conceptos = await ClienteArbol.ListarConceptosAsync(nivelHijo!.Id, ct);
            opcionesConcepto = conceptos.Select(c => new CatalogoRespuesta(c.Id, c.Nombre)).ToList();
        });
        cargando = false;
    }

    private async Task GuardarAsync()
    {
        if (enviando)
        {
            return;
        }

        enviando = true;
        NodoArbolRespuesta? creado = null;
        errorEnvio = await LlamarApiAsync(async ct =>
            creado = await ClienteArbol.CrearNodoAsync(
                new CrearNodoSolicitud(Padre?.NodoId, formulario.ConceptoComercialId ?? 0, formulario.Fraccion), ct));
        enviando = false;

        if (creado is not null)
        {
            await AlGuardar.InvokeAsync(creado);
        }
    }

    private Task CancelarAsync() => AlCancelar.InvokeAsync();
}
