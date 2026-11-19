using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using Abastecimiento.Front.Clientes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Abastecimiento.Front.Componentes.Compartidos;

/// <summary>
/// Dentro de un <see cref="EditForm"/>: pasa los errores de una <see cref="ExcepcionApi"/> al
/// <see cref="ValidationMessageStore"/>. Cada <c>errors{campo}</c> (PascalCase, ej. <c>"Espacios[0].CantidadEspacios"</c>)
/// se ubica junto a su campo del modelo; <c>errores[]</c>, <c>detail</c> y los campos sin equivalente van al resumen.
/// </summary>
public sealed partial class ResumenErroresApi : IDisposable
{
    private static readonly Regex SegmentoConIndice = new(@"^(?<nombre>\w+)\[(?<indice>\d+)\]$", RegexOptions.Compiled);

    private ValidationMessageStore? almacen;
    private EditContext? contextoActual;
    private ExcepcionApi? errorMostrado;

    [CascadingParameter]
    private EditContext? Contexto { get; set; }

    /// <summary>Error de la ultima llamada a la API (null = sin error).</summary>
    [Parameter]
    public ExcepcionApi? Error { get; set; }

    /// <summary>Alias de nombres de la API → propiedad del formulario (ej. PorcentajeParticipacion → Porcentaje).</summary>
    [Parameter]
    public IReadOnlyDictionary<string, string>? AliasCampos { get; set; }

    /// <summary>Campo del formulario donde se muestra el detalle de un 409 (ej. CodigoHanna duplicado).</summary>
    [Parameter]
    public string? CampoConflicto { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (Contexto is null)
        {
            throw new InvalidOperationException($"{nameof(ResumenErroresApi)} debe usarse dentro de un EditForm.");
        }

        if (!ReferenceEquals(Contexto, contextoActual))
        {
            Desuscribir();
            contextoActual = Contexto;
            almacen = new ValidationMessageStore(Contexto);
            Contexto.OnValidationRequested += AlValidar;
            Contexto.OnFieldChanged += AlCambiarCampo;
            errorMostrado = null;
        }

        if (!ReferenceEquals(Error, errorMostrado))
        {
            errorMostrado = Error;
            almacen!.Clear();
            if (Error is not null)
            {
                Agregar(Error);
            }

            Contexto.NotifyValidationStateChanged();
        }
    }

    /// <inheritdoc />
    public void Dispose() => Desuscribir();

    private void Agregar(ExcepcionApi error)
    {
        var modelo = contextoActual!.Model;
        var general = new FieldIdentifier(modelo, string.Empty);

        if (error.ErroresPorCampo.Count == 0 && error.Errores.Count == 0)
        {
            var mensaje = string.IsNullOrWhiteSpace(error.Detalle) ? error.Titulo : $"{error.Titulo}: {error.Detalle}";
            if (error.Estado == 409 && CampoConflicto is not null && BuscarCampo(modelo, CampoConflicto) is { } campoConflicto)
            {
                almacen!.Add(campoConflicto, error.Detalle ?? error.Titulo);
            }
            else
            {
                almacen!.Add(general, mensaje);
            }
        }
        else if (!string.IsNullOrWhiteSpace(error.Detalle) && error.Errores.Count == 0)
        {
            almacen!.Add(general, error.Detalle);
        }

        foreach (var mensaje in error.Errores)
        {
            almacen!.Add(general, mensaje);
        }

        foreach (var (clave, mensajes) in error.ErroresPorCampo)
        {
            var ruta = AliasCampos is not null && AliasCampos.TryGetValue(clave, out var alias) ? alias : clave;
            if (BuscarCampo(modelo, ruta) is { } campo)
            {
                almacen!.Add(campo, mensajes);
            }
            else
            {
                almacen!.Add(general, mensajes.Select(m => $"{clave}: {m}"));
            }
        }
    }

    /// <summary>Resuelve "Nombre" o "Espacios[0].CantidadEspacios" a un campo del modelo (sin distinguir mayusculas).</summary>
    internal static FieldIdentifier? BuscarCampo(object modelo, string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta) || ruta == "$")
        {
            return null;
        }

        var segmentos = ruta.TrimStart('$', '.').Split('.');
        object? actual = modelo;
        for (var i = 0; i < segmentos.Length - 1 && actual is not null; i++)
        {
            actual = Navegar(actual, segmentos[i]);
        }

        if (actual is null)
        {
            return null;
        }

        var propiedad = Propiedad(actual, segmentos[^1]);
        return propiedad is null ? null : new FieldIdentifier(actual, propiedad.Name);
    }

    private static object? Navegar(object objeto, string segmento)
    {
        var coincidencia = SegmentoConIndice.Match(segmento);
        var nombre = coincidencia.Success ? coincidencia.Groups["nombre"].Value : segmento;
        var valor = Propiedad(objeto, nombre)?.GetValue(objeto);
        if (!coincidencia.Success)
        {
            return valor;
        }

        var indice = int.Parse(coincidencia.Groups["indice"].Value, System.Globalization.CultureInfo.InvariantCulture);
        return valor is IList lista && indice < lista.Count ? lista[indice] : null;
    }

    private static PropertyInfo? Propiedad(object objeto, string nombre) =>
        objeto.GetType().GetProperty(nombre, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

    private void AlValidar(object? remitente, ValidationRequestedEventArgs e)
    {
        // Un nuevo envio descarta los errores de la API anteriores; si vuelve a fallar llegara un Error nuevo.
        almacen?.Clear();
    }

    private void AlCambiarCampo(object? remitente, FieldChangedEventArgs e)
    {
        almacen?.Clear(e.FieldIdentifier);
        if (contextoActual is not null)
        {
            almacen?.Clear(new FieldIdentifier(contextoActual.Model, string.Empty));
        }
    }

    private void Desuscribir()
    {
        if (contextoActual is not null)
        {
            contextoActual.OnValidationRequested -= AlValidar;
            contextoActual.OnFieldChanged -= AlCambiarCampo;
        }
    }
}
