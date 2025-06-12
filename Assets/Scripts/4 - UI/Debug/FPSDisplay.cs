// 07/06/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private float deltaTime = 0.0f;

    void Update()
    {
        // Calculate the time between frames
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        // Set the style for the FPS display text
        GUIStyle style = new GUIStyle();

        int width = Screen.width, height = Screen.height;
        Rect rect = new Rect(width - 100, 10, 90, 20);
        style.alignment = TextAnchor.UpperRight;
        style.fontSize = height * 2 / 100;
        style.normal.textColor = Color.white;

        // Calculate FPS and display it
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}