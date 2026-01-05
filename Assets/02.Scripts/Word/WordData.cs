using UnityEngine;

/// <summary>
/// 단어 슬롯 타입 (문장 구조)
/// </summary>
public enum WordSlotType
{
    Subject,    // 주어 (고양이가, 로봇이...)
    Verb,       // 서술어 (먹는다, 날아간다...)
    Object      // 목적어/장소 (피자를, 숲속에서...)
}

/// <summary>
/// 단어 데이터 (한글/영어 지원)
/// </summary>
[System.Serializable]
public class WordData
{
    [Header("Text")]
    public string wordKorean;     // 한글 단어
    public string wordEnglish;    // 영문 단어
    
    [Header("Category")]
    public string slotType;       // Subject, Verb, Object
    
    [Header("Resources")]
    public string iconName;       // 아이콘 리소스 이름
    public string soundName;      // 사운드 클립 이름
    
    /// <summary>
    /// 슬롯 타입 반환
    /// </summary>
    public WordSlotType GetSlotType()
    {
        return slotType switch
        {
            "Subject" => WordSlotType.Subject,
            "Verb" => WordSlotType.Verb,
            "Object" => WordSlotType.Object,
            _ => WordSlotType.Subject
        };
    }
    
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
        return GetSlotType() switch
        {
            WordSlotType.Subject => new Color(0.4f, 0.7f, 1f),      // 하늘색 (주어)
            WordSlotType.Verb => new Color(0.6f, 1f, 0.6f),         // 연두색 (서술어)
            WordSlotType.Object => new Color(1f, 0.85f, 0.4f),      // 노란색 (목적어)
            _ => Color.white
        };
    }
    
    /// <summary>
    /// 슬롯 라벨 반환 (한글)
    /// </summary>
    public string GetSlotLabel(bool isEnglish = false)
    {
        if (isEnglish)
        {
            return GetSlotType() switch
            {
                WordSlotType.Subject => "Who?",
                WordSlotType.Object => "What?",
                WordSlotType.Verb => "Does?",
                _ => ""
            };
        }
        
        return GetSlotType() switch
        {
            WordSlotType.Subject => "누가?",
            WordSlotType.Object => "무엇을?",
            WordSlotType.Verb => "해요?",
            _ => ""
        };
    }
}

/// <summary>
/// 단어 리스트 (JSON 파싱용)
/// </summary>
[System.Serializable]
public class WordDataList
{
    public WordData[] words;
}
