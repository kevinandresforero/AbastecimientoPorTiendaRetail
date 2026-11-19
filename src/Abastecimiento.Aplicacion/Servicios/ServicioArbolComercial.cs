using Abastecimiento.Aplicacion.Dtos.ArbolComercial;
using Abastecimiento.Aplicacion.Helpers;
using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Dominio.Interfaces;
using Abastecimiento.Dominio.Reglas;

namespace Abastecimiento.Aplicacion.Servicios;

public sealed class ServicioArbolComercial(IArbolComercialRepositorio arbolRepositorio, IUnidadDeTrabajo unidadDeTrabajo)
    : IArbolComercialServicio
{
    public async Task<IReadOnlyList<NivelRespuesta>> ObtenerNivelesAsync(CancellationToken ct)
    {
        var niveles = await arbolRepositorio.ObtenerNivelesAsync(ct);
        var profundidadHoja = ProfundidadHoja(niveles);
        return niveles.Select(n => MapeoHelper.ANivelRespuesta(n, profundidadHoja)).ToList();
    }

    public async Task<IReadOnlyList<NodoArbolRespuesta>> ObtenerArbolAsync(CancellationToken ct)
    {
        var arbol = ArbolComercialHelper.Indexar(await arbolRepositorio.ObtenerArbolCompletoAsync(ct));
        return ArbolComercialHelper.RecorrerEnCascada(arbol, 1m)
            .Select(r => MapeoHelper.ANodoArbolRespuesta(r, arbol.ProfundidadHoja))
            .ToList();
    }

    public async Task<IReadOnlyList<ConceptoRespuesta>> ListarConceptosAsync(int? nivelJerarquiaId, CancellationToken ct)
    {
        var profundidadHoja = ProfundidadHoja(await arbolRepositorio.ObtenerNivelesAsync(ct));
        var conceptos = await arbolRepositorio.ObtenerConceptosAsync(nivelJerarquiaId, ct);
        return conceptos.Select(c => MapeoHelper.AConceptoRespuesta(c, c.NivelJerarquia, profundidadHoja)).ToList();
    }

    public async Task<ConceptoRespuesta> CrearConceptoAsync(CrearConceptoSolicitud solicitud, CancellationToken ct)
    {
        var niveles = await arbolRepositorio.ObtenerNivelesAsync(ct);
        var nivel = niveles.FirstOrDefault(n => n.Id == solicitud.NivelJerarquiaId)
            ?? throw new ValidacionDominioExcepcion($"El nivel de jerarquia {solicitud.NivelJerarquiaId} no existe.");

        var nombre = solicitud.Nombre.Trim();
        if (nombre.Length == 0)
        {
            throw new ValidacionDominioExcepcion("El nombre del concepto es obligatorio.");
        }

        if (await arbolRepositorio.ExisteConceptoAsync(nivel.Id, nombre, ct))
        {
            throw new ConflictoExcepcion($"Ya existe el concepto '{nombre}' en el nivel {nivel.Nombre}.");
        }

        // Solo la FK: asignar la navegacion haria que EF intentara insertar el nivel de nuevo.
        var concepto = new ConceptoComercial
        {
            NivelJerarquiaId = nivel.Id,
            Nombre = nombre,
            UnidadesPorEspacio = solicitud.UnidadesPorEspacio
        };

        var profundidadHoja = ProfundidadHoja(niveles);
        ValidadorArbolComercial.ValidarConcepto(concepto, nivel, profundidadHoja);

        await arbolRepositorio.AgregarConceptoAsync(concepto, ct);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return MapeoHelper.AConceptoRespuesta(concepto, nivel, profundidadHoja);
    }

    public async Task<NodoArbolRespuesta> CrearNodoAsync(CrearNodoSolicitud solicitud, CancellationToken ct)
    {
        var concepto = await arbolRepositorio.ObtenerConceptoPorIdAsync(solicitud.ConceptoComercialId, ct)
            ?? throw new ValidacionDominioExcepcion($"El concepto comercial {solicitud.ConceptoComercialId} no existe.");

        var nodos = await arbolRepositorio.ObtenerArbolCompletoAsync(ct);

        NodoComercial? padre = null;
        if (solicitud.NodoPadreId is int padreId)
        {
            padre = nodos.FirstOrDefault(n => n.Id == padreId)
                ?? throw new NoEncontradoExcepcion($"No existe el nodo padre {padreId}.");
        }

        if (await arbolRepositorio.ExisteNodoAsync(solicitud.NodoPadreId, concepto.Id, ct))
        {
            throw new ConflictoExcepcion($"El concepto '{concepto.Nombre}' ya esta ubicado bajo ese nodo padre.");
        }

        // Copia con navegaciones solo para validar (profundidad = padre + 1, % en (0, 1], suma de hermanos <= 1).
        var candidato = new NodoComercial
        {
            NodoPadreId = solicitud.NodoPadreId,
            ConceptoComercialId = concepto.Id,
            PorcentajeParticipacion = solicitud.PorcentajeParticipacion,
            ConceptoComercial = concepto
        };
        var hermanos = nodos.Where(n => n.NodoPadreId == solicitud.NodoPadreId);
        ValidadorArbolComercial.ValidarNodo(candidato, padre, hermanos);

        // La entidad persistida lleva solo las FK para que EF no reinserte el concepto.
        var nuevo = new NodoComercial
        {
            NodoPadreId = solicitud.NodoPadreId,
            ConceptoComercialId = concepto.Id,
            PorcentajeParticipacion = solicitud.PorcentajeParticipacion
        };
        await arbolRepositorio.AgregarNodoAsync(nuevo, ct);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return await ObtenerNodoDelArbolAsync(nuevo.Id, ct);
    }

    public async Task<NodoArbolRespuesta> ActualizarPorcentajeAsync(int nodoId, ActualizarPorcentajeSolicitud solicitud, CancellationToken ct)
    {
        var nodo = await arbolRepositorio.ObtenerNodoParaEditarAsync(nodoId, ct)
            ?? throw new NoEncontradoExcepcion($"No existe el nodo {nodoId}.");

        var nodos = await arbolRepositorio.ObtenerArbolCompletoAsync(ct);
        var padre = nodo.NodoPadreId is int padreId ? nodos.FirstOrDefault(n => n.Id == padreId) : null;
        var hermanos = nodos.Where(n => n.NodoPadreId == nodo.NodoPadreId && n.Id != nodo.Id);

        nodo.PorcentajeParticipacion = solicitud.PorcentajeParticipacion;
        ValidadorArbolComercial.ValidarNodo(nodo, padre, hermanos);

        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return await ObtenerNodoDelArbolAsync(nodo.Id, ct);
    }

    private async Task<NodoArbolRespuesta> ObtenerNodoDelArbolAsync(int nodoId, CancellationToken ct)
    {
        var arbol = ArbolComercialHelper.Indexar(await arbolRepositorio.ObtenerArbolCompletoAsync(ct));
        var recorrido = ArbolComercialHelper.RecorrerEnCascada(arbol, 1m).FirstOrDefault(r => r.Nodo.Id == nodoId)
            ?? throw new NoEncontradoExcepcion($"No existe el nodo {nodoId}.");

        return MapeoHelper.ANodoArbolRespuesta(recorrido, arbol.ProfundidadHoja);
    }

    private static int ProfundidadHoja(IReadOnlyList<NivelJerarquia> niveles) =>
        niveles.Count == 0 ? 0 : ValidadorArbolComercial.ObtenerProfundidadHoja(niveles);
}
