using Verse;
using UnityEngine;
using HarmonyLib;

namespace CommandPalette
{
    public class CommandPalette : Mod
    {
        public static Settings Settings { get; private set; }

        public CommandPalette( ModContentPack content ) : base( content )
        {
            // initialize settings
            Settings = GetSettings<Settings>();

#if DEBUG
            Harmony.DEBUG = true;
#endif
            Harmony harmony = new Harmony( "Fluffy.CommandPalette" );
            harmony.PatchAll();
        }

        public override void DoSettingsWindowContents( Rect inRect )
        {
            base.DoSettingsWindowContents( inRect );
            GetSettings<Settings>().DoWindowContents( inRect );
        }

        public override string SettingsCategory()
        {
            return "Command Palette";
        }

    }
}