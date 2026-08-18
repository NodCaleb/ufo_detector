using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using UfoDetector.Models;
using UfoDetector.Models.Enums;
using UfoDetector.Services;

namespace UfoDetector.ViewModels;

public partial class DetectorViewModel : ObservableObject
{
    private readonly ISensorTickService _tickService;
    private readonly ITransitionOrchestrator _orchestrator;
    private readonly IPreferences _preferences;

    // Suppresses preference writes triggered by the constructor restore
    private bool _isInitializing = true;

    private const string PrefKeySensitivity      = "Sensitivity";
    private const string PrefKeyNoiseSuppression = "NoiseSuppression";

    // ── Sensor readings ───────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NeutronNormalized))]
    public partial double NeutronValue     { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IonisationNormalized))]
    public partial double IonisationValue  { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GeomagneticNormalized))]
    public partial double GeomagneticValue { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThermalNormalized))]
    public partial double ThermalValue     { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChronoNormalized))]
    public partial double ChronoValue      { get; set; }

    [ObservableProperty] public partial SensorStatus NeutronStatus     { get; set; }
    [ObservableProperty] public partial SensorStatus IonisationStatus  { get; set; }
    [ObservableProperty] public partial SensorStatus GeomagneticStatus { get; set; }
    [ObservableProperty] public partial SensorStatus ThermalStatus     { get; set; }
    [ObservableProperty] public partial SensorStatus ChronoStatus      { get; set; }

    [ObservableProperty] public partial double[] InfrasoundBands { get; set; }

    // ── Controls (Phase 4) ────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsActiveMode))]
    [NotifyPropertyChangedFor(nameof(ModeLabel))]
    public partial DetectorMode Mode { get; set; }

    [ObservableProperty] public partial int Sensitivity      { get; set; }
    [ObservableProperty] public partial int NoiseSuppression { get; set; }

    // ── Phase 5: anomaly transition state ─────────────────────────────────────

    [ObservableProperty] public partial Anomaly? ActiveAnomaly { get; set; }
    [ObservableProperty] public partial TransitionPhase Phase  { get; set; }
    [ObservableProperty] public partial double LerpProgress    { get; set; }

    public bool   IsActiveMode => Mode == DetectorMode.Active;
    public string ModeLabel    => Mode == DetectorMode.Active ? "АКТИВ" : "ПАССИВ";

    // ── Units (static) ────────────────────────────────────────────────────────

    public string NeutronUnit     => "мЗв/ч";
    public string IonisationUnit  => "%";
    public string GeomagneticUnit => "нТл";
    public string ThermalUnit     => "°C";
    public string ChronoUnit      => "Δt с";

    // ── Normalised fill-bar values [0,1] ─────────────────────────────────────

    public double NeutronNormalized     => Math.Clamp(NeutronValue     / SensorBaseline.Neutron.CriticalThreshold,     0.0, 1.0);
    public double IonisationNormalized  => Math.Clamp(IonisationValue  / SensorBaseline.Ionisation.CriticalThreshold,  0.0, 1.0);
    public double GeomagneticNormalized => Math.Clamp(GeomagneticValue / SensorBaseline.Geomagnetic.CriticalThreshold, 0.0, 1.0);
    public double ThermalNormalized     => Math.Clamp(ThermalValue     / SensorBaseline.Thermal.CriticalThreshold,     0.0, 1.0);
    public double ChronoNormalized      => Math.Clamp(ChronoValue      / SensorBaseline.Chrono.CriticalThreshold,      0.0, 1.0);

    // ── Constructor ───────────────────────────────────────────────────────────

    public DetectorViewModel(
        ISensorTickService    tickService,
        ITransitionOrchestrator orchestrator,
        IPreferences          preferences)
    {
        _tickService  = tickService;
        _orchestrator = orchestrator;
        _preferences  = preferences;

        // Sensor baselines
        NeutronValue     = SensorBaseline.Neutron.BaselineValue;
        IonisationValue  = SensorBaseline.Ionisation.BaselineValue;
        GeomagneticValue = SensorBaseline.Geomagnetic.BaselineValue;
        ThermalValue     = SensorBaseline.Thermal.BaselineValue;
        ChronoValue      = SensorBaseline.Chrono.BaselineValue;

        NeutronStatus     = SensorStatus.Normal;
        IonisationStatus  = SensorStatus.Normal;
        GeomagneticStatus = SensorStatus.Normal;
        ThermalStatus     = SensorStatus.Normal;
        ChronoStatus      = SensorStatus.Normal;

        InfrasoundBands = new double[20];
        Array.Fill(InfrasoundBands, 0.05);

        // Controls — restore persisted values; init flag prevents writing them back
        Mode             = DetectorMode.Passive;
        Sensitivity      = _preferences.Get(PrefKeySensitivity,      50, null);
        NoiseSuppression = _preferences.Get(PrefKeyNoiseSuppression, 50, null);

        _isInitializing = false;

        _tickService.UpdateControls(Mode, Sensitivity, NoiseSuppression);
        _tickService.Ticked += OnTicked;
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private void ToggleMode()
    {
        Mode = Mode == DetectorMode.Active ? DetectorMode.Passive : DetectorMode.Active;
        _tickService.UpdateControls(Mode, Sensitivity, NoiseSuppression);
    }

    // ── Preference persistence hooks ─────────────────────────────────────────

    partial void OnSensitivityChanged(int value)
    {
        if (!_isInitializing)
        {
            _preferences.Set(PrefKeySensitivity, value, null);
            _tickService.UpdateControls(Mode, value, NoiseSuppression);
        }
    }

    partial void OnNoiseSuppressionChanged(int value)
    {
        if (!_isInitializing)
        {
            _preferences.Set(PrefKeyNoiseSuppression, value, null);
            _tickService.UpdateControls(Mode, Sensitivity, value);
        }
    }

    // ── Tick handler ──────────────────────────────────────────────────────────

    private void OnTicked(object? sender, EventArgs e)
    {
        NeutronValue     = _tickService.NeutronValue;
        IonisationValue  = _tickService.IonisationValue;
        GeomagneticValue = _tickService.GeomagneticValue;
        ThermalValue     = _tickService.ThermalValue;
        ChronoValue      = _tickService.ChronoValue;

        NeutronStatus     = _tickService.NeutronStatus;
        IonisationStatus  = _tickService.IonisationStatus;
        GeomagneticStatus = _tickService.GeomagneticStatus;
        ThermalStatus     = _tickService.ThermalStatus;
        ChronoStatus      = _tickService.ChronoStatus;

        // Copy bands to avoid retaining a reference to the service's internal array
        var bands = _tickService.InfrasoundBands;
        var copy = new double[20];
        Array.Copy(bands, copy, 20);
        InfrasoundBands = copy;

        ActiveAnomaly = _tickService.ActiveAnomaly;
        Phase         = _tickService.Phase;
        LerpProgress  = _tickService.LerpProgress;
    }
}
