using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Soenneker.Asyncs.Initializers;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Blazor.Utils.ResourceLoader.Abstract;
using Soenneker.Blazor.Rrweb.Replay.Abstract;
using Soenneker.Blazor.Rrweb.Replay.Configuration;

namespace Soenneker.Blazor.Rrweb.Replay;

public sealed class RrwebReplayInterop : IRrwebReplayInterop
{
    private const string _modulePath = "./_content/Soenneker.Blazor.Rrweb.Replay/js/rrwebreplayinterop.js";
    private readonly IResourceLoader _resourceLoader;
    private readonly IModuleImportUtil _moduleImportUtil;
    private readonly AsyncInitializer<bool> _initializer;
    private IJSObjectReference? _interop;
    private bool _disposed;

    public RrwebReplayInterop(IResourceLoader resourceLoader, IModuleImportUtil moduleImportUtil)
    {
        _resourceLoader = resourceLoader;
        _moduleImportUtil = moduleImportUtil;
        _initializer = new AsyncInitializer<bool>(InitializeResources);
    }

    private async ValueTask InitializeResources(bool useCdn, CancellationToken cancellationToken)
    {
        await _resourceLoader.LoadStyle(useCdn
                ? "https://cdn.jsdelivr.net/npm/@rrweb/replay@2.1.7/dist/style.min.css"
                : "_content/Soenneker.Blazor.Rrweb.Replay/css/rrweb-replay.min.css",
                useCdn ? "sha384-2zp5yTttnrewFxGO1gZqarXk4eQ3cDSsWIedGAhaF9GOw1f+TiLoxMFBZGCHow2O" : null, cancellationToken: cancellationToken);
        await _resourceLoader.LoadScriptAndWaitForVariable(useCdn
                ? "https://cdn.jsdelivr.net/npm/@rrweb/replay@2.1.7/dist/replay.umd.min.cjs"
                : "_content/Soenneker.Blazor.Rrweb.Replay/js/rrweb-replay.min.js",
            "rrwebReplay", useCdn ? "sha384-LX0Qc58DtX90qtHq3rjSoGcuLK4GDV3PHOW6PmFdtge6n0wvY8M6dD7L6DGOzNUh" : null, cancellationToken: cancellationToken);
        IJSObjectReference module = await _moduleImportUtil.GetContentModuleReference(_modulePath, cancellationToken);
        _interop = await module.InvokeAsync<IJSObjectReference>("createInterop", cancellationToken);
    }

    public ValueTask Initialize(bool useCdn = true, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _initializer.Init(useCdn, cancellationToken);
    }

    public async ValueTask Create(string id, ElementReference element, JsonElement[] events, RrwebReplayOptions? options = null,
        bool useCdn = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(events);
        options ??= new RrwebReplayOptions();
        if (!double.IsFinite(options.Speed) || options.Speed <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Replay speed must be finite and positive.");
        if (!options.LiveMode && events.Length < 2)
            throw new ArgumentException("Replay requires at least two recorded events.", nameof(events));
        await Initialize(useCdn, cancellationToken);
        await InvokeVoid("create", cancellationToken, id, element,
            JsonSerializer.Serialize(events, LibraryJsonContext.Default.JsonElementArray),
            JsonSerializer.Serialize(options, LibraryJsonContext.Default.RrwebReplayOptions));
    }

    public ValueTask Play(string id, double? timeOffset = null, CancellationToken cancellationToken = default)
    {
        ValidateOffset(timeOffset);
        return InvokeVoid("play", cancellationToken, id, timeOffset);
    }

    public ValueTask Pause(string id, double? timeOffset = null, CancellationToken cancellationToken = default)
    {
        ValidateOffset(timeOffset);
        return InvokeVoid("pause", cancellationToken, id, timeOffset);
    }

    public ValueTask SetSpeed(string id, double speed, CancellationToken cancellationToken = default)
    {
        if (!double.IsFinite(speed) || speed <= 0)
            throw new ArgumentOutOfRangeException(nameof(speed));
        return InvokeVoid("setSpeed", cancellationToken, id, speed);
    }

    public ValueTask<double> GetCurrentTime(string id, CancellationToken cancellationToken = default)
        => Invoke<double>("getCurrentTime", cancellationToken, id);

    public ValueTask<double> GetDuration(string id, CancellationToken cancellationToken = default)
        => Invoke<double>("getDuration", cancellationToken, id);

    public ValueTask AddEvent(string id, JsonElement recordedEvent, CancellationToken cancellationToken = default)
        => InvokeVoid("addEvent", cancellationToken, id, recordedEvent.GetRawText());

    public ValueTask StartLive(string id, double? baselineTime = null, CancellationToken cancellationToken = default)
    {
        ValidateOffset(baselineTime);
        return InvokeVoid("startLive", cancellationToken, id, baselineTime);
    }

    public ValueTask Destroy(string id, CancellationToken cancellationToken = default)
        => InvokeVoid("destroy", cancellationToken, id);

    private static void ValidateOffset(double? offset)
    {
        if (offset.HasValue && (!double.IsFinite(offset.Value) || offset.Value < 0))
            throw new ArgumentOutOfRangeException(nameof(offset));
    }

    private ValueTask InvokeVoid(string method, CancellationToken cancellationToken, params object?[] args)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_interop is null)
            throw new InvalidOperationException("Initialize must be called before this operation.");
        return _interop.InvokeVoidAsync(method, cancellationToken, args);
    }

    private ValueTask<T> Invoke<T>(string method, CancellationToken cancellationToken, params object?[] args)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_interop is null)
            throw new InvalidOperationException("Initialize must be called before this operation.");
        return _interop.InvokeAsync<T>(method, cancellationToken, args);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        await _initializer.DisposeAsync();
        try
        {
            if (_interop is not null)
            {
                try { await _interop.InvokeVoidAsync("dispose"); }
                finally { await _interop.DisposeAsync(); }
            }
        }
        catch (JSDisconnectedException) { }
        // The module import service owns its shared cached module reference.
    }
}
