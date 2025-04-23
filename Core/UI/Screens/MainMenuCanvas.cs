using Microsoft.Xna.Framework;
using Potato.Core.Logging;

namespace Potato.Core.UI.Screens
{
    /// <summary>
    /// Écran de menu principal montrant comment utiliser le système UI basé sur les canvas
    /// </summary>
    public class MainMenuCanvas : UICanvas
    {
        // Pour la compatibilité avec le code existant, on garde Instance
        // mais il sera utilisé différemment
        public static MainMenuCanvas Current { get; private set; }

        private Button _playButton;
        private Button _optionsButton;
        private Button _quitButton;
        private Label _titleLabel;
        private Label _versionLabel;

        public MainMenuCanvas() : base("MainMenu")
        {
            Current = this;
        }

        public override void OnDestroy()
        {
            // Nettoyer la référence statique
            if (Current == this)
            {
                Current = null;
            }
            
            base.OnDestroy();
        }
        
        public override void Start()
        {
            base.Start();
            CreateUI();
        }

        private void CreateUI()
        {
            // Récupérer les dimensions de l'écran
            var viewport = GameManager.Instance.GraphicsDevice.Viewport;
            int screenWidth = viewport.Width;
            int screenHeight = viewport.Height;

            // Créer le titre du jeu
            _titleLabel = UIBuilder.CreateLabel("POTATO", new Vector2(screenWidth / 2, 100), Color.Gold, 2.0f, true);
            // Le centrage est défini dans la méthode CreateLabel via le paramètre centered=true
            this.AddElement(_titleLabel);

            // Version du jeu
            _versionLabel = UIBuilder.CreateLabel("Version 1.0", new Vector2(screenWidth / 2, 150), Color.White, 0.8f, true);
            // Le centrage est défini dans la méthode CreateLabel via le paramètre centered=true
            this.AddElement(_versionLabel);

            // Bouton Play
            _playButton = UIBuilder.CreateButton("JOUER", new Vector2(screenWidth / 2 - 100, screenHeight / 2 - 50), new Vector2(200, 50));
            _playButton.OnClickAction = OnPlayButtonClicked;
            // Centrer le bouton horizontalement
            _playButton.Position = new Vector2((screenWidth - _playButton.Size.X) / 2, _playButton.Position.Y);
            this.AddElement(_playButton);

            // Bouton Options
            _optionsButton = UIBuilder.CreateButton("OPTIONS", new Vector2(screenWidth / 2 - 100, screenHeight / 2 + 20), new Vector2(200, 50));
            _optionsButton.OnClickAction = OnOptionsButtonClicked;
            // Centrer le bouton horizontalement
            _optionsButton.Position = new Vector2((screenWidth - _optionsButton.Size.X) / 2, _optionsButton.Position.Y);
            this.AddElement(_optionsButton);

            // Bouton Quit
            _quitButton = UIBuilder.CreateButton("QUITTER", new Vector2(screenWidth / 2 - 100, screenHeight / 2 + 90), new Vector2(200, 50));
            _quitButton.OnClickAction = OnQuitButtonClicked;
            // Centrer le bouton horizontalement
            _quitButton.Position = new Vector2((screenWidth - _quitButton.Size.X) / 2, _quitButton.Position.Y);
            this.AddElement(_quitButton);
        }

        private void OnPlayButtonClicked()
        {
            // Désactiver le bouton pour éviter les clics multiples
            _playButton.IsEnabled = false;
            
            // Cacher notre canvas d'abord
            this.IsVisible = false;
            
            // Journaliser l'intention avant de changer d'état
            Logger.Info("Passage à l'écran de sélection des personnages depuis le menu principal", LogCategory.UI);
            
            // Changer l'état du jeu directement, sans délai
            GameManager.Instance.SetGameState(GameManager.GameState.CharacterSelection);
        }

        private void OnOptionsButtonClicked()
        {
            // Ouvrir l'écran des options - gestion manuelle des Canvas
            var canvasManager = CanvasManager.Instance;
            canvasManager.ShowCanvas("Options");
            this.IsVisible = false;
        }

        private void OnQuitButtonClicked()
        {
            // Quitter le jeu
            GameManager.Instance.Exit();
        }
        
        // Méthodes publiques pour exposer des fonctionnalités au niveau de la scène
        public void Show()
        {
            this.IsVisible = true;
        }
        
        public void Hide()
        {
            this.IsVisible = false;
        }
    }
}