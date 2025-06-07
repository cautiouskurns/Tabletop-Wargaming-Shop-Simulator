using UnityEngine;

namespace TabletopShop
{
    /// <summary>
    /// Simple test to verify OnGUI is working in the project
    /// </summary>
    public class SimpleGUITest : MonoBehaviour
    {
        private void OnGUI()
        {
            GUI.Label(new Rect(10, 50, 300, 20), "SimpleGUITest: OnGUI is working!");
            GUI.Label(new Rect(10, 70, 300, 20), $"Frame: {Time.frameCount}");
            GUI.Label(new Rect(10, 90, 300, 20), $"Time: {Time.time:F1}s");
        }
    }
}
