using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        
        public MainMenuScene() : base("MainMenu")
        {
            _gameManager = GameManager.Instance;
            
            // S'abonner aux événements de scène
            OnSceneLoaded += InitializeMainMenu;
            OnSceneUnloaded += CleanupMainMenu;
        }
        
        private void InitializeMainMenu()
        {
            Logger.Instance.Debug("Initialisation du menu principal", LogCategory.UI);
            
            // Créer l'interface utilisateur du menu principal
            // À l'avenir, cela sera fait en utilisant les GameObjects et des composants UI
            MainMenuCanvas.Instance.CreateUI(); // Utiliser CreateUI() au lieu de Show()
        }
        
        private void CleanupMainMenu()
        {
            // Pour le moment, nous n'avons pas de méthode Hide() ou équivalente dans MainMenuCanvas
            // À l'avenir, nous pourrons implémenter cette fonctionnalité
            
            // Nettoyage supplémentaire si nécessaire
            Logger.Instance.Debug("Nettoyage du menu principal", LogCategory.UI);
        }
        
        public override void Update(GameTime gameTime)
        {
            // La mise à jour du menu principal est principalement gérée par le UIManager
            // À terme, nous pourrions ajouter ici des animations d'arrière-plan, etc.
            
            base.Update(gameTime);
        }
    }
}