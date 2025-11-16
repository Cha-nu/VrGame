using UnityEngine;

// 모든 마법의 기본이 되는 추상 클래스
// ScriptableObject를 상속받아 에셋 파일로 만들 수 있게 함
public abstract class BaseMagicSpell : ScriptableObject
{
    [Header("기본 설정")]
    [Tooltip("P-Dollar가 인식할 룬 문자 이름 (예: Circle)")]
    public string RuneName; //

    // 모든 마법은 이 CastSpell 메서드를 반드시 구현해야 함
    // 'spawnPoint'는 마법이 발사될 위치(컨트롤러의 MagicSpawnPoint)
    public abstract void CastSpell(Transform spawnPoint);
}