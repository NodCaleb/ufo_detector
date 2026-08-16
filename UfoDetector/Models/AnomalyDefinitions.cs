using UfoDetector.Models.Enums;

namespace UfoDetector.Models;

public static class AnomalyDefinitions
{
    public static readonly IReadOnlyList<Anomaly> All = new[]
    {
        // ── Anomaly 1: Электромагнитный след ─────────────────────────────────
        new Anomaly(
            Id: 1,
            Name: "Электромагнитный след",
            Narrative:
                "Остаточный электромагнитный след от прошедшего объекта.\n" +
                "Объект уже не здесь, но эфирная «рябь» ещё не рассеялась.",
            Trigger: new AnomalyTrigger(
                Mode: DetectorMode.Passive,
                SensitivityMin: 60, SensitivityMax: 70,
                NoiseSuppMin: 15,   NoiseSuppMax: 25),
            SensorTargets: new SensorTargetValues(
                NeutronRadiation:  3.2,  NeutronStatus:     SensorStatus.Normal,
                Ionisation:       12.0,  IonisationStatus:  SensorStatus.Normal,
                GeomagneticField: 55.0,  GeomagneticStatus: SensorStatus.Anomaly,
                ThermalAnomaly:    0.0,  ThermalStatus:     SensorStatus.Normal,
                ChronoAnomaly:   0.003,  ChronoStatus:      SensorStatus.Normal,
                // Broad low-amplitude peak at 2–5 Hz; all other bins near zero
                InfrasoundBands: [0.05, 0.05, 0.50, 0.55, 0.60, 0.55, 0.05, 0.05,
                                  0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05,
                                  0.05, 0.05, 0.05, 0.05]),
            RadarBlips:
            [
                new RadarBlipTemplate(
                    Type: BlipType.EM,
                    CountMin: 2, CountMax: 3,
                    InitialDistanceMin: 0.30, InitialDistanceMax: 0.70,
                    DriftAngularSpeed: 0.035, DriftRadialSpeed: 0.0,
                    IsFixed: false),
            ]),

        // ── Anomaly 2: Нейтронный выброс ─────────────────────────────────────
        new Anomaly(
            Id: 2,
            Name: "Нейтронный выброс",
            Narrative:
                "Локальный источник нейтронного излучения — артефакт или устройство внеземного\n" +
                "происхождения в непосредственной близости. Активный режим «засвечивает» его точнее.",
            Trigger: new AnomalyTrigger(
                Mode: DetectorMode.Active,
                SensitivityMin: 80, SensitivityMax: 89,
                NoiseSuppMin: 10,   NoiseSuppMax: 20),
            SensorTargets: new SensorTargetValues(
                NeutronRadiation:  85.0, NeutronStatus:     SensorStatus.Danger,
                Ionisation:        40.0, IonisationStatus:  SensorStatus.Elevated,
                GeomagneticField:  47.0, GeomagneticStatus: SensorStatus.Normal,
                ThermalAnomaly:     0.0, ThermalStatus:     SensorStatus.Normal,
                ChronoAnomaly:    0.003, ChronoStatus:      SensorStatus.Normal,
                // Acoustic silence
                InfrasoundBands: [0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02,
                                  0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02, 0.02,
                                  0.02, 0.02, 0.02, 0.02]),
            RadarBlips:
            [
                new RadarBlipTemplate(
                    Type: BlipType.Radiation,
                    CountMin: 1, CountMax: 1,
                    InitialDistanceMin: 0.25, InitialDistanceMax: 0.35,
                    DriftAngularSpeed: 0.0,   DriftRadialSpeed: 0.0,
                    IsFixed: true),
            ]),

        // ── Anomaly 3: Ионосферный пузырь ────────────────────────────────────
        new Anomaly(
            Id: 3,
            Name: "Ионосферный пузырь",
            Narrative:
                "Пузырь ионизированного воздуха, характерный для зависания объекта над точкой.\n" +
                "Высокое подавление помех «отфильтровывает» мелкие блипы и оставляет только контур явления.",
            Trigger: new AnomalyTrigger(
                Mode: DetectorMode.Passive,
                SensitivityMin: 30, SensitivityMax: 40,
                NoiseSuppMin: 65,   NoiseSuppMax: 75),
            SensorTargets: new SensorTargetValues(
                NeutronRadiation:   3.2, NeutronStatus:     SensorStatus.Normal,
                Ionisation:        78.0, IonisationStatus:  SensorStatus.Elevated,
                GeomagneticField:  47.0, GeomagneticStatus: SensorStatus.Normal,
                ThermalAnomaly:     0.8, ThermalStatus:     SensorStatus.Normal,
                ChronoAnomaly:    0.003, ChronoStatus:      SensorStatus.Normal,
                // Peak at 4–6 Hz, above alarm threshold
                InfrasoundBands: [0.05, 0.05, 0.05, 0.05, 0.75, 0.80, 0.75, 0.05,
                                  0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05,
                                  0.05, 0.05, 0.05, 0.05]),
            RadarBlips:
            [
                new RadarBlipTemplate(
                    Type: BlipType.Ionisation,
                    CountMin: 4, CountMax: 5,
                    InitialDistanceMin: 0.70, InitialDistanceMax: 0.90,
                    DriftAngularSpeed: 0.05, DriftRadialSpeed: 0.0,
                    IsFixed: false),
            ]),

        // ── Anomaly 4: Геомагнитный разлом ───────────────────────────────────
        new Anomaly(
            Id: 4,
            Name: "Геомагнитный разлом",
            Narrative:
                "Разрыв в геомагнитном поле — возможный признак работающего двигателя объекта\n" +
                "или прохождения сквозь локальное силовое поле. Симметрия блипов указывает на\n" +
                "искусственную природу источника.",
            Trigger: new AnomalyTrigger(
                Mode: DetectorMode.Active,
                SensitivityMin: 50, SensitivityMax: 60,
                NoiseSuppMin: 40,   NoiseSuppMax: 50),
            SensorTargets: new SensorTargetValues(
                NeutronRadiation:  12.0, NeutronStatus:     SensorStatus.Normal,
                Ionisation:        12.0, IonisationStatus:  SensorStatus.Normal,
                GeomagneticField: 310.0, GeomagneticStatus: SensorStatus.Critical,
                ThermalAnomaly:     0.0, ThermalStatus:     SensorStatus.Normal,
                ChronoAnomaly:    0.003, ChronoStatus:      SensorStatus.Normal,
                // Two sharp peaks at bins 8 and 14
                InfrasoundBands: [0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05, 0.05,
                                  0.90, 0.05, 0.05, 0.05, 0.05, 0.05, 0.90, 0.05,
                                  0.05, 0.05, 0.05, 0.05]),
            RadarBlips:
            [
                // Two blips; RadarCanvasView positions them symmetrically (angle + π)
                new RadarBlipTemplate(
                    Type: BlipType.Geomagnetic,
                    CountMin: 2, CountMax: 2,
                    InitialDistanceMin: 0.40, InitialDistanceMax: 0.60,
                    DriftAngularSpeed: 0.01,  DriftRadialSpeed: 0.0,
                    IsFixed: false),
            ]),

        // ── Anomaly 5: Хронопертурбация ───────────────────────────────────────
        new Anomaly(
            Id: 5,
            Name: "Хронопертурбация",
            Narrative:
                "Наиболее редкая и опасная сигнатура: локальное искажение хода времени.\n" +
                "Приборы максимальной чувствительности без фильтрации фиксируют «сырое» возмущение\n" +
                "пространства-времени. Нахождение в зоне дольше 5 минут не рекомендуется.",
            Trigger: new AnomalyTrigger(
                Mode: DetectorMode.Active,
                SensitivityMin: 90, SensitivityMax: 100,
                NoiseSuppMin:    0, NoiseSuppMax:   10),
            SensorTargets: new SensorTargetValues(
                NeutronRadiation:   3.2, NeutronStatus:     SensorStatus.Normal,
                Ionisation:        55.0, IonisationStatus:  SensorStatus.Elevated,
                GeomagneticField:  47.0, GeomagneticStatus: SensorStatus.Normal,
                ThermalAnomaly:     3.1, ThermalStatus:     SensorStatus.Elevated,
                ChronoAnomaly:      0.8, ChronoStatus:      SensorStatus.Critical,
                // Broadband noise, no dominant peak
                InfrasoundBands: [0.55, 0.52, 0.58, 0.53, 0.57, 0.54, 0.56, 0.51,
                                  0.59, 0.53, 0.55, 0.52, 0.57, 0.53, 0.56, 0.54,
                                  0.58, 0.52, 0.55, 0.53]),
            RadarBlips:
            [
                // Pinned at centre, no drift
                new RadarBlipTemplate(
                    Type: BlipType.Chrono,
                    CountMin: 1, CountMax: 1,
                    InitialDistanceMin: 0.0, InitialDistanceMax: 0.0,
                    DriftAngularSpeed: 0.0,  DriftRadialSpeed: 0.0,
                    IsFixed: true),
            ]),
    };
}
