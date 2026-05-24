using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private float speed = 5f; // 弾の飛ぶ速度
    private float magnitude = 0.1f;
    private float duration = 0.4f;
    private float rotationSpeed = 720f;

    private float _targetX;          // 💡 目標とするX座標（ズレを含む）
    private Vector2 _moveDirection; // 💡 弾の移動方向ベクトル
    private bool _isHeadingDown = false; // 💡 すでに真下に折れたかどうかのフラグ
    private float _thresholdX = 0.15f;   // 💡 どこまで近づいたら「到達」とみなすかの許容範囲

    void Start()
    {
        // 💡 画面外に出ても消えないと重くなるので、3秒後に自動消滅させる
        Destroy(gameObject, 3f);
    }

    // 💡 Enemy.cs から弾が生成された瞬間に呼び出される設定関数
    public void SetTarget(Player player)
    {
        // 💡 プレイヤーの現在のX座標から、指定の範囲（-0.5〜0.5）でランダムに狙いをズラして記憶する
        _targetX = player.transform.position.x - Random.Range(-0.5f, 0.5f);

        // 💡 生まれた位置から、狙いのX座標が右にあれば 1（右進）、左にあれば -1（左進）を初期方向にする
        float signX = System.Math.Sign(_targetX - transform.position.x);
        _moveDirection = new Vector2(signX, 0f);
    }

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        // 💡 まだ下に折れ曲がっていない場合、記憶した _targetX との距離を監視する
        if (!_isHeadingDown)
        {
            // 自分のX座標と、狙いのX座標の「差の絶対値」を計算
            float distanceX = Mathf.Abs(transform.position.x - _targetX);

            // 設定した許容範囲（0.15マス以内）まで近づいたら、カクッと真下に折れる
            if (distanceX <= _thresholdX)
            {
                _moveDirection = Vector2.down; // 移動方向を真下 (0, -1) に変更
                _isHeadingDown = true;

                // 💡 カクッと折れたブレを綺麗にするため、X座標をターゲット位置にカチッと完全に固定する
                transform.position = new Vector3(_targetX, transform.position.y, transform.position.z);
            }
        }

        // 計算された移動方向（左右、または真下）に向かって等速移動
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