using HireMe.TemplateUtils;
using HireMe.Utils;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if IL2CPP
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.UI.Items;
using Il2CppScheduleOne.UI.Phone;
#elif MONO
using ScheduleOne.DevUtilities;
using ScheduleOne.UI;
using ScheduleOne.UI.Items;
using ScheduleOne.UI.Phone;
#endif

namespace HireMe.UI {
    public static class UIManager {
        static Core mod = MelonAssembly.FindMelonInstance<Core>();

        public const string ASSET_BUNDLE_NAME = "hiremeassets";
        public static GameObject propertyContainerPrefab;
        public static GameObject hireEmployeeUIPrefab;
        public static GameObject handlerPrefab;
        public static GameObject cleanerPrefab;
        public static GameObject chemistPrefab;
        public static GameObject botanistPrefab;
        public static GameObject addHandlerListingPrefab;
        public static GameObject addCleanerListingPrefab;
        public static GameObject addChemistListingPrefab;
        public static GameObject addBotanistListingPrefab;
        public static Sprite handlerAdd;
        public static Sprite handlerIcon;
        public static Sprite cleanerAdd;
        public static Sprite cleanerIcon;
        public static Sprite chemistAdd;
        public static Sprite chemistIcon;
        public static Sprite botanistAdd;
        public static Sprite botanistIcon;
        public static Sprite expandIcon;
        public static Sprite collapseIcon;
        public static Sprite deleteIcon;
        public static Sprite appIcon;

        public static HireEmployeeInterface hiringInterface;
        public static Button appButton;

        public static void LoadAssets() {
            try {
                if (propertyContainerPrefab == null)
                    propertyContainerPrefab = AssetBundleUtils.LoadAssetFromBundle<GameObject>("propertycontainer.prefab", ASSET_BUNDLE_NAME);

                if (hireEmployeeUIPrefab == null)
                    hireEmployeeUIPrefab = AssetBundleUtils.LoadAssetFromBundle<GameObject>("hireemployeeui.prefab", ASSET_BUNDLE_NAME);

                if (handlerPrefab == null)
                    handlerPrefab = AssetBundleUtils.LoadAssetFromBundle<GameObject>("handler.prefab", ASSET_BUNDLE_NAME);

                if (cleanerPrefab == null)
                    cleanerPrefab = AssetBundleUtils.LoadAssetFromBundle<GameObject>("cleaner.prefab", ASSET_BUNDLE_NAME);

                if (chemistPrefab == null)
                    chemistPrefab = AssetBundleUtils.LoadAssetFromBundle<GameObject>("chemist.prefab", ASSET_BUNDLE_NAME);

                if (botanistPrefab == null)
                    botanistPrefab = AssetBundleUtils.LoadAssetFromBundle<GameObject>("botanist.prefab", ASSET_BUNDLE_NAME);

                if (addHandlerListingPrefab == null)
                    addHandlerListingPrefab = AssetBundleUtils.LoadAssetFromBundle<GameObject>("addhandlerlisting.prefab", ASSET_BUNDLE_NAME);

                if (addCleanerListingPrefab == null)
                    addCleanerListingPrefab = AssetBundleUtils.LoadAssetFromBundle<GameObject>("addcleanerlisting.prefab", ASSET_BUNDLE_NAME);

                if (addChemistListingPrefab == null)
                    addChemistListingPrefab = AssetBundleUtils.LoadAssetFromBundle<GameObject>("addchemistlisting.prefab", ASSET_BUNDLE_NAME);

                if (addBotanistListingPrefab == null)
                    addBotanistListingPrefab = AssetBundleUtils.LoadAssetFromBundle<GameObject>("addbotanistlisting.prefab", ASSET_BUNDLE_NAME);

                if (handlerAdd == null)
                    handlerAdd = AssetBundleUtils.LoadAssetFromBundle<Sprite>("handler_add.png", ASSET_BUNDLE_NAME);

                if (handlerIcon == null)
                    handlerIcon = AssetBundleUtils.LoadAssetFromBundle<Sprite>("handler.png", ASSET_BUNDLE_NAME);

                if (cleanerAdd == null)
                    cleanerAdd = AssetBundleUtils.LoadAssetFromBundle<Sprite>("cleaner_add.png", ASSET_BUNDLE_NAME);

                if (cleanerIcon == null)
                    cleanerIcon = AssetBundleUtils.LoadAssetFromBundle<Sprite>("cleaner.png", ASSET_BUNDLE_NAME);

                if (chemistAdd == null)
                    chemistAdd = AssetBundleUtils.LoadAssetFromBundle<Sprite>("chemist_add.png", ASSET_BUNDLE_NAME);

                if (chemistIcon == null)
                    chemistIcon = AssetBundleUtils.LoadAssetFromBundle<Sprite>("chemist.png", ASSET_BUNDLE_NAME);

                if (botanistAdd == null)
                    botanistAdd = AssetBundleUtils.LoadAssetFromBundle<Sprite>("botanist_add.png", ASSET_BUNDLE_NAME);

                if (botanistIcon == null)
                    botanistIcon = AssetBundleUtils.LoadAssetFromBundle<Sprite>("botanist.png", ASSET_BUNDLE_NAME);

                if (expandIcon == null)
                    expandIcon = AssetBundleUtils.LoadAssetFromBundle<Sprite>("expand.png", ASSET_BUNDLE_NAME);

                if (collapseIcon == null)
                    collapseIcon = AssetBundleUtils.LoadAssetFromBundle<Sprite>("collapse.png", ASSET_BUNDLE_NAME);

                if (deleteIcon == null)
                    deleteIcon = AssetBundleUtils.LoadAssetFromBundle<Sprite>("deletebutton.png", ASSET_BUNDLE_NAME);
                if (appIcon == null)
                    appIcon = AssetBundleUtils.LoadAssetFromBundle<Sprite>("appicon.png", ASSET_BUNDLE_NAME);
            } catch (Exception e) {
                mod.Unregister($"Unregistering mod due to error:\n {e.Source}\n {e.Message}");
            }
        }

        public static void SetupUI(Transform parent) {
            GameObject temp = GameObject.Instantiate(hireEmployeeUIPrefab,parent);
            hiringInterface = temp.transform.GetChild(0).gameObject.AddComponent<HireEmployeeInterface>();
            hiringInterface.propertyContainerPrefab = propertyContainerPrefab;
            // assign popup handler
            Transform errorPopupTransform = temp.transform.Find("HireEmployeeInterface/PopupBackground");
            if (errorPopupTransform != null) {
                hiringInterface.errorPopup = errorPopupTransform.gameObject.AddComponent<PopupHandler>();
            }

            Transform listingsContainer = temp.transform.Find("HireEmployeeInterface/Cart/EmployeeListingsContainer");
            if (listingsContainer != null) SetupListings(listingsContainer, temp.transform);
            hiringInterface.Setup();
            temp.SetActive(false);
        }

        private static void SetupListings(Transform parentContainer, Transform parentCanvas) {
            GameObject botanistListingGo = GameObject.Instantiate(addBotanistListingPrefab, parentContainer);
            EmployeeListing botanistListing = botanistListingGo.AddComponent<EmployeeListing>();
            botanistListing.addIcon = botanistAdd;
            botanistListing.defaultIcon = botanistIcon;
            botanistListing.employeePrefab = botanistPrefab;
            botanistListing.dragParent = parentCanvas;
            hiringInterface.botanistListing = botanistListing;

            GameObject chemistListingGo = GameObject.Instantiate(addChemistListingPrefab, parentContainer);
            EmployeeListing chemistListing = chemistListingGo.AddComponent<EmployeeListing>();
            chemistListing.addIcon = chemistAdd;
            chemistListing.defaultIcon = chemistIcon;
            chemistListing.employeePrefab = chemistPrefab;
            chemistListing.dragParent = parentCanvas;
            hiringInterface.chemistListing = chemistListing;

            GameObject handlerListingGo = GameObject.Instantiate(addHandlerListingPrefab, parentContainer);
            EmployeeListing handlerListing = handlerListingGo.AddComponent<EmployeeListing>();
            handlerListing.addIcon = handlerAdd;
            handlerListing.defaultIcon = handlerIcon;
            handlerListing.employeePrefab = handlerPrefab;
            handlerListing.dragParent = parentCanvas;
            hiringInterface.handlerListing = handlerListing;

            GameObject cleanerListingGo = GameObject.Instantiate(addCleanerListingPrefab, parentContainer);
            EmployeeListing cleanerListing = cleanerListingGo.AddComponent<EmployeeListing>();
            cleanerListing.addIcon = cleanerAdd;
            cleanerListing.defaultIcon = cleanerIcon;
            cleanerListing.employeePrefab = cleanerPrefab;
            cleanerListing.dragParent = parentCanvas;
            hiringInterface.cleanerListing = cleanerListing;
        }

        public static void AppClose() {
            Singleton<ItemUIManager>.instance.InputsContainer.gameObject.SetActive(false);
            Singleton<GameplayMenuInterface>.instance.Open();
        }

        public static void SetupPhoneApp(HomeScreen home) {
            GameplayMenu phoneStuff = home.transform.GetComponentInParent<GameplayMenu>();
            RectTransform component = UnityEngine.Object.Instantiate<GameObject>(home.appIconPrefab, home.appIconContainer).GetComponent<RectTransform>();
            component.Find("Mask/Image").GetComponent<Image>().sprite = appIcon;
            component.Find("Label").GetComponent<Text>().text = "HireMe";
            appButton = component.GetComponent<Button>();
            home.appIcons.Add(appButton);
            appButton.onClick.AddListener((UnityAction)hiringInterface.Open);
            var notificationContainer = appButton.transform.Find("Notifications");
            notificationContainer.gameObject.SetActive(false);
            appButton.gameObject.SetActive(ModSettings.EnablePhoneApp.Value);
            hiringInterface.onClose = (UnityAction)AppClose;
        }

        public static void TogglePhoneApp(bool isEnabled) {
            if(appButton == null) {
                MelonLogger.Error("Phone App is null and cannot be toggled");
                return;
            }

            appButton.gameObject.SetActive(isEnabled);
        }

    }
}
