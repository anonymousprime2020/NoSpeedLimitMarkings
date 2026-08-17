using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.Objects;
using Game.Rendering;
using Game.SceneFlow;
using NoSpeedLimitMarkings.Systems;
using Unity.Entities;

namespace NoSpeedLimitMarkings
{
    public sealed class Mod : IMod
    {
        public const string Id = nameof(NoSpeedLimitMarkings);
        public const string DisplayName = "No Speed Limit Markings";

        public static readonly ILog Log = LogManager
            .GetLogger($"{Id}.{nameof(Mod)}")
            .SetShowsErrorsInUI(false);

        public static Setting Settings { get; private set; }

        public void OnLoad(UpdateSystem updateSystem)
        {
            Log.Info($"Loading {DisplayName}");

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                Log.Info($"Executable asset: {asset.path}");
            }

            Settings = new Setting(this);
            Settings.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Settings));
            AssetDatabase.global.LoadSettings(Id, Settings, new Setting(this));

            // Remove speed-number candidates before the vanilla generator chooses
            // secondary road objects, track any instances loaded from a save, and
            // filter those exact instances from vanilla culling output.
            updateSystem.UpdateBefore<SpeedLimitMarkingPrefabSystem, SecondaryObjectSystem>(
                SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<SpeedLimitMarkingTrackingSystem>(
                SystemUpdatePhase.Modification5);
            updateSystem.UpdateBefore<SpeedLimitMarkingRenderSystem, BatchInstanceSystem>(
                SystemUpdatePhase.Rendering);
        }

        public void OnDispose()
        {
            Log.Info($"Disposing {DisplayName}");

            try
            {
                var world = World.DefaultGameObjectInjectionWorld;
                if (world != null && world.IsCreated)
                {
                    world.GetExistingSystemManaged<SpeedLimitMarkingTrackingSystem>()
                        ?.RestoreForModUnload();
                    world.GetExistingSystemManaged<SpeedLimitMarkingPrefabSystem>()
                        ?.RestoreForModUnload();
                }
            }
            catch (System.Exception exception)
            {
                Log.Warn($"Could not restore speed markings during unload: {exception}");
            }

            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }
        }
    }
}

