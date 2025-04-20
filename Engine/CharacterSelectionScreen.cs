using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Potato.Core.Entities;
using Potato.Core.Weapons;
using Potato.Core.UI;
using System;
using Potato.Core;

namespace Potato.Engine
{
    public class CharacterSelectionScreen : GameBehaviour
    {
        private List<PlayerCharacter> _characters;
        private List<Weapon> _startingWeapons;
        private int _selectedCharacterIndex;
        private int _selectedWeaponIndex;
        // private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;
        private bool _isSelectionConfirmed;
        private SpriteFont _font;
        
        // Cache des textures pour améliorer les performances
        private Dictionary<Color, Texture2D> _characterTextureCache;
        private Texture2D _pixelTexture;
        
        private Rectangle _characterPreviewRect;
        private Rectangle _weaponPreviewRect;
        private float _selectionTimer;
        private float _animationTime = 0f;
        
        // Grille de personnages
        private List<Button> _characterButtons;
        private int _charactersPerRow = 4;
        private int _totalRows = 1;
        private int _characterIconSize = 80;
        private int _characterGridSpacing = 20;

        // UI Components
        private Panel _mainPanel;
        private Panel _characterStatsPanel;
        private Panel _characterPreviewPanel;
        private Panel _weaponPanel;
        private Button _backButton;
        private Button _confirmButton;
        private Label _characterNameLabel;
        private Label _characterDescLabel;
        private Label _titleLabel;
        private Label _weaponLabel;
        private Button _nextWeaponButton;
        private Button _prevWeaponButton;
        
        // Couleurs et styles
        private Color _panelColor = new Color(40, 40, 60, 220);
        private Color _titleColor = new Color(255, 215, 0); // Gold
        private Color _highlightColor = new Color(255, 255, 0); // Yellow
        private Color _positiveStatColor = new Color(100, 255, 100);
        private Color _negativeStatColor = new Color(255, 100, 100);
        private Color _confirmButtonColor = new Color(50, 180, 50, 230); // Green for confirm button
        
        public override void Start()
        {
            base.Awake();

            _game = GameManager.Instance;
            _characters = new List<PlayerCharacter>();
            _startingWeapons = new List<Weapon>();
            _characterButtons = new List<Button>();
            _selectedCharacterIndex = 0;
            _selectedWeaponIndex = 0;
            _isSelectionConfirmed = false;
            _selectionTimer = 0;
            
            // Initialiser le cache de textures
            _characterTextureCache = new Dictionary<Color, Texture2D>();
            
            // Créer une texture de pixel blanc pour le rendu
            _pixelTexture = new Texture2D(_game.GraphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            
            // Charger la police
            try
            {
                _font = _game.Content.Load<SpriteFont>("DefaultFont");
            }
            catch
            {
                _font = null;
            }
            
            InitializeCharacters();
            InitializeWeapons();
            
            // Set up preview rectangles
            _characterPreviewRect = new Rectangle(
                _game.GraphicsDevice.Viewport.Width / 4,
                _game.GraphicsDevice.Viewport.Height / 2 - 100,
                200,
                200);
                
            _weaponPreviewRect = new Rectangle(
                _game.GraphicsDevice.Viewport.Width * 3 / 4,
                _game.GraphicsDevice.Viewport.Height / 2 - 50,
                100,
                100);
                
            InitializeUI();
        }
        
        private void InitializeCharacters()
        {
            // Add available character types
            _characters.Add(new PlayerCharacter("Balanced", "A well-rounded character with balanced stats. Good for beginners!", Color.White)
            {
                StatModifiers = new Dictionary<StatType, float>
                {
                    { StatType.MaxHealth, 5 },
                    { StatType.Speed, 5 },
                    { StatType.Harvesting, 8 }
                }
            });
            
            _characters.Add(new PlayerCharacter("Tanky", "Slow but extremely durable. Can withstand heavy damage.", Color.DarkBlue)
            {
                StatModifiers = new Dictionary<StatType, float>
                {
                    { StatType.MaxHealth, 50 },
                    { StatType.Speed, -30 },
                    { StatType.Damage, 5 }
                }
            });
            
            _characters.Add(new PlayerCharacter("Speedy", "Lightning fast but fragile. Dodge attacks rather than taking hits.", Color.Cyan)
            {
                StatModifiers = new Dictionary<StatType, float>
                {
                    { StatType.MaxHealth, -20 },
                    { StatType.Speed, 70 },
                    { StatType.CriticalChance, 0.1f }
                }
            });
            
            // Ajouter quelques personnages verrouillés pour démonstration
            _characters.Add(new PlayerCharacter("Locked", "Defeat the boss to unlock this character", Color.DarkGray)
            {
                IsLocked = true
            });
        }
        
        private void InitializeWeapons()
        {
            // Add available starting weapons
            _startingWeapons.Add(new MeleeWeapon("Sword"));
            _startingWeapons.Add(new RangedWeapon("Bow"));
        }
        
        private void InitializeUI()
        {
            // Get screen dimensions
            int screenWidth = _game.GraphicsDevice.Viewport.Width;
            int screenHeight = _game.GraphicsDevice.Viewport.Height;
            
            // Main panel with gradient background
            _mainPanel = new Panel(
                new Vector2(0, 0),
                new Vector2(screenWidth, screenHeight),
                new Color(20, 20, 40, 220));
            _mainPanel.Title = "";
            
            // Titre principal avec style amélioré
            _titleLabel = new Label(
                new Vector2(screenWidth / 2, 50),
                "CHARACTER SELECTION");
            _titleLabel.TextColor = _titleColor;
            _titleLabel.Scale = 2.0f;
            _titleLabel.DrawBackground = true;
            _titleLabel.BackgroundColor = new Color(60, 60, 80, 150);
            _titleLabel.CornerRadius = 10f;
            _titleLabel.Padding = 10;
            _mainPanel.AddChild(_titleLabel);
            
            // Bouton retour stylisé
            _backButton = new Button(
                new Vector2(50, 50),
                new Vector2(100, 40),
                "Back");
            _backButton.Color = new Color(60, 60, 80, 200);
            _backButton.HoverColor = new Color(80, 80, 100, 220);
            _backButton.BorderColor = new Color(100, 100, 150);
            _backButton.HasBorder = true;
            _backButton.CornerRadius = 5f;
            _backButton.OnClick += (_) => {
                // Retour au menu principal
                _isSelectionConfirmed = true;
                UIManager.Instance.RemoveElement(_mainPanel);
            };
            _mainPanel.AddChild(_backButton);
            
            // Panel d'aperçu du personnage (côté gauche)
            _characterPreviewPanel = new Panel(
                new Vector2(screenWidth / 2 - 450, 130),
                new Vector2(300, 350),
                _panelColor,
                "PREVIEW");
            _characterPreviewPanel.BorderColor = new Color(80, 80, 120);
            _characterPreviewPanel.HasBorder = true;
            _characterPreviewPanel.BorderThickness = 2;
            _characterPreviewPanel.CornerRadius = 10f;
            _mainPanel.AddChild(_characterPreviewPanel);
            
            // Panel d'informations du personnage (côté droit)
            _characterStatsPanel = new Panel(
                new Vector2(screenWidth / 2 + 100, 130),
                new Vector2(350, 350),
                _panelColor,
                "CHARACTER DETAILS");
            _characterStatsPanel.BorderColor = new Color(80, 80, 120);
            _characterStatsPanel.HasBorder = true;
            _characterStatsPanel.BorderThickness = 2;
            _characterStatsPanel.CornerRadius = 10f;
            _mainPanel.AddChild(_characterStatsPanel);
            
            // Nom et description du personnage
            _characterNameLabel = new Label(
                new Vector2(175, 50),
                _characters[_selectedCharacterIndex].Name);
            _characterNameLabel.TextColor = Color.White;
            _characterNameLabel.Scale = 1.5f;
            _characterStatsPanel.AddChild(_characterNameLabel);
            
            _characterDescLabel = new Label(
                new Vector2(175, 90),
                _characters[_selectedCharacterIndex].Description);
            _characterDescLabel.TextColor = Color.LightGray;
            _characterStatsPanel.AddChild(_characterDescLabel);
            
            // Panel de sélection d'arme
            _weaponPanel = new Panel(
                new Vector2(screenWidth / 2 - 150, 130),
                new Vector2(200, 200),
                _panelColor,
                "WEAPON");
            _weaponPanel.BorderColor = new Color(80, 80, 120);
            _weaponPanel.HasBorder = true;
            _weaponPanel.BorderThickness = 2;
            _weaponPanel.CornerRadius = 10f;
            _mainPanel.AddChild(_weaponPanel);
            
            // Étiquette d'arme
            _weaponLabel = new Label(
                new Vector2(100, 50),
                _startingWeapons[_selectedWeaponIndex].Name);
            _weaponLabel.TextColor = Color.White;
            _weaponPanel.AddChild(_weaponLabel);
            
            // Boutons de navigation d'arme
            _prevWeaponButton = new Button(
                new Vector2(40, 100),
                new Vector2(30, 30),
                "<");
            _prevWeaponButton.OnClick += (_) => {
                _selectedWeaponIndex--;
                if (_selectedWeaponIndex < 0)
                    _selectedWeaponIndex = _startingWeapons.Count - 1;
                _weaponLabel.Text = _startingWeapons[_selectedWeaponIndex].Name;
            };
            _weaponPanel.AddChild(_prevWeaponButton);
            
            _nextWeaponButton = new Button(
                new Vector2(130, 100),
                new Vector2(30, 30),
                ">");
            _nextWeaponButton.OnClick += (_) => {
                _selectedWeaponIndex++;
                if (_selectedWeaponIndex >= _startingWeapons.Count)
                    _selectedWeaponIndex = 0;
                _weaponLabel.Text = _startingWeapons[_selectedWeaponIndex].Name;
            };
            _weaponPanel.AddChild(_nextWeaponButton);
            
            // Création de la grille de personnages
            CreateCharacterGrid();
            
            // Bouton de confirmation large et visible au bas de l'écran - NOUVEAU STYLE PLUS VISIBLE
            _confirmButton = new Button(
                new Vector2(screenWidth / 2 - 150, screenHeight - 100),
                new Vector2(300, 70),
                "START GAME!");
            
            // Couleurs vives pour attirer l'attention
            _confirmButton.Color = new Color(220, 50, 50, 255); // Rouge vif
            _confirmButton.HoverColor = new Color(255, 60, 60, 255); // Rouge plus clair au survol
            _confirmButton.PressedColor = new Color(180, 40, 40, 255); // Rouge plus foncé quand pressé
            
            // Bordure épaisse et voyante
            _confirmButton.HasBorder = true;
            _confirmButton.BorderColor = Color.Yellow; // Bordure jaune pour contraste
            _confirmButton.BorderThickness = 4;
            
            // Coins arrondis pour un look moderne
            _confirmButton.CornerRadius = 20f;
            
            // Texte bien visible
            _confirmButton.TextColor = Color.White;
            _confirmButton.Scale = 1.5f; // Texte plus grand
            
            // Gestionnaire d'événements
            _confirmButton.OnClick += (_) => {
                ConfirmSelection();
            };
            
            // Ajouter le bouton directement au panneau principal (pas à un sous-panneau)
            _mainPanel.AddChild(_confirmButton);
        }
        
        public void OpenCharacterSelectionScreen()
        {
            // Afficher l'écran de sélection de personnage
            _isSelectionConfirmed = false;
            UIManager.Instance.AddElement(_mainPanel);
            
            // Mettre à jour l'interface utilisateur
            UpdateCharacterUI();
        }

        private void CreateCharacterGrid()
        {
            int screenWidth = _game.GraphicsDevice.Viewport.Width;
            
            // Calculer la position de départ de la grille
            int startX = (screenWidth - (_charactersPerRow * (_characterIconSize + _characterGridSpacing))) / 2;
            int startY = 500;
            
            // Panel pour la grille de personnages
            Panel characterGridPanel = new Panel(
                new Vector2(startX - 20, startY - 20),
                new Vector2((_characterIconSize + _characterGridSpacing) * _charactersPerRow + 40, 
                            (_characterIconSize + _characterGridSpacing) * _totalRows + 40),
                new Color(30, 30, 50, 200),
                "AVAILABLE CHARACTERS");
            characterGridPanel.BorderColor = new Color(80, 80, 120);
            characterGridPanel.HasBorder = true;
            characterGridPanel.BorderThickness = 2;
            characterGridPanel.CornerRadius = 10f;
            _mainPanel.AddChild(characterGridPanel);
            
            // Créer une grille de boutons pour les personnages
            for (int row = 0; row < _totalRows; row++)
            {
                for (int col = 0; col < _charactersPerRow; col++)
                {
                    int index = row * _charactersPerRow + col;
                    if (index >= _characters.Count) break;
                    
                    int posX = startX + col * (_characterIconSize + _characterGridSpacing);
                    int posY = startY + row * (_characterIconSize + _characterGridSpacing);
                    
                    Button charButton = new Button(
                        new Vector2(posX, posY),
                        new Vector2(_characterIconSize, _characterIconSize),
                        "");
                    
                    // Définir l'apparence en fonction de l'état (verrouillé ou non)
                    if (_characters[index].IsLocked)
                    {
                        charButton.Color = new Color(50, 50, 50, 200);
                        charButton.TextColor = Color.Gray;
                        charButton.Text = "X";
                        charButton.BorderColor = new Color(100, 100, 100, 150);
                    }
                    else
                    {
                        charButton.Color = _characters[index].Color;
                        charButton.TextColor = Color.White;
                        charButton.BorderColor = new Color(200, 200, 200, 150);
                    }
                    
                    // Style du bouton
                    charButton.CornerRadius = 5f;
                    charButton.HasBorder = true;
                    
                    // Encadrement spécial pour le personnage sélectionné
                    if (index == _selectedCharacterIndex)
                    {
                        charButton.BorderThickness = 3;
                        charButton.BorderColor = _highlightColor;
                    }
                    else
                    {
                        charButton.BorderThickness = 1;
                    }
                    
                    // Capturer l'index pour le gestionnaire d'événements
                    int characterIndex = index;
                    charButton.OnClick += (_) => {
                        if (!_characters[characterIndex].IsLocked)
                        {
                            // Mettre à jour l'ancien bouton sélectionné
                            if (_selectedCharacterIndex < _characterButtons.Count)
                            {
                                _characterButtons[_selectedCharacterIndex].BorderThickness = 1;
                                _characterButtons[_selectedCharacterIndex].BorderColor = new Color(200, 200, 200, 150);
                            }
                            
                            // Définir le nouveau bouton sélectionné
                            _selectedCharacterIndex = characterIndex;
                            _characterButtons[characterIndex].BorderThickness = 3;
                            _characterButtons[characterIndex].BorderColor = _highlightColor;
                            
                            // Mettre à jour l'interface
                            UpdateCharacterUI();
                        }
                    };
                    
                    _characterButtons.Add(charButton);
                    _mainPanel.AddChild(charButton);
                }
            }
        }
        
        private void UpdateCharacterUI()
        {
            // Mettre à jour le nom et la description du personnage sélectionné
            _characterNameLabel.Text = _characters[_selectedCharacterIndex].Name;
            _characterDescLabel.Text = _characters[_selectedCharacterIndex].Description;
            
            // Réinitialiser l'animation
            _animationTime = 0f;
        }
        
        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _selectionTimer += deltaTime;
            
            // Mettre à jour le gestionnaire d'animations
            AnimationManager.Instance.Update(gameTime);                          
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_isSelectionConfirmed)
                return;
            
            // Dessiner un fond pour l'écran
            Texture2D pixel = new Texture2D(_game.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            
            // Draw character preview with animation
            Color characterColor = _characters[_selectedCharacterIndex].Color;
            float scale = 1.0f + (float)Math.Sin(_animationTime * 2) * 0.05f; // Animation légère de pulsation
            
            // Créer une texture pour le personnage avec des détails
            Texture2D characterTexture = CreateCharacterTexture(_game.GraphicsDevice, characterColor);
            
            // Position au centre du panel de prévisualisation
            Vector2 characterPos = new Vector2(
                _characterPreviewPanel.Position.X + _characterPreviewPanel.Size.X / 2,
                _characterPreviewPanel.Position.Y + _characterPreviewPanel.Size.Y / 2 - 20);
            
            // Dessiner le personnage avec une animation de pulsation
            spriteBatch.Draw(
                characterTexture, 
                new Rectangle(
                    (int)characterPos.X, 
                    (int)characterPos.Y, 
                    (int)(120 * scale), 
                    (int)(120 * scale)), 
                null,
                Color.White,
                0f,
                new Vector2(60, 60), // Centre de rotation
                SpriteEffects.None,
                0f);
            
            // Dessiner l'arme
            DrawWeaponPreview(spriteBatch, pixel);
            
            // Dessiner les statistiques du personnage
            if (_font != null)
            {
                int yPos = 150;
                var statMods = _characters[_selectedCharacterIndex].StatModifiers;
                
                // Titre de la section stats
                string statTitle = "CHARACTER STATS:";
                Vector2 titlePos = new Vector2(_characterStatsPanel.Position.X + 30, _characterStatsPanel.Position.Y + 130);
                spriteBatch.DrawString(_font, statTitle, titlePos, Color.White);
                
                // Ligne de séparation
                spriteBatch.Draw(
                    pixel, 
                    new Rectangle(
                        (int)titlePos.X, 
                        (int)titlePos.Y + 25, 
                        300, 
                        1), 
                    Color.Gray);
                
                // Dessiner chaque statistique avec une barre colorée
                foreach (var statMod in statMods)
                {
                    string modText = FormatStatModifier(statMod.Key, statMod.Value);
                    Color modColor = statMod.Value > 0 ? _positiveStatColor : _negativeStatColor;
                    
                    Vector2 textPos = new Vector2(_characterStatsPanel.Position.X + 40, _characterStatsPanel.Position.Y + yPos);
                    spriteBatch.DrawString(_font, modText, textPos, modColor);
                    
                    // Dessiner une barre pour visualiser la statistique
                    int barLength = (int)Math.Abs(statMod.Value) * 3; // Longueur proportionnelle à la valeur
                    int barHeight = 6;
                    int barX = (int)textPos.X + 200;
                    int barY = (int)textPos.Y + 8;
                    
                    spriteBatch.Draw(
                        pixel, 
                        new Rectangle(barX, barY, barLength, barHeight), 
                        modColor);
                    
                    // Contour de la barre
                    spriteBatch.Draw(pixel, new Rectangle(barX - 1, barY - 1, barLength + 2, 1), Color.White);
                    spriteBatch.Draw(pixel, new Rectangle(barX - 1, barY + barHeight, barLength + 2, 1), Color.White);
                    spriteBatch.Draw(pixel, new Rectangle(barX - 1, barY, 1, barHeight), Color.White);
                    spriteBatch.Draw(pixel, new Rectangle(barX + barLength, barY, 1, barHeight), Color.White);
                    
                    yPos += 40;
                }
            }
            
            // Dessiner directement un énorme bouton de validation en bas de l'écran
            int screenWidth = _game.GraphicsDevice.Viewport.Width;
            int screenHeight = _game.GraphicsDevice.Viewport.Height;
            
            // Fond du bouton (rectangle rouge)
            Rectangle buttonRect = new Rectangle(
                screenWidth / 2 - 150, 
                screenHeight - 100, 
                300, 
                70);
            
            // Animation de pulsation pour le bouton
            float buttonPulse = 1.0f + (float)Math.Sin(_animationTime * 3) * 0.05f;
            Rectangle pulseRect = new Rectangle(
                buttonRect.X - (int)(buttonRect.Width * (buttonPulse - 1) / 2),
                buttonRect.Y - (int)(buttonRect.Height * (buttonPulse - 1) / 2),
                (int)(buttonRect.Width * buttonPulse),
                (int)(buttonRect.Height * buttonPulse));
            
            // Dessiner le rectangle de base du bouton (rouge)
            spriteBatch.Draw(pixel, pulseRect, new Color(220, 50, 50, 255));
            
            // Dessiner la bordure jaune
            int borderThickness = 4;
            spriteBatch.Draw(pixel, new Rectangle(pulseRect.X - borderThickness, pulseRect.Y - borderThickness, pulseRect.Width + borderThickness * 2, borderThickness), Color.Yellow); // Haut
            spriteBatch.Draw(pixel, new Rectangle(pulseRect.X - borderThickness, pulseRect.Y + pulseRect.Height, pulseRect.Width + borderThickness * 2, borderThickness), Color.Yellow); // Bas
            spriteBatch.Draw(pixel, new Rectangle(pulseRect.X - borderThickness, pulseRect.Y, borderThickness, pulseRect.Height), Color.Yellow); // Gauche
            spriteBatch.Draw(pixel, new Rectangle(pulseRect.X + pulseRect.Width, pulseRect.Y, borderThickness, pulseRect.Height), Color.Yellow); // Droite
            
            // Dessiner le texte du bouton
            if (_font != null)
            {
                string buttonText = "PLAY NOW!";
                Vector2 textSize = _font.MeasureString(buttonText) * 1.5f;
                Vector2 textPos = new Vector2(
                    buttonRect.X + buttonRect.Width / 2 - textSize.X / 2,
                    buttonRect.Y + buttonRect.Height / 2 - textSize.Y / 2);
                
                // Dessiner une ombre pour le texte
                spriteBatch.DrawString(_font, buttonText, new Vector2(textPos.X + 2, textPos.Y + 2), new Color(0, 0, 0, 150), 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
                // Dessiner le texte
                spriteBatch.DrawString(_font, buttonText, textPos, Color.White, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
            }
            
            // Vérifier si le bouton est cliqué
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && 
                _previousMouseState.LeftButton == ButtonState.Released &&
                buttonRect.Contains(mouseState.X, mouseState.Y))
            {
                ConfirmSelection();
            }
            
            _previousMouseState = mouseState;
        }
        
        private void DrawWeaponPreview(SpriteBatch spriteBatch, Texture2D pixel)
        {
            Vector2 weaponPos = new Vector2(
                _weaponPanel.Position.X + _weaponPanel.Size.X / 2,
                _weaponPanel.Position.Y + _weaponPanel.Size.Y / 2 + 30);
            
            if (_startingWeapons[_selectedWeaponIndex] is MeleeWeapon)
            {
                // Draw a sword with more detail
                spriteBatch.Draw(pixel, new Rectangle((int)weaponPos.X - 5, (int)weaponPos.Y - 40, 10, 60), Color.Silver); // Blade
                spriteBatch.Draw(pixel, new Rectangle((int)weaponPos.X - 15, (int)weaponPos.Y - 5, 30, 10), Color.Goldenrod); // Crossguard
                spriteBatch.Draw(pixel, new Rectangle((int)weaponPos.X - 3, (int)weaponPos.Y + 20, 6, 15), new Color(139, 69, 19)); // Handle
                
                // Blade reflection
                spriteBatch.Draw(pixel, new Rectangle((int)weaponPos.X - 3, (int)weaponPos.Y - 35, 2, 50), Color.White * 0.7f);
            }
            else if (_startingWeapons[_selectedWeaponIndex] is RangedWeapon)
            {
                // Bow arc
                for (int i = 0; i < 20; i++)
                {
                    float angle = (float)i / 20 * MathHelper.Pi;
                    int x = (int)(weaponPos.X + Math.Sin(angle) * 25);
                    int y = (int)(weaponPos.Y - 25 + Math.Cos(angle) * 25);
                    spriteBatch.Draw(pixel, new Rectangle(x, y, 3, 3), new Color(139, 69, 19));
                }
                
                // Bow string
                spriteBatch.Draw(pixel, new Rectangle((int)weaponPos.X, (int)weaponPos.Y - 25, 1, 50), Color.White);
                
                // Arrow
                spriteBatch.Draw(pixel, new Rectangle((int)weaponPos.X + 5, (int)weaponPos.Y - 1, 25, 2), Color.SaddleBrown);
                spriteBatch.Draw(pixel, new Rectangle((int)weaponPos.X + 30, (int)weaponPos.Y - 5, 5, 10), Color.DarkGray);
            }
            
            // Dessiner une petite flèche pointant du personnage vers l'arme
            DrawArrowBetween(
                spriteBatch, 
                new Vector2(_characterPreviewPanel.Position.X + _characterPreviewPanel.Size.X, 
                            _characterPreviewPanel.Position.Y + _characterPreviewPanel.Size.Y / 2),
                new Vector2(_weaponPanel.Position.X, 
                            _weaponPanel.Position.Y + _weaponPanel.Size.Y / 2),
                pixel,
                Color.White * 0.7f);
        }
        
        // Ajoute une nouvelle méthode pour dessiner une flèche entre deux points
        private void DrawArrowBetween(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Texture2D pixel, Color color)
        {
            Vector2 direction = end - start;
            float distance = direction.Length();
            direction.Normalize();
            
            // Line
            for (int i = 0; i < distance - 10; i++)
            {
                Vector2 pos = start + direction * i;
                spriteBatch.Draw(pixel, new Rectangle((int)pos.X, (int)pos.Y, 2, 2), color);
            }
            
            // Arrow head
            Vector2 arrowTip = end - direction * 10;
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X) * 5;
            
            Vector2 leftPoint = arrowTip - perpendicular;
            Vector2 rightPoint = arrowTip + perpendicular;
            
            for (int i = 0; i < 10; i++)
            {
                Vector2 lerp1 = Vector2.Lerp(leftPoint, end, i / 10f);
                Vector2 lerp2 = Vector2.Lerp(rightPoint, end, i / 10f);
                
                spriteBatch.Draw(pixel, new Rectangle((int)lerp1.X, (int)lerp1.Y, 2, 2), color);
                spriteBatch.Draw(pixel, new Rectangle((int)lerp2.X, (int)lerp2.Y, 2, 2), color);
            }
        }
        
        private Texture2D CreateCharacterTexture(GraphicsDevice graphicsDevice, Color color)
        {
            // Vérifier si la texture est déjà en cache
            if (_characterTextureCache.ContainsKey(color))
            {
                return _characterTextureCache[color];
            }
            
            // Sinon, créer une nouvelle texture
            Texture2D texture = new Texture2D(graphicsDevice, 120, 120);
            Color[] data = new Color[120 * 120];
            
            // Remplir avec transparence par défaut
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.Transparent;
            }
            
            // Dessiner un cercle pour le corps
            int radius = 58;
            int centerX = 60;
            int centerY = 60;
            
            for (int y = 0; y < 120; y++)
            {
                for (int x = 0; x < 120; x++)
                {
                    int distance = (int)Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                    if (distance < radius)
                    {
                        // Ombrage subtil pour donner du volume
                        float shade = 0.7f + (float)(x - centerX) / radius * 0.3f;
                        data[y * 120 + x] = new Color(
                            (int)(color.R * shade),
                            (int)(color.G * shade),
                            (int)(color.B * shade),
                            color.A);
                    }
                }
            }
            
            // Ajouter des yeux
            int eyeSize = 5;
            int eyeY = 50;
            
            // Œil gauche
            for (int y = eyeY - eyeSize; y < eyeY + eyeSize; y++)
            {
                for (int x = 45 - eyeSize; x < 45 + eyeSize; x++)
                {
                    if ((x - 45) * (x - 45) + (y - eyeY) * (y - eyeY) < eyeSize * eyeSize)
                    {
                        data[y * 120 + x] = Color.Black;
                    }
                }
            }
            
            // Œil droit
            for (int y = eyeY - eyeSize; y < eyeY + eyeSize; y++)
            {
                for (int x = 75 - eyeSize; x < 75 + eyeSize; x++)
                {
                    if ((x - 75) * (x - 75) + (y - eyeY) * (y - eyeY) < eyeSize * eyeSize)
                    {
                        data[y * 120 + x] = Color.Black;
                    }
                }
            }
            
            // Ajouter une bouche
            for (int y = 75; y < 80; y++)
            {
                for (int x = 45; x < 75; x++)
                {
                    // Courbe de la bouche
                    if (y - 75 <= Math.Sin((x - 45) * Math.PI / 30) * 4)
                    {
                        data[y * 120 + x] = Color.Black;
                    }
                }
            }
            
            texture.SetData(data);
            
            // Ajouter la texture au cache
            _characterTextureCache[color] = texture;
            
            return texture;
        }
        
        private string FormatStatModifier(StatType type, float value)
        {
            string prefix = value > 0 ? "+" : "";
            
            switch (type)
            {
                case StatType.MaxHealth:
                    return $"{prefix}{value} HP Max";
                case StatType.Speed:
                    return $"{prefix}{value} Speed";
                case StatType.Harvesting:
                    return $"{prefix}{value} Harvest";
                case StatType.Damage:
                    return $"{prefix}{value} Damage";
                case StatType.CriticalChance:
                    return $"{prefix}{value * 100} Crit";
                default:
                    return $"{type}: {prefix}{value}";
            }
        }
        
        private void ConfirmSelection()
        {
            if (_characters[_selectedCharacterIndex].IsLocked)
                return;
                
            _isSelectionConfirmed = true;
            
            // Create player with selected character
            PlayerCharacter selectedCharacter = _characters[_selectedCharacterIndex];
            Player player = selectedCharacter.CreatePlayer();
            
            // Create weapon with selected weapon
            Weapon selectedWeapon = _startingWeapons[_selectedWeaponIndex];
            
            // Utiliser GameManager au lieu de Game1
            GameManager game = GameManager.Instance;
                   
            // Maintenant configurer le joueur et l'arme
            game.SetSelectedPlayer(player);
            game.AddSelectedWeapon(selectedWeapon);
            
            // Start the game
            game.StartGame();
            
            // Remove UI elements from the manager
            UIManager.Instance.RemoveElement(_mainPanel);
        }
        
        public bool IsConfirmed()
        {
            return _isSelectionConfirmed;
        }
    }
}