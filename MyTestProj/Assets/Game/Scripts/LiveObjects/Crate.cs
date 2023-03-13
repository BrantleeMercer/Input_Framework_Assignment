using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        private bool _isReadyToBreak = false;

        private List<Rigidbody> _brakeOff = new List<Rigidbody>();

        [SerializeField, Header("Input References")]
        public InputActionReference BreakCreateReference;
        
        private InteractableZone _referenceZone;
        private bool _punchHeldDown = false;
        private bool _crateBusted = false;
        
        public static Action OnBreakingCratesStarted;
        public static Action OnBreakingCratesEnded;

        private void BreakingCratesStarted(InteractableZone zone)
        {
            _wholeCrate.SetActive(false);
            _brokenCrate.SetActive(true);
            
            if (zone.GetZoneID() == 6)
            {
                _isReadyToBreak = true;
                _referenceZone = zone;
            }

            if (_isReadyToBreak && zone.GetZoneID() == 6) //Crate zone
            {
                zone.ToggleMainInteractable(false);
                OnBreakingCratesStarted?.Invoke();
                BreakCreateReference.action.Enable();

                BreakCreateReference.action.started += BreakActionButtonPressed;
                BreakCreateReference.action.canceled += BreakActionButtonLetGo;
            }
        }

        private void BreakingCratesEnded()
        {
            _isReadyToBreak = false;
            _crateCollider.enabled = false;
            _interactableZone.CompleteTask(6);
            Debug.Log("Completely Busted");
            _crateBusted = true;
            _referenceZone.ToggleMainInteractable(true);
            OnBreakingCratesEnded?.Invoke();
            BreakCreateReference.action.Disable();
            
            BreakCreateReference.action.started -= BreakActionButtonPressed;
            BreakCreateReference.action.canceled -= BreakActionButtonLetGo;
        }

        private void BreakActionButtonPressed(InputAction.CallbackContext context)
        {
            _punchHeldDown = true;
        }
        
        private void BreakActionButtonLetGo(InputAction.CallbackContext context)
        {
            _punchHeldDown = false;
        }
        
        private void DamageCrate()
        {
            if (_brakeOff.Count <= 0)
                return;
            
            StartCoroutine(PunchDelay());
            StartCoroutine(HoldPunchDown());

        }
        
        private void BreakPart(float force)
        {
            if(_crateBusted)
                return;
            
            Vector3 forceDirection = Vector3.up;
            
            int rng = Random.Range(0, _brakeOff.Count);
            _brakeOff[rng].constraints = RigidbodyConstraints.None;
            _brakeOff[rng].AddForce(forceDirection * (force * Time.deltaTime), ForceMode.Force);
            _brakeOff.Remove(_brakeOff[rng]);            
        }

        private IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }
            
            _interactableZone.ResetAction(6);
        }

        private IEnumerator HoldPunchDown()
        {
            float counter = 0.0f;
            
            while (_punchHeldDown)
            {
                //Upper limit of force of 4
                if (counter >= 4)
                    _punchHeldDown = false;
                yield return new WaitForSeconds(1f);
                counter++;
            }
            BreakPart(counter);
        }
        
        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += BreakingCratesStarted;
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= BreakingCratesStarted;
        }

        private void Update()
        {
            //Turn off break crate
            if(_brakeOff.Count == 0 && _isReadyToBreak)
            {
                if(_crateBusted)
                    return;
                
                BreakingCratesEnded();
            }
            
            if(_punchHeldDown)
                DamageCrate();
        }

        private void Start()
        {
            _brakeOff.AddRange(_pieces);
        }
    }
}
