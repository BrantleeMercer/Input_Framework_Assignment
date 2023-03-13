using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;
using Scripts.Helpers;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt,
            Forward, 
            Back, 
            Left, 
            Right,
        }

        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;
        
        /// <summary>
        /// Reference to the drone ascend key (Default to <see cref="KeyCode.Space"/> key)
        /// </summary>
        [SerializeField, Tooltip("Reference to the drone ascend key (Default to \'Space\' key)"), Header("Drone Input References")]
        public InputActionReference DroneAscendKeyReference;
        /// <summary>
        /// Reference to the drone descend key (Default to <see cref="KeyCode.V"/> key)
        /// </summary>
        [SerializeField, Tooltip("Reference to the drone ascend key (Default to \'V\' key)")]
        public InputActionReference DroneDescendKeyReference;
        /// <summary>
        /// Reference to the drone movement keys (Default to 'WASD')
        /// </summary>
        [SerializeField, Tooltip("Reference to the drone movement keys (Default to \'WASD\' keys)")]
        public InputActionReference DroneMovementReference;
        /// <summary>
        /// Reference to the drone yaw keys (Default to <see cref="KeyCode.LeftArrow"/> and <see cref="KeyCode.RightArrow"/>)
        /// </summary>
        [SerializeField, Tooltip("Reference to the drone yaw (Default to left and right arrow keys)")]
        public InputActionReference DroneYawReference;

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        private bool _ascendPressed = false;
        private bool _descendPressed = false;

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);

                DroneAscendKeyReference.action.Enable(); 
                DroneAscendKeyReference.action.started += DroneAscendKeyHasBeenPressed;
                DroneAscendKeyReference.action.canceled += DroneAscendKeyHasBeenReleased;
                
                DroneDescendKeyReference.action.Enable();
                DroneDescendKeyReference.action.started += DroneDescendKeyHasBeenPressed;
                DroneDescendKeyReference.action.canceled += DroneDescendKeyHasBeenReleased;
                
                DroneMovementReference.action.Enable();
                DroneYawReference.action.Enable();
                
                ActionMapManager.OnEscapeKeyPressed += ExitFlightModePressed;
            }
        }

        private void ExitFlightMode()
        {            
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);
            
            DroneAscendKeyReference.action.Disable();
            DroneAscendKeyReference.action.started -= DroneAscendKeyHasBeenPressed;
            DroneAscendKeyReference.action.canceled -= DroneAscendKeyHasBeenReleased;
            
            
            DroneDescendKeyReference.action.Disable();
            DroneDescendKeyReference.action.started -= DroneDescendKeyHasBeenPressed;
            DroneDescendKeyReference.action.canceled -= DroneDescendKeyHasBeenReleased;

            DroneMovementReference.action.Disable();
            DroneYawReference.action.Disable();
            
            ActionMapManager.OnEscapeKeyPressed -= ExitFlightModePressed;
        }

        private void CalculateMovementUpdate()
        {
            float yawDirection = DroneYawReference.action.ReadValue<float>();
            
            if (yawDirection < 0)
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y -= _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
            if (yawDirection > 0)
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y += _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
        }

        private void CalculateMovementFixedUpdate()
        {
            if (_ascendPressed)
            {
                _rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration);
            }
            if (_descendPressed)
            {
                _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);
            }
        }

        private void CalculateTilt()
        {
            float h = DroneMovementReference.action.ReadValue<Vector2>().x;
            float v = DroneMovementReference.action.ReadValue<Vector2>().y;
            
            if (h == 0 && v == 0)
            {
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
                return;
            }
                            
            if (h < 0) 
                transform.rotation = Quaternion.Euler(00, transform.localRotation.eulerAngles.y, 30);
            else if (h > 0)
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, -30);
            
            if (v > 0)
                transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
            else if (v < 0)
                transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
        }
        
        private void DroneAscendKeyHasBeenPressed(InputAction.CallbackContext context)
        {
            _ascendPressed = true;
        }
        
        private void DroneAscendKeyHasBeenReleased(InputAction.CallbackContext context)
        {
            _ascendPressed = false;
        }
        
        private void DroneDescendKeyHasBeenPressed(InputAction.CallbackContext context)
        {
            _descendPressed = true;
        }
        
        private void DroneDescendKeyHasBeenReleased(InputAction.CallbackContext context)
        {
            _descendPressed = false;
        }

        private void ExitFlightModePressed()
        {
            _inFlightMode = false;
            onExitFlightmode?.Invoke();
            ExitFlightMode();
        }

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }
        
        private void Update()
        {
            if (_inFlightMode)
            {
                CalculateTilt();
                CalculateMovementUpdate();
            }
        }
    }
}
