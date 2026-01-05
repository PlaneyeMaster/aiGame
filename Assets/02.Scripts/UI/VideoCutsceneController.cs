using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// 생성 대기 중 컷신 영상 재생 컨트롤러
/// </summary>
public class VideoCutsceneController : MonoBehaviour
{
    public static VideoCutsceneController Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float fadeInDuration = 0.5f; // 페이드인 시간

    private const string MOVIE_PATH = "Movie/AIGen"; // Resources 폴더 기준 경로

    private GameObject cutsceneCanvas;
    private CanvasGroup canvasGroup; // 페이드 효과용
    private RawImage displayImage;
    private VideoPlayer videoPlayer;
    private Action onVideoFinished;

    private bool isPlaying = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeUI()
    {
        // 1. 영상 재생용 Canvas 생성 (최상단 노출)
        cutsceneCanvas = new GameObject("VideoCutsceneCanvas");
        cutsceneCanvas.transform.SetParent(this.transform);

        Canvas canvas = cutsceneCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // 최상단 표시

        cutsceneCanvas.AddComponent<CanvasScaler>();
        cutsceneCanvas.AddComponent<GraphicRaycaster>();

        // 페이드 효과를 위한 CanvasGroup 추가
        canvasGroup = cutsceneCanvas.AddComponent<CanvasGroup>();

        // 2. 배경 (Black 대신 White, 영상 그대로 출력) 및 영상 출력용 RawImage
        GameObject rawImageObj = new GameObject("VideoDisplay");
        rawImageObj.transform.SetParent(cutsceneCanvas.transform, false);

        displayImage = rawImageObj.AddComponent<RawImage>();
        displayImage.color = Color.white; // 텍스처 색상 그대로 표현

        // 전체 화면 채우기
        RectTransform rt = displayImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // 3. VideoPlayer 설정
        videoPlayer = rawImageObj.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.APIOnly; // RawImage에 텍스처를 직접 할당하기 위함
        videoPlayer.isLooping = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

        // 오디오 소스 추가 (소리 재생용)
        AudioSource audioSource = rawImageObj.AddComponent<AudioSource>();
        videoPlayer.SetTargetAudioSource(0, audioSource);

        // 이벤트 연결
        videoPlayer.loopPointReached += OnLoopPointReached;
        videoPlayer.prepareCompleted += OnPrepareCompleted;

        // 초기엔 숨김
        cutsceneCanvas.SetActive(false);
    }

    /// <summary>
    /// 컷신 재생 시작
    /// </summary>
    public void PlayCutscene(Action onCompleteCallback)
    {
        onVideoFinished = onCompleteCallback;

        VideoClip clip = Resources.Load<VideoClip>(MOVIE_PATH);
        if (clip == null)
        {
            Debug.LogError($"[VideoCutscene] Cannot find movie at Resources/{MOVIE_PATH}");
            onCompleteCallback?.Invoke();
            return;
        }

        cutsceneCanvas.SetActive(true);
        canvasGroup.alpha = 0f; // 투명하게 시작 (페이드인 준비)

        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        isPlaying = true;
    }

    private void OnPrepareCompleted(VideoPlayer source)
    {
        // 준비 완료 후 재생
        displayImage.texture = source.texture;
        source.Play();

        // 페이드인 시작
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = t / fadeInDuration;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private void OnLoopPointReached(VideoPlayer source)
    {
        // 영상 종료 시
        Debug.Log("[VideoCutscene] Video Finished");
        isPlaying = false;
        onVideoFinished?.Invoke();
    }

    /// <summary>
    /// 강제 종료 또는 완료 후 숨김
    /// </summary>
    public void StopCutscene()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        cutsceneCanvas.SetActive(false);
        isPlaying = false;
    }

    public bool IsPlaying => isPlaying;
}
