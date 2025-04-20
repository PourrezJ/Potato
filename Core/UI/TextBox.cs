using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Potato.Core.UI
{
    public class TextBox : UIElement
    {
        private string _text = "";
        private string _placeholder = "";
        private SpriteFont _font;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        private bool _isFocused = false;
        private float _cursorBlinkTime = 0f;
        private bool _showCursor = false;
        private int _cursorPosition = 0;
        private int _selectionStart = -1;
        private int _maxLength = 100;
        private StringBuilder _inputBuffer = new StringBuilder();
        private double _keyRepeatTimer = 0;
        private Keys _lastPressedKey = Keys.None;
        
        // Appearance properties
        private Color _backgroundColor = new Color(60, 60, 60, 200);
        private Color _focusedColor = new Color(80, 80, 80, 220);
        private Color _textColor = Color.White;
        private Color _placeholderColor = new Color(180, 180, 180, 150);
        private Color _selectionColor = new Color(100, 150, 200, 150);
        private Color _cursorColor = new Color(220, 220, 220);
        private float _cornerRadius = 5f;
        private int _padding = 10;

        public event Action<string> OnTextChanged;
        public event Action<string> OnTextSubmitted;

        public TextBox(Vector2 position, Vector2 size, string placeholder = "") 
            : base(position, size)
        {
            _placeholder = placeholder;
        }

        public override void Update(GameTime gameTime)
        {
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();
            
            // Update cursor blink
            if (_isFocused)
            {
                _cursorBlinkTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_cursorBlinkTime >= 0.5f)
                {
                    _cursorBlinkTime = 0;
                    _showCursor = !_showCursor;
                }
            }
            
            // Handle mouse click to focus/unfocus
            if (_currentMouseState.LeftButton == ButtonState.Released && 
                _previousMouseState.LeftButton == ButtonState.Pressed)
            {
                Point mousePos = new Point(_currentMouseState.X, _currentMouseState.Y);
                bool wasFocused = _isFocused;
                _isFocused = Bounds.Contains(mousePos);
                
                if (_isFocused && !wasFocused)
                {
                    // Reset cursor state when gaining focus
                    _cursorBlinkTime = 0;
                    _showCursor = true;
                    
                    // Set cursor position based on click position
                    if (_text.Length > 0 && _font != null)
                    {
                        int relativeX = mousePos.X - ((int)Position.X + _padding);
                        _cursorPosition = GetCharIndexFromPosition(relativeX);
                    }
                    else
                    {
                        _cursorPosition = 0;
                    }
                    
                    _selectionStart = -1;
                }
            }
            
            // Handle keyboard input when focused
            if (_isFocused)
            {
                HandleKeyboardInput(gameTime);
            }
        }

        private void HandleKeyboardInput(GameTime gameTime)
        {
            double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
            const double KEY_INITIAL_DELAY = 0.5;
            const double KEY_REPEAT_INTERVAL = 0.03;
            
            // Handle text input
            foreach (Keys key in _currentKeyboardState.GetPressedKeys())
            {
                bool keyJustPressed = !_previousKeyboardState.IsKeyDown(key);
                
                // Check for special keys first
                if (key == Keys.Back || key == Keys.Delete || 
                    key == Keys.Left || key == Keys.Right || 
                    key == Keys.Enter || key == Keys.Tab)
                {
                    bool shouldProcess = keyJustPressed;
                    
                    // Handle key repeat for navigation keys
                    if (!keyJustPressed && key == _lastPressedKey)
                    {
                        _keyRepeatTimer += deltaTime;
                        if (_keyRepeatTimer > KEY_INITIAL_DELAY)
                        {
                            double repeatDelta = _keyRepeatTimer - KEY_INITIAL_DELAY;
                            if (repeatDelta > KEY_REPEAT_INTERVAL)
                            {
                                shouldProcess = true;
                                _keyRepeatTimer = KEY_INITIAL_DELAY + (repeatDelta % KEY_REPEAT_INTERVAL);
                            }
                        }
                    }
                    else if (keyJustPressed)
                    {
                        _lastPressedKey = key;
                        _keyRepeatTimer = 0;
                    }
                    
                    if (shouldProcess)
                    {
                        switch (key)
                        {
                            case Keys.Back:
                                HandleBackspace();
                                break;
                                
                            case Keys.Delete:
                                HandleDelete();
                                break;
                                
                            case Keys.Left:
                                if (_currentKeyboardState.IsKeyDown(Keys.LeftShift) || 
                                    _currentKeyboardState.IsKeyDown(Keys.RightShift))
                                {
                                    // Start selection if not already selecting
                                    if (_selectionStart == -1)
                                        _selectionStart = _cursorPosition;
                                }
                                else
                                {
                                    _selectionStart = -1;
                                }
                                
                                if (_cursorPosition > 0)
                                    _cursorPosition--;
                                    
                                // Reset cursor blink
                                _cursorBlinkTime = 0;
                                _showCursor = true;
                                break;
                                
                            case Keys.Right:
                                if (_currentKeyboardState.IsKeyDown(Keys.LeftShift) || 
                                    _currentKeyboardState.IsKeyDown(Keys.RightShift))
                                {
                                    // Start selection if not already selecting
                                    if (_selectionStart == -1)
                                        _selectionStart = _cursorPosition;
                                }
                                else
                                {
                                    _selectionStart = -1;
                                }
                                
                                if (_cursorPosition < _text.Length)
                                    _cursorPosition++;
                                    
                                // Reset cursor blink
                                _cursorBlinkTime = 0;
                                _showCursor = true;
                                break;
                                
                            case Keys.Enter:
                                OnTextSubmitted?.Invoke(_text);
                                _isFocused = false;
                                break;
                                
                            case Keys.Tab:
                                // Could implement tab navigation between UI elements here
                                _isFocused = false;
                                break;
                        }
                    }
                }
                else if (keyJustPressed)
                {
                    // Normal character input
                    char? character = GetCharFromKey(key, _currentKeyboardState);
                    if (character.HasValue && _text.Length < _maxLength)
                    {
                        DeleteSelectedText();
                        
                        _text = _text.Insert(_cursorPosition, character.Value.ToString());
                        _cursorPosition++;
                        
                        OnTextChanged?.Invoke(_text);
                        
                        // Reset selection
                        _selectionStart = -1;
                        
                        // Reset cursor blink
                        _cursorBlinkTime = 0;
                        _showCursor = true;
                    }
                }
            }
        }

        private void HandleBackspace()
        {
            if (_selectionStart != -1)
            {
                DeleteSelectedText();
            }
            else if (_cursorPosition > 0)
            {
                _text = _text.Remove(_cursorPosition - 1, 1);
                _cursorPosition--;
                OnTextChanged?.Invoke(_text);
            }
            
            // Reset cursor blink
            _cursorBlinkTime = 0;
            _showCursor = true;
        }

        private void HandleDelete()
        {
            if (_selectionStart != -1)
            {
                DeleteSelectedText();
            }
            else if (_cursorPosition < _text.Length)
            {
                _text = _text.Remove(_cursorPosition, 1);
                OnTextChanged?.Invoke(_text);
            }
            
            // Reset cursor blink
            _cursorBlinkTime = 0;
            _showCursor = true;
        }

        private void DeleteSelectedText()
        {
            if (_selectionStart != -1 && _selectionStart != _cursorPosition)
            {
                int start = Math.Min(_selectionStart, _cursorPosition);
                int length = Math.Abs(_selectionStart - _cursorPosition);
                
                _text = _text.Remove(start, length);
                _cursorPosition = start;
                _selectionStart = -1;
                
                OnTextChanged?.Invoke(_text);
            }
        }

        private int GetCharIndexFromPosition(int xPos)
        {
            if (_text.Length == 0 || _font == null)
                return 0;
                
            // Binary search would be more efficient for longer texts
            for (int i = 0; i <= _text.Length; i++)
            {
                string textPortion = _text.Substring(0, i);
                Vector2 size = _font.MeasureString(textPortion);
                
                if (size.X >= xPos)
                    return i;
            }
            
            return _text.Length;
        }

        private char? GetCharFromKey(Keys key, KeyboardState keyboardState)
        {
            bool shift = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
            
            // Handle letter keys
            if (key >= Keys.A && key <= Keys.Z)
            {
                char c = (char)('a' + (key - Keys.A));
                if (shift)
                    c = char.ToUpper(c);
                return c;
            }
            
            // Handle number keys
            if (key >= Keys.D0 && key <= Keys.D9 && !shift)
            {
                return (char)('0' + (key - Keys.D0));
            }
            
            // Handle numpad
            if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            {
                return (char)('0' + (key - Keys.NumPad0));
            }
            
            // Handle symbols with shift
            if (shift)
            {
                switch (key)
                {
                    case Keys.D1: return '!';
                    case Keys.D2: return '@';
                    case Keys.D3: return '#';
                    case Keys.D4: return '$';
                    case Keys.D5: return '%';
                    case Keys.D6: return '^';
                    case Keys.D7: return '&';
                    case Keys.D8: return '*';
                    case Keys.D9: return '(';
                    case Keys.D0: return ')';
                    case Keys.OemMinus: return '_';
                    case Keys.OemPlus: return '+';
                    case Keys.OemOpenBrackets: return '{';
                    case Keys.OemCloseBrackets: return '}';
                    case Keys.OemPipe: return '|';
                    case Keys.OemSemicolon: return ':';
                    case Keys.OemQuotes: return '"';
                    case Keys.OemComma: return '<';
                    case Keys.OemPeriod: return '>';
                    case Keys.OemQuestion: return '?';
                }
            }
            else
            {
                // Handle symbols without shift
                switch (key)
                {
                    case Keys.Space: return ' ';
                    case Keys.OemMinus: return '-';
                    case Keys.OemPlus: return '=';
                    case Keys.OemOpenBrackets: return '[';
                    case Keys.OemCloseBrackets: return ']';
                    case Keys.OemPipe: return '\\';
                    case Keys.OemSemicolon: return ';';
                    case Keys.OemQuotes: return '\'';
                    case Keys.OemComma: return ',';
                    case Keys.OemPeriod: return '.';
                    case Keys.OemQuestion: return '/';
                }
            }
            
            return null;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_font == null)
            {
                _font = UIManager.DefaultFont;
            }

            // Draw background
            Color bgColor = _isFocused ? _focusedColor : _backgroundColor;
            DrawRoundedRectangle(spriteBatch, Bounds, bgColor, _cornerRadius);
            
            // Text measurements
            bool hasText = !string.IsNullOrEmpty(_text);
            string textToDraw = hasText ? _text : _placeholder;
            Color textColor = hasText ? _textColor : _placeholderColor;
            Vector2 textPosition = new Vector2(Position.X + _padding, Position.Y + (Size.Y - _font.MeasureString("M").Y) / 2);
            
            // Calculate clip rectangle for text that's too long
            Rectangle textRect = new Rectangle(
                (int)textPosition.X, 
                (int)textPosition.Y,
                (int)Size.X - (_padding * 2),
                (int)Size.Y);
                
            // Draw selection background if text is selected
            if (_isFocused && _selectionStart != -1 && _selectionStart != _cursorPosition && hasText)
            {
                int start = Math.Min(_selectionStart, _cursorPosition);
                int end = Math.Max(_selectionStart, _cursorPosition);
                
                string beforeText = _text.Substring(0, start);
                string selectedText = _text.Substring(start, end - start);
                
                Vector2 startPos = _font.MeasureString(beforeText);
                Vector2 selectionSize = _font.MeasureString(selectedText);
                
                Rectangle selectionRect = new Rectangle(
                    (int)(textPosition.X + startPos.X),
                    (int)textPosition.Y,
                    (int)selectionSize.X,
                    (int)_font.MeasureString("M").Y);
                    
                spriteBatch.Draw(UIManager.Pixel, selectionRect, _selectionColor);
            }
            
            // Draw text
            spriteBatch.DrawString(_font, textToDraw, textPosition, textColor);
            
            // Draw cursor
            if (_isFocused && _showCursor && hasText)
            {
                string textBeforeCursor = _text.Substring(0, _cursorPosition);
                Vector2 cursorPos = _font.MeasureString(textBeforeCursor);
                
                Vector2 cursorPosition = new Vector2(
                    textPosition.X + cursorPos.X,
                    textPosition.Y);
                    
                Vector2 cursorSize = new Vector2(1, _font.MeasureString("M").Y);
                
                spriteBatch.Draw(
                    UIManager.Pixel,
                    new Rectangle(
                        (int)cursorPosition.X,
                        (int)cursorPosition.Y,
                        (int)cursorSize.X,
                        (int)cursorSize.Y),
                    _cursorColor);
            }
            else if (_isFocused && _showCursor && !hasText)
            {
                // Draw cursor at start position when textbox is empty
                Vector2 cursorSize = new Vector2(1, _font.MeasureString("M").Y);
                
                spriteBatch.Draw(
                    UIManager.Pixel,
                    new Rectangle(
                        (int)textPosition.X,
                        (int)textPosition.Y,
                        (int)cursorSize.X,
                        (int)cursorSize.Y),
                    _cursorColor);
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
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value ?? "";
                    _cursorPosition = Math.Min(_cursorPosition, _text.Length);
                    _selectionStart = -1;
                    OnTextChanged?.Invoke(_text);
                }
            }
        }
        
        public string Placeholder
        {
            get => _placeholder;
            set => _placeholder = value ?? "";
        }
        
        public int MaxLength
        {
            get => _maxLength;
            set => _maxLength = Math.Max(1, value);
        }
        
        public bool IsFocused
        {
            get => _isFocused;
            set => _isFocused = value;
        }
        
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }
        
        public Color FocusedColor
        {
            get => _focusedColor;
            set => _focusedColor = value;
        }
        
        public Color TextColor
        {
            get => _textColor;
            set => _textColor = value;
        }
        
        public Color PlaceholderColor
        {
            get => _placeholderColor;
            set => _placeholderColor = value;
        }
               
    }
}