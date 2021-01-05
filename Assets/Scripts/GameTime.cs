/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is not linked to a game object
using UnityEngine;

/*
 * This class defines the constant timestep interval used for the game's physics computations
 * The lower the time interval, the faster the game's overall physics motions will appear
 * For example, the OperateCannons class is defined to make the cannon move by a particular angle once every INTERVAL amount of time.
 * So, by decreasing INTERVAL, the cannon will move faster. This is the same concept for other moving objects, including Balloon and Cannonball
 */
public static class GameTime
{
    public const float INTERVAL = 0.02f; // 0.02 seconds
    public const float RATE = 1 / INTERVAL; // The framerate equivalent to the interval

    /*
     * This factor determines by how much an object's position (such as a cannon's angle or a cannonballs height) should be updated for physics computatiosn based on the actual time between frames
     * For example, if a cannon is set to move by 0.5 degrees once every GameTime.INTERVAL (so once every 0.02 seconds) but the time between frames was only 0.01 seconds, then the timeFactor()
     * will be 0.01/0.02 = 1/2. So for that single frame, the cannon will move by 1/2 of 0.5 degrees in order to keep its motion at 0.5 degrees every 0.02 seconds.
     */
    public static float TimeFactor()
    {
        return Time.deltaTime * RATE;
    }
}
