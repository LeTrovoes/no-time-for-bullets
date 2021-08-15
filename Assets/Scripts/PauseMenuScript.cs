using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class PauseMenuScript : MonoBehaviour
{

    [SerializeField]
    private Button goBackBtn;

    [SerializeField]
    private Button mainMenuBtn;

    [SerializeField]
    private Button exitBtn;

    void Start()
    {
        goBackBtn.onClick.AddListener(OnGoBackClick);
        mainMenuBtn.onClick.AddListener(OnGoMainMenuClick);
        exitBtn.onClick.AddListener(OnExitClick);
    }

    private void OnGoBackClick()
    {
        GameManager.Instance.TogglePause();
    }

    private void OnGoMainMenuClick()
    {
        DOTween.CompleteAll();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
    }

    private void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
