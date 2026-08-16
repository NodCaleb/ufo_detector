using Android.App;
using Android.Content.PM;

namespace UfoDetector;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ScreenOrientation = ScreenOrientation.Portrait,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Density | ConfigChanges.UiMode)]
public class MainActivity : MauiAppCompatActivity
{
}
