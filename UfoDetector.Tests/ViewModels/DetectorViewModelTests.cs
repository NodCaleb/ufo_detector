using Microsoft.Maui.Storage;
using Moq;
using UfoDetector.Models;
using UfoDetector.Models.Enums;
using UfoDetector.Services;
using UfoDetector.ViewModels;

namespace UfoDetector.Tests.ViewModels;

public class DetectorViewModelTests
{
    private static DetectorViewModel CreateViewModel(
        out Mock<ISensorTickService> tickMock,
        out Mock<ITransitionOrchestrator> orchMock,
        out Mock<IAnomalyEvaluator> evalMock,
        out Mock<IPreferences> prefsMock)
    {
        tickMock = new Mock<ISensorTickService>();
        // Seed the mock with baseline values so the VM reads them at init
        tickMock.Setup(t => t.NeutronValue).Returns(SensorBaseline.Neutron.BaselineValue);
        tickMock.Setup(t => t.IonisationValue).Returns(SensorBaseline.Ionisation.BaselineValue);
        tickMock.Setup(t => t.GeomagneticValue).Returns(SensorBaseline.Geomagnetic.BaselineValue);
        tickMock.Setup(t => t.ThermalValue).Returns(SensorBaseline.Thermal.BaselineValue);
        tickMock.Setup(t => t.ChronoValue).Returns(SensorBaseline.Chrono.BaselineValue);
        tickMock.Setup(t => t.NeutronStatus).Returns(SensorStatus.Normal);
        tickMock.Setup(t => t.IonisationStatus).Returns(SensorStatus.Normal);
        tickMock.Setup(t => t.GeomagneticStatus).Returns(SensorStatus.Normal);
        tickMock.Setup(t => t.ThermalStatus).Returns(SensorStatus.Normal);
        tickMock.Setup(t => t.ChronoStatus).Returns(SensorStatus.Normal);
        tickMock.Setup(t => t.InfrasoundBands).Returns(new double[20]);

        orchMock  = new Mock<ITransitionOrchestrator>();
        evalMock  = new Mock<IAnomalyEvaluator>();
        prefsMock = new Mock<IPreferences>();
        // Return the defaultValue argument so Get<T> behaves like "no saved value"
        prefsMock
            .Setup(p => p.Get(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns<string, int, string?>((_, def, _) => def);

        return new DetectorViewModel(tickMock.Object, orchMock.Object, evalMock.Object, prefsMock.Object);
    }

    // ── Existing Phase 3 tests ────────────────────────────────────────────────

    [Fact]
    public void Initial_SensorValues_Match_Baselines()
    {
        var vm = CreateViewModel(out _, out _, out _, out _);

        Assert.Equal(SensorBaseline.Neutron.BaselineValue,     vm.NeutronValue);
        Assert.Equal(SensorBaseline.Ionisation.BaselineValue,  vm.IonisationValue);
        Assert.Equal(SensorBaseline.Geomagnetic.BaselineValue, vm.GeomagneticValue);
        Assert.Equal(SensorBaseline.Thermal.BaselineValue,     vm.ThermalValue);
        Assert.Equal(SensorBaseline.Chrono.BaselineValue,      vm.ChronoValue);
    }

    [Fact]
    public void Initial_Statuses_Are_All_Normal()
    {
        var vm = CreateViewModel(out _, out _, out _, out _);

        Assert.Equal(SensorStatus.Normal, vm.NeutronStatus);
        Assert.Equal(SensorStatus.Normal, vm.IonisationStatus);
        Assert.Equal(SensorStatus.Normal, vm.GeomagneticStatus);
        Assert.Equal(SensorStatus.Normal, vm.ThermalStatus);
        Assert.Equal(SensorStatus.Normal, vm.ChronoStatus);
    }

    [Fact]
    public void InfrasoundBands_Initialised_To_20_Elements()
    {
        var vm = CreateViewModel(out _, out _, out _, out _);

        Assert.NotNull(vm.InfrasoundBands);
        Assert.Equal(20, vm.InfrasoundBands.Length);
    }

    [Fact]
    public void When_Ticked_Values_Refresh_From_Service()
    {
        var vm = CreateViewModel(out var tickMock, out _, out _, out _);

        const double newNeutron = 4.5;
        tickMock.Setup(t => t.NeutronValue).Returns(newNeutron);
        tickMock.Setup(t => t.NeutronStatus).Returns(SensorStatus.Normal);

        // Fire the Ticked event
        tickMock.Raise(t => t.Ticked += null, EventArgs.Empty);

        Assert.Equal(newNeutron, vm.NeutronValue);
    }

    // ── T035: Phase 4 — Mode, Sensitivity, NoiseSuppression ──────────────────

    [Fact]
    public void Mode_Default_Is_Passive()
    {
        var vm = CreateViewModel(out _, out _, out _, out _);
        Assert.Equal(DetectorMode.Passive, vm.Mode);
    }

    [Fact]
    public void ToggleModeCommand_Switches_Passive_To_Active()
    {
        var vm = CreateViewModel(out _, out _, out _, out _);
        vm.ToggleModeCommand.Execute(null);
        Assert.Equal(DetectorMode.Active, vm.Mode);
    }

    [Fact]
    public void ToggleModeCommand_Switches_Active_Back_To_Passive()
    {
        var vm = CreateViewModel(out _, out _, out _, out _);
        vm.ToggleModeCommand.Execute(null); // → Active
        vm.ToggleModeCommand.Execute(null); // → Passive
        Assert.Equal(DetectorMode.Passive, vm.Mode);
    }

    [Fact]
    public void Sensitivity_Default_Is_50()
    {
        var vm = CreateViewModel(out _, out _, out _, out _);
        Assert.Equal(50, vm.Sensitivity);
    }

    [Fact]
    public void NoiseSuppression_Default_Is_50()
    {
        var vm = CreateViewModel(out _, out _, out _, out _);
        Assert.Equal(50, vm.NoiseSuppression);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Sensitivity_Accepts_Boundary_Values(int value)
    {
        var vm = CreateViewModel(out _, out _, out _, out _);
        vm.Sensitivity = value;
        Assert.Equal(value, vm.Sensitivity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void NoiseSuppression_Accepts_Boundary_Values(int value)
    {
        var vm = CreateViewModel(out _, out _, out _, out _);
        vm.NoiseSuppression = value;
        Assert.Equal(value, vm.NoiseSuppression);
    }

    [Fact]
    public void Sensitivity_Set_Saves_To_Preferences()
    {
        var vm = CreateViewModel(out _, out _, out _, out var prefsMock);
        vm.Sensitivity = 75;
        prefsMock.Verify(p => p.Set("Sensitivity", 75, null), Times.Once);
    }

    [Fact]
    public void NoiseSuppression_Set_Saves_To_Preferences()
    {
        var vm = CreateViewModel(out _, out _, out _, out var prefsMock);
        vm.NoiseSuppression = 15;
        prefsMock.Verify(p => p.Set("NoiseSuppression", 15, null), Times.Once);
    }

    [Fact]
    public void Constructor_Restores_Sensitivity_From_Preferences()
    {
        var prefsMock = new Mock<IPreferences>();
        prefsMock.Setup(p => p.Get("Sensitivity",      50, null)).Returns(75);
        prefsMock.Setup(p => p.Get("NoiseSuppression", 50, null)).Returns(50);

        var tickMock = new Mock<ISensorTickService>();
        tickMock.Setup(t => t.InfrasoundBands).Returns(new double[20]);

        var vm = new DetectorViewModel(
            tickMock.Object,
            new Mock<ITransitionOrchestrator>().Object,
            new Mock<IAnomalyEvaluator>().Object,
            prefsMock.Object);

        Assert.Equal(75, vm.Sensitivity);
    }

    [Fact]
    public void Constructor_Restores_NoiseSuppression_From_Preferences()
    {
        var prefsMock = new Mock<IPreferences>();
        prefsMock.Setup(p => p.Get("Sensitivity",      50, null)).Returns(50);
        prefsMock.Setup(p => p.Get("NoiseSuppression", 50, null)).Returns(15);

        var tickMock = new Mock<ISensorTickService>();
        tickMock.Setup(t => t.InfrasoundBands).Returns(new double[20]);

        var vm = new DetectorViewModel(
            tickMock.Object,
            new Mock<ITransitionOrchestrator>().Object,
            new Mock<IAnomalyEvaluator>().Object,
            prefsMock.Object);

        Assert.Equal(15, vm.NoiseSuppression);
    }
}
