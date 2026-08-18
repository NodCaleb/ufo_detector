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
}
