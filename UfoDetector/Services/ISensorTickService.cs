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

    Task StartAsync();
    void Stop();
}
