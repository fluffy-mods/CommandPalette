// MainTabsRoot_HandleLowPriorityShortcuts.cs
// Copyright Karel Kroeze, -2020

using HarmonyLib;
using RimWorld;

namespace CommandPalette
{
    [HarmonyPatch( typeof( MainTabsRoot ), nameof( MainTabsRoot.HandleLowPriorityShortcuts ) )]
    public static class MainTabsRoot_HandleLowPriorityShortcuts
    {
        public static void Prefix()
        {
            PaletteWindow.Get.HandleShortcuts();
        }
    }
}