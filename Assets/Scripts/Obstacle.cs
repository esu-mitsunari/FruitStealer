using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private float speed = 5f;
    private float magnitude = 0.1f;
    private float duration = 0.4f;
    private float rotationSpeed = 720f;

    private float _targetX;
    private Vector2 _moveDirection;
    private bool _isHeadingDown = false;
    private float _thresholdX = 0.15f;

    void Start()
    {
        Destroy(gameObject, 3f);
    }

    public void SetTarget(Player player)
    {
        _targetX = player.transform.position.x - Random.Range(-0.5f, 0.5f);

        float signX = System.Math.Sign(_targetX - transform.position.x);
        _moveDirection = new Vector2(signX, 0f);
    }

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

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

    void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            player.StartShake(duration, magnitude);
            Destroy(gameObject);
        }
    }
}