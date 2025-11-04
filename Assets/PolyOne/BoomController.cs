// Scripts/BombController.cs

using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class BombController : MonoBehaviour
{
    [Header("폭발 설정")]
    public GameObject explosionEffectPrefab; 
    public int damageAmount = 20;               
    public float explosionRadius = 5.0f;        
    public float explosionDelay = 0.1f;         

    [Tooltip("Stump 오브젝트가 있는 Layer를 설정하세요.")]
    public LayerMask stumpLayer; 

    private bool hasExploded = false;
    private bool isGrabbed = true;             
    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable; 

    [Header("폭발 사운드 설정")]
    public AudioSource audioSource; // 소리를 재생할 AudioSource 컴포넌트
    public AudioClip boomSound;     // 폭발 시 재생할 사운드 클립 (boom.mp3)


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Grab Interactable 이벤트 등록
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
        else
        {
            Debug.LogError("XRGrabInteractable component is missing! Grab will not work.");
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        // 이 시점부터 폭탄이 물리적으로 던져지며 충돌 감지가 활성화됩니다.
    }

    void OnCollisionEnter(Collision collision)
    {
        // 아직 터지지 않았고, 플레이어가 던진 상태일 때 (잡고 있지 않을 때)
        if (!hasExploded && !isGrabbed && !collision.gameObject.CompareTag("Player"))
        {
            // 폭발 로직 시작
            StartCoroutine(DelayedExplosion());
        }
    }

    IEnumerator DelayedExplosion()
    {
        hasExploded = true; // 중복 폭발 방지
        
        // 이동 중지
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero;
        }

        yield return new WaitForSeconds(explosionDelay);

        Explode();
    }

    void Explode()
    {
        // 🔊 폭발 사운드 재생 로직
        if (audioSource != null && boomSound != null)
        {
            // PlayOneShot을 사용하여 한 번만 소리를 재생합니다.
            audioSource.PlayOneShot(boomSound); 
            Debug.LogError("boom sound played");
        }
        else
        {
            Debug.LogWarning(gameObject.name + ": Boom sound or AudioSource is missing for explosion.");
        }
        // 1. 시각 효과 재생
        if (explosionEffectPrefab != null)
        {
            GameObject explosionInstance = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosionInstance, 2.0f);
        }

        // 2. 주변 Stump 검색 및 데미지 적용
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, stumpLayer);

        foreach (Collider hitCollider in colliders)
        {
            StumpHealth health = hitCollider.GetComponent<StumpHealth>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
            }
        }

        // 3. 폭탄 오브젝트 파괴
        Destroy(gameObject, 2.0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}