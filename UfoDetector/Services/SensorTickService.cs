using UfoDetector.Models;
using UfoDetector.Models.Enums;

namespace UfoDetector.Services;

public class SensorTickService : ISensorTickService
{
    private readonly IDispatcherTimer _timer;
    private readonly IAnomalyEvaluator _evaluator;
    private readonly ITransitionOrchestrator _orchestrator;
    private readonly Random _rng = new();

    private readonly DetectorState _state = new();

    private double _neutronValue    = SensorBaseline.Neutron.BaselineValue;
    private double _ionisationValue = SensorBaseline.Ionisation.BaselineValue;
    private double _geomagneticValue = SensorBaseline.Geomagnetic.BaselineValue;
    private double _thermalValue    = SensorBaseline.Thermal.BaselineValue;
    private double _chronoValue     = SensorBaseline.Chrono.BaselineValue;
    private readonly double[] _infrasoundBands = new double[20];

    public static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(100);

    public event EventHandler? Ticked;

    public double NeutronValue     => _neutronValue;
    public double IonisationValue  => _ionisationValue;
    public double GeomagneticValue => _geomagneticValue;
    public double ThermalValue     => _thermalValue;
    public double ChronoValue      => _chronoValue;
    public double[] InfrasoundBands => _infrasoundBands;

    public Anomaly? ActiveAnomaly => _state.ActiveAnomaly;
    public TransitionPhase Phase  => _state.Phase;
    public double LerpProgress    => _state.LerpProgress;

    public SensorTickService(
        IDispatcherTimer timer,
        IAnomalyEvaluator evaluator,
        ITransitionOrchestrator orchestrator)
    {
        _timer       = timer;
        _evaluator   = evaluator;
        _orchestrator = orchestrator;
        _timer.Interval    = TickInterval;
        _timer.IsRepeating = true;
        _timer.Tick += (_, _) => Tick();
        Array.Fill(_infrasoundBands, 0.05);
    }

    public void UpdateControls(DetectorMode mode, int sensitivity, int noiseSuppression)
    {
        _state.Mode             = mode;
        _state.Sensitivity      = sensitivity;
        _state.NoiseSuppression = noiseSuppression;
    }

    public Task StartAsync() { _timer.Start(); return Task.CompletedTask; }
    public void Stop() => _timer.Stop();

    internal void Tick()
    {
        _state.PendingAnomaly = _evaluator.Evaluate(_state);
        _orchestrator.Step(_state, TickInterval.TotalSeconds);

        if (_state.Phase is TransitionPhase.Appearing or TransitionPhase.Active or TransitionPhase.Fading
            && _state.ActiveAnomaly is not null)
        {
            LerpSensors(_state.ActiveAnomaly.SensorTargets, _state.LerpProgress);
        }
        else
        {
            DriftSensors();
        }
        Ticked?.Invoke(this, EventArgs.Empty);
    }

    // Derived status: baseline thresholds below lerp midpoint, target statuses above
    public SensorStatus NeutronStatus     => ComputeStatus(_neutronValue,    SensorBaseline.Neutron,
                                                _state.ActiveAnomaly?.SensorTargets.NeutronStatus,     _state.LerpProgress);
    public SensorStatus IonisationStatus  => ComputeStatus(_ionisationValue,  SensorBaseline.Ionisation,
                                                _state.ActiveAnomaly?.SensorTargets.IonisationStatus,  _state.LerpProgress);
    public SensorStatus GeomagneticStatus => ComputeStatus(_geomagneticValue, SensorBaseline.Geomagnetic,
                                                _state.ActiveAnomaly?.SensorTargets.GeomagneticStatus, _state.LerpProgress);
    public SensorStatus ThermalStatus     => ComputeStatus(_thermalValue,     SensorBaseline.Thermal,
                                                _state.ActiveAnomaly?.SensorTargets.ThermalStatus,     _state.LerpProgress);
    public SensorStatus ChronoStatus      => ComputeStatus(_chronoValue,      SensorBaseline.Chrono,
                                                _state.ActiveAnomaly?.SensorTargets.ChronoStatus,      _state.LerpProgress);

    private static SensorStatus ComputeStatus(double value, SensorBaseline baseline,
        SensorStatus? targetStatus, double lerpProgress)
    {
        if (targetStatus.HasValue && lerpProgress >= 0.5)
            return targetStatus.Value;
        return baseline.Classify(value);
    }

    private void LerpSensors(SensorTargetValues targets, double p)
    {
        _neutronValue     = Lerp(SensorBaseline.Neutron.BaselineValue,     targets.NeutronRadiation,  p);
        _ionisationValue  = Lerp(SensorBaseline.Ionisation.BaselineValue,  targets.Ionisation,        p);
        _geomagneticValue = Lerp(SensorBaseline.Geomagnetic.BaselineValue, targets.GeomagneticField,  p);
        _thermalValue     = Lerp(SensorBaseline.Thermal.BaselineValue,     targets.ThermalAnomaly,    p);
        _chronoValue      = Lerp(SensorBaseline.Chrono.BaselineValue,      targets.ChronoAnomaly,     p);
        LerpInfrasound(targets.InfrasoundBands, p);
    }

    private void LerpInfrasound(double[] targetBands, double p)
    {
        for (int i = 0; i < 20; i++)
            _infrasoundBands[i] = Lerp(0.05, targetBands[i], p);
    }

    private static double Lerp(double from, double to, double t) => from + (to - from) * t;

    private void DriftSensors()
    {
        _neutronValue     = Drift(_neutronValue,     SensorBaseline.Neutron);
        _ionisationValue  = Drift(_ionisationValue,  SensorBaseline.Ionisation);
        _geomagneticValue = Drift(_geomagneticValue, SensorBaseline.Geomagnetic);
        _thermalValue     = Drift(_thermalValue,     SensorBaseline.Thermal);
        _chronoValue      = Drift(_chronoValue,      SensorBaseline.Chrono);
        for (int i = 0; i < 20; i++)
            _infrasoundBands[i] = Math.Clamp(0.05 + (_rng.NextDouble() - 0.5) * 0.01, 0.01, 0.10);
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
}

