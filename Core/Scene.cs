using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core.Logging;

namespace Potato.Core
{
    /// <summary>
    /// Représente une scène dans le jeu, servant de conteneur pour les GameObjects.
    /// </summary>
    public class Scene
    {
        // Nom et état de la scène
        private string _name;
        private bool _isLoaded = false;
        private bool _isActive = false;
        
        // Liste des objets appartenant à cette scène
        private List<GameObject> _sceneObjects = new List<GameObject>();
        
        // Liste des objets marqués comme persistants entre les scènes
        private static List<GameObject> _persistentObjects = new List<GameObject>();
        
        // Événements pour les transitions de scène
        public event Action OnSceneLoaded;
        public event Action OnSceneUnloaded;
        public event Action<GameTime> OnSceneUpdate;
        public event Action<SpriteBatch> OnSceneDraw;
        
        // Propriétés publiques
        public string Name => _name;
        public bool IsLoaded => _isLoaded;
        public bool IsActive => _isActive;
        public IReadOnlyList<GameObject> SceneObjects => _sceneObjects.AsReadOnly();
        public static IReadOnlyList<GameObject> PersistentObjects => _persistentObjects.AsReadOnly();
        
        public Scene(string name)
        {
            _name = name;
        }
        
        /// <summary>
        /// Charge la scène et initialise ses objets
        /// </summary>
        public virtual void Load()
        {
            if (_isLoaded)
                return;
                
            // Initialiser l'état
            _isLoaded = true;
            Logger.Instance.Info($"Scene '{_name}' loaded", LogCategory.Core);
            
            // Déclencher l'événement
            OnSceneLoaded?.Invoke();
        }
        
        /// <summary>
        /// Active la scène
        /// </summary>
        public virtual void Activate()
        {
            if (!_isLoaded)
            {
                Load();
            }
            
            _isActive = true;
            Logger.Instance.Info($"Scene '{_name}' activated", LogCategory.Core);
        }
        
        /// <summary>
        /// Désactive la scène sans la décharger complètement
        /// </summary>
        public virtual void Deactivate()
        {
            _isActive = false;
            Logger.Instance.Info($"Scene '{_name}' deactivated", LogCategory.Core);
        }
        
        /// <summary>
        /// Décharge la scène et nettoie ses ressources
        /// </summary>
        public virtual void Unload()
        {
            if (!_isLoaded)
                return;
            
            // Désactiver d'abord
            Deactivate();
            
            // Détruire tous les GameObjects spécifiques à cette scène
            foreach (var obj in _sceneObjects.ToArray())
            {
                if (!IsPersistent(obj))
                {
                    obj.Destroy();
                }
            }
            
            _sceneObjects.Clear();
            _isLoaded = false;
            
            // Déclencher l'événement
            OnSceneUnloaded?.Invoke();
            
            Logger.Instance.Info($"Scene '{_name}' unloaded", LogCategory.Core);
        }
        
        /// <summary>
        /// Enregistre un GameObject comme appartenant à cette scène
        /// </summary>
        public void RegisterGameObject(GameObject gameObject)
        {
            if (!_sceneObjects.Contains(gameObject))
            {
                _sceneObjects.Add(gameObject);
            }
        }
        
        /// <summary>
        /// Désenregistre un GameObject de cette scène
        /// </summary>
        public void UnregisterGameObject(GameObject gameObject)
        {
            _sceneObjects.Remove(gameObject);
        }
        
        /// <summary>
        /// Marque un GameObject comme devant persister entre les scènes
        /// </summary>
        public static void MarkAsPersistent(GameObject gameObject)
        {
            if (gameObject != null && !_persistentObjects.Contains(gameObject))
            {
                _persistentObjects.Add(gameObject);
                Logger.Instance.Debug($"GameObject '{gameObject.Name}' marked as persistent", LogCategory.Core);
            }
        }
        
        /// <summary>
        /// Retire la marque de persistance d'un GameObject
        /// </summary>
        public static void UnmarkAsPersistent(GameObject gameObject)
        {
            if (gameObject != null && _persistentObjects.Contains(gameObject))
            {
                _persistentObjects.Remove(gameObject);
                Logger.Instance.Debug($"GameObject '{gameObject.Name}' no longer persistent", LogCategory.Core);
            }
        }
        
        /// <summary>
        /// Vérifie si un GameObject est persistant
        /// </summary>
        public static bool IsPersistent(GameObject gameObject)
        {
            return gameObject != null && _persistentObjects.Contains(gameObject);
        }
        
        /// <summary>
        /// Met à jour la scène et ses objets
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
            if (!_isActive)
                return;
            
            // Laisser les sous-classes implémenter leurs propres logiques
            OnSceneUpdate?.Invoke(gameTime);
        }
        
        /// <summary>
        /// Dessine la scène et ses objets
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!_isActive)
                return;
            
            // Laisser les sous-classes implémenter leurs propres logiques
            OnSceneDraw?.Invoke(spriteBatch);
        }
        
        /// <summary>
        /// Crée un nouveau GameObject attaché à cette scène
        /// </summary>
        public GameObject CreateGameObject(string name = "GameObject")
        {
            return new GameObject(name, this);
        }
        
        /// <summary>
        /// Trouve un GameObject par son nom dans cette scène
        /// </summary>
        public GameObject FindGameObjectByName(string name)
        {
            return _sceneObjects.FirstOrDefault(obj => obj.Name == name && !obj.IsDestroyed);
        }
        
        /// <summary>
        /// Trouve tous les GameObjects avec le tag spécifié dans cette scène
        /// </summary>
        public List<GameObject> FindGameObjectsWithTag(string tag)
        {
            return _sceneObjects.Where(obj => obj.Tag == tag && !obj.IsDestroyed).ToList();
        }
    }
}