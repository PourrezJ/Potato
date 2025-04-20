using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Potato.Core.Logging;

namespace Potato.Core.UI
{
    public class UIElement
    {
        // Propriétés de base
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Color Color { get; set; } = Color.White;
        public float Scale { get; set; } = 1.0f;
        public float Rotation { get; set; } = 0.0f;
        public float Opacity { get; set; } = 1.0f;
        public bool IsVisible { get; set; } = true;
        
        // Position relative au parent
        public Vector2 RelativePosition { get; set; } = Vector2.Zero;
        
        // Marges et rembourrage
        public Thickness Margin { get; set; } = new Thickness(0);
        public Thickness Padding { get; set; } = new Thickness(0);
        
        // Animation et transitions
        protected Dictionary<string, Transition> _activeTransitions = new Dictionary<string, Transition>();
        
        // Ombre
        public bool HasShadow { get; set; } = false;
        public Vector2 ShadowOffset { get; set; } = new Vector2(4, 4);
        public Color ShadowColor { get; set; } = new Color(0, 0, 0, 128);
        
        // Bordure arrondie
        public float CornerRadius { get; set; } = 0f;
        
        // Bordure
        public bool HasBorder { get; set; } = false;
        public Color BorderColor { get; set; } = Color.Black;
        public int BorderThickness { get; set; } = 1;
        
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        
        // Rendu optimisé avec RenderTarget2D si nécessaire
        protected RenderTarget2D _renderTarget;
        protected bool _needsRedraw = true;
        protected bool _useRenderTarget = false;
        
        // Parent et enfants
        public UIElement Parent { get; protected set; }
        protected List<UIElement> _children = new List<UIElement>();
        
        // Canvas auquel cet élément appartient
        public UICanvas Canvas { get; set; }
        
        // États d'interaction
        public bool IsHovered { get; protected set; }
        public bool IsPressed { get; protected set; }
        public bool IsEnabled { get; set; } = true;
        
        // Gestionnaire d'événements
        public event Action<UIElement> OnClick;
        public event Action<UIElement> OnHover;
        public event Action<UIElement> OnHoverExit;

        public UIElement(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }
        
        public virtual void Initialize(GraphicsDevice graphicsDevice)
        {
            if (_useRenderTarget && _renderTarget == null)
            {
                _renderTarget = new RenderTarget2D(
                    graphicsDevice,
                    (int)Size.X,
                    (int)Size.Y,
                    false,
                    graphicsDevice.PresentationParameters.BackBufferFormat,
                    DepthFormat.None
                );
                _needsRedraw = true;
            }
            
            // Initialiser les enfants
            foreach (var child in _children)
            {
                child.Initialize(graphicsDevice);
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!IsVisible || !IsEnabled)
                return;
                
            // Mettre à jour les transitions actives
            UpdateTransitions(gameTime);
            
            // Mettre à jour les enfants
            foreach (var child in _children)
            {
                child.Update(gameTime);
            }
        }
        
        public virtual void UpdateInput(MouseState currentMouseState, MouseState previousMouseState)
        {
            if (!IsVisible || !IsEnabled)
                return;
                
            bool wasHovered = IsHovered;
            
            // Vérification spéciale pour les cas où l'élément est dans un panneau
            IsHovered = Contains(new Point(currentMouseState.X, currentMouseState.Y));
            
            // Pour le débogage - Afficher lorsqu'un bouton est survolé
            if (IsHovered && this is Button button)
            {
                Logger.Instance.Debug($"Button '{button.Text}' is hovered at point {currentMouseState.X},{currentMouseState.Y}", LogCategory.UI);
            }
            
            // Gérer l'événement de survol
            if (IsHovered && !wasHovered)
            {
                OnHover?.Invoke(this);
            }
            else if (!IsHovered && wasHovered)
            {
                OnHoverExit?.Invoke(this);
            }
            
            // Gérer l'événement de clic
            if (IsHovered && 
                currentMouseState.LeftButton == ButtonState.Released && 
                previousMouseState.LeftButton == ButtonState.Pressed)
            {
                Logger.Instance.Debug($"UI Element clicked at {currentMouseState.X},{currentMouseState.Y}", LogCategory.UI);
                OnClick?.Invoke(this);
            }
            
            // Mettre à jour l'état pressé
            IsPressed = IsHovered && currentMouseState.LeftButton == ButtonState.Pressed;
            
            // Propager aux enfants en premier (ordre inverse pour que les éléments au-dessus reçoivent les événements en premier)
            for (int i = _children.Count - 1; i >= 0; i--)
            {
                var child = _children[i];
                child.UpdateInput(currentMouseState, previousMouseState);
                
                // Si un enfant est hovering/clicked, interrompre la propagation aux autres enfants
                if (child.IsHovered || child.IsPressed)
                    break;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
                return;
                
            // Dessiner l'élément UI
            if (_useRenderTarget && _renderTarget != null)
            {
                DrawToRenderTarget(spriteBatch);
                spriteBatch.Draw(_renderTarget, Position, null, Color * Opacity, Rotation, Vector2.Zero, Scale, SpriteEffects.None, 0);
            }
            else
            {
                // Dessiner directement
                DrawChildren(spriteBatch);
            }
        }
        
        protected virtual void DrawToRenderTarget(SpriteBatch spriteBatch)
        {
            if (!_needsRedraw)
                return;
                
            // Configurer le rendu vers la cible
            var graphicsDevice = spriteBatch.GraphicsDevice;
            graphicsDevice.SetRenderTarget(_renderTarget);
            graphicsDevice.Clear(Color.Transparent);
            
            // Commencer le batch avec les paramètres appropriés
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            
            // Dessiner les enfants
            DrawChildren(spriteBatch, true);
            
            // Terminer le batch
            spriteBatch.End();
            
            // Revenir au rendu normal
            graphicsDevice.SetRenderTarget(null);
            
            _needsRedraw = false;
        }
        
        protected virtual void DrawChildren(SpriteBatch spriteBatch, bool isRenderTarget = false)
        {
            // Les classes dérivées peuvent implémenter cette méthode
            foreach (var child in _children)
            {
                if (isRenderTarget)
                {
                    // Ajuster la position pour le renderTarget
                    Vector2 originalPos = child.Position;
                    child.Position -= Position;
                    child.Draw(spriteBatch);
                    child.Position = originalPos;
                }
                else
                {
                    child.Draw(spriteBatch);
                }
            }
        }

        public bool Contains(Point point)
        {
            if (Parent != null)
            {
                // Si nous sommes un enfant, calculer notre position absolue
                Vector2 absolutePosition = Position;
                
                // Ajouter plus de logs pour déboguer les positions
                Logger.Instance.Debug($"Element position: {Position.X},{Position.Y}, Size: {Size.X},{Size.Y}", LogCategory.UI);
                
                // Vérifier si le point est dans nos limites absolues
                Rectangle absoluteBounds = new Rectangle(
                    (int)absolutePosition.X, 
                    (int)absolutePosition.Y, 
                    (int)(Size.X * Scale), 
                    (int)(Size.Y * Scale)
                );
                
                Logger.Instance.Debug($"Checking point {point.X},{point.Y} against bounds {absoluteBounds}", LogCategory.UI);
                
                return absoluteBounds.Contains(point);
            }
            
            // Pour un élément racine, utiliser les limites absolues
            Rectangle bounds = new Rectangle(
                (int)Position.X, 
                (int)Position.Y, 
                (int)(Size.X * Scale), 
                (int)(Size.Y * Scale)
            );
            
            return bounds.Contains(point);
        }

        public bool Contains(Vector2 point)
        {
            return Contains(new Point((int)point.X, (int)point.Y));
        }
        
        public void AddChild(UIElement child)
        {
            if (child.Parent != null)
            {
                throw new InvalidOperationException("L'élément a déjà un parent.");
            }
            
            child.Parent = this;
            _children.Add(child);
            
            // Sauvegarder la position originale comme position relative
            child.RelativePosition = child.Position - Position;
            
            // La position absolue ne change pas, elle reste celle définie à la création
            
            _needsRedraw = true;
        }
        
        public void RemoveChild(UIElement child)
        {
            if (_children.Contains(child))
            {
                child.Parent = null;
                _children.Remove(child);
                _needsRedraw = true;
            }
        }
        
        public void StartTransition(string property, float targetValue, float duration, EasingFunction easingFunction = null)
        {
            if (easingFunction == null)
                easingFunction = EasingFunctions.Linear;
                
            float startValue = GetPropertyValue(property);
            
            var transition = new Transition
            {
                PropertyName = property,
                StartValue = startValue,
                TargetValue = targetValue,
                Duration = duration,
                ElapsedTime = 0,
                EasingFunction = easingFunction
            };
            
            _activeTransitions[property] = transition;
        }
        
        private void UpdateTransitions(GameTime gameTime)
        {
            List<string> completedTransitions = new List<string>();
            
            foreach (var kvp in _activeTransitions)
            {
                string property = kvp.Key;
                Transition transition = kvp.Value;
                
                transition.ElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                float progress = Math.Min(transition.ElapsedTime / transition.Duration, 1f);
                float easedProgress = transition.EasingFunction(progress);
                
                float currentValue = transition.StartValue + (transition.TargetValue - transition.StartValue) * easedProgress;
                SetPropertyValue(property, currentValue);
                
                if (progress >= 1f)
                {
                    completedTransitions.Add(property);
                }
            }
            
            // Supprimer les transitions terminées
            foreach (string property in completedTransitions)
            {
                _activeTransitions.Remove(property);
            }
            
            if (completedTransitions.Count > 0)
            {
                _needsRedraw = true;
            }
        }
        
        private float GetPropertyValue(string propertyName)
        {
            switch (propertyName)
            {
                case "Opacity": return Opacity;
                case "Scale": return Scale;
                case "Rotation": return Rotation;
                case "PositionX": return Position.X;
                case "PositionY": return Position.Y;
                default: throw new ArgumentException($"La propriété {propertyName} n'est pas supportée pour les transitions.");
            }
        }
        
        private void SetPropertyValue(string propertyName, float value)
        {
            switch (propertyName)
            {
                case "Opacity": Opacity = value; break;
                case "Scale": Scale = value; break;
                case "Rotation": Rotation = value; break;
                case "PositionX": Position = new Vector2(value, Position.Y); break;
                case "PositionY": Position = new Vector2(Position.X, value); break;
                default: throw new ArgumentException($"La propriété {propertyName} n'est pas supportée pour les transitions.");
            }
        }

        // Permet d'accéder aux enfants pour le raycast
        public List<UIElement> GetChildren()
        {
            return _children;
        }

        // Méthodes publiques pour la gestion des états d'interaction par le UIManager
        public void SetHovered(bool isHovered)
        {
            bool wasHovered = IsHovered;
            IsHovered = isHovered;
            
            // Déclencher les événements appropriés
            if (IsHovered && !wasHovered)
            {
                OnHover?.Invoke(this);
            }
            else if (!IsHovered && wasHovered)
            {
                OnHoverExit?.Invoke(this);
            }
        }
        
        public void SetPressed(bool isPressed)
        {
            IsPressed = isPressed;
        }
        
        public void TriggerClick()
        {
            Logger.Instance.Debug($"TriggerClick called on {GetType().Name}", LogCategory.UI);
            
            // Invoquer l'événement OnClick
            OnClick?.Invoke(this);
            
            // Si c'est un bouton avec une action directe, l'exécuter directement
            if (this is Button button && button.OnClickAction != null)
            {
                Logger.Instance.Info($"Executing OnClickAction for button '{button.Text}'", LogCategory.UI);
                try
                {
                    button.OnClickAction.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"Error executing OnClickAction: {ex.Message}", LogCategory.UI);
                }
            }
        }
    }
    
    // Classe pour les marges et rembourrages
    public struct Thickness
    {
        public float Left;
        public float Top;
        public float Right;
        public float Bottom;
        
        public Thickness(float uniform)
        {
            Left = Top = Right = Bottom = uniform;
        }
        
        public Thickness(float horizontal, float vertical)
        {
            Left = Right = horizontal;
            Top = Bottom = vertical;
        }
        
        public Thickness(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
    
    // Classes pour les transitions
    public class Transition
    {
        public string PropertyName;
        public float StartValue;
        public float TargetValue;
        public float Duration;
        public float ElapsedTime;
        public EasingFunction EasingFunction;
    }
    
    public delegate float EasingFunction(float t);
    
    public static class EasingFunctions
    {
        public static float Linear(float t) => t;
        
        public static float EaseInQuad(float t) => t * t;
        
        public static float EaseOutQuad(float t) => 1 - (1 - t) * (1 - t);
        
        public static float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2 * t * t : 1 - MathF.Pow(-2 * t + 2, 2) / 2;
        }
        
        public static float EaseInBack(float t)
        {
            float c1 = 1.70158f;
            return t * t * ((c1 + 1) * t - c1);
        }
        
        public static float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            return 1 + (t - 1) * (t - 1) * ((c1 + 1) * (t - 1) + c1);
        }
        
        public static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            
            if (t < 1 / d1) {
                return n1 * t * t;
            } else if (t < 2 / d1) {
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            } else if (t < 2.5 / d1) {
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            } else {
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
            }
        }
    }
}