using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Potato.Core.UI
{
    public class Slider : UIElement
    {
        private float _minValue;
        private float _maxValue;
        private float _value;
        private float _step;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private bool _isDragging = false;
        private SpriteFont _font;
        private string _label;
        private bool _showValue = true;
        
        // Appearance properties
        private Color _trackColor = new Color(70, 70, 70, 200);
        private Color _fillColor = new Color(120, 180, 210, 230);
        private Color _handleColor = new Color(200, 200, 200, 255);
        private Color _handleHoverColor = new Color(230, 230, 230, 255);
        private Color _textColor = Color.White;
        private int _trackHeight = 6;
        private int _handleSize = 16;
        private int _labelOffset = 25; // Vertical space for label

        public event Action<float> OnValueChanged;

        public Slider(Vector2 position, Vector2 size, float minValue, float maxValue, float value = 0, string label = "") 
            : base(position, size)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _value = MathHelper.Clamp(value, minValue, maxValue);
            _step = 0; // Default to no stepping
            _label = label;
        }

        public override void Update(GameTime gameTime)
        {
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
            
            Point mousePos = new Point(_currentMouseState.X, _currentMouseState.Y);
            
            // Calculate the handle position
            Vector2 handlePos = GetHandlePosition();
            Rectangle handleRect = new Rectangle(
                (int)handlePos.X - _handleSize / 2,
                (int)handlePos.Y - _handleSize / 2,
                _handleSize,
                _handleSize);
                
            // Check for handle hover and drag
            bool isHandleHovered = handleRect.Contains(mousePos);
            
            // Start dragging
            if (isHandleHovered && 
                _currentMouseState.LeftButton == ButtonState.Pressed && 
                _previousMouseState.LeftButton == ButtonState.Released)
            {
                _isDragging = true;
            }
            
            // End dragging
            if (_isDragging && _currentMouseState.LeftButton == ButtonState.Released)
            {
                _isDragging = false;
            }
            
            // Process dragging
            if (_isDragging)
            {
                UpdateValueFromMousePosition(mousePos.X);
            }
            
            // Check for track click
            if (!_isDragging && 
                _currentMouseState.LeftButton == ButtonState.Pressed && 
                _previousMouseState.LeftButton == ButtonState.Released)
            {
                Rectangle trackRect = GetTrackRectangle();
                if (trackRect.Contains(mousePos))
                {
                    UpdateValueFromMousePosition(mousePos.X);
                    _isDragging = true;
                }
            }
            
            // Check for keyboard or gamepad input when slider is focused
            // (This would require a concept of 'focused' UI element)
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_font == null)
            {
                _font = UIManager.DefaultFont;
            }

            // Draw label if present
            if (_font != null && !string.IsNullOrEmpty(_label))
            {
                Vector2 textSize = _font.MeasureString(_label);
                Vector2 textPos = new Vector2(
                    Position.X,
                    Position.Y - textSize.Y - 5); // Position above the slider
                
                spriteBatch.DrawString(_font, _label, textPos, _textColor);
            }
            
            // Draw track (background)
            Rectangle trackRect = GetTrackRectangle();
            spriteBatch.Draw(UIManager.Pixel, trackRect, _trackColor);
            
            // Draw filled portion of track
            float fillWidth = ((GetValuePercentage() * trackRect.Width));
            Rectangle fillRect = new Rectangle(
                trackRect.X, 
                trackRect.Y, 
                (int)fillWidth, 
                trackRect.Height);
            spriteBatch.Draw(UIManager.Pixel, fillRect, _fillColor);
            
            // Draw handle
            Vector2 handlePos = GetHandlePosition();
            
            // Determine if handle is being hovered or dragged
            Point mousePos = new Point(_currentMouseState.X, _currentMouseState.Y);
            bool isHandleActive = _isDragging || 
                new Rectangle(
                    (int)handlePos.X - _handleSize / 2,
                    (int)handlePos.Y - _handleSize / 2,
                    _handleSize,
                    _handleSize).Contains(mousePos);
            
            Color currentHandleColor = isHandleActive ? _handleHoverColor : _handleColor;
            
            // Draw handle as a circle (approximated by drawing a small rounded rectangle)
            DrawRoundedRectangle(
                spriteBatch,
                new Rectangle(
                    (int)handlePos.X - _handleSize / 2,
                    (int)handlePos.Y - _handleSize / 2,
                    _handleSize,
                    _handleSize),
                currentHandleColor,
                _handleSize / 2);
            
            // Draw value text if enabled
            if (_showValue && _font != null)
            {
                string valueText = _value.ToString("0.##"); // Format to show up to 2 decimal places
                Vector2 textSize = _font.MeasureString(valueText);
                Vector2 textPos = new Vector2(
                    Position.X + Size.X + 10, // Position to the right of the slider
                    Position.Y + (_labelOffset - textSize.Y) / 2); // Center vertically
                
                spriteBatch.DrawString(_font, valueText, textPos, _textColor);
            }
        }

        private Rectangle GetTrackRectangle()
        {
            return new Rectangle(
                (int)Position.X,
                (int)Position.Y + _labelOffset + (_handleSize - _trackHeight) / 2, // Center track vertically
                (int)Size.X,
                _trackHeight);
        }

        private Vector2 GetHandlePosition()
        {
            Rectangle trackRect = GetTrackRectangle();
            float handleX = trackRect.X + (GetValuePercentage() * trackRect.Width);
            float handleY = trackRect.Y + trackRect.Height / 2;
            
            return new Vector2(handleX, handleY);
        }

        private float GetValuePercentage()
        {
            return (_value - _minValue) / (_maxValue - _minValue);
        }

        private void UpdateValueFromMousePosition(int mouseX)
        {
            Rectangle trackRect = GetTrackRectangle();
            
            // Calculate percentage based on mouse position
            float percentage = MathHelper.Clamp(
                (mouseX - trackRect.X) / (float)trackRect.Width,
                0f,
                1f);
            
            // Convert percentage to value
            float newValue = _minValue + percentage * (_maxValue - _minValue);
            
            // Apply step if set
            if (_step > 0)
            {
                newValue = (float)Math.Round(newValue / _step) * _step;
            }
            
            // Clamp value to valid range
            newValue = MathHelper.Clamp(newValue, _minValue, _maxValue);
            
            // Set new value if changed
            if (newValue != _value)
            {
                _value = newValue;
                OnValueChanged?.Invoke(_value);
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
        public float Value
        {
            get => _value;
            set
            {
                value = MathHelper.Clamp(value, _minValue, _maxValue);
                if (_step > 0)
                {
                    value = (float)Math.Round(value / _step) * _step;
                }
                
                if (value != _value)
                {
                    _value = value;
                    OnValueChanged?.Invoke(_value);
                }
            }
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
                _maxValue = value;
                _value = MathHelper.Clamp(_value, _minValue, _maxValue);
            }
        }
        
        public float Step
        {
            get => _step;
            set => _step = value >= 0 ? value : 0;
        }
        
        public string Label
        {
            get => _label;
            set => _label = value;
        }
        
        public bool ShowValue
        {
            get => _showValue;
            set => _showValue = value;
        }
        
        public Color TrackColor
        {
            get => _trackColor;
            set => _trackColor = value;
        }
        
        public Color FillColor
        {
            get => _fillColor;
            set => _fillColor = value;
        }
        
        public Color HandleColor
        {
            get => _handleColor;
            set => _handleColor = value;
        }
        
        public Color HandleHoverColor
        {
            get => _handleHoverColor;
            set => _handleHoverColor = value;
        }
        
        public int TrackHeight
        {
            get => _trackHeight;
            set => _trackHeight = Math.Max(2, value);
        }
        
        public int HandleSize
        {
            get => _handleSize;
            set => _handleSize = Math.Max(6, value);
        }

        public Color TextColor
        {
            get => _textColor;
            set => _textColor = value;
        }
    }
}