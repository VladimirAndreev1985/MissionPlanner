using System;
using System.Collections.Generic;

namespace MissionPlanner.Utilities
{
    public enum PresetType
    {
        Reconnaissance,  // Разведка — облёт периметра заданного квадрата
        Patrol,          // Патрулирование — кольцевой маршрут с барражированием
        AreaSurvey,      // Обследование области — змейка
        PointObserve,    // Наблюдение точки — полёт к точке + барражирование
        QuickReturn      // Быстрый возврат — RTL
    }

    public static class MissionPresets
    {
        /// <summary>
        /// Get display name in Russian
        /// </summary>
        public static string GetName(PresetType type)
        {
            switch (type)
            {
                case PresetType.Reconnaissance: return "Разведка периметра";
                case PresetType.Patrol: return "Патрулирование";
                case PresetType.AreaSurvey: return "Обследование области";
                case PresetType.PointObserve: return "Наблюдение точки";
                case PresetType.QuickReturn: return "Быстрый возврат (RTL)";
                default: return type.ToString();
            }
        }

        /// <summary>
        /// Get description in Russian
        /// </summary>
        public static string GetDescription(PresetType type)
        {
            switch (type)
            {
                case PresetType.Reconnaissance:
                    return "Облёт прямоугольного периметра вокруг заданной точки на заданной высоте. Подходит для осмотра территории.";
                case PresetType.Patrol:
                    return "Кольцевой маршрут с точками барражирования. БПА патрулирует заданную зону циклически.";
                case PresetType.AreaSurvey:
                    return "Маршрут \"змейкой\" для полного покрытия прямоугольной области. Подходит для аэрофотосъёмки.";
                case PresetType.PointObserve:
                    return "Полёт к указанной точке и барражирование над ней. Подходит для длительного наблюдения.";
                case PresetType.QuickReturn:
                    return "Немедленный возврат на точку старта.";
                default: return "";
            }
        }

        /// <summary>
        /// Generate waypoint list for a preset.
        /// centerLat/centerLng = target point, altitude in meters, radius in meters.
        /// Returns list of (lat, lng, alt, command) tuples where command is MAVLink MAV_CMD ushort value.
        /// MAV_CMD values: WAYPOINT=16, LOITER_UNLIM=17, LOITER_TURNS=18, RETURN_TO_LAUNCH=20
        /// </summary>
        public static List<(double lat, double lng, double alt, ushort cmd)>
            GenerateWaypoints(PresetType type, double centerLat, double centerLng,
                              double altitude = 100, double radius = 200)
        {
            var waypoints = new List<(double, double, double, ushort)>();

            switch (type)
            {
                case PresetType.Reconnaissance:
                    // 4 corners of rectangle around center
                    double dlat = radius / 111320.0;
                    double dlng = radius / (111320.0 * Math.Cos(centerLat * Math.PI / 180));
                    waypoints.Add((centerLat + dlat, centerLng - dlng, altitude, 16)); // NW
                    waypoints.Add((centerLat + dlat, centerLng + dlng, altitude, 16)); // NE
                    waypoints.Add((centerLat - dlat, centerLng + dlng, altitude, 16)); // SE
                    waypoints.Add((centerLat - dlat, centerLng - dlng, altitude, 16)); // SW
                    waypoints.Add((centerLat + dlat, centerLng - dlng, altitude, 16)); // back to NW
                    waypoints.Add((0, 0, 0, 20)); // RTL
                    break;

                case PresetType.Patrol:
                    // Circle of 6 points
                    for (int i = 0; i < 6; i++)
                    {
                        double angle = i * 60.0 * Math.PI / 180.0;
                        double lat = centerLat + (radius / 111320.0) * Math.Cos(angle);
                        double lng = centerLng + (radius / (111320.0 * Math.Cos(centerLat * Math.PI / 180))) * Math.Sin(angle);
                        waypoints.Add((lat, lng, altitude, 16));
                    }
                    // Return to first point for cycling
                    waypoints.Add((waypoints[0].Item1, waypoints[0].Item2, altitude, 16));
                    break;

                case PresetType.AreaSurvey:
                    // Serpentine/lawnmower pattern
                    double dlatS = radius / 111320.0;
                    double dlngS = radius / (111320.0 * Math.Cos(centerLat * Math.PI / 180));
                    int lines = 5;
                    double spacing = 2.0 * dlatS / (lines - 1);
                    for (int i = 0; i < lines; i++)
                    {
                        double lat = centerLat - dlatS + i * spacing;
                        if (i % 2 == 0)
                        {
                            waypoints.Add((lat, centerLng - dlngS, altitude, 16));
                            waypoints.Add((lat, centerLng + dlngS, altitude, 16));
                        }
                        else
                        {
                            waypoints.Add((lat, centerLng + dlngS, altitude, 16));
                            waypoints.Add((lat, centerLng - dlngS, altitude, 16));
                        }
                    }
                    waypoints.Add((0, 0, 0, 20)); // RTL
                    break;

                case PresetType.PointObserve:
                    waypoints.Add((centerLat, centerLng, altitude, 16)); // fly to point
                    waypoints.Add((centerLat, centerLng, altitude, 17)); // loiter unlimited
                    break;

                case PresetType.QuickReturn:
                    waypoints.Add((0, 0, 0, 20)); // RTL only
                    break;
            }

            return waypoints;
        }
    }
}
