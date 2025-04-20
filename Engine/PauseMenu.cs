using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Potato.Core.Logging;
using Potato.Core;
using Potato.Core.UI;

namespace Potato.Engine
{
    /// <summary>
    /// Gère le menu pause du jeu avec une approche simplifiée utilisant des rectangles
    /// pour la détection des clics et un rendu direct.
    /// </summary>
    public class PauseMenu : GameBehaviour
    {
        // Références et états
        private bool _showingOptions;
        private SpriteFont _font;
        private Texture2D _panel;
        private Texture2D _buttonTexture;
        private MouseState _previousMouseState;
        private bool _isVisible;
        
        // Rectangles des boutons
        private Rectangle _resumeButtonRect;
        private Rectangle _optionsButtonRect;
        private Rectangle _quitButtonRect;
        private Rectangle _backButtonRect;
        
        // Options et sliders
        private float _musicVolume = 0.8f;
        private float _soundVolume = 0.6f;
        private bool _fullscreen = true;
        private Rectangle _musicSliderRect;
        private Rectangle _soundSliderRect;
        private Rectangle _fullscreenToggleRect;
        private bool _isDraggingMusicSlider;
        private bool _isDraggingSoundSlider;
        
        // Dimensions et style du menu
        private readonly int _menuWidth = 450;
        private readonly int _menuHeight = 550;
        private readonly int _buttonWidth = 280;
        private readonly int _buttonHeight = 60;
        
        // Couleurs des boutons
        private readonly Color _resumeButtonColor = new Color(40, 120, 40, 220);
        private readonly Color _optionsButtonColor = new Color(60, 60, 120, 220);
        private readonly Color _quitButtonColor = new Color(120, 40, 40, 220);
        private readonly Color _backButtonColor = new Color(70, 70, 140, 220);
        private readonly Color _buttonBorderColor = new Color(255, 255, 255, 178); // White * 0.7f
        
        /// <summary>
        /// Événement déclenché lorsque le joueur clique sur le bouton Resume
        /// </summary>
        public event Action OnResume;
        
        /// <summary>
        /// Événement déclenché lorsque le joueur clique sur le bouton Quit
        /// </summary>
        public event Action OnQuit;
        
        /// <summary>
        /// Crée une nouvelle instance du menu pause
        /// </summary>
        public PauseMenu()
        {
            _showingOptions = false;
            _isVisible = false;
        }
        
        public override void Awake()
        {
            base.Awake();
                 
            // Créer les textures de base et calculer les positions des boutons
            CreateTextures();
            CalculateButtonPositions();
            
            Logger.Instance.Info("Menu pause initialisé avec l'approche simplifiée", LogCategory.UI);
        }
        
        /// <summary>
        /// Crée les textures de base pour le menu pause
        /// </summary>
        private void CreateTextures()
        {
            // Créer une texture pour le panneau principal (couleur bleu foncé semi-transparente)
            _panel = new Texture2D(_game.GraphicsDevice, 1, 1);
            _panel.SetData(new[] { new Color(15, 15, 35, 240) });
            
            // Créer une texture blanche pour les boutons et éléments UI
            _buttonTexture = new Texture2D(_game.GraphicsDevice, 1, 1);
            _buttonTexture.SetData(new[] { Color.White });
        }
        
        /// <summary>
        /// Calcule les positions des boutons en fonction de la taille de l'écran
        /// </summary>
        private void CalculateButtonPositions()
        {
            // Obtenir les dimensions de l'écran
            int screenWidth = _game.GraphicsDevice.Viewport.Width;
            int screenHeight = _game.GraphicsDevice.Viewport.Height;
            
            // Position du panneau central
            int panelX = screenWidth / 2 - _menuWidth / 2;
            int panelY = screenHeight / 2 - _menuHeight / 2;
            
            // Position des boutons (centrés horizontalement dans le panneau)
            int buttonX = panelX + (_menuWidth - _buttonWidth) / 2;
            
            // Créer les rectangles pour chaque bouton
            // Bouton Resume (en haut)
            _resumeButtonRect = new Rectangle(
                buttonX,
                panelY + 150,
                _buttonWidth,
                _buttonHeight
            );
            
            // Bouton Options (au milieu)
            _optionsButtonRect = new Rectangle(
                buttonX,
                panelY + 150 + _buttonHeight + 20, // Ajouter un espacement de 20px
                _buttonWidth,
                _buttonHeight
            );
            
            // Bouton Quit (en bas)
            _quitButtonRect = new Rectangle(
                buttonX,
                panelY + 150 + 2 * (_buttonHeight + 20), // Deux espacements
                _buttonWidth,
                _buttonHeight
            );
            
            // Bouton Back pour l'écran d'options
            _backButtonRect = new Rectangle(
                buttonX,
                panelY + 420, // Position Y fixe en bas du panneau
                _buttonWidth,
                _buttonHeight
            );
            
            // Rectangles des sliders et toggle dans le menu d'options
            _musicSliderRect = new Rectangle(panelX + 200, panelY + 165, 180, 20);
            _soundSliderRect = new Rectangle(panelX + 200, panelY + 225, 180, 20);
            _fullscreenToggleRect = new Rectangle(panelX + 200, panelY + 285, 40, 20);
        }
        
        /// <summary>
        /// Affiche le menu pause
        /// </summary>
        public void Show()
        {
            _isVisible = true;
            _showingOptions = false; // Toujours revenir au menu principal lors de l'ouverture
            Logger.Instance.Info("Menu pause affiché", LogCategory.UI);
        }
        
        /// <summary>
        /// Cache le menu pause
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
            Logger.Instance.Info("Menu pause masqué", LogCategory.UI);
        }
        
        /// <summary>
        /// Met à jour le menu pause, gère la détection des clics
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if (!_isVisible)
                return;
                
            MouseState currentMouseState = Mouse.GetState();
            Point mousePosition = new Point(currentMouseState.X, currentMouseState.Y);
            
            // Gestion des clics
            if (currentMouseState.LeftButton == ButtonState.Released && _previousMouseState.LeftButton == ButtonState.Pressed)
            {
                Logger.Instance.Info($"🖱️ CLIC à {mousePosition.X}, {mousePosition.Y}", LogCategory.UI);
                
                // Traiter les clics selon l'écran actuel
                if (!_showingOptions)
                {
                    ProcessMainMenuClicks(mousePosition);
                }
                else
                {
                    ProcessOptionsMenuClicks(mousePosition);
                }
                
                // Réinitialiser l'état des sliders
                _isDraggingMusicSlider = false;
                _isDraggingSoundSlider = false;
            }
            // Détection début de clic (pour les sliders)
            else if (currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                if (_showingOptions)
                {
                    // Vérifier si on clique sur un slider
                    if (_musicSliderRect.Contains(mousePosition))
                    {
                        _isDraggingMusicSlider = true;
                        UpdateSliderValue(_musicSliderRect, mousePosition.X, ref _musicVolume);
                        Logger.Instance.Info($"🎵 Slider musique cliqué: {_musicVolume}", LogCategory.UI);
                    }
                    else if (_soundSliderRect.Contains(mousePosition))
                    {
                        _isDraggingSoundSlider = true;
                        UpdateSliderValue(_soundSliderRect, mousePosition.X, ref _soundVolume);
                        Logger.Instance.Info($"🔊 Slider son cliqué: {_soundVolume}", LogCategory.UI);
                    }
                    else if (_fullscreenToggleRect.Contains(mousePosition))
                    {
                        _fullscreen = !_fullscreen;
                        Logger.Instance.Info($"📺 Toggle plein écran: {_fullscreen}", LogCategory.UI);
                    }
                }
            }
            // Gestion du glissement (dragging) des sliders
            else if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                if (_isDraggingMusicSlider)
                {
                    UpdateSliderValue(_musicSliderRect, mousePosition.X, ref _musicVolume);
                }
                else if (_isDraggingSoundSlider)
                {
                    UpdateSliderValue(_soundSliderRect, mousePosition.X, ref _soundVolume);
                }
            }
            
            _previousMouseState = currentMouseState;
        }
        
        /// <summary>
        /// Met à jour la valeur d'un slider en fonction de la position X de la souris
        /// </summary>
        private void UpdateSliderValue(Rectangle sliderRect, int mouseX, ref float value)
        {
            if (mouseX <= sliderRect.X)
            {
                value = 0f;
            }
            else if (mouseX >= sliderRect.X + sliderRect.Width)
            {
                value = 1f;
            }
            else
            {
                value = (float)(mouseX - sliderRect.X) / sliderRect.Width;
            }
        }
        
        /// <summary>
        /// Traite les clics dans le menu principal
        /// </summary>
        private void ProcessMainMenuClicks(Point mousePosition)
        {
            // Afficher les rectangles des boutons pour le débogage
            Logger.Instance.Debug($"📋 Resume: {_resumeButtonRect}", LogCategory.UI);
            Logger.Instance.Debug($"📋 Options: {_optionsButtonRect}", LogCategory.UI);
            Logger.Instance.Debug($"📋 Quit: {_quitButtonRect}", LogCategory.UI);
            
            // Vérifier chaque bouton
            if (_resumeButtonRect.Contains(mousePosition))
            {
                Logger.Instance.Info("✅ Bouton RESUME cliqué", LogCategory.UI);
                Hide();
                OnResume?.Invoke();
            }
            else if (_optionsButtonRect.Contains(mousePosition))
            {
                Logger.Instance.Info("✅ Bouton OPTIONS cliqué", LogCategory.UI);
                _showingOptions = true;
            }
            else if (_quitButtonRect.Contains(mousePosition))
            {
                Logger.Instance.Info("✅ Bouton QUIT cliqué", LogCategory.UI);
                Hide();
                OnQuit?.Invoke();
            }
        }
        
        /// <summary>
        /// Traite les clics dans le menu d'options
        /// </summary>
        private void ProcessOptionsMenuClicks(Point mousePosition)
        {
            // Afficher le rectangle du bouton Back pour le débogage
            Logger.Instance.Debug($"📋 Back: {_backButtonRect}", LogCategory.UI);
            
            // Vérifier le bouton Back
            if (_backButtonRect.Contains(mousePosition))
            {
                Logger.Instance.Info("✅ Bouton BACK cliqué", LogCategory.UI);
                ApplySettings();
                _showingOptions = false;
            }
        }
        
        /// <summary>
        /// Applique les paramètres d'options (à implémenter)
        /// </summary>
        private void ApplySettings()
        {
            // Ici on pourrait appliquer les paramètres d'options
            // Par exemple, changer le volume ou activer/désactiver le plein écran
            Logger.Instance.Info("Paramètres appliqués", LogCategory.UI);
        }
        
        /// <summary>
        /// Dessine le menu pause
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!_isVisible || _panel == null || _buttonTexture == null)
                return;
                
            if (_font == null)
            {
                _font = UIManager.DefaultFont;
            }

            // Obtenir les dimensions de l'écran
            int screenWidth = _game.GraphicsDevice.Viewport.Width;
            int screenHeight = _game.GraphicsDevice.Viewport.Height;
            
            // Dessiner le fond semi-transparent
            spriteBatch.Draw(
                _buttonTexture,
                new Rectangle(0, 0, screenWidth, screenHeight),
                new Color(0, 0, 0, 160) // Noir semi-transparent
            );
            
            // Position du panneau central
            int panelX = screenWidth / 2 - _menuWidth / 2;
            int panelY = screenHeight / 2 - _menuHeight / 2;
            
            // Dessiner le panneau principal
            spriteBatch.Draw(
                _panel,
                new Rectangle(panelX, panelY, _menuWidth, _menuHeight),
                Color.White
            );
            
            // Dessiner une bordure pour le panneau
            DrawPanelBorder(spriteBatch, panelX, panelY);
            
            // Dessiner le titre approprié
            DrawTitle(spriteBatch, panelX, panelY);
            
            // Dessiner le contenu selon l'écran actuel
            if (!_showingOptions)
            {
                DrawMainMenuContent(spriteBatch);
            }
            else
            {
                DrawOptionsMenuContent(spriteBatch, panelX, panelY);
            }
        }
        
        /// <summary>
        /// Dessine une bordure autour du panneau principal
        /// </summary>
        private void DrawPanelBorder(SpriteBatch spriteBatch, int panelX, int panelY)
        {
            // Dessiner une bordure pour le panneau (4 côtés)
            int borderThickness = 2;
            
            // Côté supérieur
            spriteBatch.Draw(
                _buttonTexture,
                new Rectangle(panelX, panelY, _menuWidth, borderThickness),
                _buttonBorderColor
            );
            
            // Côté gauche
            spriteBatch.Draw(
                _buttonTexture,
                new Rectangle(panelX, panelY, borderThickness, _menuHeight),
                _buttonBorderColor
            );
            
            // Côté droit
            spriteBatch.Draw(
                _buttonTexture,
                new Rectangle(panelX + _menuWidth - borderThickness, panelY, borderThickness, _menuHeight),
                _buttonBorderColor
            );
            
            // Côté inférieur
            spriteBatch.Draw(
                _buttonTexture,
                new Rectangle(panelX, panelY + _menuHeight - borderThickness, _menuWidth, borderThickness),
                _buttonBorderColor
            );
        }
        
        /// <summary>
        /// Dessine le titre du menu
        /// </summary>
        private void DrawTitle(SpriteBatch spriteBatch, int panelX, int panelY)
        {
            if (_font == null)
                return;
                
            string title = _showingOptions ? "OPTIONS" : "PAUSE";
            Vector2 titleSize = _font.MeasureString(title) * 2.0f;
            spriteBatch.DrawString(
                _font,
                title,
                new Vector2(panelX + _menuWidth / 2 - titleSize.X / 2, panelY + 70),
                Color.Gold,
                0f,
                Vector2.Zero,
                2.0f,
                SpriteEffects.None,
                0f
            );
            
            // Dessiner une ligne décorative sous le titre
            spriteBatch.Draw(
                _buttonTexture,
                new Rectangle(panelX + _menuWidth / 2 - 100, panelY + 115, 200, 2),
                _buttonBorderColor
            );
        }
        
        /// <summary>
        /// Dessine le contenu du menu principal
        /// </summary>
        private void DrawMainMenuContent(SpriteBatch spriteBatch)
        {
            // Dessiner les trois boutons principaux
            DrawButton(spriteBatch, _resumeButtonRect, "Resume Game", _resumeButtonColor);
            DrawButton(spriteBatch, _optionsButtonRect, "Options", _optionsButtonColor);
            DrawButton(spriteBatch, _quitButtonRect, "Quit Game", _quitButtonColor);
        }
        
        /// <summary>
        /// Dessine le contenu du menu d'options
        /// </summary>
        private void DrawOptionsMenuContent(SpriteBatch spriteBatch, int panelX, int panelY)
        {
            if (_font == null)
                return;
                
            // Dessiner les labels des options
            spriteBatch.DrawString(
                _font,
                "Music Volume",
                new Vector2(panelX + 50, panelY + 160),
                Color.White
            );
            
            spriteBatch.DrawString(
                _font,
                "Sound Volume",
                new Vector2(panelX + 50, panelY + 220),
                Color.White
            );
            
            spriteBatch.DrawString(
                _font,
                "Fullscreen",
                new Vector2(panelX + 50, panelY + 280),
                Color.White
            );
            
            // Dessiner des sliders simplifiés pour le volume (à titre d'exemple visuel)
            DrawSimpleSlider(spriteBatch, _musicSliderRect, _musicVolume);
            DrawSimpleSlider(spriteBatch, _soundSliderRect, _soundVolume);
            
            // Dessiner un toggle simplifié pour le plein écran
            DrawSimpleToggle(spriteBatch, _fullscreenToggleRect, _fullscreen);
            
            // Dessiner le bouton Back
            DrawButton(spriteBatch, _backButtonRect, "Apply & Back", _backButtonColor);
        }
        
        /// <summary>
        /// Dessine un slider simplifié
        /// </summary>
        private void DrawSimpleSlider(SpriteBatch spriteBatch, Rectangle bounds, float value)
        {
            // Fond du slider
            spriteBatch.Draw(
                _buttonTexture,
                bounds,
                new Color(40, 40, 40, 180)
            );
            
            // Partie remplie du slider
            spriteBatch.Draw(
                _buttonTexture,
                new Rectangle(bounds.X, bounds.Y, (int)(bounds.Width * value), bounds.Height),
                new Color(60, 120, 200, 220)
            );
            
            // Poignée du slider
            spriteBatch.Draw(
                _buttonTexture,
                new Rectangle((int)(bounds.X + bounds.Width * value - 5), bounds.Y - 4, 10, bounds.Height + 8),
                Color.White
            );
            
            // Afficher la valeur en pourcentage
            if (_font != null)
            {
                string valueText = $"{(int)(value * 100)}%";
                spriteBatch.DrawString(
                    _font,
                    valueText,
                    new Vector2(bounds.X + bounds.Width + 20, bounds.Y),
                    Color.White
                );
            }
        }
        
        /// <summary>
        /// Dessine un toggle simplifié
        /// </summary>
        private void DrawSimpleToggle(SpriteBatch spriteBatch, Rectangle bounds, bool isOn)
        {
            // Fond du toggle
            Color bgColor = isOn ? new Color(60, 180, 60, 220) : new Color(80, 80, 80, 180);
            spriteBatch.Draw(
                _buttonTexture,
                bounds,
                bgColor
            );
            
            // Poignée du toggle
            int handleX = isOn ? bounds.X + bounds.Width - 16 : bounds.X + 2;
            spriteBatch.Draw(
                _buttonTexture,
                new Rectangle(handleX, bounds.Y + 2, 14, bounds.Height - 4),
                Color.White
            );
        }
        
        /// <summary>
        /// Dessine un bouton
        /// </summary>
        private void DrawButton(SpriteBatch spriteBatch, Rectangle buttonRect, string text, Color backgroundColor)
        {
            // Dessiner le fond du bouton
            spriteBatch.Draw(_buttonTexture, buttonRect, backgroundColor);
            
            // Dessiner la bordure du bouton (4 côtés)
            int borderThickness = 2;
            
            // Côté supérieur
            spriteBatch.Draw(
                _buttonTexture,
                new Rectangle(buttonRect.X, buttonRect.Y, buttonRect.Width, borderThickness),
                _buttonBorderColor
            );
            
            // Côté gauche
            spriteBatch.Draw(
                _buttonTexture,
                new Rectangle(buttonRect.X, buttonRect.Y, borderThickness, buttonRect.Height),
                _buttonBorderColor
            );
            
            // Côté droit
            spriteBatch.Draw(
                _buttonTexture,
                new Rectangle(buttonRect.X + buttonRect.Width - borderThickness, buttonRect.Y, borderThickness, buttonRect.Height),
                _buttonBorderColor
            );
            
            // Côté inférieur
            spriteBatch.Draw(
                _buttonTexture,
                new Rectangle(buttonRect.X, buttonRect.Y + buttonRect.Height - borderThickness, buttonRect.Width, borderThickness),
                _buttonBorderColor
            );
            
            // Dessiner le texte du bouton
            if (_font != null)
            {
                Vector2 textSize = _font.MeasureString(text) * 1.0f;
                spriteBatch.DrawString(
                    _font,
                    text,
                    new Vector2(
                        buttonRect.X + buttonRect.Width / 2 - textSize.X / 2,
                        buttonRect.Y + buttonRect.Height / 2 - textSize.Y / 2
                    ),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    1.0f,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }
}