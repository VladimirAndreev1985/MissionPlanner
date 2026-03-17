using System;
using System.Drawing;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace MissionPlanner.Maps
{
    /// <summary>
    /// Tactical marker types (duplicated here to avoid circular dependency with Utilities)
    /// </summary>
    public enum TacticalMarkerType
    {
        Friendly,   // Свои
        Enemy,      // Противник
        Unknown,    // Неизвестный
        Objective   // Цель/Объект
    }

    [Serializable]
    public class GMapMarkerTactical : GMapMarker
    {
        private const int MarkerSize = 16;
        private TacticalMarkerType tacticalType;

        private static readonly Size SizeSt = new Size(MarkerSize, MarkerSize);

        public GMapMarkerTactical(PointLatLng p, TacticalMarkerType type)
            : base(p)
        {
            tacticalType = type;
            Size = SizeSt;
            Offset = new Point(-MarkerSize / 2, -MarkerSize / 2);
        }

        public override void OnRender(IGraphics g)
        {
            int x = LocalPosition.X;
            int y = LocalPosition.Y;
            int s = MarkerSize;

            switch (tacticalType)
            {
                case TacticalMarkerType.Friendly:
                    // Blue filled circle with white border
                    using (var brush = new SolidBrush(Color.RoyalBlue))
                    using (var pen = new Pen(Color.White, 2f))
                    {
                        g.FillEllipse(brush, x, y, s, s);
                        g.DrawEllipse(pen, x, y, s, s);
                    }
                    break;

                case TacticalMarkerType.Enemy:
                    // Red filled diamond/rhombus
                    var diamond = new Point[]
                    {
                        new Point(x + s / 2, y),
                        new Point(x + s, y + s / 2),
                        new Point(x + s / 2, y + s),
                        new Point(x, y + s / 2)
                    };
                    using (var brush = new SolidBrush(Color.Red))
                    using (var pen = new Pen(Color.DarkRed, 1.5f))
                    {
                        g.FillPolygon(brush, diamond);
                        g.DrawPolygon(pen, diamond);
                    }
                    break;

                case TacticalMarkerType.Unknown:
                    // Yellow filled triangle
                    var triangle = new Point[]
                    {
                        new Point(x + s / 2, y),
                        new Point(x + s, y + s),
                        new Point(x, y + s)
                    };
                    using (var brush = new SolidBrush(Color.Gold))
                    using (var pen = new Pen(Color.DarkGoldenrod, 1.5f))
                    {
                        g.FillPolygon(brush, triangle);
                        g.DrawPolygon(pen, triangle);
                    }
                    break;

                case TacticalMarkerType.Objective:
                    // Orange crosshair/circle
                    using (var brush = new SolidBrush(Color.FromArgb(128, Color.Orange)))
                    using (var penCircle = new Pen(Color.OrangeRed, 2f))
                    using (var penCross = new Pen(Color.OrangeRed, 1.5f))
                    {
                        g.FillEllipse(brush, x + 2, y + 2, s - 4, s - 4);
                        g.DrawEllipse(penCircle, x + 1, y + 1, s - 2, s - 2);
                        // Crosshair lines
                        g.DrawLine(penCross, x + s / 2, y, x + s / 2, y + s);
                        g.DrawLine(penCross, x, y + s / 2, x + s, y + s / 2);
                    }
                    break;
            }
        }
    }
}
