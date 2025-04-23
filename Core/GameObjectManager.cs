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
        
        // Dictionnaires pour accélérer l'itération par scène
        private static readonly Dictionary<Scene, List<GameObject>> _gameObjectsByScene = new Dictionary<Scene, List<GameObject>>();
        private static readonly List<GameObject> _persistentObjects = new List<GameObject>();
        
        // Listes pour gérer l'ajout et la suppression en toute sécurité
        private static readonly List<GameObject> _pendingAddition = new List<GameObject>();
        private static readonly List<GameObject> _pendingRemoval = new List<GameObject>();
        
        // Cache des objets actifs pour éviter de recréer des listes pendant l'itération
        private static readonly List<GameObject> _activeObjectsCache = new List<GameObject>();
        
        // Flag pour empêcher les modifications pendant l'itération
        private static bool _isProcessingObjects = false;
        
        /// <summary>
        /// Initialise le gestionnaire
        /// </summary>
        public static void Initialize()
        {
            _gameObjects.Clear();
            _gameObjectsByScene.Clear();
            _persistentObjects.Clear();
            _pendingAddition.Clear();
            _pendingRemoval.Clear();
            _activeObjectsCache.Clear();
            _isProcessingObjects = false;
            
            Logger.Info("GameObjectManager initialisé", LogCategory.Core);
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
                    AddGameObjectToCollections(gameObject);
                    
                    // Réveiller l'objet (appeler Awake et Start)
                    gameObject.Awake();
                    gameObject.Start();
                }
            }
        }
        
        /// <summary>
        /// Ajoute un GameObject aux collections appropriées
        /// </summary>
        private static void AddGameObjectToCollections(GameObject gameObject)
        {
            _gameObjects.Add(gameObject);
            
            // Aussi ajouter à la collection par scène
            if (Scene.IsPersistent(gameObject))
            {
                _persistentObjects.Add(gameObject);
            }
            else if (gameObject.Scene != null)
            {
                if (!_gameObjectsByScene.TryGetValue(gameObject.Scene, out var sceneObjects))
                {
                    sceneObjects = new List<GameObject>();
                    _gameObjectsByScene[gameObject.Scene] = sceneObjects;
                }
                sceneObjects.Add(gameObject);
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
                RemoveGameObjectFromCollections(gameObject);
            }
        }
        
        /// <summary>
        /// Supprime un GameObject de toutes les collections
        /// </summary>
        private static void RemoveGameObjectFromCollections(GameObject gameObject)
        {
            _gameObjects.Remove(gameObject);
            
            if (Scene.IsPersistent(gameObject))
            {
                _persistentObjects.Remove(gameObject);
            }
            else if (gameObject.Scene != null && _gameObjectsByScene.TryGetValue(gameObject.Scene, out var sceneObjects))
            {
                sceneObjects.Remove(gameObject);
                
                // Nettoyer les listes vides
                if (sceneObjects.Count == 0)
                {
                    _gameObjectsByScene.Remove(gameObject.Scene);
                }
            }
        }
        
        /// <summary>
        /// Met à jour tous les GameObjects actifs
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            ProcessPendingLists();
            
            _isProcessingObjects = true;
            
            // Préparer la liste des objets à mettre à jour
            PrepareActiveObjectsList();
            
            // Mettre à jour tous les objets actifs
            foreach (var gameObject in _activeObjectsCache)
            {
                try
                {
                    gameObject.Update(gameTime);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Erreur lors de la mise à jour de {gameObject.Name}: {ex.Message}", LogCategory.Core);
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
            
            // Utiliser la même liste d'objets actifs que pour Update
            foreach (var gameObject in _activeObjectsCache)
            {
                try
                {
                    gameObject.Draw(spriteBatch);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Erreur lors du dessin de {gameObject.Name}: {ex.Message}", LogCategory.Core);
                }
            }
            
            _isProcessingObjects = false;
            
            ProcessPendingLists();
        }
        
        /// <summary>
        /// Prépare la liste des objets actifs pour l'itération
        /// </summary>
        private static void PrepareActiveObjectsList()
        {
            _activeObjectsCache.Clear();
            
            // Ajouter les objets de la scène active
            Scene activeScene = SceneManager.ActiveScene;
            if (activeScene != null && _gameObjectsByScene.TryGetValue(activeScene, out var sceneObjects))
            {
                foreach (var obj in sceneObjects)
                {
                    if (!obj.IsDestroyed && obj.IsActive)
                    {
                        _activeObjectsCache.Add(obj);
                    }
                }
            }
            
            // Ajouter les objets persistants
            foreach (var obj in _persistentObjects)
            {
                if (!obj.IsDestroyed && obj.IsActive)
                {
                    _activeObjectsCache.Add(obj);
                }
            }
        }
        
        /// <summary>
        /// Trouve un GameObject par son nom
        /// </summary>
        public static GameObject Find(string name)
        {
            // Chercher d'abord dans la scène active
            var activeScene = SceneManager.ActiveScene;
            if (activeScene != null)
            {
                var obj = activeScene.FindGameObjectByName(name);
                if (obj != null)
                    return obj;
            }
            
            // Sinon chercher dans tous les objets (y compris les persistants)
            return _gameObjects.FirstOrDefault(go => go.Name == name && !go.IsDestroyed);
        }
        
        /// <summary>
        /// Trouve tous les GameObjects avec le tag spécifié
        /// </summary>
        public static IEnumerable<GameObject> FindGameObjectsWithTag(string tag)
        {
            // Chercher d'abord dans la scène active
            var activeScene = SceneManager.ActiveScene;
            if (activeScene != null)
            {
                var objs = activeScene.FindGameObjectsWithTag(tag);
                if (objs.Count > 0)
                    return objs;
            }
            
            // Sinon chercher dans tous les objets (y compris les persistants)
            return _gameObjects.Where(go => go.Tag == tag && !go.IsDestroyed);
        }
        
        /// <summary>
        /// Détruit tous les GameObjects non-persistants
        /// </summary>
        public static void DestroyAll(bool includePersistent = false)
        {
            foreach (var gameObject in _gameObjects.ToList())
            {
                if (includePersistent || !Scene.IsPersistent(gameObject))
                {
                    gameObject.Destroy();
                }
            }
            
            ProcessPendingLists();
        }
        
        /// <summary>
        /// Signale qu'un GameObject a changé de scène
        /// </summary>
        public static void NotifySceneChanged(GameObject gameObject, Scene oldScene, Scene newScene)
        {
            if (_isProcessingObjects)
            {
                // Reporter la mise à jour après l'itération
                _pendingRemoval.Add(gameObject);
                _pendingAddition.Add(gameObject);
                return;
            }
            
            // Supprimer de l'ancienne collection
            if (oldScene != null && _gameObjectsByScene.TryGetValue(oldScene, out var oldSceneObjects))
            {
                oldSceneObjects.Remove(gameObject);
                
                // Nettoyer les listes vides
                if (oldSceneObjects.Count == 0)
                {
                    _gameObjectsByScene.Remove(oldScene);
                }
            }
            else if (oldScene == null && Scene.IsPersistent(gameObject))
            {
                _persistentObjects.Remove(gameObject);
            }
            
            // Ajouter à la nouvelle collection
            if (Scene.IsPersistent(gameObject))
            {
                _persistentObjects.Add(gameObject);
            }
            else if (newScene != null)
            {
                if (!_gameObjectsByScene.TryGetValue(newScene, out var newSceneObjects))
                {
                    newSceneObjects = new List<GameObject>();
                    _gameObjectsByScene[newScene] = newSceneObjects;
                }
                newSceneObjects.Add(gameObject);
            }
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
                    AddGameObjectToCollections(gameObject);
                    
                    // Réveiller l'objet (appeler Awake et Start)
                    gameObject.Awake();
                    gameObject.Start();
                }
            }
            _pendingAddition.Clear();
            
            // Supprimer les objets en attente
            foreach (var gameObject in _pendingRemoval)
            {
                RemoveGameObjectFromCollections(gameObject);
            }
            _pendingRemoval.Clear();
        }
        
        /// <summary>
        /// Obtient le nombre total d'objets actifs gérés
        /// </summary>
        public static int Count => _gameObjects.Count;
        
        /// <summary>
        /// Obtient le nombre d'objets dans la scène active
        /// </summary>
        public static int ActiveSceneObjectCount
        {
            get
            {
                var activeScene = SceneManager.ActiveScene;
                if (activeScene != null && _gameObjectsByScene.TryGetValue(activeScene, out var sceneObjects))
                {
                    return sceneObjects.Count;
                }
                return 0;
            }
        }
    }
}