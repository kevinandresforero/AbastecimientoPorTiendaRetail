using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Abastecimiento.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ArbolPorTienda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TiendaId",
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                type: "int",
                nullable: false,
                // EDITADO A MANO (spec de datos §7.1): EF genera 0. Con 1 todos los nodos existentes, incluidos los creados
                // a mano sobre el arbol global, quedan en la tienda 5503 (TiendaId 1); con 0 la FK del final fallaria.
                defaultValue: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 1,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 4,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 5,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 6,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 7,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 8,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 9,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 10,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 11,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 12,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 13,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 14,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 15,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 16,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 17,
                column: "TiendaId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 18,
                column: "TiendaId",
                value: 1);

            migrationBuilder.InsertData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                columns: new[] { "Id", "ConceptoComercialId", "NodoPadreId", "PorcentajeParticipacion", "TiendaId" },
                values: new object[,]
                {
                    { 2001, 1, null, 1.00m, 2 },
                    { 3001, 1, null, 1.00m, 3 },
                    { 2002, 2, 2001, 0.30m, 2 },
                    { 3002, 2, 3001, 0.30m, 3 },
                    { 2003, 3, 2002, 0.94m, 2 },
                    { 2004, 4, 2002, 0.06m, 2 },
                    { 3003, 3, 3002, 0.94m, 3 },
                    { 3004, 4, 3002, 0.06m, 3 },
                    { 2005, 5, 2003, 1.00m, 2 },
                    { 2015, 5, 2004, 1.00m, 2 },
                    { 3005, 5, 3003, 1.00m, 3 },
                    { 3015, 5, 3004, 1.00m, 3 },
                    { 2006, 6, 2005, 1.00m, 2 },
                    { 2016, 6, 2015, 1.00m, 2 },
                    { 3006, 6, 3005, 1.00m, 3 },
                    { 3016, 6, 3015, 1.00m, 3 },
                    { 2007, 7, 2006, 0.40m, 2 },
                    { 2008, 8, 2006, 0.45m, 2 },
                    { 2009, 9, 2006, 0.15m, 2 },
                    { 2017, 7, 2016, 1.00m, 2 },
                    { 3007, 7, 3006, 0.40m, 3 },
                    { 3008, 8, 3006, 0.45m, 3 },
                    { 3009, 9, 3006, 0.15m, 3 },
                    { 3017, 7, 3016, 1.00m, 3 },
                    { 2010, 10, 2007, 0.50m, 2 },
                    { 2011, 11, 2007, 0.50m, 2 },
                    { 2012, 12, 2008, 0.60m, 2 },
                    { 2013, 13, 2008, 0.40m, 2 },
                    { 2014, 14, 2009, 1.00m, 2 },
                    { 2018, 15, 2017, 1.00m, 2 },
                    { 3010, 10, 3007, 0.50m, 3 },
                    { 3011, 11, 3007, 0.50m, 3 },
                    { 3012, 12, 3008, 0.60m, 3 },
                    { 3013, 13, 3008, 0.40m, 3 },
                    { 3014, 14, 3009, 1.00m, 3 },
                    { 3018, 15, 3017, 1.00m, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_NodosComerciales_TiendaId",
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                column: "TiendaId");

            migrationBuilder.CreateIndex(
                name: "UQ_NodosComerciales_TiendaId_ConceptoComercialId_Raiz",
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                columns: new[] { "TiendaId", "ConceptoComercialId" },
                unique: true,
                filter: "[NodoPadreId] IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_NodosComerciales_Tiendas_TiendaId",
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                column: "TiendaId",
                principalSchema: "tiendas",
                principalTable: "Tiendas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodosComerciales_Tiendas_TiendaId",
                schema: "nodoscomerciales",
                table: "NodosComerciales");

            migrationBuilder.DropIndex(
                name: "IX_NodosComerciales_TiendaId",
                schema: "nodoscomerciales",
                table: "NodosComerciales");

            migrationBuilder.DropIndex(
                name: "UQ_NodosComerciales_TiendaId_ConceptoComercialId_Raiz",
                schema: "nodoscomerciales",
                table: "NodosComerciales");

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2010);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2011);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2012);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2013);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2014);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2018);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3010);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3011);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3012);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3013);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3014);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3018);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2007);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2008);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2009);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2017);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3007);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3008);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3009);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3017);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2006);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2016);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3006);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3016);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2005);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2015);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3005);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3015);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2003);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2004);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3003);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3004);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2002);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3002);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 2001);

            migrationBuilder.DeleteData(
                schema: "nodoscomerciales",
                table: "NodosComerciales",
                keyColumn: "Id",
                keyValue: 3001);

            migrationBuilder.DropColumn(
                name: "TiendaId",
                schema: "nodoscomerciales",
                table: "NodosComerciales");
        }
    }
}
