using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility {

    /// <summary>
    /// Fractal Borwnian Motion. Adds multiple perlin noises together where each new perlin noise is slightly bigger or smaller than the last.
    /// This generates a smoother height for a terrain point.
    /// </summary>
    /// <param name="x"> x location </param>
    /// <param name="y"> y location </param>
    /// <param name="oct"> How many perlin noises to be calculated </param>
    /// <param name="persistance"> How much increase or decrease will the next noise will have </param>
    /// <returns> A height for the terrain </returns>
    public static float fBM(float x, float y, int oct, float persistance) {

        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;


        for (int i = 0; i < oct; i++) {
            total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistance;
            frequency *= 2;
        }

        return total / maxValue;
    }

    public static float Map(float value, float originalMin, float originalMax, float targetMin, float targetMax) {
        return (value - originalMin) * (targetMax - targetMin) / (originalMax - originalMin) + targetMin;
    }
}