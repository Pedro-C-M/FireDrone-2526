using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentralBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ControlStations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Lat = table.Column<float>(type: "REAL", nullable: true),
                    Lon = table.Column<float>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControlStations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Perimeters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perimeters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaseStations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ControlStationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseStations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseStations_ControlStations_ControlStationId",
                        column: x => x.ControlStationId,
                        principalTable: "ControlStations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Coordinate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    PerimeterId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coordinate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coordinate_Perimeters_PerimeterId",
                        column: x => x.PerimeterId,
                        principalTable: "Perimeters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PerimeterId = table.Column<int>(type: "INTEGER", nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routes_Perimeters_PerimeterId",
                        column: x => x.PerimeterId,
                        principalTable: "Perimeters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Drones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BaseStationId = table.Column<int>(type: "INTEGER", nullable: true),
                    ControlStationId = table.Column<int>(type: "INTEGER", nullable: true),
                    FlightPlanId = table.Column<int>(type: "INTEGER", nullable: true),
                    SampleId = table.Column<int>(type: "INTEGER", nullable: true),
                    State = table.Column<string>(type: "TEXT", nullable: true),
                    Lat = table.Column<float>(type: "REAL", nullable: true),
                    Lon = table.Column<float>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drones_BaseStations_BaseStationId",
                        column: x => x.BaseStationId,
                        principalTable: "BaseStations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Drones_ControlStations_ControlStationId",
                        column: x => x.ControlStationId,
                        principalTable: "ControlStations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DronCharacteristics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DronId = table.Column<int>(type: "INTEGER", nullable: true),
                    Model = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DronCharacteristics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DronCharacteristics_Drones_DronId",
                        column: x => x.DronId,
                        principalTable: "Drones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Samples",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DronId = table.Column<int>(type: "INTEGER", nullable: true),
                    File = table.Column<string>(type: "TEXT", nullable: true),
                    Lat = table.Column<float>(type: "REAL", nullable: true),
                    Lon = table.Column<float>(type: "REAL", nullable: true),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Samples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Samples_Drones_DronId",
                        column: x => x.DronId,
                        principalTable: "Drones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Model = table.Column<string>(type: "TEXT", nullable: true),
                    DronCharacteristicsId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sensors_DronCharacteristics_DronCharacteristicsId",
                        column: x => x.DronCharacteristicsId,
                        principalTable: "DronCharacteristics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChangeModes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Moment = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false),
                    FlightPlanId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeModes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlightPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RutaId = table.Column<int>(type: "INTEGER", nullable: false),
                    DronId = table.Column<int>(type: "INTEGER", nullable: false),
                    EstControlId = table.Column<int>(type: "INTEGER", nullable: true),
                    StartingPointId = table.Column<int>(type: "INTEGER", nullable: true),
                    EndingPointId = table.Column<int>(type: "INTEGER", nullable: true),
                    CtrlId = table.Column<int>(type: "INTEGER", nullable: true),
                    StartPointId = table.Column<int>(type: "INTEGER", nullable: true),
                    EndPointId = table.Column<int>(type: "INTEGER", nullable: true),
                    StartingTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndingTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    State = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightPlans_ControlStations_CtrlId",
                        column: x => x.CtrlId,
                        principalTable: "ControlStations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FlightPlans_Drones_DronId",
                        column: x => x.DronId,
                        principalTable: "Drones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlightPlans_Routes_RutaId",
                        column: x => x.RutaId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incidences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FlightPlanId = table.Column<int>(type: "INTEGER", nullable: true),
                    Msg = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidences_FlightPlans_FlightPlanId",
                        column: x => x.FlightPlanId,
                        principalTable: "FlightPlans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoutePoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RouteId = table.Column<int>(type: "INTEGER", nullable: true),
                    Long = table.Column<float>(type: "REAL", nullable: true),
                    Lat = table.Column<float>(type: "REAL", nullable: true),
                    Height = table.Column<float>(type: "REAL", nullable: true),
                    Velocity = table.Column<float>(type: "REAL", nullable: true),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FlightPlanId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutePoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutePoints_FlightPlans_FlightPlanId",
                        column: x => x.FlightPlanId,
                        principalTable: "FlightPlans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoutePoints_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaseStations_ControlStationId",
                table: "BaseStations",
                column: "ControlStationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeModes_FlightPlanId",
                table: "ChangeModes",
                column: "FlightPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Coordinate_PerimeterId",
                table: "Coordinate",
                column: "PerimeterId");

            migrationBuilder.CreateIndex(
                name: "IX_DronCharacteristics_DronId",
                table: "DronCharacteristics",
                column: "DronId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drones_BaseStationId",
                table: "Drones",
                column: "BaseStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Drones_ControlStationId",
                table: "Drones",
                column: "ControlStationId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPlans_CtrlId",
                table: "FlightPlans",
                column: "CtrlId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPlans_DronId",
                table: "FlightPlans",
                column: "DronId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlightPlans_EndPointId",
                table: "FlightPlans",
                column: "EndPointId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPlans_RutaId",
                table: "FlightPlans",
                column: "RutaId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPlans_StartPointId",
                table: "FlightPlans",
                column: "StartPointId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidences_FlightPlanId",
                table: "Incidences",
                column: "FlightPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutePoints_FlightPlanId",
                table: "RoutePoints",
                column: "FlightPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutePoints_RouteId",
                table: "RoutePoints",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_PerimeterId",
                table: "Routes",
                column: "PerimeterId");

            migrationBuilder.CreateIndex(
                name: "IX_Samples_DronId",
                table: "Samples",
                column: "DronId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_DronCharacteristicsId",
                table: "Sensors",
                column: "DronCharacteristicsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeModes_FlightPlans_FlightPlanId",
                table: "ChangeModes",
                column: "FlightPlanId",
                principalTable: "FlightPlans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FlightPlans_RoutePoints_EndPointId",
                table: "FlightPlans",
                column: "EndPointId",
                principalTable: "RoutePoints",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FlightPlans_RoutePoints_StartPointId",
                table: "FlightPlans",
                column: "StartPointId",
                principalTable: "RoutePoints",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseStations_ControlStations_ControlStationId",
                table: "BaseStations");

            migrationBuilder.DropForeignKey(
                name: "FK_Drones_ControlStations_ControlStationId",
                table: "Drones");

            migrationBuilder.DropForeignKey(
                name: "FK_FlightPlans_ControlStations_CtrlId",
                table: "FlightPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_RoutePoints_FlightPlans_FlightPlanId",
                table: "RoutePoints");

            migrationBuilder.DropTable(
                name: "ChangeModes");

            migrationBuilder.DropTable(
                name: "Coordinate");

            migrationBuilder.DropTable(
                name: "Incidences");

            migrationBuilder.DropTable(
                name: "Samples");

            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.DropTable(
                name: "DronCharacteristics");

            migrationBuilder.DropTable(
                name: "ControlStations");

            migrationBuilder.DropTable(
                name: "FlightPlans");

            migrationBuilder.DropTable(
                name: "Drones");

            migrationBuilder.DropTable(
                name: "RoutePoints");

            migrationBuilder.DropTable(
                name: "BaseStations");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Perimeters");
        }
    }
}
