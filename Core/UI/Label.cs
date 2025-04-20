using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Potato.Core.UI
{
    public class Label : UIElement
    {
        private string _text;
        private SpriteFont _font;
        private Color _textColor = Color.White;
        private bool _drawBackground = false;
        private Color _backgroundColor = new Color(0, 0, 0, 100);
        private float _cornerRadius = 5f;
        private int _padding = 5;
        private TextAlignment _horizontalAlignment = TextAlignment.Left;
        private TextAlignment _verticalAlignment = TextAlignment.Top;
        private bool _autoSize = true;
        
        public Label(Vector2 position, string text) 
            : base(position, Vector2.Zero)
        {
            _text = text;
            UpdateSize();
        }
        
        public Label(Vector2 position, Vector2 size, string text) 
            : base(position, size)
        {
            _text = text;
            _autoSize = false;
        }
        
        public Label(Vector2 position, Vector2 size, string text, SpriteFont font) 
            : base(position, size)
        {
            _text = text;
            _font = font;
            _autoSize = false;
        }

        public override void Update(GameTime gameTime)
        {
            // Labels don't need updates for basic functionality
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
                return;
                
            // Draw background if enabled
            if (_drawBackground)
            {
                DrawRoundedRectangle(spriteBatch, Bounds, _backgroundColor, _cornerRadius);
            }
            
            if (_font == null)
            {
                _font = UIManager.DefaultFont;
            }

            // Draw text if font is available
            if (_font != null && !string.IsNullOrEmpty(_text))
            {
                Vector2 textSize = _font.MeasureString(_text);
                Vector2 textPosition = CalculateTextPosition(textSize);
                
                spriteBatch.DrawString(_font, _text, textPosition, _textColor);
            }
        }
        
        private Vector2 CalculateTextPosition(Vector2 textSize)
        {
            float x = Position.X;
            float y = Position.Y;
            
            // Calculate horizontal position based on alignment
            switch (_horizontalAlignment)
            {
                case TextAlignment.Center:
                    x = Position.X + (Size.X - textSize.X) / 2;
                    break;
                case TextAlignment.Right:
                    x = Position.X + Size.X - textSize.X - _padding;
                    break;
                case TextAlignment.Left:
                default:
                    x = Position.X + _padding;
                    break;
            }
            
            // Calculate vertical position based on alignment
            switch (_verticalAlignment)
            {
                case TextAlignment.Center:
                    y = Position.Y + (Size.Y - textSize.Y) / 2;
                    break;
                case TextAlignment.Bottom:
                    y = Position.Y + Size.Y - textSize.Y - _padding;
                    break;
                case TextAlignment.Top:
                default:
                    y = Position.Y + _padding;
                    break;
            }
            
            return new Vector2(x, y);
        }
        
        private void UpdateSize()
        {
            if (_autoSize && _font != null && !string.IsNullOrEmpty(_text))
            {
                Vector2 textSize = _font.MeasureString(_text);
                Size = new Vector2(
                    textSize.X + (_padding * 2),
                    textSize.Y + (_padding * 2));
            }
        }
        
        private void DrawRoundedRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, float radius)
        {
            // Implementation identical to other UI components
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
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                if (_autoSize)
                {
                    UpdateSize();
                }
            }
        }
        
        public SpriteFont Font
        {
            get => _font;
            set
            {
                _font = value;
                if (_autoSize)
                {
                    UpdateSize();
                }
            }
        }
        
        public Color TextColor
        {
            get => _textColor;
            set => _textColor = value;
        }
        
        public bool DrawBackground
        {
            get => _drawBackground;
            set => _drawBackground = value;
        }
        
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
        
        public new int Padding
        {
            get => _padding;
            set
            {
                _padding = value;
                if (_autoSize)
                {
                    UpdateSize();
                }
            }
        }
        
        public TextAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set => _horizontalAlignment = value;
        }
        
        public TextAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set => _verticalAlignment = value;
        }
        
        public bool AutoSize
        {
            get => _autoSize;
            set
            {
                _autoSize = value;
                if (_autoSize)
                {
                    UpdateSize();
                }
            }
        }
    }
    
    public enum TextAlignment
    {
        Left,
        Center,
        Right,
        Top,
        Bottom
    }
}