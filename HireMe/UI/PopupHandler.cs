using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HireMe.UI {

#if IL2CPP
    [RegisterTypeInIl2Cpp]
#endif
    public class PopupHandler : MonoBehaviour {
        public Button closePopup;

#if IL2CPP
        public PopupHandler(IntPtr ptr) : base(ptr) { }
#endif

        void Awake() {
            closePopup = GetComponentInChildren<Button>();
            closePopup.onClick.AddListener((UnityAction)ClosePopup);
        }
        public void OpenPopup() {
            this.gameObject.SetActive(true);
        }
        public void ClosePopup() {
            this.gameObject.SetActive(false);
        }
    }
}
