using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform shotPoint;

    private float moveSpeed = 10f;
    private float jumpDuration = 0.2f;

    private bool _isJumping = false;
    private bool _isStunned = false;
    private Vector3 _originalPosition;

    private bool _playerIsShaking = false;

    private int _bulletCount = 0;

    public int BulletCount => _bulletCount;

    [SerializeField] private Sprite[] walkSprites;
    private float walkFrameInterval = 0.3f;
    private SpriteRenderer _spriteRenderer;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _originalPosition = transform.position;
        _spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(WalkAnimationRoutine());
    }

    private IEnumerator WalkAnimationRoutine()
    {
        int index = 0;
        while (true)
        {
            if (walkSprites != null && walkSprites.Length > 0 && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = walkSprites[index];
                index = (index + 1) % walkSprites.Length;
            }
            yield return new WaitForSeconds(walkFrameInterval);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null &&
           (GameManager.Instance.CurrentState == GameManager.GameState.GameClear ||
            GameManager.Instance.CurrentState == GameManager.GameState.GameOver)) return;

        if (_isStunned || _isJumping) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);
        float targetX = Mathf.MoveTowards(transform.position.x, mousePos.x, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.FuryMode)
            {
                StartCoroutine(JumpAttackRoutine());
            }
            else
            {
                if (_bulletCount > 0)
                {
                    Shoot();
                    _bulletCount--;
                    if (GameManager.Instance != null) GameManager.Instance.UpdateBulletCount(_bulletCount);
                }

            }
        }
    }

    public void AddBullet(int amount)
    {
        _bulletCount += amount;
        Debug.Log($"現在の所持弾数: {_bulletCount}");
        if (GameManager.Instance != null) GameManager.Instance.UpdateBulletCount(_bulletCount);
    }

    void Shoot()
    {
        Enemy enemy = Object.FindFirstObjectByType<Enemy>();
        if (enemy == null) return;

        Vector3 spawnPosition = shotPoint != null ? shotPoint.position : transform.position;
        Vector2 targetDirection = Vector2.up;

        Vector3 heading = enemy.transform.position - spawnPosition;
        targetDirection = new Vector2(heading.x, heading.y).normalized;

        Quaternion bulletRotation = Quaternion.FromToRotation(Vector3.up, targetDirection);
        Instantiate(bullet, spawnPosition, bulletRotation);
    }

    private IEnumerator JumpAttackRoutine()
    {
        _isJumping = true;

        Enemy enemy = Object.FindFirstObjectByType<Enemy>();
        if (enemy == null) { _isJumping = false; yield break; }

        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(startPos.x, enemy.transform.position.y, startPos.z);

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckCatchSuccess();
        }

        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.GameClear)
        {
            elapsed = 0f;
            Vector3 currentPos = transform.position;
            Vector3 returnPos = new Vector3(currentPos.x, _originalPosition.y, currentPos.z);

            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / jumpDuration;
                transform.position = Vector3.Lerp(currentPos, returnPos, t);
                yield return null;
            }
        }

        _isJumping = false;
    }

    public void ApplyPenalty(float duration)
    {
        StartCoroutine(PenaltyRoutine(duration));
    }

    private IEnumerator PenaltyRoutine(float duration)
    {
        _isStunned = true;

        yield return new WaitForSeconds(duration);

        _isStunned = false;
    }

    public void StartShake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        _playerIsShaking = true;
        Vector3 originalPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;
            transform.position = new Vector3(originalPos.x + offsetX, originalPos.y + offsetY, originalPos.z);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.position = originalPos;
        _playerIsShaking = false;
    }
}