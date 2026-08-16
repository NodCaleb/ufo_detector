# Feature Specification: UFO Anomaly Detector Mobile App

**Feature Branch**: `001-anomaly-detector-app`

**Created**: 2026-08-16

**Status**: Draft

**Input**: User description: "Создай мобильное приложение, выполняющее функции, описанные в файле anomaly-detection-rules.md. Прототип интерфейса содержиться в файле ui-preview-3.html"

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Viewing Live Sensor Readings (Priority: P1)

As a player using the detector in a LARP session, I want to see a live dashboard
with all sensor gauges animated and updating in real time so that the device looks
like a functioning scientific instrument.

**Why this priority**: This is the core visual effect of the app. Without animated
gauges the device has no credibility as a prop. It must work before any anomaly
logic is added.

**Independent Test**: Launch the app — all gauges (neutron radiation, air ionisation,
geomagnetic field, thermal anomaly, chrono-anomaly, infrasound spectrum) are visible,
animate continuously, and show "НОРМА" baseline values. Can be demonstrated to a
stakeholder in isolation.

**Acceptance Scenarios**:

1. **Given** the app is launched, **When** the main screen appears, **Then** six sensor
   panels are visible with animated values fluctuating within normal ranges.
2. **Given** the main screen is active, **When** 1–2 seconds pass, **Then** every numeric
   gauge value updates with a ±3–5 % random drift without user interaction.
3. **Given** the app is running, **When** a gauge is in the "НОРМА" range, **Then** the
   gauge fill and status label are green.

---

### User Story 2 — Adjusting Mode, Sensitivity, and Noise Suppression (Priority: P2)

As a player, I want to manipulate the mode toggle (АКТИВ/ПАССИВ), the sensitivity
slider (0–100 %), and the noise-suppression slider (0–100 %) so that I can navigate
toward an anomaly trigger combination.

**Why this priority**: The controls are the only input mechanism. Without them no anomaly
can ever be triggered.

**Independent Test**: Set mode to АКТИВ, sensitivity to 75 %, noise suppression to 15 %.
Confirm the displayed values match the set values exactly. No anomaly needs to appear.

**Acceptance Scenarios**:

1. **Given** the controls panel is visible, **When** the player taps the mode toggle,
   **Then** the mode switches between АКТИВ and ПАССИВ and the LED indicator updates.
2. **Given** the sensitivity slider is visible, **When** the player drags it to a new
   position, **Then** the numeric label next to the slider updates to the new percentage
   value in 1 % increments.
3. **Given** the noise-suppression slider is visible, **When** the player drags it to a
   new position, **Then** the numeric label updates to the new percentage value in 1 %
   increments.
4. **Given** any slider, **When** the player releases the thumb, **Then** the value
   remains at the set position and does not snap or drift.

---

### User Story 3 — Triggering an Anomaly (Priority: P3)

As a player who knows the secret combination, I want the detector to display the
correct anomaly signature when all three conditions (mode, sensitivity, noise
suppression) are simultaneously satisfied so that I can demonstrate the effect to
other participants.

**Why this priority**: This is the primary "wow moment" of the prop. It requires P1
and P2 to be working first.

**Independent Test**: Set controls to the exact Anomaly 1 combination (ПАССИВ,
sensitivity 65 %, noise suppression 20 %). The radar, geomagnetic gauge, and
infrasound panel must transition into the Anomaly 1 state within 3 seconds.

**Acceptance Scenarios**:

1. **Given** controls match an anomaly combination, **When** all three conditions are
   met simultaneously, **Then** the detector transitions into that anomaly's visual
   state within 2–3 seconds (gradual onset).
2. **Given** an anomaly is active, **When** any one of the three conditions moves
   outside its range, **Then** the anomaly fades out within 1.5 seconds (gradual offset).
3. **Given** an anomaly is fading out and the player immediately enters a different
   anomaly's combination, **Then** the first anomaly fully fades before the second
   anomaly starts to appear.
4. **Given** an anomaly is active, **When** no control has changed, **Then** the anomaly
   remains visible continuously.

---

### User Story 4 — Anomaly-Specific Sensor Signatures (Priority: P4)

As a player demonstrating the device, I want each of the five anomalies to show a
distinct, recognisable pattern across the correct sensors so that each anomaly feels
unique.

**Why this priority**: Relies on P3; the trigger must work before signatures matter.

**Independent Test**: Cycle through all five anomaly combinations. Verify each
activates only its documented sensors and that no two anomalies produce identical
radar + gauge combinations.

**Acceptance Scenarios**:

1. **Given** Anomaly 1 ("Электромагнитный след") is active, **When** the radar is
   observed, **Then** 2–3 slowly drifting green EM blips appear; the geomagnetic gauge
   reads ~55 нТл with status АНОМАЛИЯ (amber); the infrasound spectrum shows a broad
   low-amplitude peak at 2–5 Hz.
2. **Given** Anomaly 2 ("Нейтронный выброс") is active, **When** the radar is
   observed, **Then** 1 bright amber Radiation blip appears in the near zone; the
   neutron-radiation gauge is in the red zone (~85 мЗв/ч) with status ОПАСНОСТЬ;
   ionisation shows ~40 % ПОВЫШЕН; infrasound is silent.
3. **Given** Anomaly 3 ("Ионосферный пузырь") is active, **When** the radar is
   observed, **Then** 4–5 blue Ionisation blips orbit the perimeter; ionisation
   gauge reads ~78 % ПОВЫШЕН; infrasound shows a peak at 4–6 Hz above the alarm
   threshold; thermal shows +0.8 °C НОРМА.
4. **Given** Anomaly 4 ("Геомагнитный разлом") is active, **When** the radar is
   observed, **Then** 2 cyan Geomagnetic blips pulse symmetrically; geomagnetic
   gauge reads ~310 нТл КРИТИЧНО (red); infrasound shows two sharp peaks at 8 Hz
   and 14 Hz; neutron shows ~12 мЗв/ч НОРМА.
5. **Given** Anomaly 5 ("Хронопертурбация") is active, **When** the radar is
   observed, **Then** a single purple Chrono blip is pinned at center; chrono gauge
   shows Δt +0.8 с КРИТИЧЕСКОЕ (red); thermal reads +3.1 °C ПОВЫШЕН; ionisation
   ~55 % ПОВЫШЕН; infrasound shows broadband noise across 0–20 Hz.

---

### Edge Cases

- What happens when the user sets values exactly on the boundary of an anomaly range
  (e.g., sensitivity = 60 % exactly)? The boundary value MUST be inclusive (within range).
- What happens if the user rapidly toggles the mode button multiple times? The last
  stable state is used; no crash or visual corruption occurs.
- What happens when the device is rotated? The app is locked to portrait orientation;
  rotation has no effect.
- What happens if two slider thumbs are dragged simultaneously? Each slider operates
  independently; simultaneous touches on both sliders must not cause a crash.
- What happens during anomaly transition if another anomaly is entered? The outgoing
  anomaly must fully complete its 1.5 s fade before the incoming anomaly starts.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The app MUST display a real-time dashboard containing: rotating radar
  canvas, neutron-radiation gauge, air-ionisation gauge, geomagnetic-field gauge,
  thermal-anomaly gauge, chrono-anomaly bar, and infrasound spectrum canvas.
- **FR-002**: All numeric sensor values MUST animate continuously with ±3–5 % random
  drift at a 1–2 second update period when no anomaly is active.
- **FR-003**: The radar MUST render a rotating sweep line at ≥60 fps using SkiaSharp,
  with blips fading after the sweep passes.
- **FR-004**: The infrasound spectrum MUST render a real-time bar-graph animation at
  ≥60 fps using SkiaSharp.
- **FR-005**: The mode toggle MUST switch between АКТИВ and ПАССИВ states; the
  selected state MUST be visually distinct (colour, LED indicator).
- **FR-006**: The sensitivity slider MUST accept values from 0 to 100 in 1 %
  increments and display the current value numerically.
- **FR-007**: The noise-suppression slider MUST accept values from 0 to 100 in 1 %
  increments and display the current value numerically.
- **FR-008**: The app MUST evaluate the anomaly trigger condition on every change of
  mode, sensitivity, or noise suppression using the following logic: anomaly is
  active when mode matches AND sensitivity is within the anomaly's ±~10 % range
  AND noise suppression is within the anomaly's ±~10 % range.
- **FR-009**: When an anomaly becomes active, its sensor signatures MUST appear
  gradually over 2–3 seconds (smooth interpolation from baseline to target values).
- **FR-010**: When an anomaly becomes inactive, its sensor signatures MUST fade
  gradually over 1.5 seconds back to baseline values.
- **FR-011**: If a second anomaly is triggered while the first is still fading,
  the second anomaly MUST NOT start appearing until the first has fully faded.
- **FR-012**: Each gauge MUST use the colour scheme: green = normal, amber = elevated/
  warning, red = critical/danger — matching the status labels НОРМА / ПОВЫШЕН /
  АНОМАЛИЯ / ОПАСНОСТЬ / КРИТИЧНО / КРИТИЧЕСКОЕ.
- **FR-013**: The five anomaly definitions (trigger conditions + sensor target values)
  MUST be stored as data (not hard-coded per UI element) so they can be adjusted
  without changing rendering code.
- **FR-014**: The app MUST NOT make any network requests or require any permission
  beyond those needed for display.
- **FR-015**: The app MUST be locked to portrait orientation and target a tablet form
  factor (≥768 × 1024 px).

### Key Entities

- **Anomaly**: Identifier, name, narrative text, trigger conditions (mode, sensitivity
  range, noise-suppression range), sensor target values for each gauge and radar blip
  configuration.
- **SensorReading**: Current value, unit, status level (normal/warning/critical) for
  each sensor channel. Animates between baseline and anomaly target.
- **DetectorState**: Current mode (АКТИВ/ПАССИВ), sensitivity percentage, noise-
  suppression percentage, active anomaly (nullable), transition phase (idle/appearing/
  active/fading).
- **RadarBlip**: Type (EM, Radiation, Ionisation, Thermal, Geomagnetic, Chrono),
  position (angle, distance from centre), drift behaviour, colour.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All sensor gauges update visually within 2 seconds of app launch on a
  mid-range Android tablet.
- **SC-002**: Radar and infrasound canvas render at a stable 60 fps during normal
  operation and during anomaly transitions, with no visible frame drops during a
  30-second observation.
- **SC-003**: An anomaly is triggered within 3 seconds of the player setting all three
  control values to within a valid combination range.
- **SC-004**: An anomaly fully disappears within 1.5 seconds of any one control value
  leaving its valid range.
- **SC-005**: All five anomaly combinations produce visually distinct states, verifiable
  by a first-time observer without reading documentation.
- **SC-006**: The app runs continuously for 30 minutes without crashing, memory leaking,
  or dropping below 45 fps on the target device.
- **SC-007**: Each slider produces values in exactly 1 % steps with no skipping, verifiable
  by dragging slowly from 0 to 100 and back.

## Assumptions

- The app is a standalone visual-effects simulator; no real sensors are involved.
- All sensor values are algorithmically generated — no hardware access or permissions
  are needed beyond standard display.
- Target runtime is .NET MAUI on Android; target device profile is tablet
  (≥768 × 1024 px) in portrait orientation, matching the UI prototype frame.
- The UI colour scheme and layout are taken from `user_stories/ui-preview-3.html`
  (phosphor-green on dark background, CRT scanline overlay, Share Tech Mono font
  approximated by a monospace system font on Android).
- Slider orientation in the prototype is vertical; MAUI's standard `Slider` control
  will be used rotated, or replaced with a custom SkiaSharp touch-drag control if MAUI
  does not support vertical sliders natively.
- Localisation is Russian only; no other locale is required.
- The SkiaSharp draw-loop exemption from unit tests (Constitution Principle V) applies
  to the radar and infrasound canvases; all ViewModels and anomaly-logic services are
  fully covered by unit tests.
- Boundary values for anomaly ranges are inclusive on both ends.
