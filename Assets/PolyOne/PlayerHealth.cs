// Scripts/PlayerHealth.cs (XR Origin에 부착)

using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // --- 설정 변수 ---
    [Header("플레이어 체력 설정")]
    public int maxHealth = 100;
    private int currentHealth;

    // --- 내부 변수 ---
    private bool isInvulnerable = false; // 피격 무적 상태 플래그

    [Header("사운드 설정")]
    public AudioSource audioSource; // AudioSource 컴포넌트 레퍼런스
    public AudioClip deathSound;    // 사망 시 재생할 AudioClip (death.mp3)

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log("Player Health Initialized: " + currentHealth);
    }

    /// <summary>
    /// 외부에서 데미지를 받을 때 호출되는 공개 메서드입니다.
    /// </summary>
    /// <param name="damage">입을 데미지 양</param>
    /// <param name="invulnerabilityDuration">데미지 후 무적 시간 (딜레이)</param>
    public void TakeDamage(int damage, float invulnerabilityDuration)
    {
        // 1. 무적 상태 확인
        if (isInvulnerable)
        {
            return; // 무적 상태라면 데미지를 입지 않습니다.
        }

        // 2. 데미지 적용
        currentHealth -= damage;
        Debug.Log("Player took " + damage + " damage. Current Health: " + currentHealth);

        // 3. 사망 처리
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
            return;
        }

        // 4. 무적 상태 시작 (딜레이 시작)
        isInvulnerable = true;
        
        // 지정된 시간 후 무적 상태 해제
        Invoke(nameof(EndInvulnerability), invulnerabilityDuration);
    }

    private void EndInvulnerability()
    {
        isInvulnerable = false;
        Debug.Log("Player invulnerability ended.");
    }

    void Die()
    {
        Debug.Log("Player has died. Game Over!");
        // 1. 사망 사운드 재생 (추가된 부분)
        if (audioSource != null && deathSound != null)
        {
            // PlayOneShot을 사용하여 씬이 전환되더라도 소리가 재생되도록 합니다.
            audioSource.PlayOneShot(deathSound); 
        }
        else
        {
            // 디버깅 경고
            Debug.LogWarning(gameObject.name + ": Death sound or AudioSource is missing.");
        }

        // 2. SceneLoader 인스턴스를 찾아 재시작합니다.
        SceneLoader loader = FindObjectOfType<SceneLoader>();
        
        if (loader != null)
        {
            // 사운드 재생 후 BasicScene 재시작
            loader.RestartGame(); 
        }
        else
        {
            Debug.LogError("SceneLoader component not found in the scene! Cannot restart game.");
        }

    }

    // 디버깅 및 테스트를 위해 현재 체력을 반환하는 메서드
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}