using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Potato.Core.UI.Screens
{
    /// <summary>
    /// Écran de menu principal montrant comment utiliser le système UI basé sur les canvas
    /// </summary>
    public class MainMenuCanvas : UICanvas
    {
        public static MainMenuCanvas Instance { get; private set; } 

        private Button _playButton;
        private Button _optionsButton;
        private Button _quitButton;
        private Label _titleLabel;
        private Label _versionLabel;

        public MainMenuCanvas() : base("MainMenu")
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("Une instance de MainMenuCanvas existe déjà.");
            }
            Instance = this;
        }

        public void CreateUI()
        {
            // Récupérer les dimensions de l'écran
            var viewport = GameManager.Instance.GraphicsDevice.Viewport;
            int screenWidth = viewport.Width;
            int screenHeight = viewport.Height;

            // Créer le titre du jeu
            _titleLabel = UIBuilder.CreateLabel("POTATO", new Vector2(screenWidth / 2, 100), Color.Gold, 2.0f, true);
            // Le centrage est défini dans la méthode CreateLabel via le paramètre centered=true
            AddElement(_titleLabel);

            // Version du jeu
            _versionLabel = UIBuilder.CreateLabel("Version 1.0", new Vector2(screenWidth / 2, 150), Color.White, 0.8f, true);
            // Le centrage est défini dans la méthode CreateLabel via le paramètre centered=true
            AddElement(_versionLabel);

            // Bouton Play
            _playButton = UIBuilder.CreateButton("JOUER", new Vector2(screenWidth / 2 - 100, screenHeight / 2 - 50), new Vector2(200, 50));
            _playButton.OnClickAction = OnPlayButtonClicked;
            // Centrer le bouton horizontalement
            _playButton.Position = new Vector2((screenWidth - _playButton.Size.X) / 2, _playButton.Position.Y);
            AddElement(_playButton);

            // Bouton Options
            _optionsButton = UIBuilder.CreateButton("OPTIONS", new Vector2(screenWidth / 2 - 100, screenHeight / 2 + 20), new Vector2(200, 50));
            _optionsButton.OnClickAction = OnOptionsButtonClicked;
            // Centrer le bouton horizontalement
            _optionsButton.Position = new Vector2((screenWidth - _optionsButton.Size.X) / 2, _optionsButton.Position.Y);
            AddElement(_optionsButton);

            // Bouton Quit
            _quitButton = UIBuilder.CreateButton("QUITTER", new Vector2(screenWidth / 2 - 100, screenHeight / 2 + 90), new Vector2(200, 50));
            _quitButton.OnClickAction = OnQuitButtonClicked;
            // Centrer le bouton horizontalement
            _quitButton.Position = new Vector2((screenWidth - _quitButton.Size.X) / 2, _quitButton.Position.Y);
            AddElement(_quitButton);
        }

        private void OnPlayButtonClicked()
        {
            // Changer l'état du jeu pour démarrer la partie
            GameManager.Instance.SetGameState(GameManager.GameState.CharacterSelection);
            
            // Maintenant que nous n'utilisons plus l'adaptation automatique des Canvas aux GameStates,
            // nous devons gérer manuellement la visibilité des Canvas lors des changements d'état
            CanvasManager.Instance.HideCanvas("MainMenu");
        }

        private void OnOptionsButtonClicked()
        {
            // Ouvrir l'écran des options - gestion manuelle des Canvas
            var canvasManager = CanvasManager.Instance;
            canvasManager.ShowCanvas("Options");
            canvasManager.HideCanvas("MainMenu");
        }

        private void OnQuitButtonClicked()
        {
            // Quitter le jeu
            GameManager.Instance.Exit();
        }
    }
}