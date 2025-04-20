using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Potato.Core.UI
{
    public class Toggle : UIElement
    {
        private bool _isOn;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private bool _isHovered;
        private SpriteFont _font;
        private string _label;
        
        // Appearance
        private Color _offColor = new Color(100, 100, 100, 220);
        private Color _onColor = new Color(50, 180, 100, 220);
        private Color _handleColor = Color.White;
        private Color _textColor = Color.White; // Renamed from _labelColor to _textColor
        private float _textScale = 1.0f; // New property for text scaling
        private float _cornerRadius = 8f;
        private float _animationProgress = 0f; // For smooth transition
        private float _animationSpeed = 10f;
        
        // Size constants
        private const float SwitchWidthMultiplier = 1.8f; // width = height * this
        
        public event Action<bool> OnToggled;

        public Toggle(Vector2 position, Vector2 size, bool isOn = false, string label = "")
            : base(position, size)
        {
            _isOn = isOn;
            _label = label;
        }

        public override void Update(GameTime gameTime)
        {
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
            
            // Update animation
            float targetProgress = _isOn ? 1f : 0f;
            if (_animationProgress != targetProgress)
            {
                // Calculate delta time in seconds
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                // Smoothly animate towards target
                if (_animationProgress < targetProgress)
                {
                    _animationProgress = Math.Min(_animationProgress + _animationSpeed * deltaTime, targetProgress);
                }
                else
                {
                    _animationProgress = Math.Max(_animationProgress - _animationSpeed * deltaTime, targetProgress);
                }
            }
            
            // Calculate toggle area (just the switch, not label)
            Rectangle toggleRect = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                (int)Size.X,
                (int)Size.Y);
                
            // Check for hover state
            Point mousePos = new Point(_currentMouseState.X, _currentMouseState.Y);
            _isHovered = toggleRect.Contains(mousePos);
            
            // Handle click
            if (_isHovered && 
                _currentMouseState.LeftButton == ButtonState.Released && 
                _previousMouseState.LeftButton == ButtonState.Pressed)
            {
                _isOn = !_isOn;
                OnToggled?.Invoke(_isOn);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_font == null)
            {
                _font = UIManager.DefaultFont;
            }

            // Draw the switch background
            Rectangle switchRect = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                (int)Size.X,
                (int)Size.Y);
                
            // Calculate color based on state with smooth transition
            Color trackColor = Color.Lerp(_offColor, _onColor, _animationProgress);
            
            // Draw rounded track
            DrawRoundedRectangle(spriteBatch, switchRect, trackColor, _cornerRadius);
            
            // Calculate handle position with animation
            float handleOffset = Size.Y * 0.1f; // Padding from edges
            float handleSize = Size.Y - (handleOffset * 2);
            float handleTravel = Size.X - handleSize - (handleOffset * 2);
            
            Rectangle handleRect = new Rectangle(
                (int)(Position.X + handleOffset + (_animationProgress * handleTravel)),
                (int)(Position.Y + handleOffset),
                (int)handleSize,
                (int)handleSize);
                
            // Draw handle
            DrawRoundedRectangle(spriteBatch, handleRect, _handleColor, _cornerRadius);
            
            // Draw label if present
            if (_font != null && !string.IsNullOrEmpty(_label))
            {
                Vector2 textSize = _font.MeasureString(_label) * _textScale;
                Vector2 textPos = new Vector2(
                    Position.X + Size.X + 10, // Position to the right of the switch
                    Position.Y + (Size.Y - textSize.Y) / 2 // Center vertically
                );
                
                spriteBatch.DrawString(_font, _label, textPos, _textColor, 0f, Vector2.Zero, _textScale, SpriteEffects.None, 0f);
            }
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
        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;
                    // Don't invoke OnToggled here as it's not user-initiated
                }
            }
        }
        
        public string Label
        {
            get => _label;
            set => _label = value;
        }
        
        public Color OffColor
        {
            get => _offColor;
            set => _offColor = value;
        }
        
        public Color OnColor
        {
            get => _onColor;
            set => _onColor = value;
        }
        
        public Color HandleColor
        {
            get => _handleColor;
            set => _handleColor = value;
        }
        
        public Color TextColor
        {
            get => _textColor;
            set => _textColor = value;
        }
        
        public float TextScale
        {
            get => _textScale;
            set => _textScale = value;
        }
        
        public new float CornerRadius
        {
            get => _cornerRadius;
            set => _cornerRadius = value;
        }
        
        public float AnimationSpeed
        {
            get => _animationSpeed;
            set => _animationSpeed = MathHelper.Clamp(value, 1f, 20f);
        }
    }
}