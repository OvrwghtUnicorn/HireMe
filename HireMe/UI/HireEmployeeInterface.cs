using System;
using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Unity.Collections;
using UnityEngine.InputSystem;

#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppInterop.Runtime;
using Il2CppScheduleOne.Dialogue;
using Il2CppScheduleOne.Property;
using Il2CppScheduleOne.Employees;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.NPCs.CharacterClasses;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.Variables;
using Il2CppScheduleOne.UI.Phone;
using Il2CppSystem.Drawing;
using Il2CppScheduleOne.UI.Items;
#elif MONO
using ScheduleOne;
using ScheduleOne.UI;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Dialogue;
using ScheduleOne.Property;
using ScheduleOne.Employees;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.Money;
using ScheduleOne.Variables;
using ScheduleOne.UI.Phone;
using ScheduleOne.UI.Items;
#endif

namespace HireMe.UI {

#if IL2CPP
    [RegisterTypeInIl2Cpp]
#endif
    public class HireEmployeeInterface : MonoBehaviour {
        [Serializable]
        public class PropertyInfo {
            public string propertyName;
            public int employeeCount;
            public int employeeCapacity;
            public string propertyCode;
        }

        public GameObject propertyContainerPrefab;
        public EmployeeListing botanistListing;
        public EmployeeListing chemistListing;
        public EmployeeListing handlerListing;
        public EmployeeListing cleanerListing;
        public PopupHandler errorPopup;

        public Transform employeeListingsContainer;
        public Transform scrollablePropertyContainer;
        public Button payButton;
        public Canvas hireEmployeeCanvas;
        public Text totalCostLabel;
        public Text errorLabel;
        public UnityAction onClose;

        private bool isMobile = false;
        private bool showMiscUI = false;
        private string shopName = "MannysNetwork";
        private bool isOpen = false;
        private bool locationChanged = false;
        private string buttonText = "Accept";
        private bool isHandlingPayment = false;

        private Dictionary<EEmployeeType, (int count, int signingFee, int rate)> employeeStats = new() {
            {EEmployeeType.Botanist, (0, 1200, 200)},
            {EEmployeeType.Chemist, (0, 1500, 300)},
            {EEmployeeType.Handler, (0, 1200, 200)},
            {EEmployeeType.Cleaner, (0, 1000, 100)},
        };

        private Dictionary<Property, List<DraggableItem>> newPropertyAssignments = new();
        private Dictionary<Property, PropertyContainer> propertyContainers = new();
        private HashSet<Property> ownedProperties = new HashSet<Property>();
        private float cash = 20000;

        public void Setup() {
#if IL2CPP
            InitializeExitListener();
#elif MONO
            GameInput.RegisterExitListener(new GameInput.ExitDelegate(this.Exit), 7);
#endif
            InitializeComponents();
            InitializeOwnedProperties();
            InitializeProperties();
            InitializeEmployeeListings();
            RefreshUI();
        }

#if IL2CPP
        public void InitializeExitListener() {
            try {
                Action<ExitAction> della = this.Exit;
                var controlledDelegate = DelegateSupport.ConvertDelegate<GameInput.ExitDelegate>(della);
                GameInput.RegisterExitListener(controlledDelegate, 7);
            } catch (Exception e) {
                MelonLogger.Error($"Failed to register delegate: {e}");
            }
        }
#endif
        public void InitializeComponents() {
            hireEmployeeCanvas = GetComponentInParent<Canvas>();
            InitializeLayoutGroups();
            InitializeButtons();
            InitializeText();
        }

        public void InitializeLayoutGroups() {
            var layouts = GetComponentsInChildren<VerticalLayoutGroup>(true);
            foreach (VerticalLayoutGroup layout in layouts) {
                if (layout.gameObject.name == "ScrollablePropertyContent") {
                    scrollablePropertyContainer = layout.transform;
                    continue;
                }

                if (layout.gameObject.name == "EmployeeListingsContainer") {
                    employeeListingsContainer = layout.transform;
                    continue;
                }
            }
        }

        public void InitializeButtons() {
            var buttons = GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons) {
                if (button.gameObject.name == "AcceptButton") {
                    payButton = button;
                    Text text = button.GetComponentInChildren<Text>();
                    buttonText = text.text;
                    payButton.onClick.AddListener((UnityAction)HandlePaymentClick);
                    continue;
                }
            }
        }

        public void InitializeOwnedProperties() {
            ownedProperties = new HashSet<Property>(Property.OwnedProperties.ToArray());
        }

        void InitializeProperties() {
            foreach (Property property in Property.Properties) {
                CreatePropertyContainer(property);
            }
        }

        public (int signingFee, int rate) GetEmployeeCosts(EEmployeeType employee) {
            Employee employeePrefab = NetworkSingleton<EmployeeManager>.Instance.GetEmployeePrefab(employee);
            int signingFee = Mathf.RoundToInt(employeePrefab.SigningFee + Fixer.GetAdditionalSigningFee());
            int rate = Mathf.RoundToInt(employeePrefab.DailyWage);

            switch (employee) {
                case EEmployeeType.Botanist:
                    if (Core.botanistDailyWage.HasValue)
                        rate = Mathf.RoundToInt((float)Core.botanistDailyWage.Value);
                    if (Core.botanistSigningFeeEnabled == true && Core.botanistSigningFee.HasValue)
                        signingFee = Mathf.RoundToInt((float)Core.botanistSigningFee.Value);
                    break;

                case EEmployeeType.Chemist:
                    if (Core.chemistDailyWage.HasValue)
                        rate = Mathf.RoundToInt((float)Core.chemistDailyWage.Value);
                    if (Core.chemistSigningFeeEnabled == true && Core.chemistSigningFee.HasValue)
                        signingFee = Mathf.RoundToInt((float)Core.chemistSigningFee.Value);
                    break;

                case EEmployeeType.Handler:
                    if (Core.handlerDailyWage.HasValue)
                        rate = Mathf.RoundToInt((float)Core.handlerDailyWage.Value);
                    if (Core.handlerSigningFeeEnabled == true && Core.handlerSigningFee.HasValue)
                        signingFee = Mathf.RoundToInt((float)Core.handlerSigningFee.Value);
                    break;

                case EEmployeeType.Cleaner:
                    if (Core.cleanerDailyWage.HasValue)
                        rate = Mathf.RoundToInt((float)Core.cleanerDailyWage.Value);
                    if (Core.cleanerSigningFeeEnabled == true && Core.cleanerSigningFee.HasValue)
                        signingFee = Mathf.RoundToInt((float)Core.cleanerSigningFee.Value);
                    break;
            }

            return (signingFee, rate);
        }

        public void InitializeEmployeeListings() {
            try {
                var keys = new List<EEmployeeType>(employeeStats.Keys);
                foreach (EEmployeeType key in keys) {
                    var employeeCosts = GetEmployeeCosts(key);
                    employeeStats[key] = (0, employeeCosts.signingFee, employeeCosts.rate);

                    if (key == EEmployeeType.Botanist) {
                        botanistListing.employeeType = key;
                        botanistListing.SetCountText(0);
                        botanistListing.SetSigningText(employeeCosts.signingFee);
                        botanistListing.SetWageText(employeeCosts.rate);
                        continue;
                    }

                    if (key == EEmployeeType.Chemist) {
                        chemistListing.employeeType = key;
                        chemistListing.SetCountText(0);
                        chemistListing.SetSigningText(employeeCosts.signingFee);
                        chemistListing.SetWageText(employeeCosts.rate);
                        continue;
                    }

                    if (key == EEmployeeType.Handler) {
                        handlerListing.employeeType = key;
                        handlerListing.SetCountText(0);
                        handlerListing.SetSigningText(employeeCosts.signingFee);
                        handlerListing.SetWageText(employeeCosts.rate);
                        continue;
                    }

                    if (key == EEmployeeType.Cleaner) {
                        cleanerListing.employeeType = key;
                        cleanerListing.SetCountText(0);
                        cleanerListing.SetSigningText(employeeCosts.signingFee);
                        cleanerListing.SetWageText(employeeCosts.rate);
                        continue;
                    }
                }
            } catch (Exception e) {

                MelonLogger.Msg(e);
            }
        }

        public void InitializeText() {
            var texts = GetComponentsInChildren<Text>(true);
            foreach (Text text in texts) {
                if (text.gameObject.name == "TotalCostLabel") {
                    totalCostLabel = text;
                    continue;
                }

                if (text.gameObject.name == "ErrorLabel") {
                    errorLabel = text;
                    continue;
                }
            }
        }

        public void CreatePropertyContainer(Property property) {
            GameObject containerObj = Instantiate(propertyContainerPrefab, scrollablePropertyContainer);
            PropertyContainer container = containerObj.AddComponent<PropertyContainer>();
            container.currentProperty = property;
            container.popup = errorPopup;
            container.employees = property.Employees.Count;
            container.total = property.EmployeeCapacity;
            container.SetPropertyText(property.PropertyName);
            container.UpdateCountText();
            newPropertyAssignments[property] = new List<DraggableItem>();
            propertyContainers[property] = container;
            containerObj.SetActive(ownedProperties.Contains(property) && property.EmployeeCapacity > 0);
        }

        public void SortProperties() {
            if (scrollablePropertyContainer == null) return;

            List<KeyValuePair<Property, PropertyContainer>> allContainers = propertyContainers.ToList();

            // Sort the properties according to the specified criteria
            allContainers.Sort((a, b) => {
                PropertyContainer aContainer = a.Value;
                PropertyContainer bContainer = b.Value;

                // Primary sort: not at max capacity comes first
                bool aAtMax = aContainer.currentProperty.Employees.Count >= aContainer.total;
                bool bAtMax = bContainer.currentProperty.Employees.Count >= bContainer.total;
                if (aAtMax != bAtMax) {
                    return aAtMax ? 1 : -1;
                }

                // Secondary sort: ascending by total capacity
                if (aContainer.total != bContainer.total) {
                    return aContainer.total.CompareTo(bContainer.total);
                }

                // Tertiary sort: ascending by current employee count
                if (aContainer.currentProperty.Employees.Count != bContainer.currentProperty.Employees.Count) {
                    return aContainer.currentProperty.Employees.Count.CompareTo(bContainer.currentProperty.Employees.Count);
                }

                // Final tie-breaker: alphabetical by property name
                return string.Compare(aContainer.currentProperty.PropertyName,
                                    bContainer.currentProperty.PropertyName,
                                    StringComparison.Ordinal);
            });

            // Reorder the children in the container
            for (int i = 0; i < allContainers.Count; i++) {
                allContainers[i].Value.transform.SetSiblingIndex(i);
            }
        }

        public void Open() {
            isMobile = true;
            showMiscUI = true;
            SetIsOpen(true);
        }

        public void SetIsOpen(bool isOpen) {
            this.isOpen = isOpen;
            PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(this.shopName);
            if (isOpen) {
                if (isMobile) {
                    Singleton<ItemUIManager>.instance.InputsContainer.gameObject.SetActive(false);
                    Singleton<GameplayMenuInterface>.instance.Close();
                    PlayerSingleton<AppsCanvas>.Instance.SetIsOpen(true);
                    PlayerSingleton<HomeScreen>.Instance.SetIsOpen(false);
                    PlayerSingleton<Phone>.Instance.SetIsHorizontal(true);
                    PlayerSingleton<Phone>.Instance.SetLookOffsetMultiplier(0.6f);
                } else {
                    PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(this.shopName);
                    PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
                    PlayerSingleton<PlayerCamera>.Instance.SetCanLook(false);
                    PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
                    PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(true);
                    PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(false);
                    Singleton<InputPromptsCanvas>.Instance.LoadModule("exitonly");
                }
                cash = NetworkSingleton<MoneyManager>.Instance.cashBalance;

                foreach (Property property in Property.OwnedProperties) {
                    if (property.EmployeeCapacity <= 0) continue;
                    if (!propertyContainers.ContainsKey(property)) continue;

                    LoadCurrentEmployees(property);

                    PropertyContainer container = propertyContainers[property];
                    if (container == null) continue;
                    if (container.currentProperty == null) continue;
                    if (!ownedProperties.Contains(property)) ownedProperties.Add(property);
                    if (!container.gameObject.activeInHierarchy) container.gameObject.SetActive(true);

                    if (newPropertyAssignments.ContainsKey(container.currentProperty)) {
                        container.employees = newPropertyAssignments[container.currentProperty].Count;
                    }

                    container.total = property.EmployeeCapacity;
                    container.UpdateCountText();
                    if (container.currentProperty.Employees.Count >= container.total) {
                        container.ShrinkContainer();
                    } else {
                        container.ExpandContainer();
                    }
                }

                SortProperties();
                RefreshUI();
            } else {
                if (isMobile) {
                    isMobile = false;
                    PlayerSingleton<AppsCanvas>.Instance.SetIsOpen(false);
                    PlayerSingleton<HomeScreen>.Instance.SetIsOpen(true);
                    PlayerSingleton<Phone>.Instance.SetIsHorizontal(false);
                    PlayerSingleton<Phone>.Instance.SetLookOffsetMultiplier(1f);
                    Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
                    if (showMiscUI) {
                        Singleton<ItemUIManager>.instance.InputsContainer.gameObject.SetActive(false);
                        Singleton<GameplayMenuInterface>.instance.Open();
                    }
                    //onClose?.Invoke();
                } else {
                    PlayerSingleton<PlayerCamera>.Instance.LockMouse();
                    PlayerSingleton<PlayerCamera>.Instance.SetCanLook(true);
                    PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
                    PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(true);
                    Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
                    Singleton<InputPromptsCanvas>.Instance.UnloadModule();
                }
            }
            hireEmployeeCanvas.enabled = isOpen;
            hireEmployeeCanvas.gameObject.SetActive(isOpen);
        }

        public void LoadCurrentEmployees(Property property) {
            PropertyContainer container = propertyContainers[property];
            if (container == null) return;
            foreach (Employee emp in property.Employees) {
                if (emp == null) continue;
                if (newPropertyAssignments[property].Any(item => (item.currentEmployee.fullName == emp.fullName && item.currentEmployee.EmployeeType == emp.EmployeeType))) continue;
                GameObject go = null;
                switch (emp.Type) {
                    case EEmployeeType.Botanist:
                        go = GameObject.Instantiate(botanistListing.employeePrefab, container.employeeContainer);
                        break;
                    case EEmployeeType.Chemist:
                        go = GameObject.Instantiate(chemistListing.employeePrefab, container.employeeContainer);
                        break;
                    case EEmployeeType.Handler:
                        go = GameObject.Instantiate(handlerListing.employeePrefab, container.employeeContainer);
                        break;
                    case EEmployeeType.Cleaner:
                        go = GameObject.Instantiate(cleanerListing.employeePrefab, container.employeeContainer);
                        break;
                }
                DraggableItem draggable = go.AddComponent<DraggableItem>();
                draggable.employeeType = emp.Type;
                draggable.currentEmployee = emp;
                draggable.currentContainer = container;
                newPropertyAssignments[property].Add(draggable);
            }
        }

        public void Exit(ExitAction action) {
            if (action.Used) {
                return;
            }
            if (this.isOpen) {
                action.Used = true;
                this.SetIsOpen(false);
            }
        }

        private void UpdateEmployeeListingCount(EEmployeeType employeeType) {
            int count = employeeStats[employeeType].count;
            switch (employeeType) {
                case EEmployeeType.Botanist:
                    botanistListing.SetCountText(count);
                    break;
                case EEmployeeType.Chemist:
                    chemistListing.SetCountText(count);
                    break;
                case EEmployeeType.Handler:
                    handlerListing.SetCountText(count);
                    break;
                case EEmployeeType.Cleaner:
                    cleanerListing.SetCountText(count);
                    break;
            }
        }

        public void OnNewEmployeeDropped(DraggableItem newEmployee, PropertyContainer targetContainer) {
            newEmployee.currentContainer = targetContainer;
            OnEmployeeAssigned(newEmployee.employeeType, targetContainer.currentProperty, newEmployee);
            targetContainer.employees += 1;
            targetContainer.UpdateCountText();
        }

        public void OnEmployeeMoved(DraggableItem item, PropertyContainer targetContainer) {
            var previousContainer = item.currentContainer;
            if (previousContainer == targetContainer) return;
            if (!item.newlySpawned && !payButton.interactable) {
                locationChanged = true;
            }

            OnEmployeeRemoved(previousContainer.currentProperty, item);
            previousContainer.employees -= 1;
            previousContainer.UpdateCountText();

            item.currentContainer = targetContainer;
            targetContainer.employees += 1;
            targetContainer.UpdateCountText();
            OnEmployeeAssigned(item.employeeType, targetContainer.currentProperty, item);
        }

        public void OnEmployeeAssigned(EEmployeeType employeeType, Property curr, DraggableItem instance) {
            if (instance.newlySpawned) {
                employeeStats[employeeType] = (employeeStats[employeeType].count + 1, employeeStats[employeeType].signingFee, employeeStats[employeeType].rate);
            }
            newPropertyAssignments[curr].Add(instance);
            UpdateEmployeeListingCount(employeeType);
            RefreshUI();
        }

        public bool OnEmployeeRemoved(Property curr, DraggableItem instance) {

            EEmployeeType employeeType = instance.employeeType;
            bool success = false;
            if (instance.newlySpawned) {
                employeeStats[employeeType] = (employeeStats[employeeType].count - 1, employeeStats[employeeType].signingFee, employeeStats[employeeType].rate);
            }
            if (newPropertyAssignments.ContainsKey(curr)) {
                success = newPropertyAssignments[curr].Remove(instance);
            }

            if (success) {
                UpdateEmployeeListingCount(employeeType);
                RefreshUI();
            }

            return success;

        }

        void RefreshUI() {
            int total = GetTotalCost();
            bool overBudget = total > cash;
            string textColor = !overBudget ? "#000000" : "#FF0004";
            totalCostLabel.text = $"<color={textColor}>${total}</color>";

            errorLabel.gameObject.SetActive(overBudget);
            if (overBudget)
                errorLabel.text = "Not enough cash!";

            payButton.interactable = !overBudget && (total > 0 || locationChanged);
        }

        public int GetNewEmployees() {
            int count = 0;
            foreach (var kvp in newPropertyAssignments) {
                var list = kvp.Value;
                foreach (var employee in list) {
                    if (employee.newlySpawned) {
                        count++;
                    }
                }
            }

            return count;
        }

        public int GetTotalCost() {
            int total = 0;
            foreach (var kvp in employeeStats) {
                int subtotal = kvp.Value.count * kvp.Value.signingFee;
                total += subtotal;
            }
            return total;
        }

        public void HandlePaymentClick() {
            if (isHandlingPayment) return;
            isHandlingPayment = true;
#if IL2CPP
            MelonCoroutines.Start(HandlePayment((Action)HandlePaymentComplete));
#else
            StartCoroutine(HandlePayment(HandlePaymentComplete));
#endif
        }

        public void HandlePaymentComplete() {
            isHandlingPayment = false;
            Text payButtonText = payButton.GetComponentInChildren<Text>();
            payButtonText.text = buttonText;
            totalCostLabel.text = "<color=#000000>$0</color>";
            payButton.interactable = false;
            employeeStats[EEmployeeType.Botanist] = (0, employeeStats[EEmployeeType.Botanist].signingFee, employeeStats[EEmployeeType.Botanist].rate);
            botanistListing.SetCountText(0);

            employeeStats[EEmployeeType.Chemist] = (0, employeeStats[EEmployeeType.Chemist].signingFee, employeeStats[EEmployeeType.Chemist].rate);
            chemistListing.SetCountText(0);

            employeeStats[EEmployeeType.Handler] = (0, employeeStats[EEmployeeType.Handler].signingFee, employeeStats[EEmployeeType.Handler].rate);
            handlerListing.SetCountText(0);

            employeeStats[EEmployeeType.Cleaner] = (0, employeeStats[EEmployeeType.Cleaner].signingFee, employeeStats[EEmployeeType.Cleaner].rate);
            cleanerListing.SetCountText(0);
            this.SetIsOpen(false);
        }

        public IEnumerator HandlePayment(Action OnComplete = null) {
            if (payButton == null) {
                MelonLogger.Error("payButton is null.");
                OnComplete?.Invoke();
                yield break;
            }

            Text payButtonText = payButton.GetComponentInChildren<Text>();
            if (payButtonText == null) {
                MelonLogger.Error("payButtonText is null.");
                OnComplete?.Invoke();
                yield break;
            }

            payButton.interactable = false;
            string baseText = "Processing";
            int dotCount = 0;
            float interval = 0.3f;
            bool isHiringDone = false;

            if (this != null) {
                MelonCoroutines.Start(HireEmployees(() => isHiringDone = true));
            } else {
                MelonLogger.Error("HireEmployeeInterface is null while starting HireEmployees.");
                OnComplete?.Invoke();
                yield break;
            }

            while (!isHiringDone) {
                if (payButtonText != null)
                    payButtonText.text = baseText + new string('.', dotCount);
                dotCount = (dotCount + 1) % 3;
                yield return new WaitForSeconds(interval);
            }

            foreach (var kvp in newPropertyAssignments) {
                var list = kvp.Value;

                List<(int index, GameObject go)> temp = new List<(int index, GameObject go)>();

                for (int i = 0; i < list.Count; i++) {
                    DraggableItem item = list[i];
                    if (!item.newlySpawned) continue;
                    if (item != null && item.gameObject != null) {
                        temp.Add((i, item.gameObject));
                    } else {
                        MelonLogger.Warning("Tried to destroy null item or item.gameObject.");
                    }
                }

                for (var j = temp.Count - 1; j >= 0; j--) {
                    list.RemoveAt(temp[j].index);
                    GameObject.Destroy(temp[j].go);
                }


            }

            OnComplete?.Invoke();
        }

        private IEnumerator HireEmployees(Action onComplete) {
            if (!NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("ClipboardAcquired")) {
                NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("ClipboardAcquired", true.ToString(), true);
            }

            foreach (var kvp in newPropertyAssignments) {
                Property property = kvp.Key;
                List<DraggableItem> employees = kvp.Value;

                foreach (var employee in employees) {
                    if (employee.newlySpawned) {
                        var type = employee.employeeType;
                        var stats = employeeStats[type];
                        int totalFee = stats.signingFee;

                        NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(-totalFee, true, false);
                        NetworkSingleton<EmployeeManager>.Instance.CreateNewEmployee(property, type);
                        continue;
                    }
                    if (property.PropertyCode != employee.currentEmployee.AssignedProperty.PropertyCode) {
                        MelonLogger.Msg($"{property.PropertyCode} | {employee.currentEmployee.AssignedProperty.PropertyCode}");
                        employee.currentEmployee.SendTransfer(property.PropertyCode);
                    }
                    yield return new WaitForSeconds(0.1f);
                }
            }

            onComplete?.Invoke();
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Tab) && isOpen) {
                showMiscUI = false;
                SetIsOpen(false);
            }
        }
    }
}
