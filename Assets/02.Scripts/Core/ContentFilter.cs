using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// 콘텐츠 안전 필터 - 19금/잔인 콘텐츠 차단
/// </summary>
public class ContentFilter : MonoBehaviour
{
    public static ContentFilter Instance { get; private set; }
    
    [Header("Filter Settings")]
    [SerializeField] private bool enableFilter = true;
    
    // 금지 키워드 목록 (영어)
    private readonly HashSet<string> blockedKeywords = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        // 폭력
        "violence", "violent", "blood", "bloody", "gore", "gory",
        "kill", "killing", "murder", "death", "dead", "die",
        "weapon", "gun", "knife", "sword", "fight", "war",
        "attack", "hurt", "pain", "torture", "abuse",
        
        // 공포
        "horror", "scary", "terrifying", "nightmare", "monster",
        "zombie", "ghost", "demon", "devil", "evil", "dark",
        "creepy", "disturbing", "grotesque",
        
        // 성인 콘텐츠
        "nude", "naked", "sexy", "sexual", "adult", "erotic",
        "nsfw", "explicit", "provocative",
        
        // 기타 부적절
        "drug", "drugs", "alcohol", "cigarette", "smoking",
        "inappropriate", "offensive", "hate", "racist"
    };
    
    // 안전한 프롬프트 접미사
    private const string SAFETY_SUFFIX = ", child-friendly, cute art style, safe for children, no violence, no scary elements, bright and cheerful, wholesome";
    
    // Negative Prompt (AI에게 피해야 할 요소 지시)
    private const string NEGATIVE_PROMPT = "violence, blood, gore, scary, horror, dark, weapons, inappropriate, nsfw, adult content, disturbing, creepy, nightmare, monster, zombie, death";
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    /// <summary>
    /// 텍스트에 금지 키워드가 포함되어 있는지 검사
    /// </summary>
    public bool ContainsBlockedContent(string text)
    {
        if (!enableFilter || string.IsNullOrEmpty(text)) return false;
        
        string lowerText = text.ToLower();
        
        foreach (var keyword in blockedKeywords)
        {
            if (lowerText.Contains(keyword.ToLower()))
            {
                Debug.LogWarning($"[ContentFilter] Blocked keyword detected: {keyword}");
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 프롬프트 정제 (안전한 접미사 추가)
    /// </summary>
    public string SanitizePrompt(string prompt)
    {
        if (string.IsNullOrEmpty(prompt)) return prompt;
        
        // 금지 키워드 제거
        string sanitized = prompt;
        foreach (var keyword in blockedKeywords)
        {
            sanitized = Regex.Replace(sanitized, $@"\b{keyword}\b", "", RegexOptions.IgnoreCase);
        }
        
        // 안전 접미사 추가
        sanitized = sanitized.Trim() + SAFETY_SUFFIX;
        
        return sanitized;
    }
    
    /// <summary>
    /// Negative Prompt 반환
    /// </summary>
    public string GetNegativePrompt()
    {
        return NEGATIVE_PROMPT;
    }
    
    /// <summary>
    /// 단어 데이터 유효성 검사
    /// </summary>
    public bool IsWordSafe(WordData word)
    {
        if (word == null) return false;
        
        return !ContainsBlockedContent(word.wordKorean) && 
               !ContainsBlockedContent(word.wordEnglish);
    }
    
    /// <summary>
    /// 런타임에 금지 키워드 추가
    /// </summary>
    public void AddBlockedKeyword(string keyword)
    {
        if (!string.IsNullOrEmpty(keyword))
        {
            blockedKeywords.Add(keyword.ToLower());
        }
    }
}
