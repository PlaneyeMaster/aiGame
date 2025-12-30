using UnityEngine;
using System.Linq;

/// <summary>
/// 단어 데이터베이스
/// </summary>
public class WordDatabase : MonoBehaviour
{
    public static WordDatabase Instance { get; private set; }
    
    private WordData[] allWords;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadWords();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void LoadWords()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("WordData");
        if (jsonFile != null)
        {
            WordDataList dataList = JsonUtility.FromJson<WordDataList>(jsonFile.text);
            allWords = dataList.words;
            Debug.Log($"[WordDatabase] {allWords.Length}개 단어 로드");
        }
        else
        {
            Debug.LogError("[WordDatabase] WordData.json 없음!");
            allWords = new WordData[0];
        }
    }
    
    public WordData[] GetAllWords() => allWords;
    
    public WordData[] GetWordsByCategory(WordCategory category)
    {
        return allWords.Where(w => w.GetCategory() == category).ToArray();
    }
    
    public WordData[] GetShuffledWords()
    {
        return allWords.OrderBy(x => Random.value).ToArray();
    }
}
