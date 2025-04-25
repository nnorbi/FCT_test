using System;
using Core.Dependency;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class HUDVideo : HUDComponent
{
	[SerializeField]
	protected RawImage UIRawImage;

	[SerializeField]
	private VideoPlayer UIVideoPlayer;

	private RenderTexture TargetTexture;

	private UnityEvent OnPrepareCompletedOneShot = new UnityEvent();

	[Construct]
	private void Construct()
	{
		UIVideoPlayer.source = VideoSource.VideoClip;
		UIVideoPlayer.timeUpdateMode = VideoTimeUpdateMode.GameTime;
		UIVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
		UIVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
	}

	public void SetResolution(int2 resolution)
	{
		if (TargetTexture != null)
		{
			throw new InvalidOperationException("Resolution can't be changed after initialization");
		}
		TargetTexture = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.Default);
		TargetTexture.Create();
		UIVideoPlayer.targetTexture = TargetTexture;
		UIRawImage.texture = TargetTexture;
		UIVideoPlayer.prepareCompleted += delegate(VideoPlayer source)
		{
			UIRawImage.texture = TargetTexture;
			UIRawImage.enabled = true;
			source.Play();
			OnPrepareCompletedOneShot.InvokeAndClear();
		};
	}

	public void PrepareAndPlayVideo(VideoClip clip, bool loop, UnityAction onPrepared = null)
	{
		if (TargetTexture == null)
		{
			throw new Exception("Init() not called on video player");
		}
		OnPrepareCompletedOneShot.RemoveAllListeners();
		UIVideoPlayer.isLooping = loop;
		UIVideoPlayer.clip = clip;
		UIVideoPlayer.Prepare();
		UIRawImage.enabled = false;
		if (onPrepared != null)
		{
			OnPrepareCompletedOneShot.AddListener(onPrepared);
		}
	}

	public void DisplayImage(Texture texture)
	{
		OnPrepareCompletedOneShot.RemoveAllListeners();
		UIVideoPlayer.Stop();
		UIRawImage.texture = texture;
		UIVideoPlayer.clip = null;
	}

	protected override void OnDispose()
	{
		UnityEngine.Object.Destroy(UIVideoPlayer.targetTexture);
	}
}
