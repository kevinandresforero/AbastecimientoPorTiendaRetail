using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Abastecimiento.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "nodoscomerciales");

            migrationBuilder.EnsureSchema(
                name: "tiendas");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "MarcasTienda",
                schema: "tiendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarcasTienda", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NivelesJerarquia",
                schema: "nodoscomerciales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Profundidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NivelesJerarquia", x => x.Id);
                    table.CheckConstraint("CK_NivelesJerarquia_Profundidad", "[Profundidad] >= 1");
                });

            migrationBuilder.CreateTable(
                name: "Regionales",
                schema: "tiendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regionales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResponsablesVisuales",
                schema: "tiendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsablesVisuales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposEspacio",
                schema: "tiendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposEspacio", x => x.Id);
                    table.CheckConstraint("CK_TiposEspacio_Categoria", "[Categoria] IN ('Mobiliario','Perimetral')");
                });

            migrationBuilder.CreateTable(
                name: "TiposTienda",
                schema: "tiendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposTienda", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreUsuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    HashContrasena = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SelloSeguridad = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IntentosFallidos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BloqueadoHastaUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.CheckConstraint("CK_Usuarios_IntentosFallidos", "[IntentosFallidos] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "ConceptosComerciales",
                schema: "nodoscomerciales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NivelJerarquiaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnidadesPorEspacio = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptosComerciales", x => x.Id);
                    table.CheckConstraint("CK_ConceptosComerciales_UnidadesPorEspacio", "[UnidadesPorEspacio] IS NULL OR [UnidadesPorEspacio] > 0");
                    table.ForeignKey(
                        name: "FK_ConceptosComerciales_NivelesJerarquia_NivelJerarquiaId",
                        column: x => x.NivelJerarquiaId,
                        principalSchema: "nodoscomerciales",
                        principalTable: "NivelesJerarquia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tiendas",
                schema: "tiendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoHanna = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MarcaTiendaId = table.Column<int>(type: "int", nullable: false),
                    RegionalId = table.Column<int>(type: "int", nullable: false),
                    TipoTiendaId = table.Column<int>(type: "int", nullable: false),
                    ResponsableVisualId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tiendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tiendas_MarcasTienda_MarcaTiendaId",
                        column: x => x.MarcaTiendaId,
                        principalSchema: "tiendas",
                        principalTable: "MarcasTienda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tiendas_Regionales_RegionalId",
                        column: x => x.RegionalId,
                        principalSchema: "tiendas",
                        principalTable: "Regionales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tiendas_ResponsablesVisuales_ResponsableVisualId",
                        column: x => x.ResponsableVisualId,
                        principalSchema: "tiendas",
                        principalTable: "ResponsablesVisuales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tiendas_TiposTienda_TipoTiendaId",
                        column: x => x.TipoTiendaId,
                        principalSchema: "tiendas",
                        principalTable: "TiposTienda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SesionesToken",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Jti = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HashTokenRefresco = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FechaEmisionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaExpiracionUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRevocacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoRevocacion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ReemplazadaPorSesionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DireccionIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    AgenteUsuario = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionesToken", x => x.Id);
                    table.CheckConstraint("CK_SesionesToken_Fechas", "[FechaExpiracionUtc] > [FechaEmisionUtc]");
                    table.CheckConstraint("CK_SesionesToken_MotivoRevocacion", "[MotivoRevocacion] IS NULL OR [MotivoRevocacion] IN ('Rotacion','CierreSesion','CambioContrasena','ReusoDetectado','Administrativa')");
                    table.CheckConstraint("CK_SesionesToken_Reemplazo", "[ReemplazadaPorSesionId] IS NULL OR [MotivoRevocacion] = 'Rotacion'");
                    table.CheckConstraint("CK_SesionesToken_Revocacion", "([FechaRevocacionUtc] IS NULL AND [MotivoRevocacion] IS NULL) OR ([FechaRevocacionUtc] IS NOT NULL AND [MotivoRevocacion] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_SesionesToken_SesionesToken_ReemplazadaPorSesionId",
                        column: x => x.ReemplazadaPorSesionId,
                        principalSchema: "auth",
                        principalTable: "SesionesToken",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SesionesToken_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "auth",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosRoles",
                schema: "auth",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosRoles", x => new { x.UsuarioId, x.RolId });
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_Roles_RolId",
                        column: x => x.RolId,
                        principalSchema: "auth",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "auth",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NodosComerciales",
                schema: "nodoscomerciales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NodoPadreId = table.Column<int>(type: "int", nullable: true),
                    ConceptoComercialId = table.Column<int>(type: "int", nullable: false),
                    PorcentajeParticipacion = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodosComerciales", x => x.Id);
                    table.CheckConstraint("CK_NodosComerciales_PorcentajeParticipacion", "[PorcentajeParticipacion] > 0 AND [PorcentajeParticipacion] <= 1");
                    table.ForeignKey(
                        name: "FK_NodosComerciales_ConceptosComerciales_ConceptoComercialId",
                        column: x => x.ConceptoComercialId,
                        principalSchema: "nodoscomerciales",
                        principalTable: "ConceptosComerciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NodosComerciales_NodosComerciales_NodoPadreId",
                        column: x => x.NodoPadreId,
                        principalSchema: "nodoscomerciales",
                        principalTable: "NodosComerciales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EspaciosTienda",
                schema: "tiendas",
                columns: table => new
                {
                    TiendaId = table.Column<int>(type: "int", nullable: false),
                    TipoEspacioId = table.Column<int>(type: "int", nullable: false),
                    CantidadEspacios = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EspaciosTienda", x => new { x.TiendaId, x.TipoEspacioId });
                    table.CheckConstraint("CK_EspaciosTienda_CantidadEspacios", "[CantidadEspacios] >= 0");
                    table.ForeignKey(
                        name: "FK_EspaciosTienda_Tiendas_TiendaId",
                        column: x => x.TiendaId,
                        principalSchema: "tiendas",
                        principalTable: "Tiendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EspaciosTienda_TiposEspacio_TipoEspacioId",
                        column: x => x.TipoEspacioId,
                        principalSchema: "tiendas",
                        principalTable: "TiposEspacio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "tiendas",
                table: "MarcasTienda",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "YAHAD" },
                    { 2, "LILI PINK" },
                    { 3, "YOI" }
                });

            migrationBuilder.InsertData(
                schema: "nodoscomerciales",
                table: "NivelesJerarquia",
                columns: new[] { "Id", "Nombre", "Profundidad" },
                values: new object[,]
                {
                    { 1, "Unidad de Negocio", 1 },
                    { 2, "Categoría", 2 },
                    { 3, "Marca de Producto", 3 },
                    { 4, "Segmento", 4 },
                    { 5, "Subcategoría", 5 },
                    { 6, "Perfil", 6 },
                    { 7, "Tipo de Producto", 7 }
                });

            migrationBuilder.InsertData(
                schema: "tiendas",
                table: "Regionales",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "BOGOTA" },
                    { 2, "ANTIOQUIA" }
                });

            migrationBuilder.InsertData(
                schema: "auth",
                table: "Roles",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Crea tiendas y administra usuarios, roles y catalogos", "admin" },
                    { 2, "Administra tiendas existentes (actualiza sus datos y espacios); no crea tiendas", "admin_tienda" },
                    { 3, "Parametriza el arbol comercial (conceptos, nodos y porcentajes)", "Analista" },
                    { 4, "Solo lectura de calculos y reportes", "Consulta" }
                });

            migrationBuilder.InsertData(
                schema: "tiendas",
                table: "TiposEspacio",
                columns: new[] { "Id", "Categoria", "Nombre" },
                values: new object[,]
                {
                    { 1, "Mobiliario", "Isla" },
                    { 2, "Mobiliario", "Casa David" },
                    { 3, "Mobiliario", "Mesa" },
                    { 4, "Mobiliario", "Góndola" },
                    { 5, "Mobiliario", "Mesa Central" },
                    { 6, "Mobiliario", "Exhibidor" },
                    { 7, "Perimetral", "Perimetral" }
                });

            migrationBuilder.InsertData(
                schema: "tiendas",
                table: "TiposTienda",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "MIXTA" },
                    { 2, "MONOMARCA" }
                });

            migrationBuilder.InsertData(
                schema: "nodoscomerciales",
                table: "ConceptosComerciales",
                columns: new[] { "Id", "NivelJerarquiaId", "Nombre", "UnidadesPorEspacio" },
                values: new object[,]
                {
                    { 1, 1, "BELLEZA Y BIENESTAR", null },
                    { 2, 2, "BEAUTY", null },
                    { 3, 3, "LILI BEAUTY", null },
                    { 4, 3, "KARITE", null },
                    { 5, 4, "GIRLS", null },
                    { 6, 5, "MAKEUP", null },
                    { 7, 6, "LABIOS", null },
                    { 8, 6, "OJOS", null },
                    { 9, 6, "ROSTRO", null },
                    { 10, 7, "LABIALES", 660 },
                    { 11, 7, "BRILLO LABIOS", 540 },
                    { 12, 7, "PESTAÑINA", 288 },
                    { 13, 7, "DELINEADOR", 420 },
                    { 14, 7, "BASE / POLVOS", 310 },
                    { 15, 7, "BÁLSAMO LABIAL", 600 }
                });

            migrationBuilder.InsertData(
                schema: "tiendas",
                table: "Tiendas",
                columns: new[] { "Id", "CodigoHanna", "MarcaTiendaId", "Nombre", "RegionalId", "ResponsableVisualId", "TipoTiendaId" },
                values: new object[,]
                {
                    { 1, 5503, 1, "LILI EN YOI YAHAD TOBERIN BOGOTA", 1, null, 1 },
                    { 2, 1001, 2, "LILI PINK UNICENTRO BOGOTA", 1, null, 2 },
                    { 3, 2045, 3, "YOI VIVA ENVIGADO", 2, null, 2 }
                });

            migrationBuilder.InsertData(
                schema: "tiendas",
                table: "EspaciosTienda",
                columns: new[] { "TiendaId", "TipoEspacioId", "CantidadEspacios" },
                values: new object[,]
                {
                    { 1, 1, 7 },
                    { 1, 2, 4 },
                    { 1, 3, 4 },
                    { 1, 7, 7 },
                    { 2, 1, 6 },
                    { 2, 4, 10 },
                    { 2, 7, 14 },
                    { 3, 5, 8 },
                    { 3, 6, 6 },
                    { 3, 7, 6 }
                });

            migrationBuilder.InsertData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                columns: new[] { "Id", "ConceptoComercialId", "NodoPadreId", "PorcentajeParticipacion" },
                values: new object[,]
                {
                    { 1, 1, null, 1.00m },
                    { 2, 2, 1, 0.30m },
                    { 3, 3, 2, 0.94m },
                    { 4, 4, 2, 0.06m },
                    { 5, 5, 3, 1.00m },
                    { 15, 5, 4, 1.00m },
                    { 6, 6, 5, 1.00m },
                    { 16, 6, 15, 1.00m },
                    { 7, 7, 6, 0.40m },
                    { 8, 8, 6, 0.45m },
                    { 9, 9, 6, 0.15m },
                    { 17, 7, 16, 1.00m },
                    { 10, 10, 7, 0.50m },
                    { 11, 11, 7, 0.50m },
                    { 12, 12, 8, 0.60m },
                    { 13, 13, 8, 0.40m },
                    { 14, 14, 9, 1.00m },
                    { 18, 15, 17, 1.00m }
                });

            migrationBuilder.CreateIndex(
                name: "UQ_ConceptosComerciales_NivelJerarquiaId_Nombre",
                schema: "nodoscomerciales",
                table: "ConceptosComerciales",
                columns: new[] { "NivelJerarquiaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EspaciosTienda_TipoEspacioId",
                schema: "tiendas",
                table: "EspaciosTienda",
                column: "TipoEspacioId");

            migrationBuilder.CreateIndex(
                name: "UQ_MarcasTienda_Nombre",
                schema: "tiendas",
                table: "MarcasTienda",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_NivelesJerarquia_Nombre",
                schema: "nodoscomerciales",
                table: "NivelesJerarquia",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_NivelesJerarquia_Profundidad",
                schema: "nodoscomerciales",
                table: "NivelesJerarquia",
                column: "Profundidad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NodosComerciales_ConceptoComercialId",
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                column: "ConceptoComercialId");

            migrationBuilder.CreateIndex(
                name: "UQ_NodosComerciales_NodoPadreId_ConceptoComercialId",
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                columns: new[] { "NodoPadreId", "ConceptoComercialId" },
                unique: true,
                filter: "[NodoPadreId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_Regionales_Nombre",
                schema: "tiendas",
                table: "Regionales",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ResponsablesVisuales_Nombre",
                schema: "tiendas",
                table: "ResponsablesVisuales",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Roles_Nombre",
                schema: "auth",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SesionesToken_ReemplazadaPorSesionId",
                schema: "auth",
                table: "SesionesToken",
                column: "ReemplazadaPorSesionId");

            migrationBuilder.CreateIndex(
                name: "IX_SesionesToken_UsuarioId_FechaExpiracionUtc",
                schema: "auth",
                table: "SesionesToken",
                columns: new[] { "UsuarioId", "FechaExpiracionUtc" });

            migrationBuilder.CreateIndex(
                name: "UQ_SesionesToken_HashTokenRefresco",
                schema: "auth",
                table: "SesionesToken",
                column: "HashTokenRefresco",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_SesionesToken_Jti",
                schema: "auth",
                table: "SesionesToken",
                column: "Jti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tiendas_MarcaTiendaId",
                schema: "tiendas",
                table: "Tiendas",
                column: "MarcaTiendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tiendas_RegionalId_MarcaTiendaId",
                schema: "tiendas",
                table: "Tiendas",
                columns: new[] { "RegionalId", "MarcaTiendaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tiendas_ResponsableVisualId",
                schema: "tiendas",
                table: "Tiendas",
                column: "ResponsableVisualId");

            migrationBuilder.CreateIndex(
                name: "IX_Tiendas_TipoTiendaId",
                schema: "tiendas",
                table: "Tiendas",
                column: "TipoTiendaId");

            migrationBuilder.CreateIndex(
                name: "UQ_Tiendas_CodigoHanna",
                schema: "tiendas",
                table: "Tiendas",
                column: "CodigoHanna",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_TiposEspacio_Nombre",
                schema: "tiendas",
                table: "TiposEspacio",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_TiposTienda_Nombre",
                schema: "tiendas",
                table: "TiposTienda",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Usuarios_Correo",
                schema: "auth",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Usuarios_NombreUsuario",
                schema: "auth",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosRoles_RolId",
                schema: "auth",
                table: "UsuariosRoles",
                column: "RolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EspaciosTienda",
                schema: "tiendas");

            migrationBuilder.DropTable(
                name: "NodosComerciales",
                schema: "nodoscomerciales");

            migrationBuilder.DropTable(
                name: "SesionesToken",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "UsuariosRoles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Tiendas",
                schema: "tiendas");

            migrationBuilder.DropTable(
                name: "TiposEspacio",
                schema: "tiendas");

            migrationBuilder.DropTable(
                name: "ConceptosComerciales",
                schema: "nodoscomerciales");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Usuarios",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "MarcasTienda",
                schema: "tiendas");

            migrationBuilder.DropTable(
                name: "Regionales",
                schema: "tiendas");

            migrationBuilder.DropTable(
                name: "ResponsablesVisuales",
                schema: "tiendas");

            migrationBuilder.DropTable(
                name: "TiposTienda",
                schema: "tiendas");

            migrationBuilder.DropTable(
                name: "NivelesJerarquia",
                schema: "nodoscomerciales");
        }
    }
}
