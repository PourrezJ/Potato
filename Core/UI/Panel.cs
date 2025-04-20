using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Potato.Core.UI
{
    public class Panel : UIElement
    {
        protected new List<UIElement> _children = new List<UIElement>();
        private Color _backgroundColor;
        private float _cornerRadius = 5f;
        private bool _drawBackground = true;
        private int _padding = 10;
        private string _title = "";
        private SpriteFont _font;
        private Color _titleColor = Color.White;
        private int _titleHeight = 30;
        private bool _hasTitle = false;
        private bool _hasBorder = false;
        private Color _borderColor = Color.Black;
        private int _borderThickness = 2;
        
        public Panel(Vector2 position, Vector2 size, Color backgroundColor) 
            : base(position, size)
        {
            _backgroundColor = backgroundColor;
        }
        
        public Panel(Vector2 position, Vector2 size, Color backgroundColor, string title) 
            : this(position, size, backgroundColor)
        {
            _title = title;
            _hasTitle = !string.IsNullOrEmpty(title);
        }

        public override void Update(GameTime gameTime)
        {
            // Update all child elements
            foreach (var child in _children)
            {
                if (child.IsVisible)
                {
                    child.Update(gameTime);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_font == null)
            {
                _font = UIManager.DefaultFont;
            }

            // Dessiner la bordure si activée
            if (HasBorder)
            {
                // Créer un rectangle légèrement plus grand pour la bordure
                Rectangle borderRect = new Rectangle(
                    (int)Position.X - BorderThickness,
                    (int)Position.Y - BorderThickness,
                    (int)Size.X + BorderThickness * 2,
                    (int)Size.Y + BorderThickness * 2);
                    
                DrawRoundedRectangle(spriteBatch, borderRect, BorderColor, _cornerRadius + BorderThickness);
            }
            
            // Draw panel background if enabled
            if (_drawBackground)
            {
                DrawRoundedRectangle(spriteBatch, Bounds, _backgroundColor, _cornerRadius);
            }
            
            // Draw title bar if panel has a title
            if (_hasTitle && _font != null)
            {
                // Title background
                Rectangle titleRect = new Rectangle(
                    (int)Position.X,
                    (int)Position.Y,
                    (int)Size.X,
                    _titleHeight);
                    
                // Draw title bar with slightly darker color
                Color titleBarColor = new Color(
                    (int)(_backgroundColor.R * 0.8f),
                    (int)(_backgroundColor.G * 0.8f),
                    (int)(_backgroundColor.B * 0.8f),
                    _backgroundColor.A);
                    
                DrawRoundedRectangle(
                    spriteBatch, 
                    titleRect, 
                    titleBarColor, 
                    new[] { _cornerRadius, _cornerRadius, 0, 0 }); // Only round top corners
                
                // Draw title text
                Vector2 textSize = _font.MeasureString(_title);
                Vector2 textPosition = new Vector2(
                    Position.X + (Size.X - textSize.X) / 2, // Center horizontally
                    Position.Y + (_titleHeight - textSize.Y) / 2); // Center vertically in title bar
                    
                spriteBatch.DrawString(_font, _title, textPosition, _titleColor);
            }
            
            // Draw all child elements
            foreach (var child in _children)
            {
                if (child.IsVisible)
                {
                    child.Draw(spriteBatch);
                }
            }
        }
        
        public new virtual void AddChild(UIElement element)
        {
            // Appel de la méthode AddChild de la classe de base pour gérer le parent
            base.AddChild(element);
            _children.Add(element);
            
            // Si une police a été définie pour ce panel, la propager aux éléments enfants
            if (_font != null && element is Button button && button.Font == null)
            {
                button.Font = _font;
            }
            else if (_font != null && element is Label label && label.Font == null)
            {
                label.Font = _font;
            }
            else if (_font != null && element is Panel childPanel && childPanel.Font == null)
            {
                childPanel.Font = _font;
            }
            
            // Vérifier si la position est déjà relative au panel ou absolue
            // Si la position est petite (semble être relative), ajustons-la
            if (Math.Abs(element.Position.X) < Size.X && Math.Abs(element.Position.Y) < Size.Y)
            {
                // La position semble relative, ajuster par rapport au panel
                int yOffset = _hasTitle ? _titleHeight : 0;
                element.Position = new Vector2(
                    Position.X + _padding + element.Position.X,
                    Position.Y + yOffset + _padding + element.Position.Y);
            }
        }
        
        public new virtual bool RemoveChild(UIElement element)
        {
            return _children.Remove(element);
        }
        
        public void ClearChildren()
        {
            _children.Clear();
        }
        
        private bool IsPositionLocal(Vector2 position)
        {
            // Heuristic to determine if a position is local to the panel
            // or already in screen coordinates
            return position.X < Size.X && position.Y < Size.Y;
        }
        
        // Draw a rounded rectangle with different corner radii
        private void DrawRoundedRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, float[] cornerRadii)
        {
            if (cornerRadii.Length != 4)
            {
                // Fall back to regular rounded rectangle if invalid corner radii
                DrawRoundedRectangle(spriteBatch, rectangle, color, cornerRadii[0]);
                return;
            }
            
            float topLeftRadius = cornerRadii[0];
            float topRightRadius = cornerRadii[1];
            float bottomRightRadius = cornerRadii[2];
            float bottomLeftRadius = cornerRadii[3];
            
            // Ensure radii are not too large
            float maxRadiusWidth = rectangle.Width / 2;
            float maxRadiusHeight = rectangle.Height / 2;
            topLeftRadius = Math.Min(topLeftRadius, Math.Min(maxRadiusWidth, maxRadiusHeight));
            topRightRadius = Math.Min(topRightRadius, Math.Min(maxRadiusWidth, maxRadiusHeight));
            bottomRightRadius = Math.Min(bottomRightRadius, Math.Min(maxRadiusWidth, maxRadiusHeight));
            bottomLeftRadius = Math.Min(bottomLeftRadius, Math.Min(maxRadiusWidth, maxRadiusHeight));
            
            // Center rectangle
            Rectangle centerRect = new Rectangle(
                rectangle.X + (int)Math.Max(topLeftRadius, bottomLeftRadius),
                rectangle.Y + (int)Math.Max(topLeftRadius, topRightRadius),
                rectangle.Width - (int)(Math.Max(topLeftRadius, bottomLeftRadius) + Math.Max(topRightRadius, bottomRightRadius)),
                rectangle.Height - (int)(Math.Max(topLeftRadius, topRightRadius) + Math.Max(bottomLeftRadius, bottomRightRadius))
            );
            spriteBatch.Draw(UIManager.Pixel, centerRect, color);
            
            // Top rectangle
            Rectangle topRect = new Rectangle(
                rectangle.X + (int)topLeftRadius,
                rectangle.Y,
                rectangle.Width - (int)(topLeftRadius + topRightRadius),
                (int)Math.Max(topLeftRadius, topRightRadius)
            );
            spriteBatch.Draw(UIManager.Pixel, topRect, color);
            
            // Bottom rectangle
            Rectangle bottomRect = new Rectangle(
                rectangle.X + (int)bottomLeftRadius,
                rectangle.Y + rectangle.Height - (int)Math.Max(bottomLeftRadius, bottomRightRadius),
                rectangle.Width - (int)(bottomLeftRadius + bottomRightRadius),
                (int)Math.Max(bottomLeftRadius, bottomRightRadius)
            );
            spriteBatch.Draw(UIManager.Pixel, bottomRect, color);
            
            // Left rectangle
            Rectangle leftRect = new Rectangle(
                rectangle.X,
                rectangle.Y + (int)topLeftRadius,
                (int)Math.Max(topLeftRadius, bottomLeftRadius),
                rectangle.Height - (int)(topLeftRadius + bottomLeftRadius)
            );
            spriteBatch.Draw(UIManager.Pixel, leftRect, color);
            
            // Right rectangle
            Rectangle rightRect = new Rectangle(
                rectangle.X + rectangle.Width - (int)Math.Max(topRightRadius, bottomRightRadius),
                rectangle.Y + (int)topRightRadius,
                (int)Math.Max(topRightRadius, bottomRightRadius),
                rectangle.Height - (int)(topRightRadius + bottomRightRadius)
            );
            spriteBatch.Draw(UIManager.Pixel, rightRect, color);
        }
        
        private void DrawRoundedRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, float radius)
        {
            // Implementation similar to Button's DrawRoundedRectangle
            if (radius <= 0)
            {
                spriteBatch.Draw(UIManager.Pixel, rectangle, color);
                return;
            }

            radius = Math.Min(radius, Math.Min(rectangle.Width, rectangle.Height) / 2);

            Rectangle centerRect = new Rectangle(
                rectangle.X + (int)radius,
                rectangle.Y + (int)radius,
                rectangle.Width - (int)(radius * 2),
                rectangle.Height - (int)(radius * 2)
            );
            spriteBatch.Draw(UIManager.Pixel, centerRect, color);

            Rectangle topRect = new Rectangle(
                rectangle.X + (int)radius,
                rectangle.Y,
                rectangle.Width - (int)(radius * 2),
                (int)radius
            );
            spriteBatch.Draw(UIManager.Pixel, topRect, color);

            Rectangle bottomRect = new Rectangle(
                rectangle.X + (int)radius,
                rectangle.Y + rectangle.Height - (int)radius,
                rectangle.Width - (int)(radius * 2),
                (int)radius
            );
            spriteBatch.Draw(UIManager.Pixel, bottomRect, color);

            Rectangle leftRect = new Rectangle(
                rectangle.X,
                rectangle.Y + (int)radius,
                (int)radius,
                rectangle.Height - (int)(radius * 2)
            );
            spriteBatch.Draw(UIManager.Pixel, leftRect, color);

            Rectangle rightRect = new Rectangle(
                rectangle.X + rectangle.Width - (int)radius,
                rectangle.Y + (int)radius,
                (int)radius,
                rectangle.Height - (int)(radius * 2)
            );
            spriteBatch.Draw(UIManager.Pixel, rightRect, color);
        }
        
        // Public properties
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }
        
        public new float CornerRadius
        {
            get => _cornerRadius;
            set => _cornerRadius = value;
        }
        
        public bool DrawBackground
        {
            get => _drawBackground;
            set => _drawBackground = value;
        }
        
        public new int Padding
        {
            get => _padding;
            set => _padding = value;
        }
        
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                _hasTitle = !string.IsNullOrEmpty(_title);
            }
        }
        
        public Color TitleColor
        {
            get => _titleColor;
            set => _titleColor = value;
        }
        
        public int TitleHeight
        {
            get => _titleHeight;
            set => _titleHeight = Math.Max(20, value);
        }
        
        public IReadOnlyList<UIElement> Children => _children;

        public new bool HasBorder
        {
            get => _hasBorder;
            set => _hasBorder = value;
        }

        public new Color BorderColor
        {
            get => _borderColor;
            set => _borderColor = value;
        }

        public new int BorderThickness
        {
            get => _borderThickness;
            set => _borderThickness = value;
        }

        // Propriété Font pour accéder à la police
        public SpriteFont Font
        {
            get => _font;
            set => _font = value;
        }
    }
}