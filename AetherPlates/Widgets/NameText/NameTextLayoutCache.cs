using System.Collections.Concurrent;
using System.Numerics;

namespace FFXIVHudPlugin.AetherPlates.Widgets.NameText;

internal readonly record struct NameTextLayoutSnapshot(
    long FrameId,
    Vector2 Position,
    Vector2 Size);

internal static class NameTextLayoutCache
{
    private static long frameId;
    private static readonly ConcurrentDictionary<ulong, NameTextLayoutSnapshot> Snapshots = new();

    public static void BeginFrame()
    {
        frameId++;
    }

    public static void Set(ulong objectId, Vector2 position, Vector2 size)
    {
        Snapshots[objectId] = new NameTextLayoutSnapshot(frameId, position, size);
    }

    public static bool TryGetCurrent(ulong objectId, out NameTextLayoutSnapshot snapshot)
    {
        if (Snapshots.TryGetValue(objectId, out snapshot) &&
            snapshot.FrameId == frameId)
        {
            return true;
        }

        snapshot = default;
        return false;
    }
}
