using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class MetaballFeature : ScriptableRendererFeature
{
    [SerializeField]
    private RenderPassEvent @event = RenderPassEvent.AfterRenderingTransparents;

    [SerializeField]
    [HideInInspector]
    private Material _myMaterial = null;

    private MetaballPass pass;

    public override void Create()
    {
        if (_myMaterial == null)
            _myMaterial = CoreUtils.CreateEngineMaterial("Postprocess/MetaballEffect");

        pass = new MetaballPass(@event, _myMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        bool isMainCamera   = renderingData.cameraData.camera.tag == "MainCamera";
        bool isSceneCamera  = renderingData.cameraData.isSceneViewCamera;

        if (!isMainCamera && !isSceneCamera) return;

        Debug.Assert(_myMaterial != null, this);
        renderer.EnqueuePass(pass);
    }
}