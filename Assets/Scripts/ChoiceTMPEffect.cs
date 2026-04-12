using TMPro;
using UnityEngine;

public class ChoiceTMPEffect : MonoBehaviour
{
    public float WobbleSpeed = 10f;
    public float WobbleIntensity = 4f;
    private TMP_Text textMesh;
    private Mesh mesh;
    private Vector3[] vertices;

    Vector2 Wobble(float time)
    {
        return new Vector2(Mathf.Sin(time * WobbleSpeed) * WobbleIntensity, Mathf.Cos(time * (WobbleSpeed * 0.8f)) * WobbleIntensity);
    }

    // =============== Lifecycle Methods ===============
    void Start()
    {
        textMesh = GetComponent<TMP_Text>();
    }

    void Update()
    {
        textMesh.ForceMeshUpdate(); // 매 프레임 메쉬 업데이트
        TMP_TextInfo textInfo = textMesh.textInfo; // 텍스트 상세 정보 가져오기
        mesh = textMesh.mesh;
        vertices = mesh.vertices;

        // 전체 정점을 돌지 않고, '글자' 단위로 루프를 돕니다.
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // 1. 핵심: 보이지 않는 문자(공백, 줄바꿈 등)는 계산에서 제외합니다.
            // 이 부분이 빠지면 화면 중앙에 흰 점이 생길 수 있습니다.
            if (!charInfo.isVisible) continue;

            // 2. 해당 글자의 시작 정점 인덱스 확인
            int vertexIndex = charInfo.vertexIndex;

            // 3. 진동 값 계산
            Vector3 offset = Wobble(Time.time + i);

            // 4. 한 글자를 이루는 4개의 정점(사각형)에 모두 오프셋 적용
            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        mesh.vertices = vertices;
        textMesh.canvasRenderer.SetMesh(mesh);
    }
}
