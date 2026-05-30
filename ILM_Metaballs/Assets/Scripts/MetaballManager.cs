using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MetaballManager : MonoBehaviour
{
    public Transform[] metaballs;
    public float influenceRadius = 0.15f;

    private static readonly int PositionsId = Shader.PropertyToID("_MetaballPositions");
    private static readonly int CountId     = Shader.PropertyToID("_MetaballCount");

    void Update()
    {
        if (metaballs == null) return;

        // En Play usa Main Camera, en Editor usa la camara de la Scene view
        Camera cam = Camera.main;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null) cam = sceneView.camera;

            // Forzar refresco continuo de la Scene view
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }
#endif
        if (cam == null) return;

        Vector4[] positions = new Vector4[11];
        int count = Mathf.Min(metaballs.Length, 11);

        for (int i = 0; i < count; i++)
        {
            if (metaballs[i] == null) continue;

            Vector3 screenPos = cam.WorldToViewportPoint(metaballs[i].position);

            // Si la esfera está detrás de la cámara
            if (screenPos.z < 0) continue;

            // Escalar el radio segun la profundidad
            float tanHalfFov = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float perspRadius = influenceRadius / (screenPos.z * 2f * tanHalfFov);

            positions[i] = new Vector4(screenPos.x, screenPos.y, screenPos.z, perspRadius);
        }

        Shader.SetGlobalVectorArray(PositionsId, positions);
        Shader.SetGlobalInt(CountId, count);
    }
}