using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core;
using Potato.Core.UI;
using Potato.Core.Logging;
using Potato.Core.Items;
using Potato.Core.Entities;
using System;
using System.Collections.Generic;

namespace Potato.Engine
{
    public class ShopScreen : Singleton<ShopScreen>
    {
        private List<ShopItem> _shopItems;
        private int _selectedItemIndex;
        // private KeyboardState _previousKeyboardState;
        private bool _isShopClosed;
        private SpriteFont _font;
        
        private Player _player;
        
        // UI Components
        private Panel _mainPanel;
        private Panel _itemDetailsPanel;
        // private Label _titleLabel;
        private Label _goldLabel;
        private Label _itemDescLabel;
        private Button _closeButton;
        private List<Button> _itemButtons;
        private Button _purchaseButton;
        
        // Animation properties
        private float _animationScale = 0f;
        private bool _isAnimating = true;
        private Color _purchaseFlashColor = Color.Transparent;
        private float _purchaseFlashTimer = 0f;
        
        // Category tabs
        private Button _weaponsTabButton;
        private Button _upgradesTabButton;
        private Button _consumablesTabButton;
        private string _currentCategory = "All";
        
        private bool _initialized = false;
        
        public ShopScreen()
        {
            // On ne doit pas accéder à GameManager.Instance ici car il n'est peut-être pas encore initialisé
            _shopItems = new List<ShopItem>();
            _selectedItemIndex = 0;
            _isShopClosed = true; // Commencer avec le shop fermé
            _itemButtons = new List<Button>();
        }
        
        public override void Awake()
        {
            base.Awake();
            
            _player = Player.Local;            
        }
                      
        private void InitializeUI()
        {
            if (_initialized)
                return;

            _initialized = true;

            // Get screen dimensions
            int screenWidth = _game.GraphicsDevice.Viewport.Width;
            int screenHeight = _game.GraphicsDevice.Viewport.Height;
            
            // Main shop panel (semi-transparent) with rounded corners
            _mainPanel = new Panel(
                new Vector2(screenWidth / 2 - 400, screenHeight / 2 - 300),
                new Vector2(800, 600),
                new Color(30, 30, 40, 240),
                $"SHOP - WAVE {GameManager.Instance.Wave} COMPLETED");
            _mainPanel.TitleColor = Color.Gold;
            _mainPanel.CornerRadius = 15f; // Rounded corners
            _mainPanel.BorderColor = new Color(180, 160, 60, 220);
            _mainPanel.HasBorder = true;
            _mainPanel.Scale = 0f; // Start with zero scale for animation
            
            // S'assurer que le panel du shop a une référence à la police
            _mainPanel.Font = _font;
            Logger.Instance.Debug("[ShopScreen] _mainPanel créé avec une taille de 800x600", LogCategory.UI);
            
            // Title and gold display
            _goldLabel = new Label(new Vector2(650, 50),"");
            _goldLabel.TextColor = Color.Gold;
            _goldLabel.Scale = 1.2f;
            _goldLabel.Font = _font; // IMPORTANT: Assurer que le label a la police
            _mainPanel.AddChild(_goldLabel);
            
            // Category tabs
            _weaponsTabButton = new Button(
                new Vector2(140, 80),
                new Vector2(120, 30),
                "WEAPONS");
            _weaponsTabButton.BackgroundColor = new Color(60, 60, 120, 200);
            _weaponsTabButton.HoverColor = new Color(80, 80, 160, 220);
            _weaponsTabButton.CornerRadius = 8f;
            _weaponsTabButton.TextColor = Color.White;
            _weaponsTabButton.TextScale = 0.9f;
            _weaponsTabButton.Font = _font; // IMPORTANT: Assurer que le bouton a la police
            // Utiliser propriété OnClickAction au lieu de l'événement
            _weaponsTabButton.OnClickAction = () => FilterItemsByCategory("Weapon");
            _mainPanel.AddChild(_weaponsTabButton);
            
            _upgradesTabButton = new Button(
                new Vector2(270, 80),
                new Vector2(120, 30),
                "UPGRADES");
            _upgradesTabButton.BackgroundColor = new Color(60, 120, 60, 200);
            _upgradesTabButton.HoverColor = new Color(80, 160, 80, 220);
            _upgradesTabButton.CornerRadius = 8f;
            _upgradesTabButton.TextColor = Color.White;
            _upgradesTabButton.TextScale = 0.9f;
            _upgradesTabButton.Font = _font; // IMPORTANT: Assurer que le bouton a la police
            _upgradesTabButton.OnClickAction = () => FilterItemsByCategory("StatUpgrade");
            _mainPanel.AddChild(_upgradesTabButton);
            
            _consumablesTabButton = new Button(
                new Vector2(400, 80),
                new Vector2(120, 30),
                "POTIONS");
            _consumablesTabButton.BackgroundColor = new Color(120, 60, 60, 200);
            _consumablesTabButton.HoverColor = new Color(160, 80, 80, 220);
            _consumablesTabButton.CornerRadius = 8f;
            _consumablesTabButton.TextColor = Color.White;
            _consumablesTabButton.TextScale = 0.9f;
            _consumablesTabButton.Font = _font; // IMPORTANT: Assurer que le bouton a la police
            _consumablesTabButton.OnClickAction = () => FilterItemsByCategory("Heal");
            _mainPanel.AddChild(_consumablesTabButton);
            
            // Item details panel (right side) with rounded corners
            _itemDetailsPanel = new Panel(
                new Vector2(460, 120),
                new Vector2(300, 380),
                new Color(40, 40, 60, 200),
                "Item Details");
            _itemDetailsPanel.CornerRadius = 10f;
            _itemDetailsPanel.HasBorder = true;
            _itemDetailsPanel.BorderColor = new Color(100, 100, 180, 180);
            _itemDetailsPanel.Font = _font; // IMPORTANT: Assurer que le panel a la police
            _mainPanel.AddChild(_itemDetailsPanel);
            
            // Item description label
            _itemDescLabel = new Label(
                new Vector2(150, 100),
                "Select an item to see details");
            _itemDescLabel.TextColor = Color.LightCyan;
            _itemDescLabel.Font = _font; // IMPORTANT: Assurer que le label a la police
            _itemDetailsPanel.AddChild(_itemDescLabel);
            
            // Purchase button with improved design
            // CORRIGÉ: Utiliser une position relative simple pour le bouton PURCHASE
            _purchaseButton = new Button(
                new Vector2(60, 300),
                new Vector2(180, 40),
                "PURCHASE");
            _purchaseButton.BackgroundColor = new Color(60, 120, 60, 220);
            _purchaseButton.HoverColor = new Color(80, 160, 80, 220);
            _purchaseButton.PressedColor = new Color(40, 100, 40, 220);
            _purchaseButton.CornerRadius = 10f;
            _purchaseButton.BorderColor = new Color(100, 200, 100, 200);
            _purchaseButton.HasBorder = true;
            _purchaseButton.TextColor = Color.White;
            _purchaseButton.TextScale = 1.0f;
            _purchaseButton.Font = _font; // IMPORTANT: Assurer que le bouton a la police
            _purchaseButton.OnClickAction = () => PurchaseSelectedItem();
            _itemDetailsPanel.AddChild(_purchaseButton);
            
            // Start Next Wave button - Remplace l'ancien bouton "Continue to Next Wave"
            _closeButton = new Button(
                new Vector2(400, 520),
                new Vector2(300, 50),
                "START NEXT WAVE");
            _closeButton.BackgroundColor = new Color(100, 60, 60, 220);
            _closeButton.HoverColor = new Color(140, 80, 80, 220);
            _closeButton.PressedColor = new Color(80, 40, 40, 220);
            _closeButton.CornerRadius = 10f;
            _closeButton.BorderColor = new Color(200, 100, 100, 200);
            _closeButton.HasBorder = true;
            _closeButton.TextScale = 1.2f;
            _closeButton.TextColor = Color.White;
            _closeButton.Font = _font; // IMPORTANT: Assurer que le bouton a la police
            _closeButton.OnClickAction = () => GameManager.Instance.StartNextWave();
            _mainPanel.AddChild(_closeButton);
            
            // Create item buttons
            CreateItemButtons();
            
            // Update the selected item display
            UpdateItemDetailsPanel();
            
            // Log pour déboguer
            Logger.Instance.Debug($"[ShopScreen] InitializeUI terminé, {_mainPanel.GetChildren().Count} éléments UI créés", LogCategory.UI);
            
            // Add the main panel to the UI manager
            UIManager.Instance.AddElement(_mainPanel);

            _initialized = true; // Indicate that the UI has been initialized

            Logger.Instance.Debug("[ShopScreen] _mainPanel ajouté à UIManager", LogCategory.UI);
        }
        
        private void GenerateShopItems()
        {
            _shopItems.Clear();
            int waveNumber = GameManager.Instance.Wave;
            Random rand = new Random();
            
            // Add stat upgrades
            _shopItems.Add(new StatUpgradeItem("Health Boost", "Increases max health by 25", 30 * waveNumber, Potato.Core.Stats.StatType.MaxHealth, 25));
            _shopItems.Add(new StatUpgradeItem("Speed Boost", "Increases movement speed by 20", 25 * waveNumber, Potato.Core.Stats.StatType.Speed, 20));
            _shopItems.Add(new StatUpgradeItem("Damage Boost", "Increases damage by 5", 35 * waveNumber, Potato.Core.Stats.StatType.Damage, 5));
            
            // Add weapons based on wave number
            if (waveNumber >= 2)
            {
                _shopItems.Add(new WeaponItem("Improved Sword", "A stronger melee weapon", 50 * waveNumber, WeaponType.Melee, 1.2f));
            }
            
            if (waveNumber >= 3)
            {
                _shopItems.Add(new WeaponItem("Enhanced Bow", "A more powerful ranged weapon", 60 * waveNumber, WeaponType.Ranged, 1.3f));
            }
        }

        private void FilterItemsByCategory(string category)
        {
            _currentCategory = category;
            
            // Remove existing item buttons
            foreach (var button in _itemButtons)
            {
                _mainPanel.RemoveChild(button);
            }
            
            // Recreate buttons with filtered items
            CreateItemButtons();
            
            // Reset selection
            _selectedItemIndex = -1;
            UpdateItemDetailsPanel();
            
            // Update button appearances
            UpdateCategoryButtonAppearance();
        }
        
        private void UpdateCategoryButtonAppearance()
        {
            // Reset all buttons
            _weaponsTabButton.BorderThickness = 0;
            _upgradesTabButton.BorderThickness = 0;
            _consumablesTabButton.BorderThickness = 0;
            
            // Highlight active button
            switch (_currentCategory)
            {
                case "Weapon":
                    _weaponsTabButton.BorderThickness = 2;
                    break;
                case "StatUpgrade":
                    _upgradesTabButton.BorderThickness = 2;
                    break;
                case "Heal":
                    _consumablesTabButton.BorderThickness = 2;
                    break;
            }
        }
        
        private void CreateItemButtons()
        {
            _itemButtons.Clear();
            int startY = 120;
            int buttonHeight = 60;
            int buttonSpacing = 10;
            int visibleCount = 0;
            
            Logger.Instance.Debug($"[ShopScreen] Création de {_shopItems.Count} boutons d'articles", LogCategory.UI);
            
            for (int i = 0; i < _shopItems.Count; i++)
            {
                ShopItem item = _shopItems[i];
                
                // Filter by category if needed
                if (_currentCategory != "All")
                {
                    if (item is WeaponItem && _currentCategory != "Weapon")
                        continue;
                    if (item is StatUpgradeItem && _currentCategory != "StatUpgrade")
                        continue;
                    if (item is HealItem && _currentCategory != "Heal")
                        continue;
                }
                
                // CORRIGÉ: Utiliser la position relative au panel
                // Les coordonnées sont maintenant relatives à la position (0,0) du panel
                // Le panel s'occupera de les repositionner correctement
                Button itemButton = new Button(
                    new Vector2(50, startY + visibleCount * (buttonHeight + buttonSpacing) - 120),
                    new Vector2(280, buttonHeight),
                    $"{item.Name} - {item.Cost} Gold");
                
                itemButton.CornerRadius = 8f;
                itemButton.HasBorder = true;
                itemButton.TextColor = Color.White;
                itemButton.TextScale = 0.9f;
                itemButton.Font = _font; // CRITIQUE: Assigner la police
                
                // Customize appearance based on item type
                if (item is WeaponItem)
                {
                    itemButton.BackgroundColor = new Color(60, 60, 130, 200);
                    itemButton.HoverColor = new Color(80, 80, 180, 220);
                    itemButton.BorderColor = new Color(100, 100, 200, 200);
                }
                else if (item is StatUpgradeItem)
                {
                    itemButton.BackgroundColor = new Color(60, 130, 60, 200);
                    itemButton.HoverColor = new Color(80, 180, 80, 220);
                    itemButton.BorderColor = new Color(100, 200, 100, 200);
                }
                else if (item is HealItem)
                {
                    itemButton.BackgroundColor = new Color(130, 60, 60, 200);
                    itemButton.HoverColor = new Color(180, 80, 80, 220);
                    itemButton.BorderColor = new Color(200, 100, 100, 200);
                }
                
                // Store the index for the onClick handler
                int index = i;
                itemButton.OnClickAction = () => {
                    Logger.Instance.Debug($"[ShopScreen] Article sélectionné: {item.Name}", LogCategory.UI);
                    _selectedItemIndex = index;
                    UpdateItemDetailsPanel();
                };
                
                // Set colors based on affordability
                if (item.Cost > _player.Gold)
                {
                    itemButton.BackgroundColor = new Color(80, 80, 80, 200);
                    itemButton.HoverColor = new Color(100, 100, 100, 220);
                    itemButton.TextColor = Color.Silver;
                    itemButton.BorderColor = new Color(150, 150, 150, 150);
                }
                
                _itemButtons.Add(itemButton);
                _mainPanel.AddChild(itemButton);
                
                // Log pour déboguer
                Logger.Instance.Debug($"[ShopScreen] Bouton créé pour {item.Name} - {item.Cost} Gold", LogCategory.UI);
                
                visibleCount++;
            }
        }
        
        private void UpdateItemDetailsPanel()
        {
            if (_selectedItemIndex >= 0 && _selectedItemIndex < _shopItems.Count)
            {
                ShopItem selectedItem = _shopItems[_selectedItemIndex];
                _itemDescLabel.Text = selectedItem.Description;
                
                // Update purchase button based on affordability
                bool canAfford = _player.Gold >= selectedItem.Cost;
                // Instead of using Enabled property, we'll just change the appearance
                // and check affordability when purchasing
                
                if (canAfford)
                {
                    _purchaseButton.BackgroundColor = new Color(60, 120, 60, 220);
                    _purchaseButton.HoverColor = new Color(80, 160, 80, 220);
                    _purchaseButton.TextColor = Color.White;
                }
                else
                {
                    _purchaseButton.BackgroundColor = new Color(100, 100, 100, 200);
                    _purchaseButton.HoverColor = new Color(100, 100, 100, 200);
                    _purchaseButton.TextColor = Color.Gray;
                }
            }
            else
            {
                _itemDescLabel.Text = "Select an item to see details";
                // Don't set Enabled property since it doesn't exist
                _purchaseButton.BackgroundColor = new Color(100, 100, 100, 200);
                _purchaseButton.HoverColor = new Color(100, 100, 100, 200);
                _purchaseButton.TextColor = Color.Gray;
            }
        }
        
        public override void Update(GameTime gameTime)
        {
            // Exit if shop is closed
            if (_isShopClosed)
                return;
            
            // Handle opening animation
            if (_isAnimating)
            {
                _animationScale += (float)gameTime.ElapsedGameTime.TotalSeconds * 4f;
                if (_animationScale >= 1f)
                {
                    _animationScale = 1f;
                    _isAnimating = false;
                }
                _mainPanel.Scale = _animationScale;
            }
            
            // Handle purchase flash effect
            if (_purchaseFlashTimer > 0)
            {
                _purchaseFlashTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                // Fade out the flash
                float alpha = _purchaseFlashTimer / 0.5f;
                _purchaseFlashColor = new Color((byte)255, (byte)255, (byte)150, (byte)(alpha * 200));
                
                if (_purchaseFlashTimer <= 0)
                {
                    _purchaseFlashColor = Color.Transparent;
                }
            }
            
            // Update UI Manager (handles all UI element updates)
            UIManager.Instance.Update(gameTime);
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_isShopClosed)
                return;

            InitializeUI();

                
            // Let the UIManager draw all elements
            
            // Draw purchase flash if active
            if (_purchaseFlashTimer > 0)
            {
                int screenWidth = _game.GraphicsDevice.Viewport.Width;
                int screenHeight = _game.GraphicsDevice.Viewport.Height;
                
                // Create 1x1 white texture for drawing rectangles
                Texture2D rect = new Texture2D(_game.GraphicsDevice, 1, 1);
                rect.SetData(new[] { Color.White });
                
                // Draw flash overlay
                spriteBatch.Draw(rect, new Rectangle(0, 0, screenWidth, screenHeight), _purchaseFlashColor);
            }
        }
        
        private void PurchaseSelectedItem()
        {
            if (_selectedItemIndex < 0 || _selectedItemIndex >= _shopItems.Count)
                return;
                
            // Extra check to ensure player can afford the item
            ShopItem selectedItem = _shopItems[_selectedItemIndex];
            if (_player.Gold < selectedItem.Cost)
                return;
            
            // Check if player has enough gold
            if (_player.Gold >= selectedItem.Cost)
            {
                // Apply the effect of the purchased item
                bool purchased = selectedItem.Purchase(_player);
                
                if (purchased)
                {
                    // Start purchase flash animation
                    _purchaseFlashTimer = 0.5f;
                    
                    // Deduct the cost from player's gold
                    _player.Gold -= selectedItem.Cost;
                    
                    // Update gold label with animation
                    _goldLabel.Text = $"Your Gold: {_player.Gold}";
                    _goldLabel.Scale = 1.5f; // Temporarily increase scale
                    
                    // Remove the item from the shop
                    _shopItems.RemoveAt(_selectedItemIndex);
                    
                    // Recreate item buttons
                    foreach (var button in _itemButtons)
                    {
                        _mainPanel.RemoveChild(button);
                    }
                    CreateItemButtons();
                    
                    // Adjust selected index if needed
                    if (_selectedItemIndex >= _shopItems.Count)
                    {
                        _selectedItemIndex = _shopItems.Count - 1;
                    }
                    
                    // Update item details panel
                    UpdateItemDetailsPanel();
                }
            }
        }
        
        public void CloseShop()
        {
            _isShopClosed = true;
            
            // Start closing animation
            _isAnimating = true;
            
            // Remove UI elements from the manager after animation
            UIManager.Instance.RemoveElement(_mainPanel);
        }
        
        public bool IsClosed()
        {
            return _isShopClosed;
        }
              
        /// <summary>
        /// Closes the shop with an animation
        /// </summary>
        /// <param name="startNextWave">If true, immediately starts the next wave after closing</param>
        public void CloseShop(bool startNextWave = false)
        {
            if (_isShopClosed)
                return;
                
            _isShopClosed = true;
            
            // Remove UI elements from the manager
            UIManager.Instance.RemoveElement(_mainPanel);
            
            Logger.Instance.Info("[ShopScreen] Shop closed", LogCategory.UI);
            
            // Start next wave if requested
            if (startNextWave && GameManager.Instance.IsReadyForNextWave)
            {
                try
                {
                    int nextWave = GameManager.Instance.Wave + 1;
                    GameManager.Instance.StartWave(nextWave);
                    Logger.Instance.Info($"[ShopScreen] Wave {nextWave} started after shop close", LogCategory.Gameplay);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"[ShopScreen] Error starting next wave: {ex.Message}", LogCategory.Gameplay);
                }
            }
        }
        
        /// <summary>
        /// Opens the shop with an animation and initializes required components
        /// </summary>
        public void OpenShop()
        {
            if (!_isShopClosed)
                return;
                
            _isShopClosed = false;
            _isAnimating = true;
            _animationScale = 0f;
            
            if (!_initialized)
            {
                // Initialize UI components if not already done
                InitializeUI();
            }

            // Reload shop items for the current wave
            GenerateShopItems();
            
            // Reset selected item
            _selectedItemIndex = -1;
            
            // Update UI with current gold
            _goldLabel.Text = $"Your Gold: {_player.Gold}";
            
            // Recreate item buttons
            CreateItemButtons();
            
            // Update detail panel
            UpdateItemDetailsPanel();
            
            // Add the main panel to UI manager
            UIManager.Instance.AddElement(_mainPanel);
            
            Logger.Instance.Info("[ShopScreen] Shop opened", LogCategory.UI);
        }
    }
}