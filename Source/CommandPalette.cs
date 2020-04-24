using System.Reflection;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using HarmonyLib;

namespace CommandPalette
{
	public class Mod : Verse.Mod
	{
		public static Settings Settings { get; private set; }
		public Mod(ModContentPack content) : base(content)
		{
			// initialize settings
			Settings = GetSettings<Settings>();


#if DEBUG
			Harmony.DEBUG = true;
#endif
			Harmony harmony = new Harmony("Fluffy.CommandPalette");
			harmony.PatchAll();

		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			GetSettings<Settings>().DoWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "Command Palette";
		}

	}
}