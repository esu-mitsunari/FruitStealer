using UnityEngine;
using UnityEngine.UIElements; // 💡 UI Toolkitを操作するために必要
using UnityEngine.SceneManagement; // 💡 シーンを切り替える（遷移する）ために必要

public class TitleManager : MonoBehaviour
{
    // UI Builderでボタンにつけた「Name (ID)」をここに正確に入力します
    [SerializeField] private string startButtonName = "StartButton";
    
    // 遷移先となるゲーム本編のシーン名
    [SerializeField] private string mainSceneName = "MainScene";

    void Start()
    {
        // 1. 同じオブジェクトについている UIDocument コンポーネントを取得
        UIDocument uiDocument = GetComponent<UIDocument>();
        
        if (uiDocument == null)
        {
            Debug.LogError("UIDocumentが見つかりません。TitleManagerはUIDocumentと同じオブジェクトにアタッチしてください。");
            return;
        }

        // 2. UIの根本（Root）となる要素を取得
        VisualElement root = uiDocument.rootVisualElement;

        // 3. 根本から、指定した名前（StartButton）のボタンを検索して持ってくる
        Button startButton = root.Q<Button>(startButtonName);

        if (startButton != null)
        {
            // 4. ボタンがクリックされたときに実行する関数（イベントメソッド）を登録
            startButton.clicked += OnStartButtonClicked;
            Debug.Log("スタートボタンのクリックイベントを正常に登録しました。");
        }
        else
        {
            Debug.LogError($"UIの中に '{startButtonName}' という名前のボタンが見つかりませんでした。UI BuilderのNameを確認してください。");
        }
    }

    // 💡 ボタンがクリックされた瞬間に呼び出される関数
    private void OnStartButtonClicked()
    {
        Debug.Log("STARTボタンが押されました！本編シーンへ切り替えます。");
        
        // 5. 指定したシーン名へ遷移する
        SceneManager.LoadScene(mainSceneName);
    }
}