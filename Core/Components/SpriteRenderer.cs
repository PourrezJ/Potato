using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Potato.Core.Components
{
    /// <summary>
    /// Composant pour afficher un sprite sur un GameObject.
    /// </summary>
    public class SpriteRenderer : Component
    {
        // Texture à afficher
        public Texture2D Texture { get; set; }
        
        // Propriétés d'affichage
        public Color Color { get; set; } = Color.White;
        public Vector2 Origin { get; set; } = Vector2.Zero;
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;
        public float LayerDepth { get; set; } = 0f;
        public Rectangle? SourceRectangle { get; set; } = null;
        
        // Décalage par rapport à la position du GameObject
        public Vector2 Offset { get; set; } = Vector2.Zero;
        
        // Utiliser les propriétés de rotation du transform ?
        public bool UseTransformRotation { get; set; } = true;
        
        // Utiliser les propriétés d'échelle du transform ?
        public bool UseTransformScale { get; set; } = true;
        
        public SpriteRenderer() { }
        
        public SpriteRenderer(Texture2D texture)
        {
            Texture = texture;
            
            // Si aucune origine n'est spécifiée, utiliser le centre de la texture par défaut
            if (texture != null)
            {
                Origin = new Vector2(texture.Width / 2, texture.Height / 2);
            }
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Texture == null || !Enabled)
                return;
            
            // Calculer la position finale (position du transform + offset)
            Vector2 position = Transform.Position + Offset;
            
            // Utiliser la rotation du transform ou non
            float rotation = UseTransformRotation ? MathHelper.ToRadians(Transform.Rotation) : 0f;
            
            // Utiliser l'échelle du transform ou non
            Vector2 scale = UseTransformScale ? Transform.ScaleValue : Vector2.One;
            
            // Dessiner le sprite
            spriteBatch.Draw(
                Texture,
                position,
                SourceRectangle,
                Color,
                rotation,
                Origin,
                scale,
                Effects,
                LayerDepth
            );
        }
    }
}