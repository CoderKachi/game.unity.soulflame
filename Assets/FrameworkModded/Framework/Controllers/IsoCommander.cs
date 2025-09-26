using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Framework;

// Requires
[RequireComponent(typeof(MovementComponent), typeof(ModelComponent), typeof(CameraComponent))]
[RequireComponent(typeof(ScreenToPlaneComponent))]

public class IsoCommander : IsoCharacterController
{
    // Services
    private InputService InputService;

    // Components
    protected CameraComponent _CameraComponent;
    protected ScreenToPlaneComponent _ScreenToPlaneComponent;

    // Variables
    protected Vector2 _currentMousePosition;
    [SerializeField] protected GameObject[] _units = default;
    [SerializeField] protected List<GameObject> _currentSelected;

    // Control States
    protected bool _actionIsModified = false;
    protected bool _actionIsDragging = false;
    protected Vector2 _dragStartPosition;
    protected Vector2 _dragEndPosition;
    protected float _dragThreshold = 10f; // Pixels

    // Debugging
    protected Texture2D _selectionTexture;
    [SerializeField] protected Color _selectionColor = default;

    // Configuration
    public LayerMask selectableLayers;
    public LayerMask controllableLayers;
    public bool panTowardsMouse;

    protected override void Awake()
    {
        base.Awake();

        InputService = Game.GetService<InputService>();

        InputService.Connect("Gameplay/ActionPrimary", ActionPrimary);
        InputService.Connect("Gameplay/ActionPrimaryModifier", ActionPrimaryModifier);
        InputService.Connect("Gameplay/Look", LookWithMouse);
        InputService.Connect("Gameplay/Move", MoveWithKeyboard);
        InputService.Connect("Gameplay/Scroll", Zoom);

        TryGetComponent<CameraComponent>(out _CameraComponent);
        TryGetComponent<ScreenToPlaneComponent>(out _ScreenToPlaneComponent);

        // 1x1 white texture
        _selectionTexture = new Texture2D(1, 1);
        _selectionTexture.SetPixel(0, 0, Color.white);
        _selectionTexture.Apply();
    }

    protected override void Update()
    {
        Vector3 worldMousePosition = _ScreenToPlaneComponent.ScreenToPlane(_currentMousePosition);

        Move(_moveDirection);
        LookTowards(worldMousePosition);

        if (panTowardsMouse)
        {
            _CameraComponent.PanTowards(worldMousePosition);
        }
    }

    bool IsControllable(GameObject unit)
    {
        // Convert unit.layer to a bitmask and AND with controllable mask
        return (controllableLayers.value & (1 << unit.layer)) != 0;
    }

    void TrySelectUnits()
    {
        float dragDistance = Vector2.Distance(_dragStartPosition, _dragEndPosition);

        if (dragDistance < _dragThreshold)
        {
            // Select a single unit
            Ray ray = _CameraComponent.GetCamera().ScreenPointToRay(_currentMousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 999f, controllableLayers))
            {
                if (!_actionIsModified)
                {
                    // Clear if not shift clicking
                    _currentSelected.Clear();
                }

                // Add the unit to the selected list
                _currentSelected.Add(hit.collider.gameObject);
            }
            else
            {
                _currentSelected.Clear();
            }
        }
        else
        {
            Rect selectionRect = ScreenToRect(_dragStartPosition, _dragEndPosition);

            // Select multiple units
            _currentSelected.Clear();

            _units = GameObject.FindGameObjectsWithTag("Unit");

            foreach (GameObject unit in _units)
            {
                if (!IsControllable(unit)) continue;
                Renderer unitRenderer = unit.GetComponent<Renderer>();
                if (unitRenderer == null) continue;
                Rect unitRect = RendererBoundsToRect(unitRenderer, _CameraComponent.GetCamera());

                if (selectionRect.Overlaps(unitRect, true))
                {
                    _currentSelected.Add(unit);
                }
            }
        }
    }

    void ActionPrimary(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                _dragStartPosition = _currentMousePosition;
                _actionIsDragging = true;
                break;
            case InputActionPhase.Canceled:
                _dragEndPosition = _currentMousePosition;
                _actionIsDragging = false;
                TrySelectUnits();
                break;
        }
    }

    void ActionPrimaryModifier(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                _actionIsModified = true;
                break;
            case InputActionPhase.Canceled:
                _actionIsModified = false;
                break;
        }
    }

    void LookWithMouse(InputAction.CallbackContext context)
    {
        _currentMousePosition = context.ReadValue<Vector2>();
    }

    void MoveWithKeyboard(InputAction.CallbackContext context)
    {
        _moveDirection = context.ReadValue<Vector2>();
    }

    void Zoom(InputAction.CallbackContext context)
    {
        _CameraComponent.Zoom(-context.ReadValue<float>());
    }

    void OnDestroy()
    {
        InputService.Disconnect("Gameplay/ActionPrimary", ActionPrimary);
        InputService.Disconnect("Gameplay/Look", LookWithMouse);
        InputService.Disconnect("Gameplay/Move", MoveWithKeyboard);
        InputService.Disconnect("Gameplay/Scroll", Zoom);
    }

    Rect ScreenToRect(Vector2 startPosition, Vector2 endPosition)
    {
        // Calculate Left Corner of Rect
        float x = Mathf.Min(startPosition.x, endPosition.x);
        float y = Mathf.Min(Screen.height - startPosition.y, Screen.height - endPosition.y); // flip Y

        float width = Mathf.Abs(endPosition.x - startPosition.x);
        float height = Mathf.Abs(endPosition.y - startPosition.y);

        return new Rect(x, y, width, height);
    }

    Rect RendererBoundsToRect(Renderer renderer, Camera cam)
    {
        Bounds b = renderer.bounds;

        Vector3[] corners = new Vector3[8];
        corners[0] = new Vector3(b.min.x, b.min.y, b.min.z);
        corners[1] = new Vector3(b.min.x, b.min.y, b.max.z);
        corners[2] = new Vector3(b.min.x, b.max.y, b.min.z);
        corners[3] = new Vector3(b.min.x, b.max.y, b.max.z);
        corners[4] = new Vector3(b.max.x, b.min.y, b.min.z);
        corners[5] = new Vector3(b.max.x, b.min.y, b.max.z);
        corners[6] = new Vector3(b.max.x, b.max.y, b.min.z);
        corners[7] = new Vector3(b.max.x, b.max.y, b.max.z);

        float xMin = float.MaxValue;
        float yMin = float.MaxValue;
        float xMax = float.MinValue;
        float yMax = float.MinValue;

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 screenPoint = cam.WorldToScreenPoint(corners[i]);

            screenPoint.y = Screen.height - screenPoint.y;

            xMin = Mathf.Min(xMin, screenPoint.x);
            yMin = Mathf.Min(yMin, screenPoint.y);
            xMax = Mathf.Max(xMax, screenPoint.x);
            yMax = Mathf.Max(yMax, screenPoint.y);
        }

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }
    
    void OnGUI()
    {
        if (_actionIsDragging)
        {
            Rect rect = ScreenToRect(_dragStartPosition, Mouse.current.position.ReadValue());

            // Set color and draw
            Color oldColor = GUI.color;
            GUI.color = _selectionColor;
            GUI.DrawTexture(rect, _selectionTexture);
            GUI.color = oldColor;
        }
    }
}
