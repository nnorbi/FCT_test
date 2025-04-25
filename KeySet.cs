using System;
using UnityEngine;

public struct KeySet
{
	public static KeySet EMPTY = new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.None);

	public KeyCode Code;

	public KeyCode Modifier0;

	public KeyCode Modifier1;

	public ControllerBinding ControllerSource;

	public bool IsControllerBinding => ControllerSource != ControllerBinding.None;

	public bool Empty => Code == KeyCode.None && ControllerSource == ControllerBinding.None;

	private static bool IsMouseKey(KeyCode code)
	{
		return code == KeyCode.Mouse0 || code == KeyCode.Mouse1;
	}

	public KeySet(KeyCode code = KeyCode.None, KeyCode modifier0 = KeyCode.None, KeyCode modifier1 = KeyCode.None, ControllerBinding controllerSource = ControllerBinding.None)
	{
		Code = code;
		Modifier0 = modifier0;
		Modifier1 = modifier1;
		ControllerSource = controllerSource;
		ControllerSource = ControllerBinding.None;
		if (ControllerSource != ControllerBinding.None && Code != KeyCode.None)
		{
			throw new Exception("Have controler source and code");
		}
		if (modifier1 != KeyCode.None && modifier0 == KeyCode.None)
		{
			throw new Exception("Modifier1 has value but Modifier0 doesn't");
		}
	}

	public void Save(string prefix, KeySet defaults)
	{
		if (Equals(defaults))
		{
			PlayerPrefs.SetInt(prefix + ".override", 0);
			return;
		}
		PlayerPrefs.SetInt(prefix + ".override", 1);
		PlayerPrefs.SetInt(prefix + ".code", (int)Code);
		PlayerPrefs.SetInt(prefix + ".controller-source", (int)ControllerSource);
		PlayerPrefs.SetInt(prefix + ".modifier0", (int)Modifier0);
		PlayerPrefs.SetInt(prefix + ".modifier1", (int)Modifier1);
	}

	public void Load(string prefix, KeySet defaults)
	{
		if (PlayerPrefs.GetInt(prefix + ".override", 0) == 0)
		{
			Code = defaults.Code;
			Modifier0 = defaults.Modifier0;
			Modifier1 = defaults.Modifier1;
			ControllerSource = defaults.ControllerSource;
		}
		else
		{
			Code = (KeyCode)PlayerPrefs.GetInt(prefix + ".code", 0);
			ControllerSource = (ControllerBinding)PlayerPrefs.GetInt(prefix + ".controller-source", 0);
			Modifier0 = (KeyCode)PlayerPrefs.GetInt(prefix + ".modifier0", 0);
			Modifier1 = (KeyCode)PlayerPrefs.GetInt(prefix + ".modifier1", 0);
		}
	}

	public bool IsCurrentlyActive(float axisThreshold)
	{
		if (IsControllerBinding)
		{
			float value = 0f;
			return value > axisThreshold;
		}
		if (Modifier1 != KeyCode.None)
		{
			return GetKey(Code) && GetKey(Modifier0) && GetKey(Modifier1);
		}
		if (Modifier0 != KeyCode.None)
		{
			return GetKey(Code) && GetKey(Modifier0);
		}
		return GetKey(Code);
	}

	public bool Equals(KeySet other)
	{
		return Code == other.Code && Modifier0 == other.Modifier0 && Modifier1 == other.Modifier1 && ControllerSource == other.ControllerSource;
	}

	public float GetAxisValue()
	{
		if (IsControllerBinding)
		{
			return 0f;
		}
		return IsCurrentlyActive(0f) ? 1 : 0;
	}

	public int GetPriority()
	{
		if (IsControllerBinding)
		{
			return 15;
		}
		if (Modifier1 != KeyCode.None)
		{
			return (int)(20000 + Modifier0 + 1000 * (int)Modifier1);
		}
		if (Modifier0 != KeyCode.None)
		{
			return (int)(20 + Modifier0);
		}
		return 10;
	}

	public bool IsBlockableByUI()
	{
		if (IsControllerBinding)
		{
			return false;
		}
		if (Modifier1 != KeyCode.None)
		{
			return IsMouseKey(Modifier1) || IsMouseKey(Modifier0) || IsMouseKey(Code);
		}
		if (Modifier0 != KeyCode.None)
		{
			return IsMouseKey(Modifier0) || IsMouseKey(Code);
		}
		return IsMouseKey(Code);
	}

	private bool GetKey(KeyCode code)
	{
		if (code >= KeyCode.Mouse0 && code <= KeyCode.Mouse6)
		{
			return Input.GetMouseButton((int)(code - 323));
		}
		return Input.GetKey(code);
	}
}
