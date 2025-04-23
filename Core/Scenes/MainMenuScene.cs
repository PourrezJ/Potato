using Microsoft.Xna.Framework;
using Potato.Core.Logging;
using Potato.Core.UI.Screens;

namespace Potato.Core.Scenes
{
    /// <summary>
    /// Scène du menu principal du jeu
    /// </summary>
    public class MainMenuScene : Scene
    {
        // Référence au GameManager pour la compatibilité avec le code existant
        private GameManager _gameManager;
        
        // Référence au GameObject contenant notre menu principal
        private GameObject _mainMenuObject;
        
        public MainMenuScene() : base("MainMenu")
        {
            _gameManager = GameManager.Instance;
            
            // S'abonner aux événements de scène
            OnSceneLoaded += InitializeMainMenu;
            OnSceneUnloaded += CleanupMainMenu;
        }
        
        private void InitializeMainMenu()
        {
            Logger.Debug("Initialisation du menu principal", LogCategory.UI);
            
            // Créer un GameObject pour l'interface du menu principal
            _mainMenuObject = new GameObject("MainMenuUI");
            
            // Ajouter le composant MainMenuCanvas au GameObject
            _mainMenuObject.AddComponent<MainMenuCanvas>();
            
            // Ajouter le GameObject à la scène
            RegisterGameObject(_mainMenuObject);
            
            // Afficher le menu une fois qu'il est initialisé
            var menuCanvas = _mainMenuObject.GetComponent<MainMenuCanvas>();
            if (menuCanvas != null)
            {
                menuCanvas.Show();
            }
        }
        
        private void CleanupMainMenu()
        {
            // Nettoyer et libérer les ressources
            if (_mainMenuObject != null)
            {
                // La destruction du GameObject entraînera l'appel à OnDestroy() sur le MainMenuCanvas
                UnregisterGameObject(_mainMenuObject);
                _mainMenuObject = null;
            }
            
            Logger.Debug("Nettoyage du menu principal", LogCategory.UI);
        }
        
        public override void Update(GameTime gameTime)
        {
            // La mise à jour des GameObjects (y compris MainMenuCanvas) est désormais gérée
            // par le système GameObject/GameBehaviour
            
            base.Update(gameTime);
        }
    }
}