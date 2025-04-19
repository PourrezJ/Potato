using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Potato.Core.Entities;
using Potato.Engine;

namespace Potato.Core.Enemies
{
    public abstract class Enemy : Entity
    {
        public int ScoreValue { get; protected set; }
        public int ExperienceValue { get; protected set; }
        public int GoldValue { get; protected set; }
        public float DetectionRange { get; protected set; }
        public float AttackRange { get; protected set; }
        public float AttackDamage { get; protected set; }
        public float AttackSpeed { get; protected set; }
        
        protected Player _targetPlayer;
        protected float _attackTimer;
        protected Random _random;
        protected EnemyState _currentState;
        
        public Enemy() : base()
        {
            _random = new Random();
            _attackTimer = 0;
            _currentState = EnemyState.Chase; // Changé de Wander à Chase par défaut
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            _targetPlayer = Player.Local;
        }
        
        public override void Update(GameTime gameTime)
        {
            if (!IsActive || IsDead)
                return;
                
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Update AI state
            UpdateAI(deltaTime);
            
            // Update attack timer
            if (_attackTimer > 0)
            {
                _attackTimer -= deltaTime;
            }
            
            // Update base entity behavior
            base.Update(gameTime);
        }
        
        protected virtual void UpdateAI(float deltaTime)
        {
            if (_targetPlayer == null || _targetPlayer.IsDead)
                return;
                
            // Calculate distance to player
            float distanceToPlayer = Vector2.Distance(Position, _targetPlayer.Position);
            
            // Update state based on distance - ennemis foncent toujours sur le joueur
            if (distanceToPlayer <= AttackRange)
            {
                _currentState = EnemyState.Attack;
            }
            else
            {
                _currentState = EnemyState.Chase; // Toujours chasser le joueur, jamais flâner
            }
            
            // Handle behavior based on current state
            switch (_currentState)
            {
                case EnemyState.Wander:
                    HandleChase(deltaTime); // Rediriger vers Chase même si état Wander
                    break;
                case EnemyState.Chase:
                    HandleChase(deltaTime);
                    break;
                case EnemyState.Attack:
                    HandleAttack(deltaTime);
                    break;
            }
        }
        
        protected virtual void HandleWander(float deltaTime)
        {
            // Basic random movement
            if (_random.NextDouble() < 0.02)
            {
                float angle = (float)(_random.NextDouble() * Math.PI * 2);
                Velocity = new Vector2(
                    (float)Math.Cos(angle),
                    (float)Math.Sin(angle)) * Stats.Speed * 0.5f;
            }
        }
        
        protected virtual void HandleChase(float deltaTime)
        {
            // Chase player
            Vector2 direction = _targetPlayer.Position - Position;
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }
            
            Velocity = direction * Stats.Speed;
        }
        
        protected virtual void HandleAttack(float deltaTime)
        {
            // Stop movement when attacking
            Velocity = Vector2.Zero;
            
            // Attack if timer allows
            if (_attackTimer <= 0)
            {
                Attack();
                _attackTimer = 1.0f / AttackSpeed;
            }
        }
        
        protected virtual void Attack()
        {
            if (_targetPlayer != null && !_targetPlayer.IsDead)
            {
                float distance = Vector2.Distance(Position, _targetPlayer.Position);
                
                if (distance <= AttackRange)
                {
                    _targetPlayer.TakeDamage(AttackDamage);
                }
            }
        }
        
        protected override void Die()
        {
            base.Die();
            
            // Drop experience and gold when killed
            if (_targetPlayer != null && !_targetPlayer.IsDead)
            {
                _targetPlayer.AddExperience(ExperienceValue);
                _targetPlayer.Gold += GoldValue;
                
                // Spawn collectibles
                SpawnCollectibles();
            }
        }
        
        protected virtual void SpawnCollectibles()
        {
            if (_game == null)
                return;
                
            // Chance de laisser tomber un collectible
            Random rand = new Random();
            float dropChance = 0.7f; // 70% de chance de laisser tomber quelque chose
            
            if (rand.NextDouble() < dropChance)
            {
                // Déterminer le type de collectible à laisser tomber
                CollectibleType type;
                int value;
                
                float goldChance = 0.6f;
                float xpChance = 0.3f;
                float healthChance = 0.1f; // Chance explicite pour les collectibles de santé
                
                double roll = rand.NextDouble();
                
                if (roll < goldChance)
                {
                    type = CollectibleType.Gold;
                    value = rand.Next(1, 5) * GoldValue / 2; // Entre 1 et 5 fois la moitié de la valeur d'or de l'ennemi
                }
                else if (roll < goldChance + xpChance)
                {
                    type = CollectibleType.Experience;
                    value = rand.Next(1, 3) * ExperienceValue / 2; // Entre 1 et 3 fois la moitié de la valeur d'XP de l'ennemi
                }
                else if (roll < goldChance + xpChance + healthChance)
                {
                    type = CollectibleType.Health;
                    value = rand.Next(5, 15); // Entre 5 et 15 points de vie
                }
                else
                {
                    // Si aucune condition n'est remplie, par défaut on donne de l'or
                    type = CollectibleType.Gold;
                    value = rand.Next(1, 3); // Petite quantité d'or
                }
                
                // Ajouter un peu de variation à la position où le collectible apparaît
                Vector2 offsetPos = new Vector2(
                    (float)(rand.NextDouble() * 20 - 10),
                    (float)(rand.NextDouble() * 20 - 10)
                );
                
                Collectible collectible = new Collectible(Position + offsetPos, type, value);
                GameManager.Instance.AddCollectible(collectible);
            }
        }
    }
    
    public enum EnemyState
    {
        Wander,
        Chase,
        Attack
    }
}