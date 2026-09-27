#!/usr/bin/env bash
# Génère l'APK sous Ubuntu : ./build-apk.sh
set -e

# 1. SDK .NET 9 + Java 17
if ! command -v dotnet >/dev/null || ! dotnet --list-sdks | grep -q '^9\.'; then
  wget -q https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel 9.0
  export DOTNET_ROOT="$HOME/.dotnet"; export PATH="$HOME/.dotnet:$PATH"
fi
command -v javac >/dev/null || sudo apt-get install -y openjdk-17-jdk

# 2. Charge de travail MAUI Android
dotnet workload install maui-android

# 3. SDK Android (téléchargé automatiquement dans ~/android-sdk)
ANDROID_SDK="${ANDROID_HOME:-$HOME/android-sdk}"
dotnet build CalculatriceMaui/CalculatriceMaui.csproj -t:InstallAndroidDependencies -f net9.0-android \
  -p:AndroidSdkDirectory="$ANDROID_SDK" -p:AcceptAndroidSDKLicenses=True

# 4. APK signé
dotnet publish CalculatriceMaui/CalculatriceMaui.csproj -f net9.0-android -c Release \
  -p:AndroidSdkDirectory="$ANDROID_SDK" -o out
cp out/*-Signed.apk Calculatrice.apk
echo "APK : $(pwd)/Calculatrice.apk"
