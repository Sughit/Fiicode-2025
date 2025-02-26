using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    public float smoothTime = 0.1f; // Timpul de netezire

    float x = 0.0f;
    float y = 0.0f;
    Vector3 currentVelocity; // Viteza curentă pentru SmoothDamp
    bool isColliding = false; // Flag pentru a verifica coliziunea
    Vector3 collisionPosition; // Poziția de coliziune
    Vector3 lastDesiredPosition; // Ultima poziție dorită

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        lastDesiredPosition = transform.position;
    }

    void LateUpdate()
    {
        if (target)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 desiredPosition = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;

            // Verificăm coliziunile
            RaycastHit hit;
            if (Physics.Linecast(target.position, desiredPosition, out hit))
            {
                // Ajustăm poziția camerei pentru a evita coliziunea
                collisionPosition = hit.point;
                isColliding = true;
                lastDesiredPosition = collisionPosition;
            }
            else
            {
                isColliding = false;
            }

            // Netezim tranziția doar când există coliziune
            if (isColliding)
            {
                if (Vector3.Distance(transform.position, collisionPosition) > 0.01f)
                {
                    // Netezim tranziția către poziția de coliziune
                    transform.position = Vector3.SmoothDamp(transform.position, collisionPosition, ref currentVelocity, smoothTime);
                }
                else
                {
                    // Folosim poziția de coliziune direct
                    transform.position = collisionPosition;
                }
            }
            else
            {
                // Folosim poziția dorită direct dacă nu există coliziuni
                transform.position = desiredPosition;
            }

            transform.rotation = rotation;
        }
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}