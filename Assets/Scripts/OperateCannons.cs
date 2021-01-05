/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Cannons game object
using System.Collections.Generic;
using UnityEngine;

/*
 * The purpose of this class is the operate the left and right cannons with the keyboard
 */
public class OperateCannons : MonoBehaviour
{
    public GameObject leftCannonBarrel; // The left cannon's barrel
    public GameObject rightCannonBarrel; // The right cannon's barrel
    public GameObject cannonball; // The cannonball that the cannons will shoot
    public GameObject cannonballs; // The parent object of the cannoballs that will be shot by the cannons
    private bool alreadyLaunched = false; // True if a cannon was already launched recently
    private bool alreadySwitched = false; // True if a cannon switch was done recently
    private bool alreadyChangedVelocity = false; // True if the velocity of a muzzle has recently been changed

    public static float barrelLength = 50f; // Length of the barrel
    public static float[] leftCannonPosition = new float[2] { 35f, 165f }; // (x, y) position of the left cannon's pivot point
    public static float[] rightCannonPosition = new float[2] { 1060f, 165f }; // (x, y) position of the right cannon's pivot point
    public static int whichCannon = 0; // 0 means the left cannon is selected. 1 means the right cannon is selected.
    public static int rightMuzzleVelocity = 15; // The current velocity of the right cannon's muzzle
    public static int leftMuzzleVelocity = 15; // The current velocity of the left cannon's muzzle
    public static List<GameObject> activeCannonballs = new List<GameObject>(); // To store all non-destroyed cannonballs
    private const float LAUNCH_TIME_DELAY = 0.40f; // Time delay between cannon launches
    private const float SWITCH_TIME_DELAY = 0.20f; // Time delay between cannon switches
    private const float VELOCITY_CHANGE_TIME_DELAY = 0.15f; // Time delay for each increase in muzzle velocity
    private const int MAX_MUZZLE_VELOCITY = 30; // The maximum muzzle velocity
    private const int MIN_MUZZLE_VELOCITY = 5; // The maximum muzzle velocity
    private const float MAX_BARREL_ANGLE = 90.0f; // Maximum elevation of the barrel
    private const float MIN_BARREL_ANGLE = 0.0f; // Minimum elevation of the barrel
    private const float BARREL_ANGLE_INCREMENT_PER_INTERVAL = 0.5f; // The amount of degrees by which the barrel rotates up/down once every GameTime.INTERVAL

    // Start is called before the first frame update
    void Start()
    {
        leftCannonBarrel.transform.parent.transform.position = new Vector3(leftCannonPosition[0], leftCannonPosition[1], 0f); // Set the position of the leftCannonBarrel's parent object
        rightCannonBarrel.transform.parent.transform.position = new Vector3(rightCannonPosition[0], rightCannonPosition[1], 0f); // Set the position of the rightCannonBarrel's parent object
        leftCannonBarrel.transform.rotation = Quaternion.Euler(0, 0, 45); // Set the initial rotation of the left barrel
        rightCannonBarrel.transform.rotation = Quaternion.Euler(0, 0, 315); // Set the initial rotation of the right barrel
    }

    // Update is called once per frame
    void Update()
    {
        // Switch selected cannon
        if (!alreadySwitched && Input.GetKey("tab"))
        {
            whichCannon++;
            whichCannon %= 2;
            alreadySwitched = true; // Indicate that a switch has happened recently
            Invoke("SetAlreadySwitched", SWITCH_TIME_DELAY); // Reset the alreadySwitched variable after the switchTimeDelay
        }

        // Fire cannonball
        if (!alreadyLaunched && Input.GetKey("space"))
        {
            GameObject barrel = GetSelectedBarrel();
            GameObject newCannonball = Instantiate(cannonball, barrel.transform.position, barrel.transform.rotation) as GameObject; // Create a clone of the cannonball
            newCannonball.transform.parent = cannonballs.transform; // Make the new cannonball a child of the cannonballs game object
            activeCannonballs.Add(newCannonball); // Add it to the list of active cannonballs

            alreadyLaunched = true; // Indicate that a launch has happened recently
            Invoke("SetAlreadyLaunched", LAUNCH_TIME_DELAY); // Reset the alreadyLaunched variable after the launchTimeDelay
        }

        // Change barrel elevation
        if (Input.GetKey("up") || Input.GetKey("down"))
        {
            // Get the barrel
            GameObject barrel = GetSelectedBarrel();

            // Compute the percentage of the barrel angle increment to use based on the current framerate
            // This ensures that the barrel's motion is not faster with higher framerates and slower with lower framerates
            float barrelAngleIncrementPerFrame = BARREL_ANGLE_INCREMENT_PER_INTERVAL * GameTime.TimeFactor();

            if (Input.GetKey("up")) // Rotate barrel up
            {
                if (whichCannon == 1) // Right barrel selected
                {
                    // Right barrel rotates up from 360 down to 270 degrees
                    RotateBarrelNegative(barrel, 360f - MAX_BARREL_ANGLE, barrelAngleIncrementPerFrame);
                }
                else // Left barrel selected
                {
                    // Left barrel rotates up from 0 up to 90 degrees
                    RotateBarrelPositive(barrel, MAX_BARREL_ANGLE, barrelAngleIncrementPerFrame);
                }
            }
            else  // Rotate barrel down
            {
                if (whichCannon == 1) // Right barrel selected
                {
                    // Right barrel rotates down from 270 up to 359.9 degrees
                    RotateBarrelPositive(barrel, 359.9f - MIN_BARREL_ANGLE, barrelAngleIncrementPerFrame);
                }
                else // Left barrel selected
                {
                    // Left barrel rotates down from 90 degrees down to 0 degres
                    RotateBarrelNegative(barrel, MIN_BARREL_ANGLE, barrelAngleIncrementPerFrame);
                }
            }
        }

        // Change muzzle velocity
        if (!alreadyChangedVelocity && (Input.GetKey("right") || Input.GetKey("left")))
        {
            if (Input.GetKey("right")) // Right key pressed
            {
                if (whichCannon == 1) // Right barrel selected
                {
                    if (rightMuzzleVelocity < MAX_MUZZLE_VELOCITY)
                    {
                        rightMuzzleVelocity++;
                    }
                }
                else // Left barrel selected
                {
                    if (leftMuzzleVelocity < MAX_MUZZLE_VELOCITY)
                    {
                        leftMuzzleVelocity++;
                    }
                }
            }
            else // Left key pressed
            {
                if (whichCannon == 1) // Right barrel selected
                {
                    if (rightMuzzleVelocity > MIN_MUZZLE_VELOCITY)
                    {
                        rightMuzzleVelocity--;
                    }
                }
                else // Left barrel selected
                {
                    if (leftMuzzleVelocity > MIN_MUZZLE_VELOCITY)
                    {
                        leftMuzzleVelocity--;
                    }
                }
            }

            alreadyChangedVelocity = true; // Indicate that a switch has happened recently
            Invoke("SetAlreadyChangedVelocity", VELOCITY_CHANGE_TIME_DELAY); // Reset the alreadySwitched variable after the switchTimeDelay
        }
    }

    // Returns the barrel currently selected (left or right)
    private GameObject GetSelectedBarrel()
    {
        GameObject barrel;

        // Determine which barrel to return
        if (whichCannon == 1)
        {
            barrel = rightCannonBarrel;
        }
        else
        {
            barrel = leftCannonBarrel;
        }

        return barrel;
    }

    // Returns the selected barrel's muzzle velocity
    public static int GetSelectedBarrelVelocity()
    {
        if (whichCannon == 1) // Right cannon selected
        {
            return rightMuzzleVelocity;
        }
        else // Left cannon selected
        {
            return leftMuzzleVelocity;
        }
    }

    // Returns the selected barrel's elevation angle
    public static double[] GetSelectedBarrelPosition()
    {
        double[] pos = new double[2];

        if (whichCannon == 1) // Right cannon selected
        {
            pos[0] = rightCannonPosition[0];
            pos[1] = rightCannonPosition[1];
        }
        else
        {
            pos[0] = leftCannonPosition[0];
            pos[1] = leftCannonPosition[1];
        }

        return pos;
    }

    // This method sets the alreadyLaunched variable back to false
    private void SetAlreadyLaunched()
    {
        alreadyLaunched = false;
    }

    // This method sets the switchTimeDelay variable back to false
    private void SetAlreadySwitched()
    {
        alreadySwitched = false;
    }

    // This method sets the switchTimeDelay variable back to false
    private void SetAlreadyChangedVelocity()
    {
        alreadyChangedVelocity = false;
    }

    // Removes the destroyed cannonballs from the activeCannonballs list
    public static void CleanActiveCannonballsList()
    {
        // For each element of activeCannonballs
        for (int i = 0; i < activeCannonballs.Count; i++)
        {
            if (activeCannonballs[i] == null) // Check if the cannonball has been destroyed
            {
                activeCannonballs.RemoveAt(i); // Remove the cannonball
                i--; //adjust the index after reducing the size of the list
            }
        }
    }

    // A helper method to rotate a cannon barrel in the negative z direction
    private void RotateBarrelNegative(GameObject barrel, float minAngle, float barrelAngleIncrementPerFrame)
    {
        if (barrel.transform.eulerAngles.z > minAngle) // If the barrel can still rotate down to the min angle
        {
            if (barrel.transform.eulerAngles.z > minAngle + barrelAngleIncrementPerFrame) // If it is not near the min angle
            {
                barrel.transform.Rotate(0, 0, -barrelAngleIncrementPerFrame, Space.Self); // Rotate down by the increment
            }
            else
            {
                barrel.transform.Rotate(0, 0, minAngle - barrel.transform.eulerAngles.z, Space.Self); // Rotate down to the min angle
            }
        }
    }

    // A helper method to rotate a cannon barrel in the positive z direction
    private void RotateBarrelPositive(GameObject barrel, float maxAngle, float barrelAngleIncrementPerFrame)
    {
        if (barrel.transform.eulerAngles.z < maxAngle) // If the barrel can still rotate up to the max angle
        {
            if (barrel.transform.eulerAngles.z < maxAngle - barrelAngleIncrementPerFrame) // If it is not near the max angle
            {
                barrel.transform.Rotate(0, 0, barrelAngleIncrementPerFrame, Space.Self); // Rotate up by the increment
            }
            else
            {
                barrel.transform.Rotate(0, 0, maxAngle - barrel.transform.eulerAngles.z, Space.Self); // Rotate up to the max angle
            }
        }
    }
}
