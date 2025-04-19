using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Potato.Core.UI;
using Potato.Core.Logging;
using System;
using Potato.Core;

namespace Potato.Engine
{
    public class MainMenuScreen : GameBehaviour
    {
        private SpriteFont _font;
        private Button _startButton;
        private Button _optionsButton;
        private Button _quitButton;
        
        // Pour le suivi de la souris
        private MouseState _previousMouseState;
        
        // Callback delegates
        public Action OnStartGame;
        public Action OnOptions;
        public Action OnQuit;

        private bool _isVisible = false;
        
        public override void Awake()
        {
            base.Awake();
            Logger.Instance.Info("[MainMenuScreen] Écran de menu principal créé", LogCategory.UI);
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Utiliser la police déjà chargée par le UIManager au lieu de la charger à nouveau
            _font = UIManager.Instance.GetDefaultFont();
            
            if (_font == null)
            {
                Logger.Instance.Warning("[MainMenuScreen] Impossible d'obtenir DefaultFont depuis UIManager", LogCategory.UI);
            }
            else
            {
                Logger.Instance.Debug("[MainMenuScreen] Police DefaultFont obtenue depuis UIManager", LogCategory.UI);
            }

            // Get screen dimensions
            int screenWidth = _game.GraphicsDevice.Viewport.Width;
            int screenHeight = _game.GraphicsDevice.Viewport.Height;
            
            // Start game button - centré à l'écran
            _startButton = new Button(
                new Vector2(screenWidth / 2 - 100, screenHeight / 2 - 80),
                new Vector2(200, 50),
                "START GAME", 
                _font);
                
            _startButton.BackgroundColor = new Color(60, 120, 60, 220);
            _startButton.HoverColor = new Color(80, 160, 80, 220);
            _startButton.CornerRadius = 10f;
            _startButton.BorderColor = new Color(100, 200, 100, 200);
            _startButton.HasBorder = true;
            _startButton.IsEnabled = true;
            _startButton.OnClickAction = () => { 
                Logger.Instance.Info("Start button clicked", LogCategory.UI);
                OnStartGame?.Invoke(); 
            };
            
            // Options button - en dessous du bouton start
            _optionsButton = new Button(
                new Vector2(screenWidth / 2 - 100, screenHeight / 2),
                new Vector2(200, 50),
                "OPTIONS",
                _font);
                
            _optionsButton.BackgroundColor = new Color(60, 60, 120, 220);
            _optionsButton.HoverColor = new Color(80, 80, 160, 220);
            _optionsButton.CornerRadius = 10f;
            _optionsButton.BorderColor = new Color(100, 100, 200, 200);
            _optionsButton.HasBorder = true;
            _optionsButton.IsEnabled = true;
            _optionsButton.OnClickAction = () => {
                Logger.Instance.Info("Options button clicked", LogCategory.UI);
                OnOptions?.Invoke();
            };
            
            // Quit button - en dessous du bouton options
            _quitButton = new Button(
                new Vector2(screenWidth / 2 - 100, screenHeight / 2 + 80),
                new Vector2(200, 50),
                "QUIT GAME",
                _font);
                
            _quitButton.BackgroundColor = new Color(120, 60, 60, 220);
            _quitButton.HoverColor = new Color(160, 80, 80, 220);
            _quitButton.CornerRadius = 10f;
            _quitButton.BorderColor = new Color(200, 100, 100, 200);
            _quitButton.HasBorder = true;
            _quitButton.IsEnabled = true;
            _quitButton.OnClickAction = () => {
                Logger.Instance.Info("Quit button clicked", LogCategory.UI);
                OnQuit?.Invoke();
            };
            
            // Ajouter les boutons au UIManager
            UIManager.Instance.AddElement(_startButton);
            UIManager.Instance.AddElement(_optionsButton);
            UIManager.Instance.AddElement(_quitButton);
        }
        
        public override void Update(GameTime gameTime)
        {
            if (!_isVisible)
                return;
                
            // Le UIManager gère désormais les interactions
            UIManager.Instance.Update(gameTime);
            
            // Gérer également directement les clics de souris pour la compatibilité descendante
            MouseState currentMouseState = Mouse.GetState();
            
            // Vérifier le clic sur le bouton Start
            if (_startButton.Bounds.Contains(currentMouseState.Position) && 
                currentMouseState.LeftButton == ButtonState.Released && 
                _previousMouseState.LeftButton == ButtonState.Pressed)
            {
                Logger.Instance.Debug("Direct click on START button", LogCategory.UI);
                OnStartGame?.Invoke();
            }
            
            // Vérifier le clic sur le bouton Options
            if (_optionsButton.Bounds.Contains(currentMouseState.Position) && 
                currentMouseState.LeftButton == ButtonState.Released && 
                _previousMouseState.LeftButton == ButtonState.Pressed)
            {
                Logger.Instance.Debug("Direct click on OPTIONS button", LogCategory.UI);
                OnOptions?.Invoke();
            }
            
            // Vérifier le clic sur le bouton Quit
            if (_quitButton.Bounds.Contains(currentMouseState.Position) && 
                currentMouseState.LeftButton == ButtonState.Released && 
                _previousMouseState.LeftButton == ButtonState.Pressed)
            {
                Logger.Instance.Debug("Direct click on QUIT button", LogCategory.UI);
                OnQuit?.Invoke();
            }
            
            // Mettre à jour l'état précédent pour le prochain frame
            _previousMouseState = currentMouseState;
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!_isVisible)
                return;
                
            // Draw background
            DrawBackground(spriteBatch);
            
            // Le UIManager dessine maintenant les boutons
            // UIManager.Instance.Draw(spriteBatch) est appelé dans Game1.Draw
        }
        
        private void DrawBackground(SpriteBatch spriteBatch)
        {
            // Create a simple dark background
            Texture2D pixel = new Texture2D(_game.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            
            // Draw a dark blue background
            spriteBatch.Draw(
                pixel,
                new Rectangle(0, 0, _game.GraphicsDevice.Viewport.Width, _game.GraphicsDevice.Viewport.Height),
                new Color(20, 20, 40));
        }
        
        public void Show()
        {            
            _isVisible = true;
            // Nettoyer tous les éléments UI existants pour éviter les résidus
            UIManager.Instance.ClearElements();
            
            // Ré-ajouter les boutons du menu
            UIManager.Instance.AddElement(_startButton);
            UIManager.Instance.AddElement(_optionsButton);
            UIManager.Instance.AddElement(_quitButton);
        }
        
        public void Hide()
        {
            _isVisible = false;
            // Retirer les boutons du UIManager
            UIManager.Instance.RemoveElement(_startButton);
            UIManager.Instance.RemoveElement(_optionsButton);
            UIManager.Instance.RemoveElement(_quitButton);
        }
    }
}