using System;
using Microsoft.Xna.Framework;

namespace Potato.Core
{
    public abstract class Singleton<T> : GameBehaviour where T : Singleton<T>, new()
    {
        #region Singleton Pattern
        private static T _instance;
        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            ///_instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }
        #endregion

        public Singleton()
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("Cannot create another instance of Singleton class.");
            }
            else{
                _instance = this as T;
            }
        }


        /// <summary>
        /// Méthode de dessin - à implémenter par les classes dérivées
        /// </summary>
        public virtual void Draw()
        {
        }

        /// <summary>
        /// Réinitialise le singleton pour une nouvelle partie
        /// </summary>
        public virtual void Reset()
        {
        }

        /// <summary>
        /// Libère les ressources utilisées par le singleton
        /// </summary>
        public virtual void Cleanup()
        {
            
        }
    }
}