using Godot;
using System.Collections.Generic;

public partial class SnakeContainer : Container
{
    [Export] public int Columns { get; set; } = 5; 
    [Export] public Vector2 Separation { get; set; } = new Vector2(8, 8);

    public override void _Notification(int what)
    {
        if (what == NotificationSortChildren)
            ArrangeChildren();
    }

    private void ArrangeChildren()
    {
        var children = GetChildren();
        if (children.Count == 0 || Columns <= 0)
            return;

        float currentY = 0;
        int rowIndex = 0;
        int index = 0;

        while (index < children.Count)
        {
            // 收集本行
            List<Control> row = new();
            float rowWidth = 0;
            float maxHeight = 0;

            for (int i = 0; i < Columns && index < children.Count; i++, index++)
            {
                if (children[index] is not Control c)
                    continue;

                Vector2 size = c.GetCombinedMinimumSize();
                row.Add(c);

                rowWidth += size.X;
                if (i > 0) rowWidth += Separation.X;
                maxHeight = Mathf.Max(maxHeight, size.Y);
            }

            bool reverse = rowIndex % 2 == 1;
            float currentX = reverse ? rowWidth : 0;

            foreach (var child in row)
            {
                Vector2 size = child.GetCombinedMinimumSize();

                if (reverse)
                    currentX -= size.X;

                FitChildInRect(child, new Rect2(
                    currentX,
                    currentY,
                    size.X,
                    size.Y
                ));

                if (!reverse)
                    currentX += size.X + Separation.X;
                else
                    currentX -= Separation.X;
            }

            currentY += maxHeight + Separation.Y;
            rowIndex++;
        }
    }

    public override Vector2 _GetMinimumSize()
    {
        return Vector2.Zero;
    }
}