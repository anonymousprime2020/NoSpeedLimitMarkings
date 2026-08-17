using Colossal;
using Game.Settings;
using System.Collections.Generic;

namespace NoSpeedLimitMarkings
{
    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), Mod.DisplayName },
                { m_Setting.GetOptionTabLocaleID(Setting.GeneralSection), "General" },
                { m_Setting.GetOptionGroupLocaleID(Setting.VisibilityGroup), "Visibility" },
                { m_Setting.GetOptionGroupLocaleID(Setting.DiagnosticsGroup), "Diagnostics" },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.HideSpeedLimitMarkings)),
                    "Hide pavement speed-limit markings"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.HideSpeedLimitMarkings)),
                    "Hides automatically generated speed-limit numerals painted on roads. " +
                    "Arrows, lane lines, crosswalks, roadside signs, and actual speed limits are unchanged."
                },
                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.DetailedLogging)),
                    "Detailed logging"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.DetailedLogging)),
                    "Writes prefab discovery and cleanup details to the game log for troubleshooting."
                }
            };
        }

        public void Unload()
        {
        }
    }
}

