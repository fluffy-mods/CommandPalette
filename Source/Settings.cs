using BetterKeybinding;
using UnityEngine;
using Verse;

namespace CommandPalette
{
	public class Settings : ModSettings
    {
        public float PaletteScale = 1f;
        public int MaxRecentDesignators = 10;
        public bool OpenArchitect = true;
        public int NumRows = 2;
        public int NumCols = 4;
        private KeyBind _keyBinding;
        private string _numRows;
        private string _numCols;
        private string _maxRecent;

        public KeyBind KeyBinding
        {
            get
            {
                _keyBinding??= new KeyBind( "Open Command Palette", 1);
                return _keyBinding;
            }
        }

        public void DoWindowContents( Rect canvas )
        {
            var options = new Listing_Standard();
            options.Begin( canvas );
            options.Label( $"Palette scale, lower means smaller buttons ({PaletteScale:P0})" );
            PaletteScale = options.Slider( PaletteScale, 1 / 3f, 1 );
            options.CheckboxLabeled( "Open Architect window when selecting command", ref OpenArchitect,
                                     "If enabled, when selecting a command in the command palette, the corresponding tab is opened in the Architect menu." );

            var rect = options.GetRect( 30 );
            Widgets.Label( rect.LeftPart( 2             / 3f ), "Number of commands per row" );
            Widgets.TextFieldNumeric( rect.RightPart( 1 / 3f ), ref NumCols, ref _numCols, 2, 10 );

            rect = options.GetRect( 30 );
            Widgets.Label( rect.LeftPart( 2             / 3f ), "Number of commands per column" );
            Widgets.TextFieldNumeric( rect.RightPart( 1 / 3f ), ref NumRows, ref _numRows, 1, 10 );
            
            KeyBinding.Draw( options.GetRect( 30 ) );

            rect = options.GetRect(30);
            Widgets.Label(rect.LeftPart(2 / 3f), "Maximum number of recently used designators shown");
            Widgets.TextFieldNumeric(rect.RightPart(1 / 3f), ref MaxRecentDesignators, ref _maxRecent, 0, NumCols * NumRows);
            
            options.End();
        }

        public override void ExposeData()
		{
			base.ExposeData();

            Scribe_Values.Look( ref PaletteScale, "paletteScale", 1f  );
			Scribe_Values.Look( ref OpenArchitect, "openArchitect", true  );
            Scribe_Values.Look( ref NumCols, "numCols", 4 );
            Scribe_Values.Look( ref NumRows, "numRows", 2 );
            Scribe_Values.Look(ref MaxRecentDesignators, "maxRecentDesignators", 10);
            Scribe_Deep.Look( ref _keyBinding, "keybinding" );
        }
	}
}