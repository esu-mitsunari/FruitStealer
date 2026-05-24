using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    [Header("UIの設定")]
    [SerializeField] private UIDocument uiDocument;
    public static GameManager Instance { get; private set; }
    private const float stopDuration = 0.5f;

    public enum GameState
    {
        Shooting,
        FuryMode,
        GameClear,
        GameOver
    }

    public GameState CurrentState { get; private set; } = GameState.Shooting;

    private float catchReadyY = -2.0f;

    private VisualElement _clearPanel;
    private Button _titleButton;
    private Label _bulletLabel;

    private Player _player;
    private Enemy _enemy;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        _player = Object.FindFirstObjectByType<Player>();
        _enemy = Object.FindFirstObjectByType<Enemy>();
        if (uiDocument != null)
        {
            VisualElement root = uiDocument.rootVisualElement;

            _clearPanel = root.Q<VisualElement>("ClearElement");

            _titleButton = root.Q<Button>("RestartButton");
            if (_titleButton != null)
            {
                _titleButton.clicked += OnTitleButtonClicked;
            }

            _bulletLabel = root.Q<Label>("Bullet");
        }

        if (_player != null)
        {
            UpdateBulletCount(_player.BulletCount);
        }
    }

    public void UpdateBulletCount(int count)
    {
        if (_bulletLabel != null)
        {
            _bulletLabel.text = $"Bullet: {count}";
        }
    }

    void Update()
    {
        if (CurrentState == GameState.Shooting && _enemy != null)
        {
            if (_enemy.transform.position.y <= catchReadyY)
            {
                TriggerFuryMode();
            }
        }
    }

    private void TriggerFuryMode()
    {
        CurrentState = GameState.FuryMode;
        Debug.Log("敵が発狂");
        if (_enemy != null) _enemy.SetFuryMode();
    }

    public void CheckCatchSuccess()
    {
        if (CurrentState != GameState.FuryMode || _player == null || _enemy == null) return;

        float distanceX = Mathf.Abs(_player.transform.position.x - _enemy.transform.position.x);

        if (distanceX <= 0.8f)
        {
            CurrentState = GameState.GameClear;
            Debug.Log("ゲームクリア！");
            _enemy.StopMovement(stopDuration);
            if (_clearPanel != null)
            {
                _clearPanel.style.display = DisplayStyle.Flex;
            }
        }
        else
        {
            _player.ApplyPenalty(0.4f);
        }
    }
    private void OnTitleButtonClicked()
    {
        SceneManager.LoadScene("TitleScene");
    }
}