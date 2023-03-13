using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Helpers
{
	public class ActionMapManager : MonoBehaviour
	{
		public static ActionMapManager instance { get; private set; }
		
		/// <summary>
		/// Reference to the main interact key (Default to <see cref="KeyCode.E"/> key)
		/// </summary>
		[SerializeField, Tooltip("Reference to the main interact key (Default to \'E\' key)"), Header("Interactable Input References")]
		public InputActionReference MainKeyReference;
		/// <summary>
		/// Reference to the Secondary interact key (Default to <see cref="KeyCode.Space"/> key)
		/// </summary>
		[SerializeField, Tooltip("Reference to the Secondary interact key (Default to \'Space\' key)")]
		public InputActionReference SecondaryKeyReference;
		/// <summary>
		/// Reference to the Escape interact key (Default to <see cref="KeyCode.Escape"/> key)
		/// </summary>
		[SerializeField, Tooltip("Reference to the Secondary interact key (Default to \'Escape\' key)")]
		public InputActionReference EscapeKeyReference;

		public static Action OnMainKeyPressed;
		public static Action OnMainKeyHeld;
		public static Action OnMainKeyLetGo;

		public static Action OnSecondaryKeyPressed;
		public static Action OnSecondaryKeyLetGo;

		public static Action OnEscapeKeyPressed;
		public static Action OnEscapeKeyLetGo;
		
		private void MainKeyHeldDown(InputAction.CallbackContext context)
		{
			OnMainKeyHeld?.Invoke();
		}
		
		private void MainKeyHasBeenLetGo(InputAction.CallbackContext context)
		{
			OnMainKeyLetGo?.Invoke();
		}
		
		private void MainKeyHasBeenPressed(InputAction.CallbackContext context)
		{
			OnMainKeyPressed?.Invoke();
		}
		
		private void SecondaryKeyHasBeenPressed(InputAction.CallbackContext context)
		{
			OnSecondaryKeyPressed?.Invoke();
		}
		
		private void SecondaryKeyHasBeenLetGo(InputAction.CallbackContext context)
		{
			OnSecondaryKeyLetGo?.Invoke();
		}
		
		private void EscapeKeyHasBeenPressed(InputAction.CallbackContext context)
		{
			OnEscapeKeyPressed?.Invoke();
		}
		
		private void EscapeKeyHasBeenLetGo(InputAction.CallbackContext context)
		{
			OnEscapeKeyLetGo?.Invoke();
		}
		
		private void Awake()
		{
			if (instance != null && instance != this)
			{
				Destroy(this);
			}
			else
				instance = this;
		}

		private void Start()
		{
			//Enable interaction action maps
			MainKeyReference.action.Enable();
			MainKeyReference.action.performed += MainKeyHasBeenPressed;
			MainKeyReference.action.started += MainKeyHeldDown;
			MainKeyReference.action.canceled += MainKeyHasBeenLetGo;
			
			SecondaryKeyReference.action.Enable();
			SecondaryKeyReference.action.performed += SecondaryKeyHasBeenPressed;
			SecondaryKeyReference.action.canceled += SecondaryKeyHasBeenLetGo;
			
			EscapeKeyReference.action.Enable();
			EscapeKeyReference.action.performed += EscapeKeyHasBeenPressed;
			EscapeKeyReference.action.canceled += EscapeKeyHasBeenLetGo;
		}

		private void OnDestroy()
		{
			//Disable interaction action maps
			MainKeyReference.action.Disable();
			MainKeyReference.action.performed -= MainKeyHasBeenPressed;
			MainKeyReference.action.started -= MainKeyHeldDown;
			MainKeyReference.action.canceled -= MainKeyHasBeenLetGo;
			
			SecondaryKeyReference.action.Disable();
			SecondaryKeyReference.action.performed -= SecondaryKeyHasBeenPressed;
			SecondaryKeyReference.action.canceled -= SecondaryKeyHasBeenLetGo;
			
			EscapeKeyReference.action.Disable();
			EscapeKeyReference.action.performed -= EscapeKeyHasBeenPressed;
			EscapeKeyReference.action.canceled -= EscapeKeyHasBeenLetGo;
		}
	}
}

