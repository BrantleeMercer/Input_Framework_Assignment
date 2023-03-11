using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.LiveObjects;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Game.Scripts.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        [FormerlySerializedAs("_speed")] [SerializeField]
        private float Speed = 5.0f;
        [FormerlySerializedAs("_detonator")] [SerializeField]
        private Detonator Detonator;
        [FormerlySerializedAs("_followCam")] [SerializeField]
        private CinemachineVirtualCamera FollowCam;
        [FormerlySerializedAs("_model")] [SerializeField]
        private GameObject Model;
        /// <summary>
        /// Input Action Reference for moving the player
        /// </summary>
        [SerializeField, Tooltip("Input Action Reference for moving the player"), Header("Input Action References")]
        private InputActionReference MoveReference;
        
        private bool _playerGrounded;
        private CharacterController _controller;
        private Animator _anim;
        private bool _canMove = true;

        private void CalcutateMovement()
        {
            _playerGrounded = _controller.isGrounded;
            // float h = Input.GetAxisRaw("Horizontal");
            // float v = Input.GetAxisRaw("Vertical");
            float h = MoveReference.action.ReadValue<Vector2>().x;
            float v = MoveReference.action.ReadValue<Vector2>().y;

            transform.Rotate(transform.up, h);

            var direction = transform.forward * v;
            var velocity = direction * Speed;


            _anim.SetFloat("Speed", Mathf.Abs(velocity.magnitude));


            if (_playerGrounded)
                velocity.y = 0f;
            if (!_playerGrounded)
            {
                velocity.y += -20f * Time.deltaTime;
            }
            
            _controller.Move(velocity * Time.deltaTime);                      

        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            switch(zone.GetZoneID())
            {
                case 1: //place c4
                    Detonator.Show();
                    break;
                case 2: //Trigger Explosion
                    TriggerExplosive();
                    break;
            }
        }

        private void ReleasePlayerControl()
        {
            _canMove = false;
            FollowCam.Priority = 9;
        }

        private void ReturnPlayerControl()
        {
            Model.SetActive(true);
            _canMove = true;
            FollowCam.Priority = 10;
        }

        private void HidePlayer()
        {
            Model.SetActive(false);
        }
               
        private void TriggerExplosive()
        {
            Detonator.TriggerExplosion();
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;
            Laptop.onHackComplete -= ReleasePlayerControl;
            Laptop.onHackEnded -= ReturnPlayerControl;
            Forklift.onDriveModeEntered -= ReleasePlayerControl;
            Forklift.onDriveModeExited -= ReturnPlayerControl;
            Forklift.onDriveModeEntered -= HidePlayer;
            Drone.OnEnterFlightMode -= ReleasePlayerControl;
            Drone.onExitFlightmode -= ReturnPlayerControl;
            
            MoveReference.action.Disable();
        }

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
            Laptop.onHackComplete += ReleasePlayerControl;
            Laptop.onHackEnded += ReturnPlayerControl;
            Forklift.onDriveModeEntered += ReleasePlayerControl;
            Forklift.onDriveModeExited += ReturnPlayerControl;
            Forklift.onDriveModeEntered += HidePlayer;
            Drone.OnEnterFlightMode += ReleasePlayerControl;
            Drone.onExitFlightmode += ReturnPlayerControl;
            
            MoveReference.action.Enable();
        } 

        private void Start()
        {
            _controller = GetComponent<CharacterController>();

            if (_controller == null)
                Debug.LogError("No Character Controller Present");

            _anim = GetComponentInChildren<Animator>();

            if (_anim == null)
                Debug.Log("Failed to connect the Animator");
        }

        private void Update()
        {
            if (_canMove == true)
                CalcutateMovement();

        }
    }
}

