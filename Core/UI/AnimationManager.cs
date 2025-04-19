using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Potato.Core.UI
{
    /// <summary>
    /// Gestionnaire d'animations pour les éléments d'interface utilisateur et les entités du jeu.
    /// Fournit des méthodes d'animation réutilisables et configurables.
    /// </summary>
    public class AnimationManager
    {
        // Instance singleton pour accès global
        private static AnimationManager _instance;
        public static AnimationManager Instance => _instance ?? (_instance = new AnimationManager());
        
        // Types d'animations prédéfinis
        public enum AnimationType
        {
            Pulse,
            Bounce,
            Fade,
            Rotate,
            Shake
        }
        
        // Dictionnaire associant un identifiant d'objet à son temps d'animation
        private Dictionary<string, float> _animationTimers = new Dictionary<string, float>();
        
        // Constructeur privé pour le singleton
        private AnimationManager() { }
        
        /// <summary>
        /// Met à jour tous les timers d'animation
        /// </summary>
        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Mettre à jour tous les timers d'animation
            List<string> keys = new List<string>(_animationTimers.Keys);
            foreach (string key in keys)
            {
                _animationTimers[key] += deltaTime;
            }
        }
        
        /// <summary>
        /// Obtient ou démarre un timer d'animation pour un objet spécifique
        /// </summary>
        public float GetAnimationTime(string objectId)
        {
            if (!_animationTimers.ContainsKey(objectId))
            {
                _animationTimers[objectId] = 0f;
            }
            
            return _animationTimers[objectId];
        }
        
        /// <summary>
        /// Réinitialise un timer d'animation pour un objet spécifique
        /// </summary>
        public void ResetAnimation(string objectId)
        {
            _animationTimers[objectId] = 0f;
        }
        
        /// <summary>
        /// Applique une animation de pulsation à une valeur
        /// </summary>
        public float ApplyPulse(string objectId, float baseValue, float amplitude = 0.1f, float frequency = 3.0f)
        {
            float time = GetAnimationTime(objectId);
            return baseValue * (1.0f + (float)Math.Sin(time * frequency) * amplitude);
        }
        
        /// <summary>
        /// Applique une animation de rebond à une position Y
        /// </summary>
        public float ApplyBounce(string objectId, float baseY, float height = 10.0f, float frequency = 2.0f)
        {
            float time = GetAnimationTime(objectId);
            return baseY - Math.Abs((float)Math.Sin(time * frequency)) * height;
        }
        
        /// <summary>
        /// Applique une animation de fondu à une couleur
        /// </summary>
        public Color ApplyFade(string objectId, Color baseColor, float minAlpha = 0.5f, float frequency = 1.5f)
        {
            float time = GetAnimationTime(objectId);
            float alpha = (float)((Math.Sin(time * frequency) + 1) / 2); // de 0 à 1
            alpha = minAlpha + (1 - minAlpha) * alpha; // de minAlpha à 1
            
            return new Color(baseColor.R, baseColor.G, baseColor.B, (byte)(255 * alpha));
        }
        
        /// <summary>
        /// Applique une animation de rotation à un angle
        /// </summary>
        public float ApplyRotation(string objectId, float baseRotation = 0f, float speed = 1.0f)
        {
            float time = GetAnimationTime(objectId);
            return baseRotation + time * speed;
        }
        
        /// <summary>
        /// Applique une animation de tremblement à une position
        /// </summary>
        public Vector2 ApplyShake(string objectId, Vector2 basePosition, float intensity = 2.0f, float frequency = 20.0f)
        {
            float time = GetAnimationTime(objectId);
            return new Vector2(
                basePosition.X + (float)Math.Sin(time * frequency) * intensity,
                basePosition.Y + (float)Math.Cos(time * frequency + 1.3f) * intensity
            );
        }
        
        /// <summary>
        /// Applique une animation lissée entre deux valeurs (pour les transitions)
        /// </summary>
        public float Lerp(float start, float end, float amount)
        {
            return start + (end - start) * Math.Min(1, Math.Max(0, amount));
        }
        
        /// <summary>
        /// Applique une animation lissée entre deux vecteurs
        /// </summary>
        public Vector2 Lerp(Vector2 start, Vector2 end, float amount)
        {
            return new Vector2(
                Lerp(start.X, end.X, amount),
                Lerp(start.Y, end.Y, amount)
            );
        }
        
        /// <summary>
        /// Applique une animation lissée entre deux couleurs
        /// </summary>
        public Color Lerp(Color start, Color end, float amount)
        {
            return new Color(
                (int)Lerp(start.R, end.R, amount),
                (int)Lerp(start.G, end.G, amount),
                (int)Lerp(start.B, end.B, amount),
                (int)Lerp(start.A, end.A, amount)
            );
        }
    }
}