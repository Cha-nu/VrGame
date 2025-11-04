// Scripts/StumpMovement.cs

using UnityEngine;
using UnityEngine.AI; 
using System.Collections; 

[RequireComponent(typeof(NavMeshAgent))] 
public class StumpMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    
    // --- 추적 및 이동 설정 ---
    public Transform playerTarget; 
    public float movementSpeed = 1.0f;          // NavMeshAgent의 기본 속도
    public float walkAnimationSpeed = 1.0f;     // 애니메이션 속도와 Agent 속도의 배율
    public float stoppingDistance = 0.5f; 
    public float updateRate = 0.5f;             // 타겟 위치 업데이트 주기
    public string movementFloatParameter = "Speed"; // Animator의 Speed 파라미터 이름

    // --- 넉백 설정 ---
    public float knockbackForce = 5f; 
    public float knockbackDuration = 0.5f; 

    // --- 내부 변수 ---
    private Rigidbody playerRB;
    private bool isMovementPaused = false;      // 넉백/피격으로 인해 이동이 일시 중지되었는지 여부

    [Header("플레이어 공격 설정")]
    public int contactDamage = 10;           // 접촉 시 입힐 데미지 (10)
    public float damageDelay = 2.0f;         // 데미지 후 딜레이 (무적 시간) (2초)

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Agent 속도는 기본 속도와 애니메이션 배율의 곱으로 설정
        agent.speed = movementSpeed * walkAnimationSpeed; 
        agent.stoppingDistance = stoppingDistance;

        // 플레이어 타겟 자동 찾기 (메인 카메라 사용)
        if (playerTarget == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                playerTarget = mainCam.transform; 
            }
        }

        if (playerTarget != null)
        {
            // UpdateDestination 코루틴 시작
            InvokeRepeating(nameof(UpdateDestination), 0f, updateRate);
        }
        else
        {
            Debug.LogError("Player Target is not assigned or found! Stump will not move.");
        }
    }

    void Update()
    {
        // NavMeshAgent 안정성 검사 (유효하지 않으면 움직임/애니메이션 중단)
        if (agent == null || !agent.isOnNavMesh || !agent.isActiveAndEnabled) 
        {
             if (animator != null) animator.SetFloat(movementFloatParameter, 0f);
             return;
        }

        // 1. 애니메이션 제어
        if (animator != null)
        {
            float currentSpeed = agent.velocity.magnitude;
            
            // 경로에 도착했거나, 경로 탐색 중이면 속도를 0으로 설정하여 애니메이션 중단
            if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                 currentSpeed = 0f;
            }

            // NavMeshAgent의 현재 속도를 애니메이터에 전달
            animator.SetFloat(movementFloatParameter, currentSpeed);
        }
    }

    private void UpdateDestination()
    {
        // 이동 중지 상태이거나 타겟이 없으면 건너뜀
        if (playerTarget == null || isMovementPaused) return; 

        if (agent != null && agent.isOnNavMesh)
        {
            Vector3 targetPosition = playerTarget.position;

            // Y축 고정: 타겟의 Y축을 Stump의 현재 높이로 보정하여 수평 이동만 하도록 강제
            targetPosition.y = transform.position.y; 
            
            agent.SetDestination(targetPosition);
        }
    }
    
// -----------------------------------------------------------------------------
// 피격/넉백 제어 로직
    
   
    public void PauseMovementForHit(float duration)
    {
        // 이미 넉백이나 일시 중단 상태면 중복 실행 방지
        if (isMovementPaused || agent == null || !agent.isActiveAndEnabled) return; 

        // 이동 중단 및 플래그 설정
        CancelInvoke(nameof(UpdateDestination)); // InvokeRepeating 중단
        agent.isStopped = true;
        isMovementPaused = true; 

        // 지정된 시간 후 이동 재개 코루틴 시작
        StartCoroutine(ResumeMovementAfterDelay(duration));
    }

    // --- 넉백 충돌 감지 ---
    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Ignore Raycast"))
        {
            // 1. PlayerHealth 컴포넌트 찾기 (XR Rig에 부착되어 있음)
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                // 2. 데미지 적용 및 딜레이(무적 시간) 시작
                // isMovementPaused 플래그는 넉백 처리용이므로, 데미지 로직은 별개로 처리합니다.
                
                // PlayerHealth 스크립트의 TakeDamage 메서드 호출
                // contactDamage(10)와 damageDelay(2.0초)를 전달합니다.
                playerHealth.TakeDamage(contactDamage, damageDelay);
            }
            
            // 3. (선택적) 넉백 적용 로직 (기존 로직 유지)
            if (!isMovementPaused)
            {
                // 넉백을 위해 Rigidbody를 다른 컴포넌트(예: CharacterController)에서 가져와야 할 수 있습니다.
                // 여기서는 충돌한 오브젝트에서 Rigidbody를 찾아 넉백을 시도합니다.
                playerRB = other.GetComponent<Rigidbody>();

                if (playerRB != null)
                {
                    ApplyKnockback();
                }
            }
        }
    }

    private void ApplyKnockback()
    {
        // 피격/넉백 플래그 설정 및 이동 중단
        isMovementPaused = true; 
        agent.isStopped = true; 
        CancelInvoke(nameof(UpdateDestination)); // InvokeRepeating 중단

        Vector3 knockbackDirection = (playerRB.transform.position - transform.position).normalized;
        knockbackDirection.y = 0; // 수평 넉백

        playerRB.isKinematic = false; 
        
        playerRB.linearVelocity = Vector3.zero; 
        
        playerRB.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse); 

        StartCoroutine(ResetKinematicAfterDelay(playerRB, knockbackDuration));
    }

    IEnumerator ResetKinematicAfterDelay(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; 
        }
        
        // 이동 재개 (오브젝트가 활성 상태일 때만)
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
            // InvokeRepeating 재개
            InvokeRepeating(nameof(UpdateDestination), 0f, updateRate);
        }
        
        isMovementPaused = false; 
    }
    
    IEnumerator ResumeMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 이동 재개 (오브젝트가 활성 상태일 때만)
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
            // InvokeRepeating 재개
            InvokeRepeating(nameof(UpdateDestination), 0f, updateRate);
        }
        
        isMovementPaused = false;
    }
}