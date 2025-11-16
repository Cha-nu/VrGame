using UnityEngine;

public class HudFollowCamera : MonoBehaviour
{
    [Tooltip("따라갈 VR 메인 카메라의 Transform")]
    [SerializeField] private Transform cameraToFollow;
    
    [Tooltip("카메라와 유지할 거리")]
    [SerializeField] private float distance = 1.0f;

    void Start()
    {
        if (cameraToFollow == null)
        {
            cameraToFollow = Camera.main.transform;
        }
    }

    // [!!] 모든 카메라 움직임이 끝난 후, 렌더링 직전에 실행됩니다.
    void LateUpdate()
    {
        if (cameraToFollow == null) return;
        
        // 1. 위치: 카메라의 1.3m 앞으로 HUD를 이동
        transform.position = cameraToFollow.position + cameraToFollow.forward * distance;
        
        // 2. 회전: HUD가 항상 플레이어를 바라보도록 함
        transform.rotation = cameraToFollow.rotation;
    }
}