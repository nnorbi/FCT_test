using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUDEditWaypointDialog : HUDDialog
{
	public Button UIConfirmButton;

	public Button UIDeleteButton;

	public Button UIShapeKeyInfoButton;

	public TMP_InputField UINameInput;

	public TMP_InputField UIIconKeyInput;

	public RectTransform UIPredefinedShapesParent;

	public RectTransform UIShapePrerenderParent;

	public string[] UIPredefinedShapeCodes = new string[0];

	[HideInInspector]
	public UnityEvent<PlayerWaypointEditableData> OnWaypointEdited = new UnityEvent<PlayerWaypointEditableData>();

	[HideInInspector]
	public UnityEvent OnWaypointDeleted = new UnityEvent();

	protected string CurrentName;

	protected string CurrentIconKey;

	public void InitDialogContentsNewWaypoint(PlayerWaypointEditableData data)
	{
		SetTitle("waypoints.create-new.title".tr());
		InitFields(data);
		UIDeleteButton.gameObject.SetActive(value: false);
	}

	public void InitDialogContentsExistingWaypoint(PlayerWaypointEditableData data)
	{
		SetTitle("waypoints.edit.title".tr());
		InitFields(data);
		UIDeleteButton.gameObject.SetActive(value: true);
	}

	protected void InitFields(PlayerWaypointEditableData data)
	{
		CurrentName = data.Name;
		CurrentIconKey = data.ShapeIconKey;
		UINameInput.text = CurrentName;
		UIIconKeyInput.text = CurrentIconKey;
		UIIconKeyInput.onValueChanged.AddListener(delegate
		{
			UpdateShapePrerender();
		});
		UIPredefinedShapesParent.RemoveAllChildren();
		string[] uIPredefinedShapeCodes = UIPredefinedShapeCodes;
		foreach (string code in uIPredefinedShapeCodes)
		{
			ShapeDefinition definition;
			try
			{
				definition = new ShapeDefinition(code);
			}
			catch (Exception ex)
			{
				Debug.LogError("Invalid predefined shape code: '" + code + "' -> " + ex);
				continue;
			}
			GameObject obj = new GameObject("predefined-shape", typeof(RectTransform), typeof(Button))
			{
				layer = LayerMask.NameToLayer("UI")
			};
			RectTransform rect = obj.GetComponent<RectTransform>();
			rect.SetParent(UIPredefinedShapesParent, worldPositionStays: false);
			rect.localPosition = new Vector3(0f, 0f, 0f);
			rect.localScale = new Vector3(1f, 1f, 1f);
			rect.sizeDelta = new Vector2(50f, 50f);
			Button btn = obj.GetComponent<Button>();
			HUDBeltItemRenderer.RenderShapeRaw(definition, obj.transform, 50f);
			RawImage img = (RawImage)(btn.targetGraphic = rect.GetChild(0).GetComponent<RawImage>());
			img.raycastTarget = true;
			img.material = Globals.Resources.DefaultUISpriteMaterial;
			string savedCode = code;
			HUDTheme.PrepareTheme(btn, HUDTheme.ButtonColorsNormal).onClick.AddListener(delegate
			{
				UIIconKeyInput.text = savedCode;
			});
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(UIPredefinedShapesParent);
		UpdateShapePrerender();
	}

	protected void UpdateShapePrerender()
	{
		UIShapePrerenderParent.RemoveAllChildren();
		try
		{
			ShapeDefinition definition = new ShapeDefinition(UIIconKeyInput.text);
			HUDBeltItemRenderer.RenderShapeRaw(definition, UIShapePrerenderParent, 70f);
		}
		catch (Exception)
		{
		}
	}

	protected override void ResolveReferences()
	{
		base.ResolveReferences();
		HUDTheme.PrepareTheme(UIConfirmButton, HUDTheme.ButtonColorsActive).onClick.AddListener(HandleConfirm);
		HUDTheme.PrepareTheme(UIDeleteButton, HUDTheme.ButtonColorsDanger).onClick.AddListener(delegate
		{
			OnWaypointDeleted.Invoke();
			CloseRequested.Invoke();
		});
	}

	protected override void HandleConfirm()
	{
		string name = UINameInput.text.Trim();
		string iconKey = UIIconKeyInput.text.Trim();
		try
		{
			new ShapeDefinition(iconKey);
		}
		catch (Exception)
		{
			iconKey = "";
		}
		if (iconKey.Length == 0)
		{
			Globals.UISounds.PlayError();
			return;
		}
		OnWaypointEdited.Invoke(new PlayerWaypointEditableData
		{
			Name = UINameInput.text,
			ShapeIconKey = UIIconKeyInput.text
		});
		CloseRequested.Invoke();
	}
}
