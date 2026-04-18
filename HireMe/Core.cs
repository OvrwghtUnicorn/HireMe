using MelonLoader;
using UnityEngine;
using HireMe.TemplateUtils;
using HireMe.UI;
using HireMe.Utils;
using UnityEngine.Events;


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

namespace HireMe {
    public static class BuildInfo {
        public const string Name = "Hire Me";
        public const string Description = "A user interface for hiring employees";
        public const string Author = "OverweightUnicorn";
        public const string Company = "UnicornsCanMod";
        public const string Version = "2.2.0";
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

        public override void OnInitializeMelon() {
            ModSettings.Init();
            AssetBundleUtils.Initialize(this);
            LoggerInstance.Msg($"PhoneApp: {ModSettings.EnablePhoneApp.Value}");
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