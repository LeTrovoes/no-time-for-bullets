using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public Animator animator;
    public Transform firePoint;

    [SerializeField]
    private Text grabDropTooltip;

    [SerializeField]
    private Text reloadingText;

    [SerializeField]
    private GameObject firstGunPrafab;

    [SerializeField]
    private GameObject secondGunPrafab;

    private float fireTimer = 0f;
    private float reloadTimer = 0f;
    private bool isGunOne = true;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        ChangeGun(firstGunPrafab);
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical);

        if (movement.magnitude > 0)
        {
            movement.Normalize();
            movement *= Time.deltaTime * speed;
            movement = Quaternion.Euler(0, transform.localEulerAngles.y, 0) * movement;
            transform.Translate(movement, Space.World);
        }

        float velocityZ = Vector3.Dot(movement.normalized, transform.forward);
        float velocityX = Vector3.Dot(movement.normalized, transform.right);

        animator.SetFloat("VelocityZ", velocityZ, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityX", velocityX, 0.1f, Time.deltaTime);


        float h = Input.GetAxis("Mouse X") * 5;
        transform.Rotate(0, h, 0);

        fireTimer += Time.deltaTime;

        if (fireTimer >= GameManager.Instance.gunFirePeriod + reloadTimer && GameManager.Instance.remainingShots > 0)
        {
            reloadingText.gameObject.SetActive(false);
            reloadTimer = 0f;
            if (!GameManager.Instance.gunAutomatic && Input.GetButtonDown("Fire1") || GameManager.Instance.gunAutomatic && Input.GetButton("Fire1"))
            {
                fireTimer = 0f;
                GameManager.Instance.remainingShots--;
                FireGun();
            }
        }

        /* Reload Gun */
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!GameManager.Instance.gunAutomatic) /* TODO: verificar se há munição restante */
            {
                reloadingText.gameObject.SetActive(true);
                reloadTimer = 3f;
                fireTimer = 0f;
                GameManager.Instance.remainingShots = GameManager.Instance.gunCapacity;
            }
        }

        /* Drop Gun */
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!isGunOne)
            {
                isGunOne = true;
                Destroy(firePoint.GetChild(1).gameObject);
                firePoint.GetChild(0).gameObject.SetActive(true);
                ChangeGun(firstGunPrafab);
            }
        }
    }

    void FireGun()
    {
        Ray targetRay = Camera.main.ViewportPointToRay(Vector3.one * 0.5f);
        // Debug.DrawRay(targetRay.origin, targetRay.direction * 200, Color.blue, 2f);

        RaycastHit target;
        RaycastHit hit;

        if (Physics.Raycast(targetRay, out target, 100))
        {
            Ray bulletRay = new Ray(firePoint.position, (target.point - firePoint.position).normalized);

            if (Physics.Raycast(bulletRay, out hit, 100)) {
                // Debug.DrawRay(firePoint.position, (hit.point - firePoint.position).normalized * 200, Color.red, 2f);
                GameObject hitGameObj = hit.collider.gameObject;
                if (hitGameObj.CompareTag("Enemy"))
                {
                    hitGameObj.GetComponent<EnemyScript>().GetShot(GameManager.Instance.gunDamage);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            GameManager.Instance.Score();
        }
        if (other.CompareTag("Drop"))
        {
            grabDropTooltip.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Drop"))
        {
            grabDropTooltip.gameObject.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Drop"))
        {
            if (Input.GetKey(KeyCode.E))
            {
                Destroy(other.gameObject);
                grabDropTooltip.gameObject.SetActive(false);
                firePoint.GetChild(0).gameObject.SetActive(false);
                Instantiate(secondGunPrafab, firePoint);
                ChangeGun(secondGunPrafab);
                isGunOne = false;
            }
        }
    }

    private void ChangeGun(GameObject gun)
    {
        GunScript gs = gun.GetComponent<GunScript>();
        GameManager.Instance.gunDamage = gs.damage;
        GameManager.Instance.gunCapacity = gs.capacity;
        GameManager.Instance.gunAutomatic = gs.automatic;
        GameManager.Instance.gunFirePeriod = gs.firePeriod;
        GameManager.Instance.remainingShots = GameManager.Instance.gunCapacity;
    }

}
