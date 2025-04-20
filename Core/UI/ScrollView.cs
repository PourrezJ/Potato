using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Potato.Core.UI
{
    public class ScrollView : UIElement
    {
        protected new List<UIElement> _children = new List<UIElement>();
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;
        private bool _isScrolling = false;
        private bool _isDragging = false;
        private Vector2 _dragStartPosition;
        private Vector2 _scrollPosition = Vector2.Zero;
        private Vector2 _contentSize = Vector2.Zero;
        
        // Appearance settings
        private Color _backgroundColor = new Color(40, 40, 40, 200);
        private Color _scrollBarColor = new Color(80, 80, 80, 180);
        private Color _scrollBarHoverColor = new Color(100, 100, 100, 200);
        private Color _scrollHandleColor = new Color(160, 160, 160, 200);
        private Color _scrollHandleDragColor = new Color(200, 200, 200, 220);
        private float _cornerRadius = 5f;
        private int _scrollBarSize = 15;
        private int _scrollBarPadding = 2;
        private int _contentPadding = 10;
        
        // Scroll settings
        private bool _showHorizontalScrollBar = true;
        private bool _showVerticalScrollBar = true;
        private float _scrollSpeed = 20f;
        private Rectangle _viewportRect;
        private Rectangle _horizontalScrollBarRect;
        private Rectangle _verticalScrollBarRect;
        private Rectangle _horizontalHandleRect;
        private Rectangle _verticalHandleRect;
        
        public ScrollView(Vector2 position, Vector2 size) 
            : base(position, size)
        {
            UpdateViewportRect();
        }

        public override void Update(GameTime gameTime)
        {
            // Save mouse state for drag & scroll detection
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
            
            Point mousePos = new Point(_currentMouseState.X, _currentMouseState.Y);
            bool isMouseOverContent = _viewportRect.Contains(mousePos);
            
            // Calculate content bounds
            CalculateContentSize();
            
            // Handle scrolling with mouse wheel
            if (isMouseOverContent)
            {
                int scrollDelta = _previousMouseState.ScrollWheelValue - _currentMouseState.ScrollWheelValue;
                if (scrollDelta != 0)
                {
                    // Scroll vertically with mouse wheel
                    if (_showVerticalScrollBar && HasVerticalOverflow())
                    {
                        _scrollPosition.Y += (scrollDelta / 120f) * _scrollSpeed;
                        _scrollPosition.Y = MathHelper.Clamp(_scrollPosition.Y, 0, Math.Max(0, _contentSize.Y - _viewportRect.Height));
                    }
                    // If no vertical scroll or no vertical overflow, scroll horizontally
                    else if (_showHorizontalScrollBar && HasHorizontalOverflow())
                    {
                        _scrollPosition.X += (scrollDelta / 120f) * _scrollSpeed;
                        _scrollPosition.X = MathHelper.Clamp(_scrollPosition.X, 0, Math.Max(0, _contentSize.X - _viewportRect.Width));
                    }
                }
            }
            
            // Handle scrollbar dragging
            UpdateScrollBars();
            HandleScrollBarInteraction(mousePos);
            
            // Update all child elements with adjusted positions
            foreach (var child in _children)
            {
                // Store original position
                Vector2 originalPosition = child.Position;
                
                // Temporarily adjust the child's position for scrolling
                child.Position = new Vector2(
                    originalPosition.X - _scrollPosition.X + Position.X + _contentPadding,
                    originalPosition.Y - _scrollPosition.Y + Position.Y + _contentPadding);
                
                // Only update if visible in the viewport
                if (IsVisible && IsChildVisible(child))
                {
                    child.Update(gameTime);
                }
                
                // Restore original position
                child.Position = originalPosition;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
                return;
                
            // Draw background
            DrawRoundedRectangle(spriteBatch, Bounds, _backgroundColor, _cornerRadius);
            
            // Set up clipping to viewport area
            Rectangle originalScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;
            spriteBatch.End();
            
            // Start a new SpriteBatch with scissor test enabled
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                null);
                
            // Set the scissor rectangle to the viewport area
            spriteBatch.GraphicsDevice.ScissorRectangle = _viewportRect;
            
            // Draw all child elements with adjusted positions
            foreach (var child in _children)
            {
                // Store original position
                Vector2 originalPosition = child.Position;
                
                // Temporarily adjust the child's position for scrolling
                child.Position = new Vector2(
                    originalPosition.X - _scrollPosition.X + Position.X + _contentPadding,
                    originalPosition.Y - _scrollPosition.Y + Position.Y + _contentPadding);
                
                // Only draw if visible in the viewport
                if (IsChildVisible(child))
                {
                    child.Draw(spriteBatch);
                }
                
                // Restore original position
                child.Position = originalPosition;
            }
            
            spriteBatch.End();
            
            // Restore the original SpriteBatch and scissor rectangle
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            spriteBatch.GraphicsDevice.ScissorRectangle = originalScissorRect;
            
            // Draw scroll bars
            DrawScrollBars(spriteBatch);
        }
        
        // Check if scrolling is needed
        private bool HasHorizontalOverflow()
        {
            return _contentSize.X > _viewportRect.Width;
        }
        
        private bool HasVerticalOverflow()
        {
            return _contentSize.Y > _viewportRect.Height;
        }
        
        // Check if a child element is visible in the viewport
        private bool IsChildVisible(UIElement child)
        {
            // Calculate the child's visible bounds
            Rectangle childBounds = new Rectangle(
                (int)(child.Position.X - _scrollPosition.X + Position.X + _contentPadding),
                (int)(child.Position.Y - _scrollPosition.Y + Position.Y + _contentPadding),
                (int)child.Size.X,
                (int)child.Size.Y);
                
            // Check if it intersects with the viewport
            return _viewportRect.Intersects(childBounds);
        }
        
        // Calculate the total size needed for content
        private void CalculateContentSize()
        {
            // Start with zero size
            _contentSize = Vector2.Zero;
            
            // Find the maximum width and height of all child elements
            foreach (var child in _children)
            {
                // Calculate right and bottom edges
                float right = child.Position.X + child.Size.X;
                float bottom = child.Position.Y + child.Size.Y;
                
                // Update content size if necessary
                _contentSize.X = Math.Max(_contentSize.X, right);
                _contentSize.Y = Math.Max(_contentSize.Y, bottom);
            }
            
            // Add padding
            _contentSize.X += _contentPadding * 2;
            _contentSize.Y += _contentPadding * 2;
        }
        
        // Update viewport and scrollbar rectangles
        private void UpdateViewportRect()
        {
            // Calculate scrollbar visibility
            bool needHorizontalScrollBar = _showHorizontalScrollBar && HasHorizontalOverflow();
            bool needVerticalScrollBar = _showVerticalScrollBar && HasVerticalOverflow();
            
            // Calculate the viewport rectangle, accounting for scroll bars
            _viewportRect = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                (int)Size.X - (needVerticalScrollBar ? _scrollBarSize : 0),
                (int)Size.Y - (needHorizontalScrollBar ? _scrollBarSize : 0));
            
            // Calculate scrollbar rectangles
            if (needHorizontalScrollBar)
            {
                _horizontalScrollBarRect = new Rectangle(
                    (int)Position.X,
                    (int)(Position.Y + Size.Y - _scrollBarSize),
                    (int)Size.X - (needVerticalScrollBar ? _scrollBarSize : 0),
                    _scrollBarSize);
            }
            
            if (needVerticalScrollBar)
            {
                _verticalScrollBarRect = new Rectangle(
                    (int)(Position.X + Size.X - _scrollBarSize),
                    (int)Position.Y,
                    _scrollBarSize,
                    (int)Size.Y - (needHorizontalScrollBar ? _scrollBarSize : 0));
            }
            
            // Update scroll handle rectangles
            UpdateScrollHandles();
        }
        
        // Update scroll handle positions and sizes
        private void UpdateScrollHandles()
        {
            // Calculate horizontal handle
            if (_showHorizontalScrollBar && HasHorizontalOverflow())
            {
                float ratio = _viewportRect.Width / _contentSize.X;
                int handleWidth = Math.Max((int)(_horizontalScrollBarRect.Width * ratio), 30);
                int handleX = (int)(_horizontalScrollBarRect.X + (_scrollPosition.X / _contentSize.X) * (_horizontalScrollBarRect.Width - handleWidth));
                
                _horizontalHandleRect = new Rectangle(
                    handleX,
                    _horizontalScrollBarRect.Y + _scrollBarPadding,
                    handleWidth,
                    _horizontalScrollBarRect.Height - (_scrollBarPadding * 2));
            }
            
            // Calculate vertical handle
            if (_showVerticalScrollBar && HasVerticalOverflow())
            {
                float ratio = _viewportRect.Height / _contentSize.Y;
                int handleHeight = Math.Max((int)(_verticalScrollBarRect.Height * ratio), 30);
                int handleY = (int)(_verticalScrollBarRect.Y + (_scrollPosition.Y / _contentSize.Y) * (_verticalScrollBarRect.Height - handleHeight));
                
                _verticalHandleRect = new Rectangle(
                    _verticalScrollBarRect.X + _scrollBarPadding,
                    handleY,
                    _verticalScrollBarRect.Width - (_scrollBarPadding * 2),
                    handleHeight);
            }
        }
        
        // Handle scrollbar interaction (hover, click, drag)
        private void HandleScrollBarInteraction(Point mousePos)
        {
            bool needHorizontalScrollBar = _showHorizontalScrollBar && HasHorizontalOverflow();
            bool needVerticalScrollBar = _showVerticalScrollBar && HasVerticalOverflow();
            
            // Handle horizontal scrollbar
            if (needHorizontalScrollBar)
            {
                bool isOverHorizontalHandle = _horizontalHandleRect.Contains(mousePos);
                bool wasHorizontalHandlePressed = _isScrolling && _isDragging && _dragStartPosition.Y == 0;
                
                // Start dragging horizontal handle
                if (isOverHorizontalHandle && _currentMouseState.LeftButton == ButtonState.Pressed &&
                    _previousMouseState.LeftButton == ButtonState.Released)
                {
                    _isScrolling = true;
                    _isDragging = true;
                    _dragStartPosition = new Vector2(mousePos.X - _horizontalHandleRect.X, 0);
                }
                
                // Continue dragging horizontal handle
                if (wasHorizontalHandlePressed && _currentMouseState.LeftButton == ButtonState.Pressed)
                {
                    float trackWidth = _horizontalScrollBarRect.Width - _horizontalHandleRect.Width;
                    float newHandleX = mousePos.X - _dragStartPosition.X;
                    float percentage = MathHelper.Clamp((newHandleX - _horizontalScrollBarRect.X) / trackWidth, 0, 1);
                    
                    _scrollPosition.X = percentage * (_contentSize.X - _viewportRect.Width);
                }
                
                // End dragging
                if (_isDragging && _currentMouseState.LeftButton == ButtonState.Released)
                {
                    _isScrolling = false;
                    _isDragging = false;
                }
                
                // Click in scrollbar track (not on handle)
                if (_horizontalScrollBarRect.Contains(mousePos) && !isOverHorizontalHandle &&
                    _currentMouseState.LeftButton == ButtonState.Pressed && 
                    _previousMouseState.LeftButton == ButtonState.Released)
                {
                    // Jump to clicked position
                    if (mousePos.X < _horizontalHandleRect.X)
                    {
                        _scrollPosition.X = Math.Max(0, _scrollPosition.X - _viewportRect.Width / 2);
                    }
                    else
                    {
                        _scrollPosition.X = Math.Min(_contentSize.X - _viewportRect.Width, 
                            _scrollPosition.X + _viewportRect.Width / 2);
                    }
                }
            }
            
            // Handle vertical scrollbar
            if (needVerticalScrollBar)
            {
                bool isOverVerticalHandle = _verticalHandleRect.Contains(mousePos);
                bool wasVerticalHandlePressed = _isScrolling && _isDragging && _dragStartPosition.X == 0;
                
                // Start dragging vertical handle
                if (isOverVerticalHandle && _currentMouseState.LeftButton == ButtonState.Pressed &&
                    _previousMouseState.LeftButton == ButtonState.Released)
                {
                    _isScrolling = true;
                    _isDragging = true;
                    _dragStartPosition = new Vector2(0, mousePos.Y - _verticalHandleRect.Y);
                }
                
                // Continue dragging vertical handle
                if (wasVerticalHandlePressed && _currentMouseState.LeftButton == ButtonState.Pressed)
                {
                    float trackHeight = _verticalScrollBarRect.Height - _verticalHandleRect.Height;
                    float newHandleY = mousePos.Y - _dragStartPosition.Y;
                    float percentage = MathHelper.Clamp((newHandleY - _verticalScrollBarRect.Y) / trackHeight, 0, 1);
                    
                    _scrollPosition.Y = percentage * (_contentSize.Y - _viewportRect.Height);
                }
                
                // End dragging
                if (_isDragging && _currentMouseState.LeftButton == ButtonState.Released)
                {
                    _isScrolling = false;
                    _isDragging = false;
                }
                
                // Click in scrollbar track (not on handle)
                if (_verticalScrollBarRect.Contains(mousePos) && !isOverVerticalHandle &&
                    _currentMouseState.LeftButton == ButtonState.Pressed && 
                    _previousMouseState.LeftButton == ButtonState.Released)
                {
                    // Jump to clicked position
                    if (mousePos.Y < _verticalHandleRect.Y)
                    {
                        _scrollPosition.Y = Math.Max(0, _scrollPosition.Y - _viewportRect.Height / 2);
                    }
                    else
                    {
                        _scrollPosition.Y = Math.Min(_contentSize.Y - _viewportRect.Height, 
                            _scrollPosition.Y + _viewportRect.Height / 2);
                    }
                }
            }
        }
        
        // Draw scrollbars and handles
        private void DrawScrollBars(SpriteBatch spriteBatch)
        {
            bool needHorizontalScrollBar = _showHorizontalScrollBar && HasHorizontalOverflow();
            bool needVerticalScrollBar = _showVerticalScrollBar && HasVerticalOverflow();
            
            // Draw horizontal scrollbar if needed
            if (needHorizontalScrollBar)
            {
                // Draw scrollbar track
                spriteBatch.Draw(UIManager.Pixel, _horizontalScrollBarRect, _scrollBarColor);
                
                // Draw handle
                Color handleColor = _isDragging && _dragStartPosition.Y == 0 ?
                    _scrollHandleDragColor : _scrollHandleColor;
                
                spriteBatch.Draw(UIManager.Pixel, _horizontalHandleRect, handleColor);
            }
            
            // Draw vertical scrollbar if needed
            if (needVerticalScrollBar)
            {
                // Draw scrollbar track
                spriteBatch.Draw(UIManager.Pixel, _verticalScrollBarRect, _scrollBarColor);
                
                // Draw handle
                Color handleColor = _isDragging && _dragStartPosition.X == 0 ?
                    _scrollHandleDragColor : _scrollHandleColor;
                
                spriteBatch.Draw(UIManager.Pixel, _verticalHandleRect, handleColor);
            }
            
            // Draw corner square if both scrollbars are visible
            if (needHorizontalScrollBar && needVerticalScrollBar)
            {
                Rectangle cornerRect = new Rectangle(
                    (int)(Position.X + Size.X - _scrollBarSize),
                    (int)(Position.Y + Size.Y - _scrollBarSize),
                    _scrollBarSize,
                    _scrollBarSize);
                    
                spriteBatch.Draw(UIManager.Pixel, cornerRect, _scrollBarColor);
            }
        }
        
        // Draw a rounded rectangle (similar to other UI components)
        private void DrawRoundedRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, float radius)
        {
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
        
        // Add a child element
        public new virtual void AddChild(UIElement element)
        {
            _children.Add(element);
            UpdateViewportRect();
        }
        
        // Remove a child element
        public new virtual void RemoveChild(UIElement element)
        {
            _children.Remove(element);
            UpdateViewportRect();
        }
        
        // Clear all child elements
        public void ClearChildren()
        {
            _children.Clear();
            _scrollPosition = Vector2.Zero;
            UpdateViewportRect();
        }
        
        // Update scrollbars when window resizes or content changes
        private void UpdateScrollBars()
        {
            // Ensure scroll position is valid
            if (HasHorizontalOverflow())
            {
                _scrollPosition.X = MathHelper.Clamp(_scrollPosition.X, 0, _contentSize.X - _viewportRect.Width);
            }
            else
            {
                _scrollPosition.X = 0;
            }
            
            if (HasVerticalOverflow())
            {
                _scrollPosition.Y = MathHelper.Clamp(_scrollPosition.Y, 0, _contentSize.Y - _viewportRect.Height);
            }
            else
            {
                _scrollPosition.Y = 0;
            }
            
            UpdateViewportRect();
        }
        
        // Public properties
        public Vector2 ScrollPosition
        {
            get => _scrollPosition;
            set
            {
                _scrollPosition = value;
                UpdateScrollBars();
            }
        }
        
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }
        
        public Color ScrollBarColor
        {
            get => _scrollBarColor;
            set => _scrollBarColor = value;
        }
        
        public Color ScrollHandleColor
        {
            get => _scrollHandleColor;
            set => _scrollHandleColor = value;
        }
        
        public new float CornerRadius
        {
            get { return _cornerRadius; }
            set { _cornerRadius = value; }
        }
        
        public int ScrollBarSize
        {
            get => _scrollBarSize;
            set
            {
                _scrollBarSize = Math.Max(10, value);
                UpdateViewportRect();
            }
        }
        
        public int ContentPadding
        {
            get => _contentPadding;
            set
            {
                _contentPadding = value;
                UpdateViewportRect();
            }
        }
        
        public bool ShowHorizontalScrollBar
        {
            get => _showHorizontalScrollBar;
            set
            {
                _showHorizontalScrollBar = value;
                UpdateViewportRect();
            }
        }
        
        public bool ShowVerticalScrollBar
        {
            get => _showVerticalScrollBar;
            set
            {
                _showVerticalScrollBar = value;
                UpdateViewportRect();
            }
        }
        
        public float ScrollSpeed
        {
            get => _scrollSpeed;
            set => _scrollSpeed = Math.Max(1, value);
        }
        
        public IReadOnlyList<UIElement> Children => _children;
    }
}