using System.Linq.Expressions;
using Abastecimiento.Front.Contratos;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Compartidos;

/// <summary><c>InputSelect</c> con etiqueta, alimentado por un catalogo y con opcion vacia (= sin seleccion).</summary>
public partial class SelectorCatalogo
{
    /// <summary>Id del select (para el <c>label</c>).</summary>
    [Parameter, EditorRequired]
    public string Id { get; set; } = string.Empty;

    /// <summary>Texto de la etiqueta.</summary>
    [Parameter, EditorRequired]
    public string Etiqueta { get; set; } = string.Empty;

    /// <summary>Opciones del catalogo.</summary>
    [Parameter]
    public IReadOnlyList<CatalogoRespuesta> Opciones { get; set; } = [];

    /// <summary>Texto de la opcion vacia.</summary>
    [Parameter]
    public string TextoVacio { get; set; } = "Seleccione…";

    /// <summary>Deshabilita el select.</summary>
    [Parameter]
    public bool Deshabilitado { get; set; }

    /// <summary>Id seleccionado (null = opcion vacia).</summary>
    [Parameter]
    public int? Value { get; set; }

    /// <summary>Notifica el cambio de seleccion (<c>@bind-Value</c>).</summary>
    [Parameter]
    public EventCallback<int?> ValueChanged { get; set; }

    /// <summary>Expresion del campo enlazado (para la validacion).</summary>
    [Parameter]
    public Expression<Func<int?>>? ValueExpression { get; set; }
}
