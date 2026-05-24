using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    private float scrollSpeed = 8f;
    private Transform[] _layers;
    private float _layerHeight;

    void Start()
    {
        int childCount = transform.childCount;
        _layers = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            _layers[i] = transform.GetChild(i);
        }

        if (_layers.Length > 0)
        {
            SpriteRenderer sr = _layers[0].GetComponent<SpriteRenderer>();
            if (sr != null) _layerHeight = sr.bounds.size.y;
        }
    }

    void Update()
    {
        if (_layers == null || _layers.Length == 0 || _layerHeight <= 0f) return;

        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.GameClear) return;

        float totalHeight = _layerHeight * _layers.Length;
        foreach (var layer in _layers)
        {
            layer.position += Vector3.down * scrollSpeed * Time.deltaTime;
            if (layer.position.y <= -_layerHeight)
            {
                layer.position += Vector3.up * totalHeight;
            }
        }
    }
}
