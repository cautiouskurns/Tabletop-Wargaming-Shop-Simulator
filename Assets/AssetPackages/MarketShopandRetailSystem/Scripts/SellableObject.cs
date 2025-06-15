using System.Collections;
using UnityEngine;

namespace MarketShopandRetailSystem
{
    public class SellableObject : MonoBehaviour
    {
        [HideInInspector]
        public bool isAvailable = true;
        public string Name;
        public int BuyingPrice;
        public int SellingPrice;
        public Sprite sprite;
        [HideInInspector]
        public PlacablePoint putPoint;
        public int sellableObjectIndex;
        
        private IEnumerator Start()
        {
            GetSellingPrice();
            yield return new WaitForSeconds(2f);
            if(isAvailable && putPoint == null)
            {
                // Destroy this
                CollapseAndDestroy();
            }
        }

        public int GetSellingPrice()
        {
            SellingPrice = PlayerPrefs.GetInt("ProducSellingPrice" + Name, 0) == 0 ? SellingPrice : PlayerPrefs.GetInt("ProducSellingPrice" + Name, 0);
            return SellingPrice;
        }

        public void CollapseAndDestroy()
        {
            GetComponent<Collider>().isTrigger = false;
            PlayerPrefs.DeleteKey(Name + sellableObjectIndex.ToString() + "_PosX");
            PlayerPrefs.DeleteKey(Name + sellableObjectIndex.ToString() + "_PosY");
            PlayerPrefs.DeleteKey(Name + sellableObjectIndex.ToString() + "_PosZ");
            PlayerPrefs.DeleteKey(Name + sellableObjectIndex.ToString() + "_RotX");
            PlayerPrefs.DeleteKey(Name + sellableObjectIndex.ToString() + "_RotY");
            PlayerPrefs.DeleteKey(Name + sellableObjectIndex.ToString() + "_RotZ");
            PlayerPrefs.Save();
            InventoryManager.Instance.CurrentEquipmentList.Remove(gameObject);
            gameObject.AddComponent<Rigidbody>();
            Destroy(gameObject, 4);
        }
    }
}