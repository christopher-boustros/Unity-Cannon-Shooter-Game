/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Balloon game object prefab
// The implementation of the ApplyVerletIntergration and ApplyStringSegmentLenghConstraint methods is inspired by this source: https://www.youtube.com/watch?v=FcnvwtyxLds&ab_channel=YangHoDoo
using UnityEngine;

/*
 * This class is used to draw a balloon consisting of a body and string and simulate it using verlet integration and constraints
 * This class makes the balloon move up and move left or right with wind, which is generated in the Wind script
 */
public class VerletBalloon : MonoBehaviour
{
    // Instance variables
    public GameObject balloonBody; // The game object that represents the balloon body
    public GameObject balloonString; // The game object that represents the balloon string
    private LineRenderer bodyLineRenderer; // Used to display each point of the body
    private LineRenderer stringLineRenderer; // Used to display each point of the string 
    private Vector3 initialBalloonPosition = new Vector3(545f, 100f, 0f); // The initial position of the point that connects the string and body
    private Vector3 gravityVector = new Vector3(0f, -5f, 0f); // The gravitational acceleration vector used to apply verlet integration
    private Vector3[] currentBodyPoints; // Used to store the current position of each point of the body
    private Vector3[] previousBodyPoints; // Used to store the previous position of each point of the body
    private Vector3[] currentStringPoints; // Used to store the current position of each point of the string
    private Vector3[] previousStringPoints; // Used to store the previous position of each point of the string
    private Vector3 bodyCenterPoint; // The center point of the body (updated every frame)
    private double maxMountainHeight = ProceduralTerrainGeneration.maxMountainHeightWithNoise; // The height at which wind starts making the balloon move


    // Class variables
    private const int NUM_BODY_POINTS = 6; // The number of points on the body
    private const int NUM_STRING_POINTS = 8; // The number of points on the string
    private const float BODY_SEGMENT_LENGTH = 10f; // The length of a segment between two body points
    private const float STRING_SEGMENT_LENGTH = 6f; // The length of a segment between two string points
    private const float BODY_SEGMENT_WIDTH = 2f; // The width of a segment between two body points
    private const float STRING_SEGMENT_WIDTH = 2f; // The width of a segment between two string points
    private float BODY_CENTER_POINT_OFFSET = BODY_SEGMENT_LENGTH + NUM_STRING_POINTS * STRING_SEGMENT_LENGTH; // The offset between the center of the body and the bottom of the string
    private const int CONSTRAINT_FREQUENCY = 10; // The number of times to apply the constaint each time verlet integration is applied (the higher it is, the better the constraints will be respected)
    private const float BALLOON_RISE_INCREMENT = 2.50f; // The amount by which the currentBalloonY increments each time (to make the balloon rise)
    private const int CONNECTION_POINT_INDEX = 0; // The index of the connection point (the point that connects the body with the string) in the points arrays
    private const int BODY_TOP_POINT_INDEX = 3; // The index of the top point of the body in the points arrays
    private const float HEXAGON_CONSTRAINT_ERROR_THRESHOLD_FACTOR = 0.20f; // The maximum percent error tolerated for the hexagon link constraint
    private const float STRING_HEIGHT_CONSTRAINT_DISTANCE = BODY_SEGMENT_LENGTH / 2; // The distance the string needs to keep from the hexagon for its height constraint
    private const float WIND_VELOCITY_FACTOR = 0.75f; // The factor that determines the amount by which the balloon's points move due to the wind velocity
    private const float COLLISION_BUFFER_ZONE = 1.5f; // Additional length of radius added to the radius of collision of the cannonball when detecting collisions


    // Start is called before the first frame update
    void Start()
    {
        // Get the line renderers
        bodyLineRenderer = balloonBody.GetComponent<LineRenderer>();
        stringLineRenderer = balloonString.GetComponent<LineRenderer>();

        // Set the width of the line segments in the line renderers
        bodyLineRenderer.startWidth = BODY_SEGMENT_WIDTH;
        bodyLineRenderer.endWidth = BODY_SEGMENT_WIDTH;
        stringLineRenderer.startWidth = STRING_SEGMENT_WIDTH;
        stringLineRenderer.endWidth = STRING_SEGMENT_WIDTH;

        // Set the position counts in the line renderers
        bodyLineRenderer.positionCount = NUM_BODY_POINTS;
        stringLineRenderer.positionCount = NUM_STRING_POINTS;

        // Set the body line renderer to be a loop
        bodyLineRenderer.loop = true;

        // Initialize the body and string points arrays and currentBalloonY
        InitializePointsArrays(initialBalloonPosition.x, initialBalloonPosition.y);
    }

    // FixedUpdate is called once per frame (capped at 50 fps)
    void FixedUpdate()
    {
        DrawBalloon(); // Draw the body and string of the balloon
        MoveBalloon(); // Makes the body and string of the balloon move (includes verlet integration and constraints)
        DestroyIfOffscreen(); // Destroy the balloon if off-screen
        DestroyIfBodyCollision(); // Destroy the balloon if a cannonball collided with the body
        DetectAndMoveOnStringCollision(); // Moves the string if a cannonball has collided with it
    }

    // Checks if the balloon is off-screen to destroy it
    private void DestroyIfOffscreen()
    {
        /*
         * Make cannonball disappear if off-screen
         */
        if (bodyCenterPoint.x > GameArea.MAX_X + BODY_CENTER_POINT_OFFSET ||
            bodyCenterPoint.x < GameArea.MIN_X - BODY_CENTER_POINT_OFFSET ||
            bodyCenterPoint.y > GameArea.GetCurrentMaxY() + BODY_CENTER_POINT_OFFSET) // If off the left, right, or top side of the screen
        {
            Destroy(gameObject); // Destroy the balloon
        }
    }

    // Makes the balloon move with verlet integration, constraints, upward lift, and wind
    private void MoveBalloon()
    {
        // Apply verlet integration to the body and string
        ApplyVerletIntegration(currentBodyPoints, previousBodyPoints); // To the body
        ApplyVerletIntegration(currentStringPoints, previousStringPoints); // To the string

        // Apply constraints to the body and string, each individually, multiple times
        for (int i = 0; i < CONSTRAINT_FREQUENCY; i++)
        {
            ApplyStringSegmentLenghConstraint();
            ApplyStringHeightConstraint();
            ApplyHexagonSegmentLengthConstraint();
            ApplyHexagonCenterLinkConstraint();
            ApplyConnectionConstraint();
        }

        // Make the balloon body's center point rise
        MakeBalloonRise();

        // Make the balloon body's left-most or right-most points move due to the wind
        MakeBalloonMoveWithWind();
    }

    // Applies verlet integration to arrays of current and previous points
    private void ApplyVerletIntegration(Vector3[] currentPoints, Vector3[] previousPoints)
    {
        // Iterate through each point in currentPoints, except for the 0th point
        // because this is the point that connectes the body and string
        for (int i = 1; i < currentPoints.Length; i++)
        {
            Vector3 velocity = currentPoints[i] - previousPoints[i]; // Compute the velocity as the difference between the current and previous point positions
            previousPoints[i] = currentPoints[i]; // Update the previous position of the point to the current position
            currentPoints[i] += velocity + gravityVector * Time.fixedDeltaTime; // Update the current position of the point according to the verlet integration formula
        }
    }

    // Applies the line segment length constraint to the array of string points
    // This ensures that all line segments of the string stay at a constant length
    private void ApplyStringSegmentLenghConstraint()
    {
        // Iterate through each point (at index i) in the string
        for (int i = 0; i < NUM_STRING_POINTS - 1; i++)
        {
            int j = i + 1;
            Vector3 fromPointToAdjacentPoint = currentStringPoints[j] - currentStringPoints[i]; // A vector from the point to the adjacent point
            float error = fromPointToAdjacentPoint.magnitude - STRING_SEGMENT_LENGTH; // The error in the length of the segment between the two points (compared to STRING_SEGMENT_LENGTH)

            if (System.Math.Abs(error) > 0) // If there is error
            {
                Vector3 directionOfCorrection = fromPointToAdjacentPoint.normalized; // The direction from the point to the adjacent point (as a vector)

                if (i != 0) // If i is not the 0th point
                {
                    // Apply correction to both points
                    currentStringPoints[i] += 0.5f * error * directionOfCorrection;
                    currentStringPoints[j] -= 0.5f * error * directionOfCorrection;
                }
                else // If i is the 0th point (the point that connects the string to the balloon)
                {
                    // Apply correction only to the adjacent point
                    currentStringPoints[j] -= error * directionOfCorrection;
                }
            }
        }
    }

    // Applies the string height constraint to the array of string points
    // This ensures that all string points other than the bottom-point of the body
    // are always below the bottom-left and bottom-right points of the body
    private void ApplyStringHeightConstraint()
    {
        // Iterate through all string points other than the 0th point (the point connecting to the body)
        for (int i = 1; i < NUM_STRING_POINTS; i++)
        {
            // If the point is higher than the bottom-left or bottom-right body point
            if (currentStringPoints[i].y > currentBodyPoints[1].y || currentStringPoints[i].y > currentBodyPoints[5].y)
            {
                // Set its height to slightly below the height of the lower point
                currentStringPoints[i].y = System.Math.Min(currentBodyPoints[1].y, currentBodyPoints[5].y) - STRING_HEIGHT_CONSTRAINT_DISTANCE;
            }
        }
    }

    // The constraint is that the length from a point to the center of the body remains constant
    private void ApplyHexagonCenterLinkConstraint()
    {
        // Iterate through each point in the body
        for (int i = 0; i < NUM_BODY_POINTS; i++)
        {
            Vector3 fromPointToCenter = bodyCenterPoint - currentBodyPoints[i]; // A vector from the point to the center
            float error = fromPointToCenter.magnitude - BODY_SEGMENT_LENGTH; // The error in the length from the point to the center (compared to BODY_SEGMENT_LENGTH)
            //Debug.Log("Point " + i + " Error " + error);

            if (System.Math.Abs(error) > BODY_SEGMENT_LENGTH * HEXAGON_CONSTRAINT_ERROR_THRESHOLD_FACTOR) // If the error is beyond the threshold
            {
                // Apply correction to the point
                Vector3 directionOfCorrection = fromPointToCenter.normalized;
                currentBodyPoints[i] += error * directionOfCorrection;
            }
        }
    }

    // This constraint is that the length of the line segments (between adjacent points) are constant
    private void ApplyHexagonSegmentLengthConstraint()
    {
        // Iterate through each point (at index i) in the body
        for (int i = 0; i < NUM_BODY_POINTS; i++)
        {
            int j = (i + 1) % NUM_BODY_POINTS; // The index of the adjacent point (index i + 1)
            Vector3 fromPointToAdjacentPoint = currentBodyPoints[j] - currentBodyPoints[i]; // A vector from the point to the adjacent point
            float error = fromPointToAdjacentPoint.magnitude - BODY_SEGMENT_LENGTH; // The error in the length of the segment between the two points (compared to BODY_SEGMENT_LENGTH)

            if (System.Math.Abs(error) > 0) // If there is any error
            {
                // Apply correction to both points
                Vector3 directionOfCorrection = fromPointToAdjacentPoint.normalized;
                currentBodyPoints[i] += 0.5f * error * directionOfCorrection;
                currentBodyPoints[j] -= 0.5f * error * directionOfCorrection;
            }
        }
    }

    // Applies the constraint that keeps the string connected to the body
    private void ApplyConnectionConstraint()
    {   // The point on the srting connected to the balloon always updates to the position
        // of the point on the balloon connected to the string
        currentStringPoints[CONNECTION_POINT_INDEX] = currentBodyPoints[CONNECTION_POINT_INDEX];
    }

    // Makes the ballon body's center point rise
    private void MakeBalloonRise()
    {
        // Make the body's center point rise (the rest of the balloon will rise due to the constraints)
        bodyCenterPoint.y += GameTime.TimeFactor() * BALLOON_RISE_INCREMENT;
    }

    // Makes the ballon body's center and left or right points move with the wind (depending on the wind direction)
    private void MakeBalloonMoveWithWind()
    {
        if (bodyCenterPoint.y <= maxMountainHeight) // If the center point of the balloon body is not above the mountains
        {
            return; // Do not move balloon
        }

        int point1Index; // The index of the first body point to move
        int point2Index; // The index of the second body point to move
        int point3Index; // The index of the first body point to move
        int point4Index; // The index of the second body point to move

        if (Wind.windVelocity > 0) // Wind to the right
        {
            point1Index = 4; // The first left-most point of the body (hexagon)
            point2Index = 5; // The second
            point3Index = 1;
            point4Index = 2;
        }
        else // Wind to the left or no wind
        {
            point1Index = 1; // The first right-most point of the body (hexagon)
            point2Index = 2; // The second
            point3Index = 4;
            point4Index = 5;
        }

        // Update the position of the left or right points according to the wind velocity 
        float timeFactor = GameTime.TimeFactor(); // To ensure the speed of the balloon is not dependent on framerate
        currentBodyPoints[point1Index].x += timeFactor * Wind.windVelocity * WIND_VELOCITY_FACTOR * 1.5f;
        currentBodyPoints[point2Index].x += timeFactor * Wind.windVelocity * WIND_VELOCITY_FACTOR * 1.5f;
        currentBodyPoints[point3Index].x += timeFactor * Wind.windVelocity * WIND_VELOCITY_FACTOR / 4;
        currentBodyPoints[point4Index].x += timeFactor * Wind.windVelocity * WIND_VELOCITY_FACTOR / 4;

        // Update the position of the center point
        bodyCenterPoint.x += timeFactor * Wind.windVelocity * WIND_VELOCITY_FACTOR;
    }

    // Draws the body and string of the balloon
    // It does this by setting the arrays of points to the respective line renderers
    private void DrawBalloon()
    {
        bodyLineRenderer.SetPositions(currentBodyPoints); // Set the current body points to the body line renderer
        stringLineRenderer.SetPositions(currentStringPoints); // Set the current string points to the string line renderer
    }

    // Initialize the body and string points arrays with a particular initial balloon position
    // This function can be used on a create Balloon game object to set the initial position of the balloon
    public void InitializePointsArrays(float initialXPosition, float initialYPosition)
    {
        // Set the initial position of the balloon
        initialBalloonPosition.x = initialXPosition;
        initialBalloonPosition.y = initialYPosition;

        // Initialize the arrays of body points as a hexagon (6 points)
        currentBodyPoints = GenerateBodyPoints(initialBalloonPosition);
        previousBodyPoints = (Vector3[])currentBodyPoints.Clone();

        // Initialize the arrays of string points as a line
        currentStringPoints = GenerateStringPoints(initialBalloonPosition);
        previousStringPoints = (Vector3[])currentStringPoints.Clone();
    }

    // Generates an array of body points as a hexagon (6 points)
    // Used to initialize the bodyPoints list
    // Parameter position is the position of the bottom point of the hexagon
    private Vector3[] GenerateBodyPoints(Vector3 position)
    {
        Vector3[] bodyPoints = new Vector3[NUM_BODY_POINTS]; // The list to return
        float theta = (float)(System.Math.PI / 6); // The angle to compute dx and dy (30 degrees)
        float dx = (float)(BODY_SEGMENT_LENGTH * System.Math.Cos(theta)); // The difference in y between points
        float dy = (float)(BODY_SEGMENT_LENGTH * System.Math.Sin(theta)); // The difference in x between points

        // Generate the bottom point (the point that connects the body with the string)
        Vector3 bottomPoint = position;
        bodyPoints[0] = bottomPoint;

        // Generate the lower-left point
        Vector3 lowerLeftPoint = bottomPoint;
        lowerLeftPoint.x -= dx;
        lowerLeftPoint.y += dy;
        bodyPoints[1] = lowerLeftPoint;

        // Generate the upper-left point
        Vector3 upperLeftPoint = lowerLeftPoint;
        upperLeftPoint.y += BODY_SEGMENT_LENGTH;
        bodyPoints[2] = upperLeftPoint;

        // Generate the top point
        Vector3 topPoint = upperLeftPoint;
        topPoint.x += dx;
        topPoint.y += dy;
        bodyPoints[3] = topPoint;

        // Generate the upper-right point
        Vector3 upperRightPoint = topPoint;
        upperRightPoint.x += dx;
        upperRightPoint.y -= dy;
        bodyPoints[4] = upperRightPoint;

        // Generate the lower-right point
        Vector3 lowerRightPoint = upperRightPoint;
        lowerRightPoint.y -= BODY_SEGMENT_LENGTH;
        bodyPoints[5] = lowerRightPoint;

        // Generate the center point
        Vector3 centerPoint = bottomPoint;
        centerPoint.y += dy + BODY_SEGMENT_LENGTH / 2f;

        // Initialize bodyCenterPoint
        bodyCenterPoint = centerPoint;

        return bodyPoints;
    }

    // Generates an array of string points as a line
    // Used to initialize the stringPoints list
    // Parameter position is the position of the bottom point of the hexagon
    private Vector3[] GenerateStringPoints(Vector3 position)
    {
        Vector3[] stringPoints = new Vector3[NUM_STRING_POINTS]; // The array to return

        Vector3 currentStringPointPosition = position; // Position of the current string point to add
        for (int i = 0; i < NUM_STRING_POINTS; i++) // Add string points
        {
            stringPoints[i] = currentStringPointPosition; // Add a new point
            currentStringPointPosition.y -= STRING_SEGMENT_LENGTH; // Set the position of the next point below the position of the current point
        }

        return stringPoints;
    }

    // Returns true if a cannonball has collided with the body
    private bool DetectBodyCollision()
    {
        foreach (GameObject cannonball in OperateCannons.activeCannonballs) // For each active cannonball
        {
            if (cannonball == null)
            {
                continue; // Do nothing if the cannonball is not active
            }

            CannonballMotion motion = cannonball.GetComponent<CannonballMotion>(); // Get the CannonballMotion component of the cannonball
            Vector3 cannonballCenter = new Vector3(motion.GetXPosition(), motion.GetYPosition(), 0f);
            Vector3 centerToCenter = cannonballCenter - bodyCenterPoint; // The vector from the center of the cannonball to the center for the body
            float distance = centerToCenter.magnitude; // The distance between the centers

            // If the distance is less or equal to the radius of collision of the cannonball combined with the radius of the biggest circle inscribed within a hexagon
            if (System.Math.Abs(distance) < CannonballCollisionDetection.collisionRadius + COLLISION_BUFFER_ZONE + BODY_SEGMENT_LENGTH * System.Math.Cos(System.Math.PI / 6))
            {
                // The cannonball is inside the body
                return true; // A collision occured
            }

            // For each point of the body
            for (int i = 0; i < NUM_BODY_POINTS; i++)
            {
                centerToCenter = cannonballCenter - currentBodyPoints[i];
                distance = centerToCenter.magnitude;

                // If the distance from the center of the cannonball to the point is less than or equal to the collision radius  of the cannonball
                if (System.Math.Abs(distance) <= CannonballCollisionDetection.collisionRadius + COLLISION_BUFFER_ZONE)
                {
                    // The point is inside the cannonball
                    return true; // A collision occured
                }
            }
        }

        return false; // No collision occured
    }

    /*
     * This method detects collision between a cannonball and a point on the string
     * If a collision is detected, the string moves away from the cannonball
     * NOTE: If the segment length of the string is larger than the collision radius of the cannonball,
     * than a collision may not be detected
     */
    private void DetectAndMoveOnStringCollision()
    {
        foreach (GameObject cannonball in OperateCannons.activeCannonballs) // For each active cannonball
        {
            if (cannonball == null)
            {
                continue;
            }

            CannonballMotion motion = cannonball.GetComponent<CannonballMotion>(); // Get the CannonballMotion component of the cannonball
            Vector3 cannonballCenter = new Vector3(motion.GetXPosition(), motion.GetYPosition(), 0f);
            Vector3 centerToPoint; // The vector from the center of the cannonball to a point on the string
            float distance; // The distance between the center and the point

            // For each point of the string, except the point connected to the body
            for (int i = 1; i < NUM_STRING_POINTS; i++)
            {
                centerToPoint = cannonballCenter - currentStringPoints[i];
                distance = centerToPoint.magnitude;

                // If the distance from the center of the cannonball to the point is less than or equal to the collision radius of the cannonball
                if (System.Math.Abs(distance) <= CannonballCollisionDetection.collisionRadius + COLLISION_BUFFER_ZONE)
                {
                    // The point is inside the cannonball
                    float error = CannonballCollisionDetection.collisionRadius + COLLISION_BUFFER_ZONE - distance;
                    float direction; // The direction in which to correct the position of the string points

                    // Set the direction to the direction the cannonball is moving
                    if (motion.GetXVelocity() > 0)
                    {
                        direction = 1f; // Right direction
                    }
                    else
                    {
                        direction = -1f; // Left direction
                    }

                    // For the string point and every string point below it
                    for (int j = i; j < NUM_STRING_POINTS; j++)
                    {
                        currentStringPoints[j].x += 2f * error * direction; // Correct the x position of the point
                        currentStringPoints[j].y += 2f * error; // Correct the y position of the point (always upward)
                    }
                }
            }
        }
    }

    // Destroys the balloon if a cannonball collided with the body
    private void DestroyIfBodyCollision()
    {
        if (DetectBodyCollision())
        {
            Destroy(gameObject);
        }
    }
}
