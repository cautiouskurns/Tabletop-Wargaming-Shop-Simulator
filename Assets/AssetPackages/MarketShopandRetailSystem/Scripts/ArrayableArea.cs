using System.Collections;
using System.Linq;
using UnityEngine;

namespace MarketShopandRetailSystem
{
    public class ArrayableArea : MonoBehaviour
    {
        public PlacablePoint[] PlacablePoints;
        private OrderBox OrderBox;
        private PhysicalEquipmentDetails detail;

        private void Awake()
        {
            for (int i = 0; i < PlacablePoints.Length; i++)
            {
                PlacablePoints[i].ID = i;
            }
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(0.5f);
            detail = GetComponent<PhysicalEquipmentDetails>();
            int objectID = detail.equipmentIndex;
            for (int i = 0; i < PlacablePoints.Length; i++)
            {
                if (PlayerPrefs.GetInt(detail.Name + objectID.ToString() + "_Item" + i.ToString(), 0) == -1)
                {
                    Destroy(PlacablePoints[i].objectToGrab);
                    PlacablePoints[i].isAvailable = true;
                    PlacablePoints[i].objectToGrab = null;
                }
            }
            OrderBox = GetComponent<OrderBox>();
        }

        public void RemoveItem(int i)
        {
            int objectID = detail.equipmentIndex;
            PlayerPrefs.SetInt(detail.Name + objectID.ToString() + "_Item" + i.ToString(), -1);
            PlayerPrefs.Save();

            PlacablePoints[i].isAvailable = true;
            PlacablePoints[i].objectToGrab = null;

            if (OrderBox != null)
            {
                OrderBox.textCount.text = PlacablePoints.Where(x => !x.isAvailable).Count().ToString();
            }
        }

        public void RemoveItemFromSelftForPuttingOrderBox(PlacablePoint placable)
        {
            SellableObject d = placable.objectToGrab.GetComponent<SellableObject>();
            PlayerPrefs.DeleteKey(d.Name + d.sellableObjectIndex.ToString() + "_PosX");
            PlayerPrefs.DeleteKey(d.Name + d.sellableObjectIndex.ToString() + "_PosY");
            PlayerPrefs.DeleteKey(d.Name + d.sellableObjectIndex.ToString() + "_PosZ");
            PlayerPrefs.DeleteKey(d.Name + d.sellableObjectIndex.ToString() + "_RotX");
            PlayerPrefs.DeleteKey(d.Name + d.sellableObjectIndex.ToString() + "_RotY");
            PlayerPrefs.DeleteKey(d.Name + d.sellableObjectIndex.ToString() + "_RotZ");
            placable.isAvailable = true;
            placable.objectToGrab = null;

            if (OrderBox != null)
            {
                OrderBox.textCount.text = PlacablePoints.Where(x => !x.isAvailable).Count().ToString();
            }
        }

        

        public void DestroyAllProducts()
        {
            for (int i = 0; i < PlacablePoints.Length; i++)
            {
                if(PlacablePoints[i].objectToGrab != null && PlacablePoints[i].objectToGrab.GetComponent<SellableObject>() != null)
                {
                    PlacablePoints[i].objectToGrab.GetComponent<SellableObject>().CollapseAndDestroy();
                }
            }
        }

        public void AddItem(int i, SellableObject equipment)
        {
            int objectID = detail.equipmentIndex;
            PlayerPrefs.SetInt(detail.Name + objectID.ToString() + "_Item" + i.ToString(), 1);
            int objectCount = PlayerPrefs.GetInt(equipment.Name + "_Count", -1);
            objectCount = objectCount + 1;
            PlayerPrefs.SetInt(equipment.Name + "_Count", objectCount);
            PlayerPrefs.SetFloat(equipment.Name + objectCount.ToString() + "_PosX", PlacablePoints[i].transform.position.x);
            PlayerPrefs.SetFloat(equipment.Name + objectCount.ToString() + "_PosY", PlacablePoints[i].transform.position.y);
            PlayerPrefs.SetFloat(equipment.Name + objectCount.ToString() + "_PosZ", PlacablePoints[i].transform.position.z);
            PlayerPrefs.SetFloat(equipment.Name + objectCount.ToString() + "_RotX", PlacablePoints[i].transform.eulerAngles.x);
            PlayerPrefs.SetFloat(equipment.Name + objectCount.ToString() + "_RotY", PlacablePoints[i].transform.eulerAngles.y);
            PlayerPrefs.SetFloat(equipment.Name + objectCount.ToString() + "_RotZ", PlacablePoints[i].transform.eulerAngles.z);
            PlayerPrefs.Save();
            equipment.sellableObjectIndex = objectCount;
            equipment.isAvailable = true;
            equipment.putPoint = PlacablePoints[i];
        }
    }
}