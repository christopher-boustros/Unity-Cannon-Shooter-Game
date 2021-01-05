/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Cannonball game object prefab
using UnityEngine;

/*
 * The purpose of this class is to make a cannonball move
 * A cannonball's motion is modelled with projectile physics
 */
public class CannonballMotion : MonoBehaviour
{
    private float xVelocity, yVelocity;
    private float xPosition, yPosition;
    private double cannonballAngle;
    private int whichCannon; // To store the value of OperateCannons.whichCannon at the time the cannonball was created

    private bool justBounced = false; // True if the ball bounced recently
    private bool stopBouncing = false; // True if the ball has reached its minimum velocity
    private int previousNetVelocityCounter = 0; // The number of times net velocity has stayed relatively constant after a bounce. Used to determine when to stop bouncing.

    // Define variables to determine which section of the terrain the cannonball collided with
    private static int platform1RightX = ProceduralTerrainGeneration.PLATFORM_WIDTH; // The right x-coordinate of the first flat platform
    private static int platform2LeftX = ProceduralTerrainGeneration.WATER_RIGHT_X + ProceduralTerrainGeneration.OUTER_MOUNTAIN_WIDTH; // The left x-coordinate of the second flat platform
    private static int valleyLeftX = ProceduralTerrainGeneration.WATER_LEFT_X + ProceduralTerrainGeneration.INNER_MOUNTAIN_WIDTH; // The left x-coordinate of the flat valley
    private static int valleyRightX = ProceduralTerrainGeneration.WATER_RIGHT_X - ProceduralTerrainGeneration.INNER_MOUNTAIN_WIDTH; // The right x-coordinate of the flat valley
    private static int mountain1topX = platform1RightX + ProceduralTerrainGeneration.OUTER_MOUNTAIN_WIDTH; // The x-coordinate of the top of the left mountain
    private static int mountain2topX = mountain1topX + 2 * ProceduralTerrainGeneration.INNER_MOUNTAIN_WIDTH + ProceduralTerrainGeneration.VALLEY_WIDTH; // The x-coordinate of the top of the right mountain
    private const float GRAVITATIONAL_ACCELERATION = 0.15f; // This is the change (decrease) in y-velocity per frame. The higher it is, the faster the cannonball accelerates down.
    private const float VELOCITY_FACTOR = 0.51f; // To scale up/down the velocity. The lower it is, the slower the cannonball moves.
    private const float COEFFICIENT_OF_RESTITUION = 0.75f; // The percentage of velocity that will be retained after the cannonball bounces on collision
    private const float BOUNCE_DELAY = 2f / 50; // The time delay before bouncing again after the ball has just bounced
    private const float TIME_BEFORE_DESTRUCTION = 2f; // The time delay before the ball is destroyed after it stops bouncing
    private const int MAX_PREVIOUS_NET_VELOCITY_COUNTER = 3; // The maximum value of previousNetVelocityCounter before the ball stops bouncing
    private static float STOP_BOUNCING_VELOCITY_THRESHOLD = 0.20f; // The minimum change in net velocity after a bounce without increasing previousNetVelocityCounter


    // Start is called before the first frame update
    void Start()
    {
        // Get the value of OperateCannons.whichCannon
        whichCannon = OperateCannons.whichCannon;

        if (whichCannon == 1) // Correct the angle if the cannonball is from the right cannon
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z - 180f);
        }

        cannonballAngle = transform.eulerAngles.z; // Get the cannonball angle

        // Get the initial velocity of the cannonball
        float initial_velocity = (float)OperateCannons.GetSelectedBarrelVelocity();


        // Scale the initial velocity with the velocity factor
        initial_velocity *= VELOCITY_FACTOR;

        // Split the initial velocity into x and y components
        xVelocity = (float)(initial_velocity * System.Math.Cos(ToRadians(cannonballAngle)));
        yVelocity = (float)(initial_velocity * System.Math.Sin(ToRadians(cannonballAngle)));

        // Get the initial position of the cannonball
        double[] initial_position = OperateCannons.GetSelectedBarrelPosition();
        xPosition = (float)initial_position[0];
        yPosition = (float)initial_position[1];

        // Use the angle and barrel length to update the intiial position of the cannonball
        float dx = (float)(OperateCannons.barrelLength * System.Math.Cos(ToRadians(cannonballAngle))); // Change in x
        float dy = (float)(OperateCannons.barrelLength * System.Math.Sin(ToRadians(cannonballAngle))); // Change in y
        xPosition += dx;
        yPosition += dy;

        // Set the cannonball to its initial position
        transform.position = new Vector3((float)xPosition, (float)yPosition, 0);
    }

    // Update is called once per frame
    // Used to update the position of the cannonball
    void Update()
    {
        // If the cannonball has stopped bouncing after a collision
        if (stopBouncing)
        {
            return; // Stop updating the cannonball's positon
        }

        /*
         * Cannonball motion
         */
        float timeFactor = GameTime.TimeFactor(); // Compute the percentage of the gravity and velocity to use in the physics computation based on the current framerate
                                                  // This ensures that the cannonaball's motion is not faster with higher framerates and slower with lower framerates
        yVelocity -= GRAVITATIONAL_ACCELERATION * timeFactor; // Update y velocity with gravity
        xPosition += xVelocity * timeFactor; // Update xPosition
        yPosition += yVelocity * timeFactor; // Update yPosition
        transform.position = new Vector3(xPosition, yPosition, 0.0f); // Set the cannonball's x and y positions 

        /*
         * Make cannonball disappear when off-screen
         */
        if (xPosition > GameArea.MAX_X + CannonballCollisionDetection.collisionRadius ||
            xPosition < GameArea.MIN_X - CannonballCollisionDetection.collisionRadius ||
            yPosition < GameArea.MIN_Y - CannonballCollisionDetection.collisionRadius) // If off the left, right, or bottom side of the screen
        {
            DestroyCannonball(); // Destroy the cannonball
        }
    }

    // Make the ball bounce after collision
    // This method will be called by the CannonballCollisionDetection class
    // The parameters collideX and collideY are the coordinates of the position where the ball bounced
    public void Bounce(int collideX, int collideY)
    {
        if (justBounced)
        {
            return; // Do not bounce if the ball recently bounced
        }

        if (stopBouncing)
        {
            return; // Do not bounce is the ball has stopped bouncing
        }

        float previousNetVelocity = ComputeNetVelocity(); // Store the current net velocity before it is updated

        // Update the net velocity: Reduce the x and y velocities by the coefficient of restitution
        yVelocity *= COEFFICIENT_OF_RESTITUION;
        xVelocity *= COEFFICIENT_OF_RESTITUION;

        // Determine if it should stop bouncing
        float currentNetVelocity = (float)System.Math.Sqrt(System.Math.Pow(xVelocity, 2) + System.Math.Pow(yVelocity, 2)); // Compute the updated net velocity

        if (previousNetVelocity - currentNetVelocity < STOP_BOUNCING_VELOCITY_THRESHOLD * (1 + GameTime.TimeFactor())) // If the change in net velocity after a bounce is smaller than the threshold
        {
            previousNetVelocityCounter++; // Increment the count of the number of times net velocity has stayed relatively constant
        }

        if (previousNetVelocityCounter >= MAX_PREVIOUS_NET_VELOCITY_COUNTER) // If net velocity has stayed relatively constant for a while
        {
            stopBouncing = true; // Indicate that the ball has stopped bouncing
            Invoke("DestroyCannonball", TIME_BEFORE_DESTRUCTION); // Destroy the cannonball after a specified amount of time
            return;
        }

        // Determine which section of the ground terrain the ball collided with
        if (collideX < platform1RightX || collideX > platform2LeftX || collideX > valleyLeftX && collideX < valleyRightX)
        // If the ball collided on a flat surface (the left or right platform or the bottom of the valley)
        {
            // Bounce up
            yVelocity = System.Math.Abs(yVelocity); // Make the y velocity positive
        }
        else // The ball collided on a mountain surface
        {
            if (collideX > platform1RightX && collideX < mountain1topX || collideX > valleyRightX && collideX < mountain2topX) // If it collided on the rising part of a mountain (relative to the left cannon)
            {
                // Bounce up and left
                yVelocity = System.Math.Abs(yVelocity); // Make the y velocity positive
                xVelocity = -System.Math.Abs(xVelocity); // Make the x velocity negative
            }
            else // If it collided on the falling part of a mountain
            {
                // Bounce up and right
                yVelocity = System.Math.Abs(yVelocity); // Make the y velocity negative
                xVelocity = System.Math.Abs(xVelocity); // Make the x velocity negative
            }
        }

        justBounced = true; // Indicate that the cannonball recently bounced
        Invoke("SetJustBounced", BOUNCE_DELAY); // Reset after the time delay
    }

    // Returns the current net velocity of the cannonball
    private float ComputeNetVelocity()
    {
        return (float)System.Math.Sqrt(System.Math.Pow(xVelocity, 2) + System.Math.Pow(yVelocity, 2));
    }

    // Set the variable justBounced back to false
    private void SetJustBounced()
    {
        justBounced = false;
    }

    // Destroys the cannonball and lets the OperateCannon class know
    private void DestroyCannonball()
    {
        Destroy(gameObject);
        OperateCannons.CleanActiveCannonballsList(); // Cleans the list of active cannonballs in the OperateCannons class
    }

    // Helper method to convert degrees to radians
    private double ToRadians(double angle)
    {
        return (System.Math.PI / 180) * angle;
    }

    // Getter method for xPosition
    public float GetXPosition()
    {
        return xPosition;
    }

    // Getter method for yPosition
    public float GetYPosition()
    {
        return yPosition;
    }

    // Getter method for xVelocity
    public float GetXVelocity()
    {
        return xVelocity;
    }

    // Getter method for yVelocity
    public float GetYVelocity()
    {
        return yVelocity;
    }
}
