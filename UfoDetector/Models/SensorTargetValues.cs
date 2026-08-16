using UfoDetector.Models.Enums;

namespace UfoDetector.Models;

/// <summary>
/// Target sensor readings at lerp = 1.0; status fields used directly when LerpProgress ≥ 0.5.
/// </summary>
public record SensorTargetValues(
    double NeutronRadiation,
    SensorStatus NeutronStatus,
    double Ionisation,
    SensorStatus IonisationStatus,
    double GeomagneticField,
    SensorStatus GeomagneticStatus,
    double ThermalAnomaly,
    SensorStatus ThermalStatus,
    double ChronoAnomaly,
    SensorStatus ChronoStatus,
    double[] InfrasoundBands);
