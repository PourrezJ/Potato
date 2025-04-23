using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core.Entities;
using Potato.Core.Logging;
using Potato.Core.Weapons;
using System;
using System.Collections.Generic;

namespace Potato.Core.UI.Screens
{
    /// <summary>
    /// Canvas de sélection des joueurs permettant aux utilisateurs de choisir leur personnage
    /// et de sélectionner leur arme de départ.
    /// </summary>
    public class PlayerSelectionCanvas : UICanvas
    {
        // Singleton pour l'accès facile depuis d'autres classes
        public static PlayerSelectionCanvas Current { get; private set; }
        
        // Référence au GameManager
        private GameManager _gameManager;
        
        // Éléments principaux de l'UI
        private Label _titleLabel;
        private Button _backButton;
        private Button _startGameButton;
        
        // Zone d'aperçu du personnage
        private Panel _characterPreviewPanel;
        private Label _characterNameLabel;
        private Label _characterDescriptionLabel;
        
        // Section de sélection des personnages
        private Panel _characterSelectionPanel;
        private List<Button> _characterButtons = new List<Button>();
        private int _selectedCharacterIndex = 0;
        
        // Section des statistiques
        private Panel _statsPanel;
        private Panel _healthBar;
        private Panel _strengthBar;
        private Panel _defenseBar;
        private Panel _speedBar;
        
        // Section de sélection d'arme
        private Panel _weaponSelectionPanel;
        private List<Button> _weaponButtons = new List<Button>();
        private Label _weaponNameLabel;
        private Label _weaponDescriptionLabel;
        private int _selectedWeaponIndex = 0;
        
        // Données de jeu
        private List<PlayerCharacter> _availableCharacters = new List<PlayerCharacter>();
        private List<Weapon> _availableWeapons = new List<Weapon>();
        
        // Couleurs thématiques
        private Color _primaryColor = new Color(76, 175, 80); // Vert
        private Color _secondaryColor = new Color(33, 150, 243); // Bleu
        private Color _accentColor = new Color(255, 193, 7); // Jaune/Or
        private Color _textColor = Color.White;
        private Color _panelColor = new Color(48, 48, 48, 200); // Gris foncé semi-transparent
        
        // Variable de débogage pour forcer l'affichage
        private bool _forceDebugDisplay = true;

        public PlayerSelectionCanvas() : base("PlayerSelection")
        {
            Current = this;
            _gameManager = GameManager.Instance;
            this.IsVisible = true; // S'assurer que le canvas est visible par défaut
        }
        
        public override void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
            
            base.OnDestroy();
        }
        
        public override void Awake()
        {
            base.Awake();
            
            try
            {
                // Forcer l'initialisation de UIManager.Pixel si nécessaire
                if (UIManager.Pixel == null)
                {
                    Logger.Warning("UIManager.Pixel n'est pas initialisé lors de l'éveil de PlayerSelectionCanvas, tentative d'initialisation", LogCategory.UI);
                    UIManager.Initialize();
                }
                
                // Définir explicitement la visibilité
                this.IsVisible = true;
                
                Logger.Info("PlayerSelectionCanvas Awake a réussi", LogCategory.UI);
            }
            catch (Exception ex)
            {
                Logger.Error($"Erreur lors de l'éveil de PlayerSelectionCanvas: {ex.Message}", LogCategory.UI);
            }
        }
        
        public override void Start()
        {
            base.Start();
            
            try
            {
                // S'assurer que UIManager.Pixel est initialisé
                if (UIManager.Pixel == null)
                {
                    Logger.Warning("UIManager.Pixel n'est pas initialisé, tentative d'initialisation dans PlayerSelectionCanvas", LogCategory.UI);
                    UIManager.Initialize();
                }
                
                // Initialiser les données
                InitializeCharactersAndWeapons();
                
                // Créer l'interface utilisateur
                CreateUI();
                
                // Par défaut, sélectionner le premier personnage et la première arme
                SelectCharacter(0);
                SelectWeapon(0);
                
                // Définir explicitement la visibilité
                this.IsVisible = true;
                
                // S'assurer que le canvas est enregistré dans UIManager
                UIManager.RegisterCanvas(this);
                
                // Journal
                Logger.Info("Canvas de sélection des joueurs initialisé", LogCategory.UI);
            }
            catch (Exception ex)
            {
                Logger.Error($"Erreur lors de l'initialisation de PlayerSelectionCanvas: {ex.Message}", LogCategory.UI);
            }
        }
        
        // Remplacer la méthode Draw pour forcer l'affichage en cas de problème
        public override void Draw(SpriteBatch spriteBatch)
        {
            try
            {
                // Vérifier la visibilité
                if (!IsVisible)
                {
                    Logger.Warning("Tentative de dessiner PlayerSelectionCanvas alors qu'il est marqué comme non visible", LogCategory.UI);
                    return;
                }
                
                // Vérifier que UIManager.Pixel est disponible
                if (UIManager.Pixel == null)
                {
                    Logger.Error("UIManager.Pixel est null lors du rendu de PlayerSelectionCanvas", LogCategory.UI);
                    return;
                }
                
                // Démarrer un nouveau SpriteBatch pour ce canvas
                spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone
                );
                
                try
                {
                    // Dessiner un fond de couleur pour voir si le canvas est au moins rendu
                    Rectangle screenRect = new Rectangle(0, 0, 
                        spriteBatch.GraphicsDevice.Viewport.Width, 
                        spriteBatch.GraphicsDevice.Viewport.Height);
                    spriteBatch.Draw(UIManager.Pixel, screenRect, new Color(100, 0, 100, 200));
                    
                    // Si le mode de débogage est activé, dessiner un simple texte pour tester
                    if (_forceDebugDisplay)
                    {
                        if (UIManager.DefaultFont != null)
                        {
                            string debugText = "PLAYER SELECTION SCREEN";
                            Vector2 textSize = UIManager.DefaultFont.MeasureString(debugText);
                            Vector2 position = new Vector2(
                                (screenRect.Width - textSize.X) / 2,
                                screenRect.Height / 3);
                            
                            spriteBatch.DrawString(
                                UIManager.DefaultFont,
                                debugText,
                                position,
                                Color.White
                            );
                            
                            // Dessiner un cadre autour du texte pour le rendre plus visible
                            Rectangle textRect = new Rectangle(
                                (int)position.X - 10,
                                (int)position.Y - 10,
                                (int)textSize.X + 20,
                                (int)textSize.Y + 20
                            );
                            
                            // Dessiner un cadre blanc autour du texte
                            spriteBatch.Draw(UIManager.Pixel, 
                                new Rectangle(textRect.X - 2, textRect.Y - 2, textRect.Width + 4, 2), 
                                Color.White); // Top
                            spriteBatch.Draw(UIManager.Pixel, 
                                new Rectangle(textRect.X - 2, textRect.Y + textRect.Height, textRect.Width + 4, 2), 
                                Color.White); // Bottom
                            spriteBatch.Draw(UIManager.Pixel, 
                                new Rectangle(textRect.X - 2, textRect.Y, 2, textRect.Height), 
                                Color.White); // Left
                            spriteBatch.Draw(UIManager.Pixel, 
                                new Rectangle(textRect.X + textRect.Width, textRect.Y, 2, textRect.Height), 
                                Color.White); // Right
                        }
                        else
                        {
                            Logger.Error("DefaultFont est null dans PlayerSelectionCanvas.Draw", LogCategory.UI);
                        }
                    }
                    
                    // Dessiner tous les éléments racines normalement
                    foreach (var element in _rootElements)
                    {
                        if (element != null && element.IsVisible)
                        {
                            element.Draw(spriteBatch);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Erreur pendant le rendu du canvas PlayerSelection: {ex.Message}", LogCategory.UI);
                }
                finally
                {
                    // S'assurer que End est toujours appelé, même en cas d'erreur
                    spriteBatch.End();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Erreur critique lors du rendu de PlayerSelectionCanvas: {ex.Message}", LogCategory.UI);
            }
        }
        
        private void InitializeCharactersAndWeapons()
        {
            // Créer des personnages avec des statistiques et caractéristiques différentes
            
            // Guerrier - Force élevée, défense élevée, vitesse faible
            var warrior = new PlayerCharacter("Chevalier", 
                "Un puissant chevalier maniant l'épée avec une grande force et une défense robuste.", 
                Color.SteelBlue);
            warrior.StatModifiers[StatType.MaxHealth] = 1.5f;
            warrior.StatModifiers[StatType.Damage] = 1.2f;
            warrior.StatModifiers[StatType.Speed] = 0.8f;
            
            // Archer - Vitesse élevée, dégâts moyens, défense faible
            var archer = new PlayerCharacter("Archer",
                "Un tireur agile qui excelle dans les attaques à distance avec une précision mortelle.",
                Color.ForestGreen);
            archer.StatModifiers[StatType.MaxHealth] = 0.9f;
            archer.StatModifiers[StatType.Speed] = 1.4f;
            archer.StatModifiers[StatType.Range] = 1.3f;
            archer.StatModifiers[StatType.CriticalChance] = 1.2f;
            
            // Mage - Dégâts élevés, défense très faible, vitesse moyenne
            var mage = new PlayerCharacter("Mage",
                "Un puissant lanceur de sorts maîtrisant la magie élémentaire et les attaques dévastatrices.",
                Color.MediumPurple);
            mage.StatModifiers[StatType.MaxHealth] = 0.7f;
            mage.StatModifiers[StatType.Damage] = 1.5f;
            mage.StatModifiers[StatType.AttackSpeed] = 1.2f;
            mage.StatModifiers[StatType.CriticalDamage] = 1.3f;
            
            // Paladin - Équilibré avec une santé élevée
            var paladin = new PlayerCharacter("Paladin",
                "Un champion béni aux capacités équilibrées, spécialisé dans la guérison et le support.",
                Color.Gold);
            paladin.StatModifiers[StatType.MaxHealth] = 1.3f;
            paladin.StatModifiers[StatType.Damage] = 1.1f;
            paladin.StatModifiers[StatType.CriticalChance] = 0.8f;
            paladin.StatModifiers[StatType.CriticalDamage] = 1.1f;
            
            _availableCharacters.Add(warrior);
            _availableCharacters.Add(archer);
            _availableCharacters.Add(mage);
            _availableCharacters.Add(paladin);
            
            // Créer différentes armes avec des caractéristiques uniques
            
            // Armes de mêlée
            MeleeWeapon sword = new MeleeWeapon("Épée du Croisé");
            _availableWeapons.Add(sword);
            
            MeleeWeapon axe = new MeleeWeapon("Hache de Guerre");
            _availableWeapons.Add(axe);
            
            // Armes à distance
            RangedWeapon bow = new RangedWeapon("Arc Long");
            _availableWeapons.Add(bow);
            
            RangedWeapon staff = new RangedWeapon("Bâton Arcanique");
            _availableWeapons.Add(staff);
            
            RangedWeapon crossbow = new RangedWeapon("Arbalète");
            _availableWeapons.Add(crossbow);
        }
        
        private void CreateUI()
        {
            // Récupérer les dimensions de l'écran
            var viewport = GameManager.Instance.GraphicsDevice.Viewport;
            int screenWidth = viewport.Width;
            int screenHeight = viewport.Height;
            
            // ===== Titre principal =====
            _titleLabel = UIBuilder.CreateLabel("SÉLECTION DU JOUEUR", new Vector2(screenWidth / 2, 50), _accentColor, 1.8f, true);
            this.AddElement(_titleLabel);
            
            // ===== Panel principal (fond semi-transparent) =====
            Panel mainPanel = UIBuilder.CreatePanel(
                new Vector2(50, 100),
                new Vector2(screenWidth - 100, screenHeight - 180),
                new Color(20, 20, 20, 150)
            );
            this.AddElement(mainPanel);
            
            // ===== Section de sélection de personnage (côté gauche) =====
            CreateCharacterSelectionSection(screenWidth, screenHeight);
            
            // ===== Section d'aperçu du personnage (centre) =====
            CreateCharacterPreviewSection(screenWidth, screenHeight);
            
            // ===== Section de sélection d'arme (côté droit) =====
            CreateWeaponSelectionSection(screenWidth, screenHeight);
            
            // ===== Boutons de navigation (bas de l'écran) =====
            _backButton = UIBuilder.CreateButton("RETOUR", new Vector2(150, screenHeight - 60), new Vector2(200, 50));
            _backButton.OnClickAction = OnBackButtonClicked;
            this.AddElement(_backButton);
            
            _startGameButton = UIBuilder.CreateButton("COMMENCER", new Vector2(screenWidth - 250, screenHeight - 60), new Vector2(200, 50));
            _startGameButton.OnClickAction = OnStartGameButtonClicked;
            this.AddElement(_startGameButton);
        }
        
        private void CreateCharacterSelectionSection(int screenWidth, int screenHeight)
        {
            int sectionWidth = (screenWidth - 140) / 3;
            
            // Panel de fond pour la sélection de personnage
            _characterSelectionPanel = UIBuilder.CreatePanel(
                new Vector2(70, 120),
                new Vector2(sectionWidth - 20, screenHeight - 220),
                _panelColor
            );
            this.AddElement(_characterSelectionPanel);
            
            // Titre de la section
            var sectionTitle = UIBuilder.CreateLabel("PERSONNAGES", 
                new Vector2(_characterSelectionPanel.Position.X + _characterSelectionPanel.Size.X / 2, _characterSelectionPanel.Position.Y + 30), 
                _primaryColor, 1.2f, true);
            this.AddElement(sectionTitle);
            
            // Créer un bouton pour chaque personnage
            float buttonY = _characterSelectionPanel.Position.Y + 80;
            float buttonSpacing = 70;
            
            for (int i = 0; i < _availableCharacters.Count; i++)
            {
                var character = _availableCharacters[i];
                var button = UIBuilder.CreateButton(character.Name, 
                    new Vector2(_characterSelectionPanel.Position.X + _characterSelectionPanel.Size.X / 2 - 100, buttonY), 
                    new Vector2(200, 50));
                
                // Capture de l'index pour l'utiliser dans la lambda
                int characterIndex = i;
                button.OnClickAction = () => SelectCharacter(characterIndex);
                
                this.AddElement(button);
                _characterButtons.Add(button);
                
                buttonY += buttonSpacing;
            }
        }
        
        private void CreateCharacterPreviewSection(int screenWidth, int screenHeight)
        {
            int sectionWidth = (screenWidth - 140) / 3;
            float centerX = screenWidth / 2;
            
            // Panel de fond pour l'aperçu du personnage
            _characterPreviewPanel = UIBuilder.CreatePanel(
                new Vector2(centerX - sectionWidth / 2, 120),
                new Vector2(sectionWidth, screenHeight - 220),
                _panelColor
            );
            this.AddElement(_characterPreviewPanel);
            
            // Nom du personnage (large et en évidence)
            _characterNameLabel = UIBuilder.CreateLabel("", 
                new Vector2(centerX, _characterPreviewPanel.Position.Y + 30), 
                _accentColor, 1.4f, true);
            this.AddElement(_characterNameLabel);
            
            // Description du personnage
            _characterDescriptionLabel = UIBuilder.CreateLabel("", 
                new Vector2(centerX, _characterPreviewPanel.Position.Y + 70), 
                _textColor, 0.8f, true);
            this.AddElement(_characterDescriptionLabel);
            
            // Panel des statistiques
            _statsPanel = UIBuilder.CreatePanel(
                new Vector2(centerX - sectionWidth / 2 + 20, _characterPreviewPanel.Position.Y + 130),
                new Vector2(sectionWidth - 40, 200),
                new Color(40, 40, 40, 150)
            );
            this.AddElement(_statsPanel);
            
            // Titre des statistiques
            var statsTitle = UIBuilder.CreateLabel("STATISTIQUES", 
                new Vector2(centerX, _statsPanel.Position.Y + 20), 
                _textColor, 1.0f, true);
            this.AddElement(statsTitle);
            
            // Créer les barres de progression pour les statistiques
            float barY = _statsPanel.Position.Y + 50;
            float barWidth = sectionWidth - 80;
            float barHeight = 20;
            float barSpacing = 35;
            
            // Barre de santé
            var healthLabel = UIBuilder.CreateLabel("Santé:", 
                new Vector2(_statsPanel.Position.X + 10, barY + barHeight / 2), 
                _textColor, 0.8f, false);
            this.AddElement(healthLabel);
            
            _healthBar = UIBuilder.CreatePanel(
                new Vector2(_statsPanel.Position.X + 80, barY),
                new Vector2(barWidth - 80, barHeight),
                new Color(220, 53, 69) // Rouge
            );
            this.AddElement(_healthBar);
            
            // Barre de force
            barY += barSpacing;
            var strengthLabel = UIBuilder.CreateLabel("Force:", 
                new Vector2(_statsPanel.Position.X + 10, barY + barHeight / 2), 
                _textColor, 0.8f, false);
            this.AddElement(strengthLabel);
            
            _strengthBar = UIBuilder.CreatePanel(
                new Vector2(_statsPanel.Position.X + 80, barY),
                new Vector2(barWidth - 80, barHeight),
                new Color(253, 126, 20) // Orange
            );
            this.AddElement(_strengthBar);
            
            // Barre de défense
            barY += barSpacing;
            var defenseLabel = UIBuilder.CreateLabel("Défense:", 
                new Vector2(_statsPanel.Position.X + 10, barY + barHeight / 2), 
                _textColor, 0.8f, false);
            this.AddElement(defenseLabel);
            
            _defenseBar = UIBuilder.CreatePanel(
                new Vector2(_statsPanel.Position.X + 80, barY),
                new Vector2(barWidth - 80, barHeight),
                new Color(13, 110, 253) // Bleu
            );
            this.AddElement(_defenseBar);
            
            // Barre de vitesse
            barY += barSpacing;
            var speedLabel = UIBuilder.CreateLabel("Vitesse:", 
                new Vector2(_statsPanel.Position.X + 10, barY + barHeight / 2), 
                _textColor, 0.8f, false);
            this.AddElement(speedLabel);
            
            _speedBar = UIBuilder.CreatePanel(
                new Vector2(_statsPanel.Position.X + 80, barY),
                new Vector2(barWidth - 80, barHeight),
                new Color(25, 135, 84) // Vert
            );
            this.AddElement(_speedBar);
        }
        
        private void CreateWeaponSelectionSection(int screenWidth, int screenHeight)
        {
            int sectionWidth = (screenWidth - 140) / 3;
            
            // Panel de fond pour la sélection d'arme
            _weaponSelectionPanel = UIBuilder.CreatePanel(
                new Vector2(screenWidth - 70 - sectionWidth, 120),
                new Vector2(sectionWidth - 20, screenHeight - 220),
                _panelColor
            );
            this.AddElement(_weaponSelectionPanel);
            
            // Titre de la section
            var sectionTitle = UIBuilder.CreateLabel("ARMES", 
                new Vector2(_weaponSelectionPanel.Position.X + _weaponSelectionPanel.Size.X / 2, _weaponSelectionPanel.Position.Y + 30), 
                _secondaryColor, 1.2f, true);
            this.AddElement(sectionTitle);
            
            // Créer un bouton pour chaque arme
            float buttonY = _weaponSelectionPanel.Position.Y + 80;
            float buttonSpacing = 60;
            
            for (int i = 0; i < _availableWeapons.Count; i++)
            {
                var weapon = _availableWeapons[i];
                var button = UIBuilder.CreateButton(weapon.Name, 
                    new Vector2(_weaponSelectionPanel.Position.X + _weaponSelectionPanel.Size.X / 2 - 100, buttonY), 
                    new Vector2(200, 50));
                
                // Capture de l'index pour l'utiliser dans la lambda
                int weaponIndex = i;
                button.OnClickAction = () => SelectWeapon(weaponIndex);
                
                this.AddElement(button);
                _weaponButtons.Add(button);
                
                buttonY += buttonSpacing;
            }
            
            // Informations sur l'arme sélectionnée
            float infoY = _weaponSelectionPanel.Position.Y + 400;
            
            // Nom de l'arme
            _weaponNameLabel = UIBuilder.CreateLabel("", 
                new Vector2(_weaponSelectionPanel.Position.X + _weaponSelectionPanel.Size.X / 2, infoY), 
                _accentColor, 1.2f, true);
            this.AddElement(_weaponNameLabel);
            
            // Description de l'arme
            _weaponDescriptionLabel = UIBuilder.CreateLabel("", 
                new Vector2(_weaponSelectionPanel.Position.X + _weaponSelectionPanel.Size.X / 2, infoY + 40), 
                _textColor, 0.8f, true);
            this.AddElement(_weaponDescriptionLabel);
        }
        
        private void SelectCharacter(int index)
        {
            if (index >= 0 && index < _availableCharacters.Count)
            {
                // Sélectionner le nouveau personnage
                _selectedCharacterIndex = index;
                
                var character = _availableCharacters[index];
                
                // Mettre à jour les informations du personnage
                _characterNameLabel.Text = character.Name;
                _characterDescriptionLabel.Text = character.Description;
                
                // Mettre à jour l'apparence des barres de progression
                // en fonction des modificateurs de statistiques du personnage
                float healthMod = character.StatModifiers.ContainsKey(StatType.MaxHealth) ? character.StatModifiers[StatType.MaxHealth] : 1f;
                float damageMod = character.StatModifiers.ContainsKey(StatType.Damage) ? character.StatModifiers[StatType.Damage] : 1f;
                float defenseMod = character.StatModifiers.ContainsKey(StatType.MaxHealth) ? character.StatModifiers[StatType.MaxHealth] * 0.8f : 0.8f; // Estimation de la défense
                float speedMod = character.StatModifiers.ContainsKey(StatType.Speed) ? character.StatModifiers[StatType.Speed] : 1f;
                
                // Limiter la taille des barres entre 0.2 et 1.0
                float normalizedHealth = MathHelper.Clamp(healthMod / 1.5f, 0.2f, 1.0f);
                float normalizedDamage = MathHelper.Clamp(damageMod / 1.5f, 0.2f, 1.0f);
                float normalizedDefense = MathHelper.Clamp(defenseMod / 1.5f, 0.2f, 1.0f);
                float normalizedSpeed = MathHelper.Clamp(speedMod / 1.5f, 0.2f, 1.0f);
                
                // Ajuster la taille des barres pour représenter les statistiques
                _healthBar.Size = new Vector2(_healthBar.Size.X * normalizedHealth, _healthBar.Size.Y);
                _strengthBar.Size = new Vector2(_strengthBar.Size.X * normalizedDamage, _strengthBar.Size.Y);
                _defenseBar.Size = new Vector2(_defenseBar.Size.X * normalizedDefense, _defenseBar.Size.Y);
                _speedBar.Size = new Vector2(_speedBar.Size.X * normalizedSpeed, _speedBar.Size.Y);
                
                Logger.Info($"Personnage sélectionné: {character.Name}", LogCategory.UI);
            }
        }
        
        private void SelectWeapon(int index)
        {
            if (index >= 0 && index < _availableWeapons.Count)
            {
                // Sélectionner la nouvelle arme
                _selectedWeaponIndex = index;
                
                var weapon = _availableWeapons[index];
                
                // Mettre à jour les informations de l'arme
                _weaponNameLabel.Text = weapon.Name;
                
                // Créer une description basée sur le type d'arme
                string weaponType = weapon is MeleeWeapon ? "Arme de mêlée" : "Arme à distance";
                string description = $"{weaponType}\nDégâts: {weapon.Damage}  |  Vitesse: {weapon.AttackSpeed}  |  Portée: {weapon.Range}";
                
                _weaponDescriptionLabel.Text = description;
                
                Logger.Info($"Arme sélectionnée: {weapon.Name}", LogCategory.UI);
            }
        }
        
        private void OnBackButtonClicked()
        {
            // Retourner au menu principal
            GameManager.Instance.SetGameState(GameManager.GameState.MainMenu);
            
            // Cacher ce canvas
            this.IsVisible = false;
            
            Logger.Info("Retour au menu principal", LogCategory.UI);
        }
        
        private void OnStartGameButtonClicked()
        {
            if (_selectedCharacterIndex >= 0 && _selectedWeaponIndex >= 0)
            {
                var selectedCharacter = _availableCharacters[_selectedCharacterIndex];
                var selectedWeapon = _availableWeapons[_selectedWeaponIndex];
                
                // Créer un nouveau joueur à partir du personnage sélectionné
                Player selectedPlayer = selectedCharacter.CreatePlayer();
                
                // Définir le personnage et l'arme sélectionnés pour la partie
                GameManager.Instance.SetSelectedPlayer(selectedPlayer);
                GameManager.Instance.AddSelectedWeapon(selectedWeapon);
                
                // Démarrer le jeu
                Logger.Info($"Démarrage du jeu avec {selectedCharacter.Name} et {selectedWeapon.Name}", LogCategory.Gameplay);
                GameManager.Instance.StartGame();
                
                // Cacher ce canvas
                this.IsVisible = false;
            }
            else
            {
                Logger.Warning("Tentative de démarrer le jeu sans sélection complète", LogCategory.UI);
            }
        }
        
        // Méthodes publiques pour exposer des fonctionnalités au niveau de la scène
        public void Show()
        {
            this.IsVisible = true;
            Logger.Info("Affichage du canvas de sélection des joueurs", LogCategory.UI);
        }
        
        public void Hide()
        {
            this.IsVisible = false;
        }
    }
}