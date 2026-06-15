using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace DelvUI.Config.Home.Widgets
{
    public sealed class HomeFeatureGridLayout
    {
        private readonly Vector2 _origin;
        private readonly float _gap;
        private readonly float[] _columnWidths;

        public HomeFeatureGridLayout(float totalWidth, int columns, float gap, Vector2 origin)
        {
            _origin = origin;
            _gap = gap;
            _columnWidths = new float[columns];

            float totalGap = gap * (columns - 1);
            float columnWidth = (totalWidth - totalGap) / columns;
            for (int i = 0; i < columns; i++)
            {
                _columnWidths[i] = columnWidth;
            }
        }

        public HomeFeatureGridLayout(float totalWidth, int columns, float gap)
            : this(totalWidth, columns, gap, ImGui.GetCursorPos())
        {
        }

        public void SetSlot(int column, int row, float rowHeight)
        {
            float x = _origin.X;
            for (int i = 0; i < column; i++)
            {
                x += _columnWidths[i] + _gap;
            }

            float y = _origin.Y + row * (rowHeight + _gap);
            ImGui.SetCursorPos(new Vector2(x, y));
        }

        public Vector2 GetSlotSize(int column, int columnSpan, float height)
        {
            float width = 0f;
            for (int i = 0; i < columnSpan; i++)
            {
                width += _columnWidths[column + i];
            }

            width += _gap * (columnSpan - 1);
            return new Vector2(width, height);
        }

        public float GetContentHeight(int rowCount, float rowHeight)
        {
            if (rowCount <= 0)
            {
                return 0f;
            }

            return rowCount * rowHeight + (rowCount - 1) * _gap;
        }
    }
}
