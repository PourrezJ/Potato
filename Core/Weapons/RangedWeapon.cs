using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Potato.Engine;

namespace Potato.Core.Weapons
{
    public class RangedWeapon : Weapon
    {
        public float ProjectileSpeed { get; protected set; }
        public int ProjectilesPerShot { get; protected set; }
        public float SpreadAngle { get; protected set; }
        
        public RangedWeapon(string name) : base(name)
        {
            // Configuration par défaut pour une arme à distance
            Damage = 15;
            AttackSpeed = 2.0f; // 2 tirs par seconde
            Range = 500;
            ProjectileSpeed = 500;
            ProjectilesPerShot = 1;
            SpreadAngle = 5.0f; // Légère dispersion en degrés
        }
        
        protected override void LoadContent()
        {
            base.LoadContent();
            // Créer un rectangle jaune pour l'arme
            _texture = ShapeGenerator.CreateRectangle(16, 6, Color.Yellow);
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Mettre à jour la position de l'arme pour suivre le propriétaire
            if (Owner != null)
            {
                // Positionner l'arme à droite du joueur
                Vector2 offset = new Vector2(Owner.Bounds.Width / 2, 0);
                Position = Owner.Position + offset;
            }
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive || Owner == null || _texture == null)
                return;
                
            // Dessiner l'arme à côté du joueur
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
            
            // Dessiner les projectiles (géré par la classe de base)
            base.Draw(spriteBatch);
        }
        
        protected override void Attack()
        {
            if (Owner == null)
                return;
                
            // Calculer les dégâts
            float damage = CalculateDamage();
            
            // Obtenir la direction vers la cible (souris ou ennemi le plus proche)
            Vector2 targetDirection = GetTargetDirection();
            
            if (targetDirection == Vector2.Zero)
                return;
                
            // Créer le(s) projectile(s)
            FireProjectiles(targetDirection, damage);
        }
        
        private void FireProjectiles(Vector2 baseDirection, float damage)
        {
            // Régler la direction de base sur le vecteur unitaire
            if (baseDirection != Vector2.Zero)
            {
                baseDirection.Normalize();
            }
            
            // Calculer l'angle total de dispersion en radians
            float totalSpreadRadians = MathHelper.ToRadians(SpreadAngle);
            
            for (int i = 0; i < ProjectilesPerShot; i++)
            {
                // Calculer l'angle pour ce projectile
                float angleOffset = 0;
                
                if (ProjectilesPerShot > 1)
                {
                    // Distribuer les projectiles uniformément dans l'angle de dispersion
                    if (ProjectilesPerShot == 2)
                    {
                        // Pour 2 projectiles, l'un va légèrement à gauche, l'autre légèrement à droite
                        angleOffset = (i == 0) ? -totalSpreadRadians / 2 : totalSpreadRadians / 2;
                    }
                    else
                    {
                        // Pour plus de projectiles, les répartir uniformément
                        angleOffset = totalSpreadRadians * ((float)i / (ProjectilesPerShot - 1) - 0.5f);
                    }
                }
                else
                {
                    // Si un seul projectile, ajouter une légère variation aléatoire
                    angleOffset = (float)_random.NextDouble() * totalSpreadRadians - totalSpreadRadians / 2;
                }
                
                // Calculer la direction avec l'offset d'angle
                float cosAngle = (float)Math.Cos(angleOffset);
                float sinAngle = (float)Math.Sin(angleOffset);
                
                Vector2 direction = new Vector2(
                    baseDirection.X * cosAngle - baseDirection.Y * sinAngle,
                    baseDirection.X * sinAngle + baseDirection.Y * cosAngle
                );
                
                // Normaliser la direction
                direction.Normalize();
                
                // Créer le projectile
                Projectile projectile = new Projectile(
                    Position,
                    direction,
                    damage,
                    ProjectileSpeed,
                    true // C'est un projectile du joueur
                );
                
                // Ajouter le projectile à la liste
                _projectiles.Add(projectile);
            }
        }
        
        public override void Upgrade()
        {
            base.Upgrade();
            
            // Augmenter spécifiquement les statistiques d'arme à distance
            ProjectileSpeed *= 1.1f;
            
            // Tous les 2 niveaux, ajouter un projectile supplémentaire
            if (Tier % 2 == 0 && ProjectilesPerShot < 5)
            {
                ProjectilesPerShot++;
                
                // Augmenter légèrement la dispersion avec plus de projectiles
                SpreadAngle += 2.0f;
            }
        }
    }
}