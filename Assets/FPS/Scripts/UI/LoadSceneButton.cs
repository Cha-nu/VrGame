using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.FPS.UI
{
    // [!!] MonoBehaviour만 상속받습니다.
    public class LoadSceneButton : MonoBehaviour
    {
        public string SceneName = "";

        public void LoadTargetScene()
        {
            Debug.Log($"'On Click ()' 이벤트로 LoadTargetScene() 호출됨!");
            SceneManager.LoadScene(SceneName);
        }
    }
}