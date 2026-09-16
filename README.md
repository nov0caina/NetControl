# NetControl

Cross-platform network bandwidth diagnostics, device discovery, and ARP traffic management for local networks. The modern successor to SelfishNet, built with .NET 8 and Avalonia UI.

> [!WARNING]
> **Official Repository and Security Notice:** This repository (`https://github.com/nov0caina/SelfishNet`) is the **only official source** for the modern NetControl codebase. Third-party domains (such as `selfishnet.org` and unauthorized mirrors) are not affiliated with this project, distribute unauthorized versions, and may pose malware or security risks. Do not download or execute untrusted binaries from third-party websites.

> [!IMPORTANT]
> **Ethical and Authorized Usage:** NetControl is designed strictly for authorized network diagnostics, administrative troubleshooting, and educational research in controlled laboratory environments. Executing ARP cache redirection or packet interception on networks without explicit, documented permission from the network owner is illegal and unethical.

---

## Modern Architecture & Cross-Platform Support

NetControl has been redesigned from the ground up to remove legacy Windows-only dependencies (.NET Framework 3.5, WinForms, WinPcap, and `user32.dll` P/Invoke calls) in favor of a modern, multi-platform runtime.

| Component | Legacy SelfishNet | NetControl |
| :--- | :--- | :--- |
| **Runtime Framework** | .NET Framework 3.5 (Windows only) | **.NET 8.0 (LTS)** |
| **User Interface** | Windows Forms (WinForms) | **Avalonia UI** (Cross-platform XAML) |
| **Packet Capture Engine** | PcapNet / WinPcap | **SharpPcap** (over native `libpcap` / `Npcap`) |
| **Platform Interoperability** | Native Windows P/Invoke (`user32.dll`, `gdi32.dll`) | **Managed Cross-Platform** APIs |
| **Operating Systems** | Windows XP / 7 / 8 / 10 | **Linux (x64), Windows (x64), macOS (arm64 & x64)** |
| **Packaging** | Manual installer | **Single-file self-contained binaries** |

---

## Standalone Release Packages

Standalone release archives do **not** require the .NET SDK or runtime to be pre-installed. All runtime dependencies and native graphics libraries are bundled directly into the distribution.

### 1. Linux (x86_64)

- **Package:** `NetControl-linux-x64.tar.gz`
- **Prerequisites:**
  - `libpcap` runtime (installed by default on most distributions; on Ubuntu/Debian: `sudo apt install libpcap0.8` or `sudo apt install libpcap-dev`).
- **Execution:**
  - Launch with graphical superuser elevation:
    ```bash
    pkexec ./NetControl
    ```
    or via terminal:
    ```bash
    sudo -E ./NetControl
    ```
  - **Running without sudo (Linux Capabilities):** You can assign network capabilities directly to the binary to run as an unprivileged user:
    ```bash
    sudo setcap cap_net_raw,cap_net_admin=eip ./NetControl
    ./NetControl
    ```
- **Desktop Application Launcher (Optional):**
  The Linux release archive includes `netcontrol.desktop` and `netcontrol.png`. To register the application in your desktop environment (GNOME, KDE, XFCE):
  ```bash
  mkdir -p ~/.local/share/applications ~/.local/share/icons/hicolor/256x256/apps
  cp netcontrol.desktop ~/.local/share/applications/
  cp netcontrol.png ~/.local/share/icons/hicolor/256x256/apps/netcontrol.png
  update-desktop-database ~/.local/share/applications/
  ```
- **IP Packet Forwarding (Kernel):** Enable packet routing so traffic passes uninterrupted through your machine during diagnostics:
  ```bash
  sudo sysctl -w net.ipv4.ip_forward=1
  ```

---

### 2. Windows (x86_64)

- **Package:** `NetControl-windows-x64.zip`
- **Prerequisites:**
  - **Npcap Driver:** Download and install Npcap from [npcap.com](https://npcap.com).
  - **CRITICAL:** During the Npcap installation wizard, you must check the option:
    **"Install Npcap in WinPcap API-compatible Mode"**. This ensures `wpcap.dll` and `packet.dll` are accessible to SharpPcap.
- **Execution:**
  1. Extract `NetControl-windows-x64.zip`.
  2. Right-click on `NetControl.exe`.
  3. Select **"Run as administrator"**. Elevated User Account Control (UAC) permissions are mandatory on Windows to open raw network adapters.

---

### 3. macOS (Apple Silicon arm64 & Intel x64)

- **Packages:**
  - Apple Silicon (M1, M2, M3, M4): `NetControl-macos-arm64.tar.gz`
  - Intel x86_64: `NetControl-macos-x64.tar.gz`
- **Prerequisites:**
  - macOS 11.0 (Big Sur) or later.
  - Native Berkeley Packet Filter (BPF) capture requires root access.
- **Execution:**
  1. Extract the archive:
     ```bash
     tar -xzf NetControl-macos-arm64.tar.gz
     cd dist/osx-arm64
     ```
  2. If macOS Gatekeeper marks the unnotarized binary as quarantined:
     ```bash
     xattr -d com.apple.quarantine ./NetControl
     ```
  3. Grant executable permissions:
     ```bash
     chmod +x ./NetControl
     ```
  4. Launch with root privileges:
     ```bash
     sudo ./NetControl
     ```
- **IP Packet Forwarding:**
  ```bash
  sudo sysctl -w net.inet.ip.forwarding=1
  ```

---

## Cryptographic Verification

Verify the integrity of downloaded distribution packages using SHA-256:

- **Linux / macOS:**
  ```bash
  sha256sum -c SHA256SUMS
  ```
- **Windows (PowerShell):**
  ```powershell
  Get-FileHash .\NetControl-windows-x64.zip -Algorithm SHA256
  ```

---

## Building from Source

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Platform network drivers (`libpcap-dev` on Linux, Npcap on Windows)

### 1. Clone Repository
```bash
git clone https://github.com/nov0caina/SelfishNet.git
cd SelfishNet
```

### 2. Build and Run
```bash
# Build entire solution (produces NetControl binary)
dotnet build SelfishNet.sln

# Run application locally
dotnet run --project SelfishNet/SelfishNet.csproj
```

### 3. Run Automated Tests
The repository includes an automated test suite covering concurrency, rate calculation, OUI lookups, heuristic classification, and boundary conditions:
```bash
dotnet test SelfishNet.sln
```

### 4. Publish Standalone Binaries
To compile single-file, self-contained binaries for target platforms:

```bash
# Linux x64 (produces dist/linux-x64/NetControl)
dotnet publish SelfishNet/SelfishNet.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o dist/linux-x64

# Windows x64 (produces dist/win-x64/NetControl.exe)
dotnet publish SelfishNet/SelfishNet.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o dist/win-x64

# macOS Apple Silicon (produces dist/osx-arm64/NetControl)
dotnet publish SelfishNet/SelfishNet.csproj -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o dist/osx-arm64

# macOS Intel (produces dist/osx-x64/NetControl)
dotnet publish SelfishNet/SelfishNet.csproj -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o dist/osx-x64
```

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

_____________________________________________________________________________________________________________________________________

# NetControl (Guia en Espanol)

Herramienta multiplataforma para diagnostico de ancho de banda, descubrimiento de dispositivos y gestion de trafico ARP en redes locales. El sucesor moderno de SelfishNet, desarrollada con .NET 8 y Avalonia UI.

> [!WARNING]
> **Aviso Oficial de Seguridad:** Este repositorio (`https://github.com/nov0caina/SelfishNet`) es la **unica fuente oficial** para el codigo moderno de NetControl. Los dominios de terceros (como `selfishnet.org` y otros sitios no autorizados) no tienen relacion alguna con este proyecto, redistribuyen versiones no autorizadas y pueden contener malware o riesgos de seguridad. No descargues ni ejecutes binarios procedentes de sitios no verificados.

> [!IMPORTANT]
> **Uso Etico y Responsable:** NetControl esta disenado estrictamente para tareas de diagnostico, administracion de red e investigacion academica en entornos de laboratorio controlados. La manipulacion de tablas ARP o la interceptacion de paquetes en redes sin el consentimiento previo y formal del propietario es ilegal y no etica.

---

## Arquitectura Moderna y Soporte Multiplataforma

NetControl fue redisenado completamente para eliminar las dependencias heredadas exclusivas de Windows (.NET Framework 3.5, WinForms, WinPcap y llamadas P/Invoke a `user32.dll`) reemplazandolas por una arquitectura moderna y verdaderamente multiplataforma.

| Componente | SelfishNet Original (Legacy) | NetControl |
| :--- | :--- | :--- |
| **Framework Base** | .NET Framework 3.5 (Solo Windows) | **.NET 8.0 (LTS)** |
| **Interfaz de Usuario** | Windows Forms (WinForms) | **Avalonia UI** (XAML Multiplataforma) |
| **Motor de Captura** | PcapNet / WinPcap | **SharpPcap** (sobre `libpcap` / `Npcap`) |
| **Interoperabilidad** | P/Invoke nativo (`user32.dll`, `gdi32.dll`) | **Managed Multiplataforma** |
| **Sistemas Soportados**| Windows XP / 7 / 8 / 10 | **Linux (x64), Windows (x64), macOS (arm64 y x64)** |
| **Distribucion** | Instalador tradicional | **Binarios autocontenidos en un unico archivo** |

---

## Paquetes Precompilados (Standalone)

Los paquetes de distribucion precompilados **no** requieren tener instalado el SDK ni el runtime de .NET. Incluyen todas las dependencias y librerias graficas necesarias.

### 1. Linux (x86_64)

- **Paquete:** `NetControl-linux-x64.tar.gz`
- **Prerrequisitos:**
  - Libreria `libpcap` (instalada por defecto en la gran mayoria de distribuciones; en Ubuntu/Debian: `sudo apt install libpcap0.8` o `sudo apt install libpcap-dev`).
- **Ejecucion:**
  - Ejecutar con elevacion grafica:
    ```bash
    pkexec ./NetControl
    ```
    o desde la terminal:
    ```bash
    sudo -E ./NetControl
    ```
  - **Ejecucion sin sudo (Capacidades de Linux):** Puedes asignar las capacidades de red al binario para ejecutarlo como usuario estandar:
    ```bash
    sudo setcap cap_net_raw,cap_net_admin=eip ./NetControl
    ./NetControl
    ```
- **Lanzador de Escritorio (Opcional):**
  El archivo descargable incluye `netcontrol.desktop` y `netcontrol.png`. Para integrarlo en el menu de aplicaciones de tu escritorio (GNOME, KDE, XFCE):
  ```bash
  mkdir -p ~/.local/share/applications ~/.local/share/icons/hicolor/256x256/apps
  cp netcontrol.desktop ~/.local/share/applications/
  cp netcontrol.png ~/.local/share/icons/hicolor/256x256/apps/netcontrol.png
  update-desktop-database ~/.local/share/applications/
  ```
- **Reenvio de Paquetes IP:** Habilita el reenvio en el kernel para permitir el enrutamiento continuo durante el diagnostico:
  ```bash
  sudo sysctl -w net.ipv4.ip_forward=1
  ```

---

### 2. Windows (x86_64)

- **Paquete:** `NetControl-windows-x64.zip`
- **Prerrequisitos:**
  - **Driver Npcap:** Descargar e instalar Npcap desde [npcap.com](https://npcap.com).
  - **IMPORTANTE:** Durante la instalacion, debes marcar obligatoriamente la casilla:
    **"Install Npcap in WinPcap API-compatible Mode"**. Esto provee los controladores `wpcap.dll` y `packet.dll` necesarios para SharpPcap.
- **Ejecucion:**
  1. Descomprime `NetControl-windows-x64.zip`.
  2. Haz clic derecho sobre `NetControl.exe`.
  3. Selecciona **"Ejecutar como administrador"** (es imprescindible contar con permisos elevados UAC para acceder a las interfaces de red en Windows).

---

### 3. macOS (Apple Silicon arm64 e Intel x64)

- **Paquetes:**
  - Apple Silicon (M1, M2, M3, M4): `NetControl-macos-arm64.tar.gz`
  - Intel x86_64: `NetControl-macos-x64.tar.gz`
- **Prerrequisitos:**
  - macOS 11.0 (Big Sur) o superior.
  - Permisos de superusuario para acceso a los dispositivos de captura BPF (Berkeley Packet Filter).
- **Ejecucion:**
  1. Descomprime el paquete:
     ```bash
     tar -xzf NetControl-macos-arm64.tar.gz
     cd dist/osx-arm64
     ```
  2. Si macOS Gatekeeper bloquea el binario por no estar notarizado:
     ```bash
     xattr -d com.apple.quarantine ./NetControl
     ```
  3. Asegura permisos de ejecucion:
     ```bash
     chmod +x ./NetControl
     ```
  4. Ejecuta con permisos de superusuario:
     ```bash
     sudo ./NetControl
     ```
- **Reenvio de Paquetes IP:**
  ```bash
  sudo sysctl -w net.inet.ip.forwarding=1
  ```

---

## Verificacion de Integridad Criptografica

Verifica la autenticidad e integridad de los paquetes descargados con SHA-256:

- **Linux / macOS:**
  ```bash
  sha256sum -c SHA256SUMS
  ```
- **Windows (PowerShell):**
  ```powershell
  Get-FileHash .\NetControl-windows-x64.zip -Algorithm SHA256
  ```

---

## Compilacion desde el Codigo Fuente

### Prerrequisitos
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Librerias de captura del sistema (`libpcap-dev` en Linux, Npcap en Windows)

### 1. Clonar el Repositorio
```bash
git clone https://github.com/nov0caina/SelfishNet.git
cd SelfishNet
```

### 2. Compilar y Ejecutar
```bash
# Compilar la solucion completa (genera el binario NetControl)
dotnet build SelfishNet.sln

# Ejecutar el proyecto en desarrollo
dotnet run --project SelfishNet/SelfishNet.csproj
```

### 3. Ejecutar Pruebas Automatizadas
El repositorio contiene pruebas automatizadas para validar acumuladores concurrentes, calculo de tasas, resolucion OUI y clasificaciones heurísticas:
```bash
dotnet test SelfishNet.sln
```

---

## Licencia

Este proyecto esta bajo la Licencia MIT. Consulta el archivo [LICENSE](LICENSE) para mas informacion.