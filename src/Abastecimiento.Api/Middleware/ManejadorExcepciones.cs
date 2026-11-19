using Abastecimiento.Dominio.Excepciones;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Abastecimiento.Api.Middleware;

/// <summary>
/// Unico punto de traduccion de excepciones a <c>application/problem+json</c> (RFC 7807).
/// Nunca expone stack traces: el detalle de los errores inesperados solo va al log.
/// </summary>
public sealed class ManejadorExcepciones(IProblemDetailsService problemDetails, ILogger<ManejadorExcepciones> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problema = exception switch
        {
            ValidacionDominioExcepcion validacion => ConErrores(
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Regla de negocio violada",
                    Detail = validacion.Message
                },
                validacion.Errores),
            NoEncontradoExcepcion => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Recurso no encontrado",
                Detail = exception.Message
            },
            ConflictoExcepcion => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflicto",
                Detail = exception.Message
            },
            DbUpdateException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflicto",
                Detail = "El registro viola una restriccion de la base de datos."
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno",
                Detail = "Ocurrio un error inesperado. Intente de nuevo o contacte al administrador."
            }
        };

        if (problema.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Error no controlado en {Metodo} {Ruta}", httpContext.Request.Method, httpContext.Request.Path);
        }
        else if (exception is DbUpdateException)
        {
            logger.LogWarning(exception, "Restriccion de base de datos violada en {Ruta}", httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = problema.Status!.Value;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problema,
            Exception = exception
        });
    }

    private static ProblemDetails ConErrores(ProblemDetails problema, IReadOnlyList<string> errores)
    {
        problema.Extensions["errores"] = errores;
        return problema;
    }
}
