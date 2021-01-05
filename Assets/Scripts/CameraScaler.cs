/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Main Camera game object
// The implementation of the UpdateOrthographicCameraSize() method is based on this source: https://pressstart.vip/tutorials/2018/06/14/37/understanding-orthographic-size.html
using UnityEngine;

/*
 * The purpose of this class is to set the Main Camera's position and scale its size.
 * By default, Unity will scale the height of the camera to the match the device's screen height when the height changes, but it will
 * not scale the width of the camera to match the device's screen width when the width changes.
 * So, this script makes the width and height of the camera scale to match the device's screen width and height whenever the aspect ratio is lower than the indended ratio of 16:9
 */
public class CameraScaler : MonoBehaviour
{
    private Camera cam; // The Main Camera

    public const float BASE_ORTHOGRAPHIC_CAMERA_SIZE = 307.5f; // The orthographic size of the camera when the device aspect ratio is greater than or equal to the game aspect ratio
    public const float CAMERA_X = 547f; // The camera x-position
    public const float CAMERA_Z = -10f; // The camera z-position
    // The camera y-position is always equal to the the currentOrthographicCameraSize, which is not constant

    public static readonly Color color = Color.black; // The color of the camera
    private static float currentOrthographicCameraSize = BASE_ORTHOGRAPHIC_CAMERA_SIZE; // The current orthographic camera size, which depends on the device aspect ratio
    private static float currentDeviceAspectRatio = 0f; // The aspect ratio of the device that the game is currently being played on, which may change if the user resizes the game window
    private const float GAME_ASPECT_RATIO = 16f / 9f; // The aspect ratio intended for the game (16:9)

    // The coordinates of the edges of the camera
    private static float cameraMaxX;
    private static float cameraMinX;
    private static float cameraMaxY;
    private static float cameraMinY;

    // Awake is called before any other script's Start method
    void Awake()
    {
        cam = Camera.main;
        UpdateOrthographicCameraSize(); // Update the current orthographic camera size according to the device's current aspect ratio
        cam.backgroundColor = color; // Set the background color of the camera
    }

    // Update is called once per frame (capped at 50fps)
    void Update()
    {
        // If the device's aspect ratio has changed (meaning the user has resized the game window)
        if ((float)Screen.width / (float)Screen.height != currentDeviceAspectRatio)
        {
            UpdateOrthographicCameraSize(); // Update the orthographic camera size and coordinates
        }
    }

    // Get the current y-position of the camera, which varies based on the device's aspect ratio
    public static float GetCurrentCameraY()
    {
        return currentOrthographicCameraSize;
    }

    // Get the current height of the camera, which variaes based on the device's aspect ratio
    public static float GetCurrentCameraHeight()
    {
        return currentOrthographicCameraSize * 2f;
    }

    public static float GetCameraMaxX()
    {
        return cameraMaxX;
    }

    public static float GetCameraMaxY()
    {
        return cameraMaxY;
    }

    public static float GetCameraMinX()
    {
        return cameraMinX;
    }

    public static float GetCameraMinY()
    {
        return cameraMinY;
    }

    // Updates the orthographic camera size and coordinates according the the device's current aspect ratio
    // This allows the game to scale up or down with the width of the screen
    private void UpdateOrthographicCameraSize()
    {
        currentDeviceAspectRatio = (float)Screen.width / (float)Screen.height; // Compute the device's current aspect ratio

        if (currentDeviceAspectRatio >= GAME_ASPECT_RATIO)
        {
            currentOrthographicCameraSize = BASE_ORTHOGRAPHIC_CAMERA_SIZE; // Keep the size at the base size
        }
        else
        {
            currentOrthographicCameraSize = BASE_ORTHOGRAPHIC_CAMERA_SIZE * (GAME_ASPECT_RATIO / currentDeviceAspectRatio); // Scale up/down the size to fit the screen
        }

        cam.transform.position = new Vector3(CAMERA_X, currentOrthographicCameraSize, CAMERA_Z); // Set camera position
        cam.orthographicSize = currentOrthographicCameraSize; // Set camera size

        UpdateCameraCoordinates(); // Update the coordinates of the edges
    }

    // Updates the coordinates of the edges of the camera according to the currentOrthographicCameraSize
    private void UpdateCameraCoordinates()
    {
        cameraMaxX = CAMERA_X + currentOrthographicCameraSize * Screen.width / Screen.height; // Right-bound x-position of the camera (depends on aspect ratio of the user's screen)
        cameraMinX = CAMERA_X - currentOrthographicCameraSize * Screen.width / Screen.height; // Left-bound x-position of the camera (depends on the aspect ratio of the user's screen)
        cameraMaxY = 2f * currentOrthographicCameraSize; // Upper-bound y-position of the camera
        cameraMinY = 0f; // Bottom-bound y-position of the camera
    }
}
