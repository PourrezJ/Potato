using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Engine;
using System;

namespace Potato.Core.Entities
{
    public class Collectible
    {
        public Vector2 Position { get; set; }
        public bool IsActive { get; private set; }
        public CollectibleType Type { get; private set; }
        public int Value { get; private set; }
        public Rectangle Bounds { get; private set; }
        
        private Texture2D _texture;
        private float _lifetime;
        private float _blinkTimer;
        private bool _isBlinking;
        private readonly float BLINK_INTERVAL = 0.2f;
        private readonly float MAX_LIFETIME = 15.0f; // Disparaît après 15 secondes
        
        public Collectible(Vector2 position, CollectibleType type, int value)
        {
            Position = position;
            IsActive = true;
            Type = type;
            Value = value;
            _lifetime = 0;
            _blinkTimer = 0;
            _isBlinking = false;
            
            // Création de la texture basée sur le type
            switch (Type)
            {
                case CollectibleType.Gold:
                    _texture = ShapeGenerator.CreateCircle(8, Color.Gold);
                    break;
                case CollectibleType.Experience:
                    _texture = ShapeGenerator.CreateCircle(8, Color.LimeGreen);
                    break;
                case CollectibleType.Health:
                    _texture = ShapeGenerator.CreateCircle(8, Color.Red);
                    break;
                default:
                    _texture = ShapeGenerator.CreateCircle(8, Color.White);
                    break;
            }
            
            // Initialiser les limites (bounds)
            UpdateBounds();
        }
        
        private void UpdateBounds()
        {
            Bounds = new Rectangle(
                (int)Position.X - _texture.Width / 2,
                (int)Position.Y - _texture.Height / 2,
                _texture.Width,
                _texture.Height);
        }
        
        public void Update(float deltaTime)
        {
            if (!IsActive)
                return;
                
            // Mettre à jour la durée de vie
            _lifetime += deltaTime;
            
            // Commencer à clignoter si proche de la disparition
            if (_lifetime > MAX_LIFETIME * 0.7f && !_isBlinking)
            {
                _isBlinking = true;
            }
            
            // Gérer le clignotement
            if (_isBlinking)
            {
                _blinkTimer += deltaTime;
                if (_blinkTimer >= BLINK_INTERVAL)
                {
                    _blinkTimer = 0;
                }
            }
            
            // Vérifier si doit disparaître
            if (_lifetime >= MAX_LIFETIME)
            {
                IsActive = false;
            }
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive)
                return;
                
            // Ne pas dessiner pendant une partie du cycle de clignotement
            if (_isBlinking && _blinkTimer > BLINK_INTERVAL / 2)
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
                0.3f);
        }
        
        public void Collect()
        {
            IsActive = false;
        }
    }
    
    public enum CollectibleType
    {
        Gold,
        Experience,
        Health
    }
}