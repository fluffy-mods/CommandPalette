using BetterKeybinding;
using RimWorld;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using Verse;

namespace CommandPalette
{
    public class Settings : ModSettings
    {
        public float PaletteScale = 1f;
        public int MaxRecentDesignators = 10;
        public bool OpenArchitect = true;
        public bool OpenWithSelection = false;

        public bool CloseAfterSelection = true;
        public bool CloseIfSomethingSelected = true;

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
                _keyBinding ??= new KeyBind("Open Command Palette", 1);
                return _keyBinding;
            }
        }

        public void DoWindowContents(Rect canvas)
        {
            Listing_Standard options = new Listing_Standard();
            options.Begin(canvas);
            options.Label($"Palette scale, lower means smaller buttons ({PaletteScale:P0})");
            PaletteScale = options.Slider(PaletteScale, 1 / 3f, 1);
            options.CheckboxLabeled("Open Architect window when selecting command", ref OpenArchitect,
                                     "If enabled, when selecting a command in the command palette, the corresponding tab is opened in the Architect menu.");

            Rect rect = options.GetRect(30);
            Widgets.Label(rect.LeftPart(2 / 3f), "Number of commands per row");
            Widgets.TextFieldNumeric(rect.RightPart(1 / 3f), ref NumCols, ref _numCols, 2, 10);

            rect = options.GetRect(30);
            Widgets.Label(rect.LeftPart(2 / 3f), "Number of commands per column");
            Widgets.TextFieldNumeric(rect.RightPart(1 / 3f), ref NumRows, ref _numRows, 1, 10);

            KeyBinding.Draw(options.GetRect(30));

            options.CheckboxLabeled("Close after selection", ref CloseAfterSelection, "Close the command pallete after you select a command.");

            // mutually exclusive, cannot both be true 
            // take a peek at current values, we'll resolve conflicts after getting new values
            bool _openWithSelection = OpenWithSelection;
            bool _closeIfSomethingSelected = CloseIfSomethingSelected;

            options.CheckboxLabeled("Allow opening when something is selected", ref _openWithSelection, "By default, the command palette will not open when something is selected. This makes sure that it doesn't conflict with right click menus. Enabling this options overrides that behaviour. \n\nWARNING:\nOnly enable this when you have set a different hotkey!");
            options.CheckboxLabeled("Close if something else is selected", ref _closeIfSomethingSelected, "Close the command pallete if something else was selected through command palette or in any other way.");

            // both are true, what changed?
            if (_openWithSelection && _closeIfSomethingSelected)
            {
                if (!OpenWithSelection)
                {
                    Messages.Message("Not allowed because 'close if something is selected' is active.", MessageTypeDefOf.RejectInput, false);
                }
                else if (!CloseIfSomethingSelected)
                {
                    Messages.Message("Not allowed because 'allow opening when something is selected' is active.", MessageTypeDefOf.RejectInput, false);
                }
                else
                {
                    // should never get here, but just in case return to default settings
                    OpenWithSelection = false;
                    CloseIfSomethingSelected = true;
                }
            }
            else
            {
                OpenWithSelection = _openWithSelection;
                CloseIfSomethingSelected = _closeIfSomethingSelected;
            }
            

            rect = options.GetRect(30);
            Widgets.Label(rect.LeftPart(2 / 3f), "Maximum number of recently used designators shown");
            Widgets.TextFieldNumeric(rect.RightPart(1 / 3f), ref MaxRecentDesignators, ref _maxRecent, 0, NumCols * NumRows);

            options.End();
        }



        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref PaletteScale, "paletteScale", 1f);
            Scribe_Values.Look(ref OpenArchitect, "openArchitect", true);
            Scribe_Values.Look(ref NumCols, "numCols", 4);
            Scribe_Values.Look(ref NumRows, "numRows", 2);
            Scribe_Values.Look(ref MaxRecentDesignators, "maxRecentDesignators", 10);
            Scribe_Values.Look(ref OpenWithSelection, "openWithSelection", false);
            Scribe_Values.Look(ref CloseAfterSelection, "closeAfterSelection", true);
            Scribe_Values.Look(ref CloseIfSomethingSelected, "closeIfSomethingSelected", true);
            Scribe_Deep.Look(ref _keyBinding, "keybinding");
        }
    }
}
