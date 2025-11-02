using UnityEngine;

/// <summary>
/// IPausable을 구현하는 범용 컴포넌트
/// 다양한 오브젝트에 붙여서 일시정지 기능을 쉽게 추가할 수 있습니다
/// </summary>
public class PausableObject : MonoBehaviour, IPausable
{
    [Header("Pause Settings")]
    [SerializeField] private bool _pauseAnimator = true;
    [SerializeField] private bool _pauseRigidbody = true;
    [SerializeField] private bool _pauseParticles = true;
    [SerializeField] private bool _pauseAudioSource = true;
    
    [Header("Runtime (ReadOnly)")]
    [SerializeField] private bool _isPaused = false;
    
    // 컴포넌트 캐시
    private Animator _animator;
    private Rigidbody2D _rigidbody2D;
    private ParticleSystem _particleSystem;
    private AudioSource _audioSource;
    
    // 일시정지 전 상태 저장
    private Vector2 _savedVelocity;
    private float _savedAnimatorSpeed;
    private bool _wasParticlePlaying;
    private bool _wasAudioPlaying;

    public bool IsPaused => _isPaused;

    private void Awake()
    {
        // 컴포넌트 캐시
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _particleSystem = GetComponent<ParticleSystem>();
        _audioSource = GetComponent<AudioSource>();
        
        // PauseManager에 등록
        PauseManager.Instance.RegisterPausable(this);
    }

    private void OnDestroy()
    {
        // PauseManager에서 등록 해제
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.UnregisterPausable(this);
        }
    }

    public void Pause()
    {
        if (_isPaused) return;
        
        _isPaused = true;
        
        // Animator 일시정지
        if (_pauseAnimator && _animator != null)
        {
            _savedAnimatorSpeed = _animator.speed;
            _animator.speed = 0f;
        }
        
        // Rigidbody2D 일시정지
        if (_pauseRigidbody && _rigidbody2D != null)
        {
            _savedVelocity = _rigidbody2D.linearVelocity;
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.simulated = false;
        }
        
        // ParticleSystem 일시정지
        if (_pauseParticles && _particleSystem != null)
        {
            _wasParticlePlaying = _particleSystem.isPlaying;
            if (_wasParticlePlaying)
            {
                _particleSystem.Pause();
            }
        }
        
        // AudioSource 일시정지
        if (_pauseAudioSource && _audioSource != null)
        {
            _wasAudioPlaying = _audioSource.isPlaying;
            if (_wasAudioPlaying)
            {
                _audioSource.Pause();
            }
        }
        
        Debug.Log($"[PausableObject] {gameObject.name} 일시정지");
    }

    public void Resume()
    {
        if (!_isPaused) return;
        
        _isPaused = false;
        
        // Animator 재개
        if (_pauseAnimator && _animator != null)
        {
            _animator.speed = _savedAnimatorSpeed;
        }
        
        // Rigidbody2D 재개
        if (_pauseRigidbody && _rigidbody2D != null)
        {
            _rigidbody2D.simulated = true;
            _rigidbody2D.linearVelocity = _savedVelocity;
        }
        
        // ParticleSystem 재개
        if (_pauseParticles && _particleSystem != null)
        {
            if (_wasParticlePlaying)
            {
                _particleSystem.Play();
            }
        }
        
        // AudioSource 재개
        if (_pauseAudioSource && _audioSource != null)
        {
            if (_wasAudioPlaying)
            {
                _audioSource.UnPause();
            }
        }
        
        Debug.Log($"[PausableObject] {gameObject.name} 재개");
    }
}