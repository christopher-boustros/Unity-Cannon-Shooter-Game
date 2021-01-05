/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is not linked to a game object
// This script is used in the ProceduralTerrainGeneration.cs script
using System.Collections.Generic;
using UnityEngine;

/*
 * This class implements a one-dimensional perlin noise function.
 * The perlin noise function is implemented by implementing a noise function using cosine interpolation
 * and summing different octaves of that noise function, each at a specified amplitude.
 * This method of implementing a Perlin noise function is based on this source: https://www.cs.umd.edu/class/spring2018/cmsc425/Lects/lect12-1d-perlin.pdf
 */
public class PerlinNoise : MonoBehaviour
{
    private static System.Random random = new System.Random();
    public static int numberOfSamples = 10000; // The number of random numbers to generate. This is the variable "n" im the document cited above.
    public static int numberOfOctaves = 4; // The number of octaves of the noise function to sum to implement the perlin noise function
    private static List<double> randomNumbers = new List<double>(); // This list will store the sequence of random numbers generated

    // Static constructor
    static PerlinNoise()
    {
        InitializeRandomNumbers();
    }

    /*
     * This method initializes the randomNumbers list
     */
    private static void InitializeRandomNumbers()
    {
        // Generate a sequence of random floating-point numbers between 0 and 1
        for (int i = 0; i < numberOfSamples; i++) // The mumber random numbers to generate is numberOfSamples
        {
            randomNumbers.Add(random.NextDouble()); // Add a random number between 0 and 1 to the sequence
        }
    }

    /*
     * Performs cosine interpolation between i and j using paramater a that varies between 0 and 1
     */
    private static double Cerp(double i, double j, double a)
    {
        double g = (1 - System.Math.Cos(System.Math.PI * a)) / 2.0; // convert a to be used for cosine interpolation instead of linear interpolation
        return (1 - g) * i + g * j; // Perform cosine interpolation using g
    }

    /*
     * This is the noise function.
     * It performs cosine interpolation between the numbers inside randomNumbers.
     * It always returns a number between 0 and 1.
     */
    private static double Noise(double x)
    {
        // Taking the remainder of floor(x) so that the domain of the noise function is not limited to numbers between 0 and numberOfSamples.
        // So, when x exceeds numberOfSamples, the noise function starts a new period. The number of possible inputs (samples) in a single period is numberOfSamples.
        // So, the noise function repeats such that noise(0) = noise(numberOfSamples).
        int i = (int)System.Math.Floor(x) % numberOfSamples; // This is an index of the randomNumbers list

        double a = x - i; // The fractional part of x (used for interpolation)
        double noiseValue;
        if (i != numberOfSamples - 1)
        {
            noiseValue = Cerp(randomNumbers[i], randomNumbers[i + 1], a); // Compute the cosine interpolation between the number at index i and at index i+1 in randomNumbers
        }
        else
        {
            // There is no number at index i+1
            noiseValue = Cerp(randomNumbers[i], randomNumbers[0], a); // Compute the cosine interpolation between the last and first point in randomNumbers
        }

        return noiseValue;
    }

    /*
     * This is the perlin noise function.
     * It is the sum of multiple octaves of the noise function.
     * It always returns a number between 0 and 1.
     */
    public static double PerlinNoiseFunction(double x)
    {
        double perlinNoiseValue = 0.0;

        for (int i = 1; i <= numberOfOctaves; i++) // Iterate once for every octave to sum
        {
            double periodFactor = System.Math.Pow(2, i);
            double amplitudeFactor = 1 / periodFactor;
            perlinNoiseValue += amplitudeFactor * Noise(periodFactor * x);
        }

        return perlinNoiseValue;
    }

    /*
     * This method resets the perlin noise function by reinitializing the randomNumbers list
     */
    public static void ResetPerlinNoiseFunction()
    {
        randomNumbers.Clear(); // Remove all numbers in the list
        InitializeRandomNumbers(); // Initialize the list
    }
}
