using UnityEngine;

public static class SkinnedMeshRendererExtension
{
    public static void SetBlendShapeWeightInterpolate(this SkinnedMeshRenderer renderer, int index, float value, float weight)
    {
        renderer.SetBlendShapeWeight(index, Mathf.Lerp(renderer.GetBlendShapeWeight(index), value, weight));
    }
}