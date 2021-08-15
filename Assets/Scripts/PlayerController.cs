using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed;

    private Animator animator;

    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private GameObject firstGunPrafab;

    [SerializeField]
    private GameObject secondGunPrafab;

    [SerializeField]
    private AudioSource pickUpAudioSource;

    [SerializeField]
    private AudioSource gunAudioSource;

    [SerializeField]
    private AudioSource walkAudioSource;

    [SerializeField]
    private AudioClip shotSFX;

    private float fireTimer = 0f;
    private float reloadTimer = 0f;
    private bool isGunOne = true;

    private bool isWalking = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        ChangeGun(firstGunPrafab);
    }

    private bool lastFireTriggerState = false;
    private bool GetFireTrigger()
    {
        bool currentTriggerState = Input.GetAxisRaw("TriggerFire1") >= 0.3f;
        if (currentTriggerState != lastFireTriggerState)
        {
            lastFireTriggerState = currentTriggerState;
            if (currentTriggerState)
            {
                return true;
            }
        }
        return false;
    }

    void Update()
    {
        if (!GameManager.Instance.running)
        {
            walkAudioSource.Pause();
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical);

        if (movement.magnitude > 0)
        {
            movement.Normalize();
            movement *= Time.deltaTime * speed;
            movement = Quaternion.Euler(0, transform.localEulerAngles.y, 0) * movement;
            transform.Translate(movement, Space.World);

            if (!isWalking)
            {
                isWalking = true;
                walkAudioSource.Play();
            }
        } else
        {
            isWalking = false;
            walkAudioSource.Stop();
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
            SceneManager.Instance.EndReload();
            reloadTimer = 0f;
            if (!GameManager.Instance.gunAutomatic && (Input.GetButtonDown("Fire1") || GetFireTrigger()) || GameManager.Instance.gunAutomatic && (Input.GetButton("Fire1") || Input.GetAxisRaw("TriggerFire1") >= 0.3f))
            {
                fireTimer = 0f;
                GameManager.Instance.remainingShots--;
                FireGun();
            }
        }

        /* Reload Gun */
        if (Input.GetButtonDown("Reload"))
        {
            if (!GameManager.Instance.gunAutomatic)
            {
                SceneManager.Instance.StartReload();
                reloadTimer = 3f;
                fireTimer = 0f;
                GameManager.Instance.remainingShots = GameManager.Instance.gunCapacity;
            } else if (GameManager.Instance.remainingShots == 0)
            {
                // NOTE: same as drop if holding a empty gun 2
                isGunOne = true;
                Destroy(firePoint.GetChild(1).gameObject);
                firePoint.GetChild(0).gameObject.SetActive(true);
                ChangeGun(firstGunPrafab);
            }
        }

        /* Drop Gun */
        if (Input.GetButtonDown("Drop"))
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
        gunAudioSource.PlayOneShot(shotSFX);
        Ray targetRay = Camera.main.ViewportPointToRay(Vector3.one * 0.5f);

        RaycastHit target;
        RaycastHit hit;

        if (Physics.Raycast(targetRay, out target, 100, LayerMask.NameToLayer("Player")))
        {
            Ray bulletRay = new Ray(firePoint.position, (target.point - firePoint.position).normalized);

            if (Physics.Raycast(bulletRay, out hit, 100, LayerMask.NameToLayer("Player"))) {
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
            pickUpAudioSource.Play();
        }
        if (other.CompareTag("Drop"))
        {
            SceneManager.Instance.EnterDropTrigger();
        }
        if (other.CompareTag("Heal"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.life = Mathf.Clamp(GameManager.Instance.life + 30, 0, 100);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Drop"))
        {
            SceneManager.Instance.LeaveDropTrigger();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Drop"))
        {
            if (Input.GetButton("Pick"))
            {
                Destroy(other.gameObject);
                SceneManager.Instance.LeaveDropTrigger();
                firePoint.GetChild(0).gameObject.SetActive(false);
                Instantiate(secondGunPrafab, firePoint);
                ChangeGun(secondGunPrafab);
                isGunOne = false;
            }
        }
    }

    private void ChangeGun(GameObject gun)
    {
        reloadTimer = 0f;
        SceneManager.Instance.EndReload();
        GunScript gs = gun.GetComponent<GunScript>();
        GameManager.Instance.gunDamage = gs.damage;
        GameManager.Instance.gunCapacity = gs.capacity;
        GameManager.Instance.gunAutomatic = gs.automatic;
        GameManager.Instance.gunFirePeriod = gs.firePeriod;
        GameManager.Instance.remainingShots = GameManager.Instance.gunCapacity;
    }
}
