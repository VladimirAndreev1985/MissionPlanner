using GMap.NET.WindowsForms;
using MissionPlanner.Controls;
using MissionPlanner.Maps;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MissionPlanner.Utilities
{
    public enum TacticalType
    {
        Friendly,   // Свои
        Enemy,      // Противник
        Unknown,    // Неизвестный
        Objective   // Цель/Объект
    }

    public class TacticalPoint
    {
        public PointLatLngAlt Position { get; set; }
        public TacticalType Type { get; set; }
        public string Label { get; set; }
        public DateTime Timestamp { get; set; }

        public TacticalPoint()
        {
            Timestamp = DateTime.UtcNow;
        }

        public TacticalPoint(PointLatLngAlt pos, TacticalType type, string label)
        {
            Position = pos;
            Type = type;
            Label = label;
            Timestamp = DateTime.UtcNow;
        }
    }

    public static class TacticalMarkers
    {
        private static readonly ObservableCollection<TacticalPoint> _points = new ObservableCollection<TacticalPoint>();
        public static ObservableCollection<TacticalPoint> Points => _points;

        private static EventHandler _modified;
        public static event EventHandler Modified
        {
            add
            {
                _modified += value;
                try
                {
                    if (File.Exists(filename))
                        Load();
                }
                catch { }
            }
            remove { _modified -= value; }
        }

        private static string filename = Settings.GetUserDataDirectory() + "tacticalpoints.txt";
        private static bool loading;

        static TacticalMarkers()
        {
            _points.CollectionChanged += (s, e) =>
            {
                if (!loading)
                    Save();
            };
        }

        public static void Add(PointLatLngAlt pos, TacticalType type, string label)
        {
            if (pos == null) return;

            _points.Add(new TacticalPoint(pos, type, label));

            if (_modified != null && !loading)
                _modified(null, null);
        }

        public static void AddWithDialog(PointLatLngAlt pos, TacticalType type)
        {
            if (pos == null) return;

            string label = "";
            string title = GetTypeName(type);
            if (DialogResult.OK != InputBox.Show(title, "Введите метку:", ref label))
                return;

            Add(pos, type, label);
        }

        public static void Remove(TacticalPoint point)
        {
            if (point == null) return;
            _points.Remove(point);

            if (_modified != null)
                _modified(null, null);
        }

        public static void RemoveAt(GMap.NET.PointLatLng position)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                if (_points[i].Position.Lat == position.Lat && _points[i].Position.Lng == position.Lng)
                {
                    _points.RemoveAt(i);
                    if (_modified != null)
                        _modified(null, null);
                    return;
                }
            }
        }

        public static void Clear()
        {
            _points.Clear();
            if (_modified != null)
                _modified(null, null);
        }

        public static void Save()
        {
            try
            {
                using (Stream file = File.Open(filename, FileMode.Create))
                {
                    foreach (var pt in _points)
                    {
                        string line = pt.Position.Lat.ToString(CultureInfo.InvariantCulture) + "\t" +
                                      pt.Position.Lng.ToString(CultureInfo.InvariantCulture) + "\t" +
                                      (int)pt.Type + "\t" +
                                      pt.Label + "\t" +
                                      pt.Timestamp.ToString("o") + "\r\n";
                        byte[] buffer = ASCIIEncoding.UTF8.GetBytes(line);
                        file.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            catch { }
        }

        public static void Load()
        {
            if (!File.Exists(filename))
                return;

            loading = true;
            try
            {
                using (Stream file = File.Open(filename, FileMode.Open))
                using (StreamReader sr = new StreamReader(file))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] items = sr.ReadLine().Split('\t');
                        if (items.Length < 4)
                            continue;

                        var pos = new PointLatLngAlt(
                            double.Parse(items[0], CultureInfo.InvariantCulture),
                            double.Parse(items[1], CultureInfo.InvariantCulture));
                        var type = (TacticalType)int.Parse(items[2]);
                        var label = items[3];
                        var timestamp = items.Length >= 5
                            ? DateTime.Parse(items[4], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                            : DateTime.UtcNow;

                        _points.Add(new TacticalPoint
                        {
                            Position = pos,
                            Type = type,
                            Label = label,
                            Timestamp = timestamp
                        });
                    }
                }
            }
            catch { }
            loading = false;

            if (_modified != null)
                _modified(null, null);
        }

        public static void UpdateOverlay(GMapOverlay overlay)
        {
            if (overlay == null) return;

            overlay.Clear();

            foreach (var pt in _points)
            {
                var marker = new GMapMarkerTactical(pt.Position.Point(), pt.Type)
                {
                    ToolTipMode = MarkerTooltipMode.OnMouseOver,
                    ToolTipText = GetTypeName(pt.Type) + ": " + pt.Label +
                                  "\n" + pt.Position.Lat.ToString("F6") + ", " + pt.Position.Lng.ToString("F6") +
                                  "\n" + pt.Timestamp.ToLocalTime().ToString("g")
                };
                overlay.Markers.Add(marker);
            }
        }

        public static string GetTypeName(TacticalType type)
        {
            switch (type)
            {
                case TacticalType.Friendly: return "Свои";
                case TacticalType.Enemy: return "Противник";
                case TacticalType.Unknown: return "Неизвестный";
                case TacticalType.Objective: return "Цель";
                default: return type.ToString();
            }
        }
    }
}
