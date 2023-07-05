using System.Collections;
using System.IO;
using Emulation;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.InputSystem;
using static Emulation.Globals;

public class EmulatorMonoBehaviour : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Texture2D texture;

    private Emulator emulator;
    private Controller controller;
    private Color32[] screenBuffer;
    private bool isRunning;

    private void Start()
    {
        Application.targetFrameRate = 60;
        //QualitySettings.vSyncCount = 0;
        Physics.simulationMode = SimulationMode.Script;
        Physics2D.simulationMode = SimulationMode2D.Script;
        spriteRenderer = GetComponent<SpriteRenderer>();

        Texture2D texture = new Texture2D(SCREEN_WIDTH, SCREEN_HEIGHT);
        texture.filterMode = FilterMode.Point;
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT), new Vector2(0.5f, 0.5f), 1f);
        this.texture = spriteRenderer.sprite.texture;

        ShowLoadDialog();
    }

    private void ShowLoadDialog()
    {
        FileBrowser.ShowLoadDialog(OnSuccess, OnCancel, FileBrowser.PickMode.Files, false, null, null, "Load Rom", "Load");
    }

    private void OnSuccess(string[] paths)
    {
        // Create an instance of the emulator and load the ROM
        emulator = new Emulator(paths[0]);

        screenBuffer = emulator.GetPPU().GetScreenBuffer();
        controller = emulator.GetController();

        isRunning = true;
    }

    private void OnCancel()
    {
        ShowLoadDialog();
    }

    private void Update()
    {
        if (isRunning)
        {
            emulator.Run();

            texture.SetPixels32(screenBuffer);
            texture.Apply();
        }
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        controller.SetButtonState(1, Controller.BUTTON_SELECT, context.performed);
    }

    public void OnStart(InputAction.CallbackContext context)
    {
        controller.SetButtonState(1, Controller.BUTTON_START, context.performed);
    }

    public void OnB(InputAction.CallbackContext context)
    {
        controller.SetButtonState(1, Controller.BUTTON_B, context.performed);
    }

    public void OnA(InputAction.CallbackContext context)
    {
        controller.SetButtonState(1, Controller.BUTTON_A, context.performed);
    }

    public void OnUp(InputAction.CallbackContext context)
    {
        controller.SetButtonState(1, Controller.BUTTON_UP, context.performed);
    }

    public void OnDown(InputAction.CallbackContext context)
    {
        controller.SetButtonState(1, Controller.BUTTON_DOWN, context.performed);
    }

    public void OnLeft(InputAction.CallbackContext context)
    {
        controller.SetButtonState(1, Controller.BUTTON_LEFT, context.performed);
    }

    public void OnRight(InputAction.CallbackContext context)
    {
        controller.SetButtonState(1, Controller.BUTTON_RIGHT, context.performed);
    }
}
