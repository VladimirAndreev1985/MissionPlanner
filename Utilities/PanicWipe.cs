using System;
using System.IO;

namespace MissionPlanner.Utilities
{
    /// <summary>
    /// Экстренное уничтожение данных.
    /// Безвозвратно удаляет все журналы, конфигурации, кэш карт,
    /// ключи авторизации, маршруты, параметры и прочие рабочие файлы.
    /// </summary>
    public static class PanicWipe
    {
        /// <summary>
        /// Выполнить экстренное уничтожение всех данных.
        /// Метод никогда не выбрасывает исключений.
        /// </summary>
        public static void Execute()
        {
            try
            {
                string userDataDir = Settings.GetUserDataDirectory();
                string dataDir = Settings.GetDataDirectory();
                string runDir = Settings.GetRunningDirectory();
                string logDir = Settings.GetDefaultLogDir();

                // 1. Журналы (tlog, bin, log, rlog)
                SecureDeleteDirectory(logDir);

                // 2. config.xml — основные настройки
                SecureDeleteFile(Path.Combine(userDataDir, "config.xml"));
                SecureDeleteFile(Path.Combine(runDir, "config.xml"));

                // 3. authkeys.xml — ключи подписи MAVLink
                SecureDeleteFile(Path.Combine(userDataDir, "authkeys.xml"));

                // 4. Кэш карт (gmapcache)
                SecureDeleteDirectory(Path.Combine(userDataDir, "gmapcache"));
                SecureDeleteDirectory(Path.Combine(dataDir, "gmapcache"));

                // 5. poi.txt — точки интереса
                SecureDeleteFile(Path.Combine(userDataDir, "poi.txt"));
                SecureDeleteFile(Path.Combine(runDir, "poi.txt"));

                // 6. stats.xml — статистика
                SecureDeleteFile(Path.Combine(userDataDir, "stats.xml"));

                // 7. Маршруты, параметры, геозоны, ограждения
                WipeFilesByExtension(userDataDir, "*.waypoints");
                WipeFilesByExtension(userDataDir, "*.param");
                WipeFilesByExtension(userDataDir, "*.rally");
                WipeFilesByExtension(userDataDir, "*.fen");
                WipeFilesByExtension(runDir, "*.waypoints");
                WipeFilesByExtension(runDir, "*.param");
                WipeFilesByExtension(runDir, "*.rally");
                WipeFilesByExtension(runDir, "*.fen");

                // 8. KML/KMZ файлы
                WipeFilesByExtension(userDataDir, "*.kml");
                WipeFilesByExtension(userDataDir, "*.kmz");
                WipeFilesByExtension(runDir, "*.kml");
                WipeFilesByExtension(runDir, "*.kmz");

                // 9. MissionPlanner.log из ProgramData
                SecureDeleteFile(Path.Combine(dataDir, "MissionPlanner.log"));

                // 10. trace.log, network.log
                SecureDeleteFile(Path.Combine(userDataDir, "trace.log"));
                SecureDeleteFile(Path.Combine(userDataDir, "network.log"));
                SecureDeleteFile(Path.Combine(runDir, "trace.log"));
                SecureDeleteFile(Path.Combine(runDir, "network.log"));

                // 11. tlogimagecache
                SecureDeleteDirectory(Path.Combine(userDataDir, "tlogimagecache"));
                SecureDeleteDirectory(Path.Combine(runDir, "tlogimagecache"));

                // 12. followmeraw.txt, MovingBase.txt
                SecureDeleteFile(Path.Combine(userDataDir, "followmeraw.txt"));
                SecureDeleteFile(Path.Combine(runDir, "followmeraw.txt"));
                SecureDeleteFile(Path.Combine(userDataDir, "MovingBase.txt"));
                SecureDeleteFile(Path.Combine(runDir, "MovingBase.txt"));

                // 13. Конфигурации джойстиков
                WipeFilesByExtension(userDataDir, "*.joy");
                WipeFilesByExtension(runDir, "*.joy");
                WipeFilesByExtension(userDataDir, "joystick*.xml");
                WipeFilesByExtension(runDir, "joystick*.xml");

                // 14. Прочие конфигурационные XML в каталоге данных
                WipeFilesByExtension(userDataDir, "*.xml");

                // 15. SRTM-кэш рельефа
                SecureDeleteDirectory(Path.Combine(userDataDir, "srtm"));
                SecureDeleteDirectory(Path.Combine(dataDir, "srtm"));

                // 16. custom.config.xml
                SecureDeleteFile(Path.Combine(runDir, "custom.config.xml"));
            }
            catch
            {
                // Никогда не выбрасывать исключение
            }
        }

        /// <summary>
        /// Безопасное удаление файла: перезапись нулями, затем удаление.
        /// </summary>
        private static void SecureDeleteFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return;

                var fi = new FileInfo(path);
                long length = fi.Length;

                if (length > 0)
                {
                    // Снять атрибут «только чтение»
                    fi.Attributes = FileAttributes.Normal;

                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None))
                    {
                        byte[] zeros = new byte[Math.Min(length, 65536)];
                        long remaining = length;
                        while (remaining > 0)
                        {
                            int toWrite = (int)Math.Min(remaining, zeros.Length);
                            fs.Write(zeros, 0, toWrite);
                            remaining -= toWrite;
                        }
                        fs.Flush(true);
                    }
                }

                File.Delete(path);
            }
            catch
            {
                // Продолжить уничтожение остальных данных
                try { File.Delete(path); } catch { }
            }
        }

        /// <summary>
        /// Рекурсивное безопасное удаление каталога.
        /// </summary>
        private static void SecureDeleteDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    return;

                foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    SecureDeleteFile(file);
                }

                Directory.Delete(path, true);
            }
            catch
            {
                try { Directory.Delete(path, true); } catch { }
            }
        }

        /// <summary>
        /// Удалить все файлы по маске в указанном каталоге (без рекурсии).
        /// </summary>
        private static void WipeFilesByExtension(string directory, string pattern)
        {
            try
            {
                if (!Directory.Exists(directory))
                    return;

                foreach (string file in Directory.GetFiles(directory, pattern, SearchOption.TopDirectoryOnly))
                {
                    SecureDeleteFile(file);
                }
            }
            catch
            {
                // Продолжить
            }
        }
    }
}
