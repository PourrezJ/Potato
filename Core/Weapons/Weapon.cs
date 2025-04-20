using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Potato.Core.Entities;
using System.Collections.Generic;

namespace Potato.Core.Weapons
{
    public abstract class Weapon
    {
        public Entity Owner { get; set; }
        public string Name { get; protected set; }
        public float Damage { get; protected set; }
        public float AttackSpeed { get; protected set; }
        public float Range { get; protected set; }
        public int Tier { get; protected set; }
        public bool IsActive { get; protected set; }
        public Vector2 Position { get; protected set; }
        public bool AutoFire { get; set; }
        
        protected Texture2D _texture;
        protected float _attackTimer;
        protected Random _random;
        protected MouseState _currentMouseState;
        protected MouseState _previousMouseState;
        protected List<Projectile> _projectiles;
        
        public Weapon(string name)
        {
            Name = name;
            Tier = 1;
            IsActive = true;
            _attackTimer = 0;
            _random = new Random();
            AutoFire = true; // Par défaut, le tir est automatique
            _projectiles = new List<Projectile>();
        }
        
        public virtual void Initialize()
        {
            LoadContent();
        }
        
        protected virtual void LoadContent()
        {
            try
            {
                // Essayer de charger la texture de l'arme
                _texture = GameManager.Instance.Content.Load<Texture2D>(Name.ToLower());
            }
            catch
            {
                // Si la texture n'existe pas, utiliser une texture de remplacement
                _texture = new Texture2D( GameManager.Instance.GraphicsDevice, 1, 1);
                _texture.SetData(new[] { Color.White });
            }
        }
        
        public virtual void Update(GameTime gameTime)
        {
            if (!IsActive || Owner == null)
                return;
                
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Mettre à jour l'état de la souris
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
            
            // Mettre à jour la position de l'arme pour suivre le propriétaire
            Position = Owner.Position;
            
            // Mettre à jour les projectiles existants
            UpdateProjectiles(gameTime);
            
            // Décrémenter le timer d'attaque
            _attackTimer -= deltaTime;
            
            // Vérifier si l'arme peut attaquer
            if (_attackTimer <= 0)
            {
                bool shouldAttack = false;
                
                if (AutoFire)
                {
                    // Attaque automatique basée sur le timer
                    shouldAttack = true;
                }
                else if (_currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
                {
                    // Attaque manuelle basée sur le clic de souris
                    shouldAttack = true;
                }
                
                if (shouldAttack)
                {
                    Attack();
                    _attackTimer = 1.0f / AttackSpeed; // Réinitialiser le timer en fonction de la vitesse d'attaque
                }
            }
        }
        
        private void UpdateProjectiles(GameTime gameTime)
        {
            // Mettre à jour tous les projectiles
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                if (_projectiles[i].IsDead)
                {
                    // Supprimer les projectiles morts
                    _projectiles.RemoveAt(i);
                }
                else
                {
                    // Mettre à jour les projectiles actifs
                    _projectiles[i].Update(gameTime);
                }
            }
        }
        
        public virtual void UpdateProjectilesOnly(GameTime gameTime)
        {
            // Cette méthode spéciale ne met à jour que les projectiles existants
            // sans permettre de nouveaux tirs (utilisée pendant les phases entre les vagues)
            if (!IsActive)
                return;
                
            // Mettre à jour les projectiles existants
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                if (_projectiles[i].IsDead)
                {
                    // Supprimer les projectiles morts
                    _projectiles.RemoveAt(i);
                }
                else
                {
                    // Mettre à jour les projectiles actifs
                    _projectiles[i].Update(gameTime);
                }
            }
        }
        
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            // Dessiner tous les projectiles
            foreach (var projectile in _projectiles)
            {
                projectile.Draw(spriteBatch);
            }
        }
        
        protected abstract void Attack();
        
        public virtual void Upgrade()
        {
            Tier++;
            
            // Augmenter les statistiques de l'arme en fonction du niveau
            Damage *= 1.2f;
            AttackSpeed *= 1.1f;
            Range *= 1.05f;
        }
        
        public virtual float CalculateDamage()
        {
            // Calcul de dégâts de base
            float damage = Damage;
            
            // Appliquer le modificateur de dégâts du propriétaire si disponible
            if (Owner != null)
            {
                damage *= Owner.Stats.Damage / 10f;
            }
            
            // Calcul des coups critiques
            if (Owner != null && _random.NextDouble() < Owner.Stats.CriticalChance)
            {
                damage *= Owner.Stats.CriticalDamage;
            }
            
            return damage;
        }
        
        public virtual void ToggleAutoFire()
        {
            AutoFire = !AutoFire;
        }
        
        protected Vector2 GetTargetDirection()
        {
            if (Owner == null)
                return Vector2.Zero;

            Vector2 direction = Vector2.Zero;

            // If auto fire is enabled, target the closest enemy
            if (AutoFire)
            {
                // Find the closest enemy
                var closestEnemy = FindClosestEnemy();
                
                if (closestEnemy != null)
                {
                    // Calculate direction to the closest enemy
                    direction = closestEnemy.Position - Position;
                    
                    // Normalize the direction
                    if (direction != Vector2.Zero)
                    {
                        direction.Normalize();
                    }
                    
                    return direction;
                }
            }
            
            // If auto fire is disabled or no enemies found, target the mouse position
            Vector2 mousePosition = new Vector2(_currentMouseState.X, _currentMouseState.Y);
            direction = mousePosition - Position;
            
            // Normalize the direction
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }
            
            return direction;
        }
        
        private Enemies.Enemy FindClosestEnemy()
        {
            Enemies.Enemy closestEnemy = null;
            float closestDistance = float.MaxValue;
            
            // Get all enemies from the GameManager
            var enemies = GameManager.Instance.GetEnemies();
            
            foreach (var enemy in enemies)
            {
                if (enemy.IsActive && !enemy.IsDead)
                {
                    float distance = Vector2.Distance(Position, enemy.Position);
                    
                    // Check if this enemy is within range and closer than previously found enemies
                    if (distance <= Range && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy;
                    }
                }
            }
            
            return closestEnemy;
        }

        /// <summary>
        /// Supprime tous les projectiles existants émis par cette arme.
        /// Utilisé lors du nettoyage entre les vagues.
        /// </summary>
        public void ClearProjectiles()
        {
            if (_projectiles != null)
            {
                _projectiles.Clear();
            }
        }
    }
}