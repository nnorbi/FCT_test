using UnityEngine;

public class UISounds : MonoBehaviour
{
	public AudioSource Source;

	public AudioClip ClickSound;

	public AudioClip HoverSound;

	public AudioClip MenuStateTransition;

	public AudioClip LayerSwitch;

	public AudioClip ToolbarTabSwitch;

	public AudioClip ToolbarSelectSlot;

	public AudioClip ToolbarClearSlot;

	public AudioClip PlaceBuilding;

	public AudioClip DeleteBuilding;

	public AudioClip RotateBuilding;

	public AudioClip StartPlacement;

	public AudioClip ContinuePlacement;

	public AudioClip UnlockResearch;

	public AudioClip ResearchAvailable;

	public AudioClip TutorialProgress;

	public AudioClip Error;

	protected float BaseVolume => Globals.Settings.General.SFXVolume;

	protected void PlaySFX(AudioClip clip, float volumeScale = 0.5f)
	{
		Source.PlayOneShot(clip, BaseVolume * volumeScale);
	}

	public void PlayClick()
	{
		PlaySFX(ClickSound, 0.4f);
	}

	public void PlayHover()
	{
		PlaySFX(HoverSound, 0.02f);
	}

	public void PlayMenuStateTransition()
	{
		PlaySFX(MenuStateTransition, 0.08f);
	}

	public void PlayError()
	{
		PlaySFX(Error, 0.2f);
	}

	public void PlayLayerSwitch()
	{
		PlaySFX(LayerSwitch, 0.15f);
	}

	public void PlayToolbarTabSwitch()
	{
		PlaySFX(ToolbarTabSwitch, 0.2f);
	}

	public void PlayToolbarSelectSlot()
	{
		PlaySFX(ToolbarSelectSlot, 0.25f);
	}

	public void PlayToolbarClearSlot()
	{
		PlaySFX(ToolbarClearSlot, 0.1f);
	}

	public void PlayPlaceBuilding()
	{
		PlaySFX(PlaceBuilding, 0.25f);
	}

	public void PlayDeleteBuilding()
	{
		PlaySFX(DeleteBuilding, 0.22f);
	}

	public void PlayRotateBuilding()
	{
		PlaySFX(RotateBuilding, 0.22f);
	}

	public void PlayStartPlacement()
	{
		PlaySFX(StartPlacement, 0.22f);
	}

	public void PlayTutorialProgress()
	{
		PlaySFX(TutorialProgress, 0.12f);
	}

	public void PlayContinuePlacement()
	{
		PlaySFX(StartPlacement, 0.22f);
	}

	public void PlayUnlockResearch()
	{
		PlaySFX(UnlockResearch, 0.82f);
	}

	public void PlayResearchAvailable()
	{
		PlaySFX(ResearchAvailable, 0.82f);
	}
}
