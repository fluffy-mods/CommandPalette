// PaletteWindow.cs
// Copyright Karel Kroeze, 2020-2020

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MinimumEditDistance;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace CommandPalette
{
    public class PaletteWindow : GameComponent
    {
        public static PaletteWindow Get { get; protected set; }

        public PaletteWindow( Game game )
        {
            Get = this;
        }

        private Vector2 position;
        private bool    active;
        private bool    setFocus;

        private static string _query = "";

        protected static string query
        {
            get => _query;
            set
            {
                _query               = value;
                _filteredDesignators = null;
            }
        }

        private const int WIDTH          = GIZMO_SIZE * 4 + MARGIN * 3;
        private const int MARGIN         = 6;
        private const int SEARCH_HEIGHT  = 50;
        private const int GIZMO_SIZE     = 75;
        private const int PALETTE_HEIGHT = GIZMO_SIZE * 2 + MARGIN;

        private const int FADE_OUT_START_DISTANCE  = 10;
        private const int FADE_OUT_FINISH_DISTANCE = 200;

        private static List<Designator> _allDesignators;

        public static List<Designator> AllDesignators
        {
            get
            {
                _allDesignators??=DefDatabase<DesignationCategoryDef>.AllDefsListForReading
                                                                     .SelectMany(
                                                                          dcd => dcd.ResolvedAllowedDesignators )
                                                                     .Distinct( new DesignatorEqualityComparer() )
                                                                     .ToList();
                return _allDesignators;
            }
        }

        private static Dictionary<Designator, ArchitectCategoryTab> _designatorsByCategory;

        private static FieldInfo _architectCategoryTabFieldInfo =
            AccessTools.Field( typeof( MainTabWindow_Architect ), "desPanelsCached" );

        public static Dictionary<Designator, ArchitectCategoryTab> DesignatorsByCategory
        {
            get
            {
                if ( _designatorsByCategory == null )
                {
                    var categories = _architectCategoryTabFieldInfo
                           .GetValue( MainButtonDefOf.Architect.TabWindow )
                        as List<ArchitectCategoryTab>;
                    _designatorsByCategory = DefDatabase<DesignationCategoryDef>.AllDefsListForReading.SelectMany(
                        catDef =>
                        {
                            var cat = categories.Find(
                                c => c.def == catDef );
                            return catDef
                                  .ResolvedAllowedDesignators
                                  .Select(
                                       des => (
                                           cat, des ) );
                        } ).ToDictionary(
                        k => k.des, v => v.cat );
                }

                return _designatorsByCategory;
            }
        }

        private static IEnumerable<Designator> _filteredDesignators;

        public static IEnumerable<Designator> FilteredDesignators
        {
            get
            {
                _filteredDesignators??=AllDesignators.OrderByDescending( Similarity );
                return _filteredDesignators.Where( d => d.Visible );
            }
        }

        private static HistoryList<Designator> _recentlyUsed = new HistoryList<Designator>( 10 );
        public static  IEnumerable<Designator> VisibleRecentlyUsed => _recentlyUsed.Where( d => d.Visible );

        public static float Similarity( Designator des )
        {
            var name = Similarity( des.Label, query );
            if ( des.Label.ToUpperInvariant().Contains( query.ToUpperInvariant() ) )
                name *= 3f; // give exact (partial) name matches a much higher weight
            var desc = Similarity( des.Desc, query );
            return Mathf.Max( name, desc );
        }

        public static float Similarity( string a, string b )
        {
            if ( a.NullOrEmpty() || b.NullOrEmpty() )
                return 0;
            var C        = Math.Max( a.Length, b.Length );
            var distance = Levenshtein.CalculateDistance( a, b, 1 );
            return 1 - distance / (float) C;
        }

        public void HandleShortcuts()
        {
            try
            {
                // this is injected to run right before the vanilla LowPriorityShortcuts.
                // if nothing selected and right clicked on colony view.
                if ( Find.Selector.NumSelected == 0 && !WorldRendererUtility.WorldRenderedNow
                                                    && ( CommandPalette.Settings.KeyBinding?.JustPressed ?? false ) )
                {
                    Event.current.Use();
                    position = Utilities.MousePositionOnUIScaledBeforeScaling -
                               new Vector2( GIZMO_SIZE / 2f, SEARCH_HEIGHT );
                    active   = true;
                    setFocus = true;
                }
            }
            catch ( Exception err )
            {
                Verse.Log.Error( err.ToString() );
            }
        }

        public override void GameComponentOnGUI()
        {
            if ( active )
            {
                Utilities.ApplyUIScale( Prefs.UIScale * CommandPalette.Settings.PaletteScale );

                var canvas = new Rect( position, new Vector2( WIDTH, SEARCH_HEIGHT + MARGIN + PALETTE_HEIGHT ) )
                   .Bounded( new Vector2( UI.screenWidth, UI.screenHeight ) );

                var fade = GetFadeOut( Utilities.MousePositionOnUIScaled, canvas );
                if ( fade > .95f || KeyBindingDefOf.Cancel.KeyDownEvent )
                {
                    Cancel();
                    return;
                }

                GUI.color = new Color( 1f, 1f, 1f, 1 - fade );
                var searchCanvas  = new Rect( canvas.xMin, canvas.yMin, WIDTH, SEARCH_HEIGHT );
                var paletteCanvas = new Rect( canvas.xMin, searchCanvas.yMax + MARGIN, WIDTH, PALETTE_HEIGHT );
                DoSearch( searchCanvas );
                DoPalette( paletteCanvas, fade );

                Utilities.ApplyUIScale( Prefs.UIScale );
            }
        }

        public void Cancel()
        {
            active    = false;
            position  = Vector2.zero;
            query     = "";
            GUI.color = Color.white;
        }

        public float GetFadeOut( Vector2 pos, Rect canvas )
        {
            if ( canvas.Contains( pos ) ) return 0f;
            return Mathf.InverseLerp( FADE_OUT_START_DISTANCE, FADE_OUT_FINISH_DISTANCE,
                                      GenUI.DistFromRect( canvas, pos ) );
        }

        public void DoSearch( Rect canvas )
        {
            Text.Font = GameFont.Medium;
            GUI.SetNextControlName( "searchField" );
            query = Widgets.TextField( canvas, query );
            if ( setFocus )
            {
                setFocus = false;
                GUI.FocusControl( "searchField" );
            }

            Text.Font = GameFont.Small;
        }

        public void DoPalette( Rect canvas, float fade )
        {
            var pos         = canvas.min;
            var fadeColor   = new Color( 1f, 1f, 1f, 1 - fade );
            var designators = query.NullOrEmpty() ? VisibleRecentlyUsed : FilteredDesignators;
            foreach ( var designator in designators )
            {
                if ( pos.x + GIZMO_SIZE > canvas.xMax )
                {
                    pos.x =  canvas.xMin;
                    pos.y += GIZMO_SIZE + MARGIN;
                }

                if ( pos.y + GIZMO_SIZE > canvas.yMax )
                    break;

                var iconColor = designator.defaultIconColor;
                designator.defaultIconColor = fadeColor;
                GUI.color                   = fadeColor;
                var result = designator.GizmoOnGUI( pos, GIZMO_SIZE );
                designator.defaultIconColor =  iconColor;
                pos.x                       += GIZMO_SIZE + MARGIN;
                switch ( result.State )
                {
                    case GizmoState.Interacted:
                    case GizmoState.OpenedFloatMenu when designator.RightClickFloatMenuOptions.FirstOrDefault() == null:
                        Select( designator, result );
                        return;
                    case GizmoState.OpenedFloatMenu when designator.RightClickFloatMenuOptions.FirstOrDefault() != null:
                        Find.WindowStack.Add( new FloatMenu( designator.RightClickFloatMenuOptions.ToList() ) );
                        return;
                }
            }
        }

        public void Select( Designator designator, GizmoResult result )
        {
            if ( CommandPalette.Settings.OpenArchitect
              && MainButtonDefOf.Architect.TabWindow is MainTabWindow_Architect architectTab
              && DesignatorsByCategory.TryGetValue( designator, out ArchitectCategoryTab tab ) )
            {
                Find.MainTabsRoot.SetCurrentTab( MainButtonDefOf.Architect, false );
                architectTab.selectedDesPanel = tab;
            }
            _recentlyUsed.Add( designator );
            designator.ProcessInput( result.InteractEvent );
        }
    }
}