using System;
using System.IO;
using System.Text;

namespace MissionPlanner.Utilities
{
    public static class FlightReport
    {
        /// <summary>
        /// Сгенерировать отчёт о вылете на русском языке
        /// </summary>
        public static string Generate()
        {
            var sb = new StringBuilder();
            var cs = MainV2.comPort.MAV.cs;

            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine("         ОТЧЁТ О ВЫЛЕТЕ");
            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine();

            // Дата и время
            sb.AppendLine($"Дата: {DateTime.Now:dd.MM.yyyy}");
            sb.AppendLine($"Время: {DateTime.Now:HH:mm:ss}");
            sb.AppendLine();

            // Борт
            sb.AppendLine("── БОРТ ──────────────────────────────────");
            sb.AppendLine($"Тип ЛА: {MainV2.comPort.MAV.aptype}");
            sb.AppendLine($"Автопилот: {MainV2.comPort.MAV.apname}");
            sb.AppendLine($"Прошивка: {cs.firmware}");
            sb.AppendLine($"Статус связи: {(cs.connected ? "подключено" : "нет связи")}");
            sb.AppendLine($"Арминг: {(cs.armed ? "ДА" : "НЕТ")}");
            sb.AppendLine();

            // Позиция
            sb.AppendLine("── ПОЗИЦИЯ ───────────────────────────────");
            sb.AppendLine($"Широта: {cs.lat:F7}");
            sb.AppendLine($"Долгота: {cs.lng:F7}");
            sb.AppendLine($"Высота ASL: {cs.altasl:F1} м");
            sb.AppendLine($"Высота AGL: {cs.alt:F1} м");
            sb.AppendLine($"Точка дома: {cs.HomeLocation.Lat:F7}, {cs.HomeLocation.Lng:F7}");
            sb.AppendLine();

            // Полётные данные
            sb.AppendLine("── ПОЛЁТНЫЕ ДАННЫЕ ───────────────────────");
            sb.AppendLine($"Скорость воздушная: {cs.airspeed:F1} м/с");
            sb.AppendLine($"Скорость путевая: {cs.groundspeed:F1} м/с");
            sb.AppendLine($"Курс: {cs.groundcourse:F0}°");
            sb.AppendLine($"Рыскание: {cs.yaw:F0}°");
            sb.AppendLine($"Крен: {cs.roll:F1}°");
            sb.AppendLine($"Тангаж: {cs.pitch:F1}°");
            sb.AppendLine($"Режим полёта: {cs.mode}");
            sb.AppendLine($"Время в полёте: {cs.timeInAir:F0} сек");
            sb.AppendLine();

            // Питание
            sb.AppendLine("── ПИТАНИЕ ───────────────────────────────");
            sb.AppendLine($"Напряжение: {cs.battery_voltage:F1} В");
            sb.AppendLine($"Ток: {cs.current:F1} А");
            sb.AppendLine($"Остаток: {cs.battery_remaining}%");
            sb.AppendLine($"Израсходовано: {cs.battery_usedmah:F0} мАч");
            sb.AppendLine();

            // Навигация
            sb.AppendLine("── НАВИГАЦИЯ ─────────────────────────────");
            sb.AppendLine($"GPS спутники: {cs.satcount:F0}");
            sb.AppendLine($"GPS фикс: {cs.gpsstatus}");
            sb.AppendLine($"HDOP: {cs.gpshdop:F1}");
            sb.AppendLine();

            // Статистика
            sb.AppendLine("── СТАТИСТИКА ────────────────────────────");
            sb.AppendLine($"Дистанция от дома: {cs.DistToHome:F0} м");
            sb.AppendLine($"Путь пройден: {cs.distTraveled:F0} м");
            sb.AppendLine();

            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");

            return sb.ToString();
        }

        /// <summary>
        /// Сгенерировать и сохранить отчёт в файл
        /// </summary>
        /// <returns>Полный путь к сохранённому файлу</returns>
        public static string SaveReport()
        {
            var report = Generate();
            var filename = $"FlightReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var dir = Path.Combine(Settings.GetDataDirectory(), "reports");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, filename);
            File.WriteAllText(path, report, Encoding.UTF8);
            return path;
        }
    }
}
