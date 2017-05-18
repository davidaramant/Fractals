kernel void iterate_points(
    constant const float* cReals,
    constant const float* cImags,
    global int* finalIterations)
{
    float zReal, zImag = 0;

    float zReal2, zImag2 = 0;

    size_t globalId = get_global_id(0);

    float cReal = cReals[globalId];
    float cImag = cImags[globalId];

    int iterations = 0;

    for (int i = 0; i < 30000000; i++)
    {
        zImag = 2 * zReal * zImag + cImag;
        zReal = zReal2 - zImag2 + cReal;

        zReal2 = zReal * zReal;
        zImag2 = zImag * zImag;

        // Only increment if the point is still inside the circle
        iterations += (zReal2 + zImag2) <= 4.0f;
    }

    finalIterations[globalId] = iterations;
}
