using UnityEngine;
using System.Linq;

/// <summary>
/// 단어 데이터베이스 - ScriptableObject 연동 버전
/// </summary>
public class WordDatabase : MonoBehaviour
{
    public static WordDatabase Instance { get; private set; }
    
    [Header("Word Database Asset")]
    [Tooltip("Project에서 생성한 WordDatabaseSO 에셋을 여기에 드래그")]
    [SerializeField] private WordDatabaseSO databaseAsset;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 모든 단어 반환
    /// </summary>
    public WordDataSO[] GetAllWords()
    {
        if (databaseAsset == null)
        {
            Debug.LogError("[WordDatabase] Database asset not assigned!");
            return new WordDataSO[0];
        }
        return databaseAsset.GetAllWords();
    }
    
    /// <summary>
    /// 슬롯 타입별 단어 반환
    /// </summary>
    public WordDataSO[] GetWordsBySlotType(WordSlotType slotType)
    {
        if (databaseAsset == null) return new WordDataSO[0];
        return databaseAsset.GetWordsBySlotType(slotType);
    }
    
    /// <summary>
    /// 섞인 단어 목록 반환
    /// </summary>
    public WordDataSO[] GetShuffledWords()
    {
        if (databaseAsset == null) return new WordDataSO[0];
        return databaseAsset.GetShuffledWords();
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
