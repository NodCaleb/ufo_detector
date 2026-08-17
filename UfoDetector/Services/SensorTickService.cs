using UfoDetector.Models;
using UfoDetector.Models.Enums;

namespace UfoDetector.Services;

public class SensorTickService : ISensorTickService
{
    private readonly IDispatcherTimer _timer;
    private readonly Random _rng = new();

    private double _neutronValue    = SensorBaseline.Neutron.BaselineValue;
    private double _ionisationValue = SensorBaseline.Ionisation.BaselineValue;
    private double _geomagneticValue = SensorBaseline.Geomagnetic.BaselineValue;
    private double _thermalValue    = SensorBaseline.Thermal.BaselineValue;
    private double _chronoValue     = SensorBaseline.Chrono.BaselineValue;
    private readonly double[] _infrasoundBands = new double[20];

    public static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(100);

    public event EventHandler? Ticked;

    public double NeutronValue     => _neutronValue;
    public SensorStatus NeutronStatus     => SensorBaseline.Neutron.Classify(_neutronValue);
    public double IonisationValue  => _ionisationValue;
    public SensorStatus IonisationStatus  => SensorBaseline.Ionisation.Classify(_ionisationValue);
    public double GeomagneticValue => _geomagneticValue;
    public SensorStatus GeomagneticStatus => SensorBaseline.Geomagnetic.Classify(_geomagneticValue);
    public double ThermalValue     => _thermalValue;
    public SensorStatus ThermalStatus     => SensorBaseline.Thermal.Classify(_thermalValue);
    public double ChronoValue      => _chronoValue;
    public SensorStatus ChronoStatus      => SensorBaseline.Chrono.Classify(_chronoValue);
    public double[] InfrasoundBands => _infrasoundBands;

    public SensorTickService(IDispatcherTimer timer)
    {
        _timer = timer;
        _timer.Interval = TickInterval;
        _timer.IsRepeating = true;
        _timer.Tick += (_, _) => Tick();
        Array.Fill(_infrasoundBands, 0.05);
    }

    public Task StartAsync() { _timer.Start(); return Task.CompletedTask; }
    public void Stop() => _timer.Stop();

    internal void Tick()
    {
        _neutronValue    = Drift(_neutronValue,    SensorBaseline.Neutron);
        _ionisationValue = Drift(_ionisationValue, SensorBaseline.Ionisation);
        _geomagneticValue = Drift(_geomagneticValue, SensorBaseline.Geomagnetic);
        _thermalValue    = Drift(_thermalValue,    SensorBaseline.Thermal);
        _chronoValue     = Drift(_chronoValue,     SensorBaseline.Chrono);
        DriftInfrasound();
        Ticked?.Invoke(this, EventArgs.Empty);
    }

    private double Drift(double current, SensorBaseline baseline)
    {
        // ±3–5 % of baseline per tick, clamped to ±5 % band around baseline
        double mag = baseline.BaselineValue > 0.001
            ? baseline.BaselineValue * (0.03 + _rng.NextDouble() * 0.02)
            : 0.005;
        double next = current + (_rng.NextDouble() < 0.5 ? mag : -mag);
        double lo = baseline.BaselineValue > 0.001
            ? baseline.BaselineValue * 0.95
            : -0.01;
        double hi = baseline.BaselineValue > 0.001
            ? baseline.BaselineValue * 1.05
            : 0.01;
        return Math.Clamp(next, lo, hi);
    }

    private void DriftInfrasound()
    {
        for (int i = 0; i < 20; i++)
            _infrasoundBands[i] = Math.Clamp(0.05 + (_rng.NextDouble() - 0.5) * 0.01, 0.01, 0.10);
    }
}
