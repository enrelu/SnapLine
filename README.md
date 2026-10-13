# 🧺 SnapLine (Portafotos)

> **Ultra-lightweight screenshot manager for Windows (~50 KB / ~25 MB RAM) with an elegant clothesline visual dock.**
> *Gestor de capturas de pantalla ultra-ligero para Windows con un tendedero flotante elegante e interactivo.*

[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-blue.svg)](https://microsoft.com)
[![Size](https://img.shields.io/badge/Size-~50%20KB-success.svg)]()
[![RAM](https://img.shields.io/badge/RAM-~25%20MB-brightgreen.svg)]()
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Zero Dependencies](https://img.shields.io/badge/Dependencies-Zero-orange.svg)]()

---

## ✨ Features / Características

* ⚡ **Ultra-Lightweight & Fast**: Single ~50 KB executable, ~25 MB RAM, 0% CPU on idle. Instant startup.
* ✂️ **Precise Region Snipping**: Full-screen frozen overlay with crosshair guides and dimensions badge. Fully DPI-aware (pixel-perfect on 100%, 125%, 150%, 200% scaling).
* 🧺 **Floating Clothesline Dock**:
  * 100% transparent background (no intrusive dark boxes).
  * Photorealistic natural twine rope and vector wooden clothespins (pinzas de madera).
  * Polaroid-style photo cards with natural floating drop shadows and timestamps.
  * Holds history of the **last 20 screenshots**.
* 🖱️ **Smart Hover Gesture**: Hover your mouse at the top edge of your screen (`Y <= 2 px`) for **1 second** to reveal the clothesline on demand.
* 📦 **Auto-Hide & Discreet**: Starts completely hidden. Auto-hides after 4-7 seconds of inactivity, on `Esc`, or when clicking outside.
* 🎨 **Integrated Annotation Editor**: Click any photo on the line to draw boxes, circles, arrows, and text with a color palette and undo/redo (`Ctrl+Z`).
* 🚀 **Native Drag & Drop**: Drag any screenshot directly from the line into **Discord, Slack, WhatsApp, web browsers, Photoshop, or Windows folders**. Dropping unclips and removes it from the clothesline.
* ❌ **Quick Dismiss**: Click the '×' button on any card to remove it from the line.

---

## ⌨️ Shortcuts & Controls / Atajos y Controles

| Action / Acción | Trigger / Atajo |
|---|---|
| **Take Screenshot** / *Nueva Captura* | `PrintScreen` (Impr Pant) or `Ctrl + Shift + S` |
| **Show Clothesline** / *Desplegar Cuerda* | Mantener ratón en el borde superior 1 segundo / Hover top edge 1 sec |
| **Edit Screenshot** / *Editar Captura* | Clic en la foto de la cuerda / Click on card |
| **Drag & Drop** | Arrastrar foto hacia otra app / Drag photo to target app |
| **Remove from line** / *Descolgar foto* | Clic en la '×' roja de la tarjeta / Click card '×' button |
| **Hide Clothesline** / *Ocultar Cuerda* | Tecla `Esc`, clic fuera o botón '×' del tendedero |

---

## 🛠️ Build from Source / Compilación

No external tools or heavy SDKs required. The project compiles natively using the built-in Windows .NET Framework 4.8 engine:

```cmd
# Double click or run:
build.bat
```

Or via command line:
```cmd
"%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" Portafotos.csproj /p:Configuration=Release
```

The resulting binary will be generated at `bin\Release\Portafotos.exe`.

---

## 📂 Project Structure / Estructura del Proyecto

```text
├── App.xaml / App.xaml.cs            # Entry point, tray icon, hover detector, single instance
├── Portafotos.csproj                 # Native MSBuild project definition
├── Models/
│   └── CaptureItem.cs                # Screenshot card model (thumbnail, tilt, metadata)
├── Services/
│   ├── CaptureService.cs             # Screen capture & DPI-aware cropping engine
│   ├── HotKeyManager.cs              # Low-level WH_KEYBOARD_LL hook (bypasses Windows 11 Snipping Tool)
│   └── StorageService.cs             # Local storage manager (%APPDATA%/Portafotos) & 20-item FIFO history
├── Windows/
│   ├── SnipperOverlayWindow.xaml     # Full-screen frozen region selector
│   ├── ClotheslineWindow.xaml        # Floating transparent clothesline dock & wooden pegs
│   └── AnnotationEditorWindow.xaml   # Canvas editor (shapes, arrows, text, palette, clipboard)
├── build.bat                         # Portable one-click build script
├── run.bat                           # Portable launcher script
├── LICENSE                           # MIT License
└── README.md                         # Documentation
```

---

## 📄 License

Distributed under the **MIT License**. See [`LICENSE`](LICENSE) for more details.
