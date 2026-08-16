namespace UfoDetector.Services;

public interface ISensorTickService
{
    Task StartAsync();
    void Stop();
}
