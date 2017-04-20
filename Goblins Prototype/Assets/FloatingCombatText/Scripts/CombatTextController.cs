using UnityEngine;
using UnityEngine.UI;

namespace EckTechGames.FloatingCombatText
{
	/// <summary>
	/// This class is the combat text that animates the text on the screen.
	/// </summary>
 	public class CombatTextController : MonoBehaviour
	{
		public CombatTextType combatTextType;
		// A flag used by the animation to disable the object and return it to the pool
		// for reuse. When creating your own combat text types, be sure to set this
		// to false at the end of your animation so the system can return it to the 
		// pool and reuse this Combat Text object. 
		// See ReadMe.txt for more details on creating your own animations.
		public bool combatTextShown; 

		protected Text combatText;
		protected int combatNumber;
		protected Animator combatTextAnimator;
		protected int combatTextTypeHash = Animator.StringToHash("CombatTextType");

		void Awake()
		{
			combatText = GetComponent<Text>();
			combatTextAnimator = GetComponent<Animator>();
		}

		void Update()
		{
			SetAnimatorsCombatTextType();

			// When we're done playing our animation, turn ourselves off so the
			// system can return us to the combat text pool for reuse.
			if (!combatTextShown)
			{
				gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Shows a combatTextType string
		/// </summary>
		/// <param name="combatTextType">The style of text and animation you want to display.</param>
		/// <param name="combatText">The string that you want to show.</param>
		public void ShowCombatText(CombatTextType combatTextType, string combatText)
		{
			this.combatTextShown = true;
			this.combatText.text = combatText;
			this.combatTextType = combatTextType;

			gameObject.SetActive(true);
		}

		/// <summary>
		/// Shows a combatTextType number
		/// </summary>
		/// <param name="combatTextType">The style of text and animation you want to display.</param>
		/// <param name="combatNumber">The number that you want to show.</param>
		public void ShowCombatText(CombatTextType combatTextType, int combatNumber)
		{
			// If we're already being shown, add this number to our total.
			if (combatTextShown)
				this.combatNumber += combatNumber;
			else
				this.combatNumber = combatNumber;

			ShowCombatText(combatTextType, this.combatNumber.ToString());
		}

		protected void SetAnimatorsCombatTextType()
		{
			if (combatTextAnimator.GetInteger(combatTextTypeHash) != (int)combatTextType)
			{
				combatTextAnimator.SetInteger(combatTextTypeHash, (int)combatTextType);
			}
		}
	}
}