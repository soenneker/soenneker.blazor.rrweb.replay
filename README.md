[![](https://img.shields.io/nuget/v/soenneker.blazor.rrweb.replay.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.rrweb.replay/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.rrweb.replay/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.rrweb.replay/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.blazor.rrweb.replay.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.blazor.rrweb.replay/)
[![](https://img.shields.io/badge/Demo-Live-blueviolet?style=for-the-badge&logo=github)](https://soenneker.github.io/soenneker.blazor.rrweb.replay)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.rrweb.replay/codeql.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.rrweb.replay/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Blazor.Rrweb.Replay
### A Blazor interop library for replaying browser sessions with rrweb.

## Installation

```bash
dotnet add package Soenneker.Blazor.Rrweb.Replay
```

## Setup

Register services in `Program.cs`:

```csharp
builder.Services.AddRrwebReplayInteropAsScoped();
```

Inject the higher-level utility where you need it:

```csharp
@inject IRrwebReplayInterop Replay
```

## Usage

Initialize the package once before first use:

```csharp
await Replay.Initialize();
```
