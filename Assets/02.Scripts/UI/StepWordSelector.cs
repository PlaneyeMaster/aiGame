using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 단계별 단어 선택 시스템 (누가? → 무엇을? → 해요?)
/// </summary>
public class StepWordSelector : MonoBehaviour
{
    public static StepWordSelector Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private Transform buttonContainer;        // 선택용 버튼들이 들어갈 부모
    [SerializeField] private Transform selectedWordsContainer; // 선택 완료 후 단어들 표시할 컨테이너
    [SerializeField] private TextMeshProUGUI stepLabel;        // "누가?" / "무엇을?" / "해요?"
    [SerializeField] private GameObject wordButtonPrefab;      // 단어 버튼 프리팹
    
    [Header("Settings")]
    [SerializeField] private int buttonsPerStep = 4;     // 각 단계에서 보여줄 버튼 수
    [SerializeField] private float fadeInDuration = 0.3f;
    
    [Header("Background Sprites (버튼 배경 이미지)")]
    [SerializeField] private Sprite subjectButtonSprite;   // 누가? 버튼 배경
    [SerializeField] private Sprite objectButtonSprite;    // 무엇을? 버튼 배경
    [SerializeField] private Sprite verbButtonSprite;      // 해요? 버튼 배경
    
    [Header("Selection Complete UI")]
    [SerializeField] private GameObject hideOnComplete;     // 선택 완료 시 숨길 오브젝트
    [SerializeField] private TextMeshProUGUI guideText;     // 안내 텍스트
    [SerializeField] private string defaultGuideText = "단어를 선택하세요!"; // 기본 안내 문구
    [SerializeField] private string completeGuideText = "이번에 그릴 그림은!"; // 완료 시 안내 문구
    
    // 현재 상태
    private int currentStep = 0;  // 0=누가, 1=무엇을, 2=해요
    private WordDataSO selectedSubject;
    private WordDataSO selectedObject;
    private WordDataSO selectedVerb;
    
    private List<GameObject> currentButtons = new List<GameObject>();
    
    // 이벤트
    public event System.Action OnSelectionComplete;
    
    public bool IsSelectionComplete => selectedSubject != null && selectedObject != null && selectedVerb != null;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private void Start()
    {
        StartSelection();
    }
    
    /// <summary>
    /// 선택 시작 (처음부터) - UI 초기화 포함
    /// </summary>
    public void StartSelection()
    {
        currentStep = 0;
        selectedSubject = null;
        selectedObject = null;
        selectedVerb = null;
        
        // 선택된 단어 컨테이너 비우기
        ClearSelectedWordsContainer();
        
        // 숨겨진 오브젝트 복원
        if (hideOnComplete != null)
        {
            hideOnComplete.SetActive(true);
        }
        
        // GuideText 초기화
        if (guideText != null)
        {
            guideText.text = defaultGuideText;
        }
        
        ShowCurrentStep();
    }
    
    /// <summary>
    /// 현재 단계의 버튼들 표시
    /// </summary>
    private void ShowCurrentStep()
    {
        ClearButtons();
        
        WordSlotType slotType = GetCurrentSlotType();
        WordDataSO[] allWords = GetWordsForCurrentStep();
        
        if (allWords.Length == 0)
        {
            Debug.LogError($"[StepWordSelector] No words found for {slotType}");
            return;
        }
        
        // 랜덤으로 N개 선택
        WordDataSO[] selectedWords = allWords
            .OrderBy(x => Random.value)
            .Take(buttonsPerStep)
            .ToArray();
        
        // 라벨 업데이트
        UpdateStepLabel(slotType);
        
        // 버튼 생성
        StartCoroutine(CreateButtonsWithFade(selectedWords, slotType));
    }
    
    private WordSlotType GetCurrentSlotType()
    {
        return currentStep switch
        {
            0 => WordSlotType.Subject,
            1 => WordSlotType.Object,
            2 => WordSlotType.Verb,
            _ => WordSlotType.Subject
        };
    }
    
    private WordDataSO[] GetWordsForCurrentStep()
    {
        if (WordDatabase.Instance == null) return new WordDataSO[0];
        
        return currentStep switch
        {
            0 => WordDatabase.Instance.GetSubjects(),
            1 => WordDatabase.Instance.GetObjects(),
            2 => WordDatabase.Instance.GetVerbs(),
            _ => new WordDataSO[0]
        };
    }
    
    private void UpdateStepLabel(WordSlotType slotType)
    {
        if (stepLabel == null) return;
        
        bool isEnglish = LanguageManager.Instance != null && LanguageManager.Instance.IsEnglish;
        
        stepLabel.text = slotType switch
        {
            WordSlotType.Subject => isEnglish ? "Who?" : "누가?",
            WordSlotType.Object => isEnglish ? "What?" : "무엇을?",
            WordSlotType.Verb => isEnglish ? "Does?" : "해요?",
            _ => ""
        };
    }
    
    private Sprite GetSpriteForStep(WordSlotType slotType)
    {
        return slotType switch
        {
            WordSlotType.Subject => subjectButtonSprite,
            WordSlotType.Object => objectButtonSprite,
            WordSlotType.Verb => verbButtonSprite,
            _ => null
        };
    }
    
    private IEnumerator CreateButtonsWithFade(WordDataSO[] words, WordSlotType slotType)
    {
        Sprite bgSprite = GetSpriteForStep(slotType);
        
        foreach (var word in words)
        {
            GameObject btnObj = Instantiate(wordButtonPrefab, buttonContainer);
            currentButtons.Add(btnObj);
            
            // 버튼 설정
            SetupButton(btnObj, word, bgSprite);
            
            // 페이드인
            StartCoroutine(FadeInButton(btnObj));
            
            yield return new WaitForSeconds(0.05f); // 순차적 등장
        }
    }
    
    private void SetupButton(GameObject btnObj, WordDataSO word, Sprite bgSprite)
    {
        // 버튼 컴포넌트
        Button btn = btnObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => OnWordSelected(word));
            
            // 배경 스프라이트 설정
            Image bgImage = btn.GetComponent<Image>();
            if (bgImage != null && bgSprite != null)
            {
                bgImage.sprite = bgSprite;
            }
        }
        
        // 아이콘 설정
        Image iconImage = btnObj.transform.Find("Icon")?.GetComponent<Image>();
        if (iconImage != null && word.icon != null)
        {
            iconImage.sprite = word.icon;
        }
        
        // 텍스트 설정
        TextMeshProUGUI textComp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp != null)
        {
            bool isEnglish = LanguageManager.Instance != null && LanguageManager.Instance.IsEnglish;
            textComp.text = word.GetWord(isEnglish);
        }
    }
    
    private IEnumerator FadeInButton(GameObject btnObj)
    {
        CanvasGroup cg = btnObj.GetComponent<CanvasGroup>();
        if (cg == null) cg = btnObj.AddComponent<CanvasGroup>();
        
        cg.alpha = 0f;
        float t = 0f;
        
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            cg.alpha = t / fadeInDuration;
            yield return null;
        }
        
        cg.alpha = 1f;
    }
    
    /// <summary>
    /// 단어 선택됨
    /// </summary>
    private void OnWordSelected(WordDataSO word)
    {
        // 현재 단계에 저장
        switch (currentStep)
        {
            case 0:
                selectedSubject = word;
                break;
            case 1:
                selectedObject = word;
                break;
            case 2:
                selectedVerb = word;
                break;
        }
        
        // BasketPanel 슬롯 업데이트
        UpdateBasketSlot(word);
        
        // 사운드 재생
        if (WordSoundManager.Instance != null)
        {
            // 단어 고유 사운드 (예: '고양이' 소리)
            if (word.sound != null)
            {
                WordSoundManager.Instance.PlayClip(word.sound);
            }
            
            // 슬롯 끼워지는 소리 (UI 효과음)
            WordSoundManager.Instance.PlaySlotSound();
        }
        
        // 다음 단계로
        currentStep++;
        
        if (currentStep >= 3)
        {
            // 모든 선택 완료
            ClearButtons();
            
            // 선택된 3개 단어 표시 (클릭 비활성화)
            ShowSelectedWords();
            
            OnSelectionComplete?.Invoke();
        }
        else
        {
            ShowCurrentStep();
        }
    }
    
    private void UpdateBasketSlot(WordDataSO word)
    {
        // 슬롯 업데이트 비활성화됨
    }
    
    /// <summary>
    /// 선택된 3개 단어를 별도 컨테이너에 표시
    /// </summary>
    private void ShowSelectedWords()
    {
        if (selectedWordsContainer == null) return;
        
        // 기존 선택 단어 버튼 제거
        ClearSelectedWordsContainer();
        
        // 지정된 오브젝트 숨기기
        if (hideOnComplete != null)
        {
            hideOnComplete.SetActive(false);
        }
        
        // 가이드 텍스트 변경
        if (guideText != null)
        {
            guideText.text = completeGuideText;
        }
        
        WordDataSO[] selectedWords = new WordDataSO[] { selectedSubject, selectedObject, selectedVerb };
        
        foreach (var word in selectedWords)
        {
            if (word == null) continue;
            
            GameObject btnObj = Instantiate(wordButtonPrefab, selectedWordsContainer);
            
            // 배경 스프라이트 설정
            Sprite bgSprite = GetSpriteForStep(word.slotType);
            
            // 버튼 컴포넌트 - 배경 설정만
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                Image bgImage = btn.GetComponent<Image>();
                if (bgImage != null && bgSprite != null)
                {
                    bgImage.sprite = bgSprite;
                }
            }
            
            // 아이콘 설정
            Image iconImage = btnObj.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null && word.icon != null)
            {
                iconImage.sprite = word.icon;
            }
            
            // 텍스트 설정
            TextMeshProUGUI textComp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                bool isEnglish = LanguageManager.Instance != null && LanguageManager.Instance.IsEnglish;
                textComp.text = word.GetWord(isEnglish);
            }
        }
    }
    
    private void ClearSelectedWordsContainer()
    {
        if (selectedWordsContainer == null) return;
        
        foreach (Transform child in selectedWordsContainer)
        {
            Destroy(child.gameObject);
        }
    }
    
    private void ClearButtons()
    {
        foreach (var btn in currentButtons)
        {
            if (btn != null) Destroy(btn);
        }
        currentButtons.Clear();
    }
    
    /// <summary>
    /// 선택된 단어들로 프롬프트 생성
    /// </summary>
    public string GetPromptSentence()
    {
        if (!IsSelectionComplete) return "";
        
        string subject = selectedSubject.wordEnglish;
        string obj = selectedObject.wordEnglish;
        string verb = selectedVerb.wordEnglish;
        
        return $"{subject} {verb} {obj}";
    }
    
    /// <summary>
    /// 현재 문장 반환 (UI 표시용)
    /// </summary>
    public string GetCurrentSentence(bool isEnglish = false)
    {
        string subject = selectedSubject?.GetWord(isEnglish) ?? "???";
        string obj = selectedObject?.GetWord(isEnglish) ?? "???";
        string verb = selectedVerb?.GetWord(isEnglish) ?? "???";
        
        return isEnglish ? $"{subject} {verb} {obj}" : $"{subject} + {obj} + {verb}";
    }
    
    /// <summary>
    /// 리셋
    /// </summary>
    public void Reset()
    {
        StartSelection();
    }
}
