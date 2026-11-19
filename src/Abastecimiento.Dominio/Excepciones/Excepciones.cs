namespace Abastecimiento.Dominio.Excepciones;

/// <summary>El recurso solicitado no existe (HTTP 404).</summary>
public class NoEncontradoExcepcion(string mensaje) : Exception(mensaje);

/// <summary>El recurso entra en conflicto con uno existente, ej. codigo HANNA duplicado (HTTP 409).</summary>
public class ConflictoExcepcion(string mensaje) : Exception(mensaje);

/// <summary>Se viola una regla de negocio (HTTP 400). Contiene el detalle de cada error.</summary>
public class ValidacionDominioExcepcion : Exception
{
    public ValidacionDominioExcepcion(IReadOnlyList<string> errores)
        : base("Se violaron una o mas reglas de negocio.")
    {
        Errores = errores;
    }

    public ValidacionDominioExcepcion(string error) : this(new[] { error })
    {
    }

    public IReadOnlyList<string> Errores { get; }
}
