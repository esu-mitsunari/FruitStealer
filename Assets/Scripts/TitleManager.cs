using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private string startButtonName = "StartButton";

    [SerializeField] private string mainSceneName = "MainScene";

    void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        Button startButton = root.Q<Button>(startButtonName);

        if (startButton != null)
        {
            startButton.clicked += OnStartButtonClicked;
        }
        else
        {}
    }

    private void OnStartButtonClicked()
    {
        Debug.Log("STARTボタン");

        SceneManager.LoadScene(mainSceneName);
    }
}