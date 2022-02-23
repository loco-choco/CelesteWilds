using UnityEngine;

namespace CelesteWilds.Collectibles
{
    public class CollectibleSpawner : MonoBehaviour
    {
        public static bool CanPlayerSpawnMoreCollectibles = false;
        bool wasInteractPressed = false;
        public MarshmellowCollectiblesUI marshmellowCollectiblesUI;
        Transform camera;
        public void Start() 
        {
            camera = Locator.GetPlayerCamera().transform;
        }
        public void Update() 
        {
            if (!CanPlayerSpawnMoreCollectibles)
                return;

            bool isInteractPressed = OWInput.IsPressed(InputLibrary.flashlight, InputMode.Character | InputMode.NomaiRemoteCam, 2f);

            if (!Physics.Raycast(camera.position, camera.forward, out RaycastHit hit, 100f, OWLayerMask.physicalMask))
                return;

            if (isInteractPressed && !wasInteractPressed)
            {
                string name = hit.transform + Random.value.ToString();
                Vector3 localPos = hit.transform.InverseTransformPoint(camera.transform.position);

                MarshmellowCollectibleData data = new MarshmellowCollectibleData(name)
                {
                    LocalPositionX = localPos.x,
                    LocalPositionY = localPos.y,
                    LocalPositionZ = localPos.z,
                    ParentObjectName = hit.transform.name,
                    SectorWhereItBelongs = marshmellowCollectiblesUI.LastSectorEntered
                };

                CelesteWilds.collectibleManager.AddCollectible(data);
                CelesteWilds.collectibleManager.GenerateCollectible(data.Name);
            }    

            wasInteractPressed = isInteractPressed;
        }
    }
}
