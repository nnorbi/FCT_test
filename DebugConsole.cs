using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugConsole
{
	public abstract class ConsoleOption
	{
		public string Name;

		public ConsoleOption(string name)
		{
			Name = name;
		}

		public abstract bool Parse(string raw);
	}

	public class StringOption : ConsoleOption
	{
		public string Value;

		public StringOption(string name = "string")
			: base(name)
		{
		}

		public override bool Parse(string raw)
		{
			Value = raw;
			return true;
		}
	}

	public class IntOption : ConsoleOption
	{
		public int Value;

		protected int Min;

		protected int Max;

		public IntOption(string name = "int", int min = int.MinValue, int max = int.MaxValue)
			: base(name)
		{
			Min = min;
			Max = max;
		}

		public override bool Parse(string raw)
		{
			if (!int.TryParse(raw, out Value))
			{
				return false;
			}
			if (Value < Min || Value > Max)
			{
				return false;
			}
			return true;
		}
	}

	public class LongOption : ConsoleOption
	{
		public long Value;

		protected long Min;

		protected long Max;

		public LongOption(string name = "long", long min = long.MinValue, long max = long.MaxValue)
			: base(name)
		{
			Min = min;
			Max = max;
		}

		public override bool Parse(string raw)
		{
			if (!long.TryParse(raw, out Value))
			{
				return false;
			}
			if (Value < Min || Value > Max)
			{
				return false;
			}
			return true;
		}
	}

	public class FloatOption : ConsoleOption
	{
		public float Value;

		public float Min;

		public float Max;

		public FloatOption(string name, float min, float max)
			: base(name)
		{
			Min = min;
			Max = max;
		}

		public override bool Parse(string raw)
		{
			if (!float.TryParse(raw, out Value))
			{
				return false;
			}
			if (Value < Min || Value > Max)
			{
				return false;
			}
			return true;
		}
	}

	public class BoolOption : ConsoleOption
	{
		public bool Value;

		public BoolOption(string name)
			: base(name)
		{
		}

		public override bool Parse(string raw)
		{
			if (raw == "1" || raw == "true")
			{
				Value = true;
				return true;
			}
			if (raw == "0" || raw == "false")
			{
				Value = false;
				return true;
			}
			return false;
		}
	}

	public class CommandContext
	{
		public ConsoleOption[] Options;

		public Action<string> Output;

		public T Get<T>(int n) where T : ConsoleOption
		{
			return (T)Options[n];
		}

		public string GetString(int n)
		{
			return Get<StringOption>(n).Value;
		}

		public int GetInt(int n)
		{
			return Get<IntOption>(n).Value;
		}

		public long GetLong(int n)
		{
			return Get<LongOption>(n).Value;
		}

		public float GetFloat(int n)
		{
			return Get<FloatOption>(n).Value;
		}

		public bool GetBool(int n)
		{
			return Get<BoolOption>(n).Value;
		}
	}

	public class Command
	{
		public string Id;

		public ConsoleOption[] Options;

		public Action<CommandContext> Handler;

		public bool IsCheat;
	}

	protected Dictionary<string, Command> Commands = new Dictionary<string, Command>();

	public bool CHEATS_ENABLED { get; protected set; } = Application.isEditor;

	public DebugConsole()
	{
		Register("help", delegate(CommandContext ctx)
		{
			if (!CHEATS_ENABLED)
			{
				ctx.Output("Cheats DISABLED. Cheat commands are not shown.");
			}
			else
			{
				ctx.Output("Cheats ENABLED.");
			}
			if (Singleton<GameCore>.G.Savegame.Meta.CheatsUsed)
			{
				ctx.Output("Savegame flagged: Cheats have been used in the past.");
			}
			ctx.Output("");
			ctx.Output("Available commands: ");
			ctx.Output("");
			string[] array = new string[Commands.Keys.Count];
			Commands.Keys.CopyTo(array, 0);
			Array.Sort(array);
			string[] array2 = array;
			foreach (string text in array2)
			{
				Command command = Commands[text];
				if (!command.IsCheat || CHEATS_ENABLED)
				{
					string text2 = "";
					ConsoleOption[] options = command.Options;
					foreach (ConsoleOption consoleOption in options)
					{
						text2 = text2 + "[" + consoleOption.Name + "] ";
					}
					ctx.Output(" " + (command.IsCheat ? "<color=red>CHEAT</color> " : "") + text + " " + text2);
				}
			}
		});
		Register("cheats.set-enabled", new BoolOption("enabled"), delegate(CommandContext ctx)
		{
			if (ctx.GetBool(0))
			{
				CHEATS_ENABLED = true;
				ctx.Output("Cheats have been ENABLED. Notice that using a cheat command permanently tags your savegame.");
			}
			else
			{
				CHEATS_ENABLED = false;
				ctx.Output("Cheats have been DISABLED.");
			}
		});
	}

	public void OnGameCleanup()
	{
		Commands.Clear();
	}

	public void Register(string id, ConsoleOption[] options, Action<CommandContext> handler, bool isCheat = false)
	{
		Commands.Add(id, new Command
		{
			Id = id,
			Options = options,
			Handler = handler,
			IsCheat = isCheat
		});
	}

	public void Register(string id, Action<CommandContext> handler, bool isCheat = false)
	{
		Commands.Add(id, new Command
		{
			Id = id,
			Options = new ConsoleOption[0],
			Handler = handler,
			IsCheat = isCheat
		});
	}

	public void Register(string id, ConsoleOption option0, Action<CommandContext> handler, bool isCheat = false)
	{
		Register(id, new ConsoleOption[1] { option0 }, handler, isCheat);
	}

	public void Register(string id, ConsoleOption option0, ConsoleOption option1, Action<CommandContext> handler, bool isCheat = false)
	{
		Register(id, new ConsoleOption[2] { option0, option1 }, handler, isCheat);
	}

	public void Register(string id, ConsoleOption option0, ConsoleOption option1, ConsoleOption option2, ConsoleOption option3, ConsoleOption option4, Action<CommandContext> handler, bool isCheat = false)
	{
		Register(id, new ConsoleOption[5] { option0, option1, option2, option3, option4 }, handler, isCheat);
	}

	public List<string> GetAutocompletions(string start)
	{
		List<string> result = new List<string>();
		if (start.Length < 1)
		{
			return result;
		}
		foreach (KeyValuePair<string, Command> command in Commands)
		{
			if (command.Key.StartsWith(start))
			{
				result.Add(command.Key);
			}
		}
		return result;
	}

	public void ParseAndExecute(string command, Action<string> output)
	{
		string[] parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length == 0)
		{
			return;
		}
		output("");
		string cmd = parts[0].ToLower();
		if (!Commands.ContainsKey(cmd))
		{
			output("Command not found: " + cmd);
			return;
		}
		Command commandInstance = Commands[cmd];
		if (commandInstance.IsCheat && !CHEATS_ENABLED)
		{
			output("Cheats are currently not enabled. You can enable them with cheats.set-enabled 1");
			return;
		}
		if (parts.Length != commandInstance.Options.Length + 1)
		{
			output("Invalid amount of arguments, expected: " + commandInstance.Options.Length);
			return;
		}
		for (int i = 0; i < commandInstance.Options.Length; i++)
		{
			string rawValue = parts[i + 1].Trim();
			if (!commandInstance.Options[i].Parse(rawValue))
			{
				output("Invalid argument for " + commandInstance.Options[i].Name + ": " + rawValue);
				return;
			}
		}
		commandInstance.Handler(new CommandContext
		{
			Options = commandInstance.Options,
			Output = output
		});
		if (commandInstance.IsCheat)
		{
			if (!Singleton<GameCore>.G.Savegame.Meta.CheatsUsed)
			{
				output("Your savegame has been permanently tagged for using cheats.");
			}
			Singleton<GameCore>.G.Savegame.Meta.CheatsUsed = true;
		}
	}
}
