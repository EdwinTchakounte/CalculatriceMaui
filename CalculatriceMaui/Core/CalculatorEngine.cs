using System.Globalization;
using System.Text;

namespace CalculatriceMaui.Core;

/// <summary>
/// Moteur de calcul indépendant de l'interface (aucune dépendance MAUI).
/// Fonctionne comme une calculatrice classique : les opérations sont
/// enchaînées de gauche à droite (12 + 3 × 2 = 30).
/// Les calculs utilisent le type decimal pour éviter les erreurs
/// d'arrondi binaire (0,1 + 0,2 = 0,3).
/// </summary>
public class CalculatorEngine
{
    public const int MaxDigits = 15;

    private string _entry = "0";          // saisie en cours (séparateur interne '.')
    private decimal? _accumulator;        // résultat partiel
    private string? _pendingOperator;     // opérateur en attente (+ − × ÷ ^)
    private bool _startNewEntry = true;   // le prochain chiffre remplace la saisie
    private bool _justEvaluated;          // on vient d'appuyer sur « = »
    private string? _lastOperator;        // pour répéter « = »
    private decimal _lastOperand;
    private decimal? _exact;              // valeur exacte (non arrondie) d'un résultat affiché
    private bool _hasOperand;             // l'utilisateur a fourni un second opérande

    /// <summary>Texte principal (résultat ou saisie courante).</summary>
    public string Display { get; private set; } = "0";

    /// <summary>Opération en cours, affichée au-dessus du résultat.</summary>
    public string Expression { get; private set; } = "";

    /// <summary>Vrai lorsqu'une erreur (division par zéro, dépassement…) est affichée.</summary>
    public bool HasError { get; private set; }

    /// <summary>Opérateur en attente d'un second opérande (pour surligner la touche), sinon null.</summary>
    public string? ActiveOperator => HasError || _hasOperand || _justEvaluated ? null : _pendingOperator;

    public CalculatorEngine() => Refresh();

    // ------------------------------------------------------------------ Saisie

    public void InputDigit(char digit)
    {
        if (digit < '0' || digit > '9') return;
        if (HasError) Clear();

        if (_startNewEntry)
        {
            if (_justEvaluated) ResetOperation();
            _entry = digit.ToString();
            _startNewEntry = false;
            _exact = null;
        }
        else
        {
            if (CountDigits(_entry) >= MaxDigits) return;       // limite de saisie
            _entry = _entry is "0" or "-0" ? _entry.Replace("0", "") + digit : _entry + digit;
        }
        _hasOperand = true;
        Refresh();
    }

    public void InputDecimalPoint()
    {
        if (HasError) Clear();

        if (_startNewEntry)
        {
            if (_justEvaluated) ResetOperation();
            _entry = "0.";
            _startNewEntry = false;
            _exact = null;
        }
        else if (!_entry.Contains('.'))
        {
            if (CountDigits(_entry) >= MaxDigits) return;
            _entry += ".";
        }
        _hasOperand = true;
        Refresh();
    }

    public void InputPi()
    {
        if (HasError) Clear();
        if (_justEvaluated) ResetOperation();
        _exact = 3.1415926535897932384626433833m;
        _entry = ToRaw(_exact.Value);
        _startNewEntry = true;
        _hasOperand = true;
        Refresh();
    }

    // -------------------------------------------------------------- Opérations

    /// <param name="op">"+", "−", "×", "÷" ou "^"</param>
    public void SetOperator(string op)
    {
        if (HasError) return;

        // Changement d'opérateur sans nouvelle saisie : on remplace simplement.
        if (!_hasOperand && _pendingOperator != null && !_justEvaluated)
        {
            _pendingOperator = op;
            Expression = $"{Format(_accumulator!.Value)} {op}";
            Refresh(keepExpression: true);
            return;
        }

        decimal value = CurrentValue;

        if (_pendingOperator != null && _accumulator.HasValue && !_justEvaluated)
        {
            if (!TryCompute(_accumulator.Value, value, _pendingOperator, out var result))
                return;                                         // erreur déjà affichée
            _accumulator = result;
        }
        else
        {
            _accumulator = value;
        }

        _pendingOperator = op;
        _justEvaluated = false;
        _startNewEntry = true;
        _hasOperand = false;
        _exact = _accumulator;
        _entry = ToRaw(_accumulator.Value);
        Expression = $"{Format(_accumulator.Value)} {op}";
        Refresh(keepExpression: true);
    }

    public void Evaluate()
    {
        if (HasError) return;

        decimal left, right;
        string op;

        if (_justEvaluated && _lastOperator != null)
        {
            // Appuis répétés sur « = » : on rejoue la dernière opération.
            left = CurrentValue;
            right = _lastOperand;
            op = _lastOperator;
        }
        else if (_pendingOperator != null && _accumulator.HasValue)
        {
            left = _accumulator.Value;
            right = CurrentValue;
            op = _pendingOperator;
        }
        else
        {
            Expression = $"{Format(CurrentValue)} =";
            _justEvaluated = true;
            _startNewEntry = true;
            Refresh(keepExpression: true);
            return;
        }

        string expr = $"{Format(left)} {op} {Format(right)} =";
        if (!TryCompute(left, right, op, out var result))
        {
            Expression = expr;
            return;
        }

        _lastOperator = op;
        _lastOperand = right;
        _accumulator = null;
        _pendingOperator = null;
        _exact = result;
        _entry = ToRaw(result);
        _justEvaluated = true;
        _startNewEntry = true;
        _hasOperand = false;
        Expression = expr;
        Refresh(keepExpression: true);
    }

    // ------------------------------------------------------ Édition / Effacement

    /// <summary>Remise à zéro totale (touche C).</summary>
    public void Clear()
    {
        ResetOperation();
        HasError = false;
        Refresh();
    }

    /// <summary>Efface le dernier caractère saisi (touche ⌫).</summary>
    public void Backspace()
    {
        if (HasError) { Clear(); return; }
        if (_startNewEntry) return;   // on n'efface pas un résultat calculé

        _exact = null;
        _entry = _entry.Length > 1 ? _entry[..^1] : "0";
        if (_entry is "-" or "" or "-0") _entry = "0";
        Refresh();
    }

    /// <summary>Changement de signe (touche ±).</summary>
    public void ToggleSign()
    {
        if (HasError) return;
        if (_entry == "0" || _entry == "0.") return;
        _entry = _entry.StartsWith('-') ? _entry[1..] : "-" + _entry;
        if (_exact.HasValue) _exact = -_exact.Value;
        _hasOperand = true;
        if (_justEvaluated) { _accumulator = null; _pendingOperator = null; }
        Refresh(keepExpression: true);
    }

    /// <summary>
    /// Pourcentage : avec + ou −, calcule le pourcentage de l'opérande gauche
    /// (200 + 10 % → 200 + 20) ; sinon divise simplement par 100.
    /// </summary>
    public void Percent()
    {
        if (HasError) return;
        decimal value = CurrentValue;
        decimal result = (_pendingOperator is "+" or "−" && _accumulator.HasValue && !_justEvaluated)
            ? _accumulator.Value * value / 100m
            : value / 100m;
        SetEntryFromResult(result);
    }

    // --------------------------------------------------------- Fonctions avancées

    public void SquareRoot()
    {
        if (HasError) return;
        decimal value = CurrentValue;
        if (value < 0) { Expression = $"√({Format(value)})"; SetError("Racine d'un nombre négatif"); return; }
        SetEntryFromResult((decimal)Math.Sqrt((double)value), $"√({Format(value)})");
    }

    public void Square()
    {
        if (HasError) return;
        decimal value = CurrentValue;
        try { SetEntryFromResult(value * value, $"sqr({Format(value)})"); }
        catch (OverflowException) { Expression = $"sqr({Format(value)})"; SetError("Dépassement de capacité"); }
    }

    public void Reciprocal()
    {
        if (HasError) return;
        decimal value = CurrentValue;
        if (value == 0) { Expression = $"1/({Format(value)})"; SetError("Division par zéro impossible"); return; }
        SetEntryFromResult(1m / value, $"1/({Format(value)})");
    }

    // ------------------------------------------------------------------ Interne

    private decimal CurrentValue => _exact ??
        decimal.Parse(_entry.EndsWith('.') ? _entry + "0" : _entry, NumberStyles.Float, CultureInfo.InvariantCulture);

    private bool TryCompute(decimal a, decimal b, string op, out decimal result)
    {
        result = 0;
        try
        {
            switch (op)
            {
                case "+": result = a + b; break;
                case "−": result = a - b; break;
                case "×": result = a * b; break;
                case "÷":
                    if (b == 0) { SetError("Division par zéro impossible"); return false; }
                    result = a / b; break;
                case "^":
                    double p = Math.Pow((double)a, (double)b);
                    if (double.IsNaN(p) || double.IsInfinity(p)) { SetError("Résultat non défini"); return false; }
                    result = (decimal)p; break;
                default: return false;
            }
            return true;
        }
        catch (OverflowException)
        {
            SetError("Dépassement de capacité");
            return false;
        }
    }

    private void SetEntryFromResult(decimal value, string? expressionSuffix = null)
    {
        _exact = value;
        _entry = ToRaw(value);
        _startNewEntry = true;
        _hasOperand = true;
        if (expressionSuffix != null)
        {
            Expression = _pendingOperator != null && _accumulator.HasValue && !_justEvaluated
                ? $"{Format(_accumulator.Value)} {_pendingOperator} {expressionSuffix}"
                : expressionSuffix;
        }
        if (_justEvaluated) { _accumulator = null; _pendingOperator = null; _lastOperator = null; }
        Refresh(keepExpression: true);
    }

    private void SetError(string message)
    {
        HasError = true;
        Display = message;
        _accumulator = null;
        _pendingOperator = null;
        _lastOperator = null;
        _entry = "0";
        _exact = null;
        _hasOperand = false;
        _startNewEntry = true;
        _justEvaluated = false;
    }

    private void ResetOperation()
    {
        _entry = "0";
        _exact = null;
        _hasOperand = false;
        _accumulator = null;
        _pendingOperator = null;
        _lastOperator = null;
        _startNewEntry = true;
        _justEvaluated = false;
        Expression = "";
    }

    private void Refresh(bool keepExpression = true)
    {
        if (!keepExpression) Expression = "";
        Display = FormatEntry(_entry);
    }

    private static int CountDigits(string s) => s.Count(char.IsDigit);

    // --------------------------------------------------------------- Formatage

    /// <summary>Représentation interne arrondie (séparateur '.').</summary>
    private static string ToRaw(decimal value)
    {
        value = Math.Round(value, 12, MidpointRounding.AwayFromZero);
        return value.ToString("0.############", CultureInfo.InvariantCulture);
    }

    /// <summary>Formate un nombre pour l'affichage : espaces des milliers, virgule décimale.</summary>
    public static string Format(decimal value, bool raw = false)
    {
        string s = ToRaw(value);
        return raw ? s : FormatEntry(s);
    }

    private static string FormatEntry(string raw)
    {
        // Très grands ou très petits nombres : notation scientifique.
        if (decimal.TryParse(raw.EndsWith('.') ? raw + "0" : raw, NumberStyles.Float,
                CultureInfo.InvariantCulture, out var v))
        {
            decimal abs = Math.Abs(v);
            if (abs >= 1e16m)
                return ((double)v).ToString("0.###########E+0", CultureInfo.InvariantCulture).Replace('.', ',');
        }

        bool negative = raw.StartsWith('-');
        string body = negative ? raw[1..] : raw;
        int dot = body.IndexOf('.');
        string intPart = dot >= 0 ? body[..dot] : body;
        string fracPart = dot >= 0 ? body[dot..].Replace('.', ',') : "";

        var sb = new StringBuilder();
        for (int i = 0; i < intPart.Length; i++)
        {
            if (i > 0 && (intPart.Length - i) % 3 == 0) sb.Append(' '); // espace fine insécable
            sb.Append(intPart[i]);
        }
        return (negative ? "-" : "") + sb + fracPart;
    }
}
