using BetterKeybinding;
using UnityEngine;
using Verse;

namespace CommandPalette
{
	public class Settings : ModSettings
    {
        public float PaletteScale = 1f;
        public bool OpenArchitect = true;
        private BetterKeybinding.KeyBind _keyBinding;

        public BetterKeybinding.KeyBind KeyBinding
        {
            get
            {
                _keyBinding??= new BetterKeybinding.KeyBind( "Open Command Palette", 1);
                return _keyBinding;
            }
        }

		public void DoWindowContents(Rect canvas)
        {
			var options = new Listing_Standard();
			options.Begin(canvas);
            options.Label( $"Palette scale, lower means smaller buttons ({PaletteScale:P0})" );
            PaletteScale = options.Slider( PaletteScale, 1/3f, 1 );
            options.CheckboxLabeled( "Open Architect window when selecting command", ref OpenArchitect,
                                     "If enabled, when selecting a command in the command palette, the corresponding tab is opened in the Architect menu." );
            var rect = options.GetRect( 30f );

            KeyBinding.Draw( rect, 3 );
            options.End();
		}
		
		public override void ExposeData()
		{
			base.ExposeData();

            Scribe_Values.Look( ref PaletteScale, "paletteScale", 1f  );
			Scribe_Values.Look( ref OpenArchitect, "openArchitect", true  );
            Scribe_Deep.Look( ref _keyBinding, "keybinding" );
        }
	}
}