using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Potato.Core
{
    /// <summary>
    /// Gère la position, la rotation et l'échelle d'un GameObject.
    /// Prend également en charge la hiérarchie parent-enfant.
    /// </summary>
    public class Transform : Component
    {
        #region Properties

        // Transformation locale
        private Vector2 _localPosition = Vector2.Zero;
        private float _localRotation = 0f;
        private Vector2 _localScale = Vector2.One;
        
        // Transformation globale (calculée)
        private Vector2 _worldPosition = Vector2.Zero;
        private float _worldRotation = 0f;
        private Vector2 _worldScale = Vector2.One;
        
        // Hiérarchie
        private Transform _parent;
        private readonly List<Transform> _children = new List<Transform>();
        
        // Flags indiquant si la transformation a changé
        private bool _isDirty = true;

        #endregion

        #region Public Properties

        // Propriétés de transformation locale
        public Vector2 LocalPosition 
        { 
            get => _localPosition;
            set 
            {
                if (_localPosition != value)
                {
                    _localPosition = value;
                    SetDirty();
                }
            }
        }
        
        public float LocalRotation 
        { 
            get => _localRotation;
            set 
            {
                if (_localRotation != value)
                {
                    _localRotation = value;
                    SetDirty();
                }
            }
        }
        
        public Vector2 LocalScale 
        { 
            get => _localScale;
            set 
            {
                if (_localScale != value)
                {
                    _localScale = value;
                    SetDirty();
                }
            }
        }
        
        // Propriétés de transformation globale
        public Vector2 Position
        {
            get 
            {
                if (_isDirty)
                    UpdateWorldTransform();
                return _worldPosition;
            }
            set 
            {
                if (_parent != null)
                {
                    // Convertir la position globale en position locale
                    LocalPosition = Vector2.Transform(value, Matrix.Invert(GetParentWorldMatrix()));
                }
                else
                {
                    LocalPosition = value;
                }
            }
        }
        
        public float Rotation
        {
            get 
            {
                if (_isDirty)
                    UpdateWorldTransform();
                return _worldRotation;
            }
            set 
            {
                if (_parent != null)
                {
                    // Convertir la rotation globale en rotation locale
                    LocalRotation = value - _parent.Rotation;
                }
                else
                {
                    LocalRotation = value;
                }
            }
        }
        
        public Vector2 ScaleValue
        {
            get 
            {
                if (_isDirty)
                    UpdateWorldTransform();
                return _worldScale;
            }
            set 
            {
                if (_parent != null)
                {
                    // Convertir l'échelle globale en échelle locale
                    LocalScale = new Vector2(
                        value.X / _parent.ScaleValue.X,
                        value.Y / _parent.ScaleValue.Y);
                }
                else
                {
                    LocalScale = value;
                }
            }
        }
        
        // Propriétés de hiérarchie
        public Transform Parent 
        { 
            get => _parent;
            set => SetParent(value);
        }
        
        public IReadOnlyList<Transform> Children => _children.AsReadOnly();

        #endregion

        #region Lifecycle Methods

        public override void Awake()
        {
            base.Awake();
            UpdateWorldTransform();
        }
        
        public override void Update(GameTime gameTime)
        {
            if (_isDirty)
            {
                UpdateWorldTransform();
            }
        }

        #endregion

        #region Hierarchy Management

        /// <summary>
        /// Change le parent de ce Transform
        /// </summary>
        public void SetParent(Transform parent, bool worldPositionStays = true)
        {
            if (_parent == parent)
                return;
                
            // Mémoriser la position mondiale avant changement de parent
            Vector2 oldWorldPosition = Position;
            float oldWorldRotation = Rotation;
            Vector2 oldWorldScale = ScaleValue;
            
            // Détacher de l'ancien parent
            if (_parent != null)
            {
                _parent._children.Remove(this);
            }
            
            // Attacher au nouveau parent
            _parent = parent;
            if (_parent != null)
            {
                _parent._children.Add(this);
            }
            
            // Préserver la position mondiale si demandé
            if (worldPositionStays)
            {
                Position = oldWorldPosition;
                Rotation = oldWorldRotation;
                ScaleValue = oldWorldScale;
            }
            else
            {
                // Sinon, marquer comme dirty pour recalculer
                SetDirty();
            }
        }
        
        /// <summary>
        /// Ajoute un enfant à ce Transform
        /// </summary>
        public void AddChild(Transform child)
        {
            if (child != null && child != this)
            {
                child.SetParent(this);
            }
        }

        #endregion

        #region Transform Operations

        /// <summary>
        /// Traduit ce Transform localement
        /// </summary>
        public void Translate(Vector2 translation)
        {
            LocalPosition += translation;
        }
        
        /// <summary>
        /// Fait tourner ce Transform localement
        /// </summary>
        public void Rotate(float angle)
        {
            LocalRotation += angle;
        }
        
        /// <summary>
        /// Met à l'échelle ce Transform localement
        /// </summary>
        public void ScaleBy(Vector2 scaleFactor)
        {
            LocalScale *= scaleFactor;
        }
        
        /// <summary>
        /// Traduit ce Transform dans l'espace global
        /// </summary>
        public void TranslateWorld(Vector2 translation)
        {
            Position += translation;
        }
        
        /// <summary>
        /// Regarde vers un point particulier
        /// </summary>
        public void LookAt(Vector2 target)
        {
            Vector2 direction = target - Position;
            Rotation = MathHelper.ToDegrees((float)System.Math.Atan2(direction.Y, direction.X));
        }
        
        /// <summary>
        /// Mise à jour de la transformation basée sur le changement de parent
        /// Appelé quand le GameObject parent change
        /// </summary>
        public void UpdateFromParent()
        {
            SetDirty(); // Marquer comme dirty pour forcer la mise à jour
            UpdateWorldTransform(); // Mettre à jour immédiatement
        }

        #endregion

        #region Internal Implementation

        /// <summary>
        /// Marque ce Transform et tous ses enfants comme sales (à recalculer)
        /// </summary>
        private void SetDirty()
        {
            _isDirty = true;
            
            // Propager aux enfants
            foreach (var child in _children)
            {
                child.SetDirty();
            }
        }
        
        /// <summary>
        /// Met à jour la transformation mondiale basée sur le parent et la transformation locale
        /// </summary>
        private void UpdateWorldTransform()
        {
            if (_parent != null)
            {
                // S'assurer que le parent a ses transformations à jour
                if (_parent._isDirty)
                {
                    _parent.UpdateWorldTransform();
                }
                
                // Appliquer la transformation du parent
                Matrix parentMatrix = _parent.GetWorldMatrix();
                
                // Calculer notre position mondiale
                _worldPosition = Vector2.Transform(_localPosition, parentMatrix);
                
                // Calculer notre rotation mondiale
                _worldRotation = _localRotation + _parent._worldRotation;
                
                // Calculer notre échelle mondiale
                _worldScale = new Vector2(
                    _localScale.X * _parent._worldScale.X,
                    _localScale.Y * _parent._worldScale.Y);
            }
            else
            {
                // Pas de parent, notre transformation locale est aussi notre transformation mondiale
                _worldPosition = _localPosition;
                _worldRotation = _localRotation;
                _worldScale = _localScale;
            }
            
            _isDirty = false;
        }
        
        /// <summary>
        /// Obtient la matrice de transformation mondiale
        /// </summary>
        public Matrix GetWorldMatrix()
        {
            if (_isDirty)
            {
                UpdateWorldTransform();
            }
            
            return Matrix.CreateScale(new Vector3(_worldScale.X, _worldScale.Y, 1)) *
                   Matrix.CreateRotationZ(MathHelper.ToRadians(_worldRotation)) *
                   Matrix.CreateTranslation(new Vector3(_worldPosition.X, _worldPosition.Y, 0));
        }
        
        /// <summary>
        /// Obtient la matrice de transformation du parent
        /// </summary>
        private Matrix GetParentWorldMatrix()
        {
            return _parent != null ? _parent.GetWorldMatrix() : Matrix.Identity;
        }

        #endregion
    }
}