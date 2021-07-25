using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 offset;
    public GameObject player;
    public GameObject firePoint;

    void Start()
    {
        offset = transform.position - player.transform.position;
    }

    void Update()
    {
        transform.position = player.transform.position + offset;
        float y = player.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y;
        transform.RotateAround(player.transform.position, Vector3.up, y);

        float toRotate = -Input.GetAxis("Mouse Y");

        float currentRotation = transform.rotation.eulerAngles.x > 180 ? transform.rotation.eulerAngles.x - 360 : transform.rotation.eulerAngles.x;
        float newRotation = currentRotation + toRotate;

        if (newRotation > 50)
        {
            toRotate = 50 - currentRotation;
        }
        if (newRotation < -30)
        {
            toRotate = -30 - currentRotation;
        }


        transform.RotateAround(player.transform.position + new Vector3(0, 1.7f, 0), player.transform.right, toRotate);
        firePoint.transform.Rotate(toRotate, 0, 0);


        offset = transform.position - player.transform.position;
    }
}
