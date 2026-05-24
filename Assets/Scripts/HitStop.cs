using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    // 💡 どこからでも HitStopManager.Instance で呼べるようにする（シングルトン）
    public static HitStop Instance { get; private set; }

    private bool _isHitStopping = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 💡 外部からこのメソッドを呼び出す（例: 0.1秒間停止）
    public void CallHitStop(float duration)
    {
        // すでにヒットストップ中なら二重に実行しない
        if (_isHitStopping) return;

        StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        _isHitStopping = true;

        // 1. ゲーム内の時間を完全に止める（スローにしたいなら 0.05f などにする）
        Time.timeScale = 0f;

        // ⚠️【超重要】Time.timeScale が 0 のときは WaitForSeconds は使えない（時間が進まないため）
        // そのため、現実に流れている時間を測る WaitForSecondsRealtime を使う
        yield return new WaitForSecondsRealtime(duration);

        // 2. ゲーム内の時間を通常速度に戻す
        Time.timeScale = 1f;

        _isHitStopping = false;
    }
}