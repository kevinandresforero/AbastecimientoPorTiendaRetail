namespace Abastecimiento.Dominio.Entidades.Tiendas;

/// <summary>Marca de la tienda (YAHAD, LILI PINK, YOI). Distinta de la marca de producto del arbol.</summary>
public class MarcaTienda : EntidadCatalogo;

/// <summary>Regional a la que pertenece la tienda (BOGOTA, ANTIOQUIA...).</summary>
public class Regional : EntidadCatalogo;

/// <summary>Tipo de tienda (MIXTA, MONOMARCA...).</summary>
public class TipoTienda : EntidadCatalogo;

/// <summary>Persona responsable del visual de una o varias tiendas.</summary>
public class ResponsableVisual : EntidadCatalogo;
