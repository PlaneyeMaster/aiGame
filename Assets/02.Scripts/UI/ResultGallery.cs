using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 결과 갤러리 UI (1장 전용, 심플 버전)
/// </summary>
public class ResultGallery : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject galleryPanel;
    [SerializeField] private RawImage resultImage; // 결과 이미지를 띄울 UI
    [SerializeField] private Button retryButton;   // 다시하기 버튼
    [SerializeField] private CanvasGroup canvasGroup; // Alpha 제어용

    private void Start()
    {
        if (canvasGroup == null && galleryPanel != null)
            canvasGroup = galleryPanel.GetComponent<CanvasGroup>();

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnGameStateChanged;

        HideGallery();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState state)
    {
        if (state != GameState.Viewing)
            HideGallery();
    }

    public void ShowResults(Texture2D[] imgs)
    {
        if (galleryPanel != null) galleryPanel.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (resultImage != null && imgs != null && imgs.Length > 0 && imgs[0] != null)
        {
            resultImage.texture = imgs[0];
            resultImage.gameObject.SetActive(true);

            // 이미지 비율 맞추기 (선택사항, AspectRatioFitter가 있으면 좋음)
            // AspectRatioFitter fitter = resultImage.GetComponent<AspectRatioFitter>();
            // if (fitter != null) fitter.aspectRatio = (float)imgs[0].width / imgs[0].height;
        }
    }

    public void HideGallery()
    {
        // Alpha 0으로 숨김
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // 완전히 끄기 (선택사항이나 성능상 권장)
        if (galleryPanel != null) galleryPanel.SetActive(false);
    }

    private void OnRetryClicked()
    {
        HideGallery();
        GameManager.Instance?.RetryGame();
    }
}
