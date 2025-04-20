using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Potato.Engine
{
    public static class ShapeGenerator
    {
        /// <summary>
        /// Crée une texture rectangulaire de la couleur spécifiée
        /// </summary>
        public static Texture2D CreateRectangle(int width, int height, Color color)
        {            
            GraphicsDevice graphicsDevice = GameManager.Instance.GraphicsDevice;

            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] colorData = new Color[width * height];
            
            for (int i = 0; i < colorData.Length; i++)
            {
                colorData[i] = color;
            }
            
            texture.SetData(colorData);
            return texture;
        }
        
        /// <summary>
        /// Crée une texture circulaire de la couleur spécifiée
        /// </summary>
        public static Texture2D CreateCircle(int radius, Color color)
        {
            GraphicsDevice graphicsDevice = GameManager.Instance.GraphicsDevice;

            int diameter = radius * 2;
            Texture2D texture = new Texture2D(graphicsDevice, diameter, diameter);
            Color[] colorData = new Color[diameter * diameter];
            
            int centerX = radius;
            int centerY = radius;
            
            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    int index = y * diameter + x;
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    
                    if (distance <= radius)
                    {
                        colorData[index] = color;
                    }
                    else
                    {
                        colorData[index] = Color.Transparent;
                    }
                }
            }
            
            texture.SetData(colorData);
            return texture;
        }
        
        /// <summary>
        /// Crée une texture triangulaire de la couleur spécifiée
        /// </summary>
        public static Texture2D CreateTriangle(int width, int height, Color color)
        {            
            GraphicsDevice graphicsDevice = GameManager.Instance.GraphicsDevice;

            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] colorData = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    
                    // Calculate normalized coordinates (0.0 to 1.0)
                    float nx = (float)x / width;
                    float ny = (float)y / height;
                    
                    // Triangle is defined by three points: (0,1), (0.5,0), (1,1)
                    if (ny >= 1 - nx && ny >= nx)
                    {
                        colorData[index] = color;
                    }
                    else
                    {
                        colorData[index] = Color.Transparent;
                    }
                }
            }
            
            texture.SetData(colorData);
            return texture;
        }

        /// <summary>
        /// Dessine un rectangle avec coins arrondis en utilisant une texture pixel
        /// </summary>
        public static void DrawRoundedRectangle(
            SpriteBatch spriteBatch, 
            Texture2D pixel, 
            Rectangle rectangle, 
            Color color, 
            float cornerRadius)
        {
            if (cornerRadius <= 0)
            {
                // Rectangle standard
                spriteBatch.Draw(pixel, rectangle, color);
                return;
            }

            // S'assurer que le rayon de courbure n'est pas trop grand
            cornerRadius = MathHelper.Min(cornerRadius, MathHelper.Min(rectangle.Width / 2, rectangle.Height / 2));
            
            int doubleRadius = (int)(cornerRadius * 2);
            
            // Dessiner le rectangle central
            Rectangle centerRect = new Rectangle(
                rectangle.X + (int)cornerRadius,
                rectangle.Y + (int)cornerRadius,
                rectangle.Width - doubleRadius,
                rectangle.Height - doubleRadius
            );
            spriteBatch.Draw(pixel, centerRect, color);
            
            // Dessiner les rectangles des côtés
            Rectangle topRect = new Rectangle(
                rectangle.X + (int)cornerRadius,
                rectangle.Y,
                rectangle.Width - doubleRadius,
                (int)cornerRadius
            );
            spriteBatch.Draw(pixel, topRect, color);
            
            Rectangle bottomRect = new Rectangle(
                rectangle.X + (int)cornerRadius,
                rectangle.Y + rectangle.Height - (int)cornerRadius,
                rectangle.Width - doubleRadius,
                (int)cornerRadius
            );
            spriteBatch.Draw(pixel, bottomRect, color);
            
            Rectangle leftRect = new Rectangle(
                rectangle.X,
                rectangle.Y + (int)cornerRadius,
                (int)cornerRadius,
                rectangle.Height - doubleRadius
            );
            spriteBatch.Draw(pixel, leftRect, color);
            
            Rectangle rightRect = new Rectangle(
                rectangle.X + rectangle.Width - (int)cornerRadius,
                rectangle.Y + (int)cornerRadius,
                (int)cornerRadius,
                rectangle.Height - doubleRadius
            );
            spriteBatch.Draw(pixel, rightRect, color);
            
            // Dessiner les coins arrondis (simplifiés)
            // Coin supérieur gauche
            DrawCorner(spriteBatch, pixel, new Vector2(rectangle.X + cornerRadius, rectangle.Y + cornerRadius), cornerRadius, color, 180, 270);
            
            // Coin supérieur droit
            DrawCorner(spriteBatch, pixel, new Vector2(rectangle.X + rectangle.Width - cornerRadius, rectangle.Y + cornerRadius), cornerRadius, color, 270, 360);
            
            // Coin inférieur gauche
            DrawCorner(spriteBatch, pixel, new Vector2(rectangle.X + cornerRadius, rectangle.Y + rectangle.Height - cornerRadius), cornerRadius, color, 90, 180);
            
            // Coin inférieur droit
            DrawCorner(spriteBatch, pixel, new Vector2(rectangle.X + rectangle.Width - cornerRadius, rectangle.Y + rectangle.Height - cornerRadius), cornerRadius, color, 0, 90);
        }
        
        /// <summary>
        /// Dessine un coin arrondi (portion d'un cercle) avec une texture pixel
        /// </summary>
        private static void DrawCorner(
            SpriteBatch spriteBatch, 
            Texture2D pixel, 
            Vector2 center, 
            float radius, 
            Color color, 
            float startAngle, 
            float endAngle)
        {
            // Version simplifiée qui dessine un cercle complet au lieu d'une portion
            // Pour une version complète, il faudrait dessiner des segments de ligne pour approximer l'arc
            
            int radiusInt = (int)radius;
            Rectangle cornerRect = new Rectangle(
                (int)(center.X - radius),
                (int)(center.Y - radius),
                radiusInt * 2,
                radiusInt * 2
            );
            
            // Utiliser un cercle simplifié pour les coins
            // Une implémentation plus précise pourrait être ajoutée plus tard
            spriteBatch.Draw(pixel, cornerRect, color);
        }
    }
}