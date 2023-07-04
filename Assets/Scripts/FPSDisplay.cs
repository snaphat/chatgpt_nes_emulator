using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private float deltaTime;

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        int fps = Mathf.RoundToInt(1.0f / deltaTime);
        GUI.Label(new Rect(10, 10, 100, 20), "FPS: " + fps);
    }
}
