using HireMe.UI;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if IL2CPP
using Il2CppScheduleOne.Employees;
#elif MONO
using ScheduleOne.Employees;
#endif

namespace HireMe.UI {

#if IL2CPP
    [RegisterTypeInIl2Cpp]
#endif
    public class EmployeeListing : EventTriggerBase {
        public GameObject employeePrefab; // Prefab with DraggableItem already set up
        public Transform dragParent; // Typically the Canvas
        public GameObject currentInstance;
        public DraggableItem currentDraggable;
        public Sprite addIcon;
        public Sprite defaultIcon;
        public Text employeeCount;
        public Text signingFee;
        public Text dailyWage;
        public EEmployeeType employeeType;

#if IL2CPP
        public EmployeeListing(IntPtr ptr) : base(ptr) { }
#endif
        public override void RegisterAllEventTriggers() {
            AddTrigger(EventTriggerType.BeginDrag, (UnityAction<BaseEventData>)OnBeginDrag);
            AddTrigger(EventTriggerType.Drag, (UnityAction<BaseEventData>)OnDrag);
            AddTrigger(EventTriggerType.EndDrag, (UnityAction<BaseEventData>)OnEndDrag);
        }

        public override void Initialize() {
            base.Initialize();
            var allText = GetComponentsInChildren<Text>();
            foreach (Text text in allText) {
                if (text.name == "EmployeeCount") {
                    employeeCount = text;
                    continue;
                }

                if (text.name == "DailyWage") {
                    dailyWage = text;
                    continue;
                }

                if (text.name == "SigningFee") {
                    signingFee = text;
                    continue;
                }

            }
        }

        public void SetCountText(int quantity) {
            if (employeeCount == null) return;
            employeeCount.text = $"{quantity} x {employeeType}";
        }

        public void SetSigningText(int fee) {
            if (signingFee == null) return;
            signingFee.text = $"Signing Fee: <color=#54E717>${fee}</color>";
        }

        public void SetWageText(int wage) {
            if (dailyWage == null) return;
            dailyWage.text = $"Daily Wage: <color=#54E717>${wage}</color>";
        }

        public void OnBeginDrag(BaseEventData rawData) {
#if IL2CPP
            PointerEventData eventData = rawData.TryCast<PointerEventData>();
#elif MONO
            PointerEventData eventData = rawData as PointerEventData;
#endif

            if (eventData == null)
                return;
            // Instantiate a new employee
            currentInstance = Instantiate(employeePrefab, dragParent);
            Image image = currentInstance.GetComponent<Image>();
            image.sprite = addIcon;
            currentDraggable = currentInstance.AddComponent<DraggableItem>();
            currentDraggable.newlySpawned = true;
            currentDraggable.employeeType = this.employeeType;
            currentDraggable.currentContainer = null;

            // Set it to follow the mouse
            currentDraggable.image.raycastTarget = false;
            currentInstance.transform.position = Input.mousePosition;

            // Optional: Prevent blocking the UI below
            CanvasGroup cg = currentInstance.GetComponent<CanvasGroup>();
            if (cg != null) cg.blocksRaycasts = false;
        }

        public void OnDrag(BaseEventData rawData) {
#if IL2CPP
            PointerEventData eventData = rawData.TryCast<PointerEventData>();
#elif MONO
            PointerEventData eventData = rawData as PointerEventData;
#endif

            if (eventData == null)
                return;

            if (currentInstance != null) {
                currentInstance.transform.position = Input.mousePosition;
            }
        }

        public void ClearCurrentInstance() {
            currentInstance = null;
            currentDraggable = null;
        }

        public void OnEndDrag(BaseEventData rawData) {
#if IL2CPP
            PointerEventData eventData = rawData.TryCast<PointerEventData>();
#elif MONO
            PointerEventData eventData = rawData as PointerEventData;
#endif

            if (eventData == null)
                return;

            if (currentDraggable != null) {
                if (currentDraggable.parentAfterDrag == null) {
                    GameObject.Destroy(currentInstance);
                    currentDraggable = null;
                }
            }
        }
    }
}