// Scripts/StumpSpawner.cs
using UnityEngine;
using UnityEngine.AI; // NavMesh.SamplePosition을 사용하기 위해 필요
using System.Collections;

public class StumpSpawner : MonoBehaviour
{
    // 인스펙터에 할당할 Stump 프리팹
    public GameObject stumpPrefab; 

    // 생성할 Stump의 총 개수
    public int numberOfStumpsToSpawn = 10; 

    // 플레이어(XR Origin/Camera)의 Transform
    public Transform playerTransform; 

    // Stump가 생성될 플레이어 주변의 최소/최대 거리
    public float minSpawnRadius = 5f; 
    public float maxSpawnRadius = 15f; 

    // 생성 위치를 찾지 못했을 때의 시도 횟수
    private const int MaxSpawnAttempts = 10; 

    void Start()
    {
        // 플레이어 트랜스폼이 할당되지 않았다면 메인 카메라에서 찾기
        if (playerTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                playerTransform = mainCam.transform;
            }
        }

        if (stumpPrefab == null)
        {
            Debug.LogError("Stump Prefab is not assigned in StumpSpawner!");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is not assigned or found!");
            return;
        }

        // 지정된 개수만큼 Stump 생성 시작
        StartCoroutine(SpawnStumps());
    }

    IEnumerator SpawnStumps()
    {
        for (int i = 0; i < numberOfStumpsToSpawn; i++)
        {
            Vector3 spawnPosition;
            if (TryFindRandomNavMeshPosition(out spawnPosition))
            {
                Instantiate(stumpPrefab, spawnPosition, Quaternion.identity);
                Debug.Log($"Spawned stump {i + 1}/{numberOfStumpsToSpawn}");
            }

            // 10초 대기 후 다음 생성
            yield return new WaitForSeconds(10f);
        }
    }


    /// <summary>
    /// 플레이어 주변의 NavMesh 위에 무작위 생성 위치를 찾습니다.
    /// </summary>
    /// <param name="resultPosition">찾은 유효한 위치</param>
    /// <returns>위치 찾기 성공 여부</returns>
    bool TryFindRandomNavMeshPosition(out Vector3 resultPosition)
    {
        for (int attempt = 0; attempt < MaxSpawnAttempts; attempt++)
        {
            // 1. 플레이어 주변의 무작위 위치 계산
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minSpawnRadius, maxSpawnRadius);

            Vector3 randomPos = playerTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            // 2. NavMesh.SamplePosition을 사용하여 이 위치 근처의 유효한 NavMesh 지점 찾기
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 5f, NavMesh.AllAreas))
            {
                // 유효한 위치를 찾았을 경우
                resultPosition = hit.position;
                return true;
            }
        }

        // 유효한 위치를 찾지 못했을 경우
        resultPosition = Vector3.zero;
        return false;
    }
}