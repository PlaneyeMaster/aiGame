using UnityEngine;
using System.Linq;

/// <summary>
/// 단어 데이터베이스 - ScriptableObject 기반
/// 인스펙터에서 모든 단어와 아이콘을 직접 관리
/// </summary>
[CreateAssetMenu(fileName = "WordDatabase", menuName = "AIGame/Word Database", order = 0)]
public class WordDatabaseSO : ScriptableObject
{
    [Header("Word Data (총 18개: 누가6 + 무엇을6 + 해요6)")]
    [Tooltip("모든 단어 데이터")]
    public WordDataSO[] allWords;
    
    /// <summary>
    /// 모든 단어 반환
    /// </summary>
    public WordDataSO[] GetAllWords() => allWords;
    
    /// <summary>
    /// 슬롯 타입별 단어 반환
    /// </summary>
    public WordDataSO[] GetWordsBySlotType(WordSlotType slotType)
    {
        return allWords.Where(w => w != null && w.slotType == slotType).ToArray();
    }
    
    /// <summary>
    /// 섞인 단어 목록 반환
    /// </summary>
    public WordDataSO[] GetShuffledWords()
    {
        return allWords.Where(w => w != null).OrderBy(x => Random.value).ToArray();
    }
    
    /// <summary>
    /// 누가? 단어들
    /// </summary>
    public WordDataSO[] GetSubjects() => GetWordsBySlotType(WordSlotType.Subject);
    
    /// <summary>
    /// 무엇을? 단어들
    /// </summary>
    public WordDataSO[] GetObjects() => GetWordsBySlotType(WordSlotType.Object);
    
    /// <summary>
    /// 해요? 단어들
    /// </summary>
    public WordDataSO[] GetVerbs() => GetWordsBySlotType(WordSlotType.Verb);
}
