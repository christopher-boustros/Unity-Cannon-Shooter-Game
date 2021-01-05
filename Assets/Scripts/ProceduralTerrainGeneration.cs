/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Terrain game object
// The method used in this script to implement procedural terrain generation is based on this source: https://www.youtube.com/watch?v=-5OS1s-NWRw&ab_channel=ChronoABI
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
 * This class produces the 2D terrain with perlin noise procedurally.
 * Perlin noise is added on top of the the terrain and is different each time the class generates the terrain. However, the basic shape
   of the terrain always remains similar.
 */
public class ProceduralTerrainGeneration : MonoBehaviour
{
    /*
     * The tilemaps and tiles that that terrain is made up of
     */
    public Tilemap groundTilemap; // The tilemap containing the ground tiles
    public Tilemap waterTilemap; // The tilemap containing the water tiles
    public Tile groundTile; // The ground tile
    public Tile waterTile; // The water tile

    /*
     * The widths and heights and positions of diffent sections of the terrain before noise.
     * The individual widths and heights should not be modified. Only the SCALE_FACTOR may be modified.
     */
    public const double SCALE_FACTOR = 1.0; // A factor to scale up or down down the widths and heights
    public const int MAX_HEIGHT = (int)(275 * SCALE_FACTOR); // The maximum height of the terrain (height of the mountains) excluding noise
    public const int MAX_HEIGHT_WITH_NOISE = MAX_HEIGHT + (int)MAX_VERY_STEEP_NOISE_HEIGHT; // The maximum height of the terrain (height of the mountains) with noise
    public const int PLATFORM_HEIGHT = (int)(135 * SCALE_FACTOR); // The height of the platforms
    public const int MIN_HEIGHT = (int)(40 * SCALE_FACTOR); // The minimum height of the terrain (hieight of the valley)
    public const int PLATFORM_WIDTH = (int)(187 * SCALE_FACTOR); // The width of the left and right flat platforms
    public const int VALLEY_WIDTH = (int)(200 * SCALE_FACTOR); // The width of the valley
    public const int OUTER_MOUNTAIN_WIDTH = (int)(140 * SCALE_FACTOR); // The width of the outer part of the mountains
    public const int INNER_MOUNTAIN_WIDTH = (int)(120 * SCALE_FACTOR); // The width of the inner part of the mountains
    public const int WATER_LEFT_X = PLATFORM_WIDTH + OUTER_MOUNTAIN_WIDTH; // The left x-coordinate of the water terrain
    public const int WATER_RIGHT_X = WATER_LEFT_X + INNER_MOUNTAIN_WIDTH + VALLEY_WIDTH + INNER_MOUNTAIN_WIDTH; // The right x-coordinate of the water terrain
    public const int WATER_BOTTOM_Y = 0; // The bottom y-coordinate of the water terrain
    public const int WATER_TOP_Y = (int)(PLATFORM_HEIGHT * 0.75); // The top y-coordinate of the water terrain (75% the height of the platform)
    public const int TOTAL_WIDTH = WATER_RIGHT_X + OUTER_MOUNTAIN_WIDTH + PLATFORM_WIDTH; // The total width of the ground terrain

    /* 
     * The higher the GROUND_SMOOTHNESS and WATER_SMOOTHNESS, the smoother the perlin noise values computed in GenerateGroundTerrain()
     * and GenerateWaterTerrain() will be. Smoother values result in terrain that will be smoother. 
     * A smoothness of 1.0 will result in purely random noise values, whereas a higher number results in
     * noise values that are closer to previous values (less purely random).
     */
    public const double GROUND_SMOOTHNESS = 5 * System.Math.PI; // Using Pi to add more decimal places of precision to the number
    public const double WATER_SMOOTHNESS = 14 * System.Math.PI;

    /*
     * The heights of the noise on top of different sections of the terrain
     * The values before the SCALE_FACTOR can be modified to vary the height of the generated noise
     */
    public const double MAX_FLAT_NOISE_HEIGHT = 30.0 * SCALE_FACTOR; // The maximum height of the perlin noise generated on top of flat ground terrain
    public const double MAX_SLIGHTLY_STEEP_NOISE_HEIGHT = 40.0 * SCALE_FACTOR; // The maximum height of the perlin noise generated on top of slightly steep ground terrain
    public const double MAX_VERY_STEEP_NOISE_HEIGHT = 50.0 * SCALE_FACTOR; // The maximum height of the perlin noise generated on top of very steep ground terrain
    public const double MAX_WATER_NOISE_HEIGHT = 50.0 * SCALE_FACTOR; // The maximum height of the perlin nose generated on top of water terrain

    /*
     * A list to store the heights (exluding perlin noise) of each column (x-coordinate) of the ground terrain.
     * This list will define the overall shape of the terrain and will be used to generate the terrain.
     */
    private List<int> groundColumnHeights = new List<int>();

    /*
     * These lists will store the heights of the columns of the ground and water terrain with the perlin noise after the terrains are generated
     * These lists will be useful after the terrain generation for collision detection
     */
    public static List<int> groundColumnHeightsWithNoise = new List<int>(); // Stores the ground heights of each column including the generated noise
    public static List<int> waterColumnHeightsWithNoise = new List<int>(); // Stores the water heights of each column including the generated noise

    public static int maxMountainHeightWithNoise; // The maximum height of the mountains computed as the mountains with noise are generated

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(GameArea.X, GameArea.Y, GameArea.Z); // Set the position of the Terrain game object        
        InitializeGroundColumnHeights();
        GenerateGroundTerrain();
        GenerateWaterTerrain();
    }

    /* 
     * This method generates the ground terrain with perlin noise on top using the groundColumnHeights list
     * While generating the terrain, the heights including perlin noise are added to the groundColumnHeightsWithNoise
     * The ground is a set of grey tiles in the GroundTilemap
     */
    private void GenerateGroundTerrain()
    {
        for (int x = 0; x < groundColumnHeights.Count; x++) // For each column (x-coordinate) in the terrain
        {
            int height = groundColumnHeights[x]; // Get the column height at the x-coordinate
            double perlinNoiseValue = PerlinNoise.PerlinNoiseFunction(x / GROUND_SMOOTHNESS); // Compute perline noise for the column
            int heightOfNoise; // Height of the perlin noise

            // Compute the height of the perlin noise depending on the steepness of the terrain
            if (x > 0 && System.Math.Abs(height - groundColumnHeights[x - 1]) > 1) // If the terrain is very steep
            {
                heightOfNoise = (int)System.Math.Round(MAX_VERY_STEEP_NOISE_HEIGHT * perlinNoiseValue); // Determine the height of the noise for the column for very steep terrain
            }
            else if (x > 0 && System.Math.Abs(height - groundColumnHeights[x - 1]) == 1) // If the terrain is slightly steep
            {
                heightOfNoise = (int)System.Math.Round(MAX_SLIGHTLY_STEEP_NOISE_HEIGHT * perlinNoiseValue); // Determine the height of the noise for the column for slightly steep terrain
            }
            else // The terrain is flat
            {
                heightOfNoise = (int)System.Math.Round(MAX_FLAT_NOISE_HEIGHT * perlinNoiseValue); // Determine the height of the noise for the column for flat terrain
            }

            int columnHeightWithNoise = height + heightOfNoise; // The height of the column with the height of the noise

            if (columnHeightWithNoise > maxMountainHeightWithNoise)
            {
                maxMountainHeightWithNoise = columnHeightWithNoise;
            }

            for (int y = 0; y < columnHeightWithNoise; y++) // For every y-coordinate up to columnHeightWithNoise
            {
                SetTileToTilemap(groundTile, groundTilemap, x, y); // Set a groundTile to the groundTilemap at coordinates (x, y)
            }

            groundColumnHeightsWithNoise.Add(columnHeightWithNoise); // Fill in the groundColumnHeightsWithNoise list
        }
    }

    /* 
     * This method generates the water terrain in the valley of the ground terrain with perlin noise on top
     * The water is a set of blue tiles in the WaterTilemap
     */
    private void GenerateWaterTerrain()
    {
        PerlinNoise.ResetPerlinNoiseFunction(); // Reset the perlin noise function since it was previously used to generate the ground terrain

        // Get the boundaries of the water terrain
        int leftX = WATER_LEFT_X;
        int rightX = WATER_RIGHT_X;
        int bottomY = WATER_BOTTOM_Y;
        int topY = WATER_TOP_Y;

        for (int x = leftX; x < rightX; x++) // For each column in the water (x-coordinate)
        {
            double perlinNoiseValue = PerlinNoise.PerlinNoiseFunction(x / WATER_SMOOTHNESS); // Compute perline noise for the column
            int heightOfNoise = (int)System.Math.Round(MAX_WATER_NOISE_HEIGHT * perlinNoiseValue); // Compute the height of the noise using perlinNoiseValue
            int columnHeightWithNoise = topY + heightOfNoise; // The height of the column with the height of the noise

            for (int y = bottomY; y < columnHeightWithNoise; y++) // For each y-coordinate in the column less than columnHeightWithNoise
            {
                SetTileToTilemap(waterTile, waterTilemap, x, y); // Set a water tile to the waterTilemap at coordinates (x, y)
            }

            waterColumnHeightsWithNoise.Add(columnHeightWithNoise); // Fill in the waterColumnHeightsWithNoise list
        }
    }

    /*
     * This method initializes the groundColumnHeights list to set the basic shape of the terrain that will be generated
     */
    private void InitializeGroundColumnHeights()
    {
        int maxHeight = MAX_HEIGHT; // The maximum height of the terrain (height of the mountains) before noise
        int midHeight = PLATFORM_HEIGHT; // The height of the platforms before noise
        int minHeight = MIN_HEIGHT; // The minimum height of the terrain (hieight of the valley) before noise

        // Initialize section 1 (The left platform)
        int width1 = PLATFORM_WIDTH; // The width of section 1
        for (int x = 0; x < width1; x++)
        {
            groundColumnHeights.Add(midHeight); // Section 1 has a constant height
        }

        // Initialize section 2 (the rising part of the left mountain)
        int width2 = OUTER_MOUNTAIN_WIDTH;
        for (int x = 0; x < width2; x++)
        {
            if (midHeight + x < maxHeight) // If the height can continue to increase
            {
                groundColumnHeights.Add(midHeight + x); // Increase the height (linerarly)
            }
            else
            {
                groundColumnHeights.Add(maxHeight); // Otherwise, keep the height at the maxHeight
            }
        }

        // Initialize section 3 (the falling part of the left mountain)
        int width3 = INNER_MOUNTAIN_WIDTH;
        for (int x = 0; x < width3; x++)
        {
            if (maxHeight - 2 * x > minHeight) // If the height can continue to decrease
            {
                groundColumnHeights.Add(maxHeight - 2 * x); // Decrease the height (lineraly)
            }
            else
            {
                groundColumnHeights.Add(minHeight); // Otherwise, keep the height at the minHeight
            }
        }

        // Initialize section 4 (the valley)
        int width4 = VALLEY_WIDTH;
        for (int x = 0; x < width4; x++)
        {
            groundColumnHeights.Add(minHeight); // Section 4 has a constant height
        }

        // Initialize section 5 (the rising part of the right mountain)
        int width5 = INNER_MOUNTAIN_WIDTH;
        for (int x = 0; x < width5; x++)
        {
            if (minHeight + 2 * x < maxHeight) // If the height can continue to increase
            {
                groundColumnHeights.Add(minHeight + 2 * x); // Increase the height (lineraly)
            }
            else
            {
                groundColumnHeights.Add(maxHeight); // Otherwise, keep the height at the maxHeight
            }
        }

        // Initialize section 6 (the falling part of the right mountain)
        int width6 = OUTER_MOUNTAIN_WIDTH;
        for (int x = 0; x < width6; x++)
        {
            if (maxHeight - x > midHeight) // If the height can continue to decrease down to midHeight
            {
                groundColumnHeights.Add(maxHeight - x); // Decrease the height (linearly)
            }
            else
            {
                groundColumnHeights.Add(midHeight); // Otherwise, keep the height at midHeight
            }
        }

        // Initialize section 7 (the right platform)
        int width7 = PLATFORM_WIDTH;
        for (int x = 0; x < width7; x++)
        {
            groundColumnHeights.Add(midHeight); // Section 7 has a constant height
        }
    }

    /*
     * This method takes a tile, tilemap, and 2D coordinates and sets the tile to the tilemap at these coordinates.
     * Setting a tile to a tilemap means the tile will be visible on the tilemap
     */
    public static void SetTileToTilemap(Tile tile, Tilemap tilemap, int x, int y)
    {
        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }
}
