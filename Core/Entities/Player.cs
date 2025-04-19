using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Potato.Core.Weapons;
using Potato.Engine;

namespace Potato.Core.Entities
{
    public class Player : Entity
    {
        public static Player Local { get; private set; }

        public List<Weapon> Weapons { get; private set; }
        public int Gold { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int ExperienceToNextLevel { get; set; }
        public PlayerCharacter Character { get; private set; }
        
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        private float _invincibilityTimer;
        private bool _isInvincible;
        private const float INVINCIBILITY_DURATION = 0.5f;

        // Exposer le jeu pour pouvoir y accéder depuis d'autres classes
        public Game Game => _game;
        
        public Player() : base()
        {
            Weapons = new List<Weapon>();
            Gold = 0;
            Level = 1;
            Experience = 0;
            ExperienceToNextLevel = 100;
            _invincibilityTimer = 0;
            _isInvincible = false;
        }
        
        public Player(PlayerCharacter character) : this()
        {
            Character = character;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            // Set initial player position to center of screen
            Position = new Vector2(
                Game.GraphicsDevice.Viewport.Width / 2,
                Game.GraphicsDevice.Viewport.Height / 2);
                
            // Configure player stats
            Stats.MaxHealth = 150;
            Stats.Health = Stats.MaxHealth;
            Stats.Speed = 250;
            
            // Apply character-specific stat modifiers if available
            if (Character != null)
            {
                foreach (var statMod in Character.StatModifiers)
                {
                    switch (statMod.Key)
                    {
                        case StatType.MaxHealth:
                            Stats.MaxHealth += statMod.Value;
                            Stats.Health = Stats.MaxHealth;
                            break;
                        case StatType.Speed:
                            Stats.Speed += statMod.Value;
                            break;
                        case StatType.Damage:
                            Stats.Damage += statMod.Value;
                            break;
                        case StatType.AttackSpeed:
                            Stats.AttackSpeed += statMod.Value;
                            break;
                        case StatType.Range:
                            Stats.Range += statMod.Value;
                            break;
                        case StatType.CriticalChance:
                            Stats.CriticalChance += statMod.Value;
                            break;
                        case StatType.CriticalDamage:
                            Stats.CriticalDamage += statMod.Value;
                            break;
                        case StatType.Harvesting:
                            Stats.Harvesting += statMod.Value;
                            break;
                        case StatType.Engineering:
                            Stats.Engineering += statMod.Value;
                            break;
                        case StatType.Luck:
                            Stats.Luck += statMod.Value;
                            break;
                    }
                }
            }
            
            // Ajouter une arme de base au joueur s'il n'en a pas
            if (Weapons.Count == 0)
            {
                AddDefaultWeapon();
            }

            Local = this; // Set the static Local property to this instance
        }
        
        private void AddDefaultWeapon()
        {
            // Créer une arme à distance de base
            RangedWeapon defaultWeapon = new RangedWeapon("DefaultGun");
            defaultWeapon.Initialize();
            
            // Attribuer l'arme au joueur
            AddWeapon(defaultWeapon);
        }
        
        protected override void LoadContent()
        {
            // Create a blue circle for the player unless character has a specific color
            Color playerColor = Character != null ? Character.Color : Color.Blue;
            _texture = ShapeGenerator.CreateCircle(24, playerColor);
            
            // Initialize bounds
            UpdateBounds();
        }
        
        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Update keyboard states
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();
            
            // Handle player movement
            HandleMovement(deltaTime);
            
            // Handle weapon toggles
            HandleWeaponControls();
            
            // Update weapons
            foreach (var weapon in Weapons)
            {
                weapon.Update(gameTime);
            }
            
            // Update invincibility
            if (_isInvincible)
            {
                _invincibilityTimer -= deltaTime;
                
                if (_invincibilityTimer <= 0)
                {
                    _isInvincible = false;
                }
            }
            
            // Update base entity behavior
            base.Update(gameTime);
        }
        
        private void HandleWeaponControls()
        {
            // Touche pour basculer entre le tir automatique et manuel (F)
            if (IsKeyPressed(Keys.F))
            {
                foreach (var weapon in Weapons)
                {
                    weapon.ToggleAutoFire();
                }
            }
        }
        
        private bool IsKeyPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        }
        
        private void HandleMovement(float deltaTime)
        {
            Vector2 movement = Vector2.Zero;
            
            // Check movement keys
            if (_currentKeyboardState.IsKeyDown(Keys.W) || _currentKeyboardState.IsKeyDown(Keys.Up))
                movement.Y -= 1;
                
            if (_currentKeyboardState.IsKeyDown(Keys.S) || _currentKeyboardState.IsKeyDown(Keys.Down))
                movement.Y += 1;
                
            if (_currentKeyboardState.IsKeyDown(Keys.A) || _currentKeyboardState.IsKeyDown(Keys.Left))
                movement.X -= 1;
                
            if (_currentKeyboardState.IsKeyDown(Keys.D) || _currentKeyboardState.IsKeyDown(Keys.Right))
                movement.X += 1;
                
            // Normalize movement vector if moving diagonally
            if (movement != Vector2.Zero)
            {
                movement.Normalize();
            }
            
            // Apply speed
            Velocity = movement * Stats.Speed;
            
            // Keep player within screen bounds
            var viewport = _game.GraphicsDevice.Viewport;
            Position = new Vector2(
                MathHelper.Clamp(Position.X, _texture.Width / 2, viewport.Width - _texture.Width / 2),
                MathHelper.Clamp(Position.Y, _texture.Height / 2, viewport.Height - _texture.Height / 2));
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            // If invincible, flash the player
            if (_isInvincible && (int)(_invincibilityTimer * 10) % 2 == 0)
            {
                base.Draw(spriteBatch);
            }
            else if (!_isInvincible)
            {
                base.Draw(spriteBatch);
            }
            
            // Draw weapons
            foreach (var weapon in Weapons)
            {
                weapon.Draw(spriteBatch);
            }
        }
        
        public override void TakeDamage(float damage)
        {
            if (!_isInvincible && !IsDead)
            {
                base.TakeDamage(damage);
                
                // Start invincibility
                _isInvincible = true;
                _invincibilityTimer = INVINCIBILITY_DURATION;
            }
        }
        
        public void AddWeapon(Weapon weapon)
        {
            weapon.Owner = this;
            Weapons.Add(weapon);
        }
        
        public void AddExperience(int amount)
        {
            Experience += amount;
            
            // Check for level up
            while (Experience >= ExperienceToNextLevel)
            {
                LevelUp();
            }
        }
        
        private void LevelUp()
        {
            Experience -= ExperienceToNextLevel;
            Level++;
            ExperienceToNextLevel = 100 * Level;
            
            // TODO: Show level up UI and options
        }
        
        public override void Reset()
        {
            base.Reset();
            Weapons.Clear();
            Gold = 0;
            Level = 1;
            Experience = 0;
            ExperienceToNextLevel = 100;
            _invincibilityTimer = 0;
            _isInvincible = false;
            
            // Reset player stats
            Stats.MaxHealth = 150;
            Stats.Health = Stats.MaxHealth;
            Stats.Speed = 250;
        }
    }
}