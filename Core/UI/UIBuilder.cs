using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Potato.Core.UI
{
    /// <summary>
    /// UIBuilder : outil de construction d'éléments UI
    /// </summary>
    public static class UIBuilder
    {
        /// <summary>
        /// Crée un bouton avec du texte
        /// </summary>
        public static Button CreateButton(string text, Vector2 position, Vector2? size = null, Action onClick = null)
        {
            // Taille par défaut si non spécifiée
            Vector2 buttonSize = size ?? new Vector2(200, 50);
            
            // Créer le bouton
            var button = new Button(position, buttonSize, text)
            {
                Color = new Color(50, 50, 150),
                HasBorder = true,
                BorderColor = Color.White,
                BorderThickness = 2,
                CornerRadius = 5f,
                Font = UIManager.DefaultFont
            };
            
            // Assigner l'action de clic si fournie
            if (onClick != null)
            {
                button.OnClickAction = onClick;
            }
            
            return button;
        }
        
        /// <summary>
        /// Crée un label avec du texte
        /// </summary>
        public static Label CreateLabel(string text, Vector2 position, Color? color = null, float scale = 1.0f, bool centered = false)
        {
            var label = new Label(position, text)
            {
                Color = color ?? Color.White,
                Scale = scale,
                TextColor = color ?? Color.White
            };
            
            // Centrer le texte si demandé
            if (centered)
            {
                label.HorizontalAlignment = TextAlignment.Center;
                label.VerticalAlignment = TextAlignment.Center;
            }
            
            return label;
        }
        
        /// <summary>
        /// Crée un panneau
        /// </summary>
        public static Panel CreatePanel(Vector2 position, Vector2 size, Color? color = null, bool hasBackground = true)
        {
            Color backgroundColor = color ?? new Color(20, 20, 50, 200);
            var panel = new Panel(position, size, backgroundColor)
            {
                HasBorder = true,
                BorderColor = Color.White,
                BorderThickness = 2,
                CornerRadius = 5f,
                DrawBackground = hasBackground
            };
            
            return panel;
        }
        
        /// <summary>
        /// Crée une barre de progression
        /// </summary>
        public static ProgressBar CreateProgressBar(Vector2 position, Vector2 size, float value = 0f, float maxValue = 100f)
        {
            var progressBar = new ProgressBar(position, size)
            {
                Value = value,
                MaxValue = maxValue,
                BackgroundColor = new Color(50, 50, 50, 200),
                FillColor = new Color(0, 200, 0)
            };
            
            return progressBar;
        }
        
        /// <summary>
        /// Récupère un canvas existant ou en crée un nouveau
        /// </summary>
        public static UICanvas GetOrCreateCanvas(string name)
        {
            var canvas = UIManager.Instance.GetCanvas(name);
            if (canvas == null)
            {
                canvas = UIManager.Instance.CreateCanvas(name);
            }
            
            return canvas;
        }
        
        /// <summary>
        /// Ajoute un élément UI à un canvas
        /// </summary>
        public static T AddToCanvas<T>(T element, string canvasName) where T : UIElement
        {
            var canvas = GetOrCreateCanvas(canvasName);
            canvas.AddElement(element);
            return element;
        }
    }
}