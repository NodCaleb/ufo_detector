# Contract: Anomaly Definitions

**Feature**: 001-anomaly-detector-app
**Date**: 2026-08-16
**Format**: Static C# records in `UfoDetector/Models/AnomalyDefinitions.cs`

This document specifies the authoritative definitions of all five anomalies that MUST be
reproduced exactly in `AnomalyDefinitions.cs`. The `AnomalyEvaluator` service reads this
list to determine trigger conditions (FR-008, FR-013). No UI element or View may hard-code
any of these values.

All boundary values are **inclusive** (FR-008).

---

## Anomaly 1 — Электромагнитный след

### Trigger

| Parameter | Value |
|-----------|-------|
| Mode | `Passive` |
| Sensitivity | 60–70 % |
| Noise suppression | 15–25 % |

### Sensor targets (at full activation)

| Sensor | Target value | Unit | Status |
|--------|-------------|------|--------|
| Neutron radiation | 3.2 | мЗв/ч | НОРМА |
| Ionisation | 12 | % | НОРМА |
| Geomagnetic field | **55** | нТл | **АНОМАЛИЯ** (amber) |
| Thermal anomaly | 0.0 | °C | НОРМА |
| Chrono anomaly | 0.003 | Δt s | НОРМА |
| Infrasound bands | Broad low-amplitude peak at bins 2–5 Hz; all other bins near zero | — | — |

### Radar blips

| Property | Value |
|----------|-------|
| Count | 2–3 (randomised per activation) |
| Type | `EM` (green) |
| Behaviour | Slow random drift, angular speed ~0.02–0.05 rad/s |
| Distance zone | Mid-range (0.3–0.7) |

### Narrative

> Остаточный электромагнитный след от прошедшего объекта.
> Объект уже не здесь, но эфирная «рябь» ещё не рассеялась.

---

## Anomaly 2 — Нейтронный выброс

### Trigger

| Parameter | Value |
|-----------|-------|
| Mode | `Active` |
| Sensitivity | 80–89 % |
| Noise suppression | 10–20 % |

### Sensor targets (at full activation)

| Sensor | Target value | Unit | Status |
|--------|-------------|------|--------|
| Neutron radiation | **85** | мЗв/ч | **ОПАСНОСТЬ** (red) |
| Ionisation | **40** | % | **ПОВЫШЕН** (amber) |
| Geomagnetic field | 47 | нТл | НОРМА |
| Thermal anomaly | 0.0 | °C | НОРМА |
| Chrono anomaly | 0.003 | Δt s | НОРМА |
| Infrasound bands | All bins near zero (acoustic silence) | — | — |

### Radar blips

| Property | Value |
|----------|-------|
| Count | 1 (fixed) |
| Type | `Radiation` (amber) |
| Behaviour | Stationary (`DriftAngularSpeed = 0`, `IsFixed = true`) |
| Distance zone | Near zone (distance = 0.25–0.35) |

### Narrative

> Локальный источник нейтронного излучения — артефакт или устройство внеземного
> происхождения в непосредственной близости. Активный режим «засвечивает» его точнее.

---

## Anomaly 3 — Ионосферный пузырь

### Trigger

| Parameter | Value |
|-----------|-------|
| Mode | `Passive` |
| Sensitivity | 30–40 % |
| Noise suppression | 65–75 % |

### Sensor targets (at full activation)

| Sensor | Target value | Unit | Status |
|--------|-------------|------|--------|
| Neutron radiation | 3.2 | мЗв/ч | НОРМА |
| Ionisation | **78** | % | **ПОВЫШЕН** (amber) |
| Geomagnetic field | 47 | нТл | НОРМА |
| Thermal anomaly | **0.8** | °C | НОРМА |
| Chrono anomaly | 0.003 | Δt s | НОРМА |
| Infrasound bands | Peak at 4–6 Hz bins, amplitude above alarm threshold | — | — |

### Radar blips

| Property | Value |
|----------|-------|
| Count | 4–5 (randomised per activation) |
| Type | `Ionisation` (blue) |
| Behaviour | Orbit perimeter, angular speed ~0.04–0.06 rad/s, all at similar distance |
| Distance zone | Perimeter (0.7–0.9) |

### Narrative

> Пузырь ионизированного воздуха, характерный для зависания объекта над точкой.
> Высокое подавление помех «отфильтровывает» мелкие блипы и оставляет только контур явления.

---

## Anomaly 4 — Геомагнитный разлом

### Trigger

| Parameter | Value |
|-----------|-------|
| Mode | `Active` |
| Sensitivity | 50–60 % |
| Noise suppression | 40–50 % |

### Sensor targets (at full activation)

| Sensor | Target value | Unit | Status |
|--------|-------------|------|--------|
| Neutron radiation | 12 | мЗв/ч | НОРМА |
| Ionisation | 12 | % | НОРМА |
| Geomagnetic field | **310** | нТл | **КРИТИЧНО** (red) |
| Thermal anomaly | 0.0 | °C | НОРМА |
| Chrono anomaly | 0.003 | Δt s | НОРМА |
| Infrasound bands | Two sharp peaks at 8 Hz and 14 Hz bins | — | — |

### Radar blips

| Property | Value |
|----------|-------|
| Count | 2 (fixed) |
| Type | `Geomagnetic` (cyan) |
| Behaviour | Symmetric about centre (blip 2 angle = blip 1 angle + π); pulsing size/intensity |
| Distance zone | Mid-range (0.4–0.6) |

### Narrative

> Разрыв в геомагнитном поле — возможный признак работающего двигателя объекта
> или прохождения сквозь локальное силовое поле. Симметрия блипов указывает на
> искусственную природу источника.

---

## Anomaly 5 — Хронопертурбация

### Trigger

| Parameter | Value |
|-----------|-------|
| Mode | `Active` |
| Sensitivity | 90–100 % |
| Noise suppression | 0–10 % |

### Sensor targets (at full activation)

| Sensor | Target value | Unit | Status |
|--------|-------------|------|--------|
| Neutron radiation | 3.2 | мЗв/ч | НОРМА |
| Ionisation | **55** | % | **ПОВЫШЕН** (amber) |
| Geomagnetic field | 47 | нТл | НОРМА |
| Thermal anomaly | **3.1** | °C | **ПОВЫШЕН** (amber) |
| Chrono anomaly | **0.8** | Δt s | **КРИТИЧЕСКОЕ** (red) |
| Infrasound bands | Broadband noise across all 20 bins (0–20 Hz), no dominant peak | — | — |

### Radar blips

| Property | Value |
|----------|-------|
| Count | 1 (fixed) |
| Type | `Chrono` (purple) |
| Behaviour | Pinned at centre (`Distance = 0`, `IsFixed = true`, no drift) |
| Distance zone | Centre (0) |

### Narrative

> Наиболее редкая и опасная сигнатура: локальное искажение хода времени.
> Приборы максимальной чувствительности без фильтрации фиксируют «сырое» возмущение
> пространства-времени. Нахождение в зоне дольше 5 минут не рекомендуется.

---

## Summary Table

| # | Name | Mode | Sensitivity | Noise supp. | Key sensor(s) |
|---|------|------|------------|-------------|---------------|
| 1 | Электромагнитный след | ПАССИВ | 60–70 % | 15–25 % | Geomagnetic ↑ (amber) |
| 2 | Нейтронный выброс | АКТИВ | 80–89 % | 10–20 % | Neutron ↑↑ (red), Ionisation ↑ |
| 3 | Ионосферный пузырь | ПАССИВ | 30–40 % | 65–75 % | Ionisation ↑ + Infrasound peak |
| 4 | Геомагнитный разлом | АКТИВ | 50–60 % | 40–50 % | Geomagnetic ↑↑ (red) |
| 5 | Хронопертурбация | АКТИВ | 90–100 % | 0–10 % | Chrono ↑↑ (red) + Thermal ↑ + Ionisation ↑ |
