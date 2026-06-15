using DelvUI.Config;
using DelvUI.Config.Navigation;
using DelvUI.Config.Profiles;
using DelvUI.Config.Tree;
using DelvUI.Helpers;
using DelvUI.Interface;
using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace DelvUI.Config.Home
{
    public class ProfilesImportSectionNode : SectionNode
    {
        public ProfilesImportSectionNode()
        {
            Name = NavigationConstants.ProfilesImport;
        }

        public override bool Draw(ref bool changed, float alpha)
        {
            if (!Selected)
            {
                return false;
            }

            ImGui.NewLine();

            if (!ImGui.BeginChild("AetherUI_ProfilesImport", new Vector2(0, -10), true))
            {
                return false;
            }

            bool didReset = false;

            if (ImGui.BeginTabBar("##ProfilesImportTabs"))
            {
                if (ImGui.BeginTabItem("Profiles"))
                {
                    didReset |= ProfilesManager.Instance?.Draw(ref changed) ?? false;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Import"))
                {
                    ConfigPageNode? importNode = ConfigurationManager.Instance.GetConfigPageNode<ImportConfig>();
                    if (importNode != null)
                    {
                        ImGui.BeginChild("importTabContent", new Vector2(0, 0), true);
                        didReset |= importNode.Draw(ref changed);
                        ImGui.EndChild();
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.EndChild();

            return didReset;
        }
    }
}
