using System;
using HarmonyLib;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using HireMe.UI;
using UnityEngine;

#if IL2CPP
using Il2CppScheduleOne.Dialogue;
#elif MONO
using ScheduleOne.Dialogue;
#endif


namespace HireMe.Patches {
    public class FixerDialoguePatches {

        [HarmonyPatch(typeof(DialogueController_Fixer), nameof(DialogueController_Fixer.ChoiceCallback))]
        public static class DialogueControllerFixer_ChoiceCallback_Patch {
            public static bool Prefix(DialogueController_Fixer __instance, string choiceLabel) {
                try {
                    if (choiceLabel.Contains("GENERIC_CHOICE")) {
                        UIManager.hiringInterface.SetIsOpen(true);
                        return false;
                    } else {
                        return true;
                    }
                } catch (System.Exception ex) {
                    MelonLogger.Error(ex);
                    return true;
                }
            }

        }

    }
}
