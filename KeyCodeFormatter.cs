#define UNITY_ASSERTIONS
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class KeyCodeFormatter
{
	private const string DIVIDER = " + ";

	public static Dictionary<KeyCode, string> MOUSE_TO_SPRITE_MAPPINGS = new Dictionary<KeyCode, string>
	{
		{
			KeyCode.Mouse0,
			"LeftClick"
		},
		{
			KeyCode.Mouse1,
			"RightClick"
		},
		{
			KeyCode.Mouse2,
			"MiddleClick"
		},
		{
			KeyCode.Mouse3,
			"Right"
		},
		{
			KeyCode.Mouse4,
			"Left"
		}
	};

	public static Dictionary<KeyCode, string> KEY_TO_TEXT_MAPPINGS = new Dictionary<KeyCode, string>
	{
		{
			KeyCode.Slash,
			"/"
		},
		{
			KeyCode.Asterisk,
			"*"
		},
		{
			KeyCode.Minus,
			"-"
		},
		{
			KeyCode.Plus,
			"+"
		},
		{
			KeyCode.Equals,
			"="
		},
		{
			KeyCode.BackQuote,
			"`"
		},
		{
			KeyCode.LeftBracket,
			"["
		},
		{
			KeyCode.RightBracket,
			"]"
		},
		{
			KeyCode.Backslash,
			"\\"
		},
		{
			KeyCode.Semicolon,
			";"
		},
		{
			KeyCode.Quote,
			"'"
		},
		{
			KeyCode.Comma,
			","
		},
		{
			KeyCode.Period,
			"."
		},
		{
			KeyCode.Exclaim,
			"!"
		},
		{
			KeyCode.At,
			"@"
		},
		{
			KeyCode.Hash,
			"#"
		},
		{
			KeyCode.Dollar,
			"$"
		},
		{
			KeyCode.Percent,
			"%"
		},
		{
			KeyCode.Caret,
			"^"
		},
		{
			KeyCode.Ampersand,
			"&"
		},
		{
			KeyCode.LeftParen,
			"("
		},
		{
			KeyCode.RightParen,
			")"
		},
		{
			KeyCode.Underscore,
			"_"
		},
		{
			KeyCode.Colon,
			","
		},
		{
			KeyCode.DoubleQuote,
			"\""
		},
		{
			KeyCode.Less,
			"<"
		},
		{
			KeyCode.Greater,
			">"
		},
		{
			KeyCode.Tilde,
			"~"
		},
		{
			KeyCode.LeftCurlyBracket,
			"{"
		},
		{
			KeyCode.RightCurlyBracket,
			"}"
		},
		{
			KeyCode.Question,
			"?"
		},
		{
			KeyCode.Pipe,
			"|"
		}
	};

	public static Dictionary<KeyCode, string> KEY_TO_TRANSLATION_MAPPINGS = new Dictionary<KeyCode, string>
	{
		{
			KeyCode.LeftWindows,
			"key.Win"
		},
		{
			KeyCode.RightWindows,
			"key.Win"
		},
		{
			KeyCode.LeftControl,
			"key.Ctrl"
		},
		{
			KeyCode.RightControl,
			"key.Ctrl"
		},
		{
			KeyCode.LeftAlt,
			"key.Alt"
		},
		{
			KeyCode.RightAlt,
			"key.Alt"
		},
		{
			KeyCode.CapsLock,
			"key.Capslock"
		},
		{
			KeyCode.Tab,
			"key.Tab"
		},
		{
			KeyCode.Escape,
			"key.Escape"
		},
		{
			KeyCode.Backspace,
			"key.Backspace"
		},
		{
			KeyCode.UpArrow,
			"key.UpArrow"
		},
		{
			KeyCode.RightArrow,
			"key.RightArrow"
		},
		{
			KeyCode.LeftArrow,
			"key.LeftArrow"
		},
		{
			KeyCode.DownArrow,
			"key.DownArrow"
		},
		{
			KeyCode.LeftMeta,
			"key.Command"
		},
		{
			KeyCode.RightMeta,
			"key.Command"
		},
		{
			KeyCode.LeftShift,
			"key.LeftShift"
		},
		{
			KeyCode.RightShift,
			"key.RightShift"
		},
		{
			KeyCode.Return,
			"key.Enter"
		},
		{
			KeyCode.Space,
			"key.Space"
		},
		{
			KeyCode.Insert,
			"key.Insert"
		},
		{
			KeyCode.Delete,
			"key.Delete"
		},
		{
			KeyCode.Home,
			"key.Home"
		},
		{
			KeyCode.End,
			"key.End"
		},
		{
			KeyCode.PageUp,
			"key.PageUp"
		},
		{
			KeyCode.PageDown,
			"key.PageDown"
		}
	};

	public static string Resolve(Keybinding binding)
	{
		Debug.Assert(binding != null, "Empty keybinding in resolve");
		return Resolve(FindKeySet(binding));
	}

	public static string Resolve(Keybinding binding, Keybinding additionalBinding)
	{
		Debug.Assert(binding != null, "Empty keybinding in resolve");
		return Resolve(FindKeySet(binding), FindKeySet(additionalBinding));
	}

	public static KeySet FindKeySet(Keybinding binding)
	{
		Debug.Assert(binding != null, "Empty keybinding in resolve");
		bool usingController = Singleton<GameCore>.G.LocalPlayer.InputMode == GameInputModeType.Controller;
		for (int i = 0; i < 2; i++)
		{
			KeySet keySet = binding.GetKeySetAt(i);
			if (!keySet.Empty && keySet.IsControllerBinding == usingController)
			{
				return keySet;
			}
		}
		return KeySet.EMPTY;
	}

	public static string Resolve(KeySet keySet)
	{
		string result = "";
		if (keySet.IsControllerBinding)
		{
			return "???";
		}
		if (keySet.Modifier0 != KeyCode.None)
		{
			result = result + Resolve(keySet.Modifier0) + " + ";
		}
		if (keySet.Modifier1 != KeyCode.None)
		{
			result = result + Resolve(keySet.Modifier1) + " + ";
		}
		return result + Resolve(keySet.Code);
	}

	public static string Resolve(KeySet a, KeySet b)
	{
		return Resolve(a) + " + " + Resolve(b);
	}

	private static string WrapText(string text)
	{
		return "<b>" + text + "</b>";
	}

	public static string Resolve(KeyCode code)
	{
		if (code == KeyCode.None)
		{
			return " ";
		}
		if (code >= KeyCode.Alpha0 && code <= KeyCode.Alpha9)
		{
			return WrapText(((int)(code - 48)).ToString("N0", CultureInfo.CurrentCulture));
		}
		if (MOUSE_TO_SPRITE_MAPPINGS.TryGetValue(code, out var mouseSpriteId))
		{
			return "<sprite=\"Mouse\" name=\"" + mouseSpriteId + "\">";
		}
		if (KEY_TO_TEXT_MAPPINGS.TryGetValue(code, out var textCode))
		{
			return WrapText(textCode);
		}
		if (KEY_TO_TRANSLATION_MAPPINGS.TryGetValue(code, out var translationId))
		{
			return WrapText(translationId.tr());
		}
		return WrapText(code.ToString());
	}

	private static string ResolveController(Dictionary<ControllerBinding, string> data, string controllerId, ControllerBinding code)
	{
		if (code == ControllerBinding.None)
		{
			return " ";
		}
		if (data.TryGetValue(code, out var spriteId))
		{
			return "<sprite=\"" + controllerId + "\" name=\"" + spriteId + "\">";
		}
		return code.ToString() ?? "";
	}
}
