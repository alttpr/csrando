using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Randomizer.Shared.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BasePatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Version = table.Column<string>(type: "TEXT", nullable: false),
                    Commit = table.Column<string>(type: "TEXT", nullable: false),
                    Hash = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasePatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seeds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Guid = table.Column<string>(type: "TEXT", nullable: false),
                    Mode = table.Column<string>(type: "TEXT", nullable: false),
                    SeedNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Spoiler = table.Column<string>(type: "TEXT", nullable: false),
                    GameName = table.Column<string>(type: "TEXT", nullable: false),
                    GameVersion = table.Column<string>(type: "TEXT", nullable: false),
                    GameId = table.Column<string>(type: "TEXT", nullable: false),
                    Hash = table.Column<string>(type: "TEXT", nullable: false),
                    Players = table.Column<int>(type: "INTEGER", nullable: false),
                    BasePatchId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seeds_BasePatches_BasePatchId",
                        column: x => x.BasePatchId,
                        principalTable: "BasePatches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Guid = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    SeedId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Seeds_SeedId",
                        column: x => x.SeedId,
                        principalTable: "Seeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Worlds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorldId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeedId = table.Column<int>(type: "INTEGER", nullable: false),
                    Guid = table.Column<string>(type: "TEXT", nullable: false),
                    Player = table.Column<string>(type: "TEXT", nullable: false),
                    Settings = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    Patch = table.Column<byte[]>(type: "BLOB", nullable: false),
                    SramBackup = table.Column<byte[]>(type: "BLOB", nullable: false),
                    WorldState = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Worlds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Worlds_Seeds_SeedId",
                        column: x => x.SeedId,
                        principalTable: "Seeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Guid = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Device = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    ConnectionId = table.Column<string>(type: "TEXT", nullable: false),
                    SessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    WorldId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clients_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SessionId = table.Column<int>(type: "INTEGER", nullable: false),
                    SequenceNum = table.Column<int>(type: "INTEGER", nullable: false),
                    FromWorldId = table.Column<int>(type: "INTEGER", nullable: false),
                    ToWorldId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemLocation = table.Column<int>(type: "INTEGER", nullable: false),
                    EventType = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Confirmed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionEvents_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorldId = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemWorldId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_Worlds_WorldId",
                        column: x => x.WorldId,
                        principalTable: "Worlds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ConnectionId",
                table: "Clients",
                column: "ConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_SessionId",
                table: "Clients",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_WorldId",
                table: "Locations",
                column: "WorldId");

            migrationBuilder.CreateIndex(
                name: "IX_Seeds_BasePatchId",
                table: "Seeds",
                column: "BasePatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionEvents_SessionId",
                table: "SessionEvents",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionEvents_ToWorldId_EventType",
                table: "SessionEvents",
                columns: new[] { "ToWorldId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_Guid",
                table: "Sessions",
                column: "Guid");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SeedId",
                table: "Sessions",
                column: "SeedId");

            migrationBuilder.CreateIndex(
                name: "IX_Worlds_SeedId",
                table: "Worlds",
                column: "SeedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "SessionEvents");

            migrationBuilder.DropTable(
                name: "Worlds");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Seeds");

            migrationBuilder.DropTable(
                name: "BasePatches");
        }
    }
}
