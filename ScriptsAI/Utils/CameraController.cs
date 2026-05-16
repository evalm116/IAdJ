using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Rotación")]
    public float mouseSensitivity = 2f;
    public bool invertY = false;

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minFOV = 20f;
    public float maxFOV = 80f;

    [Header("Movimiento")]
    public float moveSpeed = 10f;

    [Header("Activación")]
    public KeyCode toggleKey = KeyCode.C;

    private float rotX = 0f;
    private float rotY = 0f;
    private Camera cam;
    private bool freeCamActive = false;

    // Posición y rotación originales
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float originalFOV;

    void Start()
    {
        cam = GetComponent<Camera>();
        rotX = transform.eulerAngles.y;
        rotY = transform.eulerAngles.x;

        // Guardar estado original
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalFOV = cam.fieldOfView;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            freeCamActive = !freeCamActive;

            if (freeCamActive)
            {
                // Activar modo libre
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // Sincronizar rotación con la posición actual para evitar saltos
                rotX = transform.eulerAngles.y;
                rotY = transform.eulerAngles.x;
            }
            else
            {
                // Volver al estado original
                transform.position = originalPosition;
                transform.rotation = originalRotation;
                cam.fieldOfView = originalFOV;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        if (freeCamActive)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? 1 : -1);

            rotX += mouseX;
            rotY += mouseY;
            rotY = Mathf.Clamp(rotY, -89f, 89f);

            transform.rotation = Quaternion.Euler(rotY, rotX, 0f);

            float h = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
            float v = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
            transform.Translate(h, 0, v, Space.Self);
        }

        // Zoom solo en modo libre
        if (freeCamActive)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                cam.fieldOfView -= scroll * zoomSpeed * 10f;
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFOV, maxFOV);
            }
        }
    }
}