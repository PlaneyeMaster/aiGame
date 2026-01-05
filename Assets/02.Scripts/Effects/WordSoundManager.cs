using UnityEngine;

/// <summary>
/// 단어 사운드 매니저 - 단어 클릭 시 효과음 재생
/// </summary>
public class WordSoundManager : MonoBehaviour
{
    public static WordSoundManager Instance { get; private set; }
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("Default Sounds")]
    [SerializeField] private AudioClip defaultClickSound;
    [SerializeField] private AudioClip slotSound;
    [SerializeField] private AudioClip unslotSound;
    [SerializeField] private AudioClip completeSound;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    /// <summary>
    /// 단어별 사운드 재생
    /// </summary>
    public void PlayWordSound(string soundName)
    {
        if (string.IsNullOrEmpty(soundName))
        {
            PlayDefaultClick();
            return;
        }
        
        // Resources에서 사운드 로드
        AudioClip clip = Resources.Load<AudioClip>($"Sounds/{soundName}");
        
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            PlayDefaultClick();
        }
    }
    
    /// <summary>
    /// AudioClip 직접 재생
    /// </summary>
    public void PlayClip(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
    
    /// <summary>
    /// 기본 클릭 사운드
    /// </summary>
    public void PlayDefaultClick()
    {
        if (defaultClickSound != null)
            audioSource.PlayOneShot(defaultClickSound);
    }
    
    /// <summary>
    /// 슬롯 배치 사운드
    /// </summary>
    public void PlaySlotSound()
    {
        if (slotSound != null)
            audioSource.PlayOneShot(slotSound);
    }
    
    /// <summary>
    /// 슬롯 해제 사운드
    /// </summary>
    public void PlayUnslotSound()
    {
        if (unslotSound != null)
            audioSource.PlayOneShot(unslotSound);
    }
    
    /// <summary>
    /// 문장 완성 사운드
    /// </summary>
    public void PlayCompleteSound()
    {
        if (completeSound != null)
            audioSource.PlayOneShot(completeSound);
    }
}
