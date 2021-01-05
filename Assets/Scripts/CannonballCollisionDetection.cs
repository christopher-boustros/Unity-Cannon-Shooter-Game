/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Cannonball game object
using System.Collections.Generic;
using UnityEngine;

/*
 * The purpose of this class is to detect and hangle collisions between a cannonball and the ground or water terrain
 */
public class CannonballCollisionDetection : MonoBehaviour
{
    public static int collisionRadius = 7; // The radius of the collision-detection circle around the cannonball
    private CannonballMotion cannonballMotion; // The instance of the CannonballMotion class that is linked to the current cannonball game object

    // Start is called before the first frame update
    void Start()
    {
        cannonballMotion = gameObject.GetComponent<CannonballMotion>(); // Get the instance of the CannonballMotion class from the current cannonball game object
    }

    // Update is called once per frame
    void Update()
    {
        // Create a list that will store points on the collision-detection circle around the cannonball
        List<int[]> points = new List<int[]>();

        // Get the current x and y positions of the cannonball as integers (the center point of the cannonball) and add it to the points list
        int[] centerPoint = new int[2] {(int) transform.position.x, (int) transform.position.y};
        points.Add(centerPoint);

        // Compute the left, right , top, and bottom points of the collision-detection circle around the cannonball
        points.Add(new int[2] {centerPoint[0] - collisionRadius, centerPoint[1]}); // Left point
        points.Add(new int[2] {centerPoint[0] + collisionRadius, centerPoint[1]}); // Right point
        points.Add(new int[2] {centerPoint[0], centerPoint[1] + collisionRadius}); // Top point
        points.Add(new int[2] {centerPoint[0], centerPoint[1] - collisionRadius}); // Bottom point

        // Detect collision with water
        if (collisionWithWater(points))
        {
            Destroy(gameObject); // The cannonball disappears
            return; // End script
        }

        // Detect collision with ground
        if (collisionWithGround(points))
        {
            // The cannonball bounces
            cannonballMotion.Bounce(centerPoint[0], centerPoint[1]);
        }
    }

    // Returns true if any of the points in the list passed in as parameter is on top of the water terrain
    private bool collisionWithWater(List<int[]> points)
    {
        foreach (int[] point in points)
        {
            // Transform the coordinates so that (0, 0) is the bottom-left corner of the water terrain
            int x = point[0] - ProceduralTerrainGeneration.WATER_LEFT_X;
            int y = point[1];

            if ( x < 0 || x >= ProceduralTerrainGeneration.waterColumnHeightsWithNoise.Count) // If the x-coordinate is beyond the bounds of the water terrain
            {
                return false; // No collision
            }

            int waterHeightAtX = ProceduralTerrainGeneration.waterColumnHeightsWithNoise[x];

            if (y <= waterHeightAtX) // If the height of the point at column x is within the height of the water at column x
            {
                return true; // Collision
            }
        }

        return false; // No collision
    }

    // Returns true if any of the points in the list passed in as parameter is on top of the water terrain
    private bool collisionWithGround(List<int[]> points)
    {
        foreach (int[] point in points)
        {
            // Get the x and y coordinates
            int x = point[0];
            int y = point[1];

            if ( x < 0 || x >= ProceduralTerrainGeneration.groundColumnHeightsWithNoise.Count) // If the x-coordinate is beyond the bounds of the ground terrain
            {
                return false; // No collision
            }

            int groundHeightAtX = ProceduralTerrainGeneration.groundColumnHeightsWithNoise[x];

            if (y <= groundHeightAtX) // If the height of the point at column x is within the height of the ground at column x
            {
                return true; // Collision
            }
        }

        return false; // No collision
    }

    // Returns true if any of the points in the list passed in as parameter intersects with one of the line segments of a balloon
    private bool collisionWithBalloon()
    {
        return false;
    }
}
