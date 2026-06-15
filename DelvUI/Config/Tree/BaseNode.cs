using Dalamud.Bindings.ImGui;

using Dalamud.Interface;

using Dalamud.Interface.Textures.TextureWraps;

using Dalamud.Interface.Utility;

using DelvUI.Config.Attributes;

using DelvUI.Config.Home;

using DelvUI.Config.Navigation;

using DelvUI.Config.Presets;

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

        private bool _advancedNavExpanded = true;

        public void SetAdvancedNavigationExpanded(bool expanded)
        {
            _advancedNavExpanded = expanded;
        }



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

                    IDalamudTextureWrap? aetherUiBanner = Plugin.BannerTexture?.GetWrapOrDefault();

                    if (aetherUiBanner != null)

                    {

                        ImGui.SetCursorPos(new Vector2(15 + 150 * _scale / 2f - aetherUiBanner.Width / 2f, 5));

                        ImGui.Image(aetherUiBanner.Handle, new Vector2(aetherUiBanner.Width, aetherUiBanner.Height));

                    }



                    ImGui.SetCursorPos(new Vector2(60 * _scale, 35));

                    ImGui.Text($"v{Plugin.Version}");



                    if (ImGui.BeginChild("left pane", new Vector2(150 * _scale, -10), true, ImGuiWindowFlags.NoScrollbar))

                    {

                        didReset |= DrawNavigation(ref changed);

                    }



                    ImGui.EndChild();

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



            ImGui.PushFont(UiBuilder.IconFont);

            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, alpha));

            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth() - 28 * _scale, 5 * _scale));

            if (ImGui.Button(FontAwesomeIcon.Times.ToIconString(), new Vector2(22 * _scale, 22 * _scale)))

            {

                ConfigurationManager.Instance.CloseConfigWindow();

            }

            ImGui.PopStyleColor();

            ImGui.PopFont();

            ImGuiHelper.SetTooltip("Close");



            ImGui.PushFont(UiBuilder.IconFont);

            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(AetherAccent.X, AetherAccent.Y, AetherAccent.Z, alpha));

            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowWidth() - 60 * _scale, 5 * _scale));

            string hideString = ConfigurationManager.Instance.ShowHUD ? FontAwesomeIcon.Eye.ToIconString() : FontAwesomeIcon.EyeSlash.ToIconString();

            if (ImGui.Button(hideString, new Vector2(26 * _scale, 22 * _scale)))

            {

                ConfigurationManager.Instance.ShowHUD = !ConfigurationManager.Instance.ShowHUD;

            }

            ImGui.PopStyleColor();

            ImGui.PopFont();

            ImGuiHelper.SetTooltip(ConfigurationManager.Instance.ShowHUD ? "Hide HUD" : "Show HUD");



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



        private bool DrawNavigation(ref bool changed)

        {

            bool didReset = false;

            RefreshSelectedNode();



            if (DrawNavSelectable(NavigationConstants.Home, _homeNode.Selected))

            {

                SelectSection(_homeNode);

            }



            HomeFeatureSettingsConfig? homeSettings = GetConfigObject<HomeFeatureSettingsConfig>();

            foreach (SectionNode selectionNode in _featureSections)

            {

                if (homeSettings != null && !FeatureRegistry.IsFeatureSectionVisible(homeSettings, selectionNode.Name))

                {

                    continue;

                }



                if (DrawNavSelectable(selectionNode.Name, selectionNode.Selected))

                {

                    SelectSection(selectionNode);

                }



                DrawExportResetContextMenu(selectionNode, selectionNode.Name);

            }



            didReset |= DrawAdvancedNavigation();



            if (DrawNavSelectable(NavigationConstants.ProfilesImport, _profilesImportNode.Selected))

            {

                SelectSection(_profilesImportNode);

            }



            return didReset;

        }



        private bool DrawAdvancedNavigation()

        {

            bool didReset = false;

            bool advancedChildSelected = _advancedSections.Any(s => s.Name == SelectedOptionName);

            string advancedLabel = (_advancedNavExpanded ? "v " : "> ") + NavigationConstants.AdvancedOptions;



            if (ImGui.Selectable(advancedLabel, advancedChildSelected && !_advancedNavExpanded))

            {

                _advancedNavExpanded = !_advancedNavExpanded;

            }



            if (_advancedNavExpanded)

            {

                float indent = 14 * _scale;



                foreach (SectionNode selectionNode in _advancedSections)

                {

                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + indent);



                    if (DrawNavSelectable(selectionNode.Name, selectionNode.Selected))

                    {

                        SelectSection(selectionNode);

                    }



                    DrawExportResetContextMenu(selectionNode, selectionNode.Name);

                }

            }



            return didReset;

        }



        private static bool DrawNavSelectable(string label, bool selected)

        {

            return ImGui.Selectable(label, selected);

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


