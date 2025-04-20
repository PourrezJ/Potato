using Microsoft.Xna.Framework;
using Potato.Core.UI;
using Potato.Core.Logging;
using Potato.Core.UI.Screens;
using Potato.Engine;

namespace Potato.Core.Scenes
{
    public class GameplayScene : Scene
    {
        // Référence au GameManager pour la compatibilité avec le code existant
        private GameManager _gameManager;
        private WaveManager _waveManager;
        private MapManager _mapManager;
        private GameHudCanvas _gameHudCanvas;
        
        public GameplayScene() : base("Gameplay")
        {
            _gameManager = GameManager.Instance;
            _waveManager = WaveManager.Instance;
            _mapManager = MapManager.Instance;
            
            // Créer le canvas de HUD pour cette scène
            _gameHudCanvas = new GameHudCanvas();
            
            // S'abonner aux événements de scène
            OnSceneUpdate += UpdateGameplay;
            
            Logger.Instance.Info("GameplayScene créée - prête à activer les managers quand nécessaire", LogCategory.Gameplay);
        }
        
        public override void Load()
        {
            base.Load();
            
            // Vérifier si le joueur existe déjà
            if (_gameManager.Player == null)
            {
                Logger.Instance.Warning("Aucun joueur n'a été créé lors du chargement de la scène de gameplay", LogCategory.Gameplay);
            }
        }
        
        /// <summary>
        /// Activer les managers et les UI liés au gameplay.
        /// Note: Ces managers sont déjà initialisés dans GameManager.
        /// </summary>
        public override void Activate()
        {
            base.Activate();
            
            Logger.Instance.Info("Activation de la scène de gameplay", LogCategory.Gameplay);
            
            // Activer le WaveManager (déjà initialisé dans GameManager)
            if (_waveManager != null)
            {
                //_waveManager.Activate();
                Logger.Instance.Info("WaveManager activé par la scène de gameplay", LogCategory.Gameplay);
            }
            
            // Activer le MapManager (déjà initialisé dans GameManager)
            if (_mapManager != null)
            {
                //_mapManager.Activate();
                Logger.Instance.Info("MapManager activé par la scène de gameplay", LogCategory.Gameplay);
            }
            
            // Activer le GameHudCanvas
            if (_gameHudCanvas != null)
            {
                UIManager.RegisterCanvas(_gameHudCanvas);
                Logger.Instance.Info("GameHudCanvas ajouté à l'UIManager par la scène de gameplay", LogCategory.UI);
            }
            
            // Si le joueur existe, le marquer comme persistant pour qu'il ne soit pas détruit
            // lors du changement de scène (par exemple en allant dans la boutique)
            if (_gameManager.Player != null)
            {
                // Récupérer le GameObject du joueur et le marquer comme persistant
                // Note: Ceci est une transition vers le nouveau système, plus tard le Player sera un GameObject
                GameObject playerObject = GameObjectManager.Find("Player");
                if (playerObject != null)
                {
                    Scene.MarkAsPersistent(playerObject);
                }
            }
        }
        
        /// <summary>
        /// Désactiver les managers et les UI liés au gameplay.
        /// </summary>
        public override void Deactivate()
        {
            Logger.Instance.Info("Désactivation de la scène de gameplay", LogCategory.Gameplay);
            
            // Désactiver le WaveManager
            if (_waveManager != null)
            {
                //_waveManager.Deactivate();
                Logger.Instance.Info("WaveManager désactivé par la scène de gameplay", LogCategory.Gameplay);
            }
            
            // Désactiver le MapManager
            if (_mapManager != null)
            {
                //_mapManager.Deactivate();
                Logger.Instance.Info("MapManager désactivé par la scène de gameplay", LogCategory.Gameplay);
            }
            
            // Retirer le GameHudCanvas
            if (_gameHudCanvas != null)
            {
               // UIManager.RemoveCanvas(_gameHudCanvas.Name);
                Logger.Instance.Info("GameHudCanvas retiré de l'UIManager par la scène de gameplay", LogCategory.UI);
            }
            
            base.Deactivate();
        }
        
        private void UpdateGameplay(GameTime gameTime)
        {
            // Cette méthode est appelée à chaque frame lorsque la scène de gameplay est active
            // Pour le moment, elle est vide car la logique de jeu est gérée par GameManager
            // À terme, la logique sera déplacée ici au fur et à mesure de la migration
        }
        
        public override void Unload()
        {
            // Se désabonner des événements
            OnSceneUpdate -= UpdateGameplay;
            
            base.Unload();
        }
    }
}