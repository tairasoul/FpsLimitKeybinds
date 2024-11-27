using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Tomlyn;
using UnityEngine;

namespace FpsLimitKeybinds;

class tomlKeybinds 
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public string[] keybinds {get; set;}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}

class Keybind 
{
	public KeyboardShortcut bind;
	public int fps;
	private bool active;
	public Keybind() 
	{
		active = false;
	}
	public void Check() 
	{
		if (bind.IsPressed() && !active) 
		{
			active = true;
			Plugin.currentFps = fps;
		}
		else if (!bind.IsPressed() && active) 
		{
			active = false;
		}
	}
}

struct ParsedBind 
{
	public KeyCode[] modifiers;
	public KeyCode key;
}

[BepInPlugin("tairasoul.vaproxy.fpskeybinds", "fps-keybinds", "1.0.0")]
public class Plugin : BaseUnityPlugin 
{
	internal static int currentFps = 60;
	internal static ManualLogSource Log;
	Keybind[] fpsBinds = [];
	public Plugin() 
	{
		LoadRequiredAssemblies();
	}
	private void LoadAssemblyIfNeeded(string assemblyName)
	{
		if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == assemblyName))
		{
			Logger.LogInfo($"{assemblyName} is already loaded.");
			return;
		}
		string resource = $"fps-keybinds.libraries.{assemblyName}.dll";

		using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
		if (stream == null)
		{
			Logger.LogError($"Failed to find embedded resource: {resource}");
			return;
		}

		byte[] assemblyData = new byte[stream.Length];
		stream.Read(assemblyData, 0, assemblyData.Length);

		Assembly.Load(assemblyData);
		Logger.LogInfo($"{assemblyName} loaded successfully.");
	}
	
	private void LoadRequiredAssemblies() 
	{
		string[] assemblies = [
			"Tomlyn"
		];
		foreach (string assembly in assemblies)
			LoadAssemblyIfNeeded(assembly);
	}
	
	void Awake() 
	{
		Log = Logger;
		string configPath = Path.Combine(Paths.ConfigPath, "keybinds.toml");
		string preset = "keybinds = []";
		if (!File.Exists(configPath)) 
			File.WriteAllText(configPath, preset);	
		tomlKeybinds table = Toml.ToModel<tomlKeybinds>(File.ReadAllLines(configPath).Join(delimiter:"\n"));
		ParseKeybinds(table);
		StartCoroutine(UpdateCoroutine());
	}
	
	ParsedBind? ParseBind(string bind) 
	{
		KeyCode[] modifiers = [];
		string lower = bind.Trim().ToLower();
		string[] split = lower.Split('+');
		foreach (string comp in split) 
		{
			switch (comp) 
			{
				case "control":
				case "ctrl":
					modifiers = [.. modifiers, KeyCode.LeftControl];
					break;
				case "shift":
					modifiers = [.. modifiers, KeyCode.LeftShift];
					break;
				case "alt":
					modifiers = [.. modifiers, KeyCode.LeftAlt];
					break;
				case "win":
				case "meta":
				case "cmd":
				case "command":
					Logger.LogInfo("Adding LeftCommand to modifiers");
					modifiers = [.. modifiers, KeyCode.LeftCommand];
					break;
				default:
					if (Enum.TryParse(comp, true , out KeyCode code)) 
					{
						return new ParsedBind() 
						{
							modifiers = modifiers,
							key = code
						};
					}
					else 
					{
						Logger.LogError($"Unknown key {comp}");
						return null;
					}
			}
		}
		return null;
	}
	
	void ParseKeybinds(tomlKeybinds table) 
	{
		string[] keybinds = table.keybinds;
		foreach (string keybindString in keybinds) 
		{
			string[] split = keybindString.Split(':');
			ParsedBind? bind = ParseBind(split[0]);
			if (bind.HasValue) 
			{
				KeyboardShortcut shortcut = new(bind.Value.key, bind.Value.modifiers);
				Keybind keybind = new()
				{
					bind = shortcut
				};
				if (split[1] == "unlimited") 
				{
					keybind.fps = -1;
					fpsBinds = [ .. fpsBinds, keybind ];
				}
				else if (int.TryParse(split[1], out keybind.fps)) 
					fpsBinds = [ .. fpsBinds, keybind ];
				else 
					Logger.LogError($"Failed to parse fps string {split[1]} into an integer.");
			}
		}
	}
	
	IEnumerator UpdateCoroutine() 
	{
		while (true) 
		{
			yield return null;
			foreach (Keybind bind in fpsBinds) 
				bind.Check();
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = currentFps;
		}
	}
}	