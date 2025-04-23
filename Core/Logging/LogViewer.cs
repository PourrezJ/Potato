using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Potato.Core.Logging
{
    public class LogViewer
    {
        private Game _game;
        private SpriteFont _font;
        private bool _isVisible = false;
        private int _maxDisplayedLogs = 20;
        private float _opacity = 0.8f;
        private Color _backgroundColor = new Color(10, 10, 30, 200);
        private Dictionary<LogLevel, Color> _logColors = new Dictionary<LogLevel, Color>();
        private int _scrollOffset = 0;
        private KeyboardState? _previousKeyboardState;

        public LogViewer(Game game, SpriteFont font)
        {
            _game = game;
            _font = font;
            
            // Initialiser les couleurs pour chaque niveau de log
            _logColors[LogLevel.Debug] = Color.Gray;
            _logColors[LogLevel.Info] = Color.White;
            _logColors[LogLevel.Warning] = Color.Yellow;
            _logColors[LogLevel.Error] = Color.Red;
            _logColors[LogLevel.Critical] = Color.DarkRed;
        }

        public void Update(GameTime gameTime)
        {
            if (!_isVisible)
                return;

            // Gestion des touches pour le défilement
            KeyboardState currentKeyboardState = Keyboard.GetState();
            
            // Gestion du défilement avec les touches fléchées
            if (currentKeyboardState.IsKeyDown(Keys.Up) && 
                (_previousKeyboardState.HasValue == false || _previousKeyboardState.Value.IsKeyUp(Keys.Up)))
            {
                _scrollOffset = Math.Max(0, _scrollOffset - 1);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Down) && 
                (_previousKeyboardState.HasValue == false || _previousKeyboardState.Value.IsKeyUp(Keys.Down)))
            {
                _scrollOffset++;
            }
            else if (currentKeyboardState.IsKeyDown(Keys.PageUp) && 
                (_previousKeyboardState.HasValue == false || _previousKeyboardState.Value.IsKeyUp(Keys.PageUp)))
            {
                _scrollOffset = Math.Max(0, _scrollOffset - 10);
            }
            else if (currentKeyboardState.IsKeyDown(Keys.PageDown) && 
                (_previousKeyboardState.HasValue == false || _previousKeyboardState.Value.IsKeyUp(Keys.PageDown)))
            {
                _scrollOffset += 10;
            }
            else if (currentKeyboardState.IsKeyDown(Keys.Home) && 
                (_previousKeyboardState.HasValue == false || _previousKeyboardState.Value.IsKeyUp(Keys.Home)))
            {
                _scrollOffset = 0;
            }
            
            _previousKeyboardState = currentKeyboardState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isVisible)
                return;

            int width = _game.GraphicsDevice.Viewport.Width;
            int height = _game.GraphicsDevice.Viewport.Height;
            
            // Dessiner le fond
            Texture2D pixel = new Texture2D(_game.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            spriteBatch.Draw(pixel, new Rectangle(0, 0, width, height / 2), _backgroundColor * _opacity);
            
            // Récupérer les logs récents
            List<LogEntry> logEntries = Logger.GetRecentLogs();
            List<string> logs = logEntries.Select(entry => entry.ToString()).ToList();
            
            // S'assurer que le défilement ne dépasse pas la limite des logs disponibles
            int maxScroll = Math.Max(0, logs.Count - _maxDisplayedLogs);
            _scrollOffset = Math.Min(_scrollOffset, maxScroll);
            
            // Afficher les logs avec défilement
            float y = 10;
            int startIndex = Math.Max(0, logs.Count - _maxDisplayedLogs - _scrollOffset);
            int endIndex = Math.Min(logs.Count, startIndex + _maxDisplayedLogs);
            
            for (int i = startIndex; i < endIndex; i++)
            {
                string log = logs[i];
                
                // Déterminer la couleur en fonction du niveau de log
                Color textColor = _logColors[LogLevel.Info]; // Couleur par défaut
                
                // Analyser le niveau de log dans la chaîne (format: "[LEVEL]")
                foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
                {
                    if (log.Contains($"[{level}]") && _logColors.ContainsKey(level))
                    {
                        textColor = _logColors[level];
                        break;
                    }
                }
                
                // Tronquer le log s'il est trop long
                if (log.Length > 150)
                {
                    log = log.Substring(0, 147) + "...";
                }
                
                spriteBatch.DrawString(_font, log, new Vector2(10, y), textColor);
                y += _font.LineSpacing;
                
                // Arrêter si on dépasse la moitié de l'écran
                if (y > height / 2 - _font.LineSpacing)
                    break;
            }
            
            // Afficher les informations de défilement
            string scrollInfo = $"Logs: {logs.Count} | Offset: {_scrollOffset} | Flèches: Défiler | Home: Début | F12: Cacher";
            spriteBatch.DrawString(_font, scrollInfo, new Vector2(10, height / 2 - _font.LineSpacing - 10), Color.LightGray);
        }

        public void Toggle()
        {
            _isVisible = !_isVisible;
            Logger.Info($"Log viewer {(_isVisible ? "visible" : "hidden")}");
        }
        
        public bool IsVisible => _isVisible;
    }
}