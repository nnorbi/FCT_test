using System;
using UnityEngine;

public abstract class Fluid
{
	public float FlowRateFactor = 0.0004f;

	public float Friction = 0.001f;

	public static Fluid Deserialize(string serialized)
	{
		if (string.IsNullOrEmpty(serialized))
		{
			throw new Exception("Bad serialized string for Fluid, is empty");
		}
		if (serialized.StartsWith(ColorFluid.PREFIX))
		{
			return ColorFluid.Deserialize(serialized);
		}
		throw new Exception("Bad serialized string for fluid: " + serialized);
	}

	public static void Sync<T>(ISerializationVisitor visitor, ref T target) where T : Fluid
	{
		if (visitor.Writing)
		{
			visitor.WriteString_4(target?.Serialize());
			return;
		}
		string serializedCode = visitor.ReadString_4();
		if (string.IsNullOrEmpty(serializedCode))
		{
			target = null;
		}
		else
		{
			target = Deserialize(serializedCode) as T;
		}
	}

	public abstract Color GetMainColor();

	public abstract Color GetUIColor();

	public abstract Material GetMaterial();

	public abstract string Serialize();
}
