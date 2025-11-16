using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using PDollarGestureRecognizer;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
public class VRRuneDrawer : MonoBehaviour
{
    [Header("오브젝트 연결")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject drawingPlateObject;   // [!!] 'DrawingPlate' 게임 오브젝트
    [SerializeField] private Transform playerCameraTransform; // [!!] VR 카메라 (Main Camera)의 Transform
    [SerializeField] private HapticImpulsePlayer hapticPlayer;
    [SerializeField] private Transform magicSpawnPoint;

    [Header("마법책 (Spell Book)")]
    [Tooltip("여기에 사용할 모든 BaseMagicSpell 에셋을 연결합니다.")]
    [SerializeField] private List<BaseMagicSpell> spellBook;

    // [!!] 3. 룬 이름을 키로 사용하는 마법사전
    private Dictionary<string, BaseMagicSpell> m_MagicDictionary;

    [Header("입력 설정 (Input)")]
    [SerializeField] private InputActionReference drawAction;

    [Header("그리기 설정")]
    [SerializeField] private LayerMask drawingLayer;
    [SerializeField] private float minDrawDistance = 0.03f; // [!!] 3cm로 기본값 상향
    [SerializeField] private float maxDrawDistance = 50f;
    [SerializeField] private float plateSummonDistance = 1.0f; // [!!] 플레이트 소환 거리

    [Header("룬 문자 인식 (P-Dollar)")]
    [SerializeField] private List<Gesture> trainingSet = new List<Gesture>();
    [SerializeField] private float recognitionScoreThreshold = 0.8f;

    private bool isDrawing = false;
    private List<Vector3> strokePoints3D;
    private List<Point> strokePoints2D;

    void Start()
    {
        strokePoints3D = new List<Vector3>();
        strokePoints2D = new List<Point>();

        m_MagicDictionary = new Dictionary<string, BaseMagicSpell>();
        foreach (BaseMagicSpell spell in spellBook)
        {
            if (spell != null && !m_MagicDictionary.ContainsKey(spell.RuneName))
            {
                m_MagicDictionary.Add(spell.RuneName, spell);
            }
        }
        
        Debug.Log($"마법 사전 초기화 완료: {m_MagicDictionary.Count}개의 마법 등록됨");
        if (hapticPlayer == null)
        {
            Debug.LogError("Haptic Impulse Player가 'Inspector' 창에서 수동으로 연결되지 않았습니다!");
        }
        if (lineRenderer == null) Debug.LogError("Line Renderer가 연결되지 않았습니다!");
        if (drawingPlateObject == null) Debug.LogError("Drawing Plate Object가 연결되지 않았습니다!");
        if (playerCameraTransform == null) Debug.LogError("Player Camera Transform이 연결되지 않았습니다!");
        
        if (drawAction == null || drawAction.action == null)
            Debug.LogError("Draw Action이 연결되지 않았습니다!");
        else
            drawAction.action.Enable();

        // 훈련 세트 로드
        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
        foreach (TextAsset gestureXml in gesturesXml)
            trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));
        
        Debug.Log($"P-Dollar 훈련 세트 로드 완료: {trainingSet.Count}개");
        
        // [!!] 시작할 때 플레이트 숨기기
        drawingPlateObject.SetActive(false);
    }

    void Update()
    {
        if (drawAction == null || drawAction.action == null) return;

        bool isPressed = drawAction.action.IsPressed();
        bool wasPressedThisFrame = drawAction.action.WasPressedThisFrame();
        bool wasReleasedThisFrame = drawAction.action.WasReleasedThisFrame();

        // [!!] 1. 트리거 누른 순간: 플레이트 소환
        if (wasPressedThisFrame)
        {
            SummonPlate();
        }

        // [!!] 2. 트리거 누르는 중: 그리기
        if (isPressed && drawingPlateObject.activeSelf) // 플레이트가 활성화 되어있을 때만
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxDrawDistance, drawingLayer))
            {
                if (wasPressedThisFrame)
                {
                    StartDrawing(hit); // 그리기 시작
                }
                else
                {
                    Draw(hit); // 그리는 중
                }
            }
        }

        // [!!] 3. 트리거 뗀 순간: 인식 및 플레이트 숨기기
        if (wasReleasedThisFrame)
        {
            EndDrawing(); // 인식
            drawingPlateObject.SetActive(false); // 플레이트 숨기기
        }
    }

    void SummonPlate()
    {
        // 1. 카메라 0.7m 앞에 위치시킴
        drawingPlateObject.transform.position = playerCameraTransform.position + playerCameraTransform.forward * plateSummonDistance;
        
        // 2. 플레이트가 카메라(플레이어)와 같은 방향을 바라보게 함 (이러면 눕게 됨)
        drawingPlateObject.transform.rotation = playerCameraTransform.rotation;
        
        // [!!] 3. X축으로 90도 회전시켜 세로로 세우기
        // (Space.Self를 사용해야 플레이트의 로컬 X축 기준으로 회전합니다)
        drawingPlateObject.transform.Rotate(-90.0f, 0, 0, Space.Self); 
        
        drawingPlateObject.SetActive(true);
    }

    void StartDrawing(RaycastHit firstHit)
    {
        isDrawing = true;
        
        strokePoints3D.Clear();
        strokePoints2D.Clear();
        
        lineRenderer.positionCount = 0;
        
        Draw(firstHit); // 첫 번째 점 추가
    }

    void Draw(RaycastHit currentHit)
    {
        if (!isDrawing) return;

        Vector3 newPoint = currentHit.point;

        if (strokePoints3D.Count == 0 || Vector3.Distance(strokePoints3D[strokePoints3D.Count - 1], newPoint) > minDrawDistance)
        {
            strokePoints3D.Add(newPoint);
            lineRenderer.positionCount = strokePoints3D.Count;
            lineRenderer.SetPosition(strokePoints3D.Count - 1, newPoint);

            Vector2 uvPoint = currentHit.textureCoord;
            strokePoints2D.Add(new Point(uvPoint.x, uvPoint.y, 0));
        }
    }

    void EndDrawing()
    {
        if (!isDrawing) return;
        
        isDrawing = false;
        Debug.Log($"그리기 종료! 총 점 개수: {strokePoints2D.Count}");

        if (strokePoints2D.Count > 10)
        {
            Gesture candidate = new Gesture(strokePoints2D.ToArray());
            Result result = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());

            Debug.Log($"[인식 결과] 제스처: {result.GestureClass}, 점수: {result.Score}");

            CastMagic(result.GestureClass, result.Score);
        }
        else
        {
            Debug.Log("획이 너무 짧아 인식을 취소합니다.");
        }
        
        // [!!] 3. 인식 후 선 지우기
        lineRenderer.positionCount = 0;
    }

    void CastMagic(string gestureName, float score)
    {
        if (score < recognitionScoreThreshold)
        {
            Debug.Log($"인식 실패: 점수가 낮음 ({score} < {recognitionScoreThreshold})");
            return;
        }

        bool castSuccess = true;

        if (m_MagicDictionary.TryGetValue(gestureName, out BaseMagicSpell spellToCast))
        {
            // 마법 사전에 등록된 룬일 경우
            spellToCast.CastSpell(magicSpawnPoint);
            castSuccess = true;
        }
        else
        {
            // 룬은 인식했으나 (예: Triangle), 마법 사전에 등록되지 않은 경우
            Debug.Log($"알 수 없는 룬 문자입니다: {gestureName}");
            castSuccess = false;
        }

        if (castSuccess && hapticPlayer != null)
        {
            hapticPlayer.SendHapticImpulse(0.7f, 0.5f);
            Debug.Log("햅틱 발동!");
        }
    }
}