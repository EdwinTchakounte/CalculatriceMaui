using CalculatriceMaui.Core;

namespace CalculatriceMaui;

public partial class MainPage : ContentPage
{
    private readonly CalculatorEngine _engine = new();
    private bool? _isLandscape;

    public MainPage()
    {
        InitializeComponent();
        ApplyOrientation(landscape: false);
        UpdateDisplay();
    }

    // =====================================================================
    //  Gestionnaires d'événements des touches
    // =====================================================================

    private void OnDigitClicked(object? sender, EventArgs e)
    {
        if (sender is Button { Text: { Length: 1 } text })
            _engine.InputDigit(text[0]);
        UpdateDisplay();
    }

    private void OnDecimalClicked(object? sender, EventArgs e)
    {
        _engine.InputDecimalPoint();
        UpdateDisplay();
    }

    private void OnOperatorClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: string op })
            _engine.SetOperator(op);
        UpdateDisplay();
    }

    private void OnEqualsClicked(object? sender, EventArgs e)
    {
        _engine.Evaluate();
        UpdateDisplay();
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        _engine.Clear();
        UpdateDisplay();
    }

    private void OnBackspaceClicked(object? sender, EventArgs e)
    {
        _engine.Backspace();
        UpdateDisplay();
    }

    private void OnToggleSignClicked(object? sender, EventArgs e)
    {
        _engine.ToggleSign();
        UpdateDisplay();
    }

    private void OnPercentClicked(object? sender, EventArgs e)
    {
        _engine.Percent();
        UpdateDisplay();
    }

    // --- Fonctions avancées -------------------------------------------------

    private void OnSqrtClicked(object? sender, EventArgs e)       { _engine.SquareRoot(); UpdateDisplay(); }
    private void OnSquareClicked(object? sender, EventArgs e)     { _engine.Square();     UpdateDisplay(); }
    private void OnReciprocalClicked(object? sender, EventArgs e) { _engine.Reciprocal(); UpdateDisplay(); }
    private void OnPiClicked(object? sender, EventArgs e)         { _engine.InputPi();    UpdateDisplay(); }

    private void OnSciToggled(object? sender, ToggledEventArgs e)
    {
        SciPanel.IsVisible = e.Value;
        // Le pavé change de taille : les polices sont recalculées via SizeChanged.
    }

    // =====================================================================
    //  Affichage
    // =====================================================================

    private void UpdateDisplay()
    {
        ExpressionLabel.Text = string.IsNullOrEmpty(_engine.Expression) ? " " : _engine.Expression;
        ResultLabel.Text = _engine.Display;
        ResultLabel.TextColor = _engine.HasError
            ? AppColor("ErrorText")
            : AppColor("TextPrimary");

        FitResultFont();
        HighlightActiveOperator();

        // Garde la fin de l'opération visible si elle dépasse la largeur.
        Dispatcher.Dispatch(async () =>
        {
            try { await ExpressionScroll.ScrollToAsync(ExpressionLabel, ScrollToPosition.End, false); }
            catch { /* la vue n'est pas encore mesurée */ }
        });
    }

    /// <summary>
    /// Réduit la taille du résultat pour qu'il tienne toujours sur une ligne,
    /// quelle que soit la largeur de l'écran ou la longueur du nombre.
    /// </summary>
    private void FitResultFont()
    {
        double cardHeight = DisplayCard.Height > 0 ? DisplayCard.Height : 160;
        double available = DisplayCard.Width > 0 ? DisplayCard.Width - 40 : 300;

        // Taille maximale proportionnelle à la hauteur de la carte.
        double max = Math.Clamp(cardHeight * 0.42, 30, 72);
        if (_engine.HasError) max = Math.Min(max, 30);

        int length = Math.Max(1, ResultLabel.Text?.Length ?? 1);
        double fit = available / (length * 0.62);   // ≈ largeur moyenne d'un chiffre

        ResultLabel.FontSize = Math.Clamp(Math.Min(max, fit), 16, 72);
        ExpressionLabel.FontSize = Math.Clamp(ResultLabel.FontSize * 0.38, 14, 22);
    }

    private void HighlightActiveOperator()
    {
        var active = _engine.ActiveOperator;
        foreach (var button in new[] { PlusButton, MinusButton, MultiplyButton, DivideButton, PowerButton })
        {
            if ((string)button.CommandParameter == active)
            {
                button.BackgroundColor = AppColor("TextPrimary");
                button.TextColor = AppColor("Accent");
            }
            else
            {
                // Retour aux valeurs définies par le style.
                button.ClearValue(Button.BackgroundColorProperty);
                button.ClearValue(Button.TextColorProperty);
            }
        }
    }

    // =====================================================================
    //  Adaptation à la taille d'écran et à l'orientation
    // =====================================================================

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width <= 0 || height <= 0) return;

        bool landscape = width > height;
        if (landscape != _isLandscape)
            ApplyOrientation(landscape);

        FitResultFont();
    }

    /// <summary>
    /// Portrait : barre / affichage / fonctions / pavé empilés verticalement.
    /// Paysage  : affichage et fonctions à gauche, pavé à droite.
    /// </summary>
    private void ApplyOrientation(bool landscape)
    {
        _isLandscape = landscape;
        var auto = GridLength.Auto;

        RootGrid.RowDefinitions.Clear();
        RootGrid.ColumnDefinitions.Clear();

        if (landscape)
        {
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.25, GridUnitType.Star)));
            RootGrid.RowDefinitions.Add(new RowDefinition(auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));
            RootGrid.RowDefinitions.Add(new RowDefinition(auto));

            Place(TopBar, row: 0, col: 0);
            Place(DisplayCard, row: 1, col: 0);
            Place(SciPanel, row: 2, col: 0);
            Place(Keypad, row: 0, col: 1, rowSpan: 3);
        }
        else
        {
            RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
            RootGrid.RowDefinitions.Add(new RowDefinition(auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));
            RootGrid.RowDefinitions.Add(new RowDefinition(auto));
            RootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(2.6, GridUnitType.Star)));

            Place(TopBar, row: 0, col: 0);
            Place(DisplayCard, row: 1, col: 0);
            Place(SciPanel, row: 2, col: 0);
            Place(Keypad, row: 3, col: 0);
        }
    }

    private static Color AppColor(string key) =>
        Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
            ? color
            : Colors.White;

    private static void Place(View view, int row, int col, int rowSpan = 1)
    {
        Grid.SetRow(view, row);
        Grid.SetColumn(view, col);
        Grid.SetRowSpan(view, rowSpan);
    }

    /// <summary>Ajuste la taille du texte des touches à la taille réelle des cellules.</summary>
    private void OnKeypadSizeChanged(object? sender, EventArgs e)
    {
        if (Keypad.Width <= 0 || Keypad.Height <= 0) return;

        double cellHeight = (Keypad.Height - 4 * Keypad.RowSpacing) / 5;
        double cellWidth = (Keypad.Width - 3 * Keypad.ColumnSpacing) / 4;
        double cell = Math.Min(cellHeight, cellWidth);

        Resources["KeyFontSize"] = Math.Clamp(cell * 0.40, 16, 34);
        Resources["SciFontSize"] = Math.Clamp(cell * 0.28, 14, 20);

        FitResultFont();
    }
}
