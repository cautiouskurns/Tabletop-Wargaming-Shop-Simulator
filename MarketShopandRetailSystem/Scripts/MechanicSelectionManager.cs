using UnityEngine;

namespace MarketShopandRetailSystem
{
    public class MechanicSelectionManager : MonoBehaviour
    {
        public GameObject Panel_MechanicSelection;
        public GameObject Button_Free;
        public GameObject Button_Build;
        public GameObject Button_CallShipment;
        public GameObject Button_Clean;
        public static MechanicSelectionManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void Click_Button_Hire_Cleaner()
        {
            // Lets check how many we have (max 4)
            int currentAmount = PlayerPrefs.GetInt("cleanerHiredAmount", 0);
            if(currentAmount < 4)
            {
                AdvancedGameManager.Instance.cleanerHired = true;
                PlayerPrefs.SetInt("cleanerHired", 1);
                currentAmount = currentAmount + 1;
                PlayerPrefs.SetInt("cleanerHiredAmount", currentAmount);
                PlayerPrefs.Save();
                AdvancedGameManager.Instance.Cleaners[currentAmount-1].SetActive(true);
                GameCanvas.Instance.Button_Hire_Cleaner_Remover.SetActive(true);
                GameCanvas.Instance.Text_Cleaner_Amount.text = currentAmount.ToString();
            }
        }

        public void Click_Button_Dismiss_Cleaner()
        {
            if (AdvancedGameManager.Instance.cleanerHired)
            {
                // Lets check how many we have (max 4)
                int currentAmount = PlayerPrefs.GetInt("cleanerHiredAmount", 0);
                if (currentAmount > 0)
                {
                    AdvancedGameManager.Instance.Cleaners[currentAmount - 1].SetActive(false);
                    currentAmount = currentAmount - 1;
                    PlayerPrefs.SetInt("cleanerHiredAmount", currentAmount);
                    PlayerPrefs.Save();

                    if(currentAmount == 0)
                    {
                        AdvancedGameManager.Instance.cleanerHired = false;
                        PlayerPrefs.SetInt("cleanerHired", 0);
                        PlayerPrefs.Save();
                        GameCanvas.Instance.Button_Hire_Cleaner_Remover.SetActive(false);
                    }
                }
                GameCanvas.Instance.Text_Cleaner_Amount.text = currentAmount.ToString();
            }
        }

        public void Click_Button_Hire_Cashier()
        {
            // Lets check how many we have (max 4)
            int currentAmount = PlayerPrefs.GetInt("cashierHiredAmount", 0);
            if (currentAmount < 4)
            {
                AdvancedGameManager.Instance.cleanerHired = true;
                PlayerPrefs.SetInt("cashierHired", 1);
                currentAmount = currentAmount + 1;
                PlayerPrefs.SetInt("cashierHiredAmount", currentAmount);
                PlayerPrefs.Save();
                AdvancedGameManager.Instance.Cashiers[currentAmount - 1].SetActive(true);
                GameCanvas.Instance.Button_Hire_Cashier_Remover.SetActive(true);
                GameCanvas.Instance.Text_Cashier_Amount.text = currentAmount.ToString();
            }
        }

        public void Click_Button_Dismiss_Cashier()
        {
            if (AdvancedGameManager.Instance.cashierHired)
            {
                // Lets check how many we have (max 4)
                int currentAmount = PlayerPrefs.GetInt("cashierHiredAmount", 0);
                if (currentAmount > 0)
                {
                    AdvancedGameManager.Instance.Cashiers[currentAmount - 1].SetActive(false);
                    currentAmount = currentAmount - 1;
                    PlayerPrefs.SetInt("cashierHiredAmount", currentAmount);
                    PlayerPrefs.Save();

                    if (currentAmount == 0)
                    {
                        AdvancedGameManager.Instance.cashierHired = false;
                        PlayerPrefs.SetInt("cashierHired", 0);
                        PlayerPrefs.Save();
                        GameCanvas.Instance.Button_Hire_Cashier_Remover.SetActive(false);
                    }
                }
                GameCanvas.Instance.Text_Cashier_Amount.text = currentAmount.ToString();
            }
        }

        public void Toogle_Panel_MechanicSelectionForSure()
        {
            Panel_MechanicSelection.SetActive(false);
            GameCanvas.Instance.Panel_Inventory.SetActive(false);
            HeroPlayerScript.Instance.ActivatePlayer();
            CameraScript.Instance.enabled = true;
            if (AdvancedGameManager.Instance.controllerType == ControllerType.PC)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            AdvancedGameManager.Instance.CurrentMode = Mode.Free;
            Button_Free.GetComponent<UnityEngine.UI.Outline>().enabled = false;
            Button_Build.GetComponent<UnityEngine.UI.Outline>().enabled = false;
            Button_CallShipment.GetComponent<UnityEngine.UI.Outline>().enabled = false;
            Button_Clean.GetComponent<UnityEngine.UI.Outline>().enabled = false;
        }

        public void Toogle_Panel_MechanicSelection(bool? Forceclose = null)
        {
            if (Forceclose == null)
            {
                if (AdvancedGameManager.Instance.CurrentMode == Mode.InMechanicSelecting)
                {
                    Panel_MechanicSelection.SetActive(false);
                    GameCanvas.Instance.Panel_Inventory.SetActive(false);
                    HeroPlayerScript.Instance.ActivatePlayer();
                    CameraScript.Instance.enabled = true;
                    if (AdvancedGameManager.Instance.controllerType == ControllerType.PC)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                    AdvancedGameManager.Instance.CurrentMode = Mode.Free;
                }
                else
                {
                    InventoryManager.Instance.CancelLocating();
                    Panel_MechanicSelection.SetActive(true);
                    HeroPlayerScript.Instance.DeactivatePlayer();
                    AdvancedGameManager.Instance.CurrentMode = Mode.InMechanicSelecting;
                    CameraScript.Instance.enabled = false;
                    if (AdvancedGameManager.Instance.controllerType == ControllerType.PC)
                    {
                        Cursor.lockState = CursorLockMode.Confined;
                        Cursor.visible = true;
                    }
                }
            }
            else if (Forceclose != null && Forceclose.Value == true)
            {
                if (Panel_MechanicSelection.activeSelf)
                {
                    Panel_MechanicSelection.SetActive(false);
                    GameCanvas.Instance.Panel_Inventory.SetActive(false);
                    HeroPlayerScript.Instance.ActivatePlayer();
                    CameraScript.Instance.enabled = true;
                    if (AdvancedGameManager.Instance.controllerType == ControllerType.PC)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                }
            }
            Button_Free.GetComponent<UnityEngine.UI.Outline>().enabled = false;
            Button_Build.GetComponent<UnityEngine.UI.Outline>().enabled = false;
            Button_CallShipment.GetComponent<UnityEngine.UI.Outline>().enabled = false;
            Button_Clean.GetComponent<UnityEngine.UI.Outline>().enabled = false;
        }

        public void Click_Select_Mechanic(int i)
        {
            switch (i)
            {
                case 1:
                    FPSHandRotator.Instance.Switch_Hand(Hand_Type.Free);
                    Toogle_Panel_MechanicSelection();
                    break;
                case 2:
                    FPSHandRotator.Instance.Switch_Hand(Hand_Type.Build);
                    Panel_MechanicSelection.SetActive(false);
                    GameCanvas.Instance.Show_Inventory();
                    break;
                case 3:
                    FPSHandRotator.Instance.Switch_Hand(Hand_Type.Free);
                    Panel_MechanicSelection.SetActive(false);
                    GameCanvas.Instance.Show_OrderBoxShipment();
                    break;
                case 4:
                    FPSHandRotator.Instance.Switch_Hand(Hand_Type.Clean);
                    Toogle_Panel_MechanicSelection();
                    break;
            }
        }
    }
}