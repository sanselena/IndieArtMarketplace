using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IndieArtMarketplace.Migrations
{
    /// <inheritdoc />
    public partial class PostgresInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "click_logs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    button_type = table.Column<string>(type: "text", nullable: false),
                    clicked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    session_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_click_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "upload_logs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    content_type = table.Column<string>(type: "text", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_upload_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    userid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    registrationdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.userid);
                });

            migrationBuilder.CreateTable(
                name: "artworks",
                schema: "public",
                columns: table => new
                {
                    artworkid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fileurl = table.Column<string>(type: "text", nullable: false),
                    uploaddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    artistid = table.Column<int>(type: "integer", nullable: false),
                    license = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_artworks", x => x.artworkid);
                    table.ForeignKey(
                        name: "FK_artworks_users_artistid",
                        column: x => x.artistid,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "musictracks",
                schema: "public",
                columns: table => new
                {
                    trackid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fileurl = table.Column<string>(type: "text", nullable: false),
                    uploaddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    artistid = table.Column<int>(type: "integer", nullable: false),
                    license = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_musictracks", x => x.trackid);
                    table.ForeignKey(
                        name: "FK_musictracks_users_artistid",
                        column: x => x.artistid,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                schema: "public",
                columns: table => new
                {
                    transactionid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    buyerid = table.Column<int>(type: "integer", nullable: false),
                    artworkid = table.Column<int>(type: "integer", nullable: true),
                    trackid = table.Column<int>(type: "integer", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    purchasedate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.transactionid);
                    table.ForeignKey(
                        name: "FK_transactions_artworks_artworkid",
                        column: x => x.artworkid,
                        principalSchema: "public",
                        principalTable: "artworks",
                        principalColumn: "artworkid");
                    table.ForeignKey(
                        name: "FK_transactions_musictracks_trackid",
                        column: x => x.trackid,
                        principalSchema: "public",
                        principalTable: "musictracks",
                        principalColumn: "trackid");
                    table.ForeignKey(
                        name: "FK_transactions_users_buyerid",
                        column: x => x.buyerid,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "userid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_artworks_artistid",
                schema: "public",
                table: "artworks",
                column: "artistid");

            migrationBuilder.CreateIndex(
                name: "IX_musictracks_artistid",
                schema: "public",
                table: "musictracks",
                column: "artistid");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_artworkid_trackid",
                schema: "public",
                table: "transactions",
                columns: new[] { "artworkid", "trackid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transactions_buyerid",
                schema: "public",
                table: "transactions",
                column: "buyerid");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_trackid",
                schema: "public",
                table: "transactions",
                column: "trackid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "click_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "transactions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "upload_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "artworks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "musictracks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");
        }
    }
}
