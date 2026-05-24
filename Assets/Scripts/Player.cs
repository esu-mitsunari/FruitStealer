using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform shotPoint;

    private float moveSpeed = 10f;
    private float jumpDuration = 0.2f; // 💡ジャンプ（突進）にかかる時間

    private bool _isJumping = false; // 💡ジャンプ中フラグ
    private bool _isStunned = false; // 💡硬直中フラグ
    private Vector3 _originalPosition; // ジャンプ前の元のY座標を覚える用
    
    private bool _playerIsShaking = false;
    
    private int _bulletCount = 0; // 💡 初期手持ち弾数（好きな数に設定してください）

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
        // クリア、ゲームオーバー、または自分が【硬直中・ジャンプ中】なら操作を受け付けない
        if (GameManager.Instance != null && 
           (GameManager.Instance.CurrentState == GameManager.GameState.GameClear || 
            GameManager.Instance.CurrentState == GameManager.GameState.GameOver)) return;

        if (_isStunned || _isJumping) return;

        // 通常の左右移動
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);
        float targetX = Mathf.MoveTowards(transform.position.x, mousePos.x, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.FuryMode)
            {
                // 💡 発狂モードなら、前方にジャンプして飛びつく！
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

    // 💡【追加】敵に向かってシュッとジャンプ（突進）するコルーチン
    private IEnumerator JumpAttackRoutine()
    {
        _isJumping = true;
        
        Enemy enemy = Object.FindFirstObjectByType<Enemy>();
        if (enemy == null) { _isJumping = false; yield break; }

        // ジャンプ開始時の座標と、目標（敵の座標）を設定
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(startPos.x, enemy.transform.position.y, startPos.z);

        float elapsed = 0f;

        // 1. 敵の方向へ滑らかに突進（前進）
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;
            // 座標を線形補間（Lerp）して前進させる
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // 2. 到着した瞬間（重なった瞬間）にGameManagerに成否判定を委託
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckCatchSuccess();
        }

        // 3. もしクリアしていなければ、元のY座標にシュッと戻る
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

    // 💡【追加】GameManagerから呼び出されるペナルティ用の窓口
    public void ApplyPenalty(float duration)
    {
        StartCoroutine(PenaltyRoutine(duration));
    }

    // 💡【追加】数秒間動けなくなるペナルティコルーチン
    private IEnumerator PenaltyRoutine(float duration)
    {
        _isStunned = true;
        
        // わかりやすいように、気絶中はプレイヤーの色を少し暗く（赤っぽくなど）しても面白いです
        // 例: GetComponent<SpriteRenderer>().color = Color.red;

        yield return new WaitForSeconds(duration);

        // 元の色に戻す
        // 例: GetComponent<SpriteRenderer>().color = Color.white;

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