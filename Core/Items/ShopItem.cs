using Potato.Core.Entities;

namespace Potato.Core.Items
{
    public abstract class ShopItem
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Cost { get; private set; }
        
        public ShopItem(string name, string description, int cost)
        {
            Name = name;
            Description = description;
            Cost = cost;
        }
        
        public abstract bool Purchase(Player player);
    }
}