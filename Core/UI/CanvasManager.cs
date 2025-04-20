using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Potato.Core.Logging;

namespace Potato.Core.UI
{
    /// <summary>
    /// Gestionnaire simplifié des canvas du jeu
    /// </summary>
    public class CanvasManager : Singleton<CanvasManager>
    {
        // Écrans actuellement visibles
        private List<string> _visibleCanvases = new List<string>();
        
        public CanvasManager()
        {
            // Constructeur vide
        }
        
        /// <summary>
        /// Affiche un canvas spécifique
        /// </summary>
        public void ShowCanvas(string canvasName)
        {
            // Vérifier si le canvas existe, sinon le créer
            var canvas = UIManager.GetCanvas(canvasName);
            if (canvas == null)
            {
                Logger.Instance.Warning($"Le canvas '{canvasName}' n'existe pas et ne peut pas être affiché", LogCategory.UI);
                return;
            }
            
            // Afficher le canvas
            UIManager.ShowCanvas(canvasName);
            
            // Ajouter à la liste des canvas visibles s'il n'y est pas déjà
            if (!_visibleCanvases.Contains(canvasName))
            {
                _visibleCanvases.Add(canvasName);
                Logger.Instance.Debug($"Canvas '{canvasName}' marqué comme visible", LogCategory.UI);
            }
        }
        
        /// <summary>
        /// Masque un canvas spécifique
        /// </summary>
        public void HideCanvas(string canvasName)
        {
            // Masquer le canvas
            UIManager.HideCanvas(canvasName);
            
            // Retirer de la liste des canvas visibles
            if (_visibleCanvases.Contains(canvasName))
            {
                _visibleCanvases.Remove(canvasName);
                Logger.Instance.Debug($"Canvas '{canvasName}' masqué", LogCategory.UI);
            }
        }
        
        /// <summary>
        /// Masque tous les canvas
        /// </summary>
        public void HideAllCanvases()
        {
            // Copier la liste pour éviter de modifier la collection pendant l'itération
            List<string> canvasesCopy = new List<string>(_visibleCanvases);
            
            foreach (var canvasName in canvasesCopy)
            {
                HideCanvas(canvasName);
            }
            
            Logger.Instance.Info("Tous les canvas ont été masqués", LogCategory.UI);
        }
        
        /// <summary>
        /// Masque tous les canvas sauf celui spécifié
        /// </summary>
        public void ShowOnlyCanvas(string canvasName)
        {
            // Copier la liste pour éviter de modifier la collection pendant l'itération
            List<string> canvasesCopy = new List<string>(_visibleCanvases);
            
            foreach (var name in canvasesCopy)
            {
                if (name != canvasName)
                {
                    HideCanvas(name);
                }
            }
            
            // S'assurer que le canvas spécifié est visible
            ShowCanvas(canvasName);
            
            Logger.Instance.Info($"Seul le canvas '{canvasName}' est maintenant visible", LogCategory.UI);
        }
        
        /// <summary>
        /// Vérifie si un canvas est visible
        /// </summary>
        public bool IsCanvasVisible(string canvasName)
        {
            return _visibleCanvases.Contains(canvasName);
        }
        
        /// <summary>
        /// Récupère la liste des canvas visibles
        /// </summary>
        public List<string> GetVisibleCanvases()
        {
            return new List<string>(_visibleCanvases);
        }
    }
}