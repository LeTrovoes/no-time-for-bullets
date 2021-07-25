using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private int score;

    [SerializeField]
    [Range(0, 100)]
    private float life;

    [SerializeField]
    private int initialPickups;

    [SerializeField]
    private float initialCountdown;

    private float countdown;

    [SerializeField]
    private GameObject plane;

    [SerializeField]
    private GameObject pickUpPrefab;

    [SerializeField]
    private Text lifeText;

    [SerializeField]
    private Text scoreText;

    [SerializeField]
    private Text countdownText;

    [SerializeField]
    private Text ammoText;

    [SerializeField]
    private Text emptyMagText;

    [SerializeField]
    private Image bloodyPannel;

    private static GameManager instance = null;

    private bool showBloodyPannel = false;
    private float bloodyTimer = 0;

    [SerializeField]
    private Material wallMaterial;

    public int gunDamage;
    public int gunCapacity;
    public bool gunAutomatic;
    public float gunFirePeriod;
    public int remainingShots;
    public int remainingAmmo;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("GM").AddComponent<GameManager>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        PositionPickUps();
        lifeText.text = "Life: " + life;
        scoreText.text = score.ToString("00") + '/' + initialPickups.ToString("00");
        countdownText.text = countdown.ToString("F2");
        countdown = initialCountdown;

        Cursor.visible = false;
        Screen.fullScreen = true;
    }

    private void Update()
    {
        SetBloodyColor();
        SetWallColor();
        ammoText.text = remainingShots.ToString("00") + '/' + gunCapacity.ToString("00");

        emptyMagText.gameObject.SetActive(remainingShots == 0);
    }

    private void FixedUpdate()
    {
        countdown = Mathf.Clamp(countdown - Time.fixedDeltaTime, 0, Mathf.Infinity);
        countdownText.text = countdown.ToString("F2");
        if (countdown == 0)
        {
            print("ACABOUUUUU");
            Time.timeScale = 0f;
        }
    }

    public void Score()
    {
        score++;
        scoreText.text = score.ToString("00") + '/' + initialPickups.ToString("00");

        if (score == initialPickups)
        {
            print("É TETRA");
            Time.timeScale = 0f;
        }
    }

    public void GetShot(int damage)
    {
        bloodyTimer = 0f;
        showBloodyPannel = true;

        life -= damage;
        life = Mathf.Clamp(life, 0, Mathf.Infinity);

        lifeText.text = "Life: " + life;

        if (life <= 0)
        {
            Debug.Log("WASTED");
            Time.timeScale = 0f;
        }
    }

    private void PositionPickUps()
    {
        float worldLimitX = plane.GetComponent<Collider>().bounds.size.x / 2;
        float worldLimitY = plane.GetComponent<Collider>().bounds.size.z / 2;

        Vector3[] pickUps = new Vector3[initialPickups];

        for (int i = 0; i < initialPickups; i++)
        {
            Vector3 position;

            do {
                float x = Random.Range(-worldLimitX, worldLimitX);
                float y = Random.Range(-worldLimitY, worldLimitY);
                position = new Vector3(x, pickUpPrefab.transform.position.y, y);
            } while (!IsValidPickUpPosition(position, pickUps));

            pickUps[i] = position;

            GameObject newPickUp = Instantiate(pickUpPrefab);
            newPickUp.transform.position = position;
        }
    }

    // TODO: verificar proximidade com paredes e com o player
    private bool IsValidPickUpPosition(Vector3 position, Vector3[] pickUps)
    {
        foreach (var pickUp in pickUps)
        {
            if (Vector3.Distance(position, pickUp) < 5f)
            {
                return false;
            }
        }
        return true;
    }

    private void SetBloodyColor()
    {
        if (showBloodyPannel)
        {
            bloodyTimer += Time.deltaTime;
            float alpha = 0.625f / (bloodyTimer + 0.5f) - 0.25f;
            bloodyPannel.color = new Color(1, 0, 0, alpha);

            if (bloodyTimer > 2)
            {
                bloodyPannel.color = new Color(1, 0, 0, 0);
                bloodyTimer = 0f;
                showBloodyPannel = false;
            }
        }
    }

    public void SetWallColor()
    {
        float h = 1 - (countdown / initialCountdown) / 2;
        wallMaterial.color = Color.HSVToRGB(h, 1, 1);
    }
}
