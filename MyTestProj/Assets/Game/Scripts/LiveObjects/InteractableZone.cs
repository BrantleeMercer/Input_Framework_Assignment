using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.UI;
using Scripts.Helpers;
using UnityEngine.InputSystem;


namespace Game.Scripts.LiveObjects
{
    public class InteractableZone : MonoBehaviour
    {
        private enum ZoneType
        {
            Collectable,
            Action,
            HoldAction
        }

        private enum KeyState
        {
            Press,
            PressHold
        }

        [SerializeField]
        private ZoneType _zoneType;
        [SerializeField]
        private int _zoneID;
        [SerializeField]
        private int _requiredID;
        [SerializeField]
        [Tooltip("Press the (---) Key to .....")]
        private string _displayMessage;
        [SerializeField]
        private GameObject[] _zoneItems;
        private bool _inZone = false;
        private bool _itemsCollected = false;
        private bool _actionPerformed = false;
        [SerializeField]
        private Sprite _inventoryIcon;
        [SerializeField]
        private KeyCode _zoneKeyInput;
        [SerializeField]
        private KeyState _keyState;
        [SerializeField]
        private GameObject _marker;
        /// <summary>
        /// Reference to the main interact key (Default to 'E' key)
        /// </summary>
        [SerializeField, Tooltip("Reference to the main interact key (Default to \'E\' key)"), Header("Input References")]
        public InputActionReference MainKeyReference;
        /// <summary>
        /// Reference to the Secondary interact key (Default to 'Space' key)
        /// </summary>
        [SerializeField, Tooltip("Reference to the Secondary interact key (Default to \'Space\' key)")]
        public InputActionReference SecondaryKeyReference;

        private bool _inHoldState = false;
        private bool _mainKeyPressed = false;
        private bool _mainKeyHeld = false;
        private bool _secondaryKeyPressed = false;

        private static int _currentZoneID = 0;
        public static int CurrentZoneID
        { 
            get 
            { 
               return _currentZoneID; 
            }
            set
            {
                _currentZoneID = value; 
                         
            }
        }

        public static event Action<InteractableZone> onZoneInteractionComplete;
        public static event Action<int> onHoldStarted;
        public static event Action<int> onHoldEnded;

        private void CollectItems()
        {
            foreach (var item in _zoneItems)
            {
                item.SetActive(false);
            }

            UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            CompleteTask(_zoneID);

            onZoneInteractionComplete?.Invoke(this);

        }

        private void PerformAction()
        {
            foreach (var item in _zoneItems)
            {
                item.SetActive(true);
            }

            if (_inventoryIcon != null)
                UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            onZoneInteractionComplete?.Invoke(this);
        }

        private void PerformHoldAction()
        {
            UIManager.Instance.DisplayInteractableZoneMessage(false);
            onHoldStarted?.Invoke(_zoneID);
        }

        public GameObject[] GetItems()
        {
            return _zoneItems;
        }

        public int GetZoneID()
        {
            return _zoneID;
        }

        public void CompleteTask(int zoneID)
        {
            if (zoneID == _zoneID)
            {
                _currentZoneID++;
                ResetInteractableZones.instance.DoResetAction();
                onZoneInteractionComplete?.Invoke(this);
            }
        }

        public void ResetAction(int zoneID)
        {
            if (zoneID == _zoneID)
                _actionPerformed = false;
        }

        public void SetMarker(InteractableZone zone)
        {
            if (_zoneID == _currentZoneID)
                _marker.SetActive(true);
            else
                _marker.SetActive(false);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && _currentZoneID > _requiredID)
            {
                switch (_zoneType)
                {
                    case ZoneType.Collectable:
                        if (_itemsCollected == false)
                        {
                            _inZone = true;
                            if (_displayMessage != null)
                            {
                                string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the {_zoneKeyInput.ToString()} key to collect");
                        }
                        break;

                    case ZoneType.Action:
                        if (_actionPerformed == false)
                        {
                            _inZone = true;
                            if (_displayMessage != null)
                            {
                                string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the {_zoneKeyInput.ToString()} key to perform action");
                        }
                        break;

                    case ZoneType.HoldAction:
                        _inZone = true;
                        if (_displayMessage != null)
                        {
                            string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                            UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                        }
                        else
                            UIManager.Instance.DisplayInteractableZoneMessage(true, $"Hold the {_zoneKeyInput.ToString()} key to perform action");
                        break;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _inZone = false;
                UIManager.Instance.DisplayInteractableZoneMessage(false);
            }
        }
        
        private void MainActionKeyStopped(InputAction.CallbackContext context)
        {
            Debug.Log("Main Key Press Let Go");
            _mainKeyHeld = false;
            _mainKeyPressed = false;
        }
        
        private void MainActionKeyStarted(InputAction.CallbackContext context)
        {
            Debug.Log("Main Key Press Held");
            _mainKeyHeld = true;
        }

        private void MainActionKeyPressed(InputAction.CallbackContext context)
        {
            Debug.Log("Main Key Pressed");
            _mainKeyPressed = true;
        }

        private void SecondaryActionKeyPerformed(InputAction.CallbackContext context)
        {
            Debug.Log("Secondary Key Pressed");
            _secondaryKeyPressed = true;
        }
        
        private void SecondaryActionKeyStopped(InputAction.CallbackContext context)
        {
            Debug.Log("Secondary Key Press Stopped");
            _secondaryKeyPressed = false;
        }
        
        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += SetMarker;
            
            MainKeyReference.action.Enable();
            MainKeyReference.action.performed += MainActionKeyPressed;
            MainKeyReference.action.started += MainActionKeyStarted;
            MainKeyReference.action.canceled += MainActionKeyStopped;
            
            SecondaryKeyReference.action.Enable();
            SecondaryKeyReference.action.performed += SecondaryActionKeyPerformed;
            SecondaryKeyReference.action.canceled += SecondaryActionKeyStopped;
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= SetMarker;
            
            MainKeyReference.action.Disable();
            MainKeyReference.action.performed -= MainActionKeyPressed;
            MainKeyReference.action.started -= MainActionKeyStarted;
            MainKeyReference.action.canceled -= MainActionKeyStopped;
            
            SecondaryKeyReference.action.Disable();
            SecondaryKeyReference.action.performed -= SecondaryActionKeyPerformed;
            SecondaryKeyReference.action.canceled -= SecondaryActionKeyStopped;
        } 
        
        private void Update()
        {
            if (_inZone == true)
            {
                //Detonate C4
                if (CurrentZoneID == 2)
                {
                    if (_secondaryKeyPressed && _keyState != KeyState.PressHold)
                    {
                        if (_actionPerformed == false)
                        {
                            PerformAction();
                            _actionPerformed = true;
                            UIManager.Instance.DisplayInteractableZoneMessage(false);
                        }
                    }
                    return;
                }

                //Everything else
                if (_mainKeyPressed && _keyState != KeyState.PressHold)
                {
                    //press
                    switch (_zoneType)
                    {
                        case ZoneType.Collectable:
                            if (_itemsCollected == false)
                            {
                                CollectItems();
                                _itemsCollected = true;
                                UIManager.Instance.DisplayInteractableZoneMessage(false);
                            }
                            break;

                        case ZoneType.Action:
                            if (_actionPerformed == false)
                            {
                                PerformAction();
                                _actionPerformed = true;
                                UIManager.Instance.DisplayInteractableZoneMessage(false);
                            }
                            break;
                    }
                }
                else if (_mainKeyHeld && _keyState == KeyState.PressHold && _inHoldState == false)
                {
                    _inHoldState = true;

                   

                    switch (_zoneType)
                    {                      
                        case ZoneType.HoldAction:
                            PerformHoldAction();
                            break;           
                    }
                }

                if (!_mainKeyHeld && _keyState == KeyState.PressHold)
                {
                    _inHoldState = false;
                    onHoldEnded?.Invoke(_zoneID);
                }

               
            }
        }

    }
}


