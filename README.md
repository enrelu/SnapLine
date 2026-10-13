# 🧺 SnapLine

<p align="center">
  <strong>An ultra-lightweight Windows screenshot manager (~53 KB / ~25 MB RAM) with an elegant clothesline floating dock.</strong><br>
  <em>Un gestor de capturas de pantalla ultra-ligero para Windows con un tendedero flotante elegante e interactivo.</em>
</p>

<p align="center">
  <a href="https://github.com/enrelu/SnapLine/releases/latest"><img src="https://img.shields.io/badge/Download-SnapLine.exe-blue?style=for-the-badge&logo=windows" alt="Download SnapLine"></a>
  <img src="https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-blue?style=for-the-badge" alt="Platform">
  <img src="https://img.shields.io/badge/Size-~53%20KB-success?style=for-the-badge" alt="Size">
  <img src="https://img.shields.io/badge/RAM-~25%20MB-brightgreen?style=for-the-badge" alt="RAM">
  <img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" alt="License">
</p>

---

## 🏷️ GitHub Topics & Tags
`csharp` • `wpf` • `windows` • `screenshot` • `snipping-tool` • `lightweight` • `productivity` • `screen-capture` • `drag-and-drop` • `clothesline` • `annotation-tool` • `windows-11`

---

## 📥 Direct Download / Descarga Directa

* **[Download Latest Release (SnapLine.exe / ZIP)](https://github.com/enrelu/SnapLine/releases/latest)**
* **No installation required**: Download `SnapLine.exe`, double-click and it works immediately. Zero dependencies (uses pre-installed native Windows .NET Framework 4.8).

---

## 🌟 Full Feature Matrix / Todas las Características

### 1. ⚡ Ultra-Lightweight Architecture (~53 KB, ~25 MB RAM)
* **Single portable executable** of only **54 KB**.
* Runs on **~25 MB of RAM** and uses **0.0% CPU** in the background.
* Instant startup (< 100 ms) with no heavy bundled browsers (no Chromium/Electron).
* Zero external runtimes or frameworks to install.

### 2. ✂️ Pixel-Perfect Region Snipping
* **Global Hotkey**: Press **`PrintScreen`** (Impr Pant) or **`Ctrl + Shift + S`** anytime.
* **Windows 11 Bypass**: Uses a low-level kernel hook (`WH_KEYBOARD_LL`) that catches `VK_SNAPSHOT` at the driver level and suppresses Windows 11's default Snipping Tool conflict.
* **DPI Scaling Aware**: Full 1:1 physical pixel compensation on high-DPI displays (100%, 125%, 150%, 175%, 200%). No screen zoom or offset bugs.
* **Interactive HUD**: Frozen desktop backdrop, darkened mask, crosshair guides, and live dimension indicator (`Width × Height px`).
* **Quick Dismiss / Peek**: Press `Esc` to cancel, or `Space` / `Tab` to peek at the clothesline without taking a new capture.

### 3. 🧺 Floating Clothesline Dock (Tendedero)
* **100% Transparent**: No bulky boxes or dark rectangles. Only the natural braided twine rope and the clothespins float directly over your desktop.
* **Vector Wooden Clothespins (Pinzas de madera)**: Realistic wooden prongs with warm wood grains, beveled highlights, and metallic central spring coils.
* **Polaroid Photo Cards**: Crisp white photo paper with soft floating drop shadows, random tilt angle (-2° to +2°) for a natural physical feel, and timestamps.
* **20-Item FIFO History**: Keeps your last 20 screenshots handy on the line. Scroll horizontally using your mouse wheel.

### 4. 🖱️ Smart Mouse Hover Gesture (Borde Superior)
* Hover your mouse pointer at the **top edge of your monitor (`Y <= 2 px`) for 1 second** and the clothesline will smoothly drop down from the top edge.
* Perfect for browsing previous screenshots without needing to take a new one.

### 5. 👻 Discreet & Auto-Hiding
* **Silent at Startup**: Starts completely hidden in the background. Only appears when taking a capture or triggering the top-edge gesture.
* **Auto-Hide Inactivity Timer**: Slides back up and hides after **7 seconds** of inactivity, or **4 seconds** after your mouse leaves the clothesline dock.
* **Hover Freeze**: Moving your mouse over any card or the rope pauses the auto-hide timer so you have all the time you need.
* **Instant Hide**: Press `Esc`, click anywhere outside, or click the subtle dismiss button (`×`) on the right end of the rope.

### 6. 🎨 Built-in Annotation Editor
* Click on any card (or hover over the pencil button) to open the full annotation editor.
* Vector tools:
  * 🔲 **Rectangles / Boxes**
  * ⭕ **Circles / Ellipses**
  * ➔ **Directional Arrows** with angled pointer heads
  * 🆃 **Text tool** with high-contrast semi-transparent backing for clarity
* **Color Palette**: Red, Green, Blue, Yellow, White, Black.
* **Stroke Thickness**: Thin (2px), Medium (4px), Thick (8px).
* **Undo History**: Full `Ctrl + Z` undo stack.
* **Save & Copy**: Save updates back to the clothesline and disk, or click **Copy** (`Ctrl + C`) to copy directly to the Windows clipboard (`Clipboard.SetImage`).

### 7. 🚀 Native OLE Drag & Drop
* Click and drag any photo card directly from the clothesline into third-party apps:
  * **Discord**, **Slack**, **WhatsApp**, **Telegram**, **Photoshop**, **web browsers**, or **Windows Explorer folders**.
* Once dropped, the card plays a gentle falling animation and **disappears from the clothesline**.
* Files remain safely saved in `%APPDATA%\SnapLine\Capturas`.

### 8. ❌ Manual Card Discard
* Hover over any card and click the red **`×`** button in the top-right corner to unclip and remove it from the line.

### 9. 🔔 System Tray Companion
* Minimalist icon in the Windows notification area (next to the clock).
* Right-click menu:
  * 📷 *Nueva captura*
  * 🧺 *Ver tendedero de fotos*
  * 📁 *Abrir carpeta de capturas* (opens `%APPDATA%\SnapLine\Capturas` in Explorer)
  * ❌ *Salir de SnapLine*

---

## ⌨️ Quick Reference / Atajos

| Acción | Disparador |
|---|---|
| **Capturar Pantalla** | `PrintScreen` (Impr Pant) o `Ctrl + Shift + S` |
| **Desplegar Cuerda** | Mantener ratón en el borde superior de la pantalla durante 1 segundo |
| **Editar Captura** | Clic en la foto de la cuerda (o botón del lápiz) |
| **Subir a Discord/Slack/Web** | Arrastrar foto desde la cuerda y soltarla en la aplicación |
| **Descolgar Foto** | Clic en la `×` roja de la tarjeta |
| **Ocultar Cuerda** | Pulsar `Esc`, hacer clic fuera o apartar el ratón 4 segundos |

---

## 🛠️ Build from Source / Compilación

The repository includes a portable build script that uses the native Windows compiler with zero external dependencies:

```cmd
# Double click or run in terminal:
build.bat
```

Or manually using Windows MSBuild:
```cmd
"%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" SnapLine.csproj /p:Configuration=Release
```

The compiled standalone executable will be generated at `bin\Release\SnapLine.exe`.

---

## 📂 Source Structure / Estructura del Código

```text
├── App.xaml / App.xaml.cs            # Entry point, tray icon, hover detection, single instance mutex
├── SnapLine.csproj                   # Native MSBuild project configuration
├── Models/
│   └── CaptureItem.cs                # Model for screenshot cards (thumbnails, tilt angle, metadata)
├── Services/
│   ├── CaptureService.cs             # GDI+ screen capture engine & DPI coordinate mapper
│   ├── HotKeyManager.cs              # Low-level WH_KEYBOARD_LL hook bypassing Win11 Snipping Tool
│   └── StorageService.cs             # %APPDATA%\SnapLine storage & 20-item FIFO history manager
├── Windows/
│   ├── SnipperOverlayWindow.xaml     # Full-screen frozen region cropping window
│   ├── ClotheslineWindow.xaml        # Floating transparent clothesline with realistic wooden pegs
│   └── AnnotationEditorWindow.xaml   # Canvas editor for shapes, arrows, text, and clipboard
├── build.bat                         # Portable one-click compilation script
├── run.bat                           # Portable launcher script
├── LICENSE                           # MIT License
└── README.md                         # Documentation
```

---

## 📄 License

This project is licensed under the **MIT License** - see the [`LICENSE`](LICENSE) file for details.
