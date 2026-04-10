using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerController controls;

    // Eventos de cámara
    //public event Action<Vector2> OnCameraMove;
    //public event Action<float> OnCameraZoom;

    // Eventos de gameplay
    public event Action OnPause;
    public event Action OnRestart;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        controls = new PlayerController();

        // Cámara
        //controls.Camera.Move.performed += ctx => OnCameraMove?.Invoke(ctx.ReadValue<Vector2>());
        //controls.Camera.Move.canceled += ctx => OnCameraMove?.Invoke(Vector2.zero);
        //controls.Camera.Zoom.performed += ctx => OnCameraZoom?.Invoke(ctx.ReadValue<float>());
        //controls.Camera.Zoom.canceled += ctx => OnCameraZoom?.Invoke(0);

        // Player
        controls.Player.Pause.performed += _ => OnPause?.Invoke();
        controls.Player.Restart.performed += _ => OnRestart?.Invoke();
    }

    void OnEnable()
    {
        controls.Player.Enable();
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }
}