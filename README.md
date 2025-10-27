# Penempatan Objek AR – Panduan Setup & Menjalankan

Repositori ini berisi proyek Unity 6 (AR Foundation) untuk menempatkan objek di AR pada Android (ARCore) dan iOS (ARKit).

## Persyaratan

- Unity Hub (terbaru)
- Unity Editor 6000.2.6f2 (Unity 6)
  - Instal melalui Unity Hub. Jika Anda membuka proyek di Hub, Hub akan menawarkan untuk memasang versi yang sesuai.
- IDE C#: Visual Studio 2022 (dengan Unity workload) atau JetBrains Rider
- Modul platform (pasang dari Unity Hub saat menambahkan Editor 6000.2.6f2):
  - Android Build Support (Android SDK & NDK Tools, OpenJDK)
  - iOS Build Support (hanya di macOS, untuk build ke iPhone/iPad)

Proyek ini menggunakan paket utama berikut (sudah direferensikan di `Packages/manifest.json`):
- AR Foundation: 6.2.0
- ARCore XR Plugin (Android): 6.2.0
- ARKit XR Plugin (iOS): 6.2.0
- Input System: 1.14.2
- URP (Universal Render Pipeline): 17.2.0
- XR Simulation Environments: dibundel di folder `ContentPackages/`

## Mendapatkan Proyek

Clone dengan Git (HTTPS):

```powershell
# Windows PowerShell
cd C:\path\to\your\projects
git clone https://github.com/Syhri/AR_Object_Placement.git
cd AR_Object_Placement
```

Atau dengan SSH:

```bash
git clone git@github.com:Syhri/AR_Object_Placement.git
```

Buka folder ini di Unity Hub dan pilih Editor versi 6000.2.6f2 jika diminta.

## Pertama Kali Membuka

1. Biarkan Unity mengimpor aset dan menyelesaikan dependensi paket (pertama kali bisa memakan beberapa menit).
2. Jika Unity meminta untuk mengaktifkan Input System baru, setujui lalu biarkan Unity restart.
3. URP mungkin meminta membuat/menetapkan pipeline asset; jika Anda melihat material berwarna pink, buka:
   - Edit → Render Pipeline → Universal Render Pipeline → Upgrade Project Materials

## Pengaturan XR/AR (Verifikasi)

Proyek sudah menyertakan AR Foundation dan plugin XR platform. Verifikasi di Project Settings:

- Project Settings → XR Plug-in Management:
  - Android: aktifkan ARCore
  - iOS (di macOS): aktifkan ARKit
- Player → Other Settings:
  - Active Input Handling = Input System Package (New)
  - Scripting Backend = IL2CPP (disarankan untuk build ke perangkat)
  - Target Architectures (Android) = ARM64

## Menjalankan di Editor (XR Simulation)

Anda bisa pratinjau logika AR menggunakan XR Simulation:

- Window → XR → Simulation
- Pilih environment (proyek ini menyertakan environment simulasi di `ContentPackages/`)
- Buka scene `Assets/Scenes/SampleScene.unity`
- Tekan Play

Catatan: Sensor perangkat dan deteksi bidang (plane) akan disimulasikan; untuk pengujian AR yang sebenarnya, lakukan build ke perangkat.

## Build ke Android (ARCore)

Prasyarat: Perangkat Android yang mendukung ARCore; Developer options dan USB debugging aktif.

1. File → Build Settings → Android → Switch Platform
2. Scenes In Build: tambahkan `Assets/Scenes/SampleScene.unity`
3. Player Settings:
   - Identification: setel Package Name (mis. `com.yourcompany.arobject`)
   - Other Settings → Minimum API Level: Android 8.0 (API 26) atau lebih tinggi
   - Scripting Backend: IL2CPP; Target Architecture: hanya ARM64
4. Sambungkan perangkat via USB (atau gunakan ADB nirkabel), lalu klik Build And Run

Jika muncul error SDK/NDK/JDK tidak ditemukan, buka kembali Unity Hub → Installs → 6000.2.6f2 → Add modules → pastikan Android Build Support + SDK/NDK + OpenJDK terpasang.

## Build ke iOS (ARKit)

Prasyarat: macOS dengan Xcode terpasang, iPhone/iPad yang mendukung ARKit.

1. Di macOS, buka proyek dengan Unity 6000.2.6f2
2. File → Build Settings → iOS → Switch Platform
3. Scenes In Build: tambahkan `Assets/Scenes/SampleScene.unity`
4. Player Settings: setel Bundle Identifier (mis. `com.yourcompany.arobject`)
5. Build → buka proyek Xcode yang dihasilkan
6. Di Xcode: pilih Team Anda, atur signing & capabilities, lalu Build & Run ke perangkat

## Catatan Folder

- `Assets/Scenes/SampleScene.unity` – scene contoh yang perlu dimasukkan ke Build Settings
- `ContentPackages/` – berisi XR Simulation environments; pastikan folder ini ikut saat berbagi repo

## Pemecahan Masalah (Troubleshooting)

- Unity versi tidak sesuai
  - Gunakan Unity Hub untuk memasang `6000.2.6f2`. Membuka dengan versi mayor berbeda (mis. 2022/2023/6000.x minor lain) dapat membuat paket bermasalah.
- Gagal restore paket
  - Window → Package Manager → klik “Resolve”/coba ulang; periksa koneksi internet; hapus folder `Library/` lalu buka ulang proyek jika perlu.
- Material pink / masalah URP
  - Edit → Render Pipeline → URP → Upgrade Project Materials; tetapkan URP Pipeline Asset di Project Settings → Graphics.
- Android SDK/NDK/JDK tidak ditemukan
  - Pasang melalui modul Unity Hub untuk versi editor yang sama persis. Hindari mencampur SDK eksternal dengan yang dikelola Hub.
- ARCore tidak terdeteksi di perangkat
  - Pastikan perangkat mendukung ARCore dan Google Play Services for AR terpasang/terbaru.
- Error build/link iOS
  - Pastikan target perangkat nyata (bukan Simulator), provisioning profile sudah diatur di Xcode, dan plugin ARKit aktif di XR Plug-in Management.

## Opsional

- Git LFS tidak wajib untuk repo ini kecuali Anda mulai menambahkan aset biner yang sangat besar (>100 MB). Jika ya, konfigurasikan Git LFS sebelum meng-commit.

---

Jika tim menemui masalah setup, sertakan versi Unity tepat (`6000.2.6f2`), platform, dan screenshot/log. Panduan ini bisa diperluas sesuai kebutuhan.
