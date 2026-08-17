using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;

namespace NoSpeedLimitMarkings
{
    [FileLocation(Mod.Id)]
    [SettingsUIGroupOrder(VisibilityGroup, DiagnosticsGroup)]
    [SettingsUIShowGroupName(VisibilityGroup, DiagnosticsGroup)]
    public sealed class Setting : ModSetting
    {
        public const string GeneralSection = "General";
        public const string VisibilityGroup = "Visibility";
        public const string DiagnosticsGroup = "Diagnostics";

        public Setting(IMod mod)
            : base(mod)
        {
            SetDefaults();
        }

        [SettingsUISection(GeneralSection, VisibilityGroup)]
        public bool HideSpeedLimitMarkings { get; set; }

        [SettingsUISection(GeneralSection, DiagnosticsGroup)]
        [SettingsUIAdvanced]
        public bool DetailedLogging { get; set; }

        public override void SetDefaults()
        {
            HideSpeedLimitMarkings = true;
            DetailedLogging = false;
        }
    }
}

