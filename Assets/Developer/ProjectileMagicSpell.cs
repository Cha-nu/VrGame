using UnityEngine;

// [CreateAssetMenu] 속성: Assets > Create > Magic > Projectile Spell 메뉴를 만듦
[CreateAssetMenu(fileName = "NewProjectileSpell", menuName = "Magic/Projectile Spell")]
public class ProjectileMagicSpell : BaseMagicSpell
{
    [Header("투사체 설정")]
    [Tooltip("발사될 프리팹 (반드시 Rigidbody와 MagicProjectile.cs를 가져야 함)")]
    [SerializeField] private GameObject projectilePrefab; // 'VFX_Arcanic_Shot_Main'을 여기 연결

    [Tooltip("투사체 발사 속도")]
    [SerializeField] private float launchSpeed = 20f;

    // BaseMagicSpell의 추상 메서드 구현
    public override void CastSpell(Transform spawnPoint)
    {
        Debug.Log($"마법 발동: {RuneName}! (투사체)");

        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab이 연결되지 않았습니다!");
            return;
        }

        // 1. 발사 효과 생성 (컨트롤러 위치)
        GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
        
        // 2. Rigidbody를 찾아 속도(velocity)를 설정
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = spawnPoint.forward * launchSpeed;
        }
        else
        {
            Debug.LogError("Projectile Prefab에 Rigidbody 컴포넌트가 없습니다!");
        }
    }
}