using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 언어 전환 버튼 UI
/// </summary>
public class LanguageToggleUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Image flagIcon;
    
    [Header("Icons (Optional)")]
    [SerializeField] private Sprite koreanFlagIcon;
    [SerializeField] private Sprite englishFlagIcon;
    
    private void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(OnToggleClicked);
        }
        
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += OnLanguageChanged;
        }
        
        UpdateUI();
    }
    
    private void OnDestroy()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
    }
    
    private void OnToggleClicked()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.ToggleLanguage();
        }
    }
    
    private void OnLanguageChanged(bool isEnglish)
    {
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        bool isEnglish = LanguageManager.Instance != null && LanguageManager.Instance.IsEnglish;
        
        if (buttonText != null)
        {
            // 현재 언어 표시 (클릭하면 전환됨을 암시)
            buttonText.text = isEnglish ? "한글" : "English";
        }
        
        if (flagIcon != null)
        {
            flagIcon.sprite = isEnglish ? koreanFlagIcon : englishFlagIcon;
        }
    }
}
