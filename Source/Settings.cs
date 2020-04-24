using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace CommandPalette
{
	class Settings : ModSettings
	{
		public void DoWindowContents(Rect canvas)
		{
			var options = new Listing_Standard();
			options.Begin(canvas);
			options.End();
		}
		
		public override void ExposeData()
		{
		}
	}
}