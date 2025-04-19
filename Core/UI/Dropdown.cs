using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Potato.Core.UI
{
    public class Dropdown<T> : UIElement
    {
        private string _displayText;
        private SpriteFont _font;
        private List<T> _items;
        private List<string> _displayItems;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private bool _isHovered;
        private bool _isOpen;
        private int _selectedIndex = -1;
        private Button _dropdownButton;
        
        // Appearance
        private Color _dropdownColor = new Color(80, 80, 80, 230);
        private Color _itemNormalColor = new Color(60, 60, 60, 230);
        private Color _itemHoverColor = new Color(100, 100, 100, 230);
        private Color _textColor = Color.White;
        private float _cornerRadius = 5f;
        private int _maxVisibleItems = 5;
        private int _itemHeight = 30;
        private int _scrollOffset = 0;

        public event Action<T, int> OnSelectionChanged;

        public Dropdown(Vector2 position, Vector2 size, List<T> items) : base(position, size)
        {
            _items = items ?? new List<T>();
            _displayItems = new List<string>();
            _font = UIManager.Instance.GetDefaultFont();

            foreach (var item in _items)
            {
                _displayItems.Add(item.ToString());
            }

            _displayText = _items.Count > 0 && _selectedIndex >= 0 ? 
                _displayItems[_selectedIndex] : "Select...";

            // Create dropdown button
            _dropdownButton = new Button(position, size, _displayText, _font);
            _dropdownButton.OnClick += ToggleDropdown;
        }

        public Dropdown(Vector2 position, Vector2 size, List<T> items, List<string> displayNames) : this(position, size, items)
        {
            if (displayNames != null && displayNames.Count == items.Count)
            {
                _displayItems = displayNames;
            }
            UpdateDisplayText();
        }

        public override void Update(GameTime gameTime)
        {
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();

            // Update dropdown button
            _dropdownButton.Position = Position;
            _dropdownButton.Size = Size;
            _dropdownButton.Text = _displayText;
            _dropdownButton.Update(gameTime);

            if (_isOpen)
            {
                var mousePos = new Point(_currentMouseState.X, _currentMouseState.Y);
                
                // Calculate the total dropdown height
                int visibleItems = Math.Min(_items.Count, _maxVisibleItems);
                int dropdownHeight = visibleItems * _itemHeight;
                
                // Create a rectangle for the dropdown area
                Rectangle dropdownArea = new Rectangle(
                    (int)Position.X, (int)Position.Y + (int)Size.Y,
                    (int)Size.X, dropdownHeight);

                // Check if mouse is in dropdown area
                if (dropdownArea.Contains(mousePos))
                {
                    // Calculate which item is being hovered
                    int relativeY = mousePos.Y - dropdownArea.Y;
                    int hoveredIndex = _scrollOffset + relativeY / _itemHeight;
                    
                    if (hoveredIndex >= 0 && hoveredIndex < _items.Count)
                    {
                        _isHovered = true;
                        
                        // Check for click
                        if (_currentMouseState.LeftButton == ButtonState.Released &&
                            _previousMouseState.LeftButton == ButtonState.Pressed)
                        {
                            SelectItem(hoveredIndex);
                            _isOpen = false;
                        }
                    }
                    else
                    {
                        _isHovered = false;
                    }
                }
                else
                {
                    _isHovered = false;
                    
                    // Close dropdown if clicked outside
                    if (_currentMouseState.LeftButton == ButtonState.Released &&
                        _previousMouseState.LeftButton == ButtonState.Pressed)
                    {
                        _isOpen = false;
                    }
                }
                
                // Check for scrolling
                int scrollValue = _previousMouseState.ScrollWheelValue - _currentMouseState.ScrollWheelValue;
                if (scrollValue != 0 && dropdownArea.Contains(mousePos))
                {
                    // Scroll down (positive) or up (negative)
                    if (scrollValue > 0 && _scrollOffset < _items.Count - _maxVisibleItems)
                        _scrollOffset++;
                    else if (scrollValue < 0 && _scrollOffset > 0)
                        _scrollOffset--;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the dropdown button
            _dropdownButton.Draw(spriteBatch);

            if (_isOpen && _items.Count > 0)
            {
                // Calculate visible items
                int visibleItems = Math.Min(_items.Count - _scrollOffset, _maxVisibleItems);
                
                // Draw dropdown box
                Rectangle dropdownBox = new Rectangle(
                    (int)Position.X,
                    (int)Position.Y + (int)Size.Y,
                    (int)Size.X,
                    visibleItems * _itemHeight);
                
                DrawRoundedRectangle(spriteBatch, dropdownBox, _dropdownColor, _cornerRadius);

                // Draw items
                for (int i = 0; i < visibleItems; i++)
                {
                    int itemIndex = i + _scrollOffset;
                    
                    // Item area
                    Rectangle itemArea = new Rectangle(
                        (int)Position.X,
                        (int)Position.Y + (int)Size.Y + i * _itemHeight,
                        (int)Size.X,
                        _itemHeight);
                    
                    // Determine item color (selected, hovered, or normal)
                    Color itemColor = itemIndex == _selectedIndex ? _itemHoverColor : _itemNormalColor;
                    
                    // Check if this specific item is being hovered
                    Point mousePos = new Point(_currentMouseState.X, _currentMouseState.Y);
                    if (itemArea.Contains(mousePos))
                    {
                        itemColor = _itemHoverColor;
                    }
                    
                    // Draw item background
                    spriteBatch.Draw(UIManager.Instance.Pixel, itemArea, itemColor);
                    
                    // Draw item text
                    if (_font != null)
                    {
                        string itemText = _displayItems[itemIndex];
                        Vector2 textSize = _font.MeasureString(itemText);
                        Vector2 textPosition = new Vector2(
                            itemArea.X + 10, // Text padding
                            itemArea.Y + (itemArea.Height - textSize.Y) / 2 // Center vertically
                        );
                        
                        spriteBatch.DrawString(_font, itemText, textPosition, _textColor);
                    }
                }
                
                // Optionally draw scroll indicators
                if (_items.Count > _maxVisibleItems)
                {
                    // Draw scroll up indicator if not at top
                    if (_scrollOffset > 0)
                    {
                        DrawScrollIndicator(spriteBatch, true);
                    }
                    
                    // Draw scroll down indicator if not at bottom
                    if (_scrollOffset < _items.Count - _maxVisibleItems)
                    {
                        DrawScrollIndicator(spriteBatch, false);
                    }
                }
            }
        }

        private void DrawScrollIndicator(SpriteBatch spriteBatch, bool isUpIndicator)
        {
            // Position of the indicator
            Rectangle indicatorRect = new Rectangle(
                (int)Position.X + (int)Size.X - 20, // Right side
                isUpIndicator ? 
                    (int)Position.Y + (int)Size.Y + 5 : // Top
                    (int)Position.Y + (int)Size.Y + (_maxVisibleItems * _itemHeight) - 15, // Bottom
                15, // Width
                10  // Height
            );
            
            // Draw triangle indicator
            // (In a real implementation, you might want to use an actual triangle texture)
            spriteBatch.Draw(UIManager.Instance.Pixel, indicatorRect, Color.LightGray);
        }
        
        private void DrawRoundedRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, float radius)
        {
            // Implementation similar to Button's DrawRoundedRectangle
            if (radius <= 0)
            {
                spriteBatch.Draw(UIManager.Instance.Pixel, rectangle, color);
                return;
            }

            radius = Math.Min(radius, Math.Min(rectangle.Width, rectangle.Height) / 2);

            Rectangle centerRect = new Rectangle(
                rectangle.X + (int)radius,
                rectangle.Y + (int)radius,
                rectangle.Width - (int)(radius * 2),
                rectangle.Height - (int)(radius * 2)
            );
            spriteBatch.Draw(UIManager.Instance.Pixel, centerRect, color);

            Rectangle topRect = new Rectangle(
                rectangle.X + (int)radius,
                rectangle.Y,
                rectangle.Width - (int)(radius * 2),
                (int)radius
            );
            spriteBatch.Draw(UIManager.Instance.Pixel, topRect, color);

            Rectangle bottomRect = new Rectangle(
                rectangle.X + (int)radius,
                rectangle.Y + rectangle.Height - (int)radius,
                rectangle.Width - (int)(radius * 2),
                (int)radius
            );
            spriteBatch.Draw(UIManager.Instance.Pixel, bottomRect, color);

            Rectangle leftRect = new Rectangle(
                rectangle.X,
                rectangle.Y + (int)radius,
                (int)radius,
                rectangle.Height - (int)(radius * 2)
            );
            spriteBatch.Draw(UIManager.Instance.Pixel, leftRect, color);

            Rectangle rightRect = new Rectangle(
                rectangle.X + rectangle.Width - (int)radius,
                rectangle.Y + (int)radius,
                (int)radius,
                rectangle.Height - (int)(radius * 2)
            );
            spriteBatch.Draw(UIManager.Instance.Pixel, rightRect, color);
        }

        private void ToggleDropdown(UIElement _)
        {
            _isOpen = !_isOpen;
            _scrollOffset = 0; // Reset scroll position
        }

        private void SelectItem(int index)
        {
            if (index >= 0 && index < _items.Count)
            {
                _selectedIndex = index;
                UpdateDisplayText();
                OnSelectionChanged?.Invoke(_items[index], index);
            }
        }

        private void UpdateDisplayText()
        {
            _displayText = _selectedIndex >= 0 && _selectedIndex < _displayItems.Count ? 
                _displayItems[_selectedIndex] : "Select...";
        }

        // Public properties
        public T SelectedItem => _selectedIndex >= 0 && _selectedIndex < _items.Count ? 
            _items[_selectedIndex] : default;
            
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value >= -1 && value < _items.Count)
                {
                    _selectedIndex = value;
                    UpdateDisplayText();
                }
            }
        }
        
        public List<T> Items
        {
            get => _items;
            set
            {
                _items = value ?? new List<T>();
                _displayItems.Clear();
                foreach (var item in _items)
                {
                    _displayItems.Add(item.ToString());
                }
                
                // Reset selection if necessary
                if (_selectedIndex >= _items.Count)
                {
                    _selectedIndex = _items.Count > 0 ? 0 : -1;
                }
                
                UpdateDisplayText();
            }
        }
        
        public int MaxVisibleItems
        {
            get => _maxVisibleItems;
            set => _maxVisibleItems = Math.Max(1, value);
        }
        
        public int ItemHeight
        {
            get => _itemHeight;
            set => _itemHeight = Math.Max(20, value);
        }
        
        public Color DropdownColor
        {
            get => _dropdownColor;
            set => _dropdownColor = value;
        }
        
        public Color ItemNormalColor
        {
            get => _itemNormalColor;
            set => _itemNormalColor = value;
        }
        
        public Color ItemHoverColor
        {
            get => _itemHoverColor;
            set => _itemHoverColor = value;
        }
        
        public new float CornerRadius
        {
            get => _cornerRadius;
            set => _cornerRadius = value;
        }
    }
}