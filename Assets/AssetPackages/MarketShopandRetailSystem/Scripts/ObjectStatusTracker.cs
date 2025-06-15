using UnityEngine;

namespace MarketShopandRetailSystem
{
    public class ObjectStatusTracker : MonoBehaviour
    {
        [Tooltip("This string must be a unique id for this item if you want to track it")]
        public string ItemUniqueID;

        void Start()
        {
            if (!string.IsNullOrEmpty(ItemUniqueID) && PlayerPrefs.GetInt(ItemUniqueID, 0) == 1)
            {
                Destroy(gameObject);
            }
        }

        public void DestroyObjectForever()
        {
            PlayerPrefs.SetInt(ItemUniqueID, 1);
            PlayerPrefs.Save();
            Destroy(gameObject, 0.5f);
        }
    }
}
