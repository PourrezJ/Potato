using Potato.Core.Entities;

namespace Potato.Core.Items;

public class StatUpgradeItem : ShopItem
{
    private Potato.Core.Stats.StatType _statType;
    private float _upgradeValue;
    
    public StatUpgradeItem(string name, string description, int cost, Potato.Core.Stats.StatType statType, float upgradeValue)
        : base(name, description, cost)
    {
        _statType = statType;
        _upgradeValue = upgradeValue;
    }
    
    public override bool Purchase(Player player)
    {
        // Créer un modificateur de statistique
        Potato.Core.Stats.StatModifier modifier = new Potato.Core.Stats.StatModifier(_statType, _upgradeValue, "Shop");
        
        // Appliquer le modificateur
        player.Stats.AddModifier(modifier);
        
        return true;
    }
}