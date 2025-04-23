using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core.Logging;
using Potato.Core.UI;
using Potato.Core.UI.Screens;

namespace Potato.Core.Scenes
{
    /// <summary>
    /// Scène de sélection des personnages et des armes
    /// </summary>
    public class CharacterSelectionScene : Scene
    {
        // Référence au GameManager pour la compatibilité avec le code existant
        private GameManager _gameManager;
        
        // Référence au GameObject contenant notre canvas de sélection de personnage
        private GameObject _playerSelectionObject;
        
        public CharacterSelectionScene() : base("CharacterSelection")
        {
            _gameManager = GameManager.Instance;
            
            // S'abonner aux événements
            OnSceneLoaded += InitializeCharacterSelection;
            OnSceneUnloaded += CleanupCharacterSelection;
        }
        
        private void InitializeCharacterSelection()
        {
            Logger.Debug("Initialisation de la sélection des personnages", LogCategory.UI);
            
            try
            {
                // Vérifier si UIManager est correctement initialisé
                if (UIManager.Pixel == null)
                {
                    Logger.Warning("UIManager.Pixel n'est pas initialisé, tentative d'initialisation", LogCategory.UI);
                    UIManager.Initialize();
                }
                
                // Créer un GameObject pour l'interface de sélection des joueurs
                _playerSelectionObject = new GameObject("PlayerSelectionUI");
                
                // Ajouter le composant PlayerSelectionCanvas au GameObject
                var canvas = _playerSelectionObject.AddComponent<PlayerSelectionCanvas>();
                
                // Ajouter le GameObject à la scène
                RegisterGameObject(_playerSelectionObject);
                
                // Afficher le canvas une fois qu'il est initialisé
                var playerSelectionCanvas = _playerSelectionObject.GetComponent<PlayerSelectionCanvas>();
                if (playerSelectionCanvas != null)
                {
                    // S'assurer que le canvas est enregistré dans l'UIManager
                    UIManager.RegisterCanvas(playerSelectionCanvas);
                    playerSelectionCanvas.IsVisible = true;
                    playerSelectionCanvas.Show();
                    Logger.Info("Canvas de sélection des joueurs affiché", LogCategory.UI);
                }
                else
                {
                    Logger.Error("Impossible de trouver le composant PlayerSelectionCanvas", LogCategory.UI);
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error($"Erreur lors de l'initialisation de la sélection des personnages: {ex.Message}", LogCategory.UI);
            }
        }
        
        private void CleanupCharacterSelection()
        {
            // Nettoyer et libérer les ressources
            if (_playerSelectionObject != null)
            {
                // La destruction du GameObject entraînera l'appel à OnDestroy() sur le PlayerSelectionCanvas
                UnregisterGameObject(_playerSelectionObject);
                _playerSelectionObject = null;
            }
            
            Logger.Debug("Nettoyage de la sélection des personnages", LogCategory.UI);
        }
        
        public override void Update(GameTime gameTime)
        {
            // La mise à jour des GameObjects (y compris PlayerSelectionCanvas) est désormais gérée
            // par le système GameObject/GameBehaviour
            
            base.Update(gameTime);
        }
    }
}