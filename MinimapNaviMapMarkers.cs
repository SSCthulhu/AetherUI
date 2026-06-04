using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System.Numerics;

namespace FFXIVHudPlugin;

/// <summary>
/// Draws markers from <see cref="AgentMap.MiniMapMarkers"/> using map-texture deltas
/// converted through the same UV window as the scrolling minimap image.
/// </summary>
internal static class MinimapNaviMapMarkers
{
    private const uint PlayerMarkerIconId = 60443;

    public static unsafe bool IsAddonLoaded() => AgentMap.Instance() is not null;

    public static int TryCollect(
        float contentHalf,
        Vector2 mapUvMin,
        Vector2 mapUvMax,
        Vector3 playerPosition,
        int offsetX,
        int offsetY,
        uint sizeFactor,
        float visibleRangeYalms,
        float markerIconSize,
        MinimapMarkerIconCache iconCache,
        List<MinimapIconMarker> markers,
        int maxMarkers)
    {
        if (maxMarkers <= 0 || markers.Count >= maxMarkers)
        {
            return 0;
        }

        try
        {
            return TryCollectCore(
                contentHalf,
                mapUvMin,
                mapUvMax,
                playerPosition,
                offsetX,
                offsetY,
                sizeFactor,
                visibleRangeYalms,
                markerIconSize,
                iconCache,
                markers,
                maxMarkers);
        }
        catch (Exception ex)
        {
            MinimapFailureLogger.LogCollectorFailure("navi_map_markers.collect", ex);
            return 0;
        }
    }

    private static unsafe int TryCollectCore(
        float contentHalf,
        Vector2 mapUvMin,
        Vector2 mapUvMax,
        Vector3 playerPosition,
        int offsetX,
        int offsetY,
        uint sizeFactor,
        float visibleRangeYalms,
        float markerIconSize,
        MinimapMarkerIconCache iconCache,
        List<MinimapIconMarker> markers,
        int maxMarkers)
    {
        var agentMap = AgentMap.Instance();
        if (agentMap is null || agentMap->CurrentMapId == 0)
        {
            return 0;
        }

        var markerCount = Math.Min(agentMap->MiniMapMarkerCount, agentMap->MiniMapMarkers.Length);
        var collected = 0;

        for (var i = 0; i < markerCount && markers.Count < maxMarkers; i++)
        {
            ref readonly var entry = ref agentMap->MiniMapMarkers[i];
            var iconId = entry.MapMarker.IconId;
            if (iconId == 0 || iconId == PlayerMarkerIconId)
            {
                continue;
            }

            var markerWorldX = entry.MapMarker.X / 16f;
            var markerWorldZ = entry.MapMarker.Y / 16f;

            if (MinimapMarkerPlacement.TryAddIconMarker(
                    markerWorldX,
                    markerWorldZ,
                    iconId,
                    playerPosition,
                    offsetX,
                    offsetY,
                    sizeFactor,
                    visibleRangeYalms,
                    contentHalf,
                    mapUvMin,
                    mapUvMax,
                    markerIconSize,
                    iconCache,
                    markers))
            {
                collected++;
            }
        }

        return collected;
    }
}
