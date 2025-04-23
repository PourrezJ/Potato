using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using System.Linq;

namespace Potato.Core.Logging
{
    /// <summary>
    /// Type de message de journal
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }
    
    /// <summary>
    /// Catégorie de message pour faciliter le filtrage
    /// </summary>
    public enum LogCategory
    {
        Core,
        Physics,
        Graphics,
        Audio,
        Input,
        UI,
        Gameplay,
        AI,
        Network,
        Performance,
        System
    }
    
    /// <summary>
    /// Une entrée de journal avec toutes les informations associées
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public LogCategory Category { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        
        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level}] [{Category}] [{Source}] {Message}";
        }
        
        public string ToShortString()
        {
            return $"[{Timestamp:HH:mm:ss}] [{Level}] {Message}";
        }
        
        // Formater un message court pour l'affichage dans l'interface
        public string FormatForUI()
        {
            Color color = GetColorForLevel(Level);
            return $"[{Timestamp:HH:mm:ss}] {Message}";
        }
        
        // Obtenir la couleur associée au niveau de journal
        public static Color GetColorForLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return Color.Gray;
                case LogLevel.Info:
                    return Color.White;
                case LogLevel.Warning:
                    return Color.Yellow;
                case LogLevel.Error:
                    return Color.OrangeRed;
                case LogLevel.Critical:
                    return Color.Red;
                default:
                    return Color.White;
            }
        }
    }
    
    /// <summary>
    /// Système de journalisation centralisé avec support pour plusieurs canaux de sortie
    /// </summary>
    public static class Logger
    {
        // Configuration
        private static LogLevel _minimumLevel = LogLevel.Error;
        private static bool _logToConsole = true;
        private static bool _logToFile = true;
        private static bool _logToUI = true;
        private static string _logFolder = "Logs";
        private static string _logFile;
        
        /// <summary>
        /// Gets or sets whether file logging is enabled
        /// </summary>
        public static bool EnableFileLogging
        {
            get { return _logToFile; }
            set { _logToFile = value; }
        }
        
        // État
        private static List<LogEntry> _recentLogs = new List<LogEntry>(1000); // Buffer circulaire
        private static int _maxRecentLogs = 1000;
        private static Dictionary<LogCategory, bool> _categoryFilters = new Dictionary<LogCategory, bool>();
        private static List<Action<LogEntry>> _logListeners = new List<Action<LogEntry>>();
        
        static Logger()
        {
            // Initialiser les filtres de catégorie
            foreach (LogCategory category in Enum.GetValues(typeof(LogCategory)))
            {
                _categoryFilters[category] = true; // Toutes les catégories sont activées par défaut
            }
            
            // Créer un nouveau fichier journal avec la date
            InitializeLogFile();
        }
        
        /// <summary>
        /// Initialise le fichier journal avec un horodatage
        /// </summary>
        private static void InitializeLogFile()
        {
            // Créer le dossier de journaux s'il n'existe pas
            if (!Directory.Exists(_logFolder))
            {
                Directory.CreateDirectory(_logFolder);
            }
            
            // Créer un nom de fichier unique avec la date et l'heure
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _logFile = Path.Combine(_logFolder, $"potato_log_{timestamp}.txt");
            
            // Ajouter un en-tête au fichier journal
            if (_logToFile)
            {
                try
                {
                    File.WriteAllText(_logFile, $"=== Potato Game Log - {timestamp} ===\n\n");
                }
                catch (Exception ex)
                {
                    // Désactiver la journalisation de fichier en cas d'erreur
                    _logToFile = false;
                    Console.WriteLine($"Error creating log file: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Configure les options de journalisation
        /// </summary>
        public static void Configure(LogLevel minLevel = LogLevel.Debug, bool logToConsole = true, bool logToFile = true, bool logToUI = true)
        {
            _minimumLevel = minLevel;
            _logToConsole = logToConsole;
            _logToFile = logToFile;
            _logToUI = logToUI;
        }
        
        /// <summary>
        /// Définit le filtre pour une catégorie spécifique
        /// </summary>
        public static void SetCategoryFilter(LogCategory category, bool enabled)
        {
            _categoryFilters[category] = enabled;
        }
        
        /// <summary>
        /// Ajoute un écouteur pour recevoir des notifications pour chaque entrée de journal
        /// </summary>
        public static void AddListener(Action<LogEntry> listener)
        {
            if (listener != null && !_logListeners.Contains(listener))
            {
                _logListeners.Add(listener);
            }
        }
        
        /// <summary>
        /// Supprime un écouteur
        /// </summary>
        public static void RemoveListener(Action<LogEntry> listener)
        {
            _logListeners.Remove(listener);
        }
        
        /// <summary>
        /// Consigne un message de journal
        /// </summary>
        public static void Log(LogLevel level, LogCategory category, string message, string source = "")
        {
            // Vérifier le niveau et le filtre de catégorie
            if (level < _minimumLevel || !_categoryFilters[category])
            {
                return;
            }
            
            // Créer une entrée de journal
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Category = category,
                Message = message,
                Source = source
            };
            
            // Ajouter au buffer circulaire
            _recentLogs.Add(entry);
            if (_recentLogs.Count > _maxRecentLogs)
            {
                _recentLogs.RemoveAt(0);
            }
            
            // Journal console si activé
            if (_logToConsole)
            {
                // Définir la couleur de la console en fonction du niveau
                ConsoleColor originalColor = Console.ForegroundColor;
                ConsoleColor logColor = GetConsoleColor(level);
                Console.ForegroundColor = logColor;
                
                Console.WriteLine(entry.ToString());
                
                // Rétablir la couleur d'origine
                Console.ForegroundColor = originalColor;
            }
            
            // Journal fichier si activé
            if (_logToFile)
            {
                try
                {
                    File.AppendAllText(_logFile, entry.ToString() + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    // Désactiver la journalisation de fichier en cas d'erreur
                    _logToFile = false;
                    Console.WriteLine($"Error writing to log file: {ex.Message}");
                }
            }
            
            // Notifier les écouteurs
            foreach (var listener in _logListeners)
            {
                try
                {
                    listener(entry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in log listener: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Consigne un message de niveau Debug
        /// </summary>
        public static void Debug(string message, LogCategory category = LogCategory.Core, string source = "")
        {
            Log(LogLevel.Debug, category, message, source);
        }
        
        /// <summary>
        /// Consigne un message de niveau Info
        /// </summary>
        public static void Info(string message, LogCategory category = LogCategory.Core, string source = "")
        {
            Log(LogLevel.Info, category, message, source);
        }
        
        /// <summary>
        /// Consigne un message de niveau Warning
        /// </summary>
        public static void Warning(string message, LogCategory category = LogCategory.Core, string source = "")
        {
            Log(LogLevel.Warning, category, message, source);
        }
        
        /// <summary>
        /// Consigne un message de niveau Error
        /// </summary>
        public static void Error(string message, LogCategory category = LogCategory.Core, string source = "")
        {
            Log(LogLevel.Error, category, message, source);
        }
        
        /// <summary>
        /// Consigne un message de niveau Critical
        /// </summary>
        public static void Critical(string message, LogCategory category = LogCategory.Core, string source = "")
        {
            Log(LogLevel.Critical, category, message, source);
        }
        
        /// <summary>
        /// Consigne une exception
        /// </summary>
        public static void Exception(Exception ex, LogCategory category = LogCategory.Core, string source = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Exception: {ex.GetType().Name}");
            sb.AppendLine($"Message: {ex.Message}");
            sb.AppendLine($"StackTrace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                sb.AppendLine($"Inner Exception: {ex.InnerException.GetType().Name}");
                sb.AppendLine($"Inner Message: {ex.InnerException.Message}");
            }
            
            Log(LogLevel.Error, category, sb.ToString(), source);
        }
        
        /// <summary>
        /// Obtient les entrées de journal récentes avec filtrage optionnel
        /// </summary>
        public static List<LogEntry> GetRecentLogs(LogLevel? minLevel = null, LogLevel? maxLevel = null, LogCategory? category = null, int maxCount = int.MaxValue)
        {
            IEnumerable<LogEntry> filteredLogs = _recentLogs;
            
            if (minLevel.HasValue)
            {
                filteredLogs = filteredLogs.Where(log => log.Level >= minLevel.Value);
            }
            
            if (maxLevel.HasValue)
            {
                filteredLogs = filteredLogs.Where(log => log.Level <= maxLevel.Value);
            }
            
            if (category.HasValue)
            {
                filteredLogs = filteredLogs.Where(log => log.Category == category.Value);
            }
            
            return filteredLogs.TakeLast(maxCount).ToList();
        }
        
        /// <summary>
        /// Vide le buffer des journaux récents
        /// </summary>
        public static void ClearRecentLogs()
        {
            _recentLogs.Clear();
        }
        
        /// <summary>
        /// Convertit un LogLevel en couleur de console
        /// </summary>
        private static ConsoleColor GetConsoleColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return ConsoleColor.Gray;
                case LogLevel.Info:
                    return ConsoleColor.White;
                case LogLevel.Warning:
                    return ConsoleColor.Yellow;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                case LogLevel.Critical:
                    return ConsoleColor.DarkRed;
                default:
                    return ConsoleColor.White;
            }
        }
        
        /// <summary>
        /// Shuts down the logger and performs any necessary cleanup
        /// </summary>
        public static void Shutdown()
        {
            // Log shutdown message
            if (_logToFile)
            {
                try
                {
                    string message = $"=== Logger shutdown at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===";
                    File.AppendAllText(_logFile, Environment.NewLine + message + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing shutdown message to log file: {ex.Message}");
                }
            }
            
            // Clear resources
            _logListeners.Clear();
            _recentLogs.Clear();
        }
    }
}