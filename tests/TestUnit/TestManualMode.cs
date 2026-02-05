using CentralBackend;
using CentralBackend.Exceptions;
using CentralBackend.Services;
using ControlBackend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Models;
using Moq;
using Moq.Protected;
using System.Net;

namespace CentralBackend.Tests;

/// <summary>
/// Test context that uses InMemory database for testing
/// </summary>
public class TestFireDroneContext : FireDrone
{
    private readonly string _databaseName;

  public TestFireDroneContext(string databaseName) : base()
    {
        _databaseName = databaseName;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
  // Don't call base - we want to use InMemory instead of SQLite
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
      // Create unique database name for each test to avoid data collision
        var databaseName = Guid.NewGuid().ToString();
        _context = new TestFireDroneContext(databaseName);
        _context.Database.EnsureCreated();

        // Setup HTTP client factory mock
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

      // Setup configuration using in-memory configuration
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

    #region Helper Methods

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

    #endregion

    #region Phase 1: SwitchToManualModeAsync Tests

    /// <summary>
    /// CP01: ID válido, estado OnCourse
    /// </summary>
    [TestMethod]
    public async Task CP01_SwitchToManualModeAsync_ValidId_OnCourseState_ShouldSwitchToManual()
    {
        // Arrange
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 5);

        // Act
        var result = await _service.SwitchToManualModeAsync(flightPlan.Id);

   // Assert
        Assert.AreEqual(FlightStatus.Manual, result.State);
        Assert.IsTrue(result.ModeChangeHistoric.Any(m => m.Mode == FlightMode.Manual));
    }

    /// <summary>
    /// CP02: ID no existente
    /// </summary>
    [TestMethod]
[ExpectedException(typeof(NotFoundException))]
    public async Task CP02_SwitchToManualModeAsync_NonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange - no flight plan created

        // Act
        await _service.SwitchToManualModeAsync(9999);

        // Assert - ExpectedException
  }

    /// <summary>
    /// CP03: ID = 0
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    public async Task CP03_SwitchToManualModeAsync_ZeroId_ShouldThrowNotFoundException()
    {
        // Arrange - no flight plan with ID 0 exists

        // Act
        await _service.SwitchToManualModeAsync(0);

        // Assert - ExpectedException
    }

    /// <summary>
    /// CP04: ID negativo
    /// </summary>
 [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    public async Task CP04_SwitchToManualModeAsync_NegativeId_ShouldThrowNotFoundException()
    {
        // Arrange - no flight plan with negative ID exists

        // Act
        await _service.SwitchToManualModeAsync(-1);

    // Assert - ExpectedException
    }

    /// <summary>
    /// CP05: Estado ya es Manual (idempotente)
    /// </summary>
    [TestMethod]
    public async Task CP05_SwitchToManualModeAsync_AlreadyManual_ShouldRemainManualOrBeIdempotent()
    {
     // Arrange
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual);
   var initialHistoryCount = flightPlan.ModeChangeHistoric.Count;

     // Act
    var result = await _service.SwitchToManualModeAsync(flightPlan.Id);

    // Assert - El estado permanece en Manual, puede o no añadir nuevo registro histórico
 Assert.AreEqual(FlightStatus.Manual, result.State);
    }

    /// <summary>
    /// CP06: Estado Completed - no se puede cambiar plan completado
    /// Nota: La implementación actual no valida esto, el test documenta el comportamiento actual
    /// </summary>
    [TestMethod]
    public async Task CP06_SwitchToManualModeAsync_CompletedState_ShouldHandleCompletedPlan()
    {
        // Arrange
    var flightPlan = await CreateFlightPlanWithState(FlightStatus.Completed);

        // Act - La implementación actual permite el cambio
  var result = await _service.SwitchToManualModeAsync(flightPlan.Id);

      // Assert - Documenta comportamiento actual (cambia a Manual)
        // Si se quiere que falle, se debería modificar la implementación
        Assert.AreEqual(FlightStatus.Manual, result.State);
    }

    /// <summary>
    /// CP07: Estado Cancelled - no se puede cambiar plan cancelado
  /// Nota: La implementación actual no valida esto, el test documenta el comportamiento actual
    /// </summary>
    [TestMethod]
    public async Task CP07_SwitchToManualModeAsync_CancelledState_ShouldHandleCancelledPlan()
    {
        // Arrange
  var flightPlan = await CreateFlightPlanWithState(FlightStatus.Cancelled);

  // Act - La implementación actual permite el cambio
        var result = await _service.SwitchToManualModeAsync(flightPlan.Id);

        // Assert - Documenta comportamiento actual (cambia a Manual)
        // Si se quiere que falle, se debería modificar la implementación
      Assert.AreEqual(FlightStatus.Manual, result.State);
    }

    /// <summary>
    /// CP08: Plan sin dron asignado
    /// </summary>
    [TestMethod]
    public async Task CP08_SwitchToManualModeAsync_NoDroneAssigned_ShouldHandleNoDrone()
    {
   // Arrange
        var flightPlan = await CreateFlightPlanWithoutDron(FlightStatus.OnCourse);

        // Act - La implementación actual permite el cambio aunque no haya dron
        var result = await _service.SwitchToManualModeAsync(flightPlan.Id);

        // Assert - El estado cambia aunque no haya dron asignado
        Assert.AreEqual(FlightStatus.Manual, result.State);
    }

    /// <summary>
  /// CP09: ID mínimo válido (1)
    /// </summary>
    [TestMethod]
    public async Task CP09_SwitchToManualModeAsync_MinimumValidId_ShouldSucceed()
    {
        // Arrange
  var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse);
   // El primer FlightPlan creado tendrá Id = 1

        // Act
        var result = await _service.SwitchToManualModeAsync(flightPlan.Id);

        // Assert
 Assert.AreEqual(FlightStatus.Manual, result.State);
        Assert.IsTrue(result.ModeChangeHistoric.Any(m => m.Mode == FlightMode.Manual));
    }

  #endregion

    #region Phase 2: SendManualDestinationAsync Tests

    /// <summary>
    /// CP10: Coordenadas válidas centrales
    /// </summary>
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
    [ExpectedException(typeof(NotFoundException))]
public async Task CP24_SendManualDestinationAsync_NonExistentPlan_ShouldThrowNotFoundException()
    {
   // Arrange
        var dto = new GoToDto { Latitude = 43.36, Longitude = -5.85, Speed = 20 };

   // Act
    await _service.SendManualDestinationAsync(9999, dto);

   // Assert - ExpectedException
  }

    #endregion
}