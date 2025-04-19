using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Engine;

namespace Potato.Core.Enemies
{
    public class BasicEnemy : Enemy
    {
        public BasicEnemy() : base()
        {
            // Configure enemy stats
            Stats.MaxHealth = 50;
            Stats.Health = Stats.MaxHealth;
            Stats.Speed = 100;
            
            // Configure enemy properties
            ScoreValue = 10;
            ExperienceValue = 5;
            GoldValue = 2;
            DetectionRange = 300;
            AttackRange = 50;
            AttackDamage = 10;
            AttackSpeed = 1.0f;
        }
        
        protected override void LoadContent()
        {
            // Create a red triangle for the enemy
            _texture = ShapeGenerator.CreateTriangle(32, 32, Color.Red);
            
            // Initialize bounds
            UpdateBounds();
        }
        
        public static BasicEnemy CreateRandomEnemy(Game game, Vector2 spawnPosition)
        {
            BasicEnemy enemy = new BasicEnemy();
            enemy.Initialize();
            enemy.Position = spawnPosition;
            
            return enemy;
        }
    }
}