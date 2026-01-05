using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 상단 문장 표시 UI - StepWordSelector 연동
/// </summary>
public class SentenceDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI sentenceText;
    [SerializeField] private GameObject panel;
    
    [Header("Style")]
    [SerializeField] private string emptySlotText = "???";
    
    private void Start()
    {
        // StepWordSelector 이벤트 구독
        if (StepWordSelector.Instance != null)
        {
            StepWordSelector.Instance.OnSelectionComplete += OnSelectionComplete;
        }
        
        // 언어 변경 이벤트 구독
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += OnLanguageChanged;
        }
        
        UpdateDisplay();
    }
    
    private void OnDestroy()
    {
        if (StepWordSelector.Instance != null)
        {
            StepWordSelector.Instance.OnSelectionComplete -= OnSelectionComplete;
        }
        
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
    }
    
    private void OnSelectionComplete()
    {
        UpdateDisplay();
    }
    
    private void OnLanguageChanged(bool isEnglish)
    {
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (StepWordSelector.Instance == null) return;
        
        bool isEnglish = LanguageManager.Instance != null && LanguageManager.Instance.IsEnglish;
        
        // 메인 문장 텍스트
        if (sentenceText != null)
        {
            sentenceText.text = StepWordSelector.Instance.GetCurrentSentence(isEnglish);
        }
    }
    
    /// <summary>
    /// 현재 문장 반환
    /// </summary>
    public string GetCurrentSentence()
    {
        if (StepWordSelector.Instance == null) return "";
        
        bool isEnglish = LanguageManager.Instance != null && LanguageManager.Instance.IsEnglish;
        return StepWordSelector.Instance.GetCurrentSentence(isEnglish);
    }
}
