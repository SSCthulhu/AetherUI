using Dalamud.Logging;
using DelvUI.Config;
using DelvUI.Enums;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Interface
{
    public delegate void DraggableHudElementSelectHandler(DraggableHudElement element);

    public class DraggableHudElement : HudElement
    {
        public DraggableHudElement(MovablePluginConfigObject config, string? displayName = null) : base(config)
        {
            _displayName = displayName ?? ID;
        }

        public event DraggableHudElementSelectHandler? SelectEvent;
        public bool Selected = false;

        private string _displayName;
        protected bool _windowPositionSet = false;
        private Vector2 _lastWindowPos = Vector2.Zero;
        private Vector2 _contentMargin = new Vector2(4, 0);
        private IReadOnlyList<DraggableHudElement>? _screenClampAnchoredChildren;
        private Vector2? _screenClampSavedPosition = null;
        private Vector2 _drawScreenClampOffset = Vector2.Zero;
        private Vector2 _lastDrawScreenClampOffset = Vector2.Zero;

        public Vector2 DrawScreenClampOffset => _drawScreenClampOffset;

        private bool _draggingEnabled = false;
        public bool DraggingEnabled
        {
            get => _draggingEnabled;
            set
            {
                _draggingEnabled = value;

                if (_draggingEnabled)
                {
                    _windowPositionSet = false;
                    _minPos = null;
                    _maxPos = null;
                }
            }
        }

        public bool CanTakeInputForDrag = false;
        public bool NeedsInputForDrag { get; private set; } = false;

        public virtual Vector2 ParentPos() { return Vector2.Zero; } // override

        public virtual bool ShouldClampIndependently => true;

        public void SetScreenClampAnchoredChildren(IReadOnlyList<DraggableHudElement>? anchoredChildren)
        {
            _screenClampAnchoredChildren = anchoredChildren;
        }

        public void ClearScreenClampAnchoredChildren()
        {
            _screenClampAnchoredChildren = null;
            _drawScreenClampOffset = Vector2.Zero;
            _lastDrawScreenClampOffset = Vector2.Zero;
        }

        public void ComputeDrawScreenClampOffset(Vector2 origin)
        {
            _drawScreenClampOffset = Vector2.Zero;

            if (ShouldApplyDrawTimeScreenClamp())
            {
                _drawScreenClampOffset = GetScreenClampConfigOffset(origin, _screenClampAnchoredChildren) ?? Vector2.Zero;
            }

            if (_draggingEnabled && _drawScreenClampOffset != _lastDrawScreenClampOffset)
            {
                _windowPositionSet = false;
                _lastDrawScreenClampOffset = _drawScreenClampOffset;
            }
        }

        public new void PrepareForDraw(Vector2 origin)
        {
            _screenClampSavedPosition = null;

            if (_drawScreenClampOffset != Vector2.Zero && !_draggingEnabled)
            {
                _screenClampSavedPosition = _config.Position;
                _config.Position += _drawScreenClampOffset;
                FlagDraggableAreaDirty();
            }

            base.PrepareForDraw(origin);
        }

        public override void Draw(Vector2 origin)
        {
            base.Draw(origin);

            if (_screenClampSavedPosition.HasValue)
            {
                _config.Position = _screenClampSavedPosition.Value;
                _screenClampSavedPosition = null;
            }
        }

        public bool TryPersistScreenClamp(Vector2 origin, IReadOnlyList<DraggableHudElement>? anchoredChildren = null)
        {
            if (!ShouldClampHudToScreen() || !ShouldClampIndependently)
            {
                return false;
            }

            Vector2? offset = GetScreenClampConfigOffset(origin, anchoredChildren);
            if (offset is not Vector2 clampOffset || clampOffset == Vector2.Zero)
            {
                return false;
            }

            _config.Position += clampOffset;
            FlagDraggableAreaDirty();
            _windowPositionSet = false;
            return true;
        }

        public bool IsOutsideScreenBounds(Vector2 origin, IReadOnlyList<DraggableHudElement>? anchoredChildren = null)
        {
            if (!ShouldClampIndependently)
            {
                return false;
            }

            return GetScreenClampConfigOffset(origin, anchoredChildren) is Vector2 offset && offset != Vector2.Zero;
        }

        public bool TryGetScreenBounds(Vector2 origin, out Vector2 screenMin, out Vector2 screenMax)
        {
            screenMin = Vector2.Zero;
            screenMax = Vector2.Zero;

            var (positions, sizes) = ChildrenPositionsAndSizes();
            if (positions.Count == 0 || sizes.Count == 0)
            {
                return false;
            }

            Vector2 margin = GlobalHudScaleHelper.Scale(_contentMargin);
            screenMin = origin + MinPos - margin;
            screenMax = origin + MaxPos + margin;
            return true;
        }

        private bool ShouldClampHudToScreen()
        {
            HUDOptionsConfig? options = ConfigurationManager.Instance.GetConfigObject<HUDOptionsConfig>();
            return options?.ClampHudToScreen ?? true;
        }

        private bool ShouldApplyDrawTimeScreenClamp()
        {
            return ShouldClampHudToScreen() && ShouldClampIndependently;
        }

        private Vector2? GetScreenClampConfigOffset(Vector2 origin, IReadOnlyList<DraggableHudElement>? anchoredChildren = null)
        {
            if (!TryGetScreenBounds(origin, out Vector2 screenMin, out Vector2 screenMax))
            {
                return null;
            }

            if (anchoredChildren != null)
            {
                foreach (DraggableHudElement child in anchoredChildren)
                {
                    if (child.TryGetScreenBounds(origin, out Vector2 childMin, out Vector2 childMax))
                    {
                        screenMin = Vector2.Min(screenMin, childMin);
                        screenMax = Vector2.Max(screenMax, childMax);
                    }
                }
            }

            Vector2 screenDelta = HudScreenBoundsHelper.ComputeClampDelta(screenMin, screenMax);

            if (screenDelta == Vector2.Zero)
            {
                return null;
            }

            return GlobalHudScaleHelper.Unscale(screenDelta);
        }

        protected sealed override void CreateDrawActions(Vector2 origin)
        {
            if (_draggingEnabled)
            {
                AddDrawAction(_config.StrataLevel, () =>
                {
                    DrawDraggableArea(origin);
                });
                return;
            }

            DrawChildren(origin);
        }

        public virtual void DrawChildren(Vector2 origin) { }

        private bool CalculateNeedsInput(Vector2 pos, Vector2 size, bool selected)
        {
            Vector2 mousePos = ImGui.GetMousePos();

            if (ImGui.IsMouseHoveringRect(pos, pos + size))
            {
                return true;
            }

            if (!selected)
            {
                return false;
            }

            var arrowsPos = DraggablesHelper.GetArrowPositions(pos, size);

            foreach (Vector2 arrowPos in arrowsPos)
            {
                if (ImGui.IsMouseHoveringRect(arrowPos, arrowPos + DraggablesHelper.ArrowSize))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void DrawDraggableArea(Vector2 origin)
        {
            var windowFlags = ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoTitleBar
            | ImGuiWindowFlags.NoResize
            | ImGuiWindowFlags.NoBackground
            | ImGuiWindowFlags.NoDecoration
            | ImGuiWindowFlags.NoSavedSettings;

            Vector2 visualClampOffset = _draggingEnabled ? _drawScreenClampOffset : Vector2.Zero;
            Vector2 scaledMargin = GlobalHudScaleHelper.Scale(_contentMargin);

            // MinPos/MaxPos are already in screen space (include global scale once).
            var size = MaxPos - MinPos + scaledMargin * 2f;
            ImGui.SetNextWindowSize(size, ImGuiCond.Always);

            // needs input?
            NeedsInputForDrag = CanTakeInputForDrag && CalculateNeedsInput(_lastWindowPos, size, Selected);

            if (!NeedsInputForDrag)
            {
                windowFlags |= ImGuiWindowFlags.NoMove;
            }

            // set initial position
            if (!_windowPositionSet)
            {
                ImGui.SetNextWindowPos(GetDragAreaScreenPosition(origin, visualClampOffset));
                _windowPositionSet = true;
            }

            // update config object position
            ImGui.Begin(ID + "_dragArea", windowFlags);
            var windowPos = ImGui.GetWindowPos();
            _lastWindowPos = windowPos;

            if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) && ImGui.IsWindowHovered())
            {
                _config.Position = GetConfigPositionFromDragArea(windowPos, origin, visualClampOffset);
            }

            // check selection
            var tooltipText = "x: " + _config.Position.X.ToString() + "    y: " + _config.Position.Y.ToString();

            if (NeedsInputForDrag && ImGui.IsMouseHoveringRect(windowPos, windowPos + size))
            {
                bool cliked = ImGui.IsMouseClicked(ImGuiMouseButton.Left) || ImGui.IsMouseDown(ImGuiMouseButton.Left);
                if (cliked && !Selected)
                {
                    SelectEvent?.Invoke(this);
                }

                // tooltip
                TooltipsHelper.Instance.ShowTooltipOnCursor(tooltipText);
            }

            // draw window
            var drawList = ImGui.GetWindowDrawList();
            var contentPos = windowPos + scaledMargin;
            var contentSize = size - scaledMargin * 2f;

            // draw draggable indicators
            drawList.AddRectFilled(contentPos, contentPos + contentSize, 0x88444444, 3);

            var lineColor = Selected ? 0xEEFFFFFF : 0x66FFFFFF;
            drawList.AddRect(contentPos, contentPos + contentSize, lineColor, 3, ImDrawFlags.None, 2);
            drawList.AddLine(contentPos + new Vector2(contentSize.X / 2f, 0), contentPos + new Vector2(contentSize.X / 2, contentSize.Y), lineColor);
            drawList.AddLine(contentPos + new Vector2(0, contentSize.Y / 2f), contentPos + new Vector2(contentSize.X, contentSize.Y / 2), lineColor);

            ImGui.End();

            // arrows
            if (Selected)
            {
                if (DraggablesHelper.DrawArrows(windowPos, size, tooltipText, out var movement))
                {
                    _minPos = null;
                    _maxPos = null;
                    _config.Position += GlobalHudScaleHelper.Unscale(movement);
                    _windowPositionSet = false;
                }
            }

            // element name
            var textSize = ImGui.CalcTextSize(_displayName);
            var textColor = Selected ? 0xFFFFFFFF : 0xEEFFFFFF;
            var textOutlineColor = Selected ? 0xFF000000 : 0xEE000000;
            DrawHelper.DrawOutlinedText(_displayName, contentPos + contentSize / 2f - textSize / 2f, textColor, textOutlineColor, drawList);
        }

        protected Vector2 GetDragAreaScreenPosition(Vector2 origin, Vector2 visualClampOffset)
        {
            return origin + MinPos - GlobalHudScaleHelper.Scale(_contentMargin) + GlobalHudScaleHelper.Scale(visualClampOffset);
        }

        protected Vector2 GetConfigPositionFromDragArea(Vector2 windowPos, Vector2 origin, Vector2 visualClampOffset)
        {
            Vector2 scaledMargin = GlobalHudScaleHelper.Scale(_contentMargin);
            Vector2 scaledSize = MaxPos - MinPos;
            Vector2 contentCenter = windowPos + scaledMargin + scaledSize * 0.5f;
            return GlobalHudScaleHelper.Unscale(contentCenter - origin) - visualClampOffset;
        }

        protected Vector2? _minPos = null;
        protected Vector2? _maxPos = null;

        private void EnsureContentBoundsCache()
        {
            if (_minPos != null && _maxPos != null)
            {
                return;
            }

            var (positions, sizes) = ChildrenPositionsAndSizes();
            if (positions.Count == 0 || sizes.Count == 0)
            {
                _minPos = Vector2.Zero;
                _maxPos = Vector2.Zero;
                return;
            }

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            var anchorConfig = _config as AnchorablePluginConfigObject;
            DrawAnchor anchor = anchorConfig?.Anchor ?? DrawAnchor.Center;

            for (int i = 0; i < positions.Count; i++)
            {
                Vector2 topLeft = GetAnchoredPosition(positions[i], sizes[i], anchor);
                Vector2 bottomRight = topLeft + GlobalHudScaleHelper.Scale(sizes[i]);
                minX = Math.Min(minX, topLeft.X);
                minY = Math.Min(minY, topLeft.Y);
                maxX = Math.Max(maxX, bottomRight.X);
                maxY = Math.Max(maxY, bottomRight.Y);
            }

            _minPos = new Vector2(minX, minY);
            _maxPos = new Vector2(maxX, maxY);
        }

        public Vector2 MinPos
        {
            get
            {
                EnsureContentBoundsCache();
                return (Vector2)_minPos!;
            }
        }

        public Vector2 MaxPos
        {
            get
            {
                EnsureContentBoundsCache();
                return (Vector2)_maxPos!;
            }
        }

        public void FlagDraggableAreaDirty()
        {
            _minPos = null;
            _maxPos = null;
        }

        protected virtual Vector2 GetAnchoredPosition(Vector2 position, Vector2 size, DrawAnchor anchor)
        {
            return Utils.GetAnchoredPosition(ParentPos() + GlobalHudScaleHelper.Scale(position), size, anchor);
        }

        protected virtual (List<Vector2>, List<Vector2>) ChildrenPositionsAndSizes()
        {
            return (new List<Vector2>(), new List<Vector2>());
        }
    }

    public abstract class ParentAnchoredDraggableHudElement : DraggableHudElement
    {
        public ParentAnchoredDraggableHudElement(MovablePluginConfigObject config, string? displayName = null)
            : base(config, displayName)
        {
        }

        protected virtual bool AnchorToParent { get; }
        protected virtual DrawAnchor ParentAnchor { get; }
        public AnchorablePluginConfigObject? ParentConfig { get; set; }

        public bool IsAnchoredToParent => AnchorToParent && ParentConfig != null;

        public override bool ShouldClampIndependently => !IsAnchoredToParent;

        private Vector2 _parentDrawScreenClampOffset = Vector2.Zero;
        private Vector2? _lastParentPosition = null;
        private Vector2 _lastParentDrawScreenClampOffset = Vector2.Zero;

        public void SetParentDrawScreenClampOffset(Vector2 offset)
        {
            _parentDrawScreenClampOffset = offset;
        }

        private bool IsAnchored => AnchorToParent && ParentConfig != null;

        public override Vector2 ParentPos()
        {
            if (!IsAnchored)
            {
                return Vector2.Zero;
            }

            Vector2 parentPosition = ParentConfig!.Position + _parentDrawScreenClampOffset;
            Vector2 parentAnchoredPos = Utils.GetAnchoredPosition(
                GlobalHudScaleHelper.Scale(parentPosition),
                ParentConfig!.Size,
                ParentConfig!.Anchor);
            return Utils.GetAnchoredPosition(parentAnchoredPos, -ParentConfig!.Size, ParentAnchor);
        }

        protected override void DrawDraggableArea(Vector2 origin)
        {
            // if the parent moved, update own draggable area
            if (IsAnchored && (
                _lastParentPosition == null ||
                _lastParentPosition != ParentConfig!.Position ||
                _lastParentDrawScreenClampOffset != _parentDrawScreenClampOffset))
            {
                _windowPositionSet = false;
                _minPos = null;
                _maxPos = null;
                _lastParentPosition = ParentConfig!.Position;
                _lastParentDrawScreenClampOffset = _parentDrawScreenClampOffset;
            }

            base.DrawDraggableArea(origin);
        }
    }
}
