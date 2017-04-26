__kernel __attribute__((vec_type_hint(float)))
void iterate_points(
    __constant const float* cReals,
    __constant const float* cImags,
    __global int* finalIterations)
{
    // Initialize z
    float zReal = 0;
    float zImag = 0;

    // Initialize z2
    float zReal2 = 0;
    float zImag2 = 0;

    size_t globalId = get_global_id(0);

    float cReal = cReals[globalId];
    float cImag = cImags[globalId];

    int iterations = 0;

    for (int i = 0; i < 15000000; i++)
    {
        float zRealTemp = zReal2 - zImag2 + cReal;
        zImag = 2 * zReal * zImag + cImag;
        zReal = zRealTemp;

        // zReal2 = pown( zReal, 2 );
        // zImag2 = pown( zImag, 2 );
        zReal2 = zReal * zReal;
        zImag2 = zImag * zImag;

        // Only increment if the point is still inside the circle
        iterations += islessequal(zReal2 + zImag2, 4.0);
    }

    finalIterations[globalId] = iterations;
}