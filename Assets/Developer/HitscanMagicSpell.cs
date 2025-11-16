using UnityEngine;
using Unity.FPS.Game; // Health 스크립트 네임스페이스

// [CreateAssetMenu] 속성: Assets > Create > Magic > Hitscan Spell 메뉴를 만듦
[CreateAssetMenu(fileName = "NewHitscanSpell", menuName = "Magic/Hitscan Spell")]
public class HitscanMagicSpell : BaseMagicSpell
{
    [Header("히트스캔 설정")]
    [SerializeField] private float magicDamage = 10f;
    [SerializeField] private float magicRange = 100f;
    [SerializeField] private LayerMask magicHitLayers; // "Enemy", "Environment" 레이어

    [Header("VFX")]
    [SerializeField] private GameObject shotEffect; // 발사 효과 (총구 화염)
    [SerializeField] private GameObject impactEffect; // 피격 효과 (스파크)

    // BaseMagicSpell의 추상 메서드 구현
    public override void CastSpell(Transform spawnPoint)
    {
        Debug.Log($"마법 발동: {RuneName}! (히트스캔)");

        Debug.DrawRay(spawnPoint.position, spawnPoint.forward * magicRange, Color.red, 10.0f);
        // 1. 발사 효과 생성 (컨트롤러 위치)
        if (shotEffect != null)
        {
            Instantiate(shotEffect, spawnPoint.position, spawnPoint.rotation);
            Debug.Log($"a");
        }
        Debug.Log($"b");

        // 2. 레이캐스트 발사
        RaycastHit hit;
        if (Physics.Raycast(spawnPoint.position, spawnPoint.forward, out hit, magicRange, magicHitLayers))
        {
            // 3. 피격 효과 생성 (맞은 위치)
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Debug.Log($"c");
            }

            // [!!] 4. 데미지 처리 (GetComponent -> GetComponentInParent)
            Health botHealth = hit.collider.GetComponentInParent<Health>();
            if (botHealth != null)
            {
                // Health.cs의 TakeDamage 메서드 호출
                Debug.Log($"{hit.collider.name} (부모: {botHealth.name})에게 데미지 {magicDamage} 적용!");
                botHealth.TakeDamage(magicDamage, spawnPoint.gameObject);
                Debug.Log($"d");
            }
            Debug.Log($"e");
        }
        else 
        {
            Debug.Log("마법이 아무것도 맞추지 못했습니다.");
        }
        Debug.Log($"f");
    }
}