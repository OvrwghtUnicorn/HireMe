using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if IL2CPP
using Il2CppScheduleOne.Property;
using Il2CppScheduleOne.Employees;
#elif MONO
using ScheduleOne.Property;
using ScheduleOne.Employees;
#endif

namespace HireMe.UI {

#if IL2CPP
    [RegisterTypeInIl2Cpp]
#endif
    public class PropertyContainer : EventTriggerBase {
        public PopupHandler popup;
        public Transform employeeContainer;
        public Button shrink;
        public Button expand;
        public Text propertyLabel;
        public Text employeeCount;
        public Property currentProperty;
        public HireEmployeeInterface hireInterface;
        public int employees { get; set; }
        public int total { get; set; }

#if IL2CPP
        public PropertyContainer(IntPtr ptr) : base(ptr) { }
#endif

        public override void Initialize() {
            base.Initialize();
            hireInterface = FindObjectOfType<HireEmployeeInterface>();
            employeeContainer = transform.GetComponentInChildren<GridLayoutGroup>().transform;
            InitializeShrinkButton();
            InitializeExpandButton();
            InitializePropertyLabel();
            InitializeEmployeeCount();
            UpdateCountText();
        }

        public override void RegisterAllEventTriggers() {
            AddTrigger(EventTriggerType.Drop, (UnityAction<BaseEventData>)OnDrop);
        }

        void InitializeShrinkButton() {
            Transform shrinkTransform = transform.Find("Header/Shrink");
            if (shrinkTransform != null) {
                shrink = shrinkTransform.GetComponent<Button>();
                shrink.onClick.AddListener((UnityAction)ShrinkContainer);
            }
        }

        void InitializeExpandButton() {
            Transform expandTransform = transform.Find("Header/Expand");
            if (expandTransform != null) {
                expand = expandTransform.GetComponent<Button>();
                expand.onClick.AddListener((UnityAction)ExpandContainer);
            }
        }

        void InitializePropertyLabel() {
            Transform propertyLabelTransform = transform.Find("Header/PropertyLabel");
            if (propertyLabelTransform != null) {
                propertyLabel = propertyLabelTransform.GetComponent<Text>();
            }
        }

        void InitializeEmployeeCount() {
            Transform employeeCountTransform = transform.Find("Header/EmployeeCount");
            if (employeeCountTransform != null) {
                employeeCount = employeeCountTransform.GetComponent<Text>();
            }
        }

        public void SetPropertyText(string newText) {
            if (propertyLabel == null) return;
            propertyLabel.text = newText;
        }

        public void UpdateCountText() {
            if (employeeCount == null) return;
            employeeCount.text = $"{employees} / {total}";
        }

        public void ShrinkContainer() {
            employeeContainer.gameObject.SetActive(false);
            shrink.gameObject.SetActive(false);
            expand.gameObject.SetActive(true);
        }

        public void ExpandContainer() {
            employeeContainer.gameObject.SetActive(true);
            shrink.gameObject.SetActive(true);
            expand.gameObject.SetActive(false);
        }

        public void OnDrop(BaseEventData rawData) {
#if IL2CPP
            PointerEventData eventData = rawData.TryCast<PointerEventData>();
#elif MONO
            PointerEventData eventData = rawData as PointerEventData;
#endif

            if (eventData == null)
                return;
            GameObject dropped = eventData.pointerDrag;
            EmployeeListing newDraggableItem = dropped.GetComponent<EmployeeListing>();
            if (newDraggableItem != null) {
                if (employees + 1 > total) {
                    popup.OpenPopup();
                    return;
                }
                DraggableItem newEmployee = newDraggableItem.currentDraggable;
                if (newEmployee == null) {
                    newEmployee = newDraggableItem.currentInstance.GetComponent<DraggableItem>();
                }
                newEmployee.newlySpawned = true;
                newEmployee.transform.SetParent(employeeContainer);
                newEmployee.image.raycastTarget = true;

                hireInterface.OnNewEmployeeDropped(newEmployee, this);

                newDraggableItem.ClearCurrentInstance();
                return;
            }

            DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
            if (draggableItem != null) {
                if (employees + 1 > total && draggableItem.parentAfterDrag.transform != employeeContainer.transform) {
                    popup.OpenPopup();
                    return;
                }
                draggableItem.parentAfterDrag = employeeContainer;
                hireInterface.OnEmployeeMoved(draggableItem, this);
                return;
            }
        }
    }
}
