/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Frames game object
using UnityEngine;

/*
 * The purpose of this class is to set the positions and widths of the frames that surround the game area.
 * The purpose of the frames are so that the cannonballs and balloons that move out of the screen are not visible 
 * if the user's screen is wider than the game area (the cannonballs and balloons will pass under the frames).
 */
public class Frames : MonoBehaviour
{
    // The frame game objects
    public GameObject RightFrame;
    public GameObject LeftFrame;
    public GameObject TopFrame;
    public GameObject BottomFrame;

    private const float WIDTH = 100f; // The width of a frame
    private const float Z = -5f; // The z-positions of the frames
    private float currentMaxY = 0f; // The maximum y-position of the Game Area that was last used to set the frames

    // Start is called before the first frame update
    void Start()
    {
        currentMaxY = GameArea.GetCurrentMaxY();
        // Set the scales of the frames
        RightFrame.transform.localScale = new Vector3(WIDTH, currentMaxY + 2 * WIDTH, 1f);
        LeftFrame.transform.localScale = new Vector3(WIDTH, currentMaxY + 2 * WIDTH, 1f);
        TopFrame.transform.localScale = new Vector3(GameArea.MAX_X + 2 * WIDTH, WIDTH, 1f);
        BottomFrame.transform.localScale = new Vector3(GameArea.MAX_X + 2 * WIDTH, WIDTH, 1f);

        // Set the positions of the frames
        RightFrame.transform.position = new Vector3(GameArea.MAX_X + WIDTH / 2, currentMaxY / 2, Z);
        LeftFrame.transform.position = new Vector3(GameArea.MIN_X - WIDTH / 2, currentMaxY / 2, Z);
        TopFrame.transform.position = new Vector3(GameArea.MAX_X / 2, currentMaxY + WIDTH / 2, Z);
        BottomFrame.transform.position = new Vector3(GameArea.MAX_X / 2, GameArea.MIN_Y - WIDTH / 2, Z);

        // Set the colors of the frames to the camera color
        RightFrame.GetComponent<SpriteRenderer>().color = CameraScaler.color;
        LeftFrame.GetComponent<SpriteRenderer>().color = CameraScaler.color;
        TopFrame.GetComponent<SpriteRenderer>().color = CameraScaler.color;
        BottomFrame.GetComponent<SpriteRenderer>().color = CameraScaler.color;
    }

    // Update is called once per frame
    void Update()
    {
        // If the maximum y-position of the Game Area has changed
        if (currentMaxY != GameArea.GetCurrentMaxY())
        {
            currentMaxY = GameArea.GetCurrentMaxY(); // Update currentMaxY

            // Update the scales of the frames
            RightFrame.transform.localScale = new Vector3(WIDTH, currentMaxY + 2 * WIDTH, 1f);
            LeftFrame.transform.localScale = new Vector3(WIDTH, currentMaxY + 2 * WIDTH, 1f);

            // Update the positions of the scales
            RightFrame.transform.position = new Vector3(GameArea.MAX_X + WIDTH / 2, currentMaxY / 2, Z);
            LeftFrame.transform.position = new Vector3(GameArea.MIN_X - WIDTH / 2, currentMaxY / 2, Z);
            TopFrame.transform.position = new Vector3(GameArea.MAX_X / 2, currentMaxY + WIDTH / 2, Z);
        }
    }
}
