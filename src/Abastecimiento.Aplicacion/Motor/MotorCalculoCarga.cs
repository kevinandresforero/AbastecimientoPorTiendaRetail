using Abastecimiento.Aplicacion.Configuracion;
using Abastecimiento.Aplicacion.Helpers;
using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Microsoft.Extensions.Options;

namespace Abastecimiento.Aplicacion.Motor;

/// <summary>
/// Implementacion del motor. Todo el calculo usa <see cref="decimal"/> sin redondeos intermedios;
/// el unico redondeo es la Carga Total de cada linea (<see cref="RedondeoHelper"/>). La profundidad hoja
/// se deriva de los datos: no hay numero de nivel fijo en el codigo.
/// </summary>
public sealed class MotorCalculoCarga(IOptions<OpcionesCalculo> opciones) : IMotorCalculoCarga
{
    public ResultadoCalculo Calcular(int espaciosTotales, IReadOnlyList<NodoComercial> nodos)
    {
        var modo = opciones.Value.ModoRedondeo;
        var arbol = ArbolComercialHelper.Indexar(nodos);

        var calculados = new List<NodoCalculado>();
        var lineas = new List<LineaCalculada>();
        var advertencias = new List<string>();

        // Regla 2: espacios del nodo = espacios del padre x % de participacion (desde los espacios totales).
        foreach (var recorrido in ArbolComercialHelper.RecorrerEnCascada(arbol, espaciosTotales))
        {
            calculados.Add(new NodoCalculado(recorrido.Nodo, recorrido.Valor));

            if (ArbolComercialHelper.EsHoja(recorrido.Nodo, arbol.ProfundidadHoja))
            {
                if (recorrido.Nodo.ConceptoComercial.UnidadesPorEspacio is not int unidadesPorEspacio)
                {
                    advertencias.Add(
                        $"El tipo de producto {ArbolComercialHelper.RutaLegible(recorrido.Camino)} no tiene unidades por espacio; se omite del calculo.");
                    continue;
                }

                // Regla 3: carga = espacios del tipo de producto x unidades por espacio; se redondea solo aqui.
                var cargaCalculada = recorrido.Valor * unidadesPorEspacio;
                lineas.Add(new LineaCalculada(
                    recorrido.Nodo,
                    recorrido.Camino,
                    recorrido.Valor,
                    unidadesPorEspacio,
                    cargaCalculada,
                    RedondeoHelper.RedondearCarga(cargaCalculada, modo)));
            }
            else if (!arbol.HijosDe[recorrido.Nodo.Id].Any())
            {
                advertencias.Add(
                    $"La rama {ArbolComercialHelper.RutaLegible(recorrido.Camino)} termina en profundidad " +
                    $"{recorrido.Nodo.ConceptoComercial.NivelJerarquia.Profundidad} sin llegar al nivel hoja ({arbol.ProfundidadHoja}).");
            }
        }

        return new ResultadoCalculo(
            calculados,
            lineas,
            lineas.Sum(l => l.EspaciosCalculados),
            lineas.Sum(l => l.CargaTotal),
            advertencias);
    }
}
