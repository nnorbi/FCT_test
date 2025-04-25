using System.Collections.Generic;
using System.Data;
using Core.Dependency;
using TMPro;
using Unity.Core.View;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HUDDebugConsole : HUDPart, IRunnableView, IView
{
	[SerializeField]
	private TMP_InputField InputField;

	[SerializeField]
	private TMP_Text ConsoleHistory;

	[SerializeField]
	private CanvasGroup GroupBg;

	[SerializeField]
	private CanvasGroup GroupInput;

	[SerializeField]
	private ScrollRect UIScrollView;

	protected bool Active = false;

	protected string GenericCommand = "";

	protected List<string> HistoryCommands = new List<string>();

	protected int HistoryIndex = -1;

	public void Run()
	{
		Invoke("ShowHelp", 0.1f);
	}

	[Construct]
	public void Construct()
	{
		GenericCommand = PlayerPrefs.GetString("console.debug-generic-command", "rendering.clear-cache");
		base.gameObject.SetActive(value: false);
		InputField.text = "";
		ConsoleHistory.text = "";
		GroupBg.alpha = 1f;
		GroupInput.alpha = 1f;
		RegisterCommands(Singleton<GameCore>.G.Console);
	}

	protected void ShowHelp()
	{
		HandleCommand("help");
	}

	protected override void OnDispose()
	{
		Application.logMessageReceived -= OnLogMessageReceived;
	}

	protected void OnLogMessageReceived(string condition, string stacktrace, LogType type)
	{
		if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
		{
			AddOutput(type.ToString() + ": " + condition);
			AddOutput(stacktrace);
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (Active)
		{
			if (context.ConsumeWasActivated("debug.debug-console-autocomplete"))
			{
				string text = InputField.text.Trim();
				List<string> matching = Singleton<GameCore>.G.Console.GetAutocompletions(text);
				if (matching.Count > 0)
				{
					string commonStart = matching[0];
					for (int i = 1; i < matching.Count; i++)
					{
						string other = matching[i];
						while (commonStart.Length > 0 && !other.StartsWith(commonStart))
						{
							commonStart = commonStart.Remove(commonStart.Length - 1);
						}
					}
					if (commonStart.Length > text.Length)
					{
						InputField.text = commonStart;
						InputField.caretPosition = commonStart.Length;
					}
					else
					{
						AddOutput("There are multiple matching commands available:");
						foreach (string match in matching)
						{
							AddOutput(" - " + match);
						}
					}
				}
				else
				{
					AddOutput("No autocompletions found");
				}
			}
			if (context.ConsumeWasActivated("debug.debug-console-select-previous"))
			{
				UpdateCommandFromHistory(1);
			}
			if (context.ConsumeWasActivated("debug.debug-console-select-next"))
			{
				UpdateCommandFromHistory(-1);
			}
			if (context.ConsumeWasActivated("global.confirm"))
			{
				string text2 = InputField.text.Trim();
				if (text2.Length > 0)
				{
					InputField.text = "";
					HandleCommand(text2);
					InputField.Select();
					InputField.ActivateInputField();
				}
				else
				{
					Hide();
				}
			}
			else if (context.ConsumeWasActivated("global.cancel"))
			{
				Hide();
			}
			else if (context.ConsumeWasActivated("debug.show-debug-console"))
			{
				Hide();
			}
		}
		else if (context.ConsumeWasActivated("debug.show-debug-console"))
		{
			Show();
		}
		SimulationSpeedManager speed = Singleton<GameCore>.G.SimulationSpeed;
		if (context.ConsumeWasActivated("debug.pause"))
		{
			speed.SetSpeed(0f);
		}
		if (context.ConsumeWasActivated("debug.slow-speed"))
		{
			speed.SetSpeed(0.1f);
		}
		if (context.ConsumeWasActivated("debug.normal-speed"))
		{
			speed.SetSpeed(1f);
		}
		if (Singleton<GameCore>.G.Console.CHEATS_ENABLED)
		{
			if (context.ConsumeWasActivated("debug.fast-speed"))
			{
				speed.SetSpeed(50f);
			}
			if (context.ConsumeWasActivated("debug.manual-max-step"))
			{
				float prev = speed.Speed;
				speed.SetSpeed(1f);
				Singleton<GameCore>.G.PerformTick(0.1f);
				speed.SetSpeed(prev);
			}
		}
		if (context.ConsumeWasActivated("debug.generic"))
		{
			HandleCommand(GenericCommand);
		}
		if (Active)
		{
			context.ConsumeAll();
		}
	}

	private void UpdateCommandFromHistory(int offset)
	{
		HistoryIndex += offset;
		if (HistoryIndex < 0)
		{
			HistoryIndex = -1;
			InputField.text = "";
			InputField.caretPosition = 0;
			return;
		}
		HistoryIndex = math.min(HistoryIndex, HistoryCommands.Count - 1);
		List<string> historyCommands = HistoryCommands;
		int num = HistoryIndex + 1;
		string command = historyCommands[historyCommands.Count - num];
		InputField.text = command;
		InputField.caretPosition = command.Length;
	}

	protected void Hide()
	{
		if (Active)
		{
			Active = false;
			GroupBg.blocksRaycasts = false;
			InputField.gameObject.SetActive(value: false);
			base.gameObject.SetActive(value: false);
		}
	}

	protected void Show()
	{
		if (!Active)
		{
			Active = true;
			GroupBg.blocksRaycasts = true;
			base.gameObject.SetActive(value: true);
			InputField.gameObject.SetActive(value: true);
			InputField.Select();
			InputField.ActivateInputField();
		}
	}

	protected void AddOutput(string line)
	{
		TMP_Text consoleHistory = ConsoleHistory;
		consoleHistory.text = consoleHistory.text + line + "\n";
		UIScrollView.normalizedPosition = new Vector2(0f, 0f);
	}

	protected void HandleCommand(string cmd)
	{
		HistoryCommands.Add(cmd);
		HistoryIndex = -1;
		Singleton<GameCore>.G.Console.ParseAndExecute(cmd, AddOutput);
	}

	protected void RegisterCommands(DebugConsole console)
	{
		console.Register("console.bind-hotkey", new DebugConsole.StringOption("command"), delegate(DebugConsole.CommandContext ctx)
		{
			BindGenericCommand(ctx, ctx.GetString(0).Replace("%20", " "));
		});
		console.Register("console.bind-last-hotkey", delegate(DebugConsole.CommandContext ctx)
		{
			List<string> historyCommands = HistoryCommands;
			BindGenericCommand(ctx, historyCommands[historyCommands.Count - 2]);
		});
		console.Register("console.clear", delegate(DebugConsole.CommandContext ctx)
		{
			ConsoleHistory.text = "";
			ctx.Output("Console has been cleared.");
		});
		console.Register("console.close", delegate
		{
			Hide();
		});
		console.Register("calc", new DebugConsole.StringOption("expression-without-spaces"), delegate(DebugConsole.CommandContext ctx)
		{
			string expression = ctx.GetString(0);
			DataTable dataTable = new DataTable();
			object obj = dataTable.Compute(expression, "");
			ctx.Output("Result: " + obj);
		});
		console.Register("language-glyph-test", delegate(DebugConsole.CommandContext ctx)
		{
			ctx.Output("\nEnglish:\nMost low-voltage <b>lights</b> cast a localised beam or a diffused glow.\n\nChinese (simplified):\n大多数低压灯<b>会投射</b>局部光束或漫射光。\n\nChinese (Traditional):\n大多數低壓燈會<b>投射局部</b>光束或漫射光。\n\nJapanese:\nほとんどの低電<b>圧ライトは</b>、局所的なビームまたは拡散した光を放ちます。\n\nKorean:\n대부분의 저전압<b> 조명은 </b>국지적인 광선이나 확산된 광선을 투사합니다.\n\nTurkish:\nDüşük voltajlı <b>ışıkların çoğu</b>, lokalize bir ışın veya dağınık bir parlaklık yayar.\n\nRussian:\nБольшинство <b>низковоль</b>тных ламп излучают локализованный луч или рассеянное свечение.\n\nFrench:\nLa plupart des <b>lampes</b> basse tension projettent un faisceau localisé ou une lueur diffuse.\n\nGerman:\nDie meisten <b>Niedervoltlampen</b> erzeugen einen lokalisierten Strahl oder ein diffuses Leuchten.\n\nThai:\nไฟแรงด\u0e31นต\u0e48ำส\u0e48วนให<b>ญ\u0e48จะปล\u0e48อ</b>ยลำแสงเฉพาะจ\u0e38ดหร\u0e37อแสงแบบกระจาย\n\nSpanish:\nLa mayoría de <b>las luces</b> de bajo voltaje emiten un haz localizado o un brillo difuso.\n\nPortuguese - Brazil:\nA maioria das <b>luzes de baixa</b> tensão emite um feixe localizado ou um brilho difuso.\n\nPolish:\nWiększość lamp <b>niskonapięciowych</b> rzuca miejscową wiązkę lub rozproszoną poświatę.\n");
		});
	}

	private void BindGenericCommand(DebugConsole.CommandContext ctx, string command)
	{
		GenericCommand = command;
		ctx.Output("Hotkey command set to '" + command + "'");
		PlayerPrefs.SetString("console.debug-generic-command", command);
		PlayerPrefs.Save();
	}
}
