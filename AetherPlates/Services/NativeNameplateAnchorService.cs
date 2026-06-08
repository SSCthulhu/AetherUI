using Dalamud.Game.Gui.NamePlate;
using Dalamud.Plugin.Services;
using System.Numerics;
using System.Runtime.InteropServices;

namespace FFXIVHudPlugin.AetherPlates.Services;

public sealed class NativeNameplateAnchorService : IDisposable
{
    public readonly record struct NativePlateMeta(
        string Name,
        string Title,
        uint JobIconId);

    private readonly INamePlateGui namePlateGui;
    private readonly Dictionary<ulong, Vector2> anchors = new();
    private readonly Dictionary<ulong, NamePlateKind> kinds = new();
    private readonly Dictionary<ulong, NativePlateMeta> metadata = new();
    private readonly HashSet<ulong> activeIds = new();
    private long frameId;
    private bool disposed;

    public NativeNameplateAnchorService(INamePlateGui namePlateGui)
    {
        this.namePlateGui = namePlateGui;
        this.namePlateGui.OnDataUpdate += this.OnDataUpdate;
    }

    public bool TryGetAnchor(ulong objectId, out Vector2 screenPos)
    {
        return this.anchors.TryGetValue(objectId, out screenPos);
    }

    public bool TryGetKind(ulong objectId, out NamePlateKind kind)
    {
        return this.kinds.TryGetValue(objectId, out kind);
    }

    public bool IsInCurrentNativeSet(ulong objectId)
    {
        return this.activeIds.Contains(objectId);
    }

    public bool TryGetMeta(ulong objectId, out NativePlateMeta meta)
    {
        return this.metadata.TryGetValue(objectId, out meta);
    }

    public long LastFrameId => this.frameId;

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        this.namePlateGui.OnDataUpdate -= this.OnDataUpdate;
        this.anchors.Clear();
        this.kinds.Clear();
        this.metadata.Clear();
    }

    private void OnDataUpdate(INamePlateUpdateContext _, IReadOnlyList<INamePlateUpdateHandler> handlers)
    {
        this.frameId++;
        this.anchors.Clear();
        this.kinds.Clear();
        this.metadata.Clear();
        this.activeIds.Clear();
        for (var i = 0; i < handlers.Count; i++)
        {
            var handler = handlers[i];
            var id = handler.GameObjectId;
            if (id == 0)
            {
                continue;
            }

            var address = handler.NamePlateObjectAddress;
            if (address == nint.Zero)
            {
                continue;
            }

            // NamePlateObjectAddress points at the first integer entry for this plate's backing data.
            // In current API level, X/Y screen coordinates are stored in the first two int slots.
            var x = Marshal.ReadInt32(address, 0);
            var y = Marshal.ReadInt32(address, sizeof(int));
            if (x <= -10000 || y <= -10000)
            {
                continue;
            }

            if (x <= 1 || y <= 1)
            {
                continue;
            }

            this.activeIds.Add(id);
            this.anchors[id] = new Vector2(x, y);
            this.kinds[id] = handler.NamePlateKind;
            this.metadata[id] = new NativePlateMeta(
                handler.Name.TextValue ?? string.Empty,
                handler.Title.TextValue ?? string.Empty,
                0u);
        }
    }
}
