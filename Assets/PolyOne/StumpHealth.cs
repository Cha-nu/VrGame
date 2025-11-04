// Scripts/StumpHealth.cs
using UnityEngine;
using UnityEngine.AI;

public class StumpHealth : MonoBehaviour
{
    public int maxHealth = 100; // 최대 체력
    private int currentHealth; // 현재 체력

    // 애니메이션 및 피격 효과 설정
    public Animator stumpAnimator; // Stump의 Animator 컴포넌트 (데미지 모션용)
    public string hitAnimationTrigger = "Hit"; // 데미지 모션 재생을 위한 Animator Trigger 파라미터 이름

    public Renderer stumpRenderer; // Stump의 메쉬 렌더러 (색상 변경용)
    public Color hitColor = Color.blue; // 파이어볼에 맞았을 때 변할 색상
    public float colorChangeDuration = 0.2f; // 색상 변경 애니메이션 지속 시간
    
    private Color originalColor;

    [Header("사운드 설정")]
    public AudioSource audioSource; // 사운드를 재생할 AudioSource 컴포넌트
    public AudioClip hitSound; // 피격 시 재생할 사운드 파일 (hit.mp3)
    public AudioClip dieSound; // 사망 시 재생할 사운드 파일 (die.mp3)
    

    [Header("드롭 설정")]
    public GameObject bombPrefab; // Inspector에 PP_Theme_03_Bomb_003을 할당
    [Range(0f, 1f)]
    public float dropChance = 0.5f; // 50% 확률로 드롭 (0.0에서 1.0 사이)

    void Start()
    {
        currentHealth = maxHealth;

        // 1. Animator 컴포넌트 찾기
        if (stumpAnimator == null)
        {
            stumpAnimator = GetComponent<Animator>();
        }
        if (stumpAnimator == null)
        {
            Debug.LogWarning(gameObject.name + ": Animator component not found. Damage animations will not play.");
        }

        // 2. Renderer 컴포넌트 찾기 및 색상 저장 (기존 로직 유지)
        if (stumpRenderer == null)
        {
            stumpRenderer = GetComponentInChildren<Renderer>();
        }

        if (stumpRenderer != null)
        {
            originalColor = stumpRenderer.material.color;
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (audioSource == null)
        {
            Debug.LogWarning(gameObject.name + ": AudioSource component not found. Sounds will not play.");
        }
    }

    /// <summary>
    /// 외부에서 데미지를 받을 때 호출되는 메서드.
    /// </summary>
    /// <param name="damage">입힐 데미지 양</param>
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage. Current Health: " + currentHealth);

        // 2. 피격 사운드 재생 (추가된 부분)
        if (audioSource != null && hitSound != null)
        {
            // PlayOneShot을 사용하여 현재 재생 중인 다른 소리에 영향을 주지 않고 재생합니다.
            audioSource.PlayOneShot(hitSound);
        }

        // 1. 애니메이션 재생
        if (stumpAnimator != null)
        {
            // Animator의 'Hit' Trigger를 발동시켜 데미지 모션 재생
            stumpAnimator.SetTrigger(hitAnimationTrigger);
        }

        
        // 3. 체력 확인 및 사망 처리
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 체력이 0 이하가 되었을 때 처리하는 메서드.
    /// </summary>
    void Die()
    {
        Debug.Log(gameObject.name + " has been defeated!");

        // 이동 스크립트 비활성화
        StumpMovement movement = GetComponent<StumpMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        // NavMeshAgent를 찾아 이동 즉시 중단
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true; // 이동을 멈춥니다.
            agent.velocity = Vector3.zero; // 혹시 모를 잔류 속도도 제거합니다.
        }

        if (audioSource != null && dieSound != null)
        {
            audioSource.PlayOneShot(dieSound);
        }
        
        // 사망 애니메이션이 있다면 재생
        if (stumpAnimator != null)
        {
            stumpAnimator.SetTrigger("Die");
        }

        // Stump 오브젝트와 자식 오브젝트에 있는 모든 콜라이더를 찾아서 비활성화
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        if (bombPrefab != null)
        {
            float randomValue = Random.value; // 0.0f와 1.0f 사이의 랜덤 값

            if (randomValue <= dropChance)
            {
                // Stump의 위치에 폭탄 프리팹 생성
                Instantiate(bombPrefab, transform.position, Quaternion.identity);
                Debug.Log("Bomb dropped at " + gameObject.name + "'s position!");
            }

            else
            {
                Debug.Log("No bomb dropped this time.");
            }
        }
        if (StumpManager.Instance != null)
        {
            StumpManager.Instance.RegisterDefeat();
        }
        else
        {
            Debug.LogWarning("StumpManager instance not found. Cannot register defeat.");
        }
        // 2초 후 오브젝트 파괴
        Destroy(gameObject, 2f); 
    }
}