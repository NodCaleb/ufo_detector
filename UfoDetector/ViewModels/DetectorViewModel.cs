using CommunityToolkit.Mvvm.ComponentModel;
using UfoDetector.Models;
using UfoDetector.Models.Enums;
using UfoDetector.Services;

namespace UfoDetector.ViewModels;

public partial class DetectorViewModel : ObservableObject
{
    private readonly ISensorTickService _tickService;
    private readonly ITransitionOrchestrator _orchestrator;

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

    // Unit labels — static, no need to be observable
    public string NeutronUnit     => "мЗв/ч";
    public string IonisationUnit  => "%";
    public string GeomagneticUnit => "нТл";
    public string ThermalUnit     => "°C";
    public string ChronoUnit      => "Δt с";

    // Normalised [0,1] fill-bar values based on critical threshold
    public double NeutronNormalized     => Math.Clamp(NeutronValue     / SensorBaseline.Neutron.CriticalThreshold,     0.0, 1.0);
    public double IonisationNormalized  => Math.Clamp(IonisationValue  / SensorBaseline.Ionisation.CriticalThreshold,  0.0, 1.0);
    public double GeomagneticNormalized => Math.Clamp(GeomagneticValue / SensorBaseline.Geomagnetic.CriticalThreshold, 0.0, 1.0);
    public double ThermalNormalized     => Math.Clamp(ThermalValue     / SensorBaseline.Thermal.CriticalThreshold,     0.0, 1.0);
    public double ChronoNormalized      => Math.Clamp(ChronoValue      / SensorBaseline.Chrono.CriticalThreshold,      0.0, 1.0);

    public DetectorViewModel(ISensorTickService tickService, ITransitionOrchestrator orchestrator)
    {
        _tickService  = tickService;
        _orchestrator = orchestrator;

        // Initialise from baselines so the UI shows correct values before first tick
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

        _tickService.Ticked += OnTicked;
    }

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
    }
}
