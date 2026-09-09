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
}
