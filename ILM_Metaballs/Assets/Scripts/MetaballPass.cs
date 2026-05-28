using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

internal class MetaballPass : ScriptableRenderPass
{
    private Material _material;

    public MetaballPass(RenderPassEvent @event, Material material)
    {
        requiresIntermediateTexture = true;
        renderPassEvent = @event;
        _material = material;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        var colorTarget = resourceData.activeColorTexture;
        TextureHandle temporaryTarget = renderGraph.CreateTexture(renderGraph.GetTextureDesc(colorTarget));

        RenderGraphUtils.BlitMaterialParameters blitParams =
            new RenderGraphUtils.BlitMaterialParameters(colorTarget, temporaryTarget, _material, 0);

        renderGraph.AddBlitPass(blitParams, passName);
        resourceData.cameraColor = temporaryTarget;
    }
}