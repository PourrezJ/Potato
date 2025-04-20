using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core.Entities;

namespace Potato.Core.UI.Screens
{
    /// <summary>
    /// Canvas pour l'interface en jeu (HUD)
    /// </summary>
    public class GameHudCanvas : UICanvas
    {
        // Éléments UI du HUD
        private Label _scoreLabel;
        private Label _waveLabel;
        private Label _timerLabel;
        private ProgressBar _healthBar;
        private Label _goldLabel;
        private Label _levelLabel;
        private ProgressBar _experienceBar;

        // Références aux données du jeu
        private GameManager _gameManager;
        private Player _player;

        public GameHudCanvas() : base("GameHUD")
        {
            _gameManager = GameManager.Instance;
        }

        public override void Awake()
        {
            base.Awake();
            //CreateUI();
        }

        private void CreateUI()
        {
            // Récupérer les dimensions de l'écran
            var viewport = _gameManager.GraphicsDevice.Viewport;
            int screenWidth = viewport.Width;
            int screenHeight = viewport.Height;

            // Créer les éléments du HUD
            // Score en haut à gauche
            _scoreLabel = UIBuilder.CreateLabel("Score: 0", new Vector2(20, 20), Color.White);
            AddElement(_scoreLabel);

            // Vague en haut à gauche
            _waveLabel = UIBuilder.CreateLabel("Vague: 1", new Vector2(20, 50), Color.White);
            AddElement(_waveLabel);

            // Timer en haut à droite
            _timerLabel = UIBuilder.CreateLabel("Temps: 0:00", new Vector2(screenWidth - 150, 20), Color.Yellow);
            AddElement(_timerLabel);

            // Barre de vie en haut à gauche
            _healthBar = UIBuilder.CreateProgressBar(new Vector2(20, 80), new Vector2(200, 20));
            _healthBar.FillColor = Color.Red;
            AddElement(_healthBar);

            // Or en haut à droite
            _goldLabel = UIBuilder.CreateLabel("Or: 0", new Vector2(screenWidth - 150, 50), Color.Gold);
            AddElement(_goldLabel);

            // Niveau en bas à gauche
            _levelLabel = UIBuilder.CreateLabel("Niveau: 1", new Vector2(20, screenHeight - 50), Color.White);
            AddElement(_levelLabel);

            // Barre d'expérience en bas
            _experienceBar = UIBuilder.CreateProgressBar(new Vector2(20, screenHeight - 30), new Vector2(screenWidth - 40, 10));
            _experienceBar.FillColor = Color.Blue;
            _experienceBar.BackgroundColor = new Color(0, 0, 50, 100);
            AddElement(_experienceBar);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Si le joueur n'est pas encore initialisé, essayer de le récupérer
            if (_player == null)
            {
                _player = Player.Local;
                if (_player == null)
                    return;
            }

            // Mettre à jour les informations du HUD
            UpdateHUDInfo();
        }

        private void UpdateHUDInfo()
        {
            // Mettre à jour le score
            _scoreLabel.Text = $"Score: {_gameManager.Score}";

            // Mettre à jour la vague
            _waveLabel.Text = $"Vague: {_gameManager.Wave}";

            // Mettre à jour le timer
            int remainingSeconds = (int)_gameManager.RemainingWaveTime;
            int minutes = remainingSeconds / 60;
            int seconds = remainingSeconds % 60;
            _timerLabel.Text = $"Temps: {minutes}:{seconds:D2}";

            // Mettre à jour la barre de vie
            if (_player != null && _player.Stats != null)
            {
                _healthBar.Value = _player.Stats.Health;
                _healthBar.MaxValue = _player.Stats.MaxHealth;
            }

            // Mettre à jour l'or
            if (_player != null)
            {
                _goldLabel.Text = $"Or: {_player.Gold}";
                _levelLabel.Text = $"Niveau: {_player.Level}";
                
                // Mettre à jour la barre d'expérience
                _experienceBar.Value = _player.Experience;
                _experienceBar.MaxValue = _player.ExperienceToNextLevel;
            }
        }
    }
}