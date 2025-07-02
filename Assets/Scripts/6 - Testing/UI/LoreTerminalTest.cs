using UnityEngine;

namespace TabletopShop
{
    public class LoreTerminalTest : MonoBehaviour
    {
        [SerializeField] private CodexTerminalUI terminalUI;

        public void OpenTerminal()
        {
            terminalUI.gameObject.SetActive(true);
        }
    }
}