using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 타이틀 화면 컨트롤러
/// </summary>
public class TitleScreenController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject titlePanel; // 타이틀 화면 전체 패널
    [SerializeField] private Button startButton;    // 시작하기 버튼

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.5f;

    private void Start()
    {
        // GameManager 상태 변경 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartClicked);
        }

        // 초기 상태 확인 (이미 Title 상태라면 바로 보여주기)
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Title)
        {
            ShowTitle();
        }
        else
        {
            HideTitle();
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }
    }

    private void OnGameStateChanged(GameState state)
    {
        if (state == GameState.Title)
        {
            ShowTitle();
        }
        else
        {
            HideTitle();
        }
    }

    private void OnStartClicked()
    {
        Debug.Log("[TitleScreen] Start Button Clicked");

        // 버튼음 재생
        if (WordSoundManager.Instance != null)
        {
            WordSoundManager.Instance.PlayDefaultClick();
        }

        GameManager.Instance?.StartGame();
    }

    private void ShowTitle()
    {
        if (titlePanel != null)
        {
            titlePanel.SetActive(true);
            // 여기에 페이드인 애니메이션 추가 가능
        }
    }

    private void HideTitle()
    {
        if (titlePanel != null)
        {
            titlePanel.SetActive(false);
            // 여기에 페이드아웃 애니메이션 추가 가능
        }
    }
}
