using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 게임 상태
/// </summary>
public enum GameState
{
    Exploring,   // 탐색
    Selecting,   // 선택 중
    Generating,  // 생성 중
    Viewing      // 결과 보기
}

/// <summary>
/// 게임 매니저
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Settings")]
    [SerializeField] private int minWordsToGenerate = 2;
    [SerializeField] private int maxWordsToSelect = 3;
    
    [Header("References")]
    [SerializeField] private BasketController basketController;
    [SerializeField] private ImageGenerator imageGenerator;
    [SerializeField] private ResultGallery resultGallery;
    [SerializeField] private ParticleController particleController;
    
    public GameState CurrentState { get; private set; } = GameState.Exploring;
    
    private List<WordData> selectedWords = new List<WordData>();
    
    public event Action<GameState> OnStateChanged;
    public event Action<WordData> OnWordSelected;
    public event Action<WordData> OnWordDeselected;
    public event Action OnSelectionCleared;
    
    public int MinWordsToGenerate => minWordsToGenerate;
    public int MaxWordsToSelect => maxWordsToSelect;
    public int SelectedWordCount => selectedWords.Count;
    public bool CanGenerate => selectedWords.Count >= minWordsToGenerate;
    public bool CanSelectMore => selectedWords.Count < maxWordsToSelect;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private void Start()
    {
        ChangeState(GameState.Exploring);
    }
    
    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
        
        if (newState == GameState.Generating)
            particleController?.StartMagicEffect();
        else if (newState == GameState.Viewing)
            particleController?.StopMagicEffect();
    }
    
    public bool SelectWord(WordData word)
    {
        if (!CanSelectMore || selectedWords.Contains(word)) return false;
        
        selectedWords.Add(word);
        OnWordSelected?.Invoke(word);
        
        if (selectedWords.Count == 1)
            ChangeState(GameState.Selecting);
        
        return true;
    }
    
    public bool DeselectWord(WordData word)
    {
        if (!selectedWords.Contains(word)) return false;
        
        selectedWords.Remove(word);
        OnWordDeselected?.Invoke(word);
        
        if (selectedWords.Count == 0)
            ChangeState(GameState.Exploring);
        
        return true;
    }
    
    public void ClearSelection()
    {
        selectedWords.Clear();
        OnSelectionCleared?.Invoke();
        ChangeState(GameState.Exploring);
    }
    
    public void StartGeneration()
    {
        if (!CanGenerate) return;
        
        ChangeState(GameState.Generating);
        string prompt = BuildPrompt();
        imageGenerator?.GenerateImages(prompt, OnImagesGenerated);
    }
    
    private string BuildPrompt()
    {
        List<string> words = new List<string>();
        foreach (var word in selectedWords)
            words.Add(word.englishWord);
        
        return $"A cute, colorful, child-friendly illustration of {string.Join(" ", words)}, cartoon style, bright colors, safe for children";
    }
    
    private void OnImagesGenerated(Texture2D[] images)
    {
        ChangeState(GameState.Viewing);
        resultGallery?.ShowResults(images);
    }
    
    public void RetryGame() => ClearSelection();
    
    public List<WordData> GetSelectedWords() => new List<WordData>(selectedWords);
}
