using System;
using System.IO;
using MelonLoader;
using MelonLoader.Utils;

namespace HireMe.TemplateUtils {

    public static class BreadTweaksCompat {
        private static readonly string BreadPrefsPath = Path.Combine(MelonEnvironment.UserDataDirectory, "Bread_Tweaks", "Employees.cfg");

        /// <summary>
        /// Checks if Bread_Tweaks appears to be installed based on presence of config file and expected categories.
        /// </summary>
        public static bool IsBreadTweaksInstalled() {
            return File.Exists(BreadPrefsPath)
                   && MelonPreferences.GetCategory("Botanist")?.GetEntry<double>("Daily Wage") != null;
        }

        public static double? GetDailyWage(string employeeType) {
            try {
                var category = MelonPreferences.GetCategory(employeeType);
                if (category == null)
                    return null;

                var wageEntry = category.GetEntry<double>("Daily Wage");
                return wageEntry?.Value;
            } catch (Exception ex) {
                MelonLogger.Warning($"[BreadCompat] Failed to get Daily Wage for '{employeeType}': {ex.Message}");
                return null;
            }
        }

        public static double? GetSigningFee(string employeeType) {
            try {
                var category = MelonPreferences.GetCategory(employeeType);
                if (category == null)
                    return null;

                var wageEntry = category.GetEntry<double>("Signing Fee");
                return wageEntry?.Value;
            } catch (Exception ex) {
                MelonLogger.Warning($"[BreadCompat] Failed to get Signing Fee for '{employeeType}': {ex.Message}");
                return null;
            }
        }

        public static bool? GetSigningFeeEnabled(string employeeType) {
            try {
                var category = MelonPreferences.GetCategory(employeeType);
                if (category == null)
                    return null;

                var wageEntry = category.GetEntry<bool>("Custom Signing Fee");
                return wageEntry?.Value;
            } catch (Exception ex) {
                MelonLogger.Warning($"[BreadCompat] Failed to get Custom Signing Fee value for '{employeeType}': {ex.Message}");
                return null;
            }
        }
    }
}
