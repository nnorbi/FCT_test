using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct BlueprintCurrency : IEquatable<BlueprintCurrency>, IComparable<BlueprintCurrency>
{
	public const uint FRACTION = 100u;

	public static readonly BlueprintCurrency Zero;

	[SerializeField]
	private long FractionAmount;

	public long Main => FractionAmount / 100;

	public short Sub => (short)(FractionAmount % 100);

	public double TotalMain => (double)FractionAmount / 100.0;

	public long TotalSub => FractionAmount;

	public BlueprintCurrency(long main, short sub)
		: this(main * 100 + sub)
	{
	}

	private BlueprintCurrency(long fractionAmount)
	{
		FractionAmount = fractionAmount;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(BlueprintCurrency other)
	{
		return this == other;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(BlueprintCurrency other)
	{
		return FractionAmount.CompareTo(other.FractionAmount);
	}

	public static BlueprintCurrency FromSub(double subCurrency)
	{
		return new BlueprintCurrency((long)math.round(subCurrency));
	}

	public static BlueprintCurrency FromMain(double mainCurrency)
	{
		return new BlueprintCurrency((long)math.round(mainCurrency * 100.0));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static BlueprintCurrency operator +(BlueprintCurrency lhs, BlueprintCurrency rhs)
	{
		return new BlueprintCurrency(lhs.FractionAmount + rhs.FractionAmount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static BlueprintCurrency operator -(BlueprintCurrency lhs, BlueprintCurrency rhs)
	{
		return new BlueprintCurrency(lhs.FractionAmount - rhs.FractionAmount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static BlueprintCurrency operator *(BlueprintCurrency lhs, int rhs)
	{
		return new BlueprintCurrency(lhs.FractionAmount * rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static BlueprintCurrency operator *(int lhs, BlueprintCurrency rhs)
	{
		return new BlueprintCurrency(lhs * rhs.FractionAmount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static BlueprintCurrency operator /(BlueprintCurrency lhs, int rhs)
	{
		return new BlueprintCurrency(lhs.FractionAmount / rhs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static BlueprintCurrency operator -(BlueprintCurrency val)
	{
		return new BlueprintCurrency(-val.FractionAmount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static BlueprintCurrency operator +(BlueprintCurrency val)
	{
		return new BlueprintCurrency(val.FractionAmount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(BlueprintCurrency lhs, BlueprintCurrency rhs)
	{
		return lhs.FractionAmount == rhs.FractionAmount;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(BlueprintCurrency lhs, BlueprintCurrency rhs)
	{
		return lhs.FractionAmount != rhs.FractionAmount;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <(BlueprintCurrency lhs, BlueprintCurrency rhs)
	{
		return lhs.FractionAmount < rhs.FractionAmount;
	}

	public static bool operator >(BlueprintCurrency lhs, BlueprintCurrency rhs)
	{
		return lhs.FractionAmount > rhs.FractionAmount;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <=(BlueprintCurrency lhs, BlueprintCurrency rhs)
	{
		return lhs.FractionAmount <= rhs.FractionAmount;
	}

	public static bool operator >=(BlueprintCurrency lhs, BlueprintCurrency rhs)
	{
		return lhs.FractionAmount >= rhs.FractionAmount;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals(object obj)
	{
		return obj is BlueprintCurrency other && Equals(other);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return FractionAmount.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return string.Format("{0}({1:0.00})", "BlueprintCurrency", TotalMain);
	}
}
