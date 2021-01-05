/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Background game object
using UnityEngine;

/*
 * The purpose of this class is to define the position, width, and height of the game's background
 */
public class Background : MonoBehaviour
{
    private static float currentHeight = 0f; // The current height of the background
    public const float Z = 10f; // The z-position of the background

    // Update is called once per frame
    void Update()
    {
        // If the height of the camera has changed
        if (currentHeight != CameraScaler.GetCurrentCameraHeight())
        {
            // Update the background height and position
            currentHeight = CameraScaler.GetCurrentCameraHeight();
            transform.localScale = new Vector3(GameArea.WIDTH, currentHeight, 1f); // Set the width and length of the background sprite
            transform.position = new Vector3(CameraScaler.CAMERA_X, CameraScaler.GetCurrentCameraY(), Z); // Set the position of the background sprite
        }

    }
}
