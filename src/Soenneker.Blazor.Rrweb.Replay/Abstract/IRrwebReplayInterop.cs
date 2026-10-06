using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Soenneker.Blazor.Rrweb.Replay.Configuration;

namespace Soenneker.Blazor.Rrweb.Replay.Abstract;

/// <summary>Creates and controls independent rrweb replayers. Call after interactive rendering.</summary>
/// <remarks>Each replay uses a unique ID and a rendered host element. Disposal destroys all replayers owned by this scope.</remarks>
public interface IRrwebReplayInterop : IAsyncDisposable
{
    /// <summary>Loads pinned rrweb JavaScript and CSS. The first call selects CDN or bundled local assets.</summary>
    ValueTask Initialize(bool useCdn = true, CancellationToken cancellationToken = default);

    /// <summary>Creates a paused replayer in the host. Events must include the initial snapshot and all subsequent events.
    /// Duplicate IDs throw; destroy the existing replayer before reusing its ID.</summary>
    ValueTask Create(string id, ElementReference element, JsonElement[] events, RrwebReplayOptions? options = null,
        bool useCdn = true, CancellationToken cancellationToken = default);

    /// <summary>Plays from an offset in milliseconds, or resumes from the current position when omitted.</summary>
    ValueTask Play(string id, double? timeOffset = null, CancellationToken cancellationToken = default);

    /// <summary>Pauses at the current position or seeks to an offset in milliseconds while paused.</summary>
    ValueTask Pause(string id, double? timeOffset = null, CancellationToken cancellationToken = default);

    /// <summary>Changes playback speed to a finite positive multiplier.</summary>
    ValueTask SetSpeed(string id, double speed, CancellationToken cancellationToken = default);

    /// <summary>Returns the current offset in milliseconds.</summary>
    ValueTask<double> GetCurrentTime(string id, CancellationToken cancellationToken = default);

    /// <summary>Returns the recording duration in milliseconds.</summary>
    ValueTask<double> GetDuration(string id, CancellationToken cancellationToken = default);

    /// <summary>Adds an rrweb event to an existing replayer, including during live playback.</summary>
    ValueTask AddEvent(string id, JsonElement recordedEvent, CancellationToken cancellationToken = default);

    /// <summary>Starts a replayer configured with LiveMode, optionally supplying a Unix timestamp in milliseconds.</summary>
    ValueTask StartLive(string id, double? baselineTime = null, CancellationToken cancellationToken = default);

    /// <summary>Destroys one replayer and removes its DOM. Repeated calls are harmless.</summary>
    ValueTask Destroy(string id, CancellationToken cancellationToken = default);
}
