using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 생성 버튼 컨트롤러 - StepWordSelector 연동
/// </summary>
public class GenerateButtonController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button generateButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Text")]
    [SerializeField] private string readyText = "그림 그리기!";
    [SerializeField] private string notReadyText = "단어를 선택하세요";
    [SerializeField] private string generatingText = "그리는 중...";

    private void Start()
    {
        if (generateButton != null)
        {
            generateButton.onClick.AddListener(OnGenerateClicked);
        }

        // StepWordSelector 이벤트 구독
        if (StepWordSelector.Instance != null)
        {
            StepWordSelector.Instance.OnSelectionComplete += OnSelectionComplete;
        }

        // 게임 매니저 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
        }

        UpdateButton();
    }

    private void OnDestroy()
    {
        if (StepWordSelector.Instance != null)
        {
            StepWordSelector.Instance.OnSelectionComplete -= OnSelectionComplete;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }
    }

    private void OnSelectionComplete()
    {
        UpdateButton();
    }

    private void OnGameStateChanged(GameState state)
    {
        UpdateButton();
    }

    private void UpdateButton()
    {
        if (generateButton == null) return;

        GameState state = GameManager.Instance?.CurrentState ?? GameState.Selecting;
        bool canGenerate = GameManager.Instance?.CanGenerate ?? false;

        switch (state)
        {
            case GameState.Generating:
                generateButton.interactable = false;
                if (buttonText != null) buttonText.text = generatingText;
                break;

            case GameState.Viewing:
                generateButton.interactable = false;
                if (buttonText != null) buttonText.text = readyText;
                break;

            default:
                generateButton.interactable = canGenerate;
                if (buttonText != null)
                {
                    buttonText.text = canGenerate ? readyText : notReadyText;
                }
                break;
        }
    }

    private void OnGenerateClicked()
    {
        Debug.Log("[GenerateButton] Clicked!");

        // 버튼음 재생
        if (WordSoundManager.Instance != null)
        {
            WordSoundManager.Instance.PlayDefaultClick();
        }

        GameManager.Instance?.StartGeneration();
    }
}
