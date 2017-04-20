using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace EckTechGames.FloatingCombatText
{
	/// <summary>
	/// This class is the OverlayCanvas that all combat text is displayed on. 
	/// If people are interested, I plan to support World Canvas text as well.
	/// Please see ReadMe.txt for scene setup and usage.
	/// </summary>
	public class OverlayCanvasController : MonoBehaviour
	{
		public static OverlayCanvasController instance;
		public Camera mainCamera; // Associate your scene's main camera to this field.
		public bool poolDamage; // When true, similar damage types will batch into single CombatText instances.

		public CombatTextAnchorController combatTextAnchorPrefab;
		
		protected Dictionary<CombatTextType, List<CombatTextAnchorController>> combatTextAnchorDictionary;

		void Awake()
		{
			if (instance == null)
				instance = this;
			else
			{
				// This means 2 OverlayCanvasControllers have been added to the scene.
				// The static instance variable is provided as a convenience for calling ShowCombatText.
				// If you find that you need more than one OverlayCanvasController, you should 
				// get rid of the global "instance" and associate the individual OverlayCanvasControllers
				// to the appropriate game objects.
				Debug.LogWarning("Detected 2 OverlayCanvasControllers. The static instance variable is still set to the first instance. Double click this warning and read the related comment.");
			}

			InitializeDictionary();
		}

		void Start()
		{
			if (mainCamera == null)
			{
				// Turn ourselves off since we're invalid.
				gameObject.SetActive(false);

				string errorMessage = "You must set OverlayCanvasController.mainCamera to the main camera of your scene. Disabling OverlayCanvas.";
				throw new System.InvalidOperationException(errorMessage);
			}
		}

		/// <summary>
		/// This is the function you will call to show combat text on the screen.
		/// See ReadMe.txt for detailed usage instructions.
		/// </summary>
		/// <param name="targetGameObject">The game object you want to attach the combat text to.</param>
		/// <param name="combatTextType">The style of text and animation you want to display.</param>
		/// <param name="combatText">The text that you want to show. When OverlayCanvasController.poolDamage is true, text of the same combatTextType is overwritten with the new text.</param>
		public void ShowCombatText(GameObject targetGameObject, CombatTextType combatTextType, string combatText)
		{
			CombatTextAnchorController combatTextAnchorInstance;
			combatTextAnchorInstance = EnsureCombatTextAnchor(targetGameObject, combatTextType);
			combatTextAnchorInstance.mainCamera = mainCamera;
			combatTextAnchorInstance.transform.SetParent(transform);
			combatTextAnchorInstance.ShowCombatText(targetGameObject, combatTextType, combatText);
		}

		/// <summary>
		/// This is the function you will call to show combat text on the screen.
		/// See ReadMe.txt for detailed usage instructions.
		/// </summary>
		/// <param name="targetGameObject">The game object you want to attach the combat text to.</param>
		/// <param name="combatTextType">The style of text and animation you want to display.</param>
		/// <param name="combatNumber">The number that you want to show. When OverlayCanvasController.poolDamage is true, numbers of the same combatTextType are totaled and displayed in one CombatText object.</param>
		public void ShowCombatText(GameObject targetGameObject, CombatTextType combatTextType, int combatNumber)
		{
			CombatTextAnchorController combatTextAnchorInstance;
			combatTextAnchorInstance = EnsureCombatTextAnchor(targetGameObject, combatTextType);
			combatTextAnchorInstance.mainCamera = mainCamera;
			combatTextAnchorInstance.transform.SetParent(transform);
			combatTextAnchorInstance.ShowCombatText(targetGameObject, combatTextType, combatNumber);
		}

		/// <summary>
		/// This function looks for a CombatTextAnchor of the right type. If one doesn't exist,
		/// it creates one.
		/// </summary>
		/// <param name="targetGameObject">Which game object is this text going to be attached to. (Used when poolDamage is enabled)</param>
		/// <param name="combatTextType">The type of text</param>
		/// <returns></returns>
		protected CombatTextAnchorController EnsureCombatTextAnchor(GameObject targetGameObject, CombatTextType combatTextType)
		{
			CombatTextAnchorController textAnchor;

			if (poolDamage)
			{
				// Look for a currently playing animation that is young enough so we can add our numbers to them.
				textAnchor = combatTextAnchorDictionary[combatTextType].Find(x => x.combatTextShown && x.targetGameObject == targetGameObject && x.ageInSeconds < 1f);
				if (textAnchor != null)
				{
					return textAnchor;
				}
			}

			// Look for a combatText that has stopped playing
			textAnchor = combatTextAnchorDictionary[combatTextType].Find(x => !x.combatTextShown);
			if(textAnchor != null)
			{
				textAnchor.ResetForReuse();
				return textAnchor;
			}

			// Otherwise we have to create a new one from our prefab.
			textAnchor = Instantiate(combatTextAnchorPrefab) as CombatTextAnchorController;
			textAnchor.name = string.Format("{0} {1}", combatTextAnchorPrefab.name, combatTextType);
			combatTextAnchorDictionary[combatTextType].Add(textAnchor);

			return textAnchor;
		}

		/// <summary>
		/// Sets up our combatTextAnchorDictionary for our CombatText object pool. 
		/// </summary>
 		protected void InitializeDictionary()
		{
			combatTextAnchorDictionary = new Dictionary<CombatTextType, List<CombatTextAnchorController>>();

			// Get a list of all the entries in CombatTextType
			Array combatTextTypes = Enum.GetValues(typeof(CombatTextType));

			// Initialize a list for each entry so we don't have to check if one's been created
			// every time we want to show combat text.
			foreach (object combatTextType in combatTextTypes)
			{
				if (combatTextAnchorDictionary.ContainsKey((CombatTextType)combatTextType))
				{
					// Turn ourselves off since we're invalid.
					gameObject.SetActive(false);

					string errorMessage = string.Format("The enumeration CombatTextType must have unique values. [CombatTextType.{0}] has the same value as another entry.", combatTextType);
					throw new InvalidOperationException(errorMessage);
				}

				combatTextAnchorDictionary[(CombatTextType)combatTextType] = new List<CombatTextAnchorController>();
			}
		}
	}
}
