using UnityEngine;

/// <summary>
/// Push Out 미니게임에서 플레이어를 밀어내는 푸셔
/// - 지정된 방향으로 이동하며 플레이어와 접촉 시 임펄스를 가한다
/// </summary>
[DisallowMultipleComponent]
public class PushOutPusher : MonoBehaviour
{
    [Header("References")]
    [Tooltip("푸셔 Rigidbody2D (Kinematic 또는 Dynamic)")]
    [SerializeField] private Rigidbody2D body;
    [Tooltip("플레이어와 접촉 판정용 Collider2D (Trigger 권장)")]
    [SerializeField] private Collider2D hitCollider;

    [Header("Movement")]
    [Tooltip("이동 속도(초기값)")]
    [SerializeField] private float defaultSpeed = 3f;

    private Vector2 _direction = Vector2.left;
    private float _speed;
    private float _impulse;
    private float _remainingLife;

    /// <summary>푸셔 초기화</summary>
    public void Configure(Vector2 direction, float speed, float impulse, float lifetime)
    {
        if (!body) body = GetComponent<Rigidbody2D>();
        if (!hitCollider) hitCollider = GetComponent<Collider2D>();

        _direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.left;
        _speed = speed > 0f ? speed : defaultSpeed;
        _impulse = impulse;
        _remainingLife = lifetime;

        if (body)
        {
            body.simulated = true;
            body.gravityScale = 0f;
            body.drag = 0f;
            body.angularDrag = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.linearVelocity = _direction * _speed;
        }
    }

    private void FixedUpdate()
    {
        if (!body) return;

        _remainingLife -= Time.fixedDeltaTime;
        if (_remainingLife <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        body.linearVelocity = _direction * _speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PushOutPlayer>();
        if (!player && other.attachedRigidbody)
        {
            player = other.attachedRigidbody.GetComponent<PushOutPlayer>();
        }

        if (player != null)
        {
            player.ApplyImpulse(_direction * _impulse);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var player = collision.collider.GetComponent<PushOutPlayer>();
        if (!player && collision.rigidbody)
        {
            player = collision.rigidbody.GetComponent<PushOutPlayer>();
        }

        if (player != null)
        {
            player.ApplyImpulse(_direction * _impulse);
        }
    }
}
