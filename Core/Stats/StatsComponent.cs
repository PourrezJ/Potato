using System;
using System.Collections.Generic;

namespace Potato.Core.Stats
{
    public class StatsComponent
    {
        // Base stats
        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }
        public float AttackSpeed { get; set; }
        public float Range { get; set; }
        public float CriticalChance { get; set; }
        public float CriticalDamage { get; set; }
        public float Harvesting { get; set; }
        public float Engineering { get; set; }
        public float Luck { get; set; }
        
        // Stat modifiers
        private List<StatModifier> _modifiers;
        
        public StatsComponent()
        {
            _modifiers = new List<StatModifier>();
            InitializeDefaultStats();
        }
        
        private void InitializeDefaultStats()
        {
            MaxHealth = 100;
            Health = MaxHealth;
            Speed = 200;
            Damage = 10;
            AttackSpeed = 1.0f;
            Range = 100;
            CriticalChance = 0.05f;
            CriticalDamage = 1.5f;
            Harvesting = 1.0f;
            Engineering = 1.0f;
            Luck = 1.0f;
        }
        
        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            ApplyModifiers();
        }
        
        public void RemoveModifier(StatModifier modifier)
        {
            _modifiers.Remove(modifier);
            ApplyModifiers();
        }
        
        public void ClearModifiers()
        {
            _modifiers.Clear();
            InitializeDefaultStats();
        }
        
        private void ApplyModifiers()
        {
            // Reset to default stats
            InitializeDefaultStats();
            
            // Apply all modifiers
            foreach (var modifier in _modifiers)
            {
                ApplyModifier(modifier);
            }
        }
        
        private void ApplyModifier(StatModifier modifier)
        {
            switch (modifier.StatType)
            {
                case StatType.MaxHealth:
                    MaxHealth += modifier.Value;
                    Health = Math.Min(Health, MaxHealth);
                    break;
                case StatType.Speed:
                    Speed += modifier.Value;
                    break;
                case StatType.Damage:
                    Damage += modifier.Value;
                    break;
                case StatType.AttackSpeed:
                    AttackSpeed += modifier.Value;
                    break;
                case StatType.Range:
                    Range += modifier.Value;
                    break;
                case StatType.CriticalChance:
                    CriticalChance += modifier.Value;
                    break;
                case StatType.CriticalDamage:
                    CriticalDamage += modifier.Value;
                    break;
                case StatType.Harvesting:
                    Harvesting += modifier.Value;
                    break;
                case StatType.Engineering:
                    Engineering += modifier.Value;
                    break;
                case StatType.Luck:
                    Luck += modifier.Value;
                    break;
            }
        }
        
        public void Reset()
        {
            _modifiers.Clear();
            InitializeDefaultStats();
        }
    }
    
    public enum StatType
    {
        MaxHealth,
        Speed,
        Damage,
        AttackSpeed,
        Range,
        CriticalChance,
        CriticalDamage,
        Harvesting,
        Engineering,
        Luck
    }
    
    public class StatModifier
    {
        public StatType StatType { get; private set; }
        public float Value { get; private set; }
        public string Source { get; private set; }
        
        public StatModifier(StatType statType, float value, string source)
        {
            StatType = statType;
            Value = value;
            Source = source;
        }
    }
}