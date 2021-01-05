/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Wind game object
using UnityEngine;

/*
 * This class generate values for wind randomly, which are used in the VerletBalloon script
 */
public class Wind : MonoBehaviour
{
    private static System.Random random = new System.Random(); // And instance of the Random class to produce wind randomly
    private bool windJustChanged; // If it is true, then the wind recently changed

    public static int windVelocity = 0; // The velocity of the wind (negative value means wind is to the left; positive is to the right)
    private const int MAX_WIND_VELOCITY = 11; // The maximum magnitude of wind velocity. 
    private const float TIME_DELAY = 2.0f; // Amount of time before wind velocity changes

    // FixedUpdate is called once per frame (capped at 50fps)
    void FixedUpdate()
    {
        if (!windJustChanged) // If the wind has not recently been changed
        {
            windJustChanged = true; // Indicates that the wind has been changed
            ChangeWind();
            Invoke("SetWindJustChanged", TIME_DELAY); // Reset windJustChanged after 2 seconds
        }
    }

    // Changes the wind velocity randomly
    private void ChangeWind()
    {
        windVelocity = random.Next(-MAX_WIND_VELOCITY, MAX_WIND_VELOCITY + 1); // Set the wind velocity between the min and max
    }

    // Sets the windJustChanged variable back to false
    private void SetWindJustChanged()
    {
        windJustChanged = false;
    }
}
