using Dalamud.Bindings.ImGui;

using Dalamud.Interface;

using Dalamud.Interface.Textures.TextureWraps;

using Dalamud.Interface.Utility;

using DelvUI.Config.Attributes;

using DelvUI.Config.Home;

using DelvUI.Config.Home.Widgets;

using DelvUI.Config.Navigation;

using DelvUI.Config.Presets;

using DelvUI.Config.Windows;

using DelvUI.Helpers;

using System;

using System.Collections.Generic;

using System.Linq;

using System.Numerics;



namespace DelvUI.Config.Tree

{

    public delegate void ConfigObjectResetEventHandler(BaseNode sender);



    public class BaseNode : Node

    {

        private static readonly Vector4 AetherAccent = new(0f / 255f, 162f / 255f, 252f / 255f, 1f);

        public event ConfigObjectResetEventHandler? ConfigObjectResetEvent;



        private Dictionary<Type, ConfigPageNode> _configPageNodesMap;



        public bool NeedsSave = false;

        public string? SelectedOptionName = NavigationConstants.Home;



        private readonly HomePageSectionNode _homeNode = new();

        private readonly ProfilesImportSectionNode _profilesImportNode = new();

        private List<SectionNode> _featureSections = new();

        private List<SectionNode> _advancedSections = new();

        public IReadOnlyCollection<Node> Sections
        {
            get
            {
                CreateNodesIfNeeded();
                List<Node> nodes = new List<Node> { _homeNode };
                nodes.AddRange(_featureSections);
                nodes.AddRange(_advancedSections);
                nodes.Add(_profilesImportNode);
                return nodes.AsReadOnly();
            }
        }

        private const float SidebarWidth = 248f;

        private const float SidebarLogoPadding = 2f;

        private const float SidebarLogoTopSpacing = 4f;

        private const float SidebarLogoMaxHeight = 225f;

        private const float SidebarLogoBottomSpacing = 12f;

        private bool _featuresSectionExpanded = true;

        private bool _advancedSectionExpanded = true;

        private bool _profilesSectionExpanded = true;

        private bool _playerTargetNavExpanded = true;

        private bool _partyRaidNavExpanded = true;

        private bool _individualFramesNavExpanded = true;

        private float _scale => ImGuiHelpers.GlobalScale;



        public BaseNode()

        {

            _configPageNodesMap = new Dictionary<Type, ConfigPageNode>();

        }



        public void AddExtraSectionNode(SectionNode node)

        {

            // Profiles are surfaced through ProfilesImportSectionNode.

        }



        public T? GetConfigObject<T>() where T : PluginConfigObject

        {

            var pageNode = GetConfigPageNode<T>();



            return pageNode != null ? (T)pageNode.ConfigObject : null;

        }



        public void RemoveConfigObject<T>() where T : PluginConfigObject

        {

            if (_configPageNodesMap.ContainsKey(typeof(T)))

            {

                _configPageNodesMap.Remove(typeof(T));

            }

        }



        public ConfigPageNode? GetConfigPageNode<T>() where T : PluginConfigObject

        {

            if (_configPageNodesMap.TryGetValue(typeof(T), out var node))

            {

                return node;

            }



            var configPageNode = GetOrAddConfig<T>();



            if (configPageNode != null && configPageNode.ConfigObject != null)

            {

                _configPageNodesMap.Add(typeof(T), configPageNode);



                return configPageNode;

            }



            return null;

        }



        public void SetConfigPageNode(ConfigPageNode configPageNode)

        {

            if (configPageNode.ConfigObject == null)

            {

                return;

            }



            _configPageNodesMap[configPageNode.ConfigObject.GetType()] = configPageNode;

        }



        public bool SetConfigObject(PluginConfigObject configObject)

        {

            if (_configPageNodesMap.TryGetValue(configObject.GetType(), out ConfigPageNode? configPageNode))

            {

                configPageNode.ConfigObject = configObject;

                return true;

            }



            return false;

        }



        private bool PushStyles()

        {

            ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 1);

            ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 1);

            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 1);

            ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 1);

            ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 1);

            ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 10);



            if (ConfigurationManager.Instance.OverrideDalamudStyle)

            {

                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(46f / 255f, 45f / 255f, 46f / 255f, 1f));

                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, .2f));

                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, .2f));



                ImGui.PushStyleColor(ImGuiCol.Separator, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, .4f));



                ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, new Vector4(20f / 255f, 21f / 255f, 20f / 255f, .7f));

                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, .7f));

                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, .7f));

                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, .7f));



                ImGui.PushStyleColor(ImGuiCol.Tab, new Vector4(46f / 255f, 45f / 255f, 46f / 255f, 1f));

                ImGui.PushStyleColor(ImGuiCol.TabActive, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, .7f));

                ImGui.PushStyleColor(ImGuiCol.TabHovered, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, .2f));

                ImGui.PushStyleColor(ImGuiCol.TabUnfocused, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, .2f));



                ImGui.PushStyleColor(ImGuiCol.Border, AetherAccent);

                ImGui.PushStyleColor(ImGuiCol.CheckMark, AetherAccent);



                ImGui.PushStyleColor(ImGuiCol.TableBorderStrong, AetherAccent);

                ImGui.PushStyleColor(ImGuiCol.TableBorderLight, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, .4f));

                ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, .2f));



                return true;

            }



            return false;

        }



        private void PopStyles(bool popColors)

        {

            if (popColors)

            {

                ImGui.PopStyleColor(17);

            }



            ImGui.PopStyleVar(6);

        }



        public void CreateNodesIfNeeded()

        {

            if (_featureSections.Count > 0)

            {

                return;

            }



            foreach (SectionNode sectionNode in _children)

            {

                if (FeatureRegistry.IsNavExcludedSection(sectionNode.Name))

                {

                    if (FeatureRegistry.IsAdvancedSection(sectionNode.Name))

                    {

                        _advancedSections.Add(sectionNode);

                    }



                    continue;

                }



                if (sectionNode.Name == NavigationConstants.Home)

                {

                    continue;

                }



                _featureSections.Add(sectionNode);

            }

        }



        public void RefreshSelectedNode()

        {

            _homeNode.Selected = SelectedOptionName == NavigationConstants.Home;

            _profilesImportNode.Selected = SelectedOptionName == NavigationConstants.ProfilesImport;



            foreach (SectionNode node in _featureSections)

            {

                node.Selected = node.Name == SelectedOptionName;

            }



            foreach (SectionNode node in _advancedSections)

            {

                node.Selected = node.Name == SelectedOptionName;

            }

        }



        public void Draw(float alpha)

        {

            CreateNodesIfNeeded();



            bool changed = false;

            bool didReset = false;



            bool popColors = PushStyles();



            EnsureValidSelection();



            ImGui.BeginGroup();

            {

                ImGui.BeginGroup();

                {

                    ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 0f);

                    if (ImGui.BeginChild("left pane", new Vector2(SidebarWidth * _scale, -10), false, ImGuiWindowFlags.NoScrollbar))

                    {

                        ImGui.PushStyleColor(ImGuiCol.ChildBg, ImGui.ColorConvertFloat4ToU32(HomeUiStyle.PanelBg));

                        DrawSidebarLogo();

                        float navHeight = ImGui.GetContentRegionAvail().Y;

                        if (navHeight > 0f && ImGui.BeginChild("left pane nav", new Vector2(0f, navHeight), false))

                        {

                            didReset |= DrawNavigation(ref changed);

                            ImGui.EndChild();

                        }



                        ImGui.PopStyleColor();

                    }



                    ImGui.EndChild();

                    ImGui.PopStyleVar();

                }



                ImGui.EndGroup();



                didReset |= DrawResetModal();



                ImGui.SameLine();



                ImGui.BeginGroup();

                {

                    RefreshSelectedNode();

                    didReset |= _homeNode.Draw(ref changed, alpha);



                    HomeFeatureSettingsConfig? homeSettings = GetConfigObject<HomeFeatureSettingsConfig>();

                    foreach (SectionNode selectionNode in _featureSections)

                    {

                        if (homeSettings != null && !FeatureRegistry.IsFeatureSectionVisible(homeSettings, selectionNode.Name))

                        {

                            selectionNode.Selected = false;

                            continue;

                        }



                        didReset |= selectionNode.Draw(ref changed, alpha);

                    }



                    foreach (SectionNode selectionNode in _advancedSections)

                    {

                        didReset |= selectionNode.Draw(ref changed, alpha);

                    }



                    didReset |= _profilesImportNode.Draw(ref changed, alpha);

                }



                ImGui.EndGroup();

            }



            ImGui.EndGroup();



            DrawTopWindowChrome(alpha);



            if (ConfigurationManager.Instance.OverrideDalamudStyle)

            {

                ConfigWindowChrome.DrawSidebarDivider(SidebarWidth * _scale, alpha);

            }



            PopStyles(popColors);



            if (didReset)

            {

                ConfigObjectResetEvent?.Invoke(this);

            }



            if (changed | didReset)

            {

                PresetManager.MarkCustomIfNeeded();

            }



            NeedsSave |= changed | didReset;

        }



        private void DrawTopWindowChrome(float alpha)

        {

            float chromeHeight = 32f * _scale;

            float buttonY = 5f * _scale;

            float buttonHeight = 22f * _scale;

            float closeWidth = 22f * _scale;

            float hideWidth = 26f * _scale;

            float closeX = ImGui.GetWindowWidth() - 28f * _scale;

            float hideX = ImGui.GetWindowWidth() - 60f * _scale;



            ImGui.SetCursorPos(Vector2.Zero);

            ImGui.PushStyleColor(ImGuiCol.ChildBg, 0);

            if (ImGui.BeginChild(

                "##aetherUiTopChrome",

                new Vector2(0f, chromeHeight),

                false,

                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))

            {

                ImGui.PushFont(UiBuilder.IconFont);

                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, alpha));



                ImGui.SetCursorPos(new Vector2(hideX, buttonY));

                string hideString = ConfigurationManager.Instance.ShowHUD

                    ? FontAwesomeIcon.Eye.ToIconString()

                    : FontAwesomeIcon.EyeSlash.ToIconString();

                if (ImGui.Button(hideString, new Vector2(hideWidth, buttonHeight)))

                {

                    ConfigurationManager.Instance.ShowHUD = !ConfigurationManager.Instance.ShowHUD;

                }

                ImGui.PopStyleColor();

                ImGui.PopFont();

                ImGuiHelper.SetTooltip(ConfigurationManager.Instance.ShowHUD ? "Hide HUD" : "Show HUD");



                ImGui.PushFont(UiBuilder.IconFont);

                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, alpha));

                ImGui.SetCursorPos(new Vector2(closeX, buttonY));

                if (ImGui.Button(FontAwesomeIcon.Times.ToIconString(), new Vector2(closeWidth, buttonHeight)))

                {

                    ConfigurationManager.Instance.CloseConfigWindow();

                }

                ImGui.PopStyleColor();

                ImGui.PopFont();

                ImGuiHelper.SetTooltip("Close");



                ImGui.EndChild();

            }

            ImGui.PopStyleColor();

        }



        private void EnsureValidSelection()

        {

            if (string.IsNullOrEmpty(SelectedOptionName))

            {

                SelectedOptionName = NavigationConstants.Home;

            }



            if (SelectedOptionName == NavigationConstants.Home ||

                SelectedOptionName == NavigationConstants.ProfilesImport ||

                FeatureRegistry.IsAdvancedSection(SelectedOptionName))

            {

                return;

            }



            HomeFeatureSettingsConfig? homeSettings = GetConfigObject<HomeFeatureSettingsConfig>();

            if (homeSettings != null && !FeatureRegistry.IsFeatureSectionVisible(homeSettings, SelectedOptionName))

            {

                SelectedOptionName = NavigationConstants.Home;

            }

        }



        private void DrawSidebarLogo()

        {

            IDalamudTextureWrap? logo = Plugin.BannerTexture?.GetWrapOrDefault();

            if (logo == null)

            {

                return;

            }



            ImGui.Dummy(new Vector2(0f, SidebarLogoTopSpacing * _scale));



            float padding = SidebarLogoPadding * _scale;

            float spacing = SidebarLogoBottomSpacing * _scale;

            float availableWidth = ImGui.GetContentRegionAvail().X - padding * 2f;

            if (availableWidth <= 0f)

            {

                return;

            }



            float maxHeight = SidebarLogoMaxHeight * _scale;

            Vector2 displaySize = HomeBrandingImage.GetFitSize(availableWidth, maxHeight, logo.Width, logo.Height);

            if (displaySize.X <= 0f || displaySize.Y <= 0f)

            {

                return;

            }



            HomeBrandingImage.DrawTexture(logo, displaySize, padding);



            string version = $"v{Plugin.Version}";

            Vector2 versionSize = ImGui.CalcTextSize(version);

            ImGui.SetCursorPosX(padding + (availableWidth - versionSize.X) * 0.5f);

            ImGui.PushStyleColor(ImGuiCol.Text, HomeUiStyle.TextMuted);

            ImGui.Text(version);

            ImGui.PopStyleColor();



            ImGui.Dummy(new Vector2(0f, spacing));

        }



        private bool DrawNavigation(ref bool changed)

        {

            bool didReset = false;

            RefreshSelectedNode();

            SyncNavGroupExpansion();

            if (SidebarNavItem.Draw(
                "##navHome",
                NavigationConstants.Home,
                SidebarNavIcons.GetTopLevelIcon(NavigationConstants.Home),
                _homeNode.Selected))
            {
                SelectSection(_homeNode);
            }

            SidebarNavItem.DrawCollapsibleSectionHeader(
                "##navSectionFeatures",
                "Features",
                ref _featuresSectionExpanded,
                FeatureNavGroups.IsFeaturesNavSection(SelectedOptionName));

            HomeFeatureSettingsConfig? homeSettings = GetConfigObject<HomeFeatureSettingsConfig>();

            if (_featuresSectionExpanded)
            {
                didReset |= DrawFeatureNavigationGroups(homeSettings);
            }

            SidebarNavItem.DrawCollapsibleSectionHeader(
                "##navSectionAdvanced",
                "Advanced",
                ref _advancedSectionExpanded,
                FeatureRegistry.IsAdvancedSection(SelectedOptionName ?? string.Empty));

            if (_advancedSectionExpanded)
            {
                didReset |= DrawAdvancedNavigation();
            }

            SidebarNavItem.DrawCollapsibleSectionHeader(
                "##navSectionProfiles",
                "Profiles",
                ref _profilesSectionExpanded,
                SelectedOptionName == NavigationConstants.ProfilesImport);

            if (_profilesSectionExpanded)
            {
                if (SidebarNavItem.Draw(
                    "##navProfilesImport",
                    NavigationConstants.ProfilesImport,
                    SidebarNavIcons.GetTopLevelIcon(NavigationConstants.ProfilesImport),
                    _profilesImportNode.Selected))
                {
                    SelectSection(_profilesImportNode);
                }
            }



            return didReset;

        }



        private void SyncNavGroupExpansion()

        {

            if (FeatureNavGroups.IsFeaturesNavSection(SelectedOptionName))

            {

                _featuresSectionExpanded = true;

            }



            if (FeatureRegistry.IsAdvancedSection(SelectedOptionName ?? string.Empty))

            {

                _advancedSectionExpanded = true;

            }



            if (SelectedOptionName == NavigationConstants.ProfilesImport)

            {

                _profilesSectionExpanded = true;

            }



            if (FeatureNavGroups.IsPlayerTargetSection(SelectedOptionName))

            {

                _playerTargetNavExpanded = true;

            }



            if (FeatureNavGroups.IsIndividualFramesSection(SelectedOptionName))

            {

                _playerTargetNavExpanded = true;

                _individualFramesNavExpanded = true;

            }



            if (FeatureNavGroups.IsPartyRaidSection(SelectedOptionName))

            {

                _partyRaidNavExpanded = true;

            }

        }



        private bool DrawFeatureNavigationGroups(HomeFeatureSettingsConfig? homeSettings)

        {

            bool didReset = false;

            float indent = 14 * _scale;



            if (HasAnyVisibleSection(homeSettings, FeatureNavGroups.PlayerParameterOrbSection)

                || HasAnyVisibleSection(homeSettings, FeatureNavGroups.IndividualFrameSections)

                || HasAnyVisibleSection(homeSettings, FeatureNavGroups.PlayerTargetAfterFramesSections))

            {

                didReset |= DrawFeatureNavGroup(

                    FeatureNavGroups.PlayerTarget,

                    ref _playerTargetNavExpanded,

                    FeatureNavGroups.IsPlayerTargetSection(SelectedOptionName),

                    () =>

                    {

                        bool groupReset = false;

                        groupReset |= DrawFeatureSectionNavItem(homeSettings, FeatureNavGroups.PlayerParameterOrbSection, indent);

                        groupReset |= DrawIndividualFramesNavGroup(homeSettings, indent);



                        foreach (string sectionName in FeatureNavGroups.PlayerTargetAfterFramesSections)

                        {

                            groupReset |= DrawFeatureSectionNavItem(homeSettings, sectionName, indent);

                        }



                        return groupReset;

                    });

            }



            if (HasAnyVisibleSection(homeSettings, FeatureNavGroups.PartyRaidSections))

            {

                didReset |= DrawFeatureNavGroup(

                    FeatureNavGroups.PartyRaid,

                    ref _partyRaidNavExpanded,

                    FeatureNavGroups.IsPartyRaidSection(SelectedOptionName),

                    () =>

                    {

                        bool groupReset = false;

                        foreach (string sectionName in FeatureNavGroups.PartyRaidSections)

                        {

                            groupReset |= DrawFeatureSectionNavItem(homeSettings, sectionName, indent);

                        }



                        return groupReset;

                    });

            }



            foreach (string sectionName in FeatureNavGroups.UtilitySections)

            {

                didReset |= DrawFeatureSectionNavItem(homeSettings, sectionName, 0f);

            }



            return didReset;

        }



        private bool DrawIndividualFramesNavGroup(HomeFeatureSettingsConfig? homeSettings, float parentIndent)

        {

            if (!HasAnyVisibleSection(homeSettings, FeatureNavGroups.IndividualFrameSections))

            {

                return false;

            }



            bool didReset = false;

            float nestedIndent = parentIndent + 14 * _scale;

            SidebarNavItem.DrawGroupHeader(
                "##navIndividualFrames",
                FeatureNavGroups.IndividualFrames,
                SidebarNavIcons.GetGroupIcon(FeatureNavGroups.IndividualFrames),
                ref _individualFramesNavExpanded,
                FeatureNavGroups.IsIndividualFramesSection(SelectedOptionName),
                parentIndent);

            if (_individualFramesNavExpanded)
            {
                foreach (string sectionName in FeatureNavGroups.IndividualFrameSections)
                {
                    didReset |= DrawFeatureSectionNavItem(homeSettings, sectionName, nestedIndent);
                }
            }

            return didReset;
        }



        private bool DrawFeatureNavGroup(string label, ref bool expanded, bool childSelected, Func<bool> drawChildren)

        {

            bool didReset = false;

            SidebarNavItem.DrawGroupHeader(
                $"##navGroup_{label}",
                label,
                SidebarNavIcons.GetGroupIcon(label),
                ref expanded,
                childSelected);

            if (expanded)
            {
                didReset |= drawChildren();
            }

            return didReset;
        }



        private bool DrawFeatureSectionNavItem(HomeFeatureSettingsConfig? homeSettings, string sectionName, float indent)

        {

            SectionNode? section = FindFeatureSection(sectionName);

            if (section == null)

            {

                return false;

            }



            if (homeSettings != null && !FeatureRegistry.IsFeatureSectionVisible(homeSettings, sectionName))

            {

                return false;

            }



            bool didReset = false;

            if (SidebarNavItem.Draw(
                $"##navSection_{sectionName}",
                SidebarNavLabels.GetDisplayLabel(sectionName),
                SidebarNavIcons.GetSectionIcon(sectionName),
                section.Selected,
                indent))
            {
                SelectSection(section);
            }



            DrawExportResetContextMenu(section, sectionName);

            return didReset;
        }



        private SectionNode? FindFeatureSection(string sectionName)

        {

            foreach (SectionNode node in _featureSections)

            {

                if (node.Name == sectionName)

                {

                    return node;

                }

            }



            return null;

        }



        private static bool HasAnyVisibleSection(HomeFeatureSettingsConfig? homeSettings, string sectionName)

        {

            return homeSettings == null || FeatureRegistry.IsFeatureSectionVisible(homeSettings, sectionName);

        }



        private static bool HasAnyVisibleSection(HomeFeatureSettingsConfig? homeSettings, params string[] sectionNames)

        {

            foreach (string sectionName in sectionNames)

            {

                if (homeSettings == null || FeatureRegistry.IsFeatureSectionVisible(homeSettings, sectionName))

                {

                    return true;

                }

            }



            return false;

        }



        private bool DrawAdvancedNavigation()

        {

            bool didReset = false;



            foreach (SectionNode selectionNode in _advancedSections)

            {

                if (SidebarNavItem.Draw(
                    $"##navAdvanced_{selectionNode.Name}",
                    SidebarNavLabels.GetDisplayLabel(selectionNode.Name),
                    SidebarNavIcons.GetSectionIcon(selectionNode.Name),
                    selectionNode.Selected))
                {
                    SelectSection(selectionNode);
                }



                DrawExportResetContextMenu(selectionNode, selectionNode.Name);

            }



            return didReset;

        }



        private void SelectSection(SectionNode section)

        {

            _homeNode.Selected = false;

            _profilesImportNode.Selected = false;



            foreach (SectionNode node in _featureSections)

            {

                node.Selected = false;

            }



            foreach (SectionNode node in _advancedSections)

            {

                node.Selected = false;

            }



            section.Selected = true;

            SelectedOptionName = section.Name;

        }



        public ConfigPageNode? GetOrAddConfig<T>() where T : PluginConfigObject

        {

            object[] attributes = typeof(T).GetCustomAttributes(true);



            foreach (object attribute in attributes)

            {

                if (attribute is SectionAttribute sectionAttribute)

                {

                    foreach (SectionNode sectionNode in _children)

                    {

                        if (sectionNode.Name == sectionAttribute.SectionName)

                        {

                            return sectionNode.GetOrAddConfig<T>();

                        }

                    }



                    SectionNode newNode = new();

                    newNode.Name = sectionAttribute.SectionName;

                    newNode.ForceAllowExport = sectionAttribute.ForceAllowExport;

                    _children.Add(newNode);



                    return newNode.GetOrAddConfig<T>();

                }

            }



            Type type = typeof(T);

            throw new ArgumentException("The provided configuration object does not specify a section: " + type.Name);

        }

    }

}


