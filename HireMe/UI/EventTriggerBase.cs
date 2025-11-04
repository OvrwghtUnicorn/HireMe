using System;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

#if IL2CPP
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppGeneric = Il2CppSystem.Collections.Generic;
#endif

namespace HireMe.UI {
#if IL2CPP
    [RegisterTypeInIl2Cpp]
#endif
    public class EventTriggerBase : MonoBehaviour {
        private EventTrigger _trigger;
#if IL2CPP
        public EventTriggerBase(IntPtr ptr) : base(ptr) { }
#endif
        void Awake() {
            EnsureEventTrigger();
            Initialize(); // safe for overrides
        }

        public virtual void Initialize() {
            RegisterAllEventTriggers(); 
        }

        private void EnsureEventTrigger() {
            _trigger = gameObject.GetComponent<EventTrigger>();
            if (_trigger == null)
                _trigger = gameObject.AddComponent<EventTrigger>();

            if (_trigger.triggers == null)
#if IL2CPP
                _trigger.triggers = new Il2CppGeneric.List<EventTrigger.Entry>();
#elif MONO
                _trigger.triggers = new List<EventTrigger.Entry>();
#endif

        }

        protected void AddTrigger(EventTriggerType type, UnityAction<BaseEventData> action) {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(action);
            _trigger.triggers.Add(entry);
        }

        /// <summary>
        /// Override this in child classes to add specific triggers you need.
        /// </summary>
        public virtual void RegisterAllEventTriggers() {
            MelonLogger.Msg("Override Me");
        }
    }
}
