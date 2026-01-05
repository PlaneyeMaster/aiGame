using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 바구니 UI - StepWordSelector 연동
/// Generate 버튼과 안내 텍스트만 관리
/// </summary>
public class BasketController : MonoBehaviour
{
    public static BasketController Instance { get; private set; }
    
    [Header("Guide Text")]
    [SerializeField] private GameObject guideTextObject;  // "단어를 선택하세요!" 텍스트 오브젝트
    
    [Header("Generate Button")]
    [SerializeField] private GameObject generateButtonObject;  // 버튼 전체 오브젝트 (표시/숨김용)
    [SerializeField] private Button generateButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 게임 상태 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
        }
        
        // StepWordSelector 이벤트 구독
        if (StepWordSelector.Instance != null)
        {
            StepWordSelector.Instance.OnSelectionComplete += OnSelectionComplete;
        }

        if (generateButton != null)
            generateButton.onClick.AddListener(OnGenerateClicked);

        // 초기 상태 설정
        ResetUI();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }
        
        if (StepWordSelector.Instance != null)
        {
            StepWordSelector.Instance.OnSelectionComplete -= OnSelectionComplete;
        }
    }
    
    /// <summary>
    /// UI 초기화 (게임 시작 또는 리셋 시)
    /// </summary>
    public void ResetUI()
    {
        // 안내 텍스트 표시
        if (guideTextObject != null) guideTextObject.SetActive(true);
        
        // Generate 버튼 숨김 및 활성화 상태 복원
        if (generateButtonObject != null) generateButtonObject.SetActive(false);
        if (generateButton != null) generateButton.interactable = true;
    }
    
    /// <summary>
    /// 선택 완료 시
    /// </summary>
    private void OnSelectionComplete()
    {
        // Generate 버튼 표시
        if (generateButtonObject != null) generateButtonObject.SetActive(true);
    }

    private void OnGameStateChanged(GameState state)
    {
        if (state == GameState.Generating)
        {
            // 생성 중에는 버튼 비활성화
            if (generateButton != null) generateButton.interactable = false;
        }
        else if (state == GameState.Selecting)
        {
            // 리셋
            ResetUI();
        }
    }

    private void OnGenerateClicked()
    {
        Debug.Log("[BasketController] Generate Button Clicked!");
        GameManager.Instance?.StartGeneration();
    }
}
