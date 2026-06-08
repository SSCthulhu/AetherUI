using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using System.Numerics;
using FFXIVHudPlugin.AetherPlates.Configuration;

namespace FFXIVHudPlugin.AetherPlates.Services;

public sealed unsafe class NameplateOcclusionService
{
    public bool IsOccluded(
        NameplateOcclusionMode mode,
        NameplateOcclusionType type,
        Vector2 screenAnchor,
        Vector3 worldAnchor,
        float maxDistance)
    {
        if (mode == NameplateOcclusionMode.None)
        {
            return false;
        }

        var distance = MathF.Max(0.01f, maxDistance);
        var collisionModule = Framework.Instance()->BGCollisionModule;
        if (collisionModule == null)
        {
            return false;
        }

        var camera = Control.Instance()->CameraManager.Camera->CameraBase.SceneCamera;
        var cameraPos = camera.Object.Position;
        var origin = new Vector3(cameraPos.X, cameraPos.Y, cameraPos.Z);
        var flag = type == NameplateOcclusionType.WallsAndObjects ? 0x2000 : 0x4000;
        var flags = stackalloc int[] { flag, 0, flag, 0 };

        if (mode == NameplateOcclusionMode.Simple)
        {
            var direction = Vector3.Normalize(worldAnchor - origin);
            RaycastHit hit;
            return collisionModule->RaycastMaterialFilter(&hit, &origin, &direction, distance, 1, flags);
        }

        var samplePoints = stackalloc Vector2[3];
        samplePoints[0] = screenAnchor;
        samplePoints[1] = screenAnchor + new Vector2(-30f, 0f);
        samplePoints[2] = screenAnchor + new Vector2(30f, 0f);

        var blockedSamples = 0;
        for (var i = 0; i < 3; i++)
        {
            var ray = camera.ScreenPointToRay(samplePoints[i]);
            var rayOrigin = new Vector3(ray.Origin.X, ray.Origin.Y, ray.Origin.Z);
            var rayDirection = new Vector3(ray.Direction.X, ray.Direction.Y, ray.Direction.Z);
            RaycastHit hit;
            if (collisionModule->RaycastMaterialFilter(&hit, &rayOrigin, &rayDirection, distance, 1, flags))
            {
                blockedSamples++;
            }
        }

        // Treat as occluded only when all sampled rays are blocked.
        return blockedSamples >= 3;
    }
}
