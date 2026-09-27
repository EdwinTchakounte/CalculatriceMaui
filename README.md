# Calculatrice .NET MAUI

**Auteur : TCHAMBA TCHAKOUNTE Edwin** · [Dépôt](https://github.com/EdwinTchakounte/CalculatriceMaui) · [Télécharger l’APK](https://github.com/EdwinTchakounte/CalculatriceMaui/releases/latest)

Application de calculatrice mobile développée avec **.NET MAUI** (Single Project) dans le cadre de l'Activité n° 4 – Atelier de développement mobile.

## Fonctionnalités

**Opérations de base**
- Addition, soustraction, multiplication, division (enchaînement de gauche à droite : `12 + 3 × 2 = 30`)
- Saisie des nombres décimaux (virgule), limite de 15 chiffres
- `C` : remise à zéro totale — `⌫` : effacement du dernier caractère
- `±` : changement de signe — `%` : pourcentage (`200 + 10 %` → `200 + 20`)
- Division par zéro gérée sans plantage (message « Division par zéro impossible »)
- Opération en cours affichée au-dessus du résultat (`12 + 3 =`)
- Appuis répétés sur `=` : répète la dernière opération

**Opérations avancées** (interrupteur « Scientifique »)
- `√x`, `x²`, `xʸ`, `1/x`, `π`
- Erreurs gérées : racine d'un nombre négatif, dépassement de capacité

**Interface**
- Disposition adaptée au **portrait** et au **paysage** (réorganisation de la Grid racine)
- Taille des touches et du résultat recalculée selon l'écran : aucun débordement sur petit écran
- Opérateur actif mis en évidence, retour visuel à l'appui, thème sombre
- Calculs en `decimal` : `0,1 + 0,2 = 0,3` (pas d'erreur d'arrondi binaire)

## Layouts utilisés (6 sur la même page)

| Layout | Rôle |
|---|---|
| `Grid` | Structure racine réorganisée selon l'orientation, et pavé 4 × 5 aux cellules proportionnelles |
| `HorizontalStackLayout` | Aligne l'intitulé « Scientifique » et son interrupteur |
| `Border` | Carte arrondie qui encadre l'écran d'affichage |
| `VerticalStackLayout` | Empile l'opération en cours au-dessus du résultat |
| `ScrollView` | Défilement horizontal d'une opération trop longue |
| `FlexLayout` | Touches scientifiques qui passent à la ligne selon la largeur disponible |

## Structure

```
CalculatriceMaui.sln
├── CalculatriceMaui/                 Projet MAUI (Single Project)
│   ├── Core/CalculatorEngine.cs      Logique de calcul (indépendante de l'UI)
│   ├── MainPage.xaml                 Interface
│   ├── MainPage.xaml.cs              Gestionnaires d'événements + adaptation à l'écran
│   ├── Resources/Styles/             Couleurs et styles des touches
│   └── Platforms/                    Android, iOS, MacCatalyst, Windows
└── tests/EngineTests/                40 tests du moteur de calcul (console)
```

## APK Android

- **Automatique** : à chaque `git push`, GitHub Actions génère l'APK → onglet *Releases* du dépôt → `Calculatrice.apk`.
- **Sous Ubuntu** : `./build-apk.sh` (installe .NET 9, Java 17, la charge MAUI Android et le SDK Android, puis produit `Calculatrice.apk`).

## Lancer le projet

Prérequis : SDK .NET 9 et la charge de travail MAUI (`dotnet workload install maui`), ou Visual Studio 2022/2026 avec « Développement .NET Multi-platform App UI ».

```bash
# Android (émulateur ou téléphone branché)
dotnet build CalculatriceMaui/CalculatriceMaui.csproj -t:Run -f net9.0-android

# Windows
dotnet build CalculatriceMaui/CalculatriceMaui.csproj -t:Run -f net9.0-windows10.0.19041.0

# Tests du moteur
dotnet run --project tests/EngineTests
```

> Avec le SDK .NET 10, remplacer `net9.0` par `net10.0` dans `CalculatriceMaui.csproj` si nécessaire.
