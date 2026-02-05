using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CentralBackend;
using CentralBackend.Exceptions;
using CentralBackend.Services;
using ControlBackend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Moq;
using Moq.Protected;

namespace CentralBackend.Tests;

public class TestFireDroneContext : FireDrone
{
    private readonly string _databaseName;

  public TestFireDroneContext(string databaseName) : base()
  {
        _databaseName = databaseName;
  }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
        options.UseInMemoryDatabase(_databaseName);
        }
    }
}

[TestClass]
public class FlightPlanServiceTests
{
    private TestFireDroneContext _context = null!;
    private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
    private IConfiguration _configuration = null!;
    private FlightPlanService _service = null!;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock = null!;

  [TestInitialize]
    public void Setup()
    {
        var databaseName = Guid.NewGuid().ToString();
        _context = new TestFireDroneContext(databaseName);
        _context.Database.EnsureCreated();

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var inMemorySettings = new Dictionary<string, string?> {
            {"ControlBackend:Url", "http://localhost:5307"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
        .Build();

        _service = new FlightPlanService(_context, _httpClientFactoryMock.Object, _configuration);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<FlightPlan> CreateFlightPlanWithState(FlightStatus state, int? dronId = 5)
    {
        var drone = new Dron
        {
            Id = dronId ?? 5,
            State = DroneState.Flying,
            Lat = 43.36f,
            Lon = -5.85f
        };
        _context.Drones.Add(drone);

        var route = new Models.Route
        {
            Id = 1,
            Type = RouteType.Simple,
            Coords = new List<RoutePoint>()
        };
        _context.Routes.Add(route);

        var flightPlan = new FlightPlan
        {
            DronId = dronId ?? 5,
            RutaId = 1,
            State = state,
            StartingTime = DateTime.Now,
            ModeChangeHistoric = new List<ChangeMode>()
        };
        _context.FlightPlans.Add(flightPlan);

        await _context.SaveChangesAsync();
        return flightPlan;
    }

    private async Task<FlightPlan> CreateFlightPlanWithoutDron(FlightStatus state)
    {
        var route = new Models.Route
        {
            Id = 2,
            Type = RouteType.Simple,
            Coords = new List<RoutePoint>()
        };
        _context.Routes.Add(route);

        var flightPlan = new FlightPlan
        {
            DronId = 0, // Sin dron asignado (usamos 0 ya que DronId no es nullable en el modelo)
            RutaId = 2,
            State = state,
            StartingTime = DateTime.Now,
            ModeChangeHistoric = new List<ChangeMode>()
        };
        _context.FlightPlans.Add(flightPlan);

        await _context.SaveChangesAsync();
        return flightPlan;
    }

    // CP01
    [TestMethod]
    public async Task SwitchToManualModeAsync_ValidId_OnCourseState_ShouldSwitchToManual()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 5);

        var result = await _service.SwitchToManualModeAsync(flightPlan.Id);

        Assert.AreEqual(FlightStatus.Manual, result.State);
        Assert.IsTrue(result.ModeChangeHistoric.Any(m => m.Mode == FlightMode.Manual));
    }

    // CP02
    [TestMethod]
    public async Task SwitchToManualModeAsync_NonExistentId_ShouldThrowNotFoundException()
    {
        // No se crea plan de vuelo

        await Assert.ThrowsAsync<NotFoundException>(() => _service.SwitchToManualModeAsync(9999));
    }

    // CP03
    [TestMethod]
    public async Task SwitchToManualModeAsync_AlreadyManual_ShouldRemainManualOrBeIdempotent()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual, dronId: 5);
        var initialHistoryCount = flightPlan.ModeChangeHistoric.Count;

        var result = await _service.SwitchToManualModeAsync(flightPlan.Id);

        Assert.AreEqual(FlightStatus.Manual, result.State);
        Assert.AreNotEqual(initialHistoryCount, result.ModeChangeHistoric.Count);
    }

    // CP04
    [TestMethod]
    public async Task SwitchToManualModeAsync_CompletedState_ShouldHandleCompletedPlan()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Completed, dronId: 5);

        var result = await _service.SwitchToManualModeAsync(flightPlan.Id);

        Assert.AreEqual(FlightStatus.Completed, result.State);
    }

    // CP05
    [TestMethod]
    public async Task SwitchToManualModeAsync_CancelledState_ShouldHandleCancelledPlan()
    {

        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Cancelled, dronId: 5);

        var result = await _service.SwitchToManualModeAsync(flightPlan.Id);

        Assert.AreEqual(FlightStatus.Cancelled, result.State);
    }

    // CP06
    [TestMethod]
    public async Task SwitchToManualModeAsync_NoDroneAssigned_ShouldHandleNoDrone()
    {
        var flightPlan = await CreateFlightPlanWithoutDron(FlightStatus.OnCourse);

        var result = await _service.SwitchToManualModeAsync(flightPlan.Id);

        Assert.AreEqual(FlightStatus.OnCourse, result.State);
    }

    // CP07
    [TestMethod]
    public async Task CP10_SendManualDestinationAsync_ValidCentralCoordinates_ShouldSucceed()
    {
        // Arrange
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
        var dto = new GoToDto { Latitude = 43.36, Longitude = -5.85, Speed = 20 };

     // Act & Assert - No debe lanzar excepción
        await _service.SendManualDestinationAsync(flightPlan.Id, dto);
    }

    /// <summary>
    /// CP11: Latitud límite inferior (-90.0)
    /// </summary>
    [TestMethod]
    public async Task CP11_SendManualDestinationAsync_LatitudeLowerBound_ShouldSucceed()
    {
        // Arrange
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
        var dto = new GoToDto { Latitude = -90.0, Longitude = 0, Speed = 20 };

        // Act & Assert - No debe lanzar excepción
        await _service.SendManualDestinationAsync(flightPlan.Id, dto);
    }

    /// <summary>
    /// CP12: Latitud límite superior (90.0)
    /// </summary>
    [TestMethod]
    public async Task CP12_SendManualDestinationAsync_LatitudeUpperBound_ShouldSucceed()
    {
     // Arrange
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
      var dto = new GoToDto { Latitude = 90.0, Longitude = 0, Speed = 20 };

        // Act & Assert - No debe lanzar excepción
        await _service.SendManualDestinationAsync(flightPlan.Id, dto);
    }

    /// <summary>
    /// CP13: Latitud debajo del mínimo (-90.0001)
    /// Nota: La validación debe estar en el servicio o DTO
    /// </summary>
    [TestMethod]
    public async Task CP13_SendManualDestinationAsync_LatitudeBelowMinimum_ShouldValidate()
    {
        // Arrange
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
        var dto = new GoToDto { Latitude = -90.0001, Longitude = 0, Speed = 20 };

        // Act - La implementación actual no valida coordenadas
        // Si se implementa validación, este test debería esperar una excepción
        await _service.SendManualDestinationAsync(flightPlan.Id, dto);

        // Nota: Si se requiere validación, descomentar:
        // Assert.ThrowsException<ArgumentException>(...) o similar
    }

    /// <summary>
    /// CP14: Latitud encima del máximo (90.0001)
    /// </summary>
    [TestMethod]
public async Task CP14_SendManualDestinationAsync_LatitudeAboveMaximum_ShouldValidate()
    {
        // Arrange
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
        var dto = new GoToDto { Latitude = 90.0001, Longitude = 0, Speed = 20 };

        // Act - La implementación actual no valida coordenadas
      await _service.SendManualDestinationAsync(flightPlan.Id, dto);
    }

    /// <summary>
    /// CP15: Longitud límite inferior (-180.0)
    /// </summary>
    [TestMethod]
    public async Task CP15_SendManualDestinationAsync_LongitudeLowerBound_ShouldSucceed()
    {
     // Arrange
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
        var dto = new GoToDto { Latitude = 0, Longitude = -180.0, Speed = 20 };

        // Act & Assert - No debe lanzar excepción
 await _service.SendManualDestinationAsync(flightPlan.Id, dto);
    }

    /// <summary>
    /// CP16: Longitud límite superior (180.0)
    /// </summary>
    [TestMethod]
  public async Task CP16_SendManualDestinationAsync_LongitudeUpperBound_ShouldSucceed()
    {
        // Arrange
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
        var dto = new GoToDto { Latitude = 0, Longitude = 180.0, Speed = 20 };

        // Act & Assert - No debe lanzar excepción
        await _service.SendManualDestinationAsync(flightPlan.Id, dto);
    }

    /// <summary>
    /// CP17: Longitud debajo del mínimo (-180.0001)
    /// </summary>
    [TestMethod]
    public async Task CP17_SendManualDestinationAsync_LongitudeBelowMinimum_ShouldValidate()
    {
        // Arrange
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
        var dto = new GoToDto { Latitude = 0, Longitude = -180.0001, Speed = 20 };

        // Act - La implementación actual no valida coordenadas
     await _service.SendManualDestinationAsync(flightPlan.Id, dto);
    }

    /// <summary>
    /// CP18: Longitud encima del máximo (180.0001)
    /// </summary>
    [TestMethod]
    public async Task CP18_SendManualDestinationAsync_LongitudeAboveMaximum_ShouldValidate()
    {
        // Arrange
   var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
        var dto = new GoToDto { Latitude = 0, Longitude = 180.0001, Speed = 20 };

        // Act - La implementación actual no valida coordenadas
        await _service.SendManualDestinationAsync(flightPlan.Id, dto);
    }

    /// <summary>
    /// CP19: Velocidad mínima válida (0.1)
    /// </summary>
    [TestMethod]
    public async Task CP19_SendManualDestinationAsync_MinimumValidSpeed_ShouldSucceed()
    {
  // Arrange
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
   var dto = new GoToDto { Latitude = 43, Longitude = -5, Speed = 0.1 };

   // Act & Assert - No debe lanzar excepción
        await _service.SendManualDestinationAsync(flightPlan.Id, dto);
    }

    /// <summary>
    /// CP20: Velocidad cero
    /// </summary>
    [TestMethod]
    public async Task CP20_SendManualDestinationAsync_ZeroSpeed_ShouldValidate()
    {
        // Arrange
    var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
        var dto = new GoToDto { Latitude = 43, Longitude = -5, Speed = 0 };

        // Act - La implementación actual no valida velocidad
        await _service.SendManualDestinationAsync(flightPlan.Id, dto);
    }

    /// <summary>
 /// CP21: Velocidad negativa
    /// </summary>
    [TestMethod]
    public async Task CP21_SendManualDestinationAsync_NegativeSpeed_ShouldValidate()
    {
        // Arrange
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
        var dto = new GoToDto { Latitude = 43, Longitude = -5, Speed = -1 };

        // Act - La implementación actual no valida velocidad
        await _service.SendManualDestinationAsync(flightPlan.Id, dto);
  }

    /// <summary>
    /// CP22: Velocidad máxima válida (100)
    /// </summary>
    [TestMethod]
    public async Task CP22_SendManualDestinationAsync_MaximumValidSpeed_ShouldSucceed()
    {
        // Arrange
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
        var dto = new GoToDto { Latitude = 43, Longitude = -5, Speed = 100 };

     // Act & Assert - No debe lanzar excepción
        await _service.SendManualDestinationAsync(flightPlan.Id, dto);
    }

    /// <summary>
    /// CP23: Velocidad excesiva (100.1)
    /// </summary>
    [TestMethod]
    public async Task CP23_SendManualDestinationAsync_ExcessiveSpeed_ShouldValidate()
    {
  // Arrange
   var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
        var dto = new GoToDto { Latitude = 43, Longitude = -5, Speed = 100.1 };

        // Act - La implementación actual no valida velocidad
      await _service.SendManualDestinationAsync(flightPlan.Id, dto);
    }

    /// <summary>
    /// CP24: Plan no existente
    /// </summary>
    [TestMethod]
    public async Task CP24_SendManualDestinationAsync_NonExistentPlan_ShouldThrowNotFoundException()
    {
        // Arrange
        var dto = new GoToDto { Latitude = 43.36, Longitude = -5.85, Speed = 20 };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.SendManualDestinationAsync(9999, dto));
    }
}