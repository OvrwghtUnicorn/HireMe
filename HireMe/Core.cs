using MelonLoader;
using UnityEngine;
using MelonLoader.Utils;
using HireMe.TemplateUtils;
using HireMe.UI;
using HireMe.Utils;
using UnityEngine.Events;
using UnityEngine.UI;
using ModManagerPhoneApp;



#if IL2CPP
using ScheduleOneGame = Il2CppScheduleOne;
using Il2CppScheduleOne.UI.Phone;
using Il2CppScheduleOne.DevUtilities;
#elif MONO
using ScheduleOneGame = ScheduleOne;
using ScheduleOne.UI.Phone;
using ScheduleOne.DevUtilities;
#endif

[assembly: MelonInfo(typeof(HireMe.Core), HireMe.BuildInfo.Name, HireMe.BuildInfo.Version, HireMe.BuildInfo.Author, HireMe.BuildInfo.DownloadLink)]
[assembly: MelonColor(255, 191, 0, 255)]
[assembly: MelonGame("TVGS", "Schedule I")]
#if IL2CPP
[assembly: MelonOptionalDependencies("ModManager&PhoneApp")]
#elif MONO
[assembly: MelonOptionalDependencies("ModManagerPhoneApp")]
#endif

namespace HireMe {
    public static class BuildInfo {
        public const string Name = "Hire Me";
        public const string Description = "A user interface for hiring employees";
        public const string Author = "OverweightUnicorn";
        public const string Company = "UnicornsCanMod";
        public const string Version = "2.0.0";
        public const string DownloadLink = null;
    }
    public class Core : MelonMod {

        public static double? botanistSigningFee;
        public static bool? botanistSigningFeeEnabled;
        public static double? botanistDailyWage;

        public static double? chemistSigningFee;
        public static bool? chemistSigningFeeEnabled;
        public static double? chemistDailyWage;

        public static double? handlerSigningFee;
        public static bool? handlerSigningFeeEnabled;
        public static double? handlerDailyWage;

        public static double? cleanerSigningFee;
        public static bool? cleanerSigningFeeEnabled;
        public static double? cleanerDailyWage;

        private static bool _modManagerFound = false;

        public override void OnInitializeMelon() {
            ModSettings.Init();
            LoggerInstance.Msg($"PhoneApp: {ModSettings.EnablePhoneApp.Value}");

            // --- Check for Mod Manager Registration ---
            try {
                // Replace "Mod Manager & Phone App" with the EXACT MelonInfo name
                _modManagerFound = MelonBase.RegisteredMelons.Any(mod => mod?.Info?.Name == "Mod Manager & Phone App");
                if (_modManagerFound) {
                    LoggerInstance.Msg("Mod Manager found. Attempting event subscription...");
                    SubscribeToModManagerEvents(); // Call helper
                } else { LoggerInstance.Warning("Mod Manager not found. Skipping event subscription."); }
            } catch (Exception ex) { LoggerInstance.Error($"Error checking RegisteredMelons: {ex}"); _modManagerFound = false; }
            // --- End Check ---

            LoggerInstance.Msg("MyCoolMod Initialization complete.");
        }

        public override void OnLateInitializeMelon() {
            ScheduleOneGame.Persistence.LoadManager.Instance.onLoadComplete.AddListener((UnityAction)Initialize);
            LoadBreadConfig();
        }

        public void Initialize() {
            AssetBundleUtils.LoadAssetBundle(UIManager.ASSET_BUNDLE_NAME);
            UIManager.LoadAssets();
            GameObject rootUI = GameObject.Find("UI");
            if (rootUI != null) {
                UIManager.SetupUI(rootUI.transform);
            }

            HomeScreen homeScreen = PlayerSingleton<HomeScreen>.instance;
            if (homeScreen) {
                UIManager.SetupPhoneApp(homeScreen);
            }
        }

        private void SubscribeToModManagerEvents() {
            try {
#if IL2CPP

                ModManagerPhoneApp.ModSettingsEvents.OnPhonePreferencesSaved += HandleSettingsUpdate;
#elif MONO
                ModManagerPhoneApp.ModSettingsEvents.OnPreferencesSaved += HandleSettingsUpdate;
#endif
                // ModManagerPhoneApp.ModSettingsEvents.OnMenuPreferencesSaved += HandleSettingsUpdate;
                LoggerInstance.Msg("Successfully subscribed to Mod Manager save events.");
            }
            // Catch potential runtime errors even if mod was registered
            catch (TypeLoadException) { LoggerInstance.Error("TypeLoadException during subscription! Mod Manager incompatible?"); _modManagerFound = false; } catch (Exception ex) { LoggerInstance.Error($"Unexpected error during subscription: {ex}"); _modManagerFound = false; }
        }

        private void HandleSettingsUpdate() // Can be static if it only accesses static fields/methods
        {
            // Use LoggerInstance if non-static, or Melon<MyCoolMod>.Logger if static
            LoggerInstance.Msg("Mod Manager saved preferences. Reloading settings...");
            try {
                bool newPhoneAppValue = ModSettings.EnablePhoneApp.Value;
                MelonLogger.Msg("EnablePhoneApp: " + newPhoneAppValue);
                UIManager.TogglePhoneApp(newPhoneAppValue);

                LoggerInstance.Msg("Settings reloaded successfully.");
            } catch (System.Exception ex) { LoggerInstance.Error($"Error applying updated settings after save: {ex}"); }
        }

        private void UnsubscribeFromModManagerEvents() {
            try {
#if IL2CPP

                ModManagerPhoneApp.ModSettingsEvents.OnPhonePreferencesSaved -= HandleSettingsUpdate;
#elif MONO
                ModManagerPhoneApp.ModSettingsEvents.OnPreferencesSaved -= HandleSettingsUpdate;
#endif
                LoggerInstance.Msg("Unsubscribed from Mod Manager events.");
            } catch (Exception ex) { LoggerInstance.Warning($"Ignoring error during unsubscribe: {ex.Message}"); }
        }

        public override void OnDeinitializeMelon() {
            if (_modManagerFound) { UnsubscribeFromModManagerEvents(); }
        }

        public void LoadBreadConfig() {
            if (!BreadTweaksCompat.IsBreadTweaksInstalled())
                return;

            LoadEmployeeConfig(
                "Botanist",
                out botanistSigningFeeEnabled,
                out botanistSigningFee,
                out botanistDailyWage
            );

            LoadEmployeeConfig(
                "Chemist",
                out chemistSigningFeeEnabled,
                out chemistSigningFee,
                out chemistDailyWage
            );

            LoadEmployeeConfig(
                "Handler",
                out handlerSigningFeeEnabled,
                out handlerSigningFee,
                out handlerDailyWage
            );

            LoadEmployeeConfig(
                "Cleaner",
                out cleanerSigningFeeEnabled,
                out cleanerSigningFee,
                out cleanerDailyWage
            );
        }

        private void LoadEmployeeConfig(
            string name,
            out bool? signingFeeEnabled,
            out double? signingFee,
            out double? dailyWage) {
            signingFeeEnabled = BreadTweaksCompat.GetSigningFeeEnabled(name);
            signingFee = BreadTweaksCompat.GetSigningFee(name);
            dailyWage = BreadTweaksCompat.GetDailyWage(name);

            List<string> parts = new List<string>();

            if (signingFeeEnabled.HasValue)
                parts.Add($"Signing Fee Enabled: {signingFeeEnabled.Value}");
            if (signingFee.HasValue)
                parts.Add($"Signing Fee: {signingFee.Value}");
            if (dailyWage.HasValue)
                parts.Add($"Daily Wage: {dailyWage.Value}");

            if (parts.Count > 0) {
                MelonLogger.Msg($"\nLoaded {name} Config\n {string.Join("\n ", parts)}");
            }
        }
    }
}