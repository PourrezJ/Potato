using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core.Logging;

namespace Potato.Core
{
    /// <summary>
    /// Gestionnaire central qui maintient tous les GameObjects actifs et gère leur cycle de vie.
    /// </summary>
    public static class GameObjectManager
    {
        // Liste des GameObjects actifs
        private static readonly List<GameObject> _gameObjects = new List<GameObject>();
        
        // Listes pour gérer l'ajout et la suppression en toute sécurité
        private static readonly List<GameObject> _pendingAddition = new List<GameObject>();
        private static readonly List<GameObject> _pendingRemoval = new List<GameObject>();
        
        // Flag pour empêcher les modifications pendant l'itération
        private static bool _isProcessingObjects = false;
        
        /// <summary>
        /// Initialise le gestionnaire
        /// </summary>
        public static void Initialize()
        {
            _gameObjects.Clear();
            _pendingAddition.Clear();
            _pendingRemoval.Clear();
            _isProcessingObjects = false;
            
            Logger.Instance.Info("GameObjectManager initialisé", LogCategory.Core);
        }
        
        /// <summary>
        /// Enregistre un nouveau GameObject
        /// </summary>
        public static void RegisterGameObject(GameObject gameObject)
        {
            if (gameObject == null)
                return;
                
            if (_isProcessingObjects)
            {
                // Si nous sommes en train d'itérer, ajouter à la liste d'attente
                if (!_pendingAddition.Contains(gameObject))
                {
                    _pendingAddition.Add(gameObject);
                }
            }
            else
            {
                // Sinon, ajouter directement
                if (!_gameObjects.Contains(gameObject))
                {
                    _gameObjects.Add(gameObject);
                    
                    // Réveiller l'objet (appeler Awake et Start)
                    gameObject.Awake();
                    gameObject.Start();
                }
            }
        }
        
        /// <summary>
        /// Désenregistre un GameObject
        /// </summary>
        public static void UnregisterGameObject(GameObject gameObject)
        {
            if (gameObject == null)
                return;
                
            if (_isProcessingObjects)
            {
                // Si nous sommes en train d'itérer, ajouter à la liste d'attente
                if (!_pendingRemoval.Contains(gameObject))
                {
                    _pendingRemoval.Add(gameObject);
                }
            }
            else
            {
                // Sinon, supprimer directement
                _gameObjects.Remove(gameObject);
            }
        }
        
        /// <summary>
        /// Met à jour tous les GameObjects actifs
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            ProcessPendingLists();
            
            _isProcessingObjects = true;
            
            // Mettre à jour tous les objets
            foreach (var gameObject in _gameObjects.ToList())
            {
                try
                {
                    if (!gameObject.IsDestroyed && gameObject.IsActive)
                    {
                        gameObject.Update(gameTime);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"Erreur lors de la mise à jour de {gameObject.Name}: {ex.Message}", LogCategory.Core);
                }
            }
            
            _isProcessingObjects = false;
            
            ProcessPendingLists();
        }
        
        /// <summary>
        /// Dessine tous les GameObjects actifs
        /// </summary>
        public static void Draw(SpriteBatch spriteBatch)
        {
            _isProcessingObjects = true;
            
            // Dessiner tous les objets
            foreach (var gameObject in _gameObjects)
            {
                try
                {
                    if (!gameObject.IsDestroyed && gameObject.IsActive)
                    {
                        gameObject.Draw(spriteBatch);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"Erreur lors du dessin de {gameObject.Name}: {ex.Message}", LogCategory.Core);
                }
            }
            
            _isProcessingObjects = false;
            
            ProcessPendingLists();
        }
        
        /// <summary>
        /// Trouve un GameObject par son nom
        /// </summary>
        public static GameObject Find(string name)
        {
            return _gameObjects.FirstOrDefault(go => go.Name == name && !go.IsDestroyed);
        }
        
        /// <summary>
        /// Trouve tous les GameObjects avec le tag spécifié
        /// </summary>
        public static IEnumerable<GameObject> FindGameObjectsWithTag(string tag)
        {
            return _gameObjects.Where(go => go.Tag == tag && !go.IsDestroyed);
        }
        
        /// <summary>
        /// Détruit tous les GameObjects
        /// </summary>
        public static void DestroyAll()
        {
            foreach (var gameObject in _gameObjects.ToList())
            {
                gameObject.Destroy();
            }
            
            _gameObjects.Clear();
            _pendingAddition.Clear();
            _pendingRemoval.Clear();
        }
        
        /// <summary>
        /// Traite les listes d'attente
        /// </summary>
        private static void ProcessPendingLists()
        {
            // Ajouter les objets en attente
            foreach (var gameObject in _pendingAddition)
            {
                if (!_gameObjects.Contains(gameObject))
                {
                    _gameObjects.Add(gameObject);
                    
                    // Réveiller l'objet (appeler Awake et Start)
                    gameObject.Awake();
                    gameObject.Start();
                }
            }
            _pendingAddition.Clear();
            
            // Supprimer les objets en attente
            foreach (var gameObject in _pendingRemoval)
            {
                _gameObjects.Remove(gameObject);
            }
            _pendingRemoval.Clear();
        }
    }
}