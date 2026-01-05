using UnityEngine;

/// <summary>
/// 단어 사운드 매니저 - 단어 클릭 시 효과음 재생
/// </summary>
public class WordSoundManager : MonoBehaviour
{
    public static WordSoundManager Instance { get; private set; }
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip bgmClip;
    
    [Header("Default Sounds")]
    [SerializeField] private AudioClip defaultClickSound;
    [SerializeField] private AudioClip slotSound;
    [SerializeField] private AudioClip unslotSound;
    [SerializeField] private AudioClip completeSound;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않음
            
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        PlayBGM();
    }
    
    private void InitializeAudioSources()
    {
        // 효과음 소스
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
            
        // BGM 소스
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.volume = 0.5f;
        }
    }
    
    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBGM()
    {
        if (bgmSource != null && bgmClip != null)
        {
            if (bgmSource.isPlaying) return;
            
            bgmSource.clip = bgmClip;
            bgmSource.Play();
        }
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
    /// 기본 클릭 사운드 (버튼음)
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
