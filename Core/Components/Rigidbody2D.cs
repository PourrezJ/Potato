using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Potato.Core.Components
{
    /// <summary>
    /// Composant pour simuler une physique simplifiée en 2D.
    /// Gère les forces, la vélocité et les collisions simples.
    /// </summary>
    public class Rigidbody2D : Component
    {
        // Propriétés physiques
        public float Mass { get; set; } = 1.0f;
        public float Drag { get; set; } = 0.1f;
        public float AngularDrag { get; set; } = 0.05f;
        public float GravityScale { get; set; } = 1.0f;
        public bool UseGravity { get; set; } = true;
        
        // État du rigidbody
        public Vector2 Velocity { get; set; } = Vector2.Zero;
        public float AngularVelocity { get; set; } = 0f;
        public bool IsKinematic { get; set; } = false;
        
        // Contraintes de mouvement
        public bool FreezePositionX { get; set; } = false;
        public bool FreezePositionY { get; set; } = false;
        public bool FreezeRotation { get; set; } = false;
        
        // Forces accumulées qui seront appliquées au prochain update
        private Vector2 _forces = Vector2.Zero;
        private float _torque = 0f;
        
        // Gravité par défaut
        private static readonly Vector2 _defaultGravity = new Vector2(0, 9.8f);
        
        public override void Update(GameTime gameTime)
        {
            if (IsKinematic)
                return;
                
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Calculer les forces totales
            Vector2 totalForce = _forces;
            
            // Ajouter la gravité si activée
            if (UseGravity)
            {
                totalForce += _defaultGravity * GravityScale * Mass;
            }
            
            // Appliquer les forces (F = ma => a = F/m)
            Vector2 acceleration = totalForce / Mass;
            
            // Mettre à jour la vitesse (v = v0 + at)
            Velocity += acceleration * deltaTime;
            
            // Appliquer la traînée (résistance)
            Velocity *= (1f - Drag * deltaTime);
            
            // Mettre à jour la position (p = p0 + vt)
            Vector2 newPosition = Transform.Position;
            
            // Appliquer les contraintes de position
            if (!FreezePositionX)
                newPosition.X += Velocity.X * deltaTime;
                
            if (!FreezePositionY)
                newPosition.Y += Velocity.Y * deltaTime;
                
            Transform.Position = newPosition;
            
            // Appliquer la rotation
            if (!FreezeRotation)
            {
                // Appliquer le couple (τ = Iα, mais simplifié)
                AngularVelocity += _torque / Mass * deltaTime;
                
                // Appliquer la traînée angulaire
                AngularVelocity *= (1f - AngularDrag * deltaTime);
                
                // Mettre à jour la rotation
                Transform.Rotation += AngularVelocity * deltaTime;
            }
            
            // Réinitialiser les forces pour le prochain frame
            _forces = Vector2.Zero;
            _torque = 0f;
        }
        
        /// <summary>
        /// Applique une force au centre de masse.
        /// </summary>
        public void AddForce(Vector2 force)
        {
            _forces += force;
        }
        
        /// <summary>
        /// Applique une force à une position spécifique, générant potentiellement un couple.
        /// </summary>
        public void AddForceAtPosition(Vector2 force, Vector2 position)
        {
            _forces += force;
            
            // Calculer le couple (moment de force)
            Vector2 relativePosition = position - Transform.Position;
            // Cross product en 2D = x1*y2 - y1*x2
            _torque += relativePosition.X * force.Y - relativePosition.Y * force.X;
        }
        
        /// <summary>
        /// Applique un couple qui fait tourner le rigidbody.
        /// </summary>
        public void AddTorque(float torque)
        {
            _torque += torque;
        }
        
        /// <summary>
        /// Arrête tout mouvement du rigidbody.
        /// </summary>
        public void Stop()
        {
            Velocity = Vector2.Zero;
            AngularVelocity = 0f;
            _forces = Vector2.Zero;
            _torque = 0f;
        }
    }
}