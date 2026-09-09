using UfoDetector.Services;
using UfoDetector.ViewModels;

namespace UfoDetector.Views;

public partial class DetectorPage : ContentPage
{
    private readonly ISensorTickService _tickService;

    public DetectorPage(DetectorViewModel viewModel, ISensorTickService tickService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _tickService = tickService;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e) =>
        await _tickService.StartAsync();

    // Rotated (270°) sliders are laid out horizontally before rotation, so their
    // WidthRequest must track the container's height for them to fill it vertically.
    private void OnVerticalSliderContainerSizeChanged(object? sender, EventArgs e)
    {
        if (sender is not Grid container || container.Height <= 0)
        {
            return;
        }

        if (container == SensitivitySliderContainer)
        {
            SensitivitySlider.WidthRequest = container.Height;
        }
        else if (container == NoiseSuppressionSliderContainer)
        {
            NoiseSuppressionSlider.WidthRequest = container.Height;
        }
    }
}
