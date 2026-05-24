using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float speed = 10f; // 弾の飛ぶ速度
    private float dropAmount = 0.5f;
    private float magnitude = 0.1f;
    private float duration = 0.4f;
    private float rotationSpeed = 720f;
    
    // 💡 誰がこの弾を撃った（所有している）かを区別するフラグ
    private bool _isDroppedByEnemy = false; 
    private float _targetX;
    private Vector2 _moveDirection;
    private bool _isHeadingDown = false;
    private float _thresholdX = 0.15f;
    
    public GameObject bulletEffect;

    void Start()
    {
        // 💡 敵から落とされたのではない場合（プレイヤーが通常通り上に撃った場合）のみ、リジッドボディで上へ飛ばす
        if (!_isDroppedByEnemy)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = transform.up * 10f; // 元の打ち出し速度
            }
        }

        // 💡 画面外に出ても消えないと重くなるので、3秒後に自動消滅させる
        Destroy(gameObject, 3f);
    }
    
    public void SetEnemyDroppedTarget(Player player)
    {
        _isDroppedByEnemy = true;
        
        // Obstacleと同じように、プレイヤーのX座標をターゲットにする
        _targetX = player.transform.position.x - Random.Range(-0.5f, 0.5f);

        float signX = System.Math.Sign(_targetX - transform.position.x);
        _moveDirection = new Vector2(signX, 0f);
        
        // 💡 敵から落とす場合は、通常の物理移動（物理速度）をオフにする
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        // 💡 敵から落とされた弾の場合のみ、Obstacleと同じカクッと折れる移動ロジックを実行する
        if (_isDroppedByEnemy)
        {
            if (!_isHeadingDown)
            {
                float distanceX = Mathf.Abs(transform.position.x - _targetX);

                if (distanceX <= _thresholdX)
                {
                    _moveDirection = Vector2.down;
                    _isHeadingDown = true;
                    transform.position = new Vector3(_targetX, transform.position.y, transform.position.z);
                }
            }

            transform.Translate(_moveDirection * speed * Time.deltaTime, Space.World);
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_isDroppedByEnemy)
        {
            // 💡 プレイヤーの攻撃としての当たり判定（従来通り）
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.FallDown(dropAmount);
                enemy.StartShake(duration, magnitude);
                Destroy(gameObject);
                Instantiate(bulletEffect, transform.position, transform.rotation);
            }
        }
        else
        {
            // 💡 敵から落とされた弾（回収用）としての当たり判定
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                // 🔴 プレイヤーの弾数を1つ増やす
                player.AddBullet(1);
                
                Debug.Log("🎒 弾を1発回収した！");
                Destroy(gameObject); // 画面から消す
            }
        }
    }
}