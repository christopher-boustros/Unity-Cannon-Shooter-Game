/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Text Manager game object
using UnityEngine;
using UnityEngine.UI;

/*
 * The purpose of this class is to draw text to the screen
 */
public class TextManager : MonoBehaviour
{
    // The text game objects
    public Text rightText;
    public Text midText;
    public Text leftText;

    // FixedUpdate is called once per frame (capped at 50 fps)
    void FixedUpdate()
    {
        // Set the text content for the muzzle velocities
        if (OperateCannons.whichCannon == 1) // If the right cannon is selected
        {
            rightText.text = "Right Muzzle velocity = " + OperateCannons.rightMuzzleVelocity + " *";
            leftText.text = "Left Muzzle velocity = " + OperateCannons.leftMuzzleVelocity;
        }
        else // If the left cannon is selected
        {
            rightText.text = "Right Muzzle velocity = " + OperateCannons.rightMuzzleVelocity;
            leftText.text = "Left Muzzle velocity = " + OperateCannons.leftMuzzleVelocity + " *";
        }

        // Set text content for wind velocity
        midText.text = "Wind velocity = " + (int)Wind.windVelocity;
    }
}
