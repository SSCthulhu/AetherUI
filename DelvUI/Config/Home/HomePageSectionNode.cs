using DelvUI.Config.Home.Widgets;
using DelvUI.Config.Navigation;
using DelvUI.Config.Presets;
using DelvUI.Config.Tree;
using DelvUI.Helpers;
using DelvUI.Interface.GeneralElements;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Config.Home
{
    public class HomePageSectionNode : SectionNode
    {
        private const float ColumnSpacing = 28f;
        private const float GridGap = 8f;
        private const float CategorySpacing = 8f;
        private const float RightColumnCtaSpacing = 14f;
        private const float RightColumnCtaHeight = 72f;
        private const float RightColumnCtaWidthRatio = 0.88f;
        private const float LeftColumnRatio = 0.64f;
        private const float CtaBottomAlignNudge = 8f;

        public HomePageSectionNode()
        {
            Name = NavigationConstants.Home;
        }

        public override bool Draw(ref bool changed, float alpha)
        {
            if (!Selected)
            {
                return false;
            }

            return DrawContent(ref changed);
        }

        private bool DrawContent(ref bool changed)
        {
            if (!ImGui.BeginChild(
                "AetherUI_HomePanel",
                new Vector2(0f, -10f),
                false,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.EndChild();
                return false;
            }

            float contentOriginX = ImGui.GetCursorPosX();
            float contentWidth = ImGui.GetContentRegionAvail().X;

            HomeHeroBanner.Draw(contentWidth);
            ImGui.Spacing();

            ImGui.SetCursorPosX(contentOriginX);
            PresetManager.DrawPresetTabBar(contentWidth);
            ImGui.Spacing();

            float scale = ImGuiHelpers.GlobalScale;
            float scrollHeight = ImGui.GetContentRegionAvail().Y;
            if (scrollHeight < 1f)
            {
                scrollHeight = 1f;
            }

            if (!ImGui.BeginChild("AetherUI_HomePageScroll", new Vector2(0f, scrollHeight), false))
            {
                ImGui.EndChild();
                ImGui.EndChild();
                return false;
            }

            bool didReset = DrawScrollableBody(ref changed, contentWidth, scale);

            ImGui.EndChild();

            ImGui.EndChild();

            return didReset;
        }

        private bool DrawScrollableBody(ref bool changed, float contentWidth, float scale)
        {
            float contentOriginX = ImGui.GetCursorPosX();
            float columnGap = ColumnSpacing * scale;
            float leftSectionWidth = contentWidth * LeftColumnRatio - columnGap * 0.5f;
            float rightSectionWidth = contentWidth - leftSectionWidth - columnGap;
            float rowStartY = ImGui.GetCursorPosY();
            float rightSectionStartX = contentOriginX + leftSectionWidth + columnGap;

            ImGui.SetCursorPos(new Vector2(contentOriginX, rowStartY));
            float quickFeaturesInnerWidth = leftSectionWidth - HomeBorderedSectionScope.SidePadding * scale * 2f;
            using (HomeBorderedSectionScope.Begin("Quick Features", leftSectionWidth))
            {
                changed |= DrawFeatureTileGrid(quickFeaturesInnerWidth);
            }

            float leftBottom = ImGui.GetCursorPosY();

            ImGui.SetCursorPos(new Vector2(rightSectionStartX, rowStartY));
            DrawGlobalStyleSection(rightSectionStartX, rightSectionWidth, ref changed);

            float rightBottom = DrawCtaCluster(rightSectionStartX, rightSectionWidth, scale, leftBottom);

            float contentBottom = Math.Max(leftBottom, rightBottom);
            ImGui.SetCursorPosY(contentBottom);
            ImGui.Dummy(new Vector2(1f, 0f));

            float maxScroll = ImGui.GetScrollMaxY();
            if (ImGui.GetScrollY() > maxScroll)
            {
                ImGui.SetScrollY(maxScroll);
            }

            return changed;
        }

        private static float DrawCtaCluster(float sectionStartX, float sectionWidth, float scale, float alignBottomY)
        {
            float ctaHeight = RightColumnCtaHeight * scale;
            float ctaSpacing = RightColumnCtaSpacing * scale;
            float ctaGap = GridGap * scale;
            float blockHeight = ctaHeight + ctaSpacing + ctaHeight;
            float clusterLayoutHeight = HomeCtaClusterScope.GetTotalLayoutHeight(blockHeight);
            float clusterTopY = alignBottomY - clusterLayoutHeight - CtaBottomAlignNudge * scale;

            ImGui.SetCursorPos(new Vector2(sectionStartX, clusterTopY));

            using (HomeCtaClusterScope scope = HomeCtaClusterScope.Begin(sectionWidth))
            {
                float buttonAreaWidth = scope.InnerWidth;
                float shrunkButtonWidth = buttonAreaWidth * RightColumnCtaWidthRatio;
                float buttonsStartX = ImGui.GetCursorPosX() + (buttonAreaWidth - shrunkButtonWidth) * 0.5f;
                float splitButtonWidth = (shrunkButtonWidth - ctaGap) * 0.5f;
                Vector2 splitCtaSize = new Vector2(splitButtonWidth, ctaHeight);
                Vector2 fullCtaSize = new Vector2(shrunkButtonWidth, ctaHeight);
                float rowTopY = ImGui.GetCursorPosY();

                ImGui.SetCursorPos(new Vector2(buttonsStartX, rowTopY));

                if (HomePrimaryButton.Draw(
                    "Advanced Options",
                    string.Empty,
                    FontAwesomeIcon.SlidersH,
                    splitCtaSize,
                    buttonId: "##homeAdvancedOptions",
                    advanceLayout: false))
                {
                    HomeNavigation.NavigateToAdvancedColors();
                }

                ImGui.SetCursorPos(new Vector2(buttonsStartX + splitButtonWidth + ctaGap, rowTopY));
                if (HomePrimaryButton.Draw(
                    "Profiles/Import",
                    string.Empty,
                    FontAwesomeIcon.FolderOpen,
                    splitCtaSize,
                    buttonId: "##homeProfilesImport",
                    advanceLayout: false))
                {
                    HomeNavigation.NavigateToProfilesImport();
                }

                ImGui.SetCursorPos(new Vector2(buttonsStartX, rowTopY + ctaHeight + ctaSpacing));
                if (HomePrimaryButton.Draw(
                    "Open HUD Editor",
                    "Customize • Position • Style",
                    FontAwesomeIcon.Lock,
                    fullCtaSize,
                    tooltip: "Unlock HUD elements so you can drag and position them on screen.",
                    buttonId: "##homeOpenHudEditor",
                    advanceLayout: false))
                {
                    ConfigurationManager.Instance.LockHUD = false;
                }
            }

            return ImGui.GetCursorPosY();
        }

        private static void DrawGlobalStyleSection(float sectionStartX, float sectionWidth, ref bool changed)
        {
            float labelOffset = GlobalTypographyControls.GetCompactContentStartOffset(sectionWidth);

            using (HomeBorderedSectionScope.Begin("Global Style", sectionWidth, labelOffset))
            {
                FontsConfig? fontsConfig = ConfigurationManager.Instance.GetConfigObject<FontsConfig>();
                HUDOptionsConfig? hudOptionsConfig = ConfigurationManager.Instance.GetConfigObject<HUDOptionsConfig>();
                if (fontsConfig != null && hudOptionsConfig != null)
                {
                    changed |= GlobalTypographyControls.DrawHomePanel(fontsConfig, hudOptionsConfig, ref changed);
                }
            }
        }

        private bool DrawFeatureTileGrid(float gridWidth)
        {
            HomeFeatureSettingsConfig? settings = ConfigurationManager.Instance.GetConfigObject<HomeFeatureSettingsConfig>();
            if (settings == null)
            {
                return false;
            }

            bool synced = FeatureRegistry.SyncHomeSettingsFromConfigs(
                ConfigurationManager.Instance.ConfigBaseNode,
                settings);

            HashSet<FeatureId> changedFeatures = new();
            float scale = ImGuiHelpers.GlobalScale;
            float rowHeight = 48f * scale;
            float categoryGap = CategorySpacing * scale;
            float contentX = ImGui.GetCursorPosX();

            DrawPlayerTargetSection(settings, gridWidth, rowHeight, contentX, changedFeatures);
            ImGui.SetCursorPos(new Vector2(contentX, ImGui.GetCursorPosY() + categoryGap));

            DrawPartyRaidSection(settings, gridWidth, rowHeight, contentX, changedFeatures);
            ImGui.SetCursorPos(new Vector2(contentX, ImGui.GetCursorPosY() + categoryGap));

            DrawUtilitySection(settings, gridWidth, rowHeight, contentX, changedFeatures);

            if (changedFeatures.Count > 0)
            {
                FeatureRegistry.ApplyHomeSettingsToConfigs(
                    ConfigurationManager.Instance.ConfigBaseNode,
                    settings,
                    changedFeatures);
            }

            return synced || changedFeatures.Count > 0;
        }

        private static void DrawPlayerTargetSection(
            HomeFeatureSettingsConfig settings,
            float gridWidth,
            float rowHeight,
            float contentX,
            HashSet<FeatureId> changedFeatures)
        {
            ImGui.SetCursorPos(new Vector2(contentX, ImGui.GetCursorPosY()));
            HomeCategoryHeader.Draw("Player / Target");
            float gridTop = ImGui.GetCursorPosY();

            HomeFeatureGridLayout grid = new HomeFeatureGridLayout(
                gridWidth,
                3,
                GridGap,
                new Vector2(contentX, gridTop));
            float groupedHeight = grid.GetContentHeight(3, rowHeight);

            grid.SetSlot(0, 0, rowHeight);
            DrawFeatureTileAt(settings, FeatureId.PlayerParameterOrb, "Player Orb", grid.GetSlotSize(0, 1, rowHeight), changedFeatures);

            grid.SetSlot(0, 1, rowHeight);
            DrawFeatureTileAt(settings, FeatureId.BuffsAndDebuffs, "Buffs & Debuffs", grid.GetSlotSize(0, 1, rowHeight), changedFeatures);

            grid.SetSlot(0, 2, rowHeight);
            DrawFeatureTileAt(settings, FeatureId.JobSpecificBars, "Job Bars", grid.GetSlotSize(0, 1, rowHeight), changedFeatures);

            grid.SetSlot(1, 0, rowHeight);
            DrawIndividualFramesTile(settings, grid.GetSlotSize(1, 1, groupedHeight), changedFeatures);

            grid.SetSlot(2, 0, rowHeight);
            DrawOtherElementsTile(settings, grid.GetSlotSize(2, 1, groupedHeight), changedFeatures);

            ImGui.SetCursorPos(new Vector2(contentX, gridTop + groupedHeight));
        }

        private static void DrawPartyRaidSection(
            HomeFeatureSettingsConfig settings,
            float gridWidth,
            float rowHeight,
            float contentX,
            HashSet<FeatureId> changedFeatures)
        {
            ImGui.SetCursorPos(new Vector2(contentX, ImGui.GetCursorPosY()));
            HomeCategoryHeader.Draw("Party / Raid");
            float gridTop = ImGui.GetCursorPosY();

            HomeFeatureGridLayout grid = new HomeFeatureGridLayout(
                gridWidth,
                2,
                GridGap,
                new Vector2(contentX, gridTop));

            grid.SetSlot(0, 0, rowHeight);
            DrawFeatureTileAt(settings, FeatureId.Nameplates, "Nameplates", grid.GetSlotSize(0, 1, rowHeight), changedFeatures);

            grid.SetSlot(1, 0, rowHeight);
            DrawFeatureTileAt(settings, FeatureId.PartyFrames, "Party Frames", grid.GetSlotSize(1, 1, rowHeight), changedFeatures);

            grid.SetSlot(0, 1, rowHeight);
            DrawFeatureTileAt(settings, FeatureId.PartyCooldowns, "Party Cooldowns", grid.GetSlotSize(0, 1, rowHeight), changedFeatures);

            grid.SetSlot(1, 1, rowHeight);
            DrawFeatureTileAt(settings, FeatureId.EnemyList, "Enemy List", grid.GetSlotSize(1, 1, rowHeight), changedFeatures);

            ImGui.SetCursorPos(new Vector2(contentX, gridTop + grid.GetContentHeight(2, rowHeight)));
        }

        private static void DrawUtilitySection(
            HomeFeatureSettingsConfig settings,
            float gridWidth,
            float rowHeight,
            float contentX,
            HashSet<FeatureId> changedFeatures)
        {
            float columnWidth = (gridWidth - GridGap) * 0.5f;
            Vector2 tileSize = new Vector2(columnWidth, rowHeight);
            float startY = ImGui.GetCursorPosY();
            float headerHeight = ImGui.GetTextLineHeight() + 2f;
            float tileY = startY + headerHeight + 2f;

            ImGui.SetCursorPos(new Vector2(contentX, startY));
            HomeCategoryHeader.Draw("Minimap");

            ImGui.SetCursorPos(new Vector2(contentX + columnWidth + GridGap, startY));
            HomeCategoryHeader.Draw("Action Camera");

            ImGui.SetCursorPos(new Vector2(contentX, tileY));
            DrawFeatureTileAt(settings, FeatureId.Minimap, "Minimap", tileSize, changedFeatures);

            ImGui.SetCursorPos(new Vector2(contentX + columnWidth + GridGap, tileY));
            DrawFeatureTileAt(settings, FeatureId.ActionCamera, "Action Camera", tileSize, changedFeatures);

            ImGui.SetCursorPos(new Vector2(contentX, tileY + rowHeight));
        }

        private static void DrawFeatureTileAt(
            HomeFeatureSettingsConfig settings,
            FeatureId featureId,
            string title,
            Vector2 tileSize,
            HashSet<FeatureId> changedFeatures)
        {
            bool enabled = FeatureRegistry.IsFeatureEnabled(settings, featureId);
            bool tileChanged = HomeFeatureTile.Draw(
                $"feature_{featureId}",
                HomeFeatureIcons.GetIcon(featureId),
                title,
                ref enabled,
                tileSize,
                HomeNavigation.GetEditTargetForFeature(featureId),
                advanceLayout: false);

            if (tileChanged)
            {
                FeatureRegistry.SetFeatureEnabled(settings, featureId, enabled);
                changedFeatures.Add(featureId);
            }
        }

        private static void DrawIndividualFramesTile(
            HomeFeatureSettingsConfig settings,
            Vector2 tileSize,
            HashSet<FeatureId> changedFeatures)
        {
            bool unitFrames = settings.UnitFrames;
            bool manaBars = settings.ManaBars;
            bool castBars = settings.CastBars;

            bool tileChanged = HomeFeatureTile.DrawIndividualFramesGroup(
                "feature_individual_frames",
                tileSize,
                ref settings.UnitFrames,
                ref settings.ManaBars,
                ref settings.CastBars);

            if (!tileChanged)
            {
                return;
            }

            settings.IndividualFramesMaster = settings.UnitFrames || settings.ManaBars || settings.CastBars;

            if (unitFrames != settings.UnitFrames)
            {
                changedFeatures.Add(FeatureId.UnitFrames);
            }

            if (manaBars != settings.ManaBars)
            {
                changedFeatures.Add(FeatureId.ManaBars);
            }

            if (castBars != settings.CastBars)
            {
                changedFeatures.Add(FeatureId.CastBars);
            }
        }

        private static void DrawOtherElementsTile(
            HomeFeatureSettingsConfig settings,
            Vector2 tileSize,
            HashSet<FeatureId> changedFeatures)
        {
            bool changed = HomeFeatureTile.DrawOtherElementsGroup(
                "feature_other_elements",
                tileSize,
                ref settings.ExperienceBar,
                ref settings.GcdIndicator,
                ref settings.PullTimer,
                ref settings.LimitBreak,
                ref settings.MpTicker);

            if (changed)
            {
                FeatureRegistry.SyncOtherElementsMaster(settings);
                changedFeatures.Add(FeatureId.OtherElements);
            }
        }
    }
}
