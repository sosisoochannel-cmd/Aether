# Android APK build

The manual GitHub Actions workflow is at .github/workflows/android-apk.yml. It creates the boot scene, registers it in Build Settings, applies the Android identity settings, and asks Unity to build an ARM64 APK.

## Required repository secrets
- UNITY_LICENSE: Unity license file contents accepted by GameCI.
- UNITY_EMAIL: Unity account email, when required by the selected license activation method.
- UNITY_PASSWORD: Unity account password, when required by the selected license activation method.

Never commit these secrets to the repository or put them in workflow files. Do not start the workflow until the Unity license/activation requirements are configured. A failed or unconfigured workflow is not evidence of a working APK build.

When successful, download the artifact named Aether-Android-APK from the Actions run. The current build is an early graybox prototype, not a content-complete game.
