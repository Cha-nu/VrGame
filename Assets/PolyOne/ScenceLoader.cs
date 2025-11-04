using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 씬 이름을 상수로 정의하여 오타 방지 및 코드 가독성 향상
    private const string BasicSceneName = "BasicScene"; // 메인 게임이 시작되는 씬

    /// <summary>
    /// 게임 시작 버튼 또는 재시작 버튼 클릭 시 호출됩니다.
    /// SampleScene에서 BasicScene으로 이동합니다.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("Loading Main Game Scene: " + BasicSceneName);
        SceneManager.LoadScene(BasicSceneName);
    }

    public void RestartGame()
    {
        Debug.Log("Restart Game Scene: " + BasicSceneName);
        SceneManager.LoadScene(BasicSceneName);
    }
    /// 애플리케이션을 종료합니다.
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("Exiting Application...");

        // 런타임 환경에서 애플리케이션을 종료합니다.
        Application.Quit();

        // 에디터에서 실행 중인 경우 재생을 중지합니다.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}