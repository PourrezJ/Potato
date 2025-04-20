using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Potato.Core.Entities;
using Potato.Engine;
using Potato.Core.Enemies;

namespace Potato.Core.Weapons
{
    public class Projectile
    {
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public float Damage { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsPlayerProjectile { get; private set; }
        
        private float _lifeTimer;
        private float _lifespan;
        private Texture2D _texture;
        private float _speed;
        private Rectangle _bounds;
        
        GraphicsDevice _graphicsDevice;

        public Projectile(Vector2 position, Vector2 direction, float damage, float speed, bool isPlayerProjectile)
        {
            Position = position;
            Damage = damage;
            _speed = speed;
            IsDead = false;
            IsPlayerProjectile = isPlayerProjectile;
            
            // Normaliser la direction et appliquer la vitesse
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }
            Velocity = direction * _speed;
            
            // Définir la durée de vie du projectile (basée sur la portée et la vitesse)
            _lifeTimer = 0;
            _lifespan = 5.0f; // 5 secondes maximum
            
            _graphicsDevice = GameManager.Instance.GraphicsDevice;

            LoadContent();
        }
        
        private void LoadContent()
        {
            // Créer une petite forme pour le projectile
            Color projectileColor = IsPlayerProjectile ? Color.Cyan : Color.Red;
            _texture = ShapeGenerator.CreateCircle(6, projectileColor);
            
            // Initialiser les limites du projectile
            UpdateBounds();
        }
        
        private void UpdateBounds()
        {
            if (_texture != null)
            {
                _bounds = new Rectangle(
                    (int)(Position.X - _texture.Width / 2),
                    (int)(Position.Y - _texture.Height / 2),
                    _texture.Width,
                    _texture.Height);
            }
        }
        
        public void Update(GameTime gameTime)
        {
            if (IsDead)
                return;
                
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Mettre à jour la position du projectile
            Position += Velocity * deltaTime;
            
            // Mettre à jour les limites
            UpdateBounds();
            
            // Vérifier si le projectile sort de l'écran
            var viewport = _graphicsDevice.Viewport;
            if (Position.X < 0 || Position.X > viewport.Width ||
                Position.Y < 0 || Position.Y > viewport.Height)
            {
                IsDead = true;
                return;
            }
            
            // Incrémenter le timer de durée de vie
            _lifeTimer += deltaTime;
            if (_lifeTimer >= _lifespan)
            {
                IsDead = true;
                return;
            }
            
            // Vérifier les collisions avec les entités
            CheckCollisions();
        }
        
        private void CheckCollisions()
        {
            // Utiliser GameManager pour accéder aux ennemis et au joueur
            if (IsPlayerProjectile)
            {
                // Vérifier les collisions avec les ennemis
                foreach (var enemy in GameManager.Instance.Enemies)
                {
                    if (enemy != null && !enemy.IsDead && _bounds.Intersects(enemy.Bounds))
                    {
                        // Appliquer les dégâts à l'ennemi touché
                        enemy.TakeDamage(Damage);
                        
                        // Marquer le projectile comme mort
                        IsDead = true;
                        return;
                    }
                }
            }
            else
            {
                // Vérifier la collision avec le joueur
                Player player = GameManager.Instance.Player;
                if (player != null && !player.IsDead && _bounds.Intersects(player.Bounds))
                {
                    // Appliquer les dégâts au joueur
                    player.TakeDamage(Damage);
                    
                    // Marquer le projectile comme mort
                    IsDead = true;
                    return;
                }
            }
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead || _texture == null)
                return;
                
            spriteBatch.Draw(
                _texture,
                Position,
                null,
                Color.White,
                0,
                new Vector2(_texture.Width / 2, _texture.Height / 2),
                1.0f,
                SpriteEffects.None,
                0.5f);
        }
    }
}