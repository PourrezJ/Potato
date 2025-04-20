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
        private List<UIElement> _rootElements = new List<UIElement>();
        private UIManager _uiManager;
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
            _uiManager = UIManager.Instance;
        }

        public override void Awake()
        {
            base.Awake();
            _uiManager.RegisterCanvas(this);
        }

        public override void OnDestroy()
        {
            _uiManager.UnregisterCanvas(this);
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

            // Important: Commencer un nouveau batch pour les éléments de ce canvas
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            
            try
            {
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
                Logger.Instance.Error($"Erreur pendant le rendu du canvas {Name}: {ex.Message}", LogCategory.UI);
            }
            finally
            {
                // S'assurer que End est toujours appelé, même en cas d'erreur
                spriteBatch.End();
            }
        }

        public List<UIElement> GetRootElements()
        {
            return _rootElements;
        }
    }
}