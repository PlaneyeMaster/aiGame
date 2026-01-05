using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 게임 상태
/// </summary>
public enum GameState
{
    Title,       // 타이틀 화면
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

    private VideoCutsceneController videoController; // 코드에서 찾거나 생성

    // 생성 상태 체크용 플래그
    private bool isVideoFinished = false;
    private bool isImageGenerated = false;
    private Texture2D[] generatedImagesCache;


    public GameState CurrentState { get; private set; } = GameState.Title;

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
        // VideoCutsceneController 찾기 또는 생성
        videoController = FindObjectOfType<VideoCutsceneController>();
        if (videoController == null)
        {
            GameObject go = new GameObject("VideoCutsceneController");
            videoController = go.AddComponent<VideoCutsceneController>();
        }

        // StepWordSelector 이벤트 구독
        if (StepWordSelector.Instance != null)
        {
            StepWordSelector.Instance.OnSelectionComplete += OnSelectionComplete;
        }

        ChangeState(GameState.Title);
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
    /// 이미지 생성 시작 (비디오 재생 + API 호출)
    /// </summary>
    public void StartGeneration()
    {
        if (!CanGenerate)
        {
            Debug.LogWarning("[GameManager] Cannot generate - selection incomplete");
            return;
        }

        ChangeState(GameState.Generating);

        // 플래그 초기화
        isVideoFinished = false;
        isImageGenerated = false;
        generatedImagesCache = null;

        // 1. 비디오 재생 시작
        if (videoController != null)
        {
            videoController.PlayCutscene(() =>
            {
                isVideoFinished = true;
                CheckGenerationComplete();
            });
        }
        else
        {
            // 비디오 컨트롤러가 없으면 바로 완료 처리
            isVideoFinished = true;
        }

        // 2. StepWordSelector에서 프롬프트 가져와서 생성 요청
        if (imageGenerator != null && StepWordSelector.Instance != null)
        {
            string prompt = StepWordSelector.Instance.GetPromptSentence();
            // 콜백을 OnImagesGenerated로 바로 연결하지 않고, 내부 캐싱 메서드로 연결
            imageGenerator.GenerateImages(prompt, OnImagesGeneratedInternal);
        }
    }

    // 이미지 생성 완료 콜백 (내부용)
    private void OnImagesGeneratedInternal(Texture2D[] images)
    {
        Debug.Log("[GameManager] Images received from API.");
        generatedImagesCache = images;
        isImageGenerated = true;
        CheckGenerationComplete();
    }

    // 두 조건(비디오, 이미지)이 모두 완료되었는지 확인
    private void CheckGenerationComplete()
    {
        if (isVideoFinished && isImageGenerated)
        {
            Debug.Log("[GameManager] All tasks finished. Showing results.");

            // 비디오 UI 끄기
            videoController?.StopCutscene();

            // 결과 표시
            if (generatedImagesCache != null)
            {
                OnImagesGenerated(generatedImagesCache);
            }
        }
    }

    // 기존 메서드는 결과 표시용으로만 사용
    private void OnImagesGenerated(Texture2D[] images)
    {
        ChangeState(GameState.Viewing);
        resultGallery?.ShowResults(images);
    }

    /// <summary>
    /// 게임 시작 (타이틀 -> 선택)
    /// </summary>
    public void StartGame()
    {
        ChangeState(GameState.Selecting);
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
