using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    [SerializeField]
    private GameObject mainButtonsGroup;

    [SerializeField]
    private Button startGameBtn;

    [SerializeField]
    private Button endGameBtn;

    [SerializeField]
    private GameObject levelButtonsGroup;

    [SerializeField]
    private Button easyBtn;

    [SerializeField]
    private Button mediumBtn;

    [SerializeField]
    private Button hardBtn;

    [SerializeField]
    private Button goBackBtn;

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        startGameBtn.onClick.AddListener(OnStartGameClick);
        endGameBtn.onClick.AddListener(OnEndGameClick);
        goBackBtn.onClick.AddListener(OnGoBackClick);
        easyBtn.onClick.AddListener(OnEasyClick);
        mediumBtn.onClick.AddListener(OnMediumClick);
        hardBtn.onClick.AddListener(OnHardClick);
    }

    private void OnStartGameClick()
    {
        mainButtonsGroup.SetActive(false);
        levelButtonsGroup.SetActive(true);
    }

    private void OnEndGameClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    private void OnGoBackClick()
    {
        mainButtonsGroup.SetActive(true);
        levelButtonsGroup.SetActive(false);
    }

    private void OnEasyClick()
    {
        GameManager.Instance.StartNewGame(GameManager.Difficulty.Easy);
    }

    private void OnMediumClick()
    {
        GameManager.Instance.StartNewGame(GameManager.Difficulty.Medium);
    }

    private void OnHardClick()
    {
        GameManager.Instance.StartNewGame(GameManager.Difficulty.Hard);
    }
}
