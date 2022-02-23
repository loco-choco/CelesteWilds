using UnityEngine;

namespace CelesteWilds.Collectibles
{
    public class MarshmellowCollectibleData : CollectibleData
    {
        public float LocalPositionX;
        public float LocalPositionY;
        public float LocalPositionZ;
        public string ParentObjectName;


        public Sector.Name SectorWhereItBelongs;
        public string DisplayName;
        public string Hint;
        public MarshmellowCollectibleData(string name) : base(name)
        {
        }

        public Transform FindCollectibleParentObject() 
        {
            return GameObject.Find(ParentObjectName).transform;
        }

        public override Collectible GenerateGameObject()
        {
            if(!IsCollected)
                return CollectibleMarshmellow.CreateCollectible(this);

            return base.GenerateGameObject();
        }
    }
}
