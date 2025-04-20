using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Potato.Engine;

namespace Potato.Core.Weapons
{
    public class MeleeWeapon : Weapon
    {
        private float _attackRange;
        // private float _attackAngle;
        
        public MeleeWeapon(string name) : base(name)
        {
            // Configure weapon properties
            Damage = 25;
            AttackSpeed = 1.2f;
            Range = 50;
            _attackRange = 60;
            // _attackAngle = MathHelper.Pi / 3; // 60 degrees
        }
        
        protected override void LoadContent()
        {
            // Create a green rectangle for the weapon
            _texture = ShapeGenerator.CreateRectangle(20, 8, Color.Green);
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Update weapon position to follow the owner
            if (Owner != null)
            {
                // Position the weapon to the right of the player
                Vector2 offset = new Vector2(Owner.Bounds.Width / 2, 0);
                Position = Owner.Position + offset;
            }
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive || Owner == null || _texture == null)
                return;
                
            // Draw weapon at player's side
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
        
        protected override void Attack()
        {
            if (Owner == null)
                return;
                
            // Get all enemies in the game
            var enemies = GameManager.Instance.Enemies;
            
            foreach (var enemy in enemies)
            {
                if (enemy.IsDead)
                    continue;
                    
                // Check if enemy is within attack range
                float distance = Vector2.Distance(Owner.Position, enemy.Position);
                
                if (distance <= _attackRange)
                {
                    // Check if enemy is within attack angle
                    Vector2 toEnemy = enemy.Position - Owner.Position;
                    float angle = (float)Math.Atan2(toEnemy.Y, toEnemy.X);
                    
                    // Apply damage to enemy
                    enemy.TakeDamage(CalculateDamage());
                }
            }
        }
    }
}