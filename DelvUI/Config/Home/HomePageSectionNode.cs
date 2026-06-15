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
        private const float CtaClusterPadding = 12f;
        private const float LeftColumnRatio = 0.64f;

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

            ImGui.NewLine();

            if (!ImGui.BeginChild("AetherUI_HomePage", new Vector2(0, -10), false))
            {
                return false;
            }

            bool didReset = DrawContent(ref changed);
            ImGui.EndChild();

            return didReset | changed;
        }

        private bool DrawContent(ref bool changed)
        {
            float contentOriginX = ImGui.GetCursorPosX();
            float contentWidth = ImGui.GetContentRegionAvail().X;

            HomeHeroBanner.Draw(contentWidth);
            ImGui.Spacing();

            ImGui.SetCursorPosX(contentOriginX);
            PresetManager.DrawPresetTabBar(contentWidth);
            ImGui.Spacing();

            float scale = ImGuiHelpers.GlobalScale;
            float columnGap = ColumnSpacing * scale;
            float leftSectionWidth = contentWidth * LeftColumnRatio - columnGap * 0.5f;
            float rightSectionWidth = contentWidth - leftSectionWidth - columnGap;
            float rowStartY = ImGui.GetCursorPosY();

            float rightSectionStartX = contentOriginX + leftSectionWidth + columnGap;

            ImGui.SetCursorPos(new Vector2(contentOriginX, rowStartY));
            float quickFeaturesInnerWidth = leftSectionWidth - HomeBorderedSectionScope.SidePadding * scale * 2f;
            using (HomeBorderedSectionScope quickFeaturesSection = HomeBorderedSectionScope.Begin("Quick Features", leftSectionWidth))
            {
                changed |= DrawFeatureTileGrid(quickFeaturesInnerWidth);
            }

            float quickFeaturesBorderBottom = ImGui.GetCursorPosY() - HomeBorderedSectionScope.BottomAdvance * scale;

            ImGui.SetCursorPos(new Vector2(rightSectionStartX, rowStartY));
            DrawRightColumn(rightSectionStartX, rightSectionWidth, quickFeaturesBorderBottom, ref changed);

            float rightSectionBottom = ImGui.GetCursorPosY();
            ImGui.SetCursorPosY(Math.Max(quickFeaturesBorderBottom + HomeBorderedSectionScope.BottomAdvance * scale, rightSectionBottom));

            return changed;
        }

        private void DrawRightColumn(float sectionStartX, float sectionWidth, float targetBottomY, ref bool changed)
        {
            float labelOffset = GlobalTypographyControls.GetCompactContentStartOffset(sectionWidth);

            using (HomeBorderedSectionScope.Begin("Global Style", sectionWidth, labelOffset))
            {
                FontsConfig? fontsConfig = ConfigurationManager.Instance.GetConfigObject<FontsConfig>();
                if (fontsConfig != null)
                {
                    changed |= GlobalTypographyControls.DrawHomePanel(fontsConfig, ref changed);
                }
            }

            float afterGlobalStyleY = ImGui.GetCursorPosY();
            float scale = ImGuiHelpers.GlobalScale;
            float clusterTopGap = RightColumnCtaSpacing * scale;
            float ctaSpacing = RightColumnCtaSpacing * scale;
            float ctaHeight = RightColumnCtaHeight * scale;
            float ctaGap = GridGap * scale;
            float clusterTopY = afterGlobalStyleY + clusterTopGap;
            float clusterBottomY = targetBottomY;
            float clusterBoxHeight = Math.Max(0f, clusterBottomY - clusterTopY);
            float clusterWidth = sectionWidth;

            float buttonAreaWidth = clusterWidth - CtaClusterPadding * scale * 2f;
            float shrunkButtonWidth = buttonAreaWidth * RightColumnCtaWidthRatio;
            float buttonsStartX = sectionStartX + (clusterWidth - shrunkButtonWidth) * 0.5f;
            float splitButtonWidth = (shrunkButtonWidth - ctaGap) * 0.5f;
            Vector2 splitCtaSize = new Vector2(splitButtonWidth, ctaHeight);
            Vector2 fullCtaSize = new Vector2(shrunkButtonWidth, ctaHeight);

            float buttonsBlockHeight = ctaHeight + ctaSpacing + ctaHeight;
            float buttonsBlockTopY = clusterTopY + Math.Max(0f, (clusterBoxHeight - buttonsBlockHeight) * 0.5f);
            float topRowY = buttonsBlockTopY;
            float hudRowY = buttonsBlockTopY + ctaHeight + ctaSpacing;

            ImGui.SetCursorPos(new Vector2(sectionStartX, clusterTopY));
            ImGui.PushID("homeCtaCluster");
            ImGui.BeginGroup();
            ImGui.Dummy(new Vector2(clusterWidth, clusterBoxHeight));

            ImGui.SetCursorPos(new Vector2(buttonsStartX, topRowY));
            if (HomePrimaryButton.Draw(
                "Advanced Options",
                string.Empty,
                FontAwesomeIcon.SlidersH,
                splitCtaSize,
                buttonId: "##homeAdvancedOptions"))
            {
                HomeNavigation.NavigateToAdvancedColors();
            }

            ImGui.SetCursorPos(new Vector2(buttonsStartX + splitButtonWidth + ctaGap, topRowY));
            if (HomePrimaryButton.Draw(
                "Profiles/Import",
                string.Empty,
                FontAwesomeIcon.FolderOpen,
                splitCtaSize,
                buttonId: "##homeProfilesImport"))
            {
                HomeNavigation.NavigateToProfilesImport();
            }

            ImGui.SetCursorPos(new Vector2(buttonsStartX, hudRowY));
            if (HomePrimaryButton.Draw(
                "Open HUD Editor",
                "Customize • Position • Style",
                FontAwesomeIcon.Lock,
                fullCtaSize,
                tooltip: "Unlock HUD elements so you can drag and position them on screen.",
                buttonId: "##homeOpenHudEditor"))
            {
                ConfigurationManager.Instance.LockHUD = false;
            }

            ImGui.EndGroup();
            ImGui.PopID();

            if (clusterBoxHeight > 1f)
            {
                Vector2 borderMin = HomeCtaClusterBorder.GetOuterMin(sectionStartX, clusterTopY);
                Vector2 borderMax = HomeCtaClusterBorder.GetOuterMax(sectionStartX, clusterBottomY, clusterWidth);
                HomeCtaClusterBorder.Draw(borderMin, borderMax);
            }

            ImGui.SetCursorPos(new Vector2(sectionStartX, clusterBottomY + HomeBorderedSectionScope.BottomAdvance * scale));
        }

        private bool DrawFeatureTileGrid(float gridWidth)
        {
            HomeFeatureSettingsConfig? settings = ConfigurationManager.Instance.GetConfigObject<HomeFeatureSettingsConfig>();
            if (settings == null)
            {
                return false;
            }

            bool changed = false;
            float scale = ImGuiHelpers.GlobalScale;
            float rowHeight = 48f * scale;
            float categoryGap = CategorySpacing * scale;
            float contentX = ImGui.GetCursorPosX();

            changed |= DrawPlayerTargetSection(settings, gridWidth, rowHeight, contentX);
            ImGui.SetCursorPos(new Vector2(contentX, ImGui.GetCursorPosY() + categoryGap));

            changed |= DrawPartyRaidSection(settings, gridWidth, rowHeight, contentX);
            ImGui.SetCursorPos(new Vector2(contentX, ImGui.GetCursorPosY() + categoryGap));

            changed |= DrawUtilitySection(settings, gridWidth, rowHeight, contentX);

            if (changed)
            {
                FeatureRegistry.ApplyHomeSettingsToConfigs(ConfigurationManager.Instance.ConfigBaseNode, settings);
            }

            return changed;
        }

        private static bool DrawPlayerTargetSection(
            HomeFeatureSettingsConfig settings,
            float gridWidth,
            float rowHeight,
            float contentX)
        {
            ImGui.SetCursorPos(new Vector2(contentX, ImGui.GetCursorPosY()));
            HomeCategoryHeader.Draw("Player / Target");
            float gridTop = ImGui.GetCursorPosY();

            bool changed = false;
            HomeFeatureGridLayout grid = new HomeFeatureGridLayout(
                gridWidth,
                3,
                GridGap,
                new Vector2(contentX, gridTop));
            float groupedHeight = grid.GetContentHeight(3, rowHeight);

            grid.SetSlot(0, 0, rowHeight);
            changed |= DrawFeatureTileAt(settings, FeatureId.PlayerParameterOrb, "Player Orb", grid.GetSlotSize(0, 1, rowHeight));

            grid.SetSlot(0, 1, rowHeight);
            changed |= DrawFeatureTileAt(settings, FeatureId.BuffsAndDebuffs, "Buffs & Debuffs", grid.GetSlotSize(0, 1, rowHeight));

            grid.SetSlot(0, 2, rowHeight);
            changed |= DrawFeatureTileAt(settings, FeatureId.JobSpecificBars, "Job Bars", grid.GetSlotSize(0, 1, rowHeight));

            grid.SetSlot(1, 0, rowHeight);
            changed |= DrawIndividualFramesTile(settings, grid.GetSlotSize(1, 1, groupedHeight));

            grid.SetSlot(2, 0, rowHeight);
            changed |= DrawOtherElementsTile(settings, grid.GetSlotSize(2, 1, groupedHeight));

            ImGui.SetCursorPos(new Vector2(contentX, gridTop + groupedHeight));

            return changed;
        }

        private static bool DrawPartyRaidSection(
            HomeFeatureSettingsConfig settings,
            float gridWidth,
            float rowHeight,
            float contentX)
        {
            ImGui.SetCursorPos(new Vector2(contentX, ImGui.GetCursorPosY()));
            HomeCategoryHeader.Draw("Party / Raid");
            float gridTop = ImGui.GetCursorPosY();

            bool changed = false;
            HomeFeatureGridLayout grid = new HomeFeatureGridLayout(
                gridWidth,
                2,
                GridGap,
                new Vector2(contentX, gridTop));

            grid.SetSlot(0, 0, rowHeight);
            changed |= DrawFeatureTileAt(settings, FeatureId.Nameplates, "Nameplates", grid.GetSlotSize(0, 1, rowHeight));

            grid.SetSlot(1, 0, rowHeight);
            changed |= DrawFeatureTileAt(settings, FeatureId.PartyFrames, "Party Frames", grid.GetSlotSize(1, 1, rowHeight));

            grid.SetSlot(0, 1, rowHeight);
            changed |= DrawFeatureTileAt(settings, FeatureId.PartyCooldowns, "Party Cooldowns", grid.GetSlotSize(0, 1, rowHeight));

            grid.SetSlot(1, 1, rowHeight);
            changed |= DrawFeatureTileAt(settings, FeatureId.EnemyList, "Enemy List", grid.GetSlotSize(1, 1, rowHeight));

            ImGui.SetCursorPos(new Vector2(contentX, gridTop + grid.GetContentHeight(2, rowHeight)));

            return changed;
        }

        private static bool DrawUtilitySection(
            HomeFeatureSettingsConfig settings,
            float gridWidth,
            float rowHeight,
            float contentX)
        {
            bool changed = false;
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
            changed |= DrawFeatureTileAt(settings, FeatureId.Minimap, "Minimap", tileSize);

            ImGui.SetCursorPos(new Vector2(contentX + columnWidth + GridGap, tileY));
            changed |= DrawFeatureTileAt(settings, FeatureId.ActionCamera, "Action Camera", tileSize);

            ImGui.SetCursorPos(new Vector2(contentX, tileY + rowHeight));

            return changed;
        }

        private static bool DrawFeatureTileAt(
            HomeFeatureSettingsConfig settings,
            FeatureId featureId,
            string title,
            Vector2 tileSize)
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
            }

            return tileChanged;
        }

        private static bool DrawIndividualFramesTile(
            HomeFeatureSettingsConfig settings,
            Vector2 tileSize)
        {
            bool tileChanged = HomeFeatureTile.DrawIndividualFramesGroup(
                "feature_individual_frames",
                tileSize,
                ref settings.UnitFrames,
                ref settings.ManaBars,
                ref settings.CastBars);

            if (tileChanged)
            {
                settings.IndividualFramesMaster = settings.UnitFrames || settings.ManaBars || settings.CastBars;
            }

            return tileChanged;
        }

        private static bool DrawOtherElementsTile(
            HomeFeatureSettingsConfig settings,
            Vector2 tileSize)
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
            }

            return changed;
        }
    }
}
