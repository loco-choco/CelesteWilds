using UnityEngine;


namespace CelesteWilds
{
    public class MatchRigidbody : MonoBehaviour
    {
        public Rigidbody targetRigidbody;
        private Rigidbody rigidbody;

        public bool matchEvenWhenKinematic = false;

        private void Start() 
        {
            rigidbody = GetComponent<Rigidbody>();
        }
        private void FixedUpdate()
        {
            if (targetRigidbody == null || rigidbody.isKinematic && !matchEvenWhenKinematic)
                return;

            rigidbody.velocity = targetRigidbody.GetPointVelocity(rigidbody.transform.position);
        }
    }
}
