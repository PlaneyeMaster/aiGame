using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 게임 상태
/// </summary>
public enum GameState
{
    Selecting,   // 단어 선택 중
    Generating,  // 생성 중
    Viewing      // 결과 보기
}

/// <summary>
/// 게임 매니저 - StepWordSelector 연동
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private ImageGenerator imageGenerator;
    [SerializeField] private ResultGallery resultGallery;
    [SerializeField] private ParticleController particleController;
    
    public GameState CurrentState { get; private set; } = GameState.Selecting;
    
    public event Action<GameState> OnStateChanged;
    
    // StepWordSelector 사용 시 체크
    public bool CanGenerate => StepWordSelector.Instance != null && StepWordSelector.Instance.IsSelectionComplete;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private void Start()
    {
        // StepWordSelector 이벤트 구독
        if (StepWordSelector.Instance != null)
        {
            StepWordSelector.Instance.OnSelectionComplete += OnSelectionComplete;
        }
        
        ChangeState(GameState.Selecting);
    }
    
    private void OnDestroy()
    {
        if (StepWordSelector.Instance != null)
        {
            StepWordSelector.Instance.OnSelectionComplete -= OnSelectionComplete;
        }
    }
    
    private void OnSelectionComplete()
    {
        Debug.Log("[GameManager] Selection complete! Ready to generate.");
        // 사운드 효과
        if (WordSoundManager.Instance != null)
        {
            WordSoundManager.Instance.PlayCompleteSound();
        }
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
    
    /// <summary>
    /// 이미지 생성 시작
    /// </summary>
    public void StartGeneration()
    {
        if (!CanGenerate)
        {
            Debug.LogWarning("[GameManager] Cannot generate - selection incomplete");
            return;
        }
        
        ChangeState(GameState.Generating);
        
        // StepWordSelector에서 프롬프트 가져오기
        if (imageGenerator != null && StepWordSelector.Instance != null)
        {
            string prompt = StepWordSelector.Instance.GetPromptSentence();
            imageGenerator.GenerateImages(prompt, OnImagesGenerated);
        }
    }
    
    private void OnImagesGenerated(Texture2D[] images)
    {
        ChangeState(GameState.Viewing);
        resultGallery?.ShowResults(images);
    }
    
    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RetryGame()
    {
        // StepWordSelector 리셋
        StepWordSelector.Instance?.Reset();
        
        // 결과 갤러리 숨기기
        resultGallery?.HideGallery();
        
        ChangeState(GameState.Selecting);
    }
}
