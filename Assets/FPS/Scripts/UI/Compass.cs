using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class Compass : MonoBehaviour
    {
        public RectTransform CompasRect;
        public float VisibilityAngle = 180f;
        public float HeightDifferenceMultiplier = 2f;
        public float MinScale = 0.5f;
        public float DistanceMinScale = 50f;
        public float CompasMarginRatio = 0.8f;

        public GameObject MarkerDirectionPrefab;

        [Header("XR Setup")]
        [Tooltip("플레이어의 시점을 나타내는 카메라의 Transform을 연결하세요. (XR Origin > Camera Offset > Main Camera)")]
        [SerializeField]
        private Transform m_PlayerCameraTransform;
        // ---------------

        // m_PlayerTransform 변수 제거 (m_PlayerCameraTransform으로 대체됨)
        // Transform m_PlayerTransform; 

        Dictionary<Transform, CompassMarker> m_ElementsDictionnary = new Dictionary<Transform, CompassMarker>();

        float m_WidthMultiplier;
        float m_HeightOffset;

        void Awake()
        {
            if (m_PlayerCameraTransform == null)
            {
                Debug.LogError("Compass 스크립트에 'Player Camera Transform'이 할당되지 않았습니다!", this);
                
                // 만약 할당되지 않았다면, 임시방편으로 MainCamera를 찾아봅니다.
                // 하지만 인스펙터에서 직접 할당하는 것이 가장 좋습니다.
                Camera mainCam = Camera.main;
                if(mainCam != null)
                {
                    m_PlayerCameraTransform = mainCam.transform;
                    Debug.LogWarning("Player Camera Transform을 자동으로 'MainCamera'로 설정했습니다.", this);
                }
                else
                {
                    // 카메라를 찾지 못하면 스크립트를 비활성화하여 오류를 방지합니다.
                    enabled = false;
                    return;
                }
            }
            // ---------------

            m_WidthMultiplier = CompasRect.rect.width / VisibilityAngle;
            m_HeightOffset = -CompasRect.rect.height / 2;
        }

        void Update()
        {
            if (m_PlayerCameraTransform == null) return; // 카메라가 없으면 실행 중지

            // this is all very WIP, and needs to be reworked
            foreach (var element in m_ElementsDictionnary)
            {
                float distanceRatio = 1;
                float heightDifference = 0;
                float angle;

                if (element.Value.IsDirection)
                {
                    // --- CHANGED ---
                    angle = Vector3.SignedAngle(m_PlayerCameraTransform.forward, // m_PlayerTransform -> m_PlayerCameraTransform
                        element.Key.transform.localPosition.normalized, Vector3.up);
                    // ---------------
                }
                else
                {
                    // --- CHANGED ---
                    Vector3 targetDir = (element.Key.transform.position - m_PlayerCameraTransform.position).normalized; // m_PlayerTransform -> m_PlayerCameraTransform
                    targetDir = Vector3.ProjectOnPlane(targetDir, Vector3.up);
                    Vector3 playerForward = Vector3.ProjectOnPlane(m_PlayerCameraTransform.forward, Vector3.up); // m_PlayerTransform -> m_PlayerCameraTransform
                    angle = Vector3.SignedAngle(playerForward, targetDir, Vector3.up);

                    Vector3 directionVector = element.Key.transform.position - m_PlayerCameraTransform.position; // m_PlayerTransform -> m_PlayerCameraTransform
                    // ---------------

                    heightDifference = (directionVector.y) * HeightDifferenceMultiplier;
                    heightDifference = Mathf.Clamp(heightDifference, -CompasRect.rect.height / 2 * CompasMarginRatio,
                        CompasRect.rect.height / 2 * CompasMarginRatio);

                    distanceRatio = directionVector.magnitude / DistanceMinScale;
                    distanceRatio = Mathf.Clamp01(distanceRatio);
                }

                if (angle > -VisibilityAngle / 2 && angle < VisibilityAngle / 2)
                {
                    element.Value.CanvasGroup.alpha = 1;
                    element.Value.CanvasGroup.transform.localPosition = new Vector2(m_WidthMultiplier * angle,
                        heightDifference + m_HeightOffset);
                    element.Value.CanvasGroup.transform.localScale =
                        Vector3.one * Mathf.Lerp(1, MinScale, distanceRatio);
                }
                else
                {
                    element.Value.CanvasGroup.alpha = 0;
                }
            }
        }

        public void RegisterCompassElement(Transform element, CompassMarker marker)
        {
            marker.transform.SetParent(CompasRect);

            m_ElementsDictionnary.Add(element, marker);
        }

        public void UnregisterCompassElement(Transform element)
        {
            if (m_ElementsDictionnary.TryGetValue(element, out CompassMarker marker) && marker.CanvasGroup != null)
                Destroy(marker.CanvasGroup.gameObject);
            m_ElementsDictionnary.Remove(element);
        }
    }
}