
using UnityEngine;

namespace CelesteWilds.Collectibles
{
    public class CollectibleMarshmellow : Collectible
    {
        public override void Collect()
        {
            Data.IsCollected = true;
            base.Collect();
            Locator.GetPlayerAudioController().PlayMarshmallowEat();
            Destroy(gameObject);
        }
        static public Mesh marshmellowMesh;
        static public Material marshmellowMaterial;

        public static CollectibleMarshmellow CreateCollectible(MarshmellowCollectibleData data) 
        {
            Transform collectibleParent = data.FindCollectibleParentObject();

            if (collectibleParent == null)
                return null;

            var go = new GameObject("CollectibleMarshmellow");
            go.AddComponent<SphereCollider>().isTrigger = true;
            go.AddComponent<CollectibleMarshmellowRotation>();

            var mesh = new GameObject("CollectibleMarshmellow_Mesh");

            if (marshmellowMesh == null)
                marshmellowMesh = GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Mallow_Root/Props_HEA_Marshmallow").GetComponent<MeshFilter>().mesh;
            if (marshmellowMaterial == null)
                marshmellowMaterial = GameObject.Find("Player_Body/RoastingSystem/Stick_Root/Stick_Pivot/Stick_Tip/Mallow_Root/Props_HEA_Marshmallow").GetComponent<MeshRenderer>().material;
            
            mesh.AddComponent<MeshFilter>().mesh = marshmellowMesh;
            mesh.AddComponent<MeshRenderer>().material = marshmellowMaterial;
            mesh.transform.parent = go.transform;
            mesh.transform.localScale = Vector3.one * 10f;
            mesh.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

            CollectibleMarshmellow collectible = go.AddComponent<CollectibleMarshmellow>();
            collectible.Data = data;

            go.transform.parent = collectibleParent;
            go.transform.localPosition = new Vector3(data.LocalPositionX, data.LocalPositionY, data.LocalPositionZ);

            return collectible;
        }
    }
}
