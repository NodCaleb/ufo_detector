using Moq;
using UfoDetector.Models;
using UfoDetector.Models.Enums;
using UfoDetector.Services;

namespace UfoDetector.Tests.Services;

public class SensorTickServiceTests
{
    private static SensorTickService CreateService(out Mock<IDispatcherTimer> timerMock)
    {
        timerMock = new Mock<IDispatcherTimer>();
        var evalMock = new Mock<IAnomalyEvaluator>();
        evalMock.Setup(e => e.Evaluate(It.IsAny<DetectorState>())).Returns((Anomaly?)null);
        var orchMock = new Mock<ITransitionOrchestrator>();
        return new SensorTickService(timerMock.Object, evalMock.Object, orchMock.Object);
    }

    [Fact]
    public void Timer_Interval_Is_100ms()
    {
        TimeSpan captured = TimeSpan.Zero;
        var timerMock = new Mock<IDispatcherTimer>();
        timerMock.SetupSet(t => t.Interval = It.IsAny<TimeSpan>())
                 .Callback<TimeSpan>(ts => captured = ts);

        var evalMock = new Mock<IAnomalyEvaluator>();
        evalMock.Setup(e => e.Evaluate(It.IsAny<DetectorState>())).Returns((Anomaly?)null);
        _ = new SensorTickService(timerMock.Object, evalMock.Object, new Mock<ITransitionOrchestrator>().Object);

        Assert.Equal(SensorTickService.TickInterval, captured);
        Assert.Equal(TimeSpan.FromMilliseconds(100), SensorTickService.TickInterval);
    }

    [Fact]
    public void Initial_Values_Equal_Baselines()
    {
        var service = CreateService(out _);

        Assert.Equal(SensorBaseline.Neutron.BaselineValue,     service.NeutronValue);
        Assert.Equal(SensorBaseline.Ionisation.BaselineValue,  service.IonisationValue);
        Assert.Equal(SensorBaseline.Geomagnetic.BaselineValue, service.GeomagneticValue);
        Assert.Equal(SensorBaseline.Thermal.BaselineValue,     service.ThermalValue);
        Assert.Equal(SensorBaseline.Chrono.BaselineValue,      service.ChronoValue);
    }

    [Fact]
    public void Initial_Statuses_Are_Normal()
    {
        var service = CreateService(out _);

        Assert.Equal(SensorStatus.Normal, service.NeutronStatus);
        Assert.Equal(SensorStatus.Normal, service.IonisationStatus);
        Assert.Equal(SensorStatus.Normal, service.GeomagneticStatus);
        Assert.Equal(SensorStatus.Normal, service.ThermalStatus);
        Assert.Equal(SensorStatus.Normal, service.ChronoStatus);
    }

    [Fact]
    public void After_Many_Ticks_Neutron_Stays_Within_5Percent_Of_Baseline()
    {
        var service = CreateService(out var timerMock);
        double baseline = SensorBaseline.Neutron.BaselineValue;

        for (int i = 0; i < 500; i++)
            timerMock.Raise(t => t.Tick += null, EventArgs.Empty);

        Assert.InRange(service.NeutronValue, baseline * 0.95, baseline * 1.05);
    }

    [Fact]
    public void After_Many_Ticks_All_Sensors_Stay_Within_5Percent_Of_Baseline()
    {
        var service = CreateService(out var timerMock);

        for (int i = 0; i < 500; i++)
            timerMock.Raise(t => t.Tick += null, EventArgs.Empty);

        double n  = SensorBaseline.Neutron.BaselineValue;
        double io = SensorBaseline.Ionisation.BaselineValue;
        double gm = SensorBaseline.Geomagnetic.BaselineValue;
        double ch = SensorBaseline.Chrono.BaselineValue;

        Assert.InRange(service.NeutronValue,     n  * 0.95, n  * 1.05);
        Assert.InRange(service.IonisationValue,  io * 0.95, io * 1.05);
        Assert.InRange(service.GeomagneticValue, gm * 0.95, gm * 1.05);
        Assert.InRange(service.ChronoValue,      ch * 0.95, ch * 1.05);

        // Thermal baseline is ~0; verify it stays near zero
        Assert.InRange(service.ThermalValue, -0.02, 0.02);
    }

    [Fact]
    public void Ticked_Event_Fires_On_Each_Timer_Tick()
    {
        var service = CreateService(out var timerMock);
        int firedCount = 0;
        service.Ticked += (_, _) => firedCount++;

        for (int i = 0; i < 5; i++)
            timerMock.Raise(t => t.Tick += null, EventArgs.Empty);

        Assert.Equal(5, firedCount);
    }
}
