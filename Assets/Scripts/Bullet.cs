using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float speed = 10f;
    private float dropAmount = 0.5f;
    private float magnitude = 0.1f;
    private float duration = 0.4f;
    private float rotationSpeed = 720f;

    private bool _isDroppedByEnemy = false;
    private float _targetX;
    private Vector2 _moveDirection;
    private bool _isHeadingDown = false;
    private float _thresholdX = 0.15f;

    public GameObject bulletEffect;

    void Start()
    {
        if (!_isDroppedByEnemy)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = transform.up * 10f;
            }
        }

        Destroy(gameObject, 3f);
    }

    public void SetEnemyDroppedTarget(Player player)
    {
        _isDroppedByEnemy = true;

        _targetX = player.transform.position.x - Random.Range(-0.5f, 0.5f);

        float signX = System.Math.Sign(_targetX - transform.position.x);
        _moveDirection = new Vector2(signX, 0f);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

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
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.AddBullet(1);

                Debug.Log("弾を回収");
                Destroy(gameObject);
            }
        }
    }
}