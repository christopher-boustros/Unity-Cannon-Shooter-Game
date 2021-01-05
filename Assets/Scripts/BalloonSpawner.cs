/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Balloon Spawner game object
using UnityEngine;

/*
 * This class spawns balloons from the water at a rate of 1 per second
 */
public class BalloonSpawner : MonoBehaviour
{
    public GameObject balloon; // A copy of the Balloon game object that will be cloned multiple times
    private const float BALLOON_Y = ProceduralTerrainGeneration.WATER_TOP_Y; // Y-position of the balloons to spawn
    private const float MAX_BALLOON_X = ProceduralTerrainGeneration.WATER_RIGHT_X - ProceduralTerrainGeneration.INNER_MOUNTAIN_WIDTH * 0.9f; // Maximum x-position of the balloons
    private const float MIN_BALLOON_X = ProceduralTerrainGeneration.WATER_LEFT_X + ProceduralTerrainGeneration.INNER_MOUNTAIN_WIDTH * 0.9f; // Minimum x-position of the balloons
    private const float timeInterval = 1f; // The time interval (in seconds) to wait before generating another balloon
    private static System.Random random = new System.Random(); // Instance of the Random class to generate random x-positions between the min and max
    private bool justSpawnedBalloon = false; // True if a balloon has recently been spawned

    // FixedUpdate is called once per frame (capped at 50 fps)
    void FixedUpdate()
    {
        if (!justSpawnedBalloon) // If a balloon has not recently been spawned
        {
            justSpawnedBalloon = true; // Indicate that a balloon has been spawned

            // Generate a random x-position between the min and max
            float randomX = MIN_BALLOON_X + (float)random.NextDouble() * (MAX_BALLOON_X - MIN_BALLOON_X);

            // Spawn a new Balloon gameobject at that random x-position
            GameObject newBalloon = Instantiate(balloon); // Create a clone of balloon
            newBalloon.transform.parent = transform; // Make the new balloon a child of this game object
            newBalloon.GetComponent<VerletBalloon>().InitializePointsArrays(randomX, BALLOON_Y); // Set the position of the balloon

            Invoke("SetJustSpawnedBalloon", timeInterval); // Reset justSpawnedBalloon after the time interval
        }
    }

    // Sets justSpawnedBalloon back to false
    private void SetJustSpawnedBalloon()
    {
        justSpawnedBalloon = false;
    }
}
