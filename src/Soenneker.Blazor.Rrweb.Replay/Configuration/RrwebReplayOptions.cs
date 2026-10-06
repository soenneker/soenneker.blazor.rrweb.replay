namespace Soenneker.Blazor.Rrweb.Replay.Configuration;

/// <summary>Serializable options for rrweb replay.</summary>
public sealed class RrwebReplayOptions
{
    /// <summary>Positive playback speed multiplier.</summary>
    public double Speed { get; set; } = 1;

    /// <summary>Skips inactive portions of a recording.</summary>
    public bool SkipInactive { get; set; } = false;

    /// <summary>Minimum inactive period in milliseconds.</summary>
    public int InactivePeriodThreshold { get; set; } = 10000;

    /// <summary>Timeout in milliseconds for loading remote stylesheets.</summary>
    public int LoadTimeout { get; set; } = 0;

    /// <summary>Logs replay warnings to the browser console.</summary>
    public bool ShowWarning { get; set; } = true;

    /// <summary>Logs diagnostic replay messages.</summary>
    public bool ShowDebug { get; set; } = false;

    /// <summary>Allows events to be supplied incrementally for live playback.</summary>
    public bool LiveMode { get; set; } = false;

    /// <summary>Allows replayed input focus to affect browser focus.</summary>
    public bool TriggerFocus { get; set; } = false;

    /// <summary>Pauses CSS animations when playback pauses.</summary>
    public bool PauseAnimation { get; set; } = true;

    /// <summary>Shows the replayed mouse trail.</summary>
    public bool MouseTail { get; set; } = true;

    /// <summary>Uses a virtual DOM while seeking.</summary>
    public bool UseVirtualDom { get; set; } = true;

    /// <summary>Additional CSS rules to inject into the replay iframe.</summary>
    public string[]? InsertStyleRules { get; set; }
}
