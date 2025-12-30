using UnityEngine;
using TMPro;

/// <summary>
/// 플로팅 단어 버블
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class WordBubble : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer bubbleRenderer;
    [SerializeField] private TextMeshPro wordText;
    [SerializeField] private float selectedScale = 1.2f;

    [Header("Animation")]
    [SerializeField] private float moveToBasketDuration = 0.5f;

    private Rigidbody2D rb;
    private WordData wordData;
    private bool isSelected = false;
    private bool isMoving = false;
    private Vector3 originalPosition;

    public WordData Data => wordData;
    public bool IsSelected => isSelected;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0.5f;
    }

    private void Start()
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        rb.AddForce(dir * moveSpeed, ForceMode2D.Impulse);
    }

    public void Initialize(WordData data)
    {
        wordData = data;
        Debug.Log($"[WordBubble] Initialize: {data.word}, wordText null? {wordText == null}");

        if (wordText != null)
        {
            wordText.text = data.word;
            Debug.Log($"[WordBubble] Text set to: {wordText.text}");
        }
        else
        {
            Debug.LogError("[WordBubble] wordText is NULL! Check prefab reference.");
        }

        if (bubbleRenderer != null) bubbleRenderer.color = data.GetCategoryColor();
        originalPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Vector2 normal = collision.contacts[0].normal;
            Vector2 dir = Vector2.Reflect(rb.linearVelocity.normalized, normal);
            rb.linearVelocity = dir * moveSpeed;
        }
    }

    private void Update()
    {
        // New Input System 처리

        // 1. 마우스 클릭
        if (UnityEngine.InputSystem.Mouse.current != null &&
            UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            CheckInput(mousePos);
        }
        // 2. 터치 (모바일)
        else if (UnityEngine.InputSystem.Touchscreen.current != null &&
                 UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = UnityEngine.InputSystem.Touchscreen.current.primaryTouch.position.ReadValue();
            CheckInput(touchPos);
        }
    }

    private void CheckInput(Vector3 screenPos)
    {
        // UI 클릭 시 무시 로직 제거 (버블 클릭 우선)
        /*
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        */

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit != null && hit.transform == transform)
        {
            Debug.Log($"[WordBubble] Clicked via Raycast! Word: {wordData?.word}");
            HandleClick();
        }
    }

    private void HandleClick()
    {
        if (isMoving) return;
        if (GameManager.Instance == null)
        {
            Debug.LogError("[WordBubble] GameManager.Instance is NULL!");
            return;
        }

        if (GameManager.Instance.CurrentState == GameState.Generating ||
            GameManager.Instance.CurrentState == GameState.Viewing) return;

        if (isSelected) Deselect();
        else Select();
    }

    public void Select()
    {
        if (!GameManager.Instance.CanSelectMore) return;

        if (GameManager.Instance.SelectWord(wordData))
        {
            isSelected = true;
            transform.localScale = Vector3.one * selectedScale;

            if (BasketController.Instance != null)
                StartCoroutine(MoveToBasket(BasketController.Instance.GetNextSlotPosition()));
        }
    }

    public void Deselect()
    {
        if (GameManager.Instance.DeselectWord(wordData))
        {
            isSelected = false;
            transform.localScale = Vector3.one;
            StartCoroutine(ReturnToOriginal());
        }
    }

    private System.Collections.IEnumerator MoveToBasket(Vector3 target)
    {
        isMoving = true;
        rb.simulated = false;

        Vector3 start = transform.position;
        float t = 0;

        while (t < moveToBasketDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, t / moveToBasketDuration);
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }

    private System.Collections.IEnumerator ReturnToOriginal()
    {
        isMoving = true;

        Vector3 start = transform.position;
        float t = 0;

        while (t < moveToBasketDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, originalPosition, t / moveToBasketDuration);
            yield return null;
        }

        transform.position = originalPosition;
        rb.simulated = true;
        rb.AddForce(Random.insideUnitCircle * moveSpeed, ForceMode2D.Impulse);
        isMoving = false;
    }

    public void ResetSelection()
    {
        if (isSelected)
        {
            isSelected = false;
            transform.localScale = Vector3.one;
            StartCoroutine(ReturnToOriginal());
        }
    }
}
