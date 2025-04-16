using UnityEngine;

public class CameraNavigation : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Transform autour duquel la caméra tourne")]
    public Transform pivot;
    [Tooltip("Caméra à déplacer pour le zoom")]
    public Transform cameraTransform;

    [Header("Rotation")]
    public float rotationSpeed = 5f;
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minZoomOffset = -5f;
    public float maxZoomOffset = 10f;

    private float currentX = 0f;
    private float currentY = 0f;
    private float zoomOffset = 0f;

    private Vector3 lastMousePosition;
    private bool isDragging = false;
    private Vector3 cameraStartLocalPos;

    void Start()
    {
        if (pivot == null || cameraTransform == null)
        {
            Debug.LogError("Pivot ou CameraTransform non assigné !");
            enabled = false;
            return;
        }

        // Initial angles from the pivot rotation
        Vector3 angles = pivot.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;

        // Save initial local position for zoom reference
        cameraStartLocalPos = cameraTransform.localPosition;
    }

    void Update()
    {
        HandleMouseDrag();
        HandleScrollZoom();
    }

    void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            currentX += delta.x * rotationSpeed * Time.deltaTime;
            currentY -= delta.y * rotationSpeed * Time.deltaTime;
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);

            pivot.rotation = Quaternion.Euler(currentY, currentX, 0);
            lastMousePosition = Input.mousePosition;
        }
    }

    void HandleScrollZoom()
    {
        float scrollDelta = Input.mouseScrollDelta.y;

        if (scrollDelta != 0)
        {
            zoomOffset += scrollDelta * zoomSpeed * Time.deltaTime;
            zoomOffset = Mathf.Clamp(zoomOffset, minZoomOffset, maxZoomOffset);

            cameraTransform.localPosition = new Vector3(cameraStartLocalPos.x, cameraStartLocalPos.y, cameraStartLocalPos.z + zoomOffset);
        }
    }
}
