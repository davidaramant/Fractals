kernel void iterate_points(
    constant const double* cReals,
    constant const double* cImags,
    global int* finalIterations)
{
    double zReal, zImag = 0;

    double zReal2, zImag2 = 0;

    size_t globalId = get_global_id(0);

    double cReal = cReals[globalId];
    double cImag = cImags[globalId];

    const int maxIterations = 15 * 1000 * 1000;
    int iterations = 0;

    for (int i = 0; i < maxIterations; i++)
    {
        zImag = 2 * zReal * zImag + cImag;
        zReal = zReal2 - zImag2 + cReal;

        zReal2 = zReal * zReal;
        zImag2 = zImag * zImag;

        // Only increment if the point is still inside the circle
        iterations += islessequal(zReal2 + zImag2, 4.0);
    }

    finalIterations[globalId] = iterations;
}

kernel void iterate_points_limit_argument(
    constant const double* cReals,
    constant const double* cImags,
    global int* finalIterations,
    const int maxIterations)
{
    double zReal, zImag = 0;

    double zReal2, zImag2 = 0;

    size_t globalId = get_global_id(0);

    double cReal = cReals[globalId];
    double cImag = cImags[globalId];

    int iterations = 0;

    for (int i = 0; i < maxIterations; i++)
    {
        zImag = 2 * zReal * zImag + cImag;
        zReal = zReal2 - zImag2 + cReal;

        zReal2 = zReal * zReal;
        zImag2 = zImag * zImag;

        // Only increment if the point is still inside the circle
        iterations += islessequal(zReal2 + zImag2, 4.0);
    }

    finalIterations[globalId] = iterations;
}

kernel void iterate_points_fma(
    constant const double* cReals,
    constant const double* cImags,
    global int* finalIterations)
{
    double zReal, zImag = 0;

    double zReal2, zImag2 = 0;

    size_t globalId = get_global_id(0);

    double cReal = cReals[globalId];
    double cImag = cImags[globalId];

    const int maxIterations = 15 * 1000 * 1000;
    int iterations = 0;

    for (int i = 0; i < maxIterations; i++)
    {
        zImag = 2 * fma(zReal, zImag, cImag);
        zReal = zReal2 - zImag2 + cReal;

        zReal2 = zReal * zReal;
        zImag2 = zImag * zImag;

        // Only increment if the point is still inside the circle
        iterations += islessequal(zReal2 + zImag2, 4.0);
    }

    finalIterations[globalId] = iterations;
}
