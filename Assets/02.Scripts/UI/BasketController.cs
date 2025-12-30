using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 상상 바구니 UI
/// </summary>
public class BasketController : MonoBehaviour
{
    public static BasketController Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private RectTransform basketPanel;
    [SerializeField] private Transform[] wordSlots;
    [SerializeField] private TextMeshProUGUI sentencePreview;
    [SerializeField] private Button generateButton;
    [SerializeField] private TextMeshProUGUI generateButtonText;

    private List<WordData> words = new List<WordData>();
    private int nextSlot = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnWordSelected += OnWordSelected;
            GameManager.Instance.OnWordDeselected += OnWordDeselected;
            GameManager.Instance.OnSelectionCleared += OnSelectionCleared;
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
        }

        if (generateButton != null)
            generateButton.onClick.AddListener(OnGenerateClicked);

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnWordSelected -= OnWordSelected;
            GameManager.Instance.OnWordDeselected -= OnWordDeselected;
            GameManager.Instance.OnSelectionCleared -= OnSelectionCleared;
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }
    }

    public Vector3 GetNextSlotPosition()
    {
        if (wordSlots != null && nextSlot < wordSlots.Length)
            return wordSlots[nextSlot].position;
        return basketPanel != null ? basketPanel.position : transform.position;
    }

    private void OnWordSelected(WordData word)
    {
        words.Add(word);
        nextSlot = words.Count;
        UpdateUI();
    }

    private void OnWordDeselected(WordData word)
    {
        words.Remove(word);
        nextSlot = words.Count;
        UpdateUI();
    }

    private void OnSelectionCleared()
    {
        words.Clear();
        nextSlot = 0;
        UpdateUI();
    }

    private void OnGameStateChanged(GameState state)
    {
        if (generateButton != null)
            generateButton.interactable = (state == GameState.Selecting && GameManager.Instance.CanGenerate);
    }

    private void UpdateUI()
    {
        // 슬롯 텍스트 업데이트
        if (wordSlots != null)
        {
            for (int i = 0; i < wordSlots.Length; i++)
            {
                var text = wordSlots[i].GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = i < words.Count ? words[i].word : "?";
            }
        }

        // 문장 미리보기
        if (sentencePreview != null)
        {
            if (words.Count == 0)
                sentencePreview.text = "Select Word";
            else
            {
                List<string> w = new List<string>();
                foreach (var word in words) w.Add(word.word);
                sentencePreview.text = string.Join(" + ", w);
            }
        }

        // 버튼 상태
        UpdateButton();
    }

    private void UpdateButton()
    {
        if (generateButton == null) return;

        bool can = GameManager.Instance != null && GameManager.Instance.CanGenerate;
        generateButton.interactable = can;

        if (generateButtonText != null)
        {
            if (can)
                generateButtonText.text = "AIgen";
            else
            {
                int need = (GameManager.Instance?.MinWordsToGenerate ?? 2) - words.Count;
                generateButtonText.text = $"Word {need}gogo";
            }
        }
    }

    private void OnGenerateClicked()
    {
        Debug.Log("[BasketController] Generate Button Clicked!");
        GameManager.Instance?.StartGeneration();
    }
}
