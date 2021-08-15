using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    const float AIM_ERROR = 0.12f;

    private float initialLife;

    private float life;

    [SerializeField]
    private float lookRadius;

    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private GameObject bodySurface;

    Vector3 lookDirection = Vector3.forward;

    NavMeshAgent agent;

    private float triggerTimer = 0f;

    private Transform playerTransform;

    private Material bodyMaterial;

    [SerializeField]
    private GameObject firstGunPrafab;

    [SerializeField]
    private GameObject secondGunPrafab;

    [SerializeField]
    private AudioSource gunAudioSource;

    [SerializeField]
    private AudioSource walkAudioSource;

    [SerializeField]
    private AudioClip shotSFX;

    private int   gunDamage;
    private float gunFirePeriod;
    private int   gunCapacity;
    private int   remainingShots;

    private bool isGunOne = true;
    private bool isWalking = false;

    private Animator animator;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        initialLife = GameManager.Instance.initialEnemyLife;
        life = initialLife;

        agent = GetComponent<NavMeshAgent>();
        agent.enabled = true;

        bodyMaterial = bodySurface.GetComponent<Renderer>().material;
        animator = GetComponent<Animator>();

        SetColor();

        GunScript gs;
        if (Random.Range(0f, 1f) <= 0.7f)
        {
            Instantiate(firstGunPrafab, firePoint);
            gs = firstGunPrafab.GetComponent<GunScript>();
        } else
        {
            Instantiate(secondGunPrafab, firePoint);
            gs = secondGunPrafab.GetComponent<GunScript>();
            isGunOne = false;
        }
        gunCapacity = gs.capacity;
        gunFirePeriod = gs.firePeriod;
        gunDamage = gs.damage;
        remainingShots = gunCapacity;
    }

    private void Update()
    {
        if (GameManager.Instance.running)
        {
            walkAudioSource.Pause();
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.running)
        {
            bool playerInSight = false;
            for (int i = 0; i < 5; i++)
            {
                playerInSight = SeekPlayer();
            }

            EvaluateAction(playerInSight);
        }

        Vector3 movement = (agent.velocity).normalized;

        float velocityZ = Vector3.Dot(movement, transform.forward);
        float velocityX = Vector3.Dot(movement, transform.right);

        animator.SetFloat("VelocityZ", velocityZ, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityX", velocityX, 0.1f, Time.deltaTime);

        if (movement.magnitude > 0.001f)
        {
            if (!isWalking)
            {
                isWalking = true;
                walkAudioSource.Play();
            }
        } else
        {
            walkAudioSource.Stop();
            isWalking = false;
        }
    }

    bool SeekPlayer()
    {

        RaycastHit hit;
        Ray ray = new Ray(transform.position, lookDirection);
        if (Physics.Raycast(ray, out hit, lookRadius) && hit.collider.gameObject.CompareTag("Player"))
        {
            agent.SetDestination(playerTransform.position);
            Vector3 directionToPlayer = (hit.collider.gameObject.transform.position - transform.position).normalized;
            Vector3 directionToLook = lookDirection = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
            Quaternion newRotation = Quaternion.LookRotation(directionToLook);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.fixedDeltaTime * 2);
            return true;
        } else
        {
            lookDirection = Quaternion.Euler(0, 5.01f, 0) * lookDirection;
            return false;
        }
    }

    private void EvaluateAction(bool playerInSight)
    {
        triggerTimer += Time.deltaTime;
        float distanceToPlayer = Vector3.Distance(playerTransform.position, transform.position);
        if (distanceToPlayer <= 20f && playerInSight)
        {
            if (triggerTimer >= gunFirePeriod + Random.Range(0.5f, 1.5f) + (remainingShots > 0 ? 0 : 3))
            {
                if (remainingShots <= 0)
                {
                    remainingShots = gunCapacity;
                }

                remainingShots--;
                triggerTimer = 0f;
                FireGun();
            }
        }
    }

    private void FireGun()
    {
        gunAudioSource.PlayOneShot(shotSFX);

        Vector3 precisionDisturbance = new Vector3(Random.Range(-AIM_ERROR, AIM_ERROR), Random.Range(-AIM_ERROR, AIM_ERROR), 0);
        Vector3 aim = (firePoint.transform.forward + precisionDisturbance).normalized;
        Ray bulletRay = new Ray(firePoint.position, aim);
        RaycastHit hit;

        if (Physics.Raycast(bulletRay, out hit, 100))
        {
            Debug.DrawRay(firePoint.position, (hit.point - firePoint.position).normalized * 200, Color.red, 0.5f);
            GameObject hitGameObj = hit.collider.gameObject;
            if (hitGameObj.CompareTag("Player"))
            {
                GameManager.Instance.GetShot(gunDamage);
            }
        }
    }

    public void GetShot(int damage)
    {
        life -= damage;

        if (life <= 0)
        {
            Destroy(gameObject);
            if (!isGunOne && Random.Range(0f, 1f) < 0.2f)
            {
                GameObject droppedGun = Instantiate(secondGunPrafab, transform.position, Quaternion.Euler(Vector3.zero));
                droppedGun.GetComponent<Collider>().enabled = true;
            }
        }

        SetColor();
    }

    public void SetColor()
    {
        float h = (life - 5) / (initialLife - 5) * 120;
        bodyMaterial.color = Color.HSVToRGB(h / 360, 1, 1);
    }
}
