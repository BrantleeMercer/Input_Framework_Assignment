using Game.Scripts.LiveObjects;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.Helpers
{
	public class ResetInteractableZones : MonoBehaviour
	{
		public static ResetInteractableZones instance { get; private set; }

		[SerializeField, Tooltip("List of problematic IZ's")]
		public InteractableZone[] InteractableZonesList;

		private void Awake()
		{
			if (instance != null && instance != this)
			{
				Destroy(this);
			}
			else
				instance = this;
		}

		public void DoResetAction()
		{
			StartCoroutine(ResetIZs());
		}

		private IEnumerator ResetIZs()
		{
			foreach (InteractableZone zone in InteractableZonesList)
			{
				zone.gameObject.SetActive(false);
				yield return new WaitForFixedUpdate();
				zone.gameObject.SetActive(true);
			}
		}
	}
}

