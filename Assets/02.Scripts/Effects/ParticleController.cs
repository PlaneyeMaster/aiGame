using UnityEngine;

/// <summary>
/// 파티클 효과 컨트롤러
/// </summary>
public class ParticleController : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] private ParticleSystem magicParticles;
    [SerializeField] private ParticleSystem touchParticles;
    [SerializeField] private ParticleSystem sparkleParticles;
    
    [SerializeField] private int touchEmitCount = 20;
    
    private Camera mainCamera;
    private bool isMagicActive = false;
    
    private void Start()
    {
        mainCamera = Camera.main;
        if (magicParticles != null) magicParticles.Stop();
        
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
    }
    
    private void Update()
    {
        if (isMagicActive)
            HandleTouch();
    }
    
    private void OnGameStateChanged(GameState state)
    {
        if (state == GameState.Generating)
            StartMagicEffect();
        else
            StopMagicEffect();
    }
    
    public void StartMagicEffect()
    {
        isMagicActive = true;
        if (magicParticles != null) magicParticles.Play();
        if (sparkleParticles != null) sparkleParticles.Play();
    }
    
    public void StopMagicEffect()
    {
        isMagicActive = false;
        if (magicParticles != null) magicParticles.Stop();
        if (sparkleParticles != null) sparkleParticles.Stop();
    }
    
    private void HandleTouch()
    {
        // 1. 마우스
        if (UnityEngine.InputSystem.Mouse.current != null && 
            UnityEngine.InputSystem.Mouse.current.leftButton.isPressed)
        {
            SpawnTouchParticle(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        }
        
        // 2. 터치
        if (UnityEngine.InputSystem.Touchscreen.current != null)
        {
            var touches = UnityEngine.InputSystem.Touchscreen.current.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                if (touch.press.isPressed)
                {
                    SpawnTouchParticle(touch.position.ReadValue());
                }
            }
        }
    }
    
    private void SpawnTouchParticle(Vector3 screenPos)
    {
        if (touchParticles == null || mainCamera == null) return;
        
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        
        touchParticles.transform.position = worldPos;
        touchParticles.Emit(touchEmitCount);
    }
}
