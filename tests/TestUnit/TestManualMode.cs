using CentralBackend.Exceptions;
using CentralBackend.Services;
using ControlBackend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
            State = Models.DroneState.Flying,
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
            DronId = 0,
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
        await Assert.ThrowsAsync<NotFoundException>(() => _service.SwitchToManualModeAsync(9999));
    }

    // CP03
    [TestMethod]
    public async Task SwitchToManualModeAsync_AlreadyManual_ShouldThrowException()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Manual, dronId: 5);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SwitchToManualModeAsync(1));
    }

    // CP04
    [TestMethod]
    public async Task SwitchToManualModeAsync_CompletedState_ShouldThrowException()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Completed, dronId: 5);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SwitchToManualModeAsync(1));
    }

    // CP05
    [TestMethod]
    public async Task SwitchToManualModeAsync_CancelledState_ShouldSwitchToManual()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.Cancelled, dronId: 5);

        var result = await _service.SwitchToManualModeAsync(flightPlan.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(FlightStatus.Manual, result.State);
    }

    // CP06
    [TestMethod]
    public async Task SwitchToManualModeAsync_NoDroneAssigned_ShouldHandleNoDrone()
    {
        var flightPlan = await CreateFlightPlanWithoutDron(FlightStatus.OnCourse);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SwitchToManualModeAsync(flightPlan.Id));
    }

    // CP07
    [TestMethod]
    public async Task SendManualDestinationAsync_ValidCentralCoordinates_ShouldSendHttpRequest()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 11);
        var dto = new GoToDto { Latitude = 43.36, Longitude = 5.85, Speed = 20 };

        await _service.SendManualDestinationAsync(flightPlan.Id, dto);

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri != null &&
                req.RequestUri.ToString().Contains("/api/drone/11/goto")),
            ItExpr.IsAny<CancellationToken>());
    }

    // CP08
    [TestMethod]
    public async Task SendManualDestinationAsync_LatitudeLowerBound_ShouldSucceed()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 8);
        var dto = new GoToDto { Latitude = -90.0, Longitude = 0, Speed = 20 };

        await _service.SendManualDestinationAsync(flightPlan.Id, dto);

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri != null &&
                req.RequestUri.ToString().Contains("/api/drone/8/goto")),
            ItExpr.IsAny<CancellationToken>());
    }

    // CP09
    [TestMethod]
    public async Task SendManualDestinationAsync_LatitudeUpperBound_ShouldSucceed()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 9);
        var dto = new GoToDto { Latitude = 90.0, Longitude = 0, Speed = 20 };

        await _service.SendManualDestinationAsync(flightPlan.Id, dto);

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri != null &&
                req.RequestUri.ToString().Contains("/api/drone/9/goto")),
            ItExpr.IsAny<CancellationToken>());
    }

    // CP10
    [TestMethod]
    public async Task SendManualDestinationAsync_LatitudeBelowMinimum_ShouldThrowException()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 10);
        var dto = new GoToDto { Latitude = -90.0001, Longitude = 0, Speed = 20 };

        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.SendManualDestinationAsync(flightPlan.Id, dto));
    }

    // CP11
    [TestMethod]
    public async Task SendManualDestinationAsync_LatitudeAboveMaximum_ShouldThrowException()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 14);
        var dto = new GoToDto { Latitude = 90.0001, Longitude = 0, Speed = 20 };

        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.SendManualDestinationAsync(flightPlan.Id, dto));
    }

    // CP12
    [TestMethod]
    public async Task SendManualDestinationAsync_LongitudeLowerBound_ShouldSucceed()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 15);
        var dto = new GoToDto { Latitude = 0, Longitude = -180.0, Speed = 20 };

        await _service.SendManualDestinationAsync(flightPlan.Id, dto);

      _httpMessageHandlerMock.Protected().Verify(
        "SendAsync",
        Times.Once(),
        ItExpr.Is<HttpRequestMessage>(req =>
            req.Method == HttpMethod.Post &&
            req.RequestUri != null &&
            req.RequestUri.ToString().Contains("/api/drone/15/goto")),
        ItExpr.IsAny<CancellationToken>());
    }

    // CP13
    [TestMethod]
    public async Task SendManualDestinationAsync_LongitudeUpperBound_ShouldSucceed()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 16);
        var dto = new GoToDto { Latitude = 0, Longitude = 180.0, Speed = 20 };

        await _service.SendManualDestinationAsync(flightPlan.Id, dto);

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri != null &&
                req.RequestUri.ToString().Contains("/api/drone/16/goto")),
            ItExpr.IsAny<CancellationToken>());
    }

    // CP14
    [TestMethod]
    public async Task SendManualDestinationAsync_LongitudeBelowMinimum_ShouldThrowException()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 17);
        var dto = new GoToDto { Latitude = 0, Longitude = -180.0001, Speed = 20 };

        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.SendManualDestinationAsync(flightPlan.Id, dto));
    }

    // CP15
    [TestMethod]
    public async Task SendManualDestinationAsync_LongitudeAboveMaximum_ShouldThrowException()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 18);
        var dto = new GoToDto { Latitude = 0, Longitude = 180.0001, Speed = 20 };

        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.SendManualDestinationAsync(flightPlan.Id, dto));
    }

    // CP16
    [TestMethod]
    public async Task SendManualDestinationAsync_MinimumValidSpeed_ShouldSucceed()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 19);
        var dto = new GoToDto { Latitude = 43, Longitude = -5, Speed = 0.1 };

        await _service.SendManualDestinationAsync(flightPlan.Id, dto);

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri != null &&
                req.RequestUri.ToString().Contains("/api/drone/19/goto")),
            ItExpr.IsAny<CancellationToken>());
    }

    // CP17
    [TestMethod]
    public async Task SendManualDestinationAsync_ZeroSpeed_ShouldThrowException()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 20);
        var dto = new GoToDto { Latitude = 43, Longitude = -5, Speed = 0 };

        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.SendManualDestinationAsync(flightPlan.Id, dto));
    }

    // CP18
    [TestMethod]
    public async Task SendManualDestinationAsync_NegativeSpeed_ShouldThrowException()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 21);
        var dto = new GoToDto { Latitude = 43, Longitude = -5, Speed = -1 };

        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.SendManualDestinationAsync(flightPlan.Id, dto));
    }

    // CP19
    [TestMethod]
    public async Task SendManualDestinationAsync_MaximumValidSpeed_ShouldSucceed()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 22);
        var dto = new GoToDto { Latitude = 43, Longitude = -5, Speed = 100 };

        await _service.SendManualDestinationAsync(flightPlan.Id, dto);

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri != null &&
                req.RequestUri.ToString().Contains("/api/drone/22/goto")),
            ItExpr.IsAny<CancellationToken>());
    }

    // CP20
    [TestMethod]
    public async Task SendManualDestinationAsync_SlightHighSpeed_ShouldThrowException()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 23);
        var dto = new GoToDto { Latitude = 43, Longitude = -5, Speed = 100.01 };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SendManualDestinationAsync(flightPlan.Id, dto));
    }

    // CP21
    [TestMethod]
    public async Task SendManualDestinationAsync_VeryHighSpeed_ShouldThrowException()
    {
        var flightPlan = await CreateFlightPlanWithState(FlightStatus.OnCourse, dronId: 23);
        var dto = new GoToDto { Latitude = 43, Longitude = -5, Speed = 1000 };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SendManualDestinationAsync(flightPlan.Id, dto));
    }
}