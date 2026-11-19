using Abastecimiento.Aplicacion.Dtos.ArbolComercial;
using Abastecimiento.Aplicacion.Helpers;
using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Dominio.Interfaces;
using Abastecimiento.Dominio.Reglas;

namespace Abastecimiento.Aplicacion.Servicios;

public sealed class ServicioArbolComercial(
    IArbolComercialRepositorio arbolRepositorio,
    ITiendaRepositorio tiendas,
    IUnidadDeTrabajo unidadDeTrabajo) : IArbolComercialServicio
{
    public async Task<IReadOnlyList<NivelRespuesta>> ObtenerNivelesAsync(CancellationToken ct)
    {
        var niveles = await arbolRepositorio.ObtenerNivelesAsync(ct);
        var profundidadHoja = ProfundidadHoja(niveles);
        return niveles.Select(n => MapeoHelper.ANivelRespuesta(n, profundidadHoja)).ToList();
    }

    public async Task<IReadOnlyList<NodoArbolRespuesta>> ObtenerArbolAsync(int codigoHanna, CancellationToken ct)
    {
        var tienda = await ObtenerTiendaAsync(codigoHanna, ct);
        var niveles = await arbolRepositorio.ObtenerNivelesAsync(ct);
        var nodos = await arbolRepositorio.ObtenerArbolPorTiendaAsync(tienda.Id, ct);

        return MapearArbol(nodos, niveles);
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

    public async Task<NodoArbolRespuesta> CrearNodoAsync(int codigoHanna, CrearNodoSolicitud solicitud, CancellationToken ct)
    {
        var tienda = await ObtenerTiendaAsync(codigoHanna, ct);
        var concepto = await arbolRepositorio.ObtenerConceptoPorIdAsync(solicitud.ConceptoComercialId, ct)
            ?? throw new ValidacionDominioExcepcion($"El concepto comercial {solicitud.ConceptoComercialId} no existe.");

        var nodos = await arbolRepositorio.ObtenerArbolPorTiendaAsync(tienda.Id, ct);

        NodoComercial? padre = null;
        if (solicitud.NodoPadreId is int padreId)
        {
            padre = await arbolRepositorio.ObtenerNodoParaEditarAsync(padreId, ct)
                ?? throw new NoEncontradoExcepcion($"No existe el nodo padre {padreId}.");

            if (padre.TiendaId != tienda.Id)
            {
                throw new ValidacionDominioExcepcion(
                    $"El nodo padre {padreId} pertenece a la tienda {padre.TiendaId}; debe ser de la tienda {tienda.Id}.");
            }
        }

        if (await arbolRepositorio.ExisteNodoAsync(tienda.Id, solicitud.NodoPadreId, concepto.Id, ct))
        {
            throw new ConflictoExcepcion(
                $"El concepto '{concepto.Nombre}' ya esta ubicado bajo ese nodo padre en la tienda {codigoHanna}.");
        }

        // Copia con navegaciones solo para validar (profundidad = padre + 1, % en (0, 1], suma de hermanos <= 1).
        var candidato = new NodoComercial
        {
            TiendaId = tienda.Id,
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
            TiendaId = tienda.Id,
            NodoPadreId = solicitud.NodoPadreId,
            ConceptoComercialId = concepto.Id,
            PorcentajeParticipacion = solicitud.PorcentajeParticipacion
        };
        await arbolRepositorio.AgregarNodoAsync(nuevo, ct);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return await ObtenerNodoDelArbolAsync(nuevo.Id, tienda.Id, ct);
    }

    public async Task<NodoArbolRespuesta> ActualizarPorcentajeAsync(int nodoId, ActualizarPorcentajeSolicitud solicitud, CancellationToken ct)
    {
        var nodo = await arbolRepositorio.ObtenerNodoParaEditarAsync(nodoId, ct)
            ?? throw new NoEncontradoExcepcion($"No existe el nodo {nodoId}.");

        var nodos = await arbolRepositorio.ObtenerArbolPorTiendaAsync(nodo.TiendaId, ct);
        var padre = nodo.NodoPadreId is int padreId ? nodos.FirstOrDefault(n => n.Id == padreId) : null;
        var hermanos = nodos.Where(n => n.NodoPadreId == nodo.NodoPadreId && n.Id != nodo.Id);

        // Se valida sobre una copia para no dejar el nodo rastreado modificado si la regla falla.
        var candidato = new NodoComercial
        {
            Id = nodo.Id,
            TiendaId = nodo.TiendaId,
            NodoPadreId = nodo.NodoPadreId,
            ConceptoComercialId = nodo.ConceptoComercialId,
            PorcentajeParticipacion = solicitud.PorcentajeParticipacion,
            ConceptoComercial = nodo.ConceptoComercial
        };
        ValidadorArbolComercial.ValidarNodo(candidato, padre, hermanos);

        nodo.PorcentajeParticipacion = solicitud.PorcentajeParticipacion;
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return await ObtenerNodoDelArbolAsync(nodo.Id, nodo.TiendaId, ct);
    }

    public async Task<CopiaArbolRespuesta> CopiarArbolAsync(int codigoHannaDestino, int codigoHannaOrigen, CancellationToken ct)
    {
        var destino = await ObtenerTiendaAsync(codigoHannaDestino, ct);
        var origen = await ObtenerTiendaAsync(codigoHannaOrigen, ct);

        var nodosDestino = await arbolRepositorio.ObtenerArbolPorTiendaAsync(destino.Id, ct);
        if (nodosDestino.Count > 0)
        {
            throw new ConflictoExcepcion(
                $"La tienda {codigoHannaDestino} ya tiene arbol comercial; la copia solo se permite si esta vacia.");
        }

        var nodosOrigen = await arbolRepositorio.ObtenerArbolPorTiendaAsync(origen.Id, ct);
        if (nodosOrigen.Count == 0)
        {
            throw new ValidacionDominioExcepcion($"La tienda origen {codigoHannaOrigen} no tiene arbol comercial.");
        }

        var niveles = await arbolRepositorio.ObtenerNivelesAsync(ct);
        var errores = ValidadorArbolComercial.ValidarArbol(nodosOrigen, ProfundidadHoja(niveles));
        if (errores.Count > 0)
        {
            throw new ValidacionDominioExcepcion(errores);
        }

        var clones = ClonarArbol(nodosOrigen, destino.Id);
        await arbolRepositorio.AgregarNodosAsync(clones, ct);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return new CopiaArbolRespuesta(codigoHannaOrigen, codigoHannaDestino, clones.Count);
    }

    private async Task<Tienda> ObtenerTiendaAsync(int codigoHanna, CancellationToken ct) =>
        await tiendas.ObtenerPorCodigoHannaAsync(codigoHanna, ct)
        ?? throw new NoEncontradoExcepcion($"No existe una tienda con el codigo HANNA {codigoHanna}.");

    private static IReadOnlyList<NodoArbolRespuesta> MapearArbol(
        IReadOnlyList<NodoComercial> nodos,
        IReadOnlyList<NivelJerarquia> niveles)
    {
        var profundidadHoja = ProfundidadHoja(niveles);
        var arbol = ArbolComercialHelper.Indexar(nodos, profundidadHoja);
        return ArbolComercialHelper.RecorrerEnCascada(arbol, 1m)
            .Select(r => MapeoHelper.ANodoArbolRespuesta(r, profundidadHoja))
            .ToList();
    }

    private async Task<NodoArbolRespuesta> ObtenerNodoDelArbolAsync(int nodoId, int tiendaId, CancellationToken ct)
    {
        var niveles = await arbolRepositorio.ObtenerNivelesAsync(ct);
        var nodos = await arbolRepositorio.ObtenerArbolPorTiendaAsync(tiendaId, ct);
        var profundidadHoja = ProfundidadHoja(niveles);
        var arbol = ArbolComercialHelper.Indexar(nodos, profundidadHoja);
        var recorrido = ArbolComercialHelper.RecorrerEnCascada(arbol, 1m)
            .FirstOrDefault(r => r.Nodo.Id == nodoId)
            ?? throw new NoEncontradoExcepcion($"No existe el nodo {nodoId} en la tienda {tiendaId}.");

        return MapeoHelper.ANodoArbolRespuesta(recorrido, profundidadHoja);
    }

    /// <summary>
    /// Clona los nodos de una tienda en otra. Los hijos se enlazan por la navegacion <c>NodoPadre</c> para que EF
    /// asigne las nuevas claves en un solo <c>SaveChanges</c>.
    /// </summary>
    private static List<NodoComercial> ClonarArbol(IReadOnlyList<NodoComercial> origen, int tiendaDestinoId)
    {
        var clonesPorId = origen.ToDictionary(
            n => n.Id,
            n => new NodoComercial
            {
                TiendaId = tiendaDestinoId,
                ConceptoComercialId = n.ConceptoComercialId,
                PorcentajeParticipacion = n.PorcentajeParticipacion
            });

        foreach (var original in origen)
        {
            if (original.NodoPadreId is int padreId)
            {
                clonesPorId[original.Id].NodoPadre = clonesPorId[padreId];
            }
        }

        return clonesPorId.Values.ToList();
    }

    private static int ProfundidadHoja(IReadOnlyList<NivelJerarquia> niveles) =>
        niveles.Count == 0 ? 0 : ValidadorArbolComercial.ObtenerProfundidadHoja(niveles);
}
