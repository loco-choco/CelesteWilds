using UnityEngine;

namespace CelesteWilds.Collectibles
{
    public class CollectibleMarshmellowRotation : MonoBehaviour
    {
        float rotationSpeed = 48f;
        float z;
        public void Update()
        {
            z += Time.deltaTime * rotationSpeed;
            transform.localRotation = Quaternion.Euler(0, 0, z);
        }
    }
}
