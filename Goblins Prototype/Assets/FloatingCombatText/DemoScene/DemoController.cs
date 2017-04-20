using UnityEngine;


namespace EckTechGames.FloatingCombatText
{
	// This is a simple script to demonstrate Floating Combat Text
	public class DemoController : MonoBehaviour
	{
		// Hitting 1-4 will show Miss, hit, critical hit, and heal. Check the readme.txt for a critical heal			
		void Update()
		{
			int damage = RollDamage();

			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				OverlayCanvasController.instance.ShowCombatText(gameObject, CombatTextType.Miss, "MISS");
			}

			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				OverlayCanvasController.instance.ShowCombatText(gameObject, CombatTextType.Hit, damage);
			}

			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				OverlayCanvasController.instance.ShowCombatText(gameObject, CombatTextType.CriticalHit, damage * 2);
			}

			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				OverlayCanvasController.instance.ShowCombatText(gameObject, CombatTextType.Heal, damage);
			}
		}

		// Simple function to roll some random damage.
		protected int RollDamage()
		{
			int damage = Random.Range(13, 21);

			return damage;
		}

		void OnMouseOver()
		{
			int damage = RollDamage();
			bool crit = Random.Range(1, 100) <= 15f; // Roll for a 15% crit chance.

			// Left-click
			if (Input.GetMouseButtonDown(0))
			{
				if (crit)
					OverlayCanvasController.instance.ShowCombatText(gameObject, CombatTextType.CriticalHit, damage * 2);
				else
					OverlayCanvasController.instance.ShowCombatText(gameObject, CombatTextType.Hit, damage);
			}

			// Right-click
			if (Input.GetMouseButtonDown(1))
			{
				// If you follow the Extension tutorial, you can uncomment this to show your critical heal effects
				//if (crit)
				//	OverlayCanvasController.instance.ShowCombatNumber(gameObject, CombatTextType.CriticalHeal, damage);
				//else
				OverlayCanvasController.instance.ShowCombatText(gameObject, CombatTextType.Heal, damage);
			}

			// Middle-click
			if (Input.GetMouseButtonDown(2))
			{
				OverlayCanvasController.instance.ShowCombatText(gameObject, CombatTextType.Miss, "MISS");
			}
		}
	}
}