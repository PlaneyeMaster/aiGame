using UnityEngine;

/// <summary>
/// 단어 카테고리
/// </summary>
public enum WordCategory
{
    Noun,        // 명사
    Adjective,   // 형용사
    PlaceAction  // 장소/동작
}

/// <summary>
/// 단어 데이터
/// </summary>
[System.Serializable]
public class WordData
{
    public string word;           // 한글
    public string englishWord;    // 영문
    public string category;       // 카테고리
    
    public WordCategory GetCategory()
    {
        return category switch
        {
            "Noun" => WordCategory.Noun,
            "Adjective" => WordCategory.Adjective,
            "PlaceAction" => WordCategory.PlaceAction,
            _ => WordCategory.Noun
        };
    }
    
    public Color GetCategoryColor()
    {
        return GetCategory() switch
        {
            WordCategory.Noun => new Color(0.4f, 0.7f, 1f),        // 하늘색
            WordCategory.Adjective => new Color(1f, 0.6f, 0.8f),   // 분홍색
            WordCategory.PlaceAction => new Color(0.6f, 1f, 0.6f), // 연두색
            _ => Color.white
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
