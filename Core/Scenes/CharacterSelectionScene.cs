using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core.Logging;
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
        
        public CharacterSelectionScene() : base("CharacterSelection")
        {
            _gameManager = GameManager.Instance;
            
            // S'abonner aux événements
            OnSceneLoaded += InitializeCharacterSelection;
            OnSceneUnloaded += CleanupCharacterSelection;
        }
        
        private void InitializeCharacterSelection()
        {
            Logger.Instance.Debug("Initialisation de la sélection des personnages", LogCategory.UI);
            
            // Créer l'interface utilisateur de sélection des personnages
            // À l'avenir, cela sera fait avec des GameObjects et des composants UI
            
            // Exemple: CharacterSelectionCanvas.Instance.Show();
            // Pour l'instant, on utilise le système existant
        }
        
        private void CleanupCharacterSelection()
        {
            // Nettoyer les ressources de l'écran de sélection
            // Exemple: CharacterSelectionCanvas.Instance.Hide();
        }
        
        public override void Update(GameTime gameTime)
        {
            // La mise à jour de l'écran de sélection est principalement gérée par le UIManager
            // À terme, nous pourrions ajouter ici des animations pour montrer les personnages, etc.
            
            base.Update(gameTime);
        }
    }
}