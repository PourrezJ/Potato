using System;

namespace Potato.Core.Attributes
{
    /// <summary>
    /// Attribut personnalisé pour définir l'ordre d'exécution d'un GameBehaviour.
    /// Fonctionne de manière similaire à l'attribut ExecutionOrder de Unity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ExecutionOrderAttribute : Attribute
    {
        // Priorités prédéfinies pour faciliter l'utilisation
        public const int FirstPriority = -2000;
        public const int EarlyPriority = -1000;
        public const int DefaultPriority = 0;
        public const int LatePriority = 1000;
        public const int LastPriority = 2000;
        
        /// <summary>
        /// Obtient l'ordre d'exécution spécifié.
        /// </summary>
        public int Order { get; }
        
        /// <summary>
        /// Initialise une nouvelle instance de l'attribut ExecutionOrder.
        /// </summary>
        /// <param name="order">L'ordre d'exécution. Les valeurs plus basses sont exécutées en premier.</param>
        public ExecutionOrderAttribute(int order = DefaultPriority)
        {
            Order = order;
        }
    }
}