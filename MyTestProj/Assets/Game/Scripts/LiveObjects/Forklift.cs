using System;
using UnityEngine;
using Cinemachine;
using Scripts.Helpers;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Forklift : MonoBehaviour
    {
        [SerializeField]
        private GameObject _lift, _steeringWheel, _leftWheel, _rightWheel, _rearWheels;
        [SerializeField]
        private Vector3 _liftLowerLimit, _liftUpperLimit;
        [SerializeField]
        private float _speed = 5f, _liftSpeed = 1f;
        [SerializeField]
        private CinemachineVirtualCamera _forkliftCam;
        [SerializeField]
        private GameObject _driverModel;
        private bool _inDriveMode = false;
        [SerializeField]
        private InteractableZone _interactableZone;
        
        /// <summary>
        /// Reference to the Forklift movement keys (Default to 'WASD')
        /// </summary>
        [SerializeField, Tooltip("Reference to the Forklift movement keys (Default to \'WASD\' keys)"), Header("Input References")]
        public InputActionReference ForkliftMovementReference;
        /// <summary>
        /// Reference to the Forklift movement of forks keys (Default to <see cref="KeyCode.R"/> and <see cref="KeyCode.T"/> keys)
        /// </summary>
        [SerializeField, Tooltip("Reference to the Forklift movement of forks keys (Default to \'R\' and \'T\' keys)")]
        public InputActionReference ForkliftForksReference;

        public static event Action onDriveModeEntered;
        public static event Action onDriveModeExited;

       
        private void EnterDriveMode(InteractableZone zone)
        {
            if (_inDriveMode !=true && zone.GetZoneID() == 5) //Enter ForkLift
            {
                _inDriveMode = true;
                _forkliftCam.Priority = 11;
                onDriveModeEntered?.Invoke();
                _driverModel.SetActive(true);
                _interactableZone.CompleteTask(5);
                
                ForkliftMovementReference.action.Enable();
                
                ForkliftForksReference.action.Enable();
                ActionMapManager.OnEscapeKeyPressed += ExitDriveModePressed;
            }
        }

        private void ExitDriveMode()
        {
            _inDriveMode = false;
            _forkliftCam.Priority = 9;            
            _driverModel.SetActive(false);
            onDriveModeExited?.Invoke();
            
            ForkliftMovementReference.action.Disable();
            ForkliftForksReference.action.Disable();
            
            ActionMapManager.OnEscapeKeyPressed -= ExitDriveModePressed;
        }

        private void CalcutateMovement()
        {
            float h = ForkliftMovementReference.action.ReadValue<Vector2>().x;
            float v = ForkliftMovementReference.action.ReadValue<Vector2>().y;
            
            var direction = new Vector3(0, 0, v);
            var velocity = direction * _speed;

            transform.Translate(velocity * Time.deltaTime);

            if (Mathf.Abs(v) > 0)
            {
                var tempRot = transform.rotation.eulerAngles;
                tempRot.y += h * _speed / 2;
                transform.rotation = Quaternion.Euler(tempRot);
            }
        }

        private void LiftControls()
        {
            float forkDirection = ForkliftForksReference.action.ReadValue<float>();
            
            if (forkDirection > 0)
                LiftUpRoutine();
            else if (forkDirection < 0)
                LiftDownRoutine();
        }

        private void LiftUpRoutine()
        {
            if (_lift.transform.localPosition.y < _liftUpperLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y += Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y >= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftUpperLimit;
        }

        private void LiftDownRoutine()
        {
            if (_lift.transform.localPosition.y > _liftLowerLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y -= Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y <= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftLowerLimit;
        }
        
        private void ExitDriveModePressed()
        {
            ExitDriveMode();
        }
        
        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += EnterDriveMode;
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterDriveMode;
        }
        
        private void Update()
        {
            if (_inDriveMode == true)
            {
                LiftControls();
                CalcutateMovement();
            }
        }
    }
}