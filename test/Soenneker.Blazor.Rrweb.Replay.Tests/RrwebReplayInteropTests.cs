using Soenneker.Blazor.Rrweb.Replay.Abstract;
using Soenneker.Tests.HostedUnit;
using System;
using System.Threading.Tasks;


namespace Soenneker.Blazor.Rrweb.Replay.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class RrwebReplayInteropTests : HostedUnitTest
{
    private readonly IRrwebReplayInterop _blazorlibrary;

    public RrwebReplayInteropTests(Host host) : base(host)
    {
        _blazorlibrary = Resolve<IRrwebReplayInterop>(true);
    }

    [Test]
    public async Task Registrar_resolves_scoped_interop()
    {
        await Assert.That(_blazorlibrary).IsTypeOf<RrwebReplayInterop>();
    }

    [Test]
    public async Task Empty_recording_is_rejected_before_loading_resources()
    {
        Func<Task> action = async () => await _blazorlibrary.Create("test", default, []);
        await Assert.ThrowsAsync<ArgumentException>(action);
    }

    [Test]
    public async Task Invalid_speed_is_rejected()
    {
        Func<Task> action = async () => await _blazorlibrary.SetSpeed("test", double.NaN);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(action);
    }

    [Test]
    public async Task Negative_seek_is_rejected()
    {
        Func<Task> action = async () => await _blazorlibrary.Play("test", -1);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(action);
    }
}

