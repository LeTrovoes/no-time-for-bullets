using UnityEngine;

public class PickUpController : MonoBehaviour
{
    [SerializeField]
    private float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(new Vector3(12, 30, 45) * Time.deltaTime * speed);
    }
}
