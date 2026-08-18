using UfoDetector.Models;
using UfoDetector.Models.Enums;
using UfoDetector.Services;

namespace UfoDetector.Tests.Services;

public class AnomalyEvaluatorTests
{
    private readonly AnomalyEvaluator _sut = new();

    private static DetectorState State(DetectorMode mode, int sensitivity, int noiseSupp) => new()
    {
        Mode             = mode,
        Sensitivity      = sensitivity,
        NoiseSuppression = noiseSupp,
    };

    // ── Each anomaly triggers on its exact combination ──────────────────────

    [Fact]
    public void Anomaly1_Triggers_On_Matching_Combination()
    {
        var result = _sut.Evaluate(State(DetectorMode.Passive, 65, 20));
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void Anomaly2_Triggers_On_Matching_Combination()
    {
        var result = _sut.Evaluate(State(DetectorMode.Active, 85, 15));
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
    }

    [Fact]
    public void Anomaly3_Triggers_On_Matching_Combination()
    {
        var result = _sut.Evaluate(State(DetectorMode.Passive, 35, 70));
        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
    }

    [Fact]
    public void Anomaly4_Triggers_On_Matching_Combination()
    {
        var result = _sut.Evaluate(State(DetectorMode.Active, 55, 45));
        Assert.NotNull(result);
        Assert.Equal(4, result.Id);
    }

    [Fact]
    public void Anomaly5_Triggers_On_Matching_Combination()
    {
        var result = _sut.Evaluate(State(DetectorMode.Active, 95, 5));
        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
    }

    // ── No match returns null ────────────────────────────────────────────────

    [Fact]
    public void No_Match_Returns_Null()
    {
        var result = _sut.Evaluate(State(DetectorMode.Passive, 50, 50));
        Assert.Null(result);
    }

    [Fact]
    public void Wrong_Mode_Returns_Null()
    {
        // Anomaly 1 requires Passive; switching to Active should return null
        var result = _sut.Evaluate(State(DetectorMode.Active, 65, 20));
        Assert.Null(result);
    }

    // ── Boundary values are inclusive (FR-008) ───────────────────────────────

    [Theory]
    [InlineData(60, 15)]  // min sensitivity, min noise
    [InlineData(60, 25)]  // min sensitivity, max noise
    [InlineData(70, 15)]  // max sensitivity, min noise
    [InlineData(70, 25)]  // max sensitivity, max noise
    public void Anomaly1_Triggers_On_All_Corner_Boundaries(int sensitivity, int noiseSupp)
    {
        var result = _sut.Evaluate(State(DetectorMode.Passive, sensitivity, noiseSupp));
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public void Anomaly1_Does_NOT_Trigger_When_Sensitivity_Is_Below_Min()
    {
        // sensitivity 59 < SensitivityMin 60 → no match
        var result = _sut.Evaluate(State(DetectorMode.Passive, 59, 20));
        Assert.Null(result);
    }

    [Fact]
    public void Anomaly1_Does_NOT_Trigger_When_Sensitivity_Is_Above_Max()
    {
        // sensitivity 71 > SensitivityMax 70 → no match
        var result = _sut.Evaluate(State(DetectorMode.Passive, 71, 20));
        Assert.Null(result);
    }

    [Fact]
    public void Anomaly1_Does_NOT_Trigger_When_NoiseSuppression_Below_Min()
    {
        var result = _sut.Evaluate(State(DetectorMode.Passive, 65, 14));
        Assert.Null(result);
    }

    [Fact]
    public void Anomaly1_Does_NOT_Trigger_When_NoiseSuppression_Above_Max()
    {
        var result = _sut.Evaluate(State(DetectorMode.Passive, 65, 26));
        Assert.Null(result);
    }

    // ── No two anomalies match simultaneously ────────────────────────────────

    [Theory]
    [InlineData(DetectorMode.Passive, 65, 20, 1)]
    [InlineData(DetectorMode.Active,  85, 15, 2)]
    [InlineData(DetectorMode.Passive, 35, 70, 3)]
    [InlineData(DetectorMode.Active,  55, 45, 4)]
    [InlineData(DetectorMode.Active,  95,  5, 5)]
    public void Exactly_One_Anomaly_Matches_Each_Trigger_Combination(
        DetectorMode mode, int sensitivity, int noiseSupp, int expectedId)
    {
        var result = _sut.Evaluate(State(mode, sensitivity, noiseSupp));
        Assert.NotNull(result);
        Assert.Equal(expectedId, result.Id);

        var allMatching = AnomalyDefinitions.All
            .Where(a =>
                a.Trigger.Mode == mode &&
                sensitivity >= a.Trigger.SensitivityMin && sensitivity <= a.Trigger.SensitivityMax &&
                noiseSupp   >= a.Trigger.NoiseSuppMin   && noiseSupp   <= a.Trigger.NoiseSuppMax)
            .ToList();
        Assert.Single(allMatching);
    }

    // ── T047: SensorTargets match contracts/anomaly-definitions.md ───────────

    [Fact]
    public void Anomaly1_SensorTargets_Match_Contract()
    {
        var a = _sut.Evaluate(State(DetectorMode.Passive, 65, 20))!;
        var t = a.SensorTargets;

        Assert.Equal(3.2,  t.NeutronRadiation,  precision: 3);
        Assert.Equal(SensorStatus.Normal,   t.NeutronStatus);
        Assert.Equal(12.0, t.Ionisation,         precision: 3);
        Assert.Equal(SensorStatus.Normal,   t.IonisationStatus);
        Assert.Equal(55.0, t.GeomagneticField,   precision: 3);
        Assert.Equal(SensorStatus.Anomaly,  t.GeomagneticStatus);
        Assert.Equal(0.0,  t.ThermalAnomaly,     precision: 3);
        Assert.Equal(SensorStatus.Normal,   t.ThermalStatus);
        Assert.Equal(0.003, t.ChronoAnomaly,     precision: 4);
        Assert.Equal(SensorStatus.Normal,   t.ChronoStatus);
        Assert.Equal(20, t.InfrasoundBands.Length);
        // broad low-amplitude peak at bins 2–5 (0-indexed)
        Assert.True(t.InfrasoundBands[2] > 0.1, "bin 2 should be elevated");
        Assert.True(t.InfrasoundBands[5] > 0.1, "bin 5 should be elevated");
        Assert.True(t.InfrasoundBands[10] < 0.1, "bin 10 should be near zero");
    }

    [Fact]
    public void Anomaly2_SensorTargets_Match_Contract()
    {
        var a = _sut.Evaluate(State(DetectorMode.Active, 85, 15))!;
        var t = a.SensorTargets;

        Assert.Equal(85.0, t.NeutronRadiation,  precision: 3);
        Assert.Equal(SensorStatus.Danger,    t.NeutronStatus);
        Assert.Equal(40.0, t.Ionisation,         precision: 3);
        Assert.Equal(SensorStatus.Elevated,  t.IonisationStatus);
        Assert.Equal(47.0, t.GeomagneticField,   precision: 3);
        Assert.Equal(SensorStatus.Normal,    t.GeomagneticStatus);
        Assert.Equal(0.0,  t.ThermalAnomaly,     precision: 3);
        Assert.Equal(SensorStatus.Normal,    t.ThermalStatus);
        Assert.Equal(0.003, t.ChronoAnomaly,     precision: 4);
        Assert.Equal(SensorStatus.Normal,    t.ChronoStatus);
        // acoustic silence — all bins near zero
        Assert.All(t.InfrasoundBands, b => Assert.True(b < 0.1, $"Anomaly 2 infrasound bin should be near zero, got {b}"));
    }

    [Fact]
    public void Anomaly3_SensorTargets_Match_Contract()
    {
        var a = _sut.Evaluate(State(DetectorMode.Passive, 35, 70))!;
        var t = a.SensorTargets;

        Assert.Equal(3.2,  t.NeutronRadiation,  precision: 3);
        Assert.Equal(SensorStatus.Normal,    t.NeutronStatus);
        Assert.Equal(78.0, t.Ionisation,         precision: 3);
        Assert.Equal(SensorStatus.Elevated,  t.IonisationStatus);
        Assert.Equal(47.0, t.GeomagneticField,   precision: 3);
        Assert.Equal(SensorStatus.Normal,    t.GeomagneticStatus);
        Assert.Equal(0.8,  t.ThermalAnomaly,     precision: 3);
        Assert.Equal(SensorStatus.Normal,    t.ThermalStatus);
        Assert.Equal(0.003, t.ChronoAnomaly,     precision: 4);
        Assert.Equal(SensorStatus.Normal,    t.ChronoStatus);
        // peak at 4–6 Hz (bins 4–6, 0-indexed) above alarm threshold
        Assert.True(t.InfrasoundBands[4] > 0.5, "bin 4 should be above alarm threshold");
        Assert.True(t.InfrasoundBands[5] > 0.5, "bin 5 should be above alarm threshold");
        Assert.True(t.InfrasoundBands[6] > 0.5, "bin 6 should be above alarm threshold");
    }

    [Fact]
    public void Anomaly4_SensorTargets_Match_Contract()
    {
        var a = _sut.Evaluate(State(DetectorMode.Active, 55, 45))!;
        var t = a.SensorTargets;

        Assert.Equal(12.0,  t.NeutronRadiation,  precision: 3);
        Assert.Equal(SensorStatus.Normal,    t.NeutronStatus);
        Assert.Equal(12.0,  t.Ionisation,         precision: 3);
        Assert.Equal(SensorStatus.Normal,    t.IonisationStatus);
        Assert.Equal(310.0, t.GeomagneticField,   precision: 3);
        Assert.Equal(SensorStatus.Critical,  t.GeomagneticStatus);
        Assert.Equal(0.0,   t.ThermalAnomaly,     precision: 3);
        Assert.Equal(SensorStatus.Normal,    t.ThermalStatus);
        Assert.Equal(0.003, t.ChronoAnomaly,      precision: 4);
        Assert.Equal(SensorStatus.Normal,    t.ChronoStatus);
        // two sharp peaks at bins 8 and 14 (0-indexed)
        Assert.True(t.InfrasoundBands[8]  > 0.5, "bin 8 should be a sharp peak");
        Assert.True(t.InfrasoundBands[14] > 0.5, "bin 14 should be a sharp peak");
    }

    [Fact]
    public void Anomaly5_SensorTargets_Match_Contract()
    {
        var a = _sut.Evaluate(State(DetectorMode.Active, 95, 5))!;
        var t = a.SensorTargets;

        Assert.Equal(3.2,  t.NeutronRadiation,   precision: 3);
        Assert.Equal(SensorStatus.Normal,    t.NeutronStatus);
        Assert.Equal(55.0, t.Ionisation,          precision: 3);
        Assert.Equal(SensorStatus.Elevated,  t.IonisationStatus);
        Assert.Equal(47.0, t.GeomagneticField,    precision: 3);
        Assert.Equal(SensorStatus.Normal,    t.GeomagneticStatus);
        Assert.Equal(3.1,  t.ThermalAnomaly,      precision: 3);
        Assert.Equal(SensorStatus.Elevated,  t.ThermalStatus);
        Assert.Equal(0.8,  t.ChronoAnomaly,       precision: 3);
        Assert.Equal(SensorStatus.Critical,  t.ChronoStatus);
        // broadband noise — all 20 bins with similar amplitude, no dominant peak
        Assert.Equal(20, t.InfrasoundBands.Length);
        Assert.All(t.InfrasoundBands, b => Assert.True(b > 0.1, $"Anomaly 5 broadband bin should be elevated, got {b}"));
    }

    // ── T047: RadarBlips match contracts/anomaly-definitions.md ─────────────

    [Fact]
    public void Anomaly1_RadarBlips_Match_Contract()
    {
        var a = _sut.Evaluate(State(DetectorMode.Passive, 65, 20))!;
        Assert.Single(a.RadarBlips);
        var blip = a.RadarBlips[0];
        Assert.Equal(BlipType.EM,  blip.Type);
        Assert.True(blip.CountMin >= 2 && blip.CountMax <= 3);
        Assert.True(blip.InitialDistanceMin >= 0.3 && blip.InitialDistanceMax <= 0.7);
        Assert.False(blip.IsFixed);
    }

    [Fact]
    public void Anomaly2_RadarBlips_Match_Contract()
    {
        var a = _sut.Evaluate(State(DetectorMode.Active, 85, 15))!;
        Assert.Single(a.RadarBlips);
        var blip = a.RadarBlips[0];
        Assert.Equal(BlipType.Radiation, blip.Type);
        Assert.Equal(1, blip.CountMin);
        Assert.Equal(1, blip.CountMax);
        Assert.Equal(0.0,  blip.DriftAngularSpeed, precision: 3);
        Assert.True(blip.IsFixed);
        Assert.True(blip.InitialDistanceMin >= 0.25 && blip.InitialDistanceMax <= 0.35);
    }

    [Fact]
    public void Anomaly3_RadarBlips_Match_Contract()
    {
        var a = _sut.Evaluate(State(DetectorMode.Passive, 35, 70))!;
        Assert.Single(a.RadarBlips);
        var blip = a.RadarBlips[0];
        Assert.Equal(BlipType.Ionisation, blip.Type);
        Assert.True(blip.CountMin >= 4 && blip.CountMax <= 5);
        Assert.True(blip.InitialDistanceMin >= 0.7 && blip.InitialDistanceMax <= 0.9);
        Assert.False(blip.IsFixed);
    }

    [Fact]
    public void Anomaly4_RadarBlips_Match_Contract()
    {
        var a = _sut.Evaluate(State(DetectorMode.Active, 55, 45))!;
        Assert.Single(a.RadarBlips);
        var blip = a.RadarBlips[0];
        Assert.Equal(BlipType.Geomagnetic, blip.Type);
        Assert.Equal(2, blip.CountMin);
        Assert.Equal(2, blip.CountMax);
        Assert.True(blip.InitialDistanceMin >= 0.4 && blip.InitialDistanceMax <= 0.6);
    }

    [Fact]
    public void Anomaly5_RadarBlips_Match_Contract()
    {
        var a = _sut.Evaluate(State(DetectorMode.Active, 95, 5))!;
        Assert.Single(a.RadarBlips);
        var blip = a.RadarBlips[0];
        Assert.Equal(BlipType.Chrono, blip.Type);
        Assert.Equal(1, blip.CountMin);
        Assert.Equal(1, blip.CountMax);
        Assert.Equal(0.0, blip.InitialDistanceMin, precision: 3);
        Assert.Equal(0.0, blip.InitialDistanceMax, precision: 3);
        Assert.Equal(0.0, blip.DriftAngularSpeed,  precision: 3);
        Assert.True(blip.IsFixed);
    }
}
