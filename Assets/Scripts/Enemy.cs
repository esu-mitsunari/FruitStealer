using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private float moveSpeed = 3f;
    private float _minInterval = 2f;
    private float _maxInterval = 4f;
    private float changeInterval;
    private float initialDelay = 1f;

    private float furySpeed = 12f;
    private float furyMinInterval = 0.0f;
    private float furyMaxInterval = 0.3f;

    private float _targetX;
    private float _targetY;
    private float _minX = -3.5f;
    private float _maxX = 3.5f;

    private float shootInterval = 0.5f;
    [SerializeField] private GameObject obstacle;
    [SerializeField] private GameObject bullet;

    [SerializeField] private Sprite[] walkSprites;
    private float walkFrameInterval = 0.3f;
    private SpriteRenderer _spriteRenderer;

    private bool _enemyIsShaking = false;
    private bool _isDead = false;

    void Start()
    {
        _targetX = transform.position.x;
        _targetY = transform.position.y;
        _spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(RandomMoveRoutine());

        StartCoroutine(ShootRoutine());

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
        if (_isDead) return;

        float nextX = Mathf.MoveTowards(transform.position.x, _targetX, moveSpeed * Time.deltaTime);

        if (!_enemyIsShaking)
        {
            transform.position = new Vector3(nextX, _targetY, transform.position.z);
        }
    }

    private IEnumerator RandomMoveRoutine()
    {
        yield return new WaitForSeconds(initialDelay);
        while (true)
        {
            changeInterval = Random.Range(_minInterval, _maxInterval);
            yield return new WaitForSeconds(changeInterval);
            _targetX = Random.Range(_minX, _maxX);
        }
    }

    private IEnumerator ShootRoutine()
    {
        yield return new WaitForSeconds(1.0f);

        while (true)
        {
            if (Random.value < 0.5f)
            {
                DropRecoverableBullet();
            }
            else
            {
                Shoot();
            }

            yield return new WaitForSeconds(shootInterval);
        }
    }

    void Shoot()
    {
        Player player = Object.FindFirstObjectByType<Player>();
        if (player == null) return;

        Vector3 spawnPosition = transform.position;

        GameObject spawnedObstacle = Instantiate(obstacle, spawnPosition, Quaternion.identity);

        Obstacle obstacleScript = spawnedObstacle.GetComponent<Obstacle>();
        if (obstacleScript != null)
        {
            obstacleScript.SetTarget(player);
        }
    }

    void DropRecoverableBullet()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Shooting) return;

        Player player = Object.FindFirstObjectByType<Player>();
        if (player == null) return;

        Vector3 spawnPosition = transform.position;

        GameObject spawnedBullet = Instantiate(bullet, spawnPosition, Quaternion.identity);

        Bullet bulletScript = spawnedBullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetEnemyDroppedTarget(player);
        }
    }

    public void FallDown(float amount)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Shooting) return;

        _targetY -= amount;
    }

    public void SetFuryMode()
    {
        moveSpeed = furySpeed;
        _minInterval = furyMinInterval;
        _maxInterval = furyMaxInterval;
        GetComponent<SpriteRenderer>().color = Color.red;

        _targetY = transform.position.y;
    }

    public void StopMovement(float stopDuration)
    {
        HitStop.Instance.CallHitStop(stopDuration);
        _isDead = true;
        StopAllCoroutines();
    }

    public void StartShake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        _enemyIsShaking = true;
        Vector3 originalPos = new Vector3(transform.position.x, _targetY, transform.position.z);
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
        _enemyIsShaking = false;
    }
}