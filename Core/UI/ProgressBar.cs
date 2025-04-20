using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Potato.Core.UI
{
    public class ProgressBar : UIElement
    {
        private float _value;
        private float _minValue = 0;
        private float _maxValue = 100;
        private string _format = "{0}/{1}";
        private bool _showText = true;
        private SpriteFont _font;
        
        // Appearance
        private Color _backgroundColor = new Color(40, 40, 40, 200);
        private Color _fillColor = new Color(50, 180, 100, 220);
        private Color _borderColor = new Color(30, 30, 30, 255);
        private Color _textColor = Color.White;
        private float _cornerRadius = 4f;
        private bool _drawBorder = true;
        private int _borderThickness = 1;
        private FillDirection _fillDirection = FillDirection.LeftToRight;
        
        public ProgressBar(Vector2 position, Vector2 size, float maxValue = 100f, float value = 0f) 
            : base(position, size)
        {
            _maxValue = maxValue;
            _value = value;
        }

        public override void Update(GameTime gameTime)
        {
            // ProgressBar doesn't need update logic for basic functionality
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
                return;

            if (_font == null)
            {
                _font = UIManager.DefaultFont;
            }
                
            // Draw background
            DrawRoundedRectangle(spriteBatch, Bounds, _backgroundColor, _cornerRadius);
            
            // Calculate fill width/height based on value
            float percentage = MathHelper.Clamp((_value - _minValue) / (_maxValue - _minValue), 0, 1);
            Rectangle fillRect = GetFillRectangle(percentage);
            
            // Draw fill
            if (percentage > 0)
            {
                DrawRoundedRectangle(spriteBatch, fillRect, _fillColor, _cornerRadius);
            }
            
            // Draw border
            if (_drawBorder && _borderThickness > 0)
            {
                RenderBorder(spriteBatch, Bounds, _borderColor, _cornerRadius, _borderThickness);
            }
            
            // Draw text
            if (_showText && _font != null)
            {
                string text = string.Format(_format, _value.ToString("F0"), _maxValue.ToString("F0"));
                Vector2 textSize = _font.MeasureString(text);
                Vector2 textPosition = new Vector2(
                    Position.X + (Size.X - textSize.X) / 2, // Center horizontally
                    Position.Y + (Size.Y - textSize.Y) / 2  // Center vertically
                );
                
                spriteBatch.DrawString(_font, text, textPosition, _textColor);
            }
        }
        
        private Rectangle GetFillRectangle(float percentage)
        {
            switch (_fillDirection)
            {
                case FillDirection.LeftToRight:
                    return new Rectangle(
                        (int)Position.X,
                        (int)Position.Y,
                        (int)(Size.X * percentage),
                        (int)Size.Y);
                    
                case FillDirection.RightToLeft:
                    return new Rectangle(
                        (int)(Position.X + Size.X * (1 - percentage)),
                        (int)Position.Y,
                        (int)(Size.X * percentage),
                        (int)Size.Y);
                    
                case FillDirection.BottomToTop:
                    return new Rectangle(
                        (int)Position.X,
                        (int)(Position.Y + Size.Y * (1 - percentage)),
                        (int)Size.X,
                        (int)(Size.Y * percentage));
                    
                case FillDirection.TopToBottom:
                    return new Rectangle(
                        (int)Position.X,
                        (int)Position.Y,
                        (int)Size.X,
                        (int)(Size.Y * percentage));
                    
                default:
                    return new Rectangle(
                        (int)Position.X,
                        (int)Position.Y,
                        (int)(Size.X * percentage),
                        (int)Size.Y);
            }
        }
        
        private void RenderBorder(SpriteBatch spriteBatch, Rectangle rect, Color color, float radius, int thickness)
        {
            // Draw four border segments
            // Top
            Rectangle topBorder = new Rectangle(
                rect.X,
                rect.Y,
                rect.Width,
                thickness);
                
            // Bottom
            Rectangle bottomBorder = new Rectangle(
                rect.X,
                rect.Y + rect.Height - thickness,
                rect.Width,
                thickness);
                
            // Left
            Rectangle leftBorder = new Rectangle(
                rect.X,
                rect.Y + thickness,
                thickness,
                rect.Height - (thickness * 2));
                
            // Right
            Rectangle rightBorder = new Rectangle(
                rect.X + rect.Width - thickness,
                rect.Y + thickness,
                thickness,
                rect.Height - (thickness * 2));
                
            // Draw border segments
            spriteBatch.Draw(UIManager.Pixel, topBorder, color);
            spriteBatch.Draw(UIManager.Pixel, bottomBorder, color);
            spriteBatch.Draw(UIManager.Pixel, leftBorder, color);
            spriteBatch.Draw(UIManager.Pixel, rightBorder, color);
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
        public float Value
        {
            get => _value;
            set => _value = MathHelper.Clamp(value, _minValue, _maxValue);
        }
        
        public float MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                _value = MathHelper.Clamp(_value, _minValue, _maxValue);
            }
        }
        
        public float MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = Math.Max(value, _minValue);
                _value = MathHelper.Clamp(_value, _minValue, _maxValue);
            }
        }
        
        public SpriteFont Font
        {
            get => _font;
            set => _font = value;
        }
        
        public string Format
        {
            get => _format;
            set => _format = value ?? "{0}/{1}";
        }
        
        public bool ShowText
        {
            get => _showText;
            set => _showText = value;
        }
        
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }
        
        public Color FillColor
        {
            get => _fillColor;
            set => _fillColor = value;
        }
        
        public new Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; }
        }
        
        public Color TextColor
        {
            get => _textColor;
            set => _textColor = value;
        }
        
        public new float CornerRadius
        {
            get { return _cornerRadius; }
            set { _cornerRadius = value; }
        }
        
        public bool DrawBorder
        {
            get => _drawBorder;
            set => _drawBorder = value;
        }
        
        public new int BorderThickness
        {
            get { return _borderThickness; }
            set { _borderThickness = Math.Max(0, value); }
        }
        
        public FillDirection FillDirection
        {
            get => _fillDirection;
            set => _fillDirection = value;
        }
    }
    
    public enum FillDirection
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }
}