using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Keybindings
{
	public List<KeybindingsLayer> Layers = new List<KeybindingsLayer>
	{
		new KeybindingsLayer("global", new Keybinding[2]
		{
			new Keybinding("cancel", new KeySet(KeyCode.Escape), new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.Action1)),
			new Keybinding("confirm", new KeySet(KeyCode.Return), new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.Action2))
		}),
		new KeybindingsLayer("main", new Keybinding[10]
		{
			new Keybinding("toggle-research", new KeySet(KeyCode.T)),
			new Keybinding("toggle-wiki", new KeySet(KeyCode.G)),
			new Keybinding("toggle-rail-management", new KeySet(KeyCode.M)),
			new Keybinding("toggle-blueprint-library", new KeySet(KeyCode.B)),
			new Keybinding("toggle-ui", new KeySet(KeyCode.F2)),
			new Keybinding("toggle-pause", new KeySet(KeyCode.P)),
			new Keybinding("screenshot", new KeySet(KeyCode.F4)),
			new Keybinding("undo", new KeySet(KeyCode.Z, DefaultControlKeyCode)),
			new Keybinding("redo", new KeySet(KeyCode.Y, DefaultControlKeyCode)),
			new Keybinding("scope-change", new KeySet(KeyCode.Space))
		}),
		new KeybindingsLayer("camera", new Keybinding[15]
		{
			new Keybinding("move-up", new KeySet(KeyCode.W), new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.LeftStickUp)),
			new Keybinding("move-left", new KeySet(KeyCode.A), new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.LeftStickLeft)),
			new Keybinding("move-down", new KeySet(KeyCode.S), new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.LeftStickDown)),
			new Keybinding("move-right", new KeySet(KeyCode.D), new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.LeftStickRight)),
			new Keybinding("mouse-drag-modifier", new KeySet(KeyCode.Mouse0), null, blockableByUI: true, systemDefined: false, 0.001f, isModifierOnly: true),
			new Keybinding("angle-drag-modifier", new KeySet(KeyCode.Mouse2), null, blockableByUI: true, systemDefined: false, 0.001f, isModifierOnly: true),
			new Keybinding("select-layer-up", new KeySet(KeyCode.E), new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.RightBumper)),
			new Keybinding("select-layer-down", new KeySet(KeyCode.Q), new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.LeftBumper)),
			new Keybinding("toggle-show-all-layers", new KeySet(KeyCode.V)),
			new Keybinding("rotate-cw", KeySet.EMPTY, new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.RightStickRight), blockableByUI: true, systemDefined: false, 0.5f),
			new Keybinding("rotate-ccw", KeySet.EMPTY, new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.RightStickLeft), blockableByUI: true, systemDefined: false, 0.5f),
			new Keybinding("zoom-in", KeySet.EMPTY, new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.RightStickDown)),
			new Keybinding("zoom-out", KeySet.EMPTY, new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.RightStickUp)),
			new Keybinding("angle-up", KeySet.EMPTY, new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.LeftTrigger)),
			new Keybinding("angle-down", KeySet.EMPTY, new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.RightTrigger))
		}),
		new KeybindingsLayer("toolbar", new Keybinding[24]
		{
			new Keybinding("select-slot-0", new KeySet(KeyCode.Alpha1)),
			new Keybinding("select-slot-1", new KeySet(KeyCode.Alpha2)),
			new Keybinding("select-slot-2", new KeySet(KeyCode.Alpha3)),
			new Keybinding("select-slot-3", new KeySet(KeyCode.Alpha4)),
			new Keybinding("select-slot-4", new KeySet(KeyCode.Alpha5)),
			new Keybinding("select-slot-5", new KeySet(KeyCode.Alpha6)),
			new Keybinding("select-slot-6", new KeySet(KeyCode.Alpha7)),
			new Keybinding("select-slot-7", new KeySet(KeyCode.Alpha8)),
			new Keybinding("select-slot-8", new KeySet(KeyCode.Alpha9)),
			new Keybinding("select-slot-9", new KeySet(KeyCode.Alpha0)),
			new Keybinding("select-toolbar-0", new KeySet(KeyCode.Alpha1, KeyCode.LeftShift)),
			new Keybinding("select-toolbar-1", new KeySet(KeyCode.Alpha2, KeyCode.LeftShift)),
			new Keybinding("select-toolbar-2", new KeySet(KeyCode.Alpha3, KeyCode.LeftShift)),
			new Keybinding("select-toolbar-3", new KeySet(KeyCode.Alpha4, KeyCode.LeftShift)),
			new Keybinding("select-toolbar-4", new KeySet(KeyCode.Alpha5, KeyCode.LeftShift)),
			new Keybinding("select-toolbar-5", new KeySet(KeyCode.Alpha6, KeyCode.LeftShift)),
			new Keybinding("select-toolbar-6", new KeySet(KeyCode.Alpha7, KeyCode.LeftShift)),
			new Keybinding("select-toolbar-7", new KeySet(KeyCode.Alpha8, KeyCode.LeftShift)),
			new Keybinding("select-toolbar-8", new KeySet(KeyCode.Alpha9, KeyCode.LeftShift)),
			new Keybinding("select-toolbar-9", new KeySet(KeyCode.Alpha0, KeyCode.LeftShift)),
			new Keybinding("next-toolbar", new KeySet(KeyCode.BackQuote)),
			new Keybinding("previous-toolbar", new KeySet(KeyCode.BackQuote, KeyCode.LeftShift)),
			new Keybinding("next-variant", new KeySet(KeyCode.Tab), new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.DPadDown)),
			new Keybinding("previous-variant", KeySet.EMPTY)
		}),
		new KeybindingsLayer("building-placement", new Keybinding[15]
		{
			new Keybinding("confirm-placement", new KeySet(KeyCode.Mouse0), new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.Action2)),
			new Keybinding("cancel-placement", new KeySet(KeyCode.Mouse1), new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.Action1)),
			new Keybinding("rotate-cw", new KeySet(KeyCode.R), new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.DPadRight)),
			new Keybinding("rotate-ccw", new KeySet(KeyCode.R, KeyCode.LeftShift)),
			new Keybinding("mirror", new KeySet(KeyCode.F)),
			new Keybinding("mirror-inverse", new KeySet(KeyCode.F, KeyCode.LeftShift)),
			new Keybinding("place-checkpoint", new KeySet(KeyCode.C), new KeySet(KeyCode.None, KeyCode.None, KeyCode.None, ControllerBinding.DPadUp)),
			new Keybinding("rotate-building-right", new KeySet(KeyCode.RightArrow)),
			new Keybinding("rotate-building-down", new KeySet(KeyCode.DownArrow)),
			new Keybinding("rotate-building-left", new KeySet(KeyCode.LeftArrow)),
			new Keybinding("rotate-building-up", new KeySet(KeyCode.UpArrow)),
			new Keybinding("transform-layer-up", new KeySet(KeyCode.E, KeyCode.LeftShift)),
			new Keybinding("transform-layer-down", new KeySet(KeyCode.Q, KeyCode.LeftShift)),
			new Keybinding("blueprint-allow-replacement", new KeySet(KeyCode.LeftShift), null, blockableByUI: true, systemDefined: false, 0.001f, isModifierOnly: true),
			new Keybinding("save-blueprint", new KeySet(KeyCode.S, DefaultControlKeyCode))
		}),
		new KeybindingsLayer("mass-selection", new Keybinding[13]
		{
			new Keybinding("select-base", new KeySet(KeyCode.Mouse0)),
			new Keybinding("select-single-modifier", new KeySet(KeyCode.LeftControl), null, blockableByUI: true, systemDefined: false, 0.001f, isModifierOnly: true),
			new Keybinding("select-area-modifier", new KeySet(KeyCode.LeftShift), null, blockableByUI: true, systemDefined: false, 0.001f, isModifierOnly: true),
			new Keybinding("deselect-area-modifier", new KeySet(KeyCode.LeftAlt), null, blockableByUI: true, systemDefined: false, 0.001f, isModifierOnly: true),
			new Keybinding("delete", new KeySet(KeyCode.X), new KeySet(KeyCode.Delete)),
			new Keybinding("select-all-of-building-type-modifier", new KeySet(KeyCode.LeftShift), null, blockableByUI: true, systemDefined: false, 0.001f, isModifierOnly: true),
			new Keybinding("pipette", new KeySet(KeyCode.F)),
			new Keybinding("quick-delete-drag", new KeySet(KeyCode.Mouse1)),
			new Keybinding("make-blueprint", new KeySet(KeyCode.C, DefaultControlKeyCode)),
			new Keybinding("paste-blueprint", new KeySet(KeyCode.V, DefaultControlKeyCode)),
			new Keybinding("select-connected", new KeySet(KeyCode.O)),
			new Keybinding("clear-selection-contents", new KeySet(KeyCode.I)),
			new Keybinding("cut-selection", new KeySet(KeyCode.X, DefaultControlKeyCode))
		}),
		new KeybindingsLayer("rail-management", new Keybinding[1]
		{
			new Keybinding("place-demo-train", new KeySet(KeyCode.K))
		}),
		new KeybindingsLayer("waypoints", new Keybinding[4]
		{
			new Keybinding("create-new", new KeySet(KeyCode.L)),
			new Keybinding("jump-back", new KeySet(KeyCode.N)),
			new Keybinding("center-on-base", new KeySet(KeyCode.H)),
			new Keybinding("edit-below-cursor", new KeySet(KeyCode.Mouse1), null, blockableByUI: false)
		}),
		new KeybindingsLayer("shape-viewer", new Keybinding[1]
		{
			new Keybinding("move-modifier", new KeySet(KeyCode.Mouse0), null, blockableByUI: false, systemDefined: false, 0.001f, isModifierOnly: true)
		}),
		new KeybindingsLayer("debug", new Keybinding[10]
		{
			new Keybinding("show-debug-console", new KeySet(KeyCode.F1)),
			new Keybinding("debug-console-autocomplete", new KeySet(KeyCode.Tab)),
			new Keybinding("debug-console-select-previous", new KeySet(KeyCode.UpArrow)),
			new Keybinding("debug-console-select-next", new KeySet(KeyCode.DownArrow)),
			new Keybinding("generic", new KeySet(KeyCode.F3)),
			new Keybinding("pause", new KeySet(KeyCode.F6)),
			new Keybinding("slow-speed", new KeySet(KeyCode.F7)),
			new Keybinding("normal-speed", new KeySet(KeyCode.F8)),
			new Keybinding("fast-speed", new KeySet(KeyCode.F9)),
			new Keybinding("manual-max-step", new KeySet(KeyCode.F10))
		})
	};

	private Dictionary<string, Keybinding> _KeybindingsById = new Dictionary<string, Keybinding>();

	public readonly UnityEvent Changed = new UnityEvent();

	private static KeyCode DefaultControlKeyCode
	{
		get
		{
			if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
			{
				return KeyCode.LeftMeta;
			}
			return KeyCode.LeftControl;
		}
	}

	public IReadOnlyDictionary<string, Keybinding> KeybindingsById => _KeybindingsById;

	protected static string GenerateBindingIdentifier(KeybindingsLayer layer, Keybinding binding)
	{
		return layer.Id + "." + binding.PartialId;
	}

	public static string[] ComputeAllKeybindingIds()
	{
		return new Keybindings().KeybindingsById.Keys.ToArray();
	}

	public Keybindings()
	{
		InitializeKeybindings();
	}

	private void InitializeKeybindings()
	{
		foreach (KeybindingsLayer layer in Layers)
		{
			foreach (Keybinding binding in layer.Bindings)
			{
				string id = GenerateBindingIdentifier(layer, binding);
				binding.AssignIdAndLoad(id);
				binding.Changed.AddListener(Changed.Invoke);
				if (!_KeybindingsById.TryAdd(id, binding))
				{
					Debug.LogError("Keybindings:: Duplicate binding " + id);
				}
			}
		}
	}

	public Keybinding GetById(string id)
	{
		foreach (KeybindingsLayer layer in Layers)
		{
			foreach (Keybinding binding in layer.Bindings)
			{
				if (binding.Id == id)
				{
					return binding;
				}
			}
		}
		return null;
	}

	public void RegisterCommands(DebugConsole console)
	{
		console.Register("keybindings.list", delegate(DebugConsole.CommandContext ctx)
		{
			ctx.Output("Keybindings");
			ctx.Output("");
			foreach (KeybindingsLayer current in Layers)
			{
				ctx.Output("");
				ctx.Output("<b>" + current.Id + "</b>:");
				foreach (Keybinding current2 in current.Bindings)
				{
					ctx.Output("  " + current2.PartialId + ": <b>" + KeyCodeFormatter.Resolve(current2) + "</b>" + (current2.Modified ? " (modified)" : ""));
				}
			}
		});
	}
}
