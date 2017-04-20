EckTech Games - Floating Combat Text
Thank you for purchasing Floating Combat Text! 

If you have any issues or any ideas for new features, don't hesitate to contact me:
ecktechgames@gmail.com

The ReadMe.txt is best viewed with wordwrap turned off. For pretier documentation go to:
https://docs.google.com/document/d/1E_8_2Hy1IwmmdomblCZUPW_J4x616zcPDJxM9jLxZGU/edit?usp=sharing

Version 1.0 - Initial Release
Version 1.1 - Simple bug fix to support targeting 2D objects (rectTransforms)


Instructions
------------
Once you import the package into your project, setting up Floating Combat Text is easy.
For a video tutorial of this, go here:
https://www.youtube.com/watch?v=Tatg-h0Ydsw

Scene Setup:
	1. From the project directory FloatingCombatText\Prefabs, drag the OverlayCanvas prefab into your scene.
	2. Select the OverlayCanvas in the object hierarchy.
	3. Drag your scene's Main Camera into the OverlayCanvas's Main Camera property in the object inspector.

At this point your OverlayCanvas is ready to use. Now you'll just need to call the ShowCombatText function.

Call ShowCombatText Function:
	1. Open up one of your scripts where you're wanting to show combat text.
	2. Add the following using declaration at the top of your file:
		using EckTechGames.FloatingCombatText;
	3. Add the following function call to show Combat Text.
		// C# Example
		OverlayCanvasController.instance.ShowCombatText(targetGameObject, CombatTextType.Hit, damage);
		// Parameter 1 is the target game object that the text will attach to.
		// Parameter 2 is the animation style you'd like to use for this text. (Miss, Hit, CriticalHit, or Heal).
		// Parameter 3 is the text (or number) that you would like to show.
		// You can add your own CombatTextTypes too. See Extension Tutorials below!

Configuration
-------------
Damage Pooling:
	Damage pooling totals up similar CombatTextTypes into one CombatText entry. If 3 hits come in for 10 damage each within 1 second of the first hit, then the combat text shown will display 30. This keeps the display a little less cluttered and shows an estimate of DPS.
	
To change this setting:
	1. Go to the Project folder FloatingCombatText\Prefabs\
	2. Select the OverlayCanvas prefab.
	3. In the inspector, toggle this setting by clicking the Pool Damage checkbox.

Change Fonts:
	1. Go to the Project's folder FloatingCombatText\Prefabs\ (you must have another font already installed)
	2. Expand out the CombatTextAnchor prefab by clicking the triangle.
	3. Select the CombatText prefab.
	4. In the Text Component, you can change out the font, style, etc.

PLEASE NOTE: To change the color, you'll have to edit the individual animations since we use Mecanim to animate the colors. (see next section)


Floating Combat Text uses Mecanim animations to move the text around, change its colors, and adjust its scale.
This gives you the freedom to create whatever types of animation effects that you'd like.
Unity does not allow you to adjust animations attached to a prefab, so you'll need a CombatText instance to work with.

Change Text Animations:
	1. In your project, navigate to FloatingCombatText\DemoScene and open the DemoScene.
	2. In the object hierarchy, expand out the TestCombatTextAnchor.
	3. In the inspector, enable the TestCombatTextAnchor by clicking the checkbox next to its name.
	4. In the object hierarchy, select the TestCombatText game object.
	5. Open your animation window (Window -> Animation)
		(If the animation controls are greyed out, you may need to select a different game object in the object hierarchy and reselect CombatText.)
	6. Select the animation that you wish to adjust by selecting the drop down next to Samples like FCT_Hit to change the hit animation.
	
Change color:
	There are 3 key frames for the color/alpha. Frame 0, Frame 60, and Frame 120. 
	Please note, you'll need to change ALL THREE FRAMES to change the color.
	1. Make sure you're recording to adjust settings. Click the red circle and the button should highlight white. (it should also make your Scene View play/pause/advance buttons turn red)
	2. On the CombatText: Text.Color Row, select the first diamond (keyframe).
	3. In the inspector, select the Text Component's color and change it to the value you want.
		Repeat this process with frames 60 and 120. 
	4. To view your changes hit the Animation window's play button.
	
	By default, the hit effect starts out (Frame 0) as Solid Red. Stays Solid red for 1 second (Frame 60). Then fades out to Transparent Red for 1 second (Frame 120).
	
Extension Tutorial
------------------
Let's create a Critical Heal effect, based on our Critical Hit effect.

Add a new enumeration for your effect:
	1. In your code editor, open up FloatingCombatText\Scripts\CombatTextType.cs 
	2. Add a new enumeration value: CriticalHeal = 5,

Duplicate the Critical Hit animation:
	1. In Unity's project view, navigate to FloatingCombatText\Animation
	2. Select FCT_CriticalHit and hit Ctrl-D (Command-D on Mac) to duplicate the Critical hit animation.
	3. Rename the duplicated animation FCT_CriticalHeal

Wire up the animation in our CombatText Animator:
	1. Double click the CombatText animator and it should open up the Animator window.
	2. Drag the animation FCT_CriticalHeal into the Animator
	3. Right click on the orange Idle state, make transition, and select the FCT_CriticalHeal state.
	
	Adjust the transition:
		1. Click the white transition line/arrow that we just created.
		Unity 5.0 
			2. Uncheck HasExitTime
			3. Add a new condition: CombatTextType Equals 5 (the value we set in our enumeration)
		Unity 4.6
			2. Uncheck Atomic
			3. Change ExitTime Condition to CombatTextType | Equals | 5 (the value we set in our enumeration)

Change the animation to a Critical Heal effect:
	Unity does not allow you to adjust animations attached to a prefab, so you'll need a CombatText instance to work with.
	1. In your project, navigate to FloatingCombatText\DemoScene and open the DemoScene.
	2. In the object hierarchy, expand out the TestCombatTextAnchor.
	3. In the inspector, enable the TestCombatTextAnchor by clicking the checkbox next to its name.
	4. In the object hierarchy, select the TestCombatText game object.
	5. Open your animation window (Window -> Animation)
		(If the animation controls are greyed out, you may need to select a different game object in the object hierarchy and reselect CombatText.)
	6. Click the name of the current animation and select FCT_CriticalHeal.
	7. Click the play button and it should loop through the current animation (which is a copy of the critical hit)
	
Let's make it go left:
	1. Make sure the red circle is on for recording (it should also make your Scene View play/pause/advance buttons turn red).
	2. In the animation window, move the current frame to 30.
	3. In the inspector, change the Pos X field to -40.
	4. Hit play on the animation window and the text should now be floating up and to the left.
	
Let's make the color green:
	1. Make sure the red circle is on for recording (it should also make your Scene View play/pause/advance buttons turn red).
	2. Select frame 0.
	3. In the inspector, change the Text's color to green and make sure the alpha is set to 255.
	4. Select frame 60 and set the Text's color to green and make sure the alpha is set to 255.
	5. Select frame 120 and set the Text's color to green and make sure the alpha is set to 0.

Wire up the Animation in the demo scene:
	1. In your code editor, open up FloatingCombatText\DemoScene\DemoController
	2. In the Update function, add the following bit of code:
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			OverlayCanvasController.instance.ShowCombatText(gameObject, CombatTextType.CriticalHeal, damage * 2);
		}
	3. Run your scene and hit the number 5 to test it.
	
Now we have a fancy new Critical Heal effect.	

Animation requirements:
When creating your own animations from scratch, be sure to do the following:
	1. For Frame 0, be sure that you set CombatTextController.CombatTextShown to true.
	2. For the last frame of your animation, set CombatTextController.CombatTextShown to false.

This will disable the Floating Combat Text game object and return it to the pool for future use.

Thanks again for purchasing Floating Combat Text. Be sure to rate it on the asset store and/or write a review. 
https://www.assetstore.unity3d.com/en/#!/content/42253/

If you aren’t going to give Floating Combat Text 5 stars, let me know what isn't perfect and I will strive to make it so.
ecktechgames@gmail.com