using UfoDetector.Models;

namespace UfoDetector.Services;

public interface ITransitionOrchestrator
{
    void Step(DetectorState state, double elapsedSeconds);
}
