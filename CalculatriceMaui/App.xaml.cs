namespace CalculatriceMaui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Dark;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new MainPage()) { Title = "Calculatrice" };
#if WINDOWS || MACCATALYST
        // Taille de départ proche d'un téléphone sur ordinateur (redimensionnable).
        window.Width = 420;
        window.Height = 760;
        window.MinimumWidth = 300;
        window.MinimumHeight = 420;
#endif
        return window;
    }
}
