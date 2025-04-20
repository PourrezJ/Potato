using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Potato.Core.Logging;
using Potato.Core.Attributes;

namespace Potato.Core.UI
{
    [ExecutionOrder(ExecutionOrderAttribute.EarlyPriority)]
    public class UIManager : Singleton<UIManager>
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


        private List<UIElement> _elements = new List<UIElement>();
        private Texture2D _pixel;
        private MouseState _previousMouseState;
        private UIElement _hoveredElement;
        private UIElement _pressedElement;

        public UIManager() { }

        public override void Awake()
        {
            base.Awake();

            try 
            {
                // Créer une texture pixel blanc pour les dessins primitifs
                _pixel = new Texture2D(_game.GraphicsDevice, 1, 1);
                _pixel.SetData(new[] { Color.White });
                
                Logger.Instance.Info("UIManager initialisé avec succès", LogCategory.UI);
            }
            catch (Exception ex) 
            {
                Logger.Instance.Error($"Erreur lors de l'initialisation du UIManager: {ex.Message}", LogCategory.UI);
            }
        }

        public void AddElement(UIElement element)
        {
            _elements.Add(element);
        }

        public void RemoveElement(UIElement element)
        {
            _elements.Remove(element);
        }

        public void ClearElements()
        {
            _elements.Clear();
        }

        public bool ContainsElement(UIElement element)
        {
            return _elements.Contains(element);
        }

        public override void Update(GameTime gameTime)
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
            
            // Mettre à jour tous les éléments UI normalement
            foreach (var element in _elements)
            {
                if (element != null && element.IsVisible)
                {
                    element.Update(gameTime);
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
        private UIElement RaycastUI(Point position)
        {
            // Vérification de sécurité - si la liste d'éléments est null ou vide, retourner null
            if (_elements == null || _elements.Count == 0)
                return null;
                
            // Parcourir les éléments UI en ordre inverse (du dessus vers le dessous)
            for (int i = _elements.Count - 1; i >= 0; i--)
            {
                var element = _elements[i];
                
                // Vérification de nullité pour éviter les NullReferenceException
                if (element == null || !element.IsVisible || !element.IsEnabled)
                    continue;
                
                // Vérification supplémentaire pour s'assurer que Bounds est valide
                try 
                {
                    // Vérifier la collision avec l'élément racine
                    if (element.Bounds.Contains(position))
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
            
            return null; // Aucun élément UI touché
        }
        
        /// <summary>
        /// Recherche récursivement l'élément enfant le plus profond sous la position spécifiée
        /// </summary>
        private UIElement RaycastUIElement(UIElement parent, Point screenPosition)
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
                    if (IsPointInElement(child, screenPosition))
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
        
        /// <summary>
        /// Vérifie si un point en coordonnées écran est dans un élément UI
        /// </summary>
        private bool IsPointInElement(UIElement element, Point screenPosition)
        {
            // Obtenir les coordonnées absolues de l'élément
            Rectangle absoluteBounds = GetAbsoluteBounds(element);
            
            // Debug
            Logger.Instance.Debug($"Testing click at {screenPosition.X},{screenPosition.Y} against {element.GetType().Name} bounds {absoluteBounds}", LogCategory.UI);
            
            if (element is Button button)
            {
                Logger.Instance.Debug($"Button '{button.Text}' bounds: {absoluteBounds}", LogCategory.UI);
            }
            
            // Vérifier si le point est dans les limites absolues
            return absoluteBounds.Contains(screenPosition);
        }
        
        /// <summary>
        /// Calcule les limites absolues d'un élément UI, en tenant compte de tous ses parents
        /// </summary>
        private Rectangle GetAbsoluteBounds(UIElement element)
        {
            Vector2 absolutePosition = element.Position;
            UIElement parent = element.Parent;
            
            // Si l'élément a un parent, calculer sa position absolue
            if (parent != null)
            {
                // Pour les éléments dans un panel, il faut appliquer les décalages
                if (parent is Panel panel)
                {
                    // Ajouter le décalage du titre si présent
                    int titleOffset = panel.TitleHeight > 0 ? panel.TitleHeight : 0;
                    int padding = panel.Padding;
                    
                    // La position est déjà absolue depuis nos corrections
                    // On n'a pas besoin d'ajouter la position du parent car c'est déjà fait
                    // dans la méthode AddChild du Panel
                    
                    // Debug
                    Logger.Instance.Debug($"Element in panel: position={element.Position}, panel={panel.Position}, titleOffset={titleOffset}, padding={padding}", LogCategory.UI);
                }
            }
            
            // Appliquer l'échelle à la taille
            Vector2 scaledSize = new Vector2(
                element.Size.X * element.Scale,
                element.Size.Y * element.Scale
            );
            
            return new Rectangle(
                (int)absolutePosition.X,
                (int)absolutePosition.Y,
                (int)scaledSize.X,
                (int)scaledSize.Y
            );
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            foreach (var element in _elements)
            {
                if (element != null && element.IsVisible)
                    element.Draw(spriteBatch);
            }
        }

        // Propriété Pixel sécurisée
        public Texture2D Pixel 
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

        public void Dispose()
        {
            _pixel?.Dispose();
        }
    }
}