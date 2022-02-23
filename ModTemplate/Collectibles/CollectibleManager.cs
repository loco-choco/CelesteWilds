using System;
using System.Collections.Generic;
using OWML.Common;

namespace CelesteWilds.Collectibles
{
    public class CollectibleManager<T> where T : CollectibleData
    {
        private string collectiblesFileName;
        public Dictionary<string, T> collectibles;
        private IModHelper helper;

        public event Action<T> OnCollected;

        public CollectibleManager(IModHelper helper, string collectiblesFileName) 
        {
            this.helper = helper;
            this.collectiblesFileName = collectiblesFileName;
            collectibles = new Dictionary<string, T>();
        }

        public void AddCollectible(T collectibleData) 
        {
            collectibles.Add(collectibleData.Name, collectibleData);
        }
        public void RemoveCollectible(T collectibleData)
        {
            collectibles.Remove(collectibleData.Name);
        }
        public void RemoveCollectible(string collectibleName)
        {
            collectibles.Remove(collectibleName);
        }
        public void LoadCollectibles()
        {
            collectibles = helper.Storage.Load<Dictionary<string, T>>(collectiblesFileName);
        }
        public void SaveCollectibles()
        {
            helper.Storage.Save(collectibles, collectiblesFileName);
        }
        public void ResetCollectibles()
        {
            foreach (var c in collectibles.Values)
                c.IsCollected = false;
        }
        public void GenerateCollectible(string name) 
        {
            if (collectibles.ContainsKey(name))
            {
                var collectibleGO = collectibles[name].GenerateGameObject();
                if (collectibleGO != null)
                    collectibleGO.OnCollected += () => OnCollected.Invoke((T)collectibleGO.Data);
            }
        }
        public void GenerateCollectibles()
        {
            foreach (var c in collectibles)
            {
                var collectibleGO = c.Value.GenerateGameObject();
                if(collectibleGO != null)
                    collectibleGO.OnCollected += () => OnCollected.Invoke((T)collectibleGO.Data);
            }
        }
    }
}
