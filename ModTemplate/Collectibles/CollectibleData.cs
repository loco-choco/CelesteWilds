
namespace CelesteWilds.Collectibles
{
    public class CollectibleData
    {
        public string Name;
        public bool IsCollected;

        public CollectibleData(string name) 
        {
            Name = name;
            IsCollected = false;
        }

        public virtual Collectible GenerateGameObject() 
        {
            return null;
        }
    }
}
