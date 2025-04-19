using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Potato.Core.UI
{
    /// <summary>
    /// Gère les fonctionnalités d'accessibilité du jeu comme les descriptions audio,
    /// textes alternatifs, contraste élevé, etc.
    /// </summary>
    public class AccessibilityManager
    {
        // Singleton pour accès global
        private static AccessibilityManager _instance;
        public static AccessibilityManager Instance => _instance ?? (_instance = new AccessibilityManager());
        
        // Paramètres d'accessibilité
        private bool _audioDescriptionsEnabled = false;
        private bool _highContrastEnabled = false;
        private bool _largeTextEnabled = false;
        private bool _screenReaderEnabled = false;
        private float _textToSpeechRate = 1.0f;
        
        // Audio descriptif
        private Dictionary<string, string> _audioDescriptions = new Dictionary<string, string>();
        private Queue<string> _pendingAnnouncements = new Queue<string>();
        
        // Configuration des couleurs à contraste élevé
        private Dictionary<string, Color> _standardColors = new Dictionary<string, Color>();
        private Dictionary<string, Color> _highContrastColors = new Dictionary<string, Color>();
        
        // État actuel
        private Game _game;
        
        // Propriétés publiques
        public bool AudioDescriptionsEnabled 
        {
            get => _audioDescriptionsEnabled;
            set => _audioDescriptionsEnabled = value;
        }
        
        public bool HighContrastEnabled
        {
            get => _highContrastEnabled;
            set => _highContrastEnabled = value;
        }
        
        public bool LargeTextEnabled
        {
            get => _largeTextEnabled;
            set => _largeTextEnabled = value;
        }
        
        public bool ScreenReaderEnabled
        {
            get => _screenReaderEnabled;
            set => _screenReaderEnabled = value;
        }
        
        public float TextToSpeechRate
        {
            get => _textToSpeechRate;
            set => _textToSpeechRate = MathHelper.Clamp(value, 0.5f, 2.0f);
        }
        
        // Constructeur privé pour le singleton
        private AccessibilityManager() 
        {
            InitializeDefaultColors();
        }
        
        /// <summary>
        /// Initialise le gestionnaire avec une référence au jeu
        /// </summary>
        public void Initialize(Game game)
        {
            _game = game;
            LoadSettings();
        }
        
        /// <summary>
        /// Charge les paramètres d'accessibilité depuis le stockage
        /// </summary>
        private void LoadSettings()
        {
            // TODO: Charger depuis un fichier de configuration
        }
        
        /// <summary>
        /// Sauvegarde les paramètres d'accessibilité
        /// </summary>
        public void SaveSettings()
        {
            // TODO: Sauvegarder dans un fichier de configuration
        }
        
        /// <summary>
        /// Initialise les couleurs par défaut et à contraste élevé
        /// </summary>
        private void InitializeDefaultColors()
        {
            // Couleurs standards
            _standardColors["background"] = new Color(40, 40, 60, 220);
            _standardColors["text"] = Color.White;
            _standardColors["highlight"] = new Color(255, 255, 0);
            _standardColors["button"] = new Color(60, 60, 80, 200);
            _standardColors["buttonHover"] = new Color(80, 80, 100, 220);
            
            // Couleurs à contraste élevé
            _highContrastColors["background"] = Color.Black;
            _highContrastColors["text"] = Color.White;
            _highContrastColors["highlight"] = Color.Yellow;
            _highContrastColors["button"] = Color.DarkBlue;
            _highContrastColors["buttonHover"] = Color.Blue;
        }
        
        /// <summary>
        /// Obtient la couleur appropriée selon le mode d'accessibilité
        /// </summary>
        public Color GetColor(string colorName)
        {
            if (_highContrastEnabled && _highContrastColors.ContainsKey(colorName))
            {
                return _highContrastColors[colorName];
            }
            else if (_standardColors.ContainsKey(colorName))
            {
                return _standardColors[colorName];
            }
            
            return Color.Magenta; // Couleur par défaut pour identifier les erreurs
        }
        
        /// <summary>
        /// Ajoute une description audio pour un élément spécifique
        /// </summary>
        public void SetDescription(string elementId, string description)
        {
            _audioDescriptions[elementId] = description;
        }
        
        /// <summary>
        /// Annonce un texte via le lecteur d'écran
        /// </summary>
        public void Announce(string text)
        {
            if (_screenReaderEnabled)
            {
                _pendingAnnouncements.Enqueue(text);
                // TODO: Intégration avec un système text-to-speech
            }
        }
        
        /// <summary>
        /// Annonce la description d'un élément si disponible
        /// </summary>
        public void AnnounceElement(string elementId)
        {
            if (_audioDescriptionsEnabled && _audioDescriptions.ContainsKey(elementId))
            {
                Announce(_audioDescriptions[elementId]);
            }
        }
        
        /// <summary>
        /// Joue le son associé à une action
        /// </summary>
        public void PlayAccessibilitySound(string soundType)
        {
            // TODO: Implémenter avec des SoundEffects
        }
        
        /// <summary>
        /// Obtient le facteur d'échelle pour le texte en fonction des paramètres d'accessibilité
        /// </summary>
        public float GetTextScale()
        {
            return _largeTextEnabled ? 1.5f : 1.0f;
        }
        
        /// <summary>
        /// Met à jour le système d'accessibilité
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Traiter les annonces en attente
            if (_pendingAnnouncements.Count > 0)
            {
                // TODO: Implémenter lorsque l'intégration text-to-speech sera ajoutée
            }
        }
    }
}