using UnityEngine;
using Unity.FPS.Game;

public class MagicProjectile : MonoBehaviour
{
    [Header("마법 설정")]
    [SerializeField] private float damageAmount = 5f;
    
    [Header("VFX")]
    [Tooltip("명중 시 생성될 피격 효과 (VFX_Arcanic_Burst)")]
    [SerializeField] private GameObject impactEffect; // [!!] 피격 효과 변수 추가

    [Header("충돌 레이어 설정")]
    [Tooltip("이 레이어에 닿으면 데미지를 줍니다.")]
    [SerializeField] private LayerMask enemyLayer;

    [Tooltip("이 레이어에 닿으면 데미지 없이 소멸합니다.")]
    [SerializeField] private LayerMask environmentLayer;

    private void OnTriggerEnter(Collider other)
    {
        int otherLayer = 1 << other.gameObject.layer;

        // [!!] 1. "Enemy" 레이어인지 확인
        if ((enemyLayer.value & otherLayer) > 0)
        {
            Health botHealth = other.GetComponentInParent<Health>(); // [!!] GetComponentInParent 사용
            if (botHealth != null)
            {
                Debug.Log($"{other.name}에게 데미지 {damageAmount} 적용!");
                botHealth.TakeDamage(damageAmount, gameObject);
            }
            
            // [!!] 피격 효과 생성
            SpawnImpactEffect();
            // 봇에 명중했으므로 이 VFX는 즉시 파괴
            Destroy(gameObject);
        }
        // [!!] 2. "Environment" (벽, 바닥) 레이어인지 확인
        else if ((environmentLayer.value & otherLayer) > 0)
        {
            // [!!] 피격 효과 생성
            SpawnImpactEffect();
            // 데미지를 주지 않고, 마법(VFX)만 즉시 파괴
            Destroy(gameObject);
        }
    }
    
    // [!!] 3. 피격 효과를 생성하는 함수
    private void SpawnImpactEffect()
    {
        if (impactEffect != null)
        {
            // 부딪힌 위치에 피격 효과 생성
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }
    }
}