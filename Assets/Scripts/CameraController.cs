using UnityEngine;





public class CameraController : MonoBehaviour
{
    [Header("Panning Settings")]
    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public bool useScreenEdgePan = true;

    [Header("Zoom Settings")]
    public float scrollSpeed = 20f;
    public float minY = 10f;
    public float maxY = 100f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 100f;

    private void Update()
    {
        HandlePan();
        HandleZoom();
        HandleRotation();
    }

    private void HandlePan()
    {
        float vertical = 0f;
        float horizontal = 0f;

        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;

        if (useScreenEdgePan)
        {
            if (Input.mousePosition.y >= Screen.height - panBorderThickness) vertical += 1f;
            if (Input.mousePosition.y <= panBorderThickness) vertical -= 1f;
            if (Input.mousePosition.x >= Screen.width - panBorderThickness) horizontal += 1f;
            if (Input.mousePosition.x <= panBorderThickness) horizontal -= 1f;
        }

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 direction = forward * vertical + right * horizontal;
        if (direction.sqrMagnitude > 1f)
            direction.Normalize();

        transform.Translate(direction * panSpeed * Time.deltaTime, Space.World);
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 pos = transform.position;
            pos.y -= scroll * scrollSpeed;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            float horizontal = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, horizontal, Space.World);
        }
    }
} 
