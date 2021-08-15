using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class SceneManager : MonoBehaviour
{
    [SerializeField]
    private Text scoreText;

    [SerializeField]
    private Text ammoText;

    [SerializeField]
    private Text emptyMagText;

    [SerializeField]
    private GameObject winText;

    [SerializeField]
    private GameObject diedText;

    [SerializeField]
    private GameObject timesUpText;

    [SerializeField]
    private Image bloodyPannel;

    [SerializeField]
    private GameObject pauseMenu;


    [SerializeField]
    private Text grabDropTooltip;

    [SerializeField]
    private Text reloadingText;

    [SerializeField]
    private Slider lifeSlider;

    private static SceneManager instance = null;

    [SerializeField]
    private Material wallMaterial;

    public static SceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("SceneManager").AddComponent<SceneManager>();
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
        GameManager.Instance.CreateGameWorld();
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        SetWallColor();
        ammoText.text = GameManager.Instance.remainingShots.ToString("00") + "<size=18>/" + GameManager.Instance.gunCapacity.ToString("00") + "</size>";

        emptyMagText.gameObject.SetActive(GameManager.Instance.remainingShots == 0);
        scoreText.text = (GameManager.Instance.initialPickups - GameManager.Instance.score).ToString("00");

        lifeSlider.value = GameManager.Instance.life / 100f;

        if (GameManager.Instance.life <= 15)
        {
            lifeSlider.fillRect.GetComponent<Image>().color = Color.red;
        } else if (GameManager.Instance.life <= 30) {
            lifeSlider.fillRect.GetComponent<Image>().color = Color.yellow;
        } else {
            lifeSlider.fillRect.GetComponent<Image>().color = Color.green;
        }

        if (Input.GetButtonDown("Pause"))
        {
            GameManager.Instance.TogglePause();
        }
        pauseMenu.SetActive(!GameManager.Instance.running && !GameManager.Instance.gameOver);
    }

    public void GetShot()
    {
        bloodyPannel.DOFade(1, 0);
        bloodyPannel.DOFade(0, 2).SetEase(Ease.OutQuart);
    }

    public void SetWallColor()
    {
        float h = 1 - (GameManager.Instance.countdown / GameManager.Instance.initialCountdown) / 2;
        wallMaterial.color = Color.HSVToRGB(h, 1, 1);
    }

    public void GameEnded()
    {
        bloodyPannel.DOFade(0, 0);

        if (GameManager.Instance.life <= 0)
        {
            diedText.SetActive(true);
        }
        else if (GameManager.Instance.countdown <= 0)
        {
            timesUpText.SetActive(true);
        }
        else
        {
            winText.SetActive(true);
        }

        StartCoroutine(GoBackToMenuScene());
    }

    private IEnumerator GoBackToMenuScene()
    {
        yield return new WaitForSecondsRealtime(3);
        DOTween.CompleteAll();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
    }

    public void EnterDropTrigger() {
        grabDropTooltip.gameObject.SetActive(true);
    }

    public void LeaveDropTrigger()
    {
        grabDropTooltip.gameObject.SetActive(false);
    }

    public void StartReload()
    {
        reloadingText.gameObject.SetActive(true);
    }

    public void EndReload()
    {
        reloadingText.gameObject.SetActive(false);
    }
}
