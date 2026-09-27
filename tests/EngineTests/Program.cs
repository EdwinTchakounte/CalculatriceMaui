using CalculatriceMaui.Core;

// Tests du moteur de calcul (exécutables sans émulateur) : dotnet run --project tests/EngineTests
int passed = 0, failed = 0;

void Check(string keys, string expectedDisplay, string? expectedExpr = null)
{
    var e = new CalculatorEngine();
    foreach (var token in keys.Split(' ', StringSplitOptions.RemoveEmptyEntries))
    {
        switch (token)
        {
            case "C": e.Clear(); break;
            case "DEL": e.Backspace(); break;
            case "NEG": e.ToggleSign(); break;
            case "%": e.Percent(); break;
            case "=": e.Evaluate(); break;
            case ",": e.InputDecimalPoint(); break;
            case "SQRT": e.SquareRoot(); break;
            case "SQR": e.Square(); break;
            case "INV": e.Reciprocal(); break;
            case "PI": e.InputPi(); break;
            case "+" or "−" or "×" or "÷" or "^": e.SetOperator(token); break;
            default: foreach (var c in token) e.InputDigit(c); break;
        }
    }
    string disp = e.Display.Replace(' ', ' ');
    bool ok = disp == expectedDisplay && (expectedExpr == null || e.Expression == expectedExpr);
    if (ok) passed++; else failed++;
    Console.WriteLine($"{(ok ? "OK  " : "FAIL")} [{keys}] -> \"{disp}\" | \"{e.Expression}\"" +
                      (ok ? "" : $"   (attendu \"{expectedDisplay}\" | \"{expectedExpr}\")"));
}

Check("12 + 3 =", "15", "12 + 3 =");
Check("12 + 3 × 2 =", "30", "15 × 2 =");
Check("0 , 1 + 0 , 2 =", "0,3");
Check("1 ÷ 3 × 3 =", "1");
Check("1 ÷ 3 = × 3 =", "1");
Check("10 − 25 =", "-15");
Check("7 ÷ 0 =", "Division par zéro impossible", "7 ÷ 0 =");
Check("7 ÷ 0 = 5", "5", "");
Check("7 ÷ 0 = + 5 =", "5", "5 =");  // après erreur, l'opérateur est ignoré
Check("7 ÷ 0 = C", "0", "");
Check("5 INV", "0,2");
Check("0 INV", "Division par zéro impossible", "1/(0)");
Check("123 DEL", "12");
Check("1 DEL", "0");
Check("12 + 3 = DEL", "15");
Check("12 NEG", "-12");
Check("12 NEG NEG", "12");
Check("12 NEG DEL DEL", "0");
Check("200 + 10 %", "20");
Check("200 + 10 % =", "220", "200 + 20 =");
Check("50 %", "0,5");
Check("2 + 3 = =", "8");
Check("2 + ×  3 =", "6", "2 × 3 =");
Check("1 , , 5", "1,5");
Check(", 5", "0,5");
Check("0 0 0", "0");
Check("1234567", "1 234 567");
Check("1234567890123456789", "123 456 789 012 345");
Check("9 SQRT", "3", "√(3)".Replace("3", "9"));
Check("12 + 9 SQRT × 2 =", "30");
Check("4 NEG SQRT", "Racine d'un nombre négatif");
Check("12 SQR", "144");
Check("2 ^ 10 =", "1 024");
Check("PI", "3,14159265359");
Check("99999999999 × 99999999999 =", "9,9999999998E+21");
Check("99999999999 × 99999999999 = SQR SQR", "Dépassement de capacité");
Check("5 =", "5", "5 =");
Check("5 + 5 = 3", "3", "");
Check("2 , 5 × 4 =", "10");
Check("0 NEG", "0");

Console.WriteLine($"\n{passed} réussis, {failed} échoués");
return failed == 0 ? 0 : 1;
