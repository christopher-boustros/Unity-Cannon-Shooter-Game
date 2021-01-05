/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Game Area game object
using UnityEngine;

/*
 * The purpose of this class is to define the position and boundaries of the GameArea
 * The boundaries are dependent on the size of the background
 * The GameArea contains a background, water and ground terrain, and surrounding frames
 * that contain all visible objects of the game
 */
public class GameArea : MonoBehaviour
{
    public const float WIDTH = ProceduralTerrainGeneration.TOTAL_WIDTH; // The width of the Game Area
    public const float X = 0f; // The x-position of the GameArea
    public const float Y = 0f; // The y-position of the GameArea
    public const float Z = 0f; // The z-position of the GameArea
    public const float MAX_X = X + WIDTH; // The maximum x-position of the game
    public const float MIN_X = X; // The minimum x-position of the game
    public const float MIN_Y = Y; // The minimum y-position of the game
    // The maximum y-position is equal to the camera's maximum y-position, which is not constant


    // Start is called once per frame
    private void Start()
    {
        transform.position = new Vector3(X, Y, Z); // Set the position
    }

    public static float GetCurrentMaxY()
    {
        return CameraScaler.GetCameraMaxY();
    }
}
