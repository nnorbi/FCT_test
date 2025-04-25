using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
[VolumeComponentMenuForRenderPipeline("Custom/FogAndOutline", new Type[] { typeof(UniversalRenderPipeline) })]
public class FogAndOutline : VolumeComponent, IPostProcessComponent
{
	public BoolParameter enabled = new BoolParameter(value: false, overrideState: true);

	public FloatParameter outlineFactor = new FloatParameter(1f, overrideState: true);

	public bool IsActive()
	{
		return enabled.value;
	}

	public bool IsTileCompatible()
	{
		return true;
	}
}
