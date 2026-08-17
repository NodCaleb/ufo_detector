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
        out Mock<ITransitionOrchestrator> orchMock)
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

        orchMock = new Mock<ITransitionOrchestrator>();
        return new DetectorViewModel(tickMock.Object, orchMock.Object);
    }

    [Fact]
    public void Initial_SensorValues_Match_Baselines()
    {
        var vm = CreateViewModel(out _, out _);

        Assert.Equal(SensorBaseline.Neutron.BaselineValue,     vm.NeutronValue);
        Assert.Equal(SensorBaseline.Ionisation.BaselineValue,  vm.IonisationValue);
        Assert.Equal(SensorBaseline.Geomagnetic.BaselineValue, vm.GeomagneticValue);
        Assert.Equal(SensorBaseline.Thermal.BaselineValue,     vm.ThermalValue);
        Assert.Equal(SensorBaseline.Chrono.BaselineValue,      vm.ChronoValue);
    }

    [Fact]
    public void Initial_Statuses_Are_All_Normal()
    {
        var vm = CreateViewModel(out _, out _);

        Assert.Equal(SensorStatus.Normal, vm.NeutronStatus);
        Assert.Equal(SensorStatus.Normal, vm.IonisationStatus);
        Assert.Equal(SensorStatus.Normal, vm.GeomagneticStatus);
        Assert.Equal(SensorStatus.Normal, vm.ThermalStatus);
        Assert.Equal(SensorStatus.Normal, vm.ChronoStatus);
    }

    [Fact]
    public void InfrasoundBands_Initialised_To_20_Elements()
    {
        var vm = CreateViewModel(out _, out _);

        Assert.NotNull(vm.InfrasoundBands);
        Assert.Equal(20, vm.InfrasoundBands.Length);
    }

    [Fact]
    public void When_Ticked_Values_Refresh_From_Service()
    {
        var vm = CreateViewModel(out var tickMock, out _);

        const double newNeutron = 4.5;
        tickMock.Setup(t => t.NeutronValue).Returns(newNeutron);
        tickMock.Setup(t => t.NeutronStatus).Returns(SensorStatus.Normal);

        // Fire the Ticked event
        tickMock.Raise(t => t.Ticked += null, EventArgs.Empty);

        Assert.Equal(newNeutron, vm.NeutronValue);
    }
}
