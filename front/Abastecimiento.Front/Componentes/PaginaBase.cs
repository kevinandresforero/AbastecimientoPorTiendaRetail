using Abastecimiento.Front.Clientes;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes;

/// <summary>
/// Base de los controladores de pantalla (<c>.razor.cs</c>): expone un token que se cancela al salir de la pagina, para
/// que las llamadas pendientes a la API no sigan corriendo.
/// </summary>
public abstract class PaginaBase : ComponentBase, IDisposable
{
    private readonly CancellationTokenSource cancelacion = new();
    private bool liberado;

    /// <summary>Token de las llamadas a la API de esta pagina.</summary>
    protected CancellationToken Cancelacion => liberado ? new CancellationToken(canceled: true) : cancelacion.Token;

    /// <summary>True si la pagina ya se cerro (no actualizar estado).</summary>
    protected bool Liberado => liberado;

    /// <summary>
    /// Ejecuta una llamada a la API y devuelve el error (o null si salio bien). Si la pagina se cerro, la cancelacion se
    /// ignora en silencio.
    /// </summary>
    protected async Task<ExcepcionApi?> LlamarApiAsync(Func<CancellationToken, Task> llamada)
    {
        try
        {
            await llamada(Cancelacion);
            return null;
        }
        catch (ExcepcionApi ex)
        {
            return ex;
        }
        catch (OperationCanceledException) when (Liberado)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Cancela las llamadas pendientes.</summary>
    protected virtual void Dispose(bool liberando)
    {
        if (liberado)
        {
            return;
        }

        liberado = true;
        if (liberando)
        {
            cancelacion.Cancel();
            cancelacion.Dispose();
        }
    }
}
