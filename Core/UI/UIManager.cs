using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Potato.Core.Logging;

namespace Potato.Core.UI
{
    public static class UIManager
    {
        public static SpriteFont DefaultFont {
            get
            {
                if (_defaultFont == null)
                {
                    _defaultFont = GameManager.Instance.Content.Load<SpriteFont>("DefaultFont");
                }
                return _defaultFont;
            }
            set => _defaultFont = value;
        }
        private static SpriteFont _defaultFont;

        // Liste des canvas au lieu d'une liste simple d'éléments
        private static List<UICanvas> _canvases = new List<UICanvas>();
        private static Dictionary<string, UICanvas> _canvasByName = new Dictionary<string, UICanvas>();
        
        private static Texture2D _pixel;
        private static MouseState _previousMouseState;
        private static UIElement _hoveredElement;
        private static UIElement _pressedElement;


        public static void Initialize()
        {
            try 
            {
                // Utiliser directement GameManager.Instance au lieu de _game
                _pixel = new Texture2D(GameManager.Instance.GraphicsDevice, 1, 1);
                _pixel.SetData(new[] { Color.White });
                
                Logger.Instance.Info("UIManager initialisé avec succès", LogCategory.UI);
            }
            catch (Exception ex) 
            {
                Logger.Instance.Error($"Erreur lors de l'initialisation du UIManager: {ex.Message}", LogCategory.UI);
            }
        }

        #region Canvas Management
        
        public static void RegisterCanvas(UICanvas canvas)
        {
            if (!_canvases.Contains(canvas))
            {
                _canvases.Add(canvas);
                if (!string.IsNullOrEmpty(canvas.Name))
                {
                    _canvasByName[canvas.Name] = canvas;
                }
                Logger.Instance.Debug($"Canvas '{canvas.Name}' enregistré", LogCategory.UI);
            }
        }
        
        public static void UnregisterCanvas(UICanvas canvas)
        {
            if (_canvases.Contains(canvas))
            {
                _canvases.Remove(canvas);
                if (!string.IsNullOrEmpty(canvas.Name) && _canvasByName.ContainsKey(canvas.Name))
                {
                    _canvasByName.Remove(canvas.Name);
                }
                Logger.Instance.Debug($"Canvas '{canvas.Name}' désenregistré", LogCategory.UI);
            }
        }
        
        public static UICanvas GetCanvas(string name)
        {
            if (_canvasByName.TryGetValue(name, out var canvas))
                return canvas;
                
            return null;
        }
        
        public static UICanvas CreateCanvas(string name)
        {
            if (_canvasByName.ContainsKey(name))
            {
                Logger.Instance.Warning($"Un canvas avec le nom '{name}' existe déjà", LogCategory.UI);
                return _canvasByName[name];
            }
            
            var canvas = new UICanvas(name);
            RegisterCanvas(canvas);
            //BehaviourManager.RegisterBehaviour(canvas);
            return canvas;
        }
        
        public static void ShowCanvas(string name)
        {
            var canvas = GetCanvas(name);
            if (canvas != null)
            {
                canvas.IsVisible = true;
                Logger.Instance.Debug($"Canvas '{name}' affiché", LogCategory.UI);
            }
        }
        
        public static void HideCanvas(string name)
        {
            var canvas = GetCanvas(name);
            if (canvas != null)
            {
                canvas.IsVisible = false;
                Logger.Instance.Debug($"Canvas '{name}' masqué", LogCategory.UI);
            }
        }
        
        public static void HideAllCanvas()
        {
            foreach (var canvas in _canvases)
            {
                canvas.IsVisible = false;
            }
            Logger.Instance.Debug("Tous les canvas ont été masqués", LogCategory.UI);
        }
        
        #endregion

        public static void Update(GameTime gameTime)
        {
            // Obtenir l'état actuel de la souris
            var currentMouseState = Mouse.GetState();
            Point mousePosition = new Point(currentMouseState.X, currentMouseState.Y);
            
            // Utiliser le raycast pour trouver l'élément UI sous la souris
            UIElement hitElement = RaycastUI(mousePosition);
            
            // Gérer les événements de survol
            if (hitElement != _hoveredElement)
            {
                // L'élément survolé a changé
                if (_hoveredElement != null)
                {
                    // Sortie de survol de l'ancien élément
                    _hoveredElement.SetHovered(false);
                }
                
                _hoveredElement = hitElement;
                
                if (_hoveredElement != null)
                {
                    // Entrée en survol du nouvel élément
                    _hoveredElement.SetHovered(true);
                    
                    // Déboguer l'élément survolé
                    if (_hoveredElement is Button button)
                    {
                        Logger.Instance.Debug($"Button '{button.Text}' is hovered at {mousePosition.X},{mousePosition.Y}", LogCategory.UI);
                    }
                }
            }
            
            // Gérer les événements de clic
            bool wasPressed = _pressedElement != null;
            bool isPressed = currentMouseState.LeftButton == ButtonState.Pressed;
            bool wasReleased = _previousMouseState.LeftButton == ButtonState.Pressed && currentMouseState.LeftButton == ButtonState.Released;
            
            if (isPressed && !wasPressed)
            {
                // Début du clic
                _pressedElement = hitElement;
                
                if (_pressedElement != null)
                {
                    _pressedElement.SetPressed(true);
                }
            }
            else if (wasReleased)
            {
                // Fin du clic
                if (_pressedElement != null && _pressedElement == _hoveredElement)
                {
                    // Un clic complet a été réalisé sur le même élément
                    Logger.Instance.Debug($"UI Element clicked at {mousePosition.X},{mousePosition.Y}", LogCategory.UI);
                    
                    // Déclencher l'événement de clic
                    _pressedElement.TriggerClick();
                }
                
                // Réinitialiser l'état de pression
                if (_pressedElement != null)
                {
                    _pressedElement.SetPressed(false);
                    _pressedElement = null;
                }
            }
            
            // Mettre à jour les canvases
            foreach (var canvas in _canvases)
            {
                if (canvas.IsVisible)
                {
                    canvas.Update(gameTime);
                }
            }
            
            // Sauvegarder l'état actuel pour le prochain frame
            _previousMouseState = currentMouseState;
        }

        /// <summary>
        /// Effectue un raycast pour trouver l'élément UI sous la position spécifiée
        /// </summary>
        /// <param name="position">Position à tester (coordonnées écran)</param>
        /// <returns>L'élément UI sous la position, ou null si aucun</returns>
        private static UIElement RaycastUI(Point position)
        {
            // Vérification de sécurité - si la liste d'éléments est null ou vide, retourner null
            if (_canvases == null || _canvases.Count == 0)
                return null;
                
            // Parcourir les canvas en ordre inverse (du dessus vers le dessous)
            for (int i = _canvases.Count - 1; i >= 0; i--)
            {
                var canvas = _canvases[i];
                
                // Ignorer les canvas cachés
                if (!canvas.IsVisible)
                    continue;
                    
                // Parcourir les éléments racines du canvas
                var rootElements = canvas.GetRootElements();
                
                // Parcourir les éléments UI en ordre inverse (du dessus vers le dessous)
                for (int j = rootElements.Count - 1; j >= 0; j--)
                {
                    var element = rootElements[j];
                    
                    // Vérification de nullité pour éviter les NullReferenceException
                    if (element == null || !element.IsVisible || !element.IsEnabled)
                        continue;
                    
                    // Vérification supplémentaire pour s'assurer que Bounds est valide
                    try 
                    {
                        // Vérifier la collision avec l'élément racine
                        if (element.Contains(position))
                        {
                            // L'élément racine est touché, maintenant chercher récursivement un enfant qui pourrait être touché
                            UIElement hitChild = RaycastUIElement(element, position);
                            return hitChild ?? element; // Retourner l'enfant s'il y en a un, sinon l'élément racine
                        }
                    }
                    catch (System.Exception ex)
                    {
                        // Journaliser l'erreur et continuer
                        Logger.Instance.Error($"Erreur lors du raycast sur l'élément {element.GetType().Name}: {ex.Message}", LogCategory.UI);
                        continue;
                    }
                }
            }
            
            return null; // Aucun élément UI touché
        }
        
        /// <summary>
        /// Recherche récursivement l'élément enfant le plus profond sous la position spécifiée
        /// </summary>
        private static UIElement RaycastUIElement(UIElement parent, Point screenPosition)
        {
            // Vérification de nullité pour éviter les NullReferenceException
            if (parent == null)
                return null;
                
            try
            {
                // Obtenir les enfants de l'élément
                var children = parent.GetChildren();
                
                // Vérification de sécurité
                if (children == null || children.Count == 0)
                    return null;
                
                // Parcourir les enfants en ordre inverse (du dessus vers le dessous)
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    var child = children[i];
                    
                    // Vérification de nullité
                    if (child == null || !child.IsVisible || !child.IsEnabled)
                        continue;
                    
                    // Vérifier si l'enfant contient la position
                    if (child.Contains(screenPosition))
                    {
                        // Rechercher récursivement dans les enfants de cet enfant
                        UIElement hitGrandchild = RaycastUIElement(child, screenPosition);
                        return hitGrandchild ?? child; // Retourner le petit-enfant s'il y en a un, sinon l'enfant
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Journaliser l'erreur et continuer
                Logger.Instance.Error($"Erreur lors du raycast sur les enfants de {parent.GetType().Name}: {ex.Message}", LogCategory.UI);
            }
            
            return null; // Aucun enfant touché
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            // Remarque: Nous ne démarrons pas un spriteBatch ici car chaque canvas 
            // gère désormais ses propres appels à Begin/End
            foreach (var canvas in _canvases)
            {
                if (canvas.IsVisible)
                {
                    canvas.Draw(spriteBatch);
                }
            }
        }

        // Propriété Pixel sécurisée
        public static Texture2D Pixel 
        { 
            get 
            {
                if (_pixel == null)
                {
                    Logger.Instance.Warning("Tentative d'accès à Pixel alors qu'il n'est pas initialisé", LogCategory.UI);
                    // Si possible, tenter de créer un pixel à la volée si GraphicsDevice est disponible
                    try
                    {
                        if (GameManager.Instance != null && GameManager.Instance.GraphicsDevice != null)
                        {
                            _pixel = new Texture2D(GameManager.Instance.GraphicsDevice, 1, 1);
                            _pixel.SetData(new[] { Color.White });
                            Logger.Instance.Info("Pixel créé à la volée", LogCategory.UI);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error($"Impossible de créer un pixel à la volée: {ex.Message}", LogCategory.UI);
                    }
                }
                return _pixel;
            }
        }

        public static void Dispose()
        {
            _pixel?.Dispose();
        }
    }
}