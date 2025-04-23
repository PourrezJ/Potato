using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core.Attributes;
using Potato.Core.Logging;

namespace Potato.Core.UI
{
    /// <summary>
    /// Représente un canvas d'interface utilisateur - un conteneur pour les éléments UI
    /// avec un rendu et une gestion des événements automatisés
    /// </summary>
    [ExecutionOrder(ExecutionOrderAttribute.EarlyPriority)]
    public class UICanvas : GameBehaviour
    {
        protected List<UIElement> _rootElements = new List<UIElement>();
        private bool _isVisible = true;
        private string _name;

        public string Name => _name;
        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }

        /// <summary>
        /// Constructeur par défaut - nécessaire pour l'auto-découverte
        /// </summary>
        public UICanvas() : this("Canvas")
        {
            // Appelle le constructeur avec paramètre par défaut
        }

        public UICanvas(string name = "Canvas")
        {
            _name = name;
            // Ne pas initialiser _uiManager ici, le faire dans Awake() pour éviter les problèmes de timing
        }

        public override void Awake()
        {
            base.Awake();
            try
            {
                UIManager.RegisterCanvas(this);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error during UICanvas Awake: {ex.Message}", LogCategory.UI);
            }
        }

        public override void OnDestroy()
        {
                UIManager.UnregisterCanvas(this);

            ClearElements();
            base.OnDestroy();
        }

        public void AddElement(UIElement element)
        {
            if (!_rootElements.Contains(element))
            {
                _rootElements.Add(element);
                element.Canvas = this;
            }
        }

        public void RemoveElement(UIElement element)
        {
            if (_rootElements.Contains(element))
            {
                _rootElements.Remove(element);
                element.Canvas = null;
            }
        }

        public void ClearElements()
        {
            foreach (var element in _rootElements)
            {
                element.Canvas = null;
            }
            _rootElements.Clear();
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsVisible)
                return;

            foreach (var element in _rootElements)
            {
                if (element != null && element.IsVisible)
                {
                    element.Update(gameTime);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
                return;

            // Important: Commencer un nouveau batch pour les éléments de ce canvas avec les paramètres appropriés
            try
            {
                if (spriteBatch == null || spriteBatch.GraphicsDevice == null)
                {
                    Logger.Error($"SpriteBatch invalide pour le canvas {Name}", LogCategory.UI);
                    return;
                }
                
                // Vérifier que UIManager.Pixel est bien initialisé
                if (UIManager.Pixel == null)
                {
                    Logger.Warning($"UIManager.Pixel est null lors du rendu du canvas {Name}, tentative d'initialisation", LogCategory.UI);
                    UIManager.Initialize();
                    if (UIManager.Pixel == null)
                    {
                        Logger.Error($"Échec de l'initialisation d'UIManager.Pixel pour {Name}", LogCategory.UI);
                        return;
                    }
                }

                spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone
                );
                
                // Dessiner explicitement un fond pour déboguer
                if (Name == "PlayerSelection")
                {
                    // Dessiner un rectangle visible pour voir si le canvas est rendu
                    Rectangle screenRect = new Rectangle(0, 0, 
                        spriteBatch.GraphicsDevice.Viewport.Width, 
                        spriteBatch.GraphicsDevice.Viewport.Height);
                    spriteBatch.Draw(UIManager.Pixel, screenRect, new Color(50, 0, 50, 128));
                    
                    Logger.Debug("Rendu du fond du PlayerSelectionCanvas", LogCategory.UI);
                }
                
                // Dessiner tous les éléments racines
                foreach (var element in _rootElements)
                {
                    if (element != null && element.IsVisible)
                    {
                        element.Draw(spriteBatch);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Erreur pendant le rendu du canvas {Name}: {ex.Message}", LogCategory.UI);
            }
            finally
            {
                try {
                    // S'assurer que End est toujours appelé, même en cas d'erreur
                    spriteBatch.End();
                }
                catch (Exception ex) {
                    Logger.Error($"Erreur lors de la fin du SpriteBatch dans {Name}: {ex.Message}", LogCategory.UI);
                }
            }
        }

        public List<UIElement> GetRootElements()
        {
            return _rootElements;
        }
    }
}