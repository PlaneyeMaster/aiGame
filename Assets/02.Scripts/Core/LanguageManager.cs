using UnityEngine;

/// <summary>
/// 언어 관리자 - 한글/영어 전환
/// </summary>
public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }
    
    [SerializeField] private bool isEnglish = false;
    
    public bool IsEnglish => isEnglish;
    
    public event System.Action<bool> OnLanguageChanged;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    /// <summary>
    /// 언어 전환
    /// </summary>
    public void ToggleLanguage()
    {
        isEnglish = !isEnglish;
        OnLanguageChanged?.Invoke(isEnglish);
    }
    
    /// <summary>
    /// 특정 언어로 설정
    /// </summary>
    public void SetLanguage(bool english)
    {
        if (isEnglish != english)
        {
            isEnglish = english;
            OnLanguageChanged?.Invoke(isEnglish);
        }
    }
}
