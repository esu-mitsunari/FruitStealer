using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private float moveSpeed = 3f;
    private float _minInterval = 2f;
    private float _maxInterval = 4f;
    private float changeInterval;
    private float initialDelay = 1f;

    private float furySpeed = 12f;       // 💡発狂時は速度4倍！
    private float furyMinInterval = 0.0f; // 💡次の移動先を決めるスパンも爆速に
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
    private bool _isDead = false; // 💡クリア・ゲームオーバー時に動きを止めるフラグ

    void Start()
    {
        _targetX = transform.position.x;
        _targetY = transform.position.y;
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // 💡 既存の移動コルーチンをスタート
        StartCoroutine(RandomMoveRoutine());

        // 🔴【追加】弾を撃つためだけの専用コルーチンを新しくスタートさせる
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
        if (_isDead) return; // 💡決着がついたら一切の移動・向き更新を止める

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
            // 💡 50%の確率で、障害物(Obstacle)の代わりに「回収できる弾」を落とす
            if (Random.value < 0.5f)
            {
                DropRecoverableBullet();
            }
            else
            {
                Shoot(); // 従来の Obstacle を落とす処理
            }

            yield return new WaitForSeconds(shootInterval);
        }
    }
    
    void Shoot()
    {
        Player player = Object.FindFirstObjectByType<Player>();
        if (player == null) return;

        Vector3 spawnPosition = transform.position;
        
        // 💡 プレハブ(obstacle)から、新しいインスタンス(spawnedObstacle)を生成
        GameObject spawnedObstacle = Instantiate(obstacle, spawnPosition, Quaternion.identity);
        
        // 💡 生成したインスタンスからコンポーネントを正しく取得する
        Obstacle obstacleScript = spawnedObstacle.GetComponent<Obstacle>();
        if (obstacleScript != null)
        {
            // 弾側のスクリプトにプレイヤーの情報を渡す
            obstacleScript.SetTarget(player);
        }
    }
    
    void DropRecoverableBullet()
    {
        // 発狂モード中などは弾を落とさないガード
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Shooting) return;

        Player player = Object.FindFirstObjectByType<Player>();
        if (player == null) return;

        Vector3 spawnPosition = transform.position;
        
        // プレイヤーの弾プレハブを敵の位置に生成
        GameObject spawnedBullet = Instantiate(bullet, spawnPosition, Quaternion.identity);
        
        Bullet bulletScript = spawnedBullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            // 💡 弾側に「敵から落とされた追跡弾」としての初期設定を行わせる
            bulletScript.SetEnemyDroppedTarget(player);
        }
    }
    
    public void FallDown(float amount)
    {
        // 💡【安全弁】発狂モード（FuryMode）に突入している間は、それ以上Y座標を下げない
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Shooting) return;
    
        _targetY -= amount;
    }

    // 💡【追加】GameManagerから呼ばれる、発狂モード突入関数
    public void SetFuryMode()
    {
        moveSpeed = furySpeed;
        _minInterval = furyMinInterval;
        _maxInterval = furyMaxInterval;
        GetComponent<SpriteRenderer>().color = Color.red;

        // 捕獲しやすいように、Y座標をプレイヤーの少し上（例：-1.5など）に強制固定しても面白いです
        _targetY = transform.position.y; 
    }

    // 💡【追加】決着時に対象の動きを完全停止させる関数
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