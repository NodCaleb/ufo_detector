using UfoDetector.Models;
using UfoDetector.Models.Enums;

namespace UfoDetector.Services;

public interface ISensorTickService
{
    event EventHandler? Ticked;

    double NeutronValue    { get; }
    SensorStatus NeutronStatus    { get; }
    double IonisationValue { get; }
    SensorStatus IonisationStatus { get; }
    double GeomagneticValue { get; }
    SensorStatus GeomagneticStatus { get; }
    double ThermalValue    { get; }
    SensorStatus ThermalStatus    { get; }
    double ChronoValue     { get; }
    SensorStatus ChronoStatus     { get; }
    double[] InfrasoundBands { get; }

    // Phase 5: anomaly state exposed to ViewModel
    Anomaly? ActiveAnomaly { get; }
    TransitionPhase Phase  { get; }
    double LerpProgress    { get; }

    Task StartAsync();
    void Stop();

    /// <summary>Sync the tick service's DetectorState with the current control values.</summary>
    void UpdateControls(DetectorMode mode, int sensitivity, int noiseSuppression);
}
