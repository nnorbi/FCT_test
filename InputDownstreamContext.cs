using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public class InputDownstreamContext
{
	protected Dictionary<Keybinding, KeySet> LastBindings = new Dictionary<Keybinding, KeySet>();

	protected Dictionary<Keybinding, KeySet> ActiveBindings = new Dictionary<Keybinding, KeySet>();

	protected readonly IReadOnlyDictionary<string, Keybinding> AllBindings;

	protected HashSet<string> ConsumedTokens = new HashSet<string>();

	protected HashSet<Keybinding> ConsumedBindings = new HashSet<Keybinding>();

	public float2 MousePosition;

	public float2 MouseDelta;

	public float WheelDelta;

	public GameObject UIHoverElement;

	public InputDownstreamContext(IReadOnlyDictionary<string, Keybinding> keybindingsById)
	{
		AllBindings = keybindingsById;
	}

	public void Update(IEnumerable<(Keybinding, KeySet)> lastBindings, IEnumerable<(Keybinding, KeySet)> activeBindings, float2 mousePosition, float2 mouseDelta, float mouseWheelDelta, GameObject mouseHoverElement)
	{
		LastBindings.Clear();
		foreach (var (binding, set) in lastBindings)
		{
			LastBindings.Add(binding, set);
		}
		ActiveBindings.Clear();
		foreach (var (binding2, set2) in activeBindings)
		{
			ActiveBindings.Add(binding2, set2);
		}
		ConsumedTokens.Clear();
		ConsumedBindings.Clear();
		MousePosition = mousePosition;
		MouseDelta = mouseDelta;
		WheelDelta = mouseWheelDelta;
		UIHoverElement = mouseHoverElement;
	}

	public void ConsumeAll(IEnumerable<string> consumeTokens = null)
	{
		ActiveBindings.Clear();
		MouseDelta = new float2(0);
		WheelDelta = 0f;
		if (consumeTokens == null)
		{
			return;
		}
		foreach (string token in consumeTokens)
		{
			ConsumedTokens.Add(token);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsActive(string id)
	{
		return IsActive(AllBindings[id]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsActive(Keybinding binding)
	{
		return !ConsumedBindings.Contains(binding) && ActiveBindings.ContainsKey(binding);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsActivated(string id)
	{
		return IsActivated(AllBindings[id]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsActivated(Keybinding binding)
	{
		return !ConsumedBindings.Contains(binding) && ActiveBindings.ContainsKey(binding) && !LastBindings.ContainsKey(binding);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsDeactivated(string id)
	{
		return IsDeactivated(AllBindings[id]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsDeactivated(Keybinding binding)
	{
		return !ConsumedBindings.Contains(binding) && !ActiveBindings.ContainsKey(binding) && LastBindings.ContainsKey(binding);
	}

	public bool ConsumeIsActive(string id)
	{
		Keybinding binding = AllBindings[id];
		if (!ActiveBindings.TryGetValue(binding, out var keySet))
		{
			return false;
		}
		return TryConsume(binding, keySet);
	}

	public float ConsumeAsAxis(string id)
	{
		Keybinding binding = AllBindings[id];
		if (!ActiveBindings.TryGetValue(binding, out var keySet))
		{
			return 0f;
		}
		if (!TryConsume(binding, keySet))
		{
			return 0f;
		}
		return keySet.GetAxisValue();
	}

	public bool ConsumeWasActivated(string id)
	{
		Keybinding binding = AllBindings[id];
		if (LastBindings.ContainsKey(binding))
		{
			return false;
		}
		if (!ActiveBindings.TryGetValue(binding, out var keySet))
		{
			return false;
		}
		return TryConsume(binding, keySet);
	}

	public bool ConsumeWasDeactivated(string id)
	{
		Keybinding binding = AllBindings[id];
		if (ActiveBindings.ContainsKey(binding))
		{
			return false;
		}
		if (!LastBindings.TryGetValue(binding, out var keySet))
		{
			return false;
		}
		return TryConsume(binding, keySet);
	}

	public bool ConsumeAllCheckOneActivated(params string[] ids)
	{
		bool result = false;
		foreach (string id in ids)
		{
			Keybinding binding = AllBindings[id];
			if (ActiveBindings.TryGetValue(binding, out var keySet) && !LastBindings.ContainsKey(binding))
			{
				result |= TryConsume(binding, keySet);
			}
		}
		return result;
	}

	protected bool TryConsume(Keybinding binding, KeySet keySet)
	{
		if (!ConsumedBindings.Add(binding))
		{
			return false;
		}
		foreach (KeyValuePair<Keybinding, KeySet> otherBinding in ActiveBindings)
		{
			if (otherBinding.Key != binding)
			{
				if (keySet.Code != KeyCode.None && otherBinding.Value.Code == keySet.Code)
				{
					ConsumedBindings.Add(otherBinding.Key);
				}
				else if (keySet.ControllerSource != ControllerBinding.None && otherBinding.Value.ControllerSource == keySet.ControllerSource)
				{
					ConsumedBindings.Add(otherBinding.Key);
				}
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool HasWheelDelta()
	{
		return math.abs(WheelDelta) > 0f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float ConsumeWheelDelta()
	{
		float result = WheelDelta;
		WheelDelta = 0f;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool HasMouseDelta()
	{
		return math.length(MouseDelta) > 0f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float2 ConsumeMouseDelta()
	{
		float2 value = MouseDelta;
		MouseDelta = new float2(0);
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool ConsumeToken(string token)
	{
		return ConsumedTokens.Add(token);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool ReleaseToken(string token)
	{
		return ConsumedTokens.Remove(token);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsTokenAvailable(string token)
	{
		return !ConsumedTokens.Contains(token);
	}
}
