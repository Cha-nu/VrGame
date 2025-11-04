// Scripts/StumpManager.cs

using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 필요합니다.
using System.Collections;

public class StumpManager : MonoBehaviour
{
    // C++ 개발의 싱글톤 패턴처럼 인스턴스에 쉽게 접근하도록 설정합니다.
    public static StumpManager Instance { get; private set; }

    [Header("게임 클리어 설정")]
    public int totalStumpCount = 10; // 총 스텀프 개체 수 (Inspector에서 11로 설정)
    private int defeatedStumpCount = 0;

    [Header("클리어 사운드 설정")]
    public AudioSource audioSource; // 소리를 재생할 AudioSource 컴포넌트 레퍼런스
    public AudioClip clearSound;    // 클리어 시 재생할 사운드 클립 (clear.mp3)


    // SceneLoader에 대한 레퍼런스 (씬 재시작을 위해 필요)
    private SceneLoader sceneLoader;

    void Awake()
    {
        // 싱글톤 패턴 적용: 단 하나의 인스턴스만 존재하도록 보장합니다.
        if (Instance == null)
        {
            Instance = this;
            // 씬 전환 시에도 유지되도록 DontDestroyOnLoad를 사용할 수도 있지만,
            // 여기서는 매번 씬 로드 시 새로 초기화된다고 가정합니다.
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // SceneLoader를 찾아 레퍼런스를 할당합니다.
        sceneLoader = FindObjectOfType<SceneLoader>();
        if (sceneLoader == null)
        {
            Debug.LogError("SceneLoader component not found! Cannot restart scene after clear.");
        }

        // 씬 로드 시 모든 스텀프를 자동으로 찾을 수도 있지만,
        // 여기서는 Inspector에서 totalStumpCount를 수동으로 설정하는 방식을 사용합니다.
    }

    /// <summary>
    /// 스텀프가 파괴될 때마다 외부(StumpHealth 등)에서 호출되는 함수입니다.
    /// </summary>
    public void RegisterDefeat()
    {
        defeatedStumpCount++;
        Debug.Log($"Stump Defeated! Total: {defeatedStumpCount} / {totalStumpCount}");

        // 클리어 조건 확인
        if (defeatedStumpCount >= totalStumpCount)
        {
            GameClear();
        }
    }

    private void GameClear()
    {
        Debug.Log("🎉 All Stumps Defeated! Game Clear!");

        if (audioSource != null && clearSound != null)
        {
            // PlayOneShot을 사용하여 씬 전환 시 소리가 끊기지 않도록 합니다.
            audioSource.PlayOneShot(clearSound);
            StartCoroutine(ClearAndRestartAfterDelay(2.0f));
        }
        else
        {
            Debug.LogWarning(gameObject.name + ": Clear sound or AudioSource is missing.");
            StartCoroutine(ClearAndRestartAfterDelay(0f));
        }
    }

    /// <summary>
    /// 클리어 사운드를 재생하고 지정된 시간만큼 기다린 후 씬을 재시작합니다.
    /// </summary>
    private IEnumerator ClearAndRestartAfterDelay(float delaySeconds)
    {
        // 1. 클리어 사운드 재생
        if (audioSource != null && clearSound != null)
        {
            // PlayOneShot을 사용하여 씬 전환 시 소리가 끊기지 않도록 합니다.
            audioSource.PlayOneShot(clearSound);

            // Debug.LogWarning(gameObject.name + ": Clear sound played and waiting."); 
        }
        else
        {
            Debug.LogWarning(gameObject.name + ": Clear sound or AudioSource is missing. Proceeding without delay.");
        }

        // 2. 지정된 시간(2초)만큼 대기
        // 사운드를 재생한 후 씬 전환 전에 플레이어가 인지할 수 있도록 잠시 기다립니다.
        yield return new WaitForSeconds(delaySeconds);

        // 3. 클리어 로직 (BasicScene 재시작)
        if (sceneLoader != null)
        {
            // 대기 시간이 끝난 후 BasicScene 재시작 함수 호출
            sceneLoader.RestartGame();
        }
        else
        {
            Debug.LogError("Cannot restart game: SceneLoader is missing.");
        }
    }
}