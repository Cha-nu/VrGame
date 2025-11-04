// Scripts/StaffFireballLauncher.cs
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // XR Interaction Toolkit 네임스페이스 추가

public class StaffFireballLauncher : MonoBehaviour
{
    public GameObject fireballPrefab; // Fire Effects Blue 프리팹을 할당할 슬롯
    public Transform firePoint; // 파이어볼이 생성될 위치 (지팡이 앞부분)
    public GameObject xrOriginObject; // XR Origin 오브젝트 (플레이어 본체)
    public float cooldownTime = 0.5f; // 발사 쿨타임

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable xrGrabInteractable;
    private float lastFireTime;

    [Header("파이어볼 사운드 설정")]
    public AudioSource audioSource; // 소리를 재생할 AudioSource 컴포넌트 레퍼런스
    public AudioClip skillSound;    // 발사 시 재생할 스킬 사운드 클립 (skill.mp3)

    void Awake()
    {
        xrGrabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (xrGrabInteractable == null)
        {
            Debug.LogError("StaffFireballLauncher requires an XRGrabInteractable component on the same GameObject.");
            enabled = false; // 컴포넌트가 없으면 이 스크립트를 비활성화
        }
    }

    void OnEnable()
    {
        // 인터랙터가 선택되었을 때 (즉, 지팡이를 잡았을 때) 이벤트 구독
        xrGrabInteractable.activated.AddListener(OnStaffActivated);
    }

    void OnDisable()
    {
        // 인터랙터 선택이 해제되었을 때 이벤트 구독 해제
        xrGrabInteractable.activated.RemoveListener(OnStaffActivated);
    }

    private void OnStaffActivated(ActivateEventArgs args)
    {
        // 트리거 버튼이 눌렸을 때 호출 (XRController의 Activate Input)
        // 쿨타임 체크
        if (Time.time >= lastFireTime + cooldownTime)
        {
            FireFireball();
            lastFireTime = Time.time;
        }
    }

    void FireFireball()
    {
        if (fireballPrefab == null || firePoint == null)
        {
            Debug.LogError("Fireball Prefab or Fire Point is not assigned!");
            return;
        }

        // 🔊 스킬 사운드 재생 로직 (추가된 부분)
        if (audioSource != null && skillSound != null)
        {
            // PlayOneShot을 사용하여 현재 재생 중인 다른 소리에 방해받지 않고 사운드를 재생합니다.
            audioSource.PlayOneShot(skillSound);
        }
        else
        {
            // 디버깅을 위해 사운드 설정 누락 시 경고를 출력합니다.
            if (audioSource == null) 
                Debug.LogWarning(gameObject.name + ": AudioSource is missing for firing sound.");
            if (skillSound == null) 
                Debug.LogWarning(gameObject.name + ": Skill Sound (AudioClip) is missing.");
        }

        // 지팡이 콜라이더 가져오기 (지팡이에 붙어 있는 모든 콜라이더)
        Collider[] staffColliders = GetComponentsInChildren<Collider>();

        // 파이어볼 인스턴스 생성
        GameObject fireballInstance = Instantiate(fireballPrefab, firePoint.position, firePoint.rotation);
        Collider fireballCollider = fireballInstance.GetComponent<Collider>();

        if (fireballCollider != null)
        {
            // 지팡이의 모든 콜라이더와 파이어볼 콜라이더 간의 충돌 무시
            foreach (Collider staffCol in staffColliders)
            {
                // 충돌 무시 설정: true를 넣으면 충돌을 무시합니다.
                Physics.IgnoreCollision(fireballCollider, staffCol, true);
            }
            if (xrOriginObject != null)
            {
                // XR Origin과 그 자식에 있는 모든 콜라이더를 가져옵니다.
                Collider[] playerColliders = xrOriginObject.GetComponentsInChildren<Collider>();
                
                foreach (Collider playerCol in playerColliders)
                {
                    // 충돌 무시 설정
                    Physics.IgnoreCollision(fireballCollider, playerCol, true);
                }
            }
            else
            {
                Debug.LogWarning("XR Origin Object is not assigned! Fireball may hit player immediately.");
            }
        }
    }
}