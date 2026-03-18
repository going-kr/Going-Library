<p align="center">
  <img src="icon/Going_logo_blue.png" alt="Going Library" width="120">
</p>

<h1 align="center">Going Library</h1>

<p align="center">
  Industrial HMI/SCADA UI Framework for .NET 8.0
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/Going.UI"><img src="https://img.shields.io/nuget/v/Going.UI?label=Going.UI&color=blue" alt="NuGet"></a>
  <a href="https://www.nuget.org/packages/Going.Basis"><img src="https://img.shields.io/nuget/v/Going.Basis?label=Going.Basis&color=blue" alt="NuGet"></a>
  <a href="https://www.nuget.org/packages/Going.UI.OpenTK"><img src="https://img.shields.io/nuget/v/Going.UI.OpenTK?label=Going.UI.OpenTK&color=blue" alt="NuGet"></a>
  <a href="https://www.nuget.org/packages/Going.UI.Forms"><img src="https://img.shields.io/nuget/v/Going.UI.Forms?label=Going.UI.Forms&color=blue" alt="NuGet"></a>
</p>

---

## Overview

**Going Library** is a C# .NET 8.0 framework for building industrial HMI/SCADA user interfaces. Built on SkiaSharp custom rendering, it provides 40+ industrial controls optimized for embedded touch panels.

### Key Features

- **40+ Industrial Controls** — GoButton, GoLamp, GoDataGrid, GoSlider, GoGauge, GoMeter, GoChart, and more
- **Visual UI Editor** — Design .gud files with drag-and-drop, deploy generated C# code
- **Industrial Communications** — Modbus RTU/TCP, MQTT, LS Electric CNet, Mitsubishi MC
- **Cross-Platform** — Runs on Raspberry Pi (linux-arm64), Windows, and Linux
- **Dark Theme Built-in** — Professional dark UI with customizable themes
- **AI-Powered Development** — [Claude Code Skill](https://github.com/going-kr/going-ui-skill) for AI-assisted HMI development

---

## Packages

| Package | Description | Target |
|---------|-------------|--------|
| **Going.UI** | Platform-independent UI core (controls, containers, themes, design) | `net8.0` |
| **Going.UI.OpenTK** | OpenTK adapter for embedded/Raspberry Pi | `net8.0` |
| **Going.UI.Forms** | WinForms adapter for Windows desktop | `net8.0-windows` |
| **Going.Basis** | Communications & utilities (Modbus, MQTT, CNet, MC) | `net8.0` |

## Quick Start

```bash
dotnet new console -n MyHMI
cd MyHMI
dotnet add package Going.UI.OpenTK
dotnet add package Going.Basis
```

```csharp
using Going.UI.OpenTK.Windows;
using OpenTK.Windowing.Common;

using var view = new MainWindow();
view.Run();

public class MainWindow : GoViewWindow
{
    public MainWindow() : base(1024, 600, WindowBorder.Hidden)
    {
        InitializeComponent();
    }
}
```

## UI Editor

**Going UI Editor** is a visual design tool for creating `.gud` files — JSON-based UI layouts that generate C# code automatically.

Download from [Releases](https://github.com/going-kr/Going-Library/releases).

### Workflow

```
1. Design UI in UIEditor (.gud file)
2. MakeCode → generates .cs files
3. Write event handlers & communication code
4. Build & deploy to target device
```

## Communications

```csharp
// Modbus RTU Master
var rtu = new MasterRTU();
rtu.WordAreas.Add(0x0000, "D");
rtu.MonitorWord_F3(1, 0x0000, 50);
rtu.Start();

var value = rtu.GetWord("D0");  // Read from cache
rtu.SetWord(1, "D0", 100);     // Write to device
```

| Protocol | Classes |
|----------|---------|
| Modbus RTU | `MasterRTU`, `SlaveRTU`, `ModbusRTUMaster` |
| Modbus TCP | `MasterTCP`, `SlaveTCP`, `ModbusTCPMaster` |
| MQTT | `MQClient` |
| LS Electric CNet | `CNet` |
| Mitsubishi MC | `MC` |

## Claude Code Skill

Build HMI applications with AI assistance using the [Going UI Skill](https://github.com/going-kr/going-ui-skill) for Claude Code.

```
/going-ui  →  generates .gud files, writes C# code, handles communications
```

## Project Structure

```
Going.UI           — Platform-independent UI core
Going.UI.Forms     — WinForms adapter
Going.UI.OpenTK    — OpenTK adapter (embedded/Raspberry Pi)
Going.Basis        — Communications & utilities
Going.UIEditor     — Visual UI design tool
```

## License

MIT
