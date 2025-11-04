// Scripts/Fireball.cs
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 10f; // 파이어볼의 이동 속도
    public float lifeTime = 5f; // 파이어볼이 사라지기까지의 시간
    public int damageAmount = 10; // 파이어볼의 데미지

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false; // 중력 영향 받지 않음
            rb.isKinematic = true; // 일단 키네마틱으로 설정 (충돌 감지는 트리거로)
        }
    }

    void Start()
    {
        // 일정 시간 후 파이어볼을 파괴
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        // 앞으로 계속 이동
        rb.MovePosition(transform.position + transform.forward * speed * Time.fixedDeltaTime);
    }

    // 다른 오브젝트와 충돌했을 때 (트리거가 아님, 물리적 충돌)
    void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject);
    }

    // 다른 오브젝트와 충돌했을 때 (트리거로 설정된 콜라이더)
    void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject);
    }

    private void HandleHit(GameObject hitObject)
    {
        Debug.Log($"Fireball hit: {hitObject.name}");

        // StoneGolem에 맞았을 때 처리 (다음 단계에서 구현)
        StumpHealth golemHealth = hitObject.GetComponent<StumpHealth>();
        if (golemHealth != null)
        {
            golemHealth.TakeDamage(damageAmount); // <- 이 부분이 데미지를 줍니다.
            Destroy(gameObject); // StoneGolem에 맞으면 파이어볼도 사라짐
        }
        else
        {
            // StoneGolem이 아닌 다른 오브젝트에 맞으면 일단 사라지게 (선택 사항)
            // 지면이나 벽에 부딪혔을 때도 사라지도록 하려면 이 부분을 활성화
             Destroy(gameObject);
        }
    }
}