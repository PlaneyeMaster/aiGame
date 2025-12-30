using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 단어 버블 스포너
/// </summary>
public class WordSpawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject wordBubblePrefab;
    [SerializeField] private int maxBubbles = 15;
    [SerializeField] private float spawnPadding = 1f;
    
    [Header("Spawn Area")]
    [SerializeField] private Vector2 areaMin = new Vector2(-7f, -3f);
    [SerializeField] private Vector2 areaMax = new Vector2(7f, 3f);
    
    private List<WordBubble> bubbles = new List<WordBubble>();
    
    private void Start()
    {
        SpawnWords();
        if (GameManager.Instance != null)
            GameManager.Instance.OnSelectionCleared += OnSelectionCleared;
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnSelectionCleared -= OnSelectionCleared;
    }
    
    public void SpawnWords()
    {
        if (WordDatabase.Instance == null) return;
        
        ClearBubbles();
        
        WordData[] words = WordDatabase.Instance.GetShuffledWords();
        int count = Mathf.Min(words.Length, maxBubbles);
        
        for (int i = 0; i < count; i++)
            SpawnBubble(words[i]);
    }
    
    private void SpawnBubble(WordData data)
    {
        if (wordBubblePrefab == null) return;
        
        Vector3 pos = GetRandomPosition();
        GameObject obj = Instantiate(wordBubblePrefab, pos, Quaternion.identity, transform);
        WordBubble bubble = obj.GetComponent<WordBubble>();
        
        if (bubble != null)
        {
            bubble.Initialize(data);
            bubbles.Add(bubble);
        }
    }
    
    private Vector3 GetRandomPosition()
    {
        for (int i = 0; i < 50; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(areaMin.x + spawnPadding, areaMax.x - spawnPadding),
                Random.Range(areaMin.y + spawnPadding, areaMax.y - spawnPadding),
                0f);
            
            bool ok = true;
            foreach (var b in bubbles)
            {
                if (b != null && Vector3.Distance(pos, b.transform.position) < spawnPadding * 2)
                {
                    ok = false;
                    break;
                }
            }
            if (ok) return pos;
        }
        
        return new Vector3(Random.Range(areaMin.x, areaMax.x), Random.Range(areaMin.y, areaMax.y), 0f);
    }
    
    public void ClearBubbles()
    {
        foreach (var b in bubbles)
            if (b != null) Destroy(b.gameObject);
        bubbles.Clear();
    }
    
    private void OnSelectionCleared()
    {
        foreach (var b in bubbles)
            if (b != null) b.ResetSelection();
    }
}
