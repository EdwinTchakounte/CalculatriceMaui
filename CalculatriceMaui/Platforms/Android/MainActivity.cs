using Android.App;
using Android.Content.PM;
using Android.OS;

namespace CalculatriceMaui;

// ConfigurationChanges : la rotation ne recrée pas l'activité, l'état de la calculatrice est conservé.
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
