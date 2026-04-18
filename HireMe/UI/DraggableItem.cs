using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using static MelonLoader.MelonLogger;
using UnityEngine.UIElements.UIR;


#if IL2CPP
using Il2CppScheduleOne.Employees;
using Il2CppScheduleOne.Property;
#elif MONO
using ScheduleOne.Employees;
using ScheduleOne.Property;
#endif

namespace HireMe.UI
{

#if IL2CPP
    [RegisterTypeInIl2Cpp]
#endif
    public class DraggableItem : EventTriggerBase
    {
        public bool newlySpawned = false;
        public Button deleteButton;
        public Image image;
        public Transform parentAfterDrag;
        public EEmployeeType employeeType;
        public Employee currentEmployee;
        public PropertyContainer currentContainer;
        public Text nameLabel;
        private GameObject nameLabelInstance;   // created at runtime for existing employees

#if IL2CPP
        public DraggableItem(IntPtr ptr) : base(ptr) { }
#endif

        public override void RegisterAllEventTriggers()
        {
            AddTrigger(EventTriggerType.BeginDrag, (UnityAction<BaseEventData>)OnBeginDrag);
            AddTrigger(EventTriggerType.Drag, (UnityAction<BaseEventData>)OnDrag);
            AddTrigger(EventTriggerType.EndDrag, (UnityAction<BaseEventData>)OnEndDrag);
            AddTrigger(EventTriggerType.PointerEnter, (UnityAction<BaseEventData>)OnPointerEnter);
            AddTrigger(EventTriggerType.PointerExit, (UnityAction<BaseEventData>)OnPointerExit);
        }

        private void EnsureNameLabel()
        {
            if (nameLabelInstance != null) return;
            if (UIManager.nameLabelPrefab == null)
            {
                MelonLogger.Warning("[HireMe] nameLabelPrefab is null — cannot create name label.");
                return;
            }

            nameLabelInstance = GameObject.Instantiate(UIManager.nameLabelPrefab, transform, false);
            RectTransform rt = nameLabelInstance.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, 28f);
            rt.anchoredPosition = Vector2.zero;
            nameLabel = nameLabelInstance.transform.GetChild(0).GetComponent<Text>();
            nameLabelInstance.SetActive(false);
        }

        public override void Initialize()
        {
            base.Initialize();
            image = GetComponent<Image>();
            deleteButton = GetComponentInChildren<Button>(true);
            if (deleteButton != null)
            {
                deleteButton.onClick.AddListener((UnityAction)OnDelete);
            }
            EnsureNameLabel();
        }

        /// <summary>
        /// Shows the name label and sets its text. Only called for existing employees.
        /// </summary>
        public void SetNameLabel(string fullName)
        {
            EnsureNameLabel();
            if (nameLabelInstance == null || nameLabel == null)
            {
                MelonLogger.Warning($"[HireMe] Name label could not be created for {fullName}.");
                return;
            }

            nameLabel.text = fullName;
            nameLabelInstance.SetActive(true);
        }

        public void OnBeginDrag(BaseEventData rawData)
        {
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

        public void OnDelete()
        {
            if (currentContainer == null) return;
            currentContainer.employees -= 1;
            currentContainer.UpdateCountText();
            if (UIManager.hiringInterface.OnEmployeeRemoved(currentContainer.currentProperty, this))
            {
                if (!newlySpawned)
                {
                    currentEmployee.Fire();
                }
                GameObject.Destroy(gameObject);
            }
        }

        public void OnDrag(BaseEventData rawData)
        {
#if IL2CPP
            PointerEventData eventData = rawData.TryCast<PointerEventData>();
#elif MONO
            PointerEventData eventData = rawData as PointerEventData;
#endif

            if (eventData == null)
                return;
            transform.position = Input.mousePosition;
        }

        public void OnEndDrag(BaseEventData rawData)
        {
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

        public void OnPointerEnter(BaseEventData rawData)
        {
#if IL2CPP
            PointerEventData eventData = rawData.TryCast<PointerEventData>();
#elif MONO
            PointerEventData eventData = rawData as PointerEventData;
#endif

            if (eventData == null)
                return;
            deleteButton.gameObject.SetActive(true);
        }

        public void OnPointerExit(BaseEventData rawData)
        {
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
