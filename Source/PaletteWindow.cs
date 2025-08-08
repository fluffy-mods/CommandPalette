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

namespace CommandPalette
{
    public class PaletteWindow : GameComponent
    {
        public static PaletteWindow Get { get; protected set; }

        public PaletteWindow(Game game)
        {
            Get = this;
        }

        private Vector2 position;
        private bool _active;

        private static string _query = "";

        private enum SearchFieldStatus
        {
            Focus, Clear, Ready
        }
        private SearchFieldStatus _searchFieldStatus = SearchFieldStatus.Focus;

        protected static string Query
        {
            get => _query;
            set
            {
                _query = value;
                _filteredDesignators = null;
            }
        }

        private int Width =>
            (int)(((GIZMO_SIZE * CommandPalette.Settings.NumCols) +
                      (MARGIN * (CommandPalette.Settings.NumCols - 1))) * CommandPalette.Settings.PaletteScale);

        private int PaletteHeight =>
            (int)(((GIZMO_SIZE * CommandPalette.Settings.NumRows) +
                      (MARGIN * (CommandPalette.Settings.NumRows - 1))) * CommandPalette.Settings.PaletteScale);
        private const int MARGIN = 6;
        private const int SEARCH_HEIGHT = 50;
        private const int GIZMO_SIZE = 75;

        private const int FADE_OUT_START_DISTANCE = 10;
        private const int FADE_OUT_FINISH_DISTANCE = 200;

        private static List<Designator> _allDesignators;

        public static List<Designator> AllDesignators
        {
            get
            {
                _allDesignators ??= DefDatabase<DesignationCategoryDef>.AllDefsListForReading
                                                                     .SelectMany(
                                                                          dcd => dcd.ResolvedAllowedDesignators)
                                                                     .Distinct(new DesignatorEqualityComparer())
                                                                     .ToList();
                return _allDesignators;
            }
        }

        private static Dictionary<Designator, ArchitectCategoryTab> _designatorsByCategory;

        private static readonly FieldInfo _architectCategoryTabFieldInfo =
            AccessTools.Field(typeof(MainTabWindow_Architect), "desPanelsCached");

        public static Dictionary<Designator, ArchitectCategoryTab> DesignatorsByCategory
        {
            get
            {
                if (_designatorsByCategory == null)
                {
                    List<ArchitectCategoryTab> categories = _architectCategoryTabFieldInfo.GetValue(MainButtonDefOf.Architect.TabWindow)
                        as List<ArchitectCategoryTab>;
                    _designatorsByCategory = new Dictionary<Designator, ArchitectCategoryTab>();
                    foreach (ArchitectCategoryTab category in categories)
                    {
                        foreach (Designator designator in category.def.ResolvedAllowedDesignators)
                        {
                            if (!_designatorsByCategory.ContainsKey(designator))
                            {
                                _designatorsByCategory.Add(designator, category);
                            }
                        }
                    }
                }

                return _designatorsByCategory;
            }
        }

        private static IEnumerable<Designator> _filteredDesignators;

        public static IEnumerable<Designator> FilteredDesignators
        {
            get
            {
                return _filteredDesignators ??= AllDesignators.Where(d => d.Visible).OrderByDescending(Similarity);
            }
        }

        public static void Notify_SettingsChanged()
        {
            _recentlyUsed.Resize(CommandPalette.Settings.MaxRecentDesignators);
        }
        private static readonly HistoryList<Designator> _recentlyUsed = new HistoryList<Designator>(CommandPalette.Settings.MaxRecentDesignators);
        public static IEnumerable<Designator> VisibleRecentlyUsed => _recentlyUsed.Where(d => d.Visible);

        public static float Similarity(Designator des)
        {
            float name = Similarity(des.Label, Query);
            if (des.Label.ToUpperInvariant().StartsWith(Query.ToUpperInvariant()))
            {
                name *= 4f; // give exact (partial) name matches a much higher weight
            }
            else if (des.Label.ToUpperInvariant().Contains(Query.ToUpperInvariant()))
            {
                name *= 2f;
            }

            float desc = Similarity(des.Desc, Query);
            return Mathf.Max(name, desc);
        }

        public static float Similarity(string a, string b)
        {
            if (a.NullOrEmpty() || b.NullOrEmpty())
            {
                return 0;
            }

            int C = Math.Max(a.Length, b.Length);
            int distance = Levenshtein.CalculateDistance(a, b, 1);
            return 1 - (distance / (float)C);
        }

        public void HandleShortcuts()
        {
            try
            {
                // this is injected to run right before the vanilla LowPriorityShortcuts.
                // if nothing selected and right clicked on colony view.
                if ((CommandPalette.Settings.OpenWithSelection || Find.Selector.NumSelected == 0)
                     && !WorldRendererUtility.WorldRendered
                     && (CommandPalette.Settings.KeyBinding?.JustPressed ?? false))
                {
                    Event.current.Use();
                    
                    if (_active)
                    {
                        Cancel();
                    }
                    else
                    {
                        _active = true;
                        _searchFieldStatus = SearchFieldStatus.Focus;
                        position = UI.MousePositionOnUIInverted - new Vector2(GIZMO_SIZE / 2f, SEARCH_HEIGHT);
                    }
                }
            }
            catch (Exception err)
            {
                Verse.Log.Error(err.ToString());
            }
        }

        public override void GameComponentOnGUI()
        {
            if (_active)
            {
                if (KeyBindingDefOf.Cancel.KeyDownEvent)
                {
                    Event.current.Use();
                    Cancel();
                    return;
                }

                if (CommandPalette.Settings.CloseIfSomethingSelected && Find.Selector.NumSelected > 0)
                {
                    Cancel();
                    return;
                }

                Rect canvas = new Rect(position, new Vector2(Width, SEARCH_HEIGHT + MARGIN + PaletteHeight))
                   .Bounded(new Vector2(UI.screenWidth, UI.screenHeight));

                float fade = GetFadeOut(UI.MousePositionOnUIInverted, canvas);
                if (fade > .95f)
                {
                    Cancel();
                    return;
                }

                GUI.color = new Color(1f, 1f, 1f, 1 - fade);
                Rect searchCanvas = new Rect(canvas.xMin, canvas.yMin, Width, SEARCH_HEIGHT);
                Rect paletteCanvas = new Rect(canvas.xMin, searchCanvas.yMax + MARGIN, Width, PaletteHeight);
                paletteCanvas.position /= CommandPalette.Settings.PaletteScale;
                paletteCanvas.size /= CommandPalette.Settings.PaletteScale;
                DoSearch(searchCanvas);
                DoPalette(paletteCanvas, fade);
                Utilities.ApplyUIScale(Prefs.UIScale);
            }
        }

        public void Cancel()
        {
            _active = false;
            position = Vector2.zero;
            Query = "";
            GUI.color = Color.white;
        }

        public float GetFadeOut(Vector2 pos, Rect canvas)
        {
            if (canvas.Contains(pos))
            {
                return 0f;
            }

            return Mathf.InverseLerp(FADE_OUT_START_DISTANCE, FADE_OUT_FINISH_DISTANCE,
                                      GenUI.DistFromRect(canvas, pos));
        }

        public void DoSearch(Rect canvas)
        {
            Text.Font = GameFont.Medium;
            GUI.SetNextControlName("searchField");
            _query = Widgets.TextField(canvas, Query);
            Text.Font = GameFont.Small;

            // if we focus and clear on the first frame, clearing the value doesn't 
            // seem to take effect. Instead, let's go through this in stages across
            // the first two frames.
            switch (_searchFieldStatus)
            {
                case SearchFieldStatus.Focus:
                    GUI.FocusControl("searchField");
                    _searchFieldStatus = SearchFieldStatus.Clear;
                    break;
                case SearchFieldStatus.Clear:
                    Query = "";
                    _searchFieldStatus = SearchFieldStatus.Ready;
                    break;
                case SearchFieldStatus.Ready:
                    Query = _query;
                    break;
            }
            
        }

        public void DoPalette(Rect canvas, float fade)
        {
            Vector2 pos = canvas.min;
            Color fadeColor = new Color(1f, 1f, 1f, 1 - fade);
            IEnumerable<Designator> designators = Query.NullOrEmpty() ? VisibleRecentlyUsed : FilteredDesignators;
            GizmoRenderParms parms = new();

            Designator _interacted = null;
            GizmoResult _interactionResult = new();

            Utilities.ApplyUIScale(Prefs.UIScale * CommandPalette.Settings.PaletteScale);
            foreach (Designator designator in designators)
            {
                // add 10 px of wiggle room for rounding errors
                if (pos.x + GIZMO_SIZE > canvas.xMax + 10)
                {
                    pos.x = canvas.xMin;
                    pos.y += GIZMO_SIZE + MARGIN;
                }

                if (pos.y + GIZMO_SIZE > canvas.yMax + 10)
                {
                    break;
                }

                Color iconColor = designator.defaultIconColor;
                designator.defaultIconColor = fadeColor;
                GUI.color = fadeColor;
                GizmoResult result = designator.GizmoOnGUI(pos, GIZMO_SIZE, parms);
                designator.defaultIconColor = iconColor;
                pos.x += GIZMO_SIZE + MARGIN;


                if (result.State >= GizmoState.Mouseover)
                {
                    _interacted = designator;
                    _interactionResult = result;
                }

            }
            
            Utilities.ApplyUIScale(Prefs.UIScale);

            if (_interacted != null)
            {
                switch (_interactionResult.State)
                {
                    case GizmoState.Interacted:
                    case GizmoState.OpenedFloatMenu when _interacted.RightClickFloatMenuOptions.FirstOrDefault() == null:
                        Select(_interacted, _interactionResult);
                        break;
                    case GizmoState.OpenedFloatMenu:
                        Find.WindowStack.Add(new FloatMenu(_interacted.RightClickFloatMenuOptions.ToList()));
                        break;
                }
            }
        }

        public void Select(Designator designator, GizmoResult result)
        {
            if (CommandPalette.Settings.OpenArchitect
              && MainButtonDefOf.Architect.TabWindow is MainTabWindow_Architect architectTab
              && DesignatorsByCategory.TryGetValue(designator, out ArchitectCategoryTab tab))
            {
                Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Architect, false);
                architectTab.selectedDesPanel = tab;
            }
            _recentlyUsed.Add(designator);
            designator.ProcessInput(result.InteractEvent);

            if (CommandPalette.Settings.CloseAfterSelection)
            {
                Cancel();
            }
        }
    }
}
