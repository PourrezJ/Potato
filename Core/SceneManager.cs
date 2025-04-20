using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core.Logging;

namespace Potato.Core
{
    /// <summary>
    /// Gestionnaire central des scènes du jeu
    /// </summary>
    public static class SceneManager
    {
        // Collection des scènes disponibles
        private static Dictionary<string, Scene> _scenes = new Dictionary<string, Scene>();
        
        // Scènes active et suivante
        private static Scene _activeScene;
        private static Scene _nextScene;
        
        // État de transition
        private static bool _isTransitioning = false;
        private static float _transitionProgress = 0f;
        private static float _transitionDuration = 0.5f;  // Durée par défaut en secondes
        
        // Mode de chargement asynchrone
        private static bool _loadAsync = false;
        private static Task _loadingTask = null;
        
        // Événements
        public static event Action<Scene, Scene> OnSceneTransitionStarted;
        public static event Action<Scene> OnSceneLoaded;
        public static event Action<Scene> OnSceneUnloaded;
        
        // Propriétés publiques
        public static Scene ActiveScene => _activeScene;
        public static bool IsTransitioning => _isTransitioning;
        public static float TransitionProgress => _transitionProgress;

        /// <summary>
        /// Initialise le gestionnaire de scènes
        /// </summary>
        public static void Initialize()
        {
            _scenes.Clear();
            _activeScene = null;
            _nextScene = null;
            _isTransitioning = false;
            
            Logger.Instance.Info("SceneManager initialized", LogCategory.Core);
        }

        /// <summary>
        /// Enregistre une scène dans le gestionnaire
        /// </summary>
        public static void RegisterScene(Scene scene)
        {
            if (scene == null)
                return;
                
            if (!_scenes.ContainsKey(scene.Name))
            {
                _scenes.Add(scene.Name, scene);
                Logger.Instance.Debug($"Scene '{scene.Name}' registered", LogCategory.Core);
            }
            else
            {
                Logger.Instance.Warning($"Scene '{scene.Name}' already registered", LogCategory.Core);
            }
        }
        
        /// <summary>
        /// Désenregistre une scène du gestionnaire
        /// </summary>
        public static void UnregisterScene(string sceneName)
        {
            if (_scenes.ContainsKey(sceneName))
            {
                if (_scenes[sceneName].IsLoaded)
                {
                    _scenes[sceneName].Unload();
                }
                
                _scenes.Remove(sceneName);
                Logger.Instance.Debug($"Scene '{sceneName}' unregistered", LogCategory.Core);
            }
        }
        
        /// <summary>
        /// Charge et active une scène par son nom
        /// </summary>
        public static void LoadScene(string sceneName, float transitionDuration = 0.5f, bool loadAsync = false)
        {
            if (!_scenes.ContainsKey(sceneName))
            {
                Logger.Instance.Error($"Scene '{sceneName}' not found", LogCategory.Core);
                return;
            }
            
            // Si une transition est déjà en cours, l'annuler
            if (_isTransitioning)
            {
                CancelTransition();
            }
            
            // Configurer la nouvelle transition
            _nextScene = _scenes[sceneName];
            _transitionDuration = transitionDuration;
            _transitionProgress = 0f;
            _isTransitioning = true;
            _loadAsync = loadAsync;
            
            // Notifier les écouteurs
            OnSceneTransitionStarted?.Invoke(_activeScene, _nextScene);
            
            Logger.Instance.Info($"Transitioning to scene '{sceneName}'", LogCategory.Core);
            
            if (transitionDuration <= 0)
            {
                // Transition immédiate
                FinishSceneTransition();
            }
        }
        
        /// <summary>
        /// Obtient une scène par son nom
        /// </summary>
        public static Scene GetScene(string sceneName)
        {
            if (_scenes.ContainsKey(sceneName))
            {
                return _scenes[sceneName];
            }
            
            return null;
        }
        
        /// <summary>
        /// Mise à jour du gestionnaire de scènes
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Gérer la transition entre scènes
            if (_isTransitioning)
            {
                UpdateSceneTransition(deltaTime);
            }
            
            // Mettre à jour la scène active
            _activeScene?.Update(gameTime);
        }
        
        /// <summary>
        /// Mise à jour de la transition entre scènes
        /// </summary>
        private static void UpdateSceneTransition(float deltaTime)
        {
            // Si le chargement asynchrone est terminé ou n'a pas été demandé
            if (_loadingTask == null || _loadingTask.IsCompleted)
            {
                // Avancer la progression
                _transitionProgress += deltaTime / _transitionDuration;
                
                // Vérifier si la transition est terminée
                if (_transitionProgress >= 1.0f)
                {
                    FinishSceneTransition();
                }
            }
        }
        
        /// <summary>
        /// Charge la nouvelle scène de façon asynchrone
        /// </summary>
        private static void StartAsyncLoading()
        {
            if (_nextScene != null && _loadAsync)
            {
                _loadingTask = Task.Run(() => 
                {
                    // Pré-charger la scène
                    if (!_nextScene.IsLoaded)
                    {
                        _nextScene.Load();
                    }
                    
                    // Notifier que le chargement est terminé
                    OnSceneLoaded?.Invoke(_nextScene);
                });
            }
        }
        
        /// <summary>
        /// Termine la transition entre scènes
        /// </summary>
        private static void FinishSceneTransition()
        {
            if (_nextScene == null)
            {
                _isTransitioning = false;
                return;
            }
            
            // Décharger l'ancienne scène
            if (_activeScene != null)
            {
                _activeScene.Deactivate();
                
                // Si l'ancienne scène n'est pas la même que la nouvelle
                if (_activeScene != _nextScene)
                {
                    _activeScene.Unload();
                    OnSceneUnloaded?.Invoke(_activeScene);
                }
            }
            
            // Activer la nouvelle scène
            if (!_nextScene.IsLoaded)
            {
                _nextScene.Load();
                OnSceneLoaded?.Invoke(_nextScene);
            }
            
            _nextScene.Activate();
            _activeScene = _nextScene;
            _nextScene = null;
            
            // Réinitialiser l'état de transition
            _isTransitioning = false;
            _transitionProgress = 0f;
            _loadingTask = null;
        }
        
        /// <summary>
        /// Annule la transition en cours
        /// </summary>
        private static void CancelTransition()
        {
            _isTransitioning = false;
            _transitionProgress = 0f;
            _nextScene = null;
            _loadingTask = null;
            
            Logger.Instance.Warning("Scene transition cancelled", LogCategory.Core);
        }
        
        /// <summary>
        /// Dessine la scène active
        /// </summary>
        public static void Draw(SpriteBatch spriteBatch)
        {
            // Dessiner la scène active
            _activeScene?.Draw(spriteBatch);
            
            // Lors d'une transition, on pourrait dessiner un effet de transition ici
            if (_isTransitioning && _transitionProgress > 0)
            {
                // Exemple simple : dessiner un fondu au noir
                // À personnaliser selon les besoins
                /*
                Texture2D fadeTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                fadeTexture.SetData(new[] { Color.Black });
                
                var viewport = spriteBatch.GraphicsDevice.Viewport;
                var fadeColor = Color.Black * _transitionProgress;
                
                spriteBatch.Draw(
                    fadeTexture,
                    new Rectangle(0, 0, viewport.Width, viewport.Height),
                    fadeColor);
                */
            }
        }
        
        /// <summary>
        /// Réinitialise le gestionnaire de scènes
        /// </summary>
        public static void Reset()
        {
            // Annuler toute transition en cours
            CancelTransition();
            
            // Décharger toutes les scènes
            foreach (var scene in _scenes.Values.ToList())
            {
                if (scene.IsLoaded)
                {
                    scene.Unload();
                }
            }
            
            _activeScene = null;
            
            Logger.Instance.Info("SceneManager reset", LogCategory.Core);
        }
    }
}