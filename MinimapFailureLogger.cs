using Dalamud.Plugin.Services;

namespace FFXIVHudPlugin;

internal static class MinimapFailureLogger
{
    private static readonly TimeSpan ThrottleWindow = TimeSpan.FromSeconds(10);
    private static readonly object SyncRoot = new();
    private static readonly Dictionary<string, LogState> States = new(StringComparer.Ordinal);
    private static IPluginLog? pluginLog;

    public static void Initialize(IPluginLog pluginLog)
    {
        lock (SyncRoot)
        {
            MinimapFailureLogger.pluginLog = pluginLog;
        }
    }

    public static void LogCollectorFailure(string key, Exception exception)
    {
        lock (SyncRoot)
        {
            if (pluginLog is null)
            {
                return;
            }

            var now = DateTime.UtcNow;
            if (!States.TryGetValue(key, out var state))
            {
                state = default;
            }

            if (now >= state.NextLogUtc)
            {
                if (state.SuppressedCount > 0)
                {
                    pluginLog.Debug($"Suppressed {state.SuppressedCount} minimap collector failures for '{key}'.");
                }

                pluginLog.Warning(exception, $"Minimap collector '{key}' failed; continuing with partial minimap state.");
                state.SuppressedCount = 0;
                state.NextLogUtc = now + ThrottleWindow;
                States[key] = state;
                return;
            }

            state.SuppressedCount++;
            States[key] = state;
        }
    }

    private struct LogState
    {
        public DateTime NextLogUtc;
        public int SuppressedCount;
    }
}
