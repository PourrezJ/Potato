using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Potato.Core.Logging;

namespace Potato.Core.UI
{
    public class Button : UIElement
    {
        // Apparence
        public Texture2D BackgroundTexture { get; set; }
        public Texture2D HoverTexture { get; set; }
        public Texture2D PressedTexture { get; set; }
        public Texture2D DisabledTexture { get; set; }
        public new float CornerRadius { get; set; } = 5;
        
        // Texte
        public string Text { get; set; }
        public SpriteFont Font { get; set; }
        public float TextScale { get; set; } = 1.0f;
        public Color TextColor { get; set; } = new Color(240, 240, 240);
        public Color HoverTextColor { get; set; } = new Color(240, 240, 240);
        public Color PressedTextColor { get; set; } = new Color(240, 240, 240);
        public Color DisabledTextColor { get; set; } = Color.DarkGray;
        
        // Styles d'arrière-plan
        public Color BackgroundColor { get; set; } = new Color(60, 60, 60);
        public Color HoverColor { get; set; } = new Color(80, 80, 80);
        public Color PressedColor { get; set; } = new Color(40, 40, 40);
        public Color DisabledColor { get; set; } = new Color(180, 180, 180, 150);
        
        // Icône
        public Texture2D Icon { get; set; }
        public Vector2 IconOffset { get; set; } = Vector2.Zero;
        public float IconScale { get; set; } = 1.0f;
        
        // Animation
        public float HoverScaleEffect { get; set; } = 1.05f;
        public float ClickScaleEffect { get; set; } = 0.95f;
        
        // Son
        public string HoverSoundEffect { get; set; }
        public string ClickSoundEffect { get; set; }
        
        // Tooltips
        public string TooltipText { get; set; }
        private bool _wasHovered = false;
        
        // Action personnalisée
        public Action OnClickAction { get; set; }
        
        public Button(Vector2 position, Vector2 size, string text = "") : base(position, size)
        {
            Text = text;
            _useRenderTarget = true;
            
            // Configurer les valeurs par défaut
            BorderColor = new Color(100, 100, 100);
            HasBorder = true;
            BorderThickness = 1;
            
            // InitializeEvents();
        }
        
        public Button(Vector2 position, Vector2 size, string text, SpriteFont font) : base(position, size)
        {
            Text = text;
            Font = font;
            _useRenderTarget = true;
            
            // Configurer les valeurs par défaut
            BorderColor = new Color(100, 100, 100);
            HasBorder = true;
            BorderThickness = 1;
            
            // InitializeEvents();
        }
        
        // private void InitializeEvents()
        // {
        //     // Configurer les événements
        //     OnClick += (sender) => {
        //         if (IsEnabled && OnClickAction != null)
        //         {
        //             // Jouer le son si défini
        //             // if (!string.IsNullOrEmpty(ClickSoundEffect))
        //             //     SoundManager.PlayEffect(ClickSoundEffect);
                        
        //             OnClickAction();
        //         }
        //     };
            
        //     OnHover += (sender) => {
        //         // Animation de survol
        //         if (IsEnabled)
        //         {
        //             StartTransition("Scale", HoverScaleEffect, 0.1f, EasingFunctions.EaseOutQuad);
                    
        //             // Jouer le son si défini
        //             // if (!string.IsNullOrEmpty(HoverSoundEffect))
        //             //     SoundManager.PlayEffect(HoverSoundEffect);
        //         }
        //     };
            
        //     OnHoverExit += (sender) => {
        //         // Animation de retour à la normale
        //         if (IsEnabled)
        //         {
        //             StartTransition("Scale", 1.0f, 0.1f, EasingFunctions.EaseOutQuad);
        //         }
        //     };
        // }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Gérer l'animation de clic
            if (IsPressed && IsEnabled)
            {
                // Une petite animation d'écrasement lors du clic
                Scale = ClickScaleEffect;
            }
            else if (!IsHovered && IsEnabled)
            {
                // Si nous ne sommes ni pressés ni survolés, retourner à l'échelle normale
                if (Math.Abs(Scale - 1.0f) > 0.01f && !_activeTransitions.ContainsKey("Scale"))
                {
                    StartTransition("Scale", 1.0f, 0.1f, EasingFunctions.EaseOutQuad);
                }
            }
            
            // Gérer l'affichage du tooltip
            if (IsHovered && !_wasHovered && !string.IsNullOrEmpty(TooltipText))
            {
                // Afficher le tooltip
                // TooltipManager.Show(TooltipText, Position + new Vector2(Size.X / 2, -10));
            }
            else if (!IsHovered && _wasHovered && !string.IsNullOrEmpty(TooltipText))
            {
                // Cacher le tooltip
                // TooltipManager.Hide();
            }
            
            _wasHovered = IsHovered;
        }

        public override void UpdateInput(MouseState currentMouseState, MouseState previousMouseState)
        {
            // Ne rien faire ici car la gestion des entrées est maintenant faite par UIManager avec le raycast
            // Cette méthode est surchargée pour empêcher le comportement par défaut
            
            // Nous ne propageons pas non plus aux enfants car UIManager s'en charge
        }
        
        protected override void DrawChildren(SpriteBatch spriteBatch, bool isRenderTarget = false)
        {
            // Déterminer les couleurs actuelles en fonction de l'état du bouton
            Color currentBackgroundColor = GetCurrentBackgroundColor();
            Color currentTextColor = GetCurrentTextColor();
            
            Rectangle bounds = isRenderTarget 
                ? new Rectangle(0, 0, (int)Size.X, (int)Size.Y)
                : Bounds;
            
            // Dessiner l'ombre si activée
            if (HasShadow)
            {
                var shadowBounds = bounds;
                shadowBounds.Offset((int)ShadowOffset.X, (int)ShadowOffset.Y);
                DrawRectangle(spriteBatch, shadowBounds, ShadowColor, CornerRadius);
            }
            
            // Dessiner la bordure et le fond
            DrawButtonBackground(spriteBatch, bounds, currentBackgroundColor);
            
            // Dessiner l'icône et le texte
            DrawIconAndText(spriteBatch, bounds, currentTextColor);
            
            // Appeler la méthode de base pour dessiner les enfants
            base.DrawChildren(spriteBatch, isRenderTarget);
        }

        private Color GetCurrentBackgroundColor()
        {
            if (!IsEnabled)
                return DisabledColor;
            else if (IsPressed)
                return PressedColor;
            else if (IsHovered)
                return HoverColor;
            else
                return BackgroundColor;
        }
        
        private Color GetCurrentTextColor()
        {
            if (!IsEnabled)
                return DisabledTextColor;
            else if (IsPressed)
                return PressedTextColor;
            else if (IsHovered)
                return HoverTextColor;
            else
                return TextColor;
        }
        
        private void DrawButtonBackground(SpriteBatch spriteBatch, Rectangle bounds, Color backgroundColor)
        {
            // S'assurer que UIManager est initialisé avant d'utiliser Pixel
            if (UIManager.Pixel == null)
                return;
            
            // Dessiner la bordure si activée
            if (HasBorder)
            {
                DrawRectangle(spriteBatch, bounds, BorderColor, CornerRadius);
                
                // Réduire la taille des limites pour le contenu intérieur
                bounds.Inflate(-BorderThickness, -BorderThickness);
            }
            
            // Dessiner le fond
            DrawRectangle(spriteBatch, bounds, backgroundColor, CornerRadius);
            
            // Dessiner la texture d'arrière-plan si disponible
            Texture2D currentTexture = GetCurrentBackgroundTexture();
                
            if (currentTexture != null)
            {
                spriteBatch.Draw(
                    currentTexture,
                    bounds,
                    null,
                    Color.White * Opacity,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        private Texture2D GetCurrentBackgroundTexture()
        {
            if (!IsEnabled && DisabledTexture != null)
                return DisabledTexture;
            else if (IsPressed && PressedTexture != null)
                return PressedTexture;
            else if (IsHovered && HoverTexture != null)
                return HoverTexture;
            else
                return BackgroundTexture;
        }
        
        private void DrawIconAndText(SpriteBatch spriteBatch, Rectangle bounds, Color textColor)
        {
            // Vérifier si le texte doit être affiché
            if (string.IsNullOrEmpty(Text))
                return;
                
            if (Font == null)
            {
                Font = UIManager.DefaultFont;
            }
            
            // Dessiner l'icône si disponible
            if (Icon != null)
            {
                Vector2 iconPosition;
                
                if (string.IsNullOrEmpty(Text))
                {
                    // Centrer l'icône si pas de texte
                    iconPosition = new Vector2(
                        bounds.X + bounds.Width / 2 - (Icon.Width * IconScale) / 2,
                        bounds.Y + bounds.Height / 2 - (Icon.Height * IconScale) / 2
                    );
                }
                else
                {
                    // Placer l'icône à gauche du texte
                    iconPosition = new Vector2(
                        bounds.X + Padding.Left + IconOffset.X,
                        bounds.Y + bounds.Height / 2 - (Icon.Height * IconScale) / 2 + IconOffset.Y
                    );
                }
                
                spriteBatch.Draw(
                    Icon,
                    iconPosition,
                    null,
                    Color.White * Opacity,
                    0f,
                    Vector2.Zero,
                    IconScale,
                    SpriteEffects.None,
                    0f
                );
            }
            
            // Dessiner le texte
            try 
            {
                Vector2 textSize = Font.MeasureString(Text) * TextScale;
                
                // Calculer la position du texte (centré)
                float textX = bounds.X + bounds.Width / 2 - textSize.X / 2;
                float textY = bounds.Y + bounds.Height / 2 - textSize.Y / 2;
                
                // Ajuster si une icône est présente
                if (Icon != null)
                {
                    textX += (Icon.Width * IconScale) / 2 + 5; // 5 = marge entre l'icône et le texte
                }
             
                spriteBatch.DrawString(
                    Font,
                    Text,
                    new Vector2(textX, textY),
                    textColor * Opacity,
                    0f,
                    Vector2.Zero,
                    TextScale,
                    SpriteEffects.None,
                    0f
                );
            }
            catch (Exception ex)
            {
                Logger.Error($"Erreur lors du dessin du texte '{Text}': {ex.Message}", LogCategory.UI);
            }
        }
        
        private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, float cornerRadius)
        {
            // Utiliser le pixel du UIManager au lieu d'en créer un nouveau
            Texture2D pixel = UIManager.Pixel;
            
            if (cornerRadius <= 0 || pixel == null)
            {
                // Rectangle standard
                spriteBatch.Draw(pixel, rectangle, color * Opacity);
            }
            else
            {
                // Tentative d'utiliser ShapeGenerator pour les coins arrondis s'il est disponible
                try
                {
                    Engine.ShapeGenerator.DrawRoundedRectangle(spriteBatch, pixel, rectangle, color * Opacity, cornerRadius);
                }
                catch
                {
                    // Fallback en cas d'échec : dessiner un rectangle standard
                    spriteBatch.Draw(pixel, rectangle, color * Opacity);
                    
                    // Log une fois seulement
                    if (cornerRadius > 0)
                    {
                        Logger.Debug("Utilisation du fallback pour dessiner un rectangle aux coins arrondis", LogCategory.UI);
                    }
                }
            }
        }
    }
}