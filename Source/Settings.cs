using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace CommandPalette
{
	public class Settings : ModSettings
    {
        public float PaletteScale = 2 / 3f;
		public void DoWindowContents(Rect canvas)
		{
			var options = new Listing_Standard();
			options.Begin(canvas);
            options.Label( $"Palette scale, lower means smaller buttons ({PaletteScale:P0})" );
            PaletteScale = options.Slider( PaletteScale, 1/3f, 1 );
			options.End();
		}
		
		public override void ExposeData()
		{
			base.ExposeData();

            Scribe_Values.Look( ref PaletteScale, "paletteScale", 2 / 3f  );
        }
	}
}