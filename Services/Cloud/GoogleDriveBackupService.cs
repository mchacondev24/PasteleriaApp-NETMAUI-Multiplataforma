using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PasteleriaApp.Data;

namespace PasteleriaApp.Services.Cloud
{
    public class GoogleDriveBackupService : IDisposable
    {
        private readonly DatabaseService _databaseService;
        private Timer? _timer;
        private bool _isTimerRunning = false;
        private string _targetHour = "22:00"; // default 10 PM

        public bool IsTimerRunning => _isTimerRunning;

        public event Action<string>? OnBackupCompleted;
        public event Action<string>? OnBackupError;

        public GoogleDriveBackupService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public void StartScheduler(string targetHour = "22:00", bool enabled = true)
        {
            _targetHour = targetHour;
            _timer?.Dispose();
            if (!enabled) return;

            // Check every 1 minute
            _timer = new Timer(async _ => await CheckBackupTimeAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            _isTimerRunning = true;
        }

        public void StopScheduler()
        {
            _timer?.Dispose();
            _timer = null;
            _isTimerRunning = false;
        }

        private async Task CheckBackupTimeAsync()
        {
            var currentTime = DateTime.Now.ToString("HH:mm");
            if (currentTime == _targetHour)
            {
                await ExecuteBackupAsync("Respaldo_Programado_GoogleDrive");
            }
        }

        public async Task<string> ExecuteBackupAsync(string reason = "Manual")
        {
            try
            {
                var backupJson = await _databaseService.ExportDatabaseJsonAsync();
                string baseDir;
                try
                {
                    baseDir = FileSystem.AppDataDirectory;
                }
                catch
                {
                    baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                }
                var backupDir = Path.Combine(baseDir, "PasteleriaApp", "Backups");
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                var fileName = $"Backup_Pasteleria_{DateTime.Now:yyyyMMdd_HHmmss}_{reason}.json";
                var filePath = Path.Combine(backupDir, fileName);
                await File.WriteAllTextAsync(filePath, backupJson);

                var msg = $"Respaldo generado con éxito ({fileName}). Preparado para sincronización en la nube de Google Drive.";
                OnBackupCompleted?.Invoke(msg);
                return filePath;
            }
            catch (Exception ex)
            {
                var errMsg = $"Error en respaldo automático: {ex.Message}";
                OnBackupError?.Invoke(errMsg);
                return string.Empty;
            }
        }

        public void Dispose()
        {
            StopScheduler();
        }
    }
}
