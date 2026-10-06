using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blazor.Utils.ResourceLoader.Registrars;
using Soenneker.Blazor.Rrweb.Replay.Abstract;

namespace Soenneker.Blazor.Rrweb.Replay.Registrars;

/// <summary>Registers rrweb replay interop and its resource dependencies.</summary>
public static class RrwebReplayInteropRegistrar
{
    /// <summary>Adds <see cref="IRrwebReplayInterop"/> as a scoped service.</summary>
    public static IServiceCollection AddRrwebReplayInteropAsScoped(this IServiceCollection services)
    {
        services.AddResourceLoaderAsScoped();
        services.TryAddScoped<IRrwebReplayInterop, RrwebReplayInterop>();
        return services;
    }
}
