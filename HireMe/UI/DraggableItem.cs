using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using static MelonLoader.MelonLogger;

#if IL2CPP
using Il2CppScheduleOne.Employees;
using Il2CppScheduleOne.Property;
#elif MONO
using ScheduleOne.Employees;
using ScheduleOne.Property;
#endif

namespace HireMe.UI {

#if IL2CPP
    [RegisterTypeInIl2Cpp]
#endif
    public class DraggableItem : EventTriggerBase {
        public bool newlySpawned = false;
        public Button deleteButton;
        public Image image;
        public Transform parentAfterDrag;
        public EEmployeeType employeeType;
        public Employee currentEmployee;
        public PropertyContainer currentContainer;

#if IL2CPP
        public DraggableItem(IntPtr ptr) : base(ptr) { }
#endif

        public override void RegisterAllEventTriggers() {
            AddTrigger(EventTriggerType.BeginDrag, (UnityAction<BaseEventData>)OnBeginDrag);
            AddTrigger(EventTriggerType.Drag, (UnityAction<BaseEventData>)OnDrag);
            AddTrigger(EventTriggerType.EndDrag, (UnityAction<BaseEventData>)OnEndDrag);
            AddTrigger(EventTriggerType.PointerEnter, (UnityAction<BaseEventData>)OnPointerEnter);
            AddTrigger(EventTriggerType.PointerExit, (UnityAction<BaseEventData>)OnPointerExit);
        }

        public override void Initialize() {
            base.Initialize();
            image = GetComponent<Image>();
            deleteButton = GetComponentInChildren<Button>(true);
            if (deleteButton != null) {
                deleteButton.onClick.AddListener((UnityAction)OnDelete);
            }
        }
        public void OnBeginDrag(BaseEventData rawData) {
#if IL2CPP
            PointerEventData eventData = rawData.TryCast<PointerEventData>();
#elif MONO
            PointerEventData eventData = rawData as PointerEventData;
#endif

            if (eventData == null)
                return;
            parentAfterDrag = transform.parent;
            transform.SetParent(UIManager.hiringInterface.hireEmployeeCanvas.transform);
            transform.SetAsLastSibling();
            image.raycastTarget = false;
        }

        public void OnDelete() {
            if (currentContainer == null) return;
            currentContainer.employees -= 1;
            currentContainer.UpdateCountText();
            if (UIManager.hiringInterface.OnEmployeeRemoved(currentContainer.currentProperty, this)) {
                if (!newlySpawned) {
                    currentEmployee.Fire();
                }
                GameObject.Destroy(gameObject);
            }
        }

        public void OnDrag(BaseEventData rawData) {
#if IL2CPP
            PointerEventData eventData = rawData.TryCast<PointerEventData>();
#elif MONO
            PointerEventData eventData = rawData as PointerEventData;
#endif

            if (eventData == null)
                return;
            transform.position = Input.mousePosition;
        }

        public void OnEndDrag(BaseEventData rawData) {
#if IL2CPP
            PointerEventData eventData = rawData.TryCast<PointerEventData>();
#elif MONO
            PointerEventData eventData = rawData as PointerEventData;
#endif

            if (eventData == null)
                return;
            transform.SetParent(parentAfterDrag);
            image.raycastTarget = true;
        }

        public void OnPointerEnter(BaseEventData rawData) {
#if IL2CPP
            PointerEventData eventData = rawData.TryCast<PointerEventData>();
#elif MONO
            PointerEventData eventData = rawData as PointerEventData;
#endif

            if (eventData == null)
                return;
            deleteButton.gameObject.SetActive(true);
        }

        public void OnPointerExit(BaseEventData rawData) {
#if IL2CPP
            PointerEventData eventData = rawData.TryCast<PointerEventData>();
#elif MONO
            PointerEventData eventData = rawData as PointerEventData;
#endif

            if (eventData == null)
                return;
            deleteButton.gameObject.SetActive(false);
        }
    }
}
