using CelesteWilds.Collectibles;
using System;
using UnityEngine;

namespace CelesteWilds
{
    public class Collectible : MonoBehaviour
    {
        public virtual void OnTriggerEnter(Collider collider)
        {
            if (collider == Locator.GetPlayerCollider())
                Collect();
        }

        public virtual void Collect() 
        {
            OnCollected?.Invoke();
        }

        public event Action OnCollected;

        public CollectibleData Data;
    }
}
