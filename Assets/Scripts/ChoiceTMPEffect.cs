using TMPro;
using UnityEngine;

public class ChoiceTMPEffect : MonoBehaviour
{
    [Header("Text Wobble Settings")]
    public float WobbleSpeed = 2f; 
    public float WobbleIntensity = 4f;
    public float MinIntensity = 0.2f; 

    private TMP_Text textMesh;
    private Mesh mesh;
    private Vector3[] vertices;

    // 개별 글자의 떨림 벡터 계산
    Vector2 Wobble(float currentIntensity)
    {
        return Random.insideUnitCircle * currentIntensity;
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

        // --- 1. 전체에 적용될 강도 파동(Intensity Cycle) 계산 ---
        // Sine파를 0~1 사이 값으로 변환하여 사용합니다.
        float rawSine = Mathf.Sin(Time.time * WobbleSpeed); // -1 ~ 1
        float intensityCycle = Mathf.Lerp(MinIntensity, 1f, (rawSine + 1f) / 2f);
        float dynamicIntensity = WobbleIntensity * intensityCycle;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;

            // --- 2. 오프셋 계산 ---
            Vector3 offset = Wobble(dynamicIntensity);
            

            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        mesh.vertices = vertices;
        textMesh.canvasRenderer.SetMesh(mesh);
    }
}