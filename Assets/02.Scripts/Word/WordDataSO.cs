using UnityEngine;

/// <summary>
/// 단어 데이터 ScriptableObject - 인스펙터에서 아이콘 직접 할당 가능
/// </summary>
[CreateAssetMenu(fileName = "WordData", menuName = "AIGame/Word Data", order = 1)]
public class WordDataSO : ScriptableObject
{
    [Header("Text")]
    [Tooltip("한글 단어")]
    public string wordKorean;
    
    [Tooltip("영문 단어 (AI 프롬프트용)")]
    public string wordEnglish;
    
    [Header("Category")]
    [Tooltip("슬롯 타입: Subject(누가), Object(무엇을), Verb(해요)")]
    public WordSlotType slotType;
    
    [Header("Visual")]
    [Tooltip("단어 아이콘 이미지")]
    public Sprite icon;
    
    [Header("Audio")]
    [Tooltip("클릭 시 재생할 사운드")]
    public AudioClip sound;
    
    /// <summary>
    /// 현재 언어에 따른 단어 반환
    /// </summary>
    public string GetWord(bool isEnglish = false)
    {
        return isEnglish ? wordEnglish : wordKorean;
    }
    
    /// <summary>
    /// 슬롯 타입별 색상 반환
    /// </summary>
    public Color GetSlotColor()
    {
        return slotType switch
        {
            WordSlotType.Subject => new Color(0.4f, 0.7f, 1f),      // 하늘색 (누가?)
            WordSlotType.Object => new Color(1f, 0.85f, 0.4f),      // 노란색 (무엇을?)
            WordSlotType.Verb => new Color(0.6f, 1f, 0.6f),         // 연두색 (해요?)
            _ => Color.white
        };
    }
    
    /// <summary>
    /// 슬롯 라벨 반환
    /// </summary>
    public string GetSlotLabel(bool isEnglish = false)
    {
        if (isEnglish)
        {
            return slotType switch
            {
                WordSlotType.Subject => "Who?",
                WordSlotType.Object => "What?",
                WordSlotType.Verb => "Does?",
                _ => ""
            };
        }
        
        return slotType switch
        {
            WordSlotType.Subject => "누가?",
            WordSlotType.Object => "무엇을?",
            WordSlotType.Verb => "해요?",
            _ => ""
        };
    }
}
