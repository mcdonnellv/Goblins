using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace EckTechGames.FloatingCombatText
{
	/// <summary>
	/// This class is just a rectTransform used to follow the game world object 
	/// that the combat text is attached to and gives an independent canvas
	/// location for the combat text to base its animation off of. Without this
	/// guy, the combat text would follow the camera instead of staying with the
	/// targetGameObject.
	/// </summary>
	public class CombatTextAnchorController : MonoBehaviour
	{
		public GameObject targetGameObject = null;
		public RectTransform targetRectTransform = null;

		public Camera mainCamera;
		public float ageInSeconds; // How long has this text been alive since the last reuse.

		public bool combatTextShown { get { return combatTextController.combatTextShown; } }
		protected CombatTextController combatTextController;

		void Awake()
		{
			combatTextController = GetComponentInChildren<CombatTextController>();
		}

		void Update()
		{
			ageInSeconds += Time.deltaTime;

			// When the target object is destroyed, we stop following it.
			if (targetGameObject != null)
			{
				// Keep ourselves pinned to the game object.
				UpdatePosition();
			}

			// When the combat text is done being shown, return to the pool and turn ourselves off.
			if (!combatTextController.combatTextShown)
			{
				gameObject.SetActive(false);
			}
		}

		public void ShowCombatTextDelay(GameObject targetGameObject, CombatTextType combatTextType, string combatText, float delay) {
			gameObject.SetActive(true);
			StartCoroutine(ShowCombatTextWithPackage(targetGameObject, combatTextType, combatText, delay));
		}

		IEnumerator ShowCombatTextWithPackage(GameObject targetGameObject, CombatTextType combatTextType, string combatText, float delayTime) {
			yield return new WaitForSeconds(delayTime);
			// Now do your thing here
			this.targetGameObject = targetGameObject;
			targetRectTransform = targetGameObject.GetComponent<RectTransform>();

			UpdatePosition();
			combatTextController.ShowCombatText(combatTextType, combatText);

			gameObject.SetActive(true);
		}

		/// <summary>
		/// Shows a combatTextType string that follows the targetGameObject
		/// </summary>
		/// <param name="targetGameObject">The game object you want to attach the combat text to.</param>
		/// <param name="combatTextType">The style of text and animation you want to display.</param>
		/// <param name="combatText">The string that you want to show.</param>
		public void ShowCombatText(GameObject targetGameObject, CombatTextType combatTextType, string combatText)
		{
			this.targetGameObject = targetGameObject;
			targetRectTransform = targetGameObject.GetComponent<RectTransform>();

			UpdatePosition();
			combatTextController.ShowCombatText(combatTextType, combatText);

			gameObject.SetActive(true);
		}

		/// <summary>
		/// Shows a combatTextType number that follows the targetGameObject
		/// </summary>
		/// <param name="targetGameObject">The game object you want to attach the combat text to.</param>
		/// <param name="combatTextType">The style of text and animation you want to display.</param>
		/// <param name="combatNumber">The number that you want to show.</param>
		public void ShowCombatText(GameObject targetGameObject, CombatTextType combatTextType, int combatNumber)
		{
			this.targetGameObject = targetGameObject;
			targetRectTransform = targetGameObject.GetComponent<RectTransform>();

			UpdatePosition();
			combatTextController.ShowCombatText(combatTextType, combatNumber);

			gameObject.SetActive(true);
		}

		public void ResetForReuse()
		{
			// Reset our age counter for damage pooling.
			ageInSeconds = 0f;
		}

		protected void UpdatePosition()
		{
			// If we're targeting a canvas object, just set our position to its position.
			if (targetRectTransform != null)
				transform.position = targetRectTransform.position;
			// If we're targeting a world object, translate our screen position from the world position.
			else
				transform.position = mainCamera.WorldToScreenPoint(targetGameObject.transform.position);
		}
	}
}