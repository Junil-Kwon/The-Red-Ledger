using TMPro;
using UnityEngine;

public class ChoiceTMPEffect : MonoBehaviour
{
    [Header("Tremble Settings")]
    [Tooltip("숫자가 클수록 파르르 빠르게 떪 (추천: 30 ~ 50)")]
    public float TrembleSpeed = 40f;      

    [Tooltip("글자가 흔들리는 최대 반경 (추천: 1.5 ~ 3)")]
    public float TrembleIntensity = 2f;  

    private TMP_Text textMesh;
    private Mesh mesh;
    private Vector3[] vertices;

    // 연속적이면서도 무작위적인 진동 오프셋 계산
    Vector2 GetTrembleOffset(float timeValue, int characterIndex)
    {
        // Perlin 노이즈를 활용해 흐르듯 끊기지 않는 진동을 만듭니다.
        // characterIndex를 섞어주어 모든 글자가 제각각 따로 떨리게 합니다.
        float x = Mathf.PerlinNoise(timeValue + characterIndex * 0.7f, 0f) * 2f - 1f;
        float y = Mathf.PerlinNoise(0f, timeValue + characterIndex * 0.7f) * 2f - 1f;
        
        return new Vector2(x, y) * TrembleIntensity;
    }

    void Start()
    {
        textMesh = GetComponent<TMP_Text>();
    }

    void Update()
    {
        textMesh.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMesh.textInfo;
        mesh = textMesh.mesh;
        vertices = mesh.vertices;

        // 시간에 속도를 곱해 멈추지 않는 빠른 시간 축을 생성
        float timeValue = Time.time * TrembleSpeed;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // 공백이나 줄바꿈 문자는 패스
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;

            // 매 프레임 글자별 고유 오프셋을 일정 강도로 적용
            Vector3 offset = GetTrembleOffset(timeValue, i);
            
            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        mesh.vertices = vertices;
        textMesh.canvasRenderer.SetMesh(mesh);
    }
}