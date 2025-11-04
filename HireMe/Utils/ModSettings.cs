using MelonLoader;

namespace HireMe.Utils {
    public static class ModSettings {
        public static MelonPreferences_Category MainCategory;
        public static MelonPreferences_Entry<bool> EnablePhoneApp;

        public static void Init() {
            MainCategory = MelonPreferences.CreateCategory("HireMe_01_Main", "Main Settings");

            EnablePhoneApp = MainCategory.CreateEntry(
                "EnablePhoneApp",
                true,
                "Enable the Phone Application",
                "Enables the Hire Me Phone Application to manage your employees remotely and at any time."
            );
        }
    }
}