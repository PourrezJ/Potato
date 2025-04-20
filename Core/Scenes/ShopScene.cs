using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core.Logging;
using Potato.Core.UI.Screens;
using Potato.Engine;

namespace Potato.Core.Scenes
{
    /// <summary>
    /// Scène de la boutique entre les vagues
    /// </summary>
    public class ShopScene : Scene
    {
        // Référence au GameManager pour la compatibilité avec le code existant
        private GameManager _gameManager;
        private WaveManager _waveManager;
        private MapManager _mapManager;
        
        public ShopScene() : base("Shop")
        {
            _gameManager = GameManager.Instance;
            _waveManager = WaveManager.Instance;
            _mapManager = MapManager.Instance;
            
            // S'abonner aux événements
            OnSceneLoaded += InitializeShop;
            OnSceneUnloaded += CleanupShop;
        }
        
        private void InitializeShop()
        {
            Logger.Instance.Debug("Initialisation de la boutique", LogCategory.UI);
            
            // S'assurer que le WaveManager est dans l'état correct (entre deux vagues)
            if (_waveManager != null && !_waveManager.IsBetweenWaves)
            {
                Logger.Instance.Warning("Le WaveManager n'est pas dans l'état 'entre deux vagues' alors que la boutique est ouverte", LogCategory.Gameplay);
            }
            
            // Créer l'interface utilisateur de la boutique
            // À l'avenir, cela sera fait avec des GameObjects et des composants UI
            
            // Exemple: ShopCanvas.Instance.Show();
            // Pour l'instant, on utilise le système existant
        }
        
        public override void Activate()
        {
            base.Activate();
            
            // Activer le MapManager mais pas le WaveManager
            if (_mapManager != null)
            {
                //_mapManager.Activate();
                Logger.Instance.Info("MapManager activé par la scène de boutique", LogCategory.Gameplay);
            }
        }
        
        public override void Deactivate()
        {
            // Désactiver le MapManager quand on quitte la boutique
            if (_mapManager != null)
            {
                //_mapManager.Deactivate();
                Logger.Instance.Info("MapManager désactivé par la scène de boutique", LogCategory.Gameplay);
            }
            
            base.Deactivate();
        }
        
        private void CleanupShop()
        {
            // Nettoyer les ressources de la boutique
            // Exemple: ShopCanvas.Instance.Hide();
        }
        
        public override void Update(GameTime gameTime)
        {
            // La mise à jour de la boutique est principalement gérée par le UIManager
            // Mais nous continuons à mettre à jour le timer entre les vagues
            if (_waveManager != null)
            {
                _waveManager.UpdateBetweenWavesTimerOnly(gameTime);
            }
            
            base.Update(gameTime);
        }
    }
}