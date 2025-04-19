using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Potato.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Potato.Core
{
    /// <summary>
    /// Gestionnaire central de tous les GameBehaviours, inspiré du système de Unity
    /// </summary>
    public class BehaviourManager
    {
        private static BehaviourManager _instance;
        public static BehaviourManager Instance => _instance ??= new BehaviourManager();
        
        private Game _game;
        private List<GameBehaviour> _behaviours = new List<GameBehaviour>();
        private List<GameBehaviour> _pendingAddition = new List<GameBehaviour>();
        private List<GameBehaviour> _pendingRemoval = new List<GameBehaviour>();
        private bool _isProcessingLists = false;
        
        private BehaviourManager()
        {
            Logger.Instance.Info("BehaviourManager créé", LogCategory.Core);
        }
        
        /// <summary>
        /// Initialise le BehaviourManager avec la référence au jeu
        /// </summary>
        public void Initialize(Game game)
        {
            _game = game;
            Logger.Instance.Info("BehaviourManager initialisé", LogCategory.Core);
        }
        
        /// <summary>
        /// Charge automatiquement tous les GameBehaviours présents dans l'assembly
        /// </summary>
        public void DiscoverBehaviours()
        {
            try
            {
                // Obtenir l'assembly courant
                Assembly assembly = Assembly.GetExecutingAssembly();
                
                // Trouver tous les types qui héritent de GameBehaviour
                var behaviourTypes = assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(GameBehaviour)) && !t.IsAbstract);
                
                int count = 0;
                foreach (var type in behaviourTypes)
                {
                    try
                    {
                        // Vérifier si ce type a déjà été instancié
                        bool alreadyExists = _behaviours.Any(b => b.GetType() == type);
                        if (alreadyExists)
                        {
                            Logger.Instance.Debug($"Comportement {type.Name} déjà instancié, ignoré", LogCategory.Core);
                            continue;
                        }
                        
                        Logger.Instance.Debug($"Tentative de création du behaviour {type.Name}", LogCategory.Core);
                        
                        // Essayer d'abord le constructeur sans paramètre
                        var defaultConstructor = type.GetConstructor(Type.EmptyTypes);
                        if (defaultConstructor != null)
                        {
                            var behaviour = (GameBehaviour)defaultConstructor.Invoke(null);
                            RegisterBehaviour(behaviour);
                            count++;
                            continue;
                        }
                        
                        // Puis essayer le constructeur avec Game
                        var gameConstructor = type.GetConstructor(new[] { typeof(Game) });
                        if (gameConstructor != null && _game != null)
                        {
                            var behaviour = (GameBehaviour)gameConstructor.Invoke(new object[] { _game });
                            RegisterBehaviour(behaviour);
                            count++;
                            continue;
                        }
                        
                        Logger.Instance.Warning($"Impossible de créer le behaviour {type.Name} : aucun constructeur compatible", LogCategory.Core);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error($"Erreur lors de la création du behaviour {type.Name}: {ex.Message}", LogCategory.Core);
                    }
                }
                
                Logger.Instance.Info($"Découvert {count} behaviours via réflexion", LogCategory.Core);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"Erreur lors de la découverte des behaviours: {ex.Message}", LogCategory.Core);
            }
        }
        
        /// <summary>
        /// Enregistre un nouveau GameBehaviour
        /// </summary>
        public void RegisterBehaviour(GameBehaviour behaviour)
        {
            if (_isProcessingLists)
            {
                _pendingAddition.Add(behaviour);
                return;
            }
            
            if (!_behaviours.Contains(behaviour))
            {
                _behaviours.Add(behaviour);
                Logger.Instance.Debug($"Behaviour {behaviour.GetType().Name} enregistré", LogCategory.Core);
            }
        }
        
        /// <summary>
        /// Supprime un GameBehaviour
        /// </summary>
        public void UnregisterBehaviour(GameBehaviour behaviour)
        {
            if (_isProcessingLists)
            {
                _pendingRemoval.Add(behaviour);
                return;
            }
            
            if (_behaviours.Contains(behaviour))
            {
                behaviour.Destroy();
                _behaviours.Remove(behaviour);
                Logger.Instance.Debug($"Behaviour {behaviour.GetType().Name} désenregistré", LogCategory.Core);
            }
        }
        
        /// <summary>
        /// Met à jour tous les GameBehaviours actifs
        /// </summary>
        public void Update(GameTime gameTime)
        {
            _isProcessingLists = true;
            
            foreach (var behaviour in _behaviours)
            {
                if (behaviour.IsActive)
                {
                    try
                    {
                        behaviour.Update(gameTime);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error($"Erreur dans Update de {behaviour.GetType().Name}: {ex.Message}", LogCategory.Core);
                    }
                }
            }
            
            _isProcessingLists = false;
            
            // Traiter les listes en attente
            ProcessPendingLists();
        }
        
        /// <summary>
        /// Dessine tous les GameBehaviours actifs
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            _isProcessingLists = true;
            
            foreach (var behaviour in _behaviours)
            {
                if (behaviour.IsActive)
                {
                    try
                    {
                        behaviour.Draw(spriteBatch);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Error($"Erreur dans Draw de {behaviour.GetType().Name}: {ex.Message}", LogCategory.Core);
                    }
                }
            }
            
            _isProcessingLists = false;
            
            // Traiter les listes en attente
            ProcessPendingLists();
        }
        
        /// <summary>
        /// Traite les listes d'ajout et de suppression en attente
        /// </summary>
        private void ProcessPendingLists()
        {
            // Traiter les ajouts
            foreach (var behaviour in _pendingAddition)
            {
                if (!_behaviours.Contains(behaviour))
                {
                    _behaviours.Add(behaviour);
                }
            }
            _pendingAddition.Clear();
            
            // Traiter les suppressions
            foreach (var behaviour in _pendingRemoval)
            {
                if (_behaviours.Contains(behaviour))
                {
                    behaviour.Destroy();
                    _behaviours.Remove(behaviour);
                }
            }
            _pendingRemoval.Clear();
        }
        
        /// <summary>
        /// Obtient tous les GameBehaviours d'un type spécifique
        /// </summary>
        public List<T> GetBehavioursOfType<T>() where T : GameBehaviour
        {
            return _behaviours.OfType<T>().ToList();
        }
        
        /// <summary>
        /// Obtient le premier GameBehaviour d'un type spécifique
        /// </summary>
        public T GetBehaviourOfType<T>() where T : GameBehaviour
        {
            return _behaviours.OfType<T>().FirstOrDefault();
        }
        
        /// <summary>
        /// Libère toutes les ressources
        /// </summary>
        public void Dispose()
        {
            foreach (var behaviour in _behaviours)
            {
                try
                {
                    behaviour.Destroy();
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"Erreur lors de la destruction de {behaviour.GetType().Name}: {ex.Message}", LogCategory.Core);
                }
            }
            
            _behaviours.Clear();
            _pendingAddition.Clear();
            _pendingRemoval.Clear();
            
            Logger.Instance.Info("BehaviourManager détruit", LogCategory.Core);
        }
    }
}