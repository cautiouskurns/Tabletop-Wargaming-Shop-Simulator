using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarketShopandRetailSystem
{
    public class AdvancedGameManager : MonoBehaviour
    {
        public GameObject CameraMain;
        public static AdvancedGameManager Instance;
        public PlayerValues playerValues;
        public ControllerType controllerType;
        public Transform incomingOrderBoxPoint;

        [HideInInspector]
        public Mode CurrentMode = Mode.Free;
        public int StartingMoneyAmount = 1000;
        public int StartingExperienceAmount = 1;


        public KeyCode InteractingKey;
        public KeyCode GrabbingKey;
        public KeyCode SelectionKey;
        [Range(0, 100)]
        public int NPCShoppingPercentageRate = 25;
        private int BaseNPCShoppingPercentageRate = 0;
        public bool postersAmountCanEffectNPCShoppingPercentageRate = false;
        public string CurrencySymbol = "$";
        public GameObject[] DustPrefabs;

        [HideInInspector]
        public ShopScript currentSelectedShop;
        List<GameObject> dusts = new List<GameObject>();
        public int ProductsProfitLimitForBeingExpensiveTimes = 3;

        public bool canHireCashier = true;
        public int cashierDailySalary = 5;
        public bool canHireCleaner = true;
        public int cleanerDailySalary = 5;

        [HideInInspector]
        public bool cashierHired = false;
        [HideInInspector]
        public int cashierHiredAmount = 0;
        [HideInInspector]
        public bool cleanerHired = false;
        [HideInInspector]
        public int cleanerHiredAmount = 0;

        public GameObject[] Cleaners;
        public GameObject[] Cashiers;

        public int EarnXPPerEarnings = 100;
        public GivingChangeOption givingChangeOption;

        [HideInInspector]
        public bool isShopOpen = false;
        public bool isHangingSignActive = true;

        public bool isPayingBillsSystemActivated = true;
        public GameObject MailBox;

        private void Awake()
        {
            Instance = this;
            isShopOpen = !isHangingSignActive;
            playerValues = new PlayerValues();

            cashierHired = (PlayerPrefs.GetInt("cashierHired", 0) == 0 ? false : true);
            cashierHiredAmount = PlayerPrefs.GetInt("cashierHiredAmount", 0);
            cleanerHired = (PlayerPrefs.GetInt("cleanerHired", 0) == 0 ? false : true);
            cleanerHiredAmount = PlayerPrefs.GetInt("cleanerHiredAmount", 0);

            for (int i = 0; i < cashierHiredAmount; i++)
            {
                Cashiers[i].SetActive(true);
            }
            for (int i = 0; i < cleanerHiredAmount; i++)
            {
                Cleaners[i].SetActive(true);
            }
        }
        public void UpdatePlayerValues()
        {
            playerValues.Money = PlayerPrefs.GetInt("Money", StartingMoneyAmount);
            playerValues.Experience = PlayerPrefs.GetInt("Experience", StartingExperienceAmount);
            playerValues.Name = PlayerPrefs.GetString("ProfileName", "");
            playerValues.Health = HeroPlayerScript.Instance.Health;
        }

        public void CheckTheRate()
        {
            InventoryManager.Instance.CheckCurrentEquipmentList();
            List<Poster> allPosters = InventoryManager.Instance.CurrentEquipmentList.Where(x => x.GetComponent<Poster>() != null).Select(x => x.GetComponent<Poster>()).ToList();
            float postersTotalAttraction = 0;
            for (int i = 0; i < allPosters.Count; i++)
            {
                postersTotalAttraction = postersTotalAttraction + allPosters[i].CustomerAttractionPoint;
            }

            NPCShoppingPercentageRate = BaseNPCShoppingPercentageRate + Mathf.RoundToInt(postersTotalAttraction);
            if (NPCShoppingPercentageRate > 100) NPCShoppingPercentageRate = 100;
        }

        public void Set_ProfileName(string Name)
        {
            PlayerPrefs.SetString("ProfileName", Name);
            PlayerPrefs.Save();
            UpdatePlayerValues();
        }

        public void Spend(int price)
        {
            int money = PlayerPrefs.GetInt("Money", StartingMoneyAmount);
            money = money - price;
            PlayerPrefs.SetInt("Money", money);
            PlayerPrefs.Save();
            GameCanvas.Instance.UpdateStatus();
            AudioManager.Instance.Play_audioClip_Coin();
            GameCanvas.Instance.Show_SpendGet(price*-1);
        }

        public void CreateDust(Transform t)
        {
            for (int i = 0; i < dusts.Count; i++)
            {
                if (dusts[i] == null)
                {
                    dusts.RemoveAt(i);
                    i--;
                }
            }
            if (dusts.Count < 5)
            {
                GameObject dd = Instantiate(DustPrefabs[UnityEngine.Random.Range(0, DustPrefabs.Length)], new Vector3(t.position.x, t.position.y + 0.05f, t.position.z), Quaternion.identity);
                dusts.Add(dd);
            }
        }

        void Start()
        {
            UpdatePlayerValues();
            GameCanvas.Instance.Crosshair.SetActive(true);
            if(controllerType == ControllerType.Mobile)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            GameCanvas.Instance.Text_Cashier_Amount.text = cashierHiredAmount.ToString();
            GameCanvas.Instance.Text_Cleaner_Amount.text = cleanerHiredAmount.ToString();
            MailBox.SetActive(isPayingBillsSystemActivated);

            DailyStart();
            InventoryManager.Instance.LoadBuiltObjects();
            InventoryManager.Instance.LoadSellableObjects();
            StartCoroutine(InventoryManager.Instance.ArrangeEverything());
            Application.targetFrameRate = 60;
            BaseNPCShoppingPercentageRate = NPCShoppingPercentageRate;
            if(postersAmountCanEffectNPCShoppingPercentageRate)
            {
                InvokeRepeating("CheckTheRate", 5, 5);
            }
        }


        public void DailyStart()
        {
            CameraMain.SetActive(true);
            GameCanvas.Instance.image_Blinking.gameObject.SetActive(true);
            GameCanvas.Instance.image_Blinking.GetComponent<Animation>().Play();
            FirstPersonController.Instance.enabled = true;
            CameraScript.Instance.enabled = true;
        }

        public void Get(CollactableType type, int amount)
        {
            if (type == CollactableType.Money)
            {
                int money = PlayerPrefs.GetInt("Money", StartingMoneyAmount);
                money = money + amount;
                PlayerPrefs.SetInt("Money", money);
                PlayerPrefs.Save();
                AudioManager.Instance.Play_audioClip_Coin();
                GameCanvas.Instance.UpdateStatus();
                GameCanvas.Instance.Show_SpendGet(amount);

                UpdatePlayerValues();
                int lastRecorded = PlayerPrefs.GetInt("LastXPEarnedMoney", 0);
                if (playerValues.Money >= lastRecorded + EarnXPPerEarnings)
                {
                    Get(CollactableType.Experience, 1);
                    PlayerPrefs.SetInt("LastXPEarnedMoney", playerValues.Money);
                    PlayerPrefs.Save();
                }
            }
            else if (type == CollactableType.Experience)
            {
                int experience = PlayerPrefs.GetInt("Experience", StartingExperienceAmount);
                experience = experience + amount;
                PlayerPrefs.SetInt("Experience", experience);
                PlayerPrefs.Save();
                AudioManager.Instance.Play_audioClip_Experience();
                GameCanvas.Instance.UpdateStatus();
                GameCanvas.Instance.Show_SpendGet(amount);
            }
        }
    }

    public enum CollactableType
    {
        Money,
        Experience
    }

    public class PlayerValues
    {
        public float Health;
        public int Money;
        public string Name;
        public int Experience;
    }

    public enum Mode
    {
        Free,
        InInventoryLocating,
        InMechanicSelecting,
        ReadingNote,
        InSellerList,
        HoldingForOrdering
    }

    public enum ControllerType
    {
        PC,
        Mobile
    }

    public enum GivingChangeOption
    {
        Keyboard,
        Banknotes,
        Both
    }
}