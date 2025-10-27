# AR Object Placement – Setup & Run Guide

This repo contains a Unity 6 (AR Foundation) project for placing objects in AR on Android (ARCore) and iOS (ARKit).

## Requirements

- Unity Hub (latest)
- Unity Editor 6000.2.6f2 (Unity 6)
  - Install via Unity Hub. If you open the project in Hub, it will offer to install the correct version.
- C# IDE: Visual Studio 2022 (with Unity workload) or JetBrains Rider
- Platform modules (install from Unity Hub when adding the Unity 6000.2.6f2 editor):
  - Android Build Support (Android SDK & NDK Tools, OpenJDK)
  - iOS Build Support (only on macOS, for building to iPhone/iPad)

Project uses these key packages (already referenced in Packages/manifest.json):
- AR Foundation: 6.2.0
- ARCore XR Plugin (Android): 6.2.0
- ARKit XR Plugin (iOS): 6.2.0
- Input System: 1.14.2
- URP (Universal Render Pipeline): 17.2.0
- XR Simulation Environments: bundled under `ContentPackages/`

## Get the project

Clone with Git (HTTPS):

```powershell
# Windows PowerShell
cd C:\path\to\your\projects
git clone https://github.com/Syhri/AR_Object_Placement.git
cd AR_Object_Placement
```

Or with SSH:

```bash
git clone git@github.com:Syhri/AR_Object_Placement.git
```

Open the folder in Unity Hub and select Editor version 6000.2.6f2 if prompted.

## First open

1. Let Unity import assets and resolve packages (first open can take a few minutes).
2. If Unity asks to enable the new Input System, accept and let it reload.
3. URP may prompt to create/assign a pipeline asset; if you see pink materials, go to:
   - Edit → Render Pipeline → Universal Render Pipeline → Upgrade Project Materials

## XR/AR settings (verify)

The project already includes AR Foundation + platform XR plugins. Verify these in Project Settings:

- Project Settings → XR Plug-in Management:
  - Android: enable ARCore
  - iOS (on macOS): enable ARKit
- Player → Other Settings:
  - Active Input Handling = Input System Package (New)
  - Scripting Backend = IL2CPP (recommended for device builds)
  - Target Architectures (Android) = ARM64

## Run in Editor (XR Simulation)

You can preview AR logic using XR Simulation:

- Window → XR → Simulation
- Select an environment (the project bundles simulation environments via `ContentPackages/`)
- Open the scene `Assets/Scenes/SampleScene.unity`
- Press Play

Note: Device sensors and actual plane detection will be simulated; for true AR testing, build to a device.

## Build to Android (ARCore)

Prereqs: Android device that supports ARCore; Developer options and USB debugging enabled.

1. File → Build Settings → Android → Switch Platform
2. Scenes In Build: add `Assets/Scenes/SampleScene.unity`
3. Player Settings:
   - Identification: set Package Name (e.g., `com.yourcompany.arobject`)
   - Other Settings → Minimum API Level: Android 8.0 (API 26) or higher
   - Scripting Backend: IL2CPP; Target Architecture: ARM64 only
4. Connect device via USB (or use wireless ADB), then click Build And Run

If you see errors about missing SDK/NDK/JDK, reopen Unity Hub → Installs → 6000.2.6f2 → Add modules → ensure Android Build Support + SDK/NDK + OpenJDK are installed.

## Build to iOS (ARKit)

Prereqs: macOS with Xcode installed, iPhone/iPad that supports ARKit.

1. On macOS, open the project in Unity 6000.2.6f2
2. File → Build Settings → iOS → Switch Platform
3. Scenes In Build: add `Assets/Scenes/SampleScene.unity`
4. Player Settings: set Bundle Identifier (e.g., `com.yourcompany.arobject`)
5. Build → open the generated Xcode project
6. In Xcode: set your Team, signing & capabilities, then Build & Run to device

## Folder notes

- `Assets/Scenes/SampleScene.unity` – sample scene to include in builds
- `ContentPackages/` – contains XR Simulation environments; keep this folder when sharing the repo

## Troubleshooting

- Unity version mismatch
  - Use Unity Hub to install `6000.2.6f2`. Opening with a different major version (e.g., 2022/2023/6000.x other minor) may break packages.
- Package restore errors
  - Window → Package Manager → click “Resolve”/retry; check internet access; remove `Library/` then reopen the project if needed.
- Pink materials / URP issues
  - Edit → Render Pipeline → URP → Upgrade Project Materials; assign a URP Pipeline Asset in Project Settings → Graphics.
- Android SDK/NDK/JDK missing
  - Install via Unity Hub modules for the exact editor version. Avoid mixing external SDKs with Hub-managed ones.
- ARCore not detected on device
  - Ensure the device supports ARCore and Google Play Services for AR is installed/up to date.
- iOS build/link errors
  - Make sure you’re on a real device (not Simulator), with a provisioning profile set in Xcode, and ARKit plugin enabled in XR Plug-in Management.

## Optional

- Git LFS is not required for this repo unless you start adding very large binary assets (>100 MB). If you do, configure Git LFS before committing them.

---

If teammates hit setup issues, share the exact Unity version (`6000.2.6f2`), platform, and a screenshot/log, and we can extend this guide.
