#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using TMPro;
using Unity.Core.View;
using UnityEngine;

public class HUDSettingRenderer : HUDComponent
{
	[SerializeField]
	private TMP_Text UISettingTitle;

	[SerializeField]
	private PrefabViewReference<HUDEnumSelectorControl> UIEnumSelectorControlPrefab;

	[SerializeField]
	private PrefabViewReference<HUDSliderControl> UISliderControlPrefab;

	[SerializeField]
	private PrefabViewReference<HUDToggleControl> UIToggleControlPrefab;

	[SerializeField]
	private RectTransform UIControlParent;

	private HUDComponent CurrentControl;

	private IHUDDialogStack DialogStack;

	private GameSetting _Setting;

	private Action SettingUpdater;

	public GameSetting Setting
	{
		set
		{
			if (_Setting != null)
			{
				throw new InvalidOperationException("Can not change setting afterwards.");
			}
			_Setting = value;
			RenderSetting();
		}
	}

	[Construct]
	private void Construct(IHUDDialogStack dialogStack)
	{
		DialogStack = dialogStack;
	}

	private void RenderSetting()
	{
		Debug.Assert(CurrentControl == null);
		Debug.Assert(SettingUpdater == null);
		UISettingTitle.text = _Setting.Title;
		_Setting.Changed.AddListener(OnSettingChanged);
		if (_Setting is FloatGameSetting floatSetting)
		{
			HandleFloatSetting(floatSetting);
		}
		else if (_Setting is DynamicEnumGameSetting<string> enumSettingString)
		{
			HandleDynamicEnumSetting(enumSettingString);
		}
		else if (_Setting is DynamicEnumGameSetting<int> enumSettingInt)
		{
			HandleDynamicEnumSetting(enumSettingInt);
		}
		else if (_Setting is DynamicEnumGameSetting<float> enumSettingFloat)
		{
			HandleDynamicEnumSetting(enumSettingFloat);
		}
		else if (_Setting is ResolutionGameSetting resolutionSetting)
		{
			HandleValueListSetting(resolutionSetting);
		}
		else if (_Setting is RefreshRateGameSetting refreshRateSetting)
		{
			HandleValueListSetting(refreshRateSetting);
		}
		else if (_Setting is IValueListGameSetting enumSetting)
		{
			HandleEnumSetting(enumSetting);
		}
		else if (_Setting is BoolGameSetting boolSetting)
		{
			HandleBoolSetting(boolSetting);
		}
		else
		{
			base.Logger.Warning?.Log("Unknown setting type: " + _Setting.GetType().Name);
		}
	}

	private void HandleFloatSetting(FloatGameSetting setting)
	{
		HUDSliderControl control = RequestChildView(UISliderControlPrefab).PlaceAt(UIControlParent);
		control.MinValue = setting.Min;
		control.MaxValue = setting.Max;
		control.CurrentValue = setting;
		control.Changed.AddListener(delegate(float value)
		{
			setting.SetValue(value);
			control.CurrentValue = value;
		});
		SettingUpdater = delegate
		{
			control.CurrentValue = setting;
		};
		CurrentControl = control;
	}

	private void HandleBoolSetting(BoolGameSetting setting)
	{
		HUDToggleControl control = RequestChildView(UIToggleControlPrefab).PlaceAt(UIControlParent);
		control.SetValueInstant(setting);
		control.ValueChangeRequested.AddListener(delegate(bool value)
		{
			setting.SetValue(!setting.Value);
			control.Value = value;
		});
		SettingUpdater = delegate
		{
			control.Value = setting.Value;
		};
		CurrentControl = control;
	}

	private void HandleDynamicEnumSetting<T>(DynamicEnumGameSetting<T> setting)
	{
		List<string> valueIds = (from entry in setting.GetAvailableEntries()
			select entry.ValueId).ToList();
		HUDEnumSelectorControl control = RequestChildView(UIEnumSelectorControlPrefab).PlaceAt(UIControlParent);
		control.Values = valueIds.Select(setting.GetValueText).ToArray();
		SettingUpdater = delegate
		{
			control.CurrentValueIndex = valueIds.IndexOf(setting.Current.ValueId);
		};
		SettingUpdater();
		control.ValueChangeRequested.AddListener(delegate(int index)
		{
			string value = valueIds[index];
			setting.SetValue(value);
			control.CurrentValueIndex = index;
		});
		CurrentControl = control;
	}

	private void HandleEnumSetting(IValueListGameSetting setting)
	{
		string[] valueIds = setting.AvailableValueIds;
		HUDEnumSelectorControl control = RequestChildView(UIEnumSelectorControlPrefab).PlaceAt(UIControlParent);
		control.Values = valueIds.Select(setting.FormatValueId).ToArray();
		SettingUpdater = delegate
		{
			control.CurrentValueIndex = setting.CurrentValueIndex;
		};
		SettingUpdater();
		control.ValueChangeRequested.AddListener(delegate(int index)
		{
			setting.CurrentValueIndex = index;
			control.CurrentValueIndex = index;
		});
		CurrentControl = control;
	}

	private void HandleValueListSetting<T>(ITypedValueListGameSetting<T> setting)
	{
		IList<T> values = setting.GenerateAvailableValues();
		HUDEnumSelectorControl control = RequestChildView(UIEnumSelectorControlPrefab).PlaceAt(UIControlParent);
		control.Values = values.Select(setting.FormatValue).ToArray();
		SettingUpdater = delegate
		{
			control.CurrentValueIndex = values.IndexOf(setting.Value);
		};
		SettingUpdater();
		control.ValueChangeRequested.AddListener(delegate(int index)
		{
			setting.SetValue(values[index]);
			control.CurrentValueIndex = index;
		});
		CurrentControl = control;
	}

	private void OnSettingChanged()
	{
		SettingUpdater?.Invoke();
		if (_Setting.RequiresRestart && _Setting.IsModified)
		{
			HUDDialogSimpleInfo dialog = DialogStack.ShowUIDialog<HUDDialogSimpleInfo>();
			dialog.InitDialogContents("menu.settings.general.pending-setting-change.title".tr(), "menu.settings.general.pending-setting-change.description".tr());
		}
	}

	protected override void OnDispose()
	{
		if (_Setting == null)
		{
			Debug.LogError("Could not remove listener");
		}
		_Setting?.Changed.RemoveListener(OnSettingChanged);
		_Setting = null;
		if (CurrentControl != null)
		{
			ReleaseChildView(CurrentControl);
			CurrentControl = null;
		}
		SettingUpdater = null;
	}
}
