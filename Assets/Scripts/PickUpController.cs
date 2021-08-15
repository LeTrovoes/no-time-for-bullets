using UnityEngine;

public class PickUpController : MonoBehaviour
{
    [SerializeField]
    private float speed;

    private Vector3 rotation = new Vector3(12, 30, 45);

    void Update()
    {
        transform.Rotate(speed * Time.deltaTime * rotation);
    }
}
