using System;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
	protected enum State
	{
		FadingIn,
		Playing,
		FadingOut,
		Leaving
	}

	[Serializable]
	public class TrackReference
	{
		public AudioClip Track;

		[Range(0f, 1f)]
		public float Pace = 0.5f;

		[Range(0f, 3f)]
		public float VolumeAdjustment = 0.5f;
	}

	protected static float FADE_IN_TIME = 15f;

	protected static float FADE_OUT_TIME = 15f;

	protected static float TRACK_WAIT_TIME = 1f;

	public static float LEAVE_GAME_FADEOUT_DURATION = 3f;

	public TrackReference MenuTrack;

	public TrackReference[] Tracks;

	protected AudioSource BGM;

	protected State CurrentState;

	protected TrackReference CurrentTrack;

	protected Sequence CurrentSequence;

	protected float MusicVolume => Globals.Settings.General.MusicVolume.Value;

	public void OnGameInitialized()
	{
		BGM = GetComponent<AudioSource>();
		Globals.Settings.General.MusicVolume.Changed.AddListener(OnMusicVolumeChanged);
		OnMusicVolumeChanged();
		FadeInNewTrack(waitFirst: false);
	}

	protected void FadeInNewTrack(bool waitFirst)
	{
		if (Singleton<GameCore>.G.SavegameCoordinator.Headless)
		{
			CurrentTrack = MenuTrack;
		}
		else
		{
			CurrentTrack = Tracks.RandomChoice();
		}
		Debug.Log("MusicManager:: Fade in " + CurrentTrack.Track.name);
		BGM.volume = 0f;
		BGM.time = 0f;
		BGM.clip = CurrentTrack.Track;
		BGM.Play();
		CurrentState = State.FadingIn;
		if (CurrentSequence != null)
		{
			CurrentSequence.Kill();
		}
		CurrentSequence = DOTween.Sequence();
		if (waitFirst)
		{
			CurrentSequence.AppendInterval(TRACK_WAIT_TIME);
		}
		CurrentSequence.Append(BGM.DOFade(CurrentTrack.VolumeAdjustment, FADE_IN_TIME));
		CurrentSequence.OnComplete(delegate
		{
			Debug.Log("MusicManager:: Now playing " + CurrentTrack.Track.name);
			CurrentState = State.Playing;
		});
	}

	public void OnGameUpdate()
	{
		switch (CurrentState)
		{
		case State.FadingIn:
			break;
		case State.Playing:
		{
			float time = BGM.time;
			if (time > BGM.clip.length - FADE_OUT_TIME)
			{
				CurrentState = State.FadingOut;
				Debug.Log("MusicManager:: Fade out");
				BGM.DOFade(0f, FADE_OUT_TIME).OnComplete(delegate
				{
					BGM.Stop();
					Debug.Log("MusicManager:: Faded out, now playing again");
					FadeInNewTrack(waitFirst: true);
				});
			}
			break;
		}
		case State.FadingOut:
			break;
		case State.Leaving:
			break;
		}
	}

	public void OnGameCleanup()
	{
		if (BGM != null)
		{
			BGM.Stop();
			DOTween.Kill(BGM);
		}
		if (CurrentSequence != null)
		{
			CurrentSequence.Kill();
		}
		Globals.Settings.General.MusicVolume.Changed.RemoveListener(OnMusicVolumeChanged);
	}

	public void OnPrepareLeaveGame()
	{
		CurrentState = State.Leaving;
		DOTween.Kill(BGM);
		if (CurrentSequence != null)
		{
			CurrentSequence.Kill();
		}
		BGM.DOFade(0f, LEAVE_GAME_FADEOUT_DURATION);
	}

	protected void OnMusicVolumeChanged()
	{
		AudioMixer output = BGM.outputAudioMixerGroup.audioMixer;
		output.SetFloat("MusicVolume", math.min(0f, math.lerp(-80f, 1f, math.pow(Globals.Settings.General.MusicVolume, 0.25f))));
	}
}
