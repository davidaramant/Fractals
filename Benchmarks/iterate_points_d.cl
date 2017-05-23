kernel void iterate_points(
    constant const double* cReals,
    constant const double* cImags,
    global int* finalIterations)
{
    double zReal = 0, zImag = 0;
    double zReal2 = 0, zImag2 = 0;

    int globalId = (int)get_global_id(0);

    double cReal = cReals[globalId];
    double cImag = cImags[globalId];

    int iterations = 0;

    for (int i = 0; i < 15 * 1000 * 1000; i++)
    {
        zImag = 2 * zReal * zImag + cImag;
        zReal = zReal2 - zImag2 + cReal;

        zReal2 = zReal * zReal;
        zImag2 = zImag * zImag;

        iterations += (zReal2 + zImag2 <= 4);
    }

    finalIterations[globalId] = iterations;
}

kernel void iterate_points_early_return(
    constant const double* cReals,
    constant const double* cImags,
    global int* finalIterations)
{
    double zReal = 0, zImag = 0;
    double zReal2 = 0, zImag2 = 0;

    int globalId = (int)get_global_id(0);

    double cReal = cReals[globalId];
    double cImag = cImags[globalId];

    int i = 0;

    for (i = 0; i < 15 * 1000 * 1000; i++)
    {
        zImag = 2 * zReal * zImag + cImag;
        zReal = zReal2 - zImag2 + cReal;

        zReal2 = zReal * zReal;
        zImag2 = zImag * zImag;

        if (zReal2 + zImag2 > 4)
        {
            break;
        }
    }

    finalIterations[globalId] = i;
}

kernel void iterate_points_limit_argument(
    constant const double* cReals,
    constant const double* cImags,
    global int* finalIterations,
    const int maxIterations)
{
    double zReal = 0, zImag = 0;
    double zReal2 = 0, zImag2 = 0;

    int globalId = (int)get_global_id(0);

    double cReal = cReals[globalId];
    double cImag = cImags[globalId];

    int iterations = 0;

    for (int i = 0; i < maxIterations; i++)
    {
        zImag = 2 * zReal * zImag + cImag;
        zReal = zReal2 - zImag2 + cReal;

        zReal2 = zReal * zReal;
        zImag2 = zImag * zImag;

        iterations += (zReal2 + zImag2 <= 4);
    }

    finalIterations[globalId] = iterations;
}

kernel void iterate_points_fma(
    constant const double* cReals,
    constant const double* cImags,
    global int* finalIterations)
{
    double zReal = 0, zImag = 0;
    double zReal2 = 0, zImag2 = 0;

    int globalId = (int)get_global_id(0);

    double cReal = cReals[globalId];
    double cImag = cImags[globalId];

    int iterations = 0;

    for (int i = 0; i < 15 * 1000 * 1000; i++)
    {
        zImag = 2 * fma(zReal, zImag, cImag);
        zReal = zReal2 - zImag2 + cReal;

        zReal2 = zReal * zReal;
        zImag2 = zImag * zImag;

        iterations += (zReal2 + zImag2 <= 4);
    }

    finalIterations[globalId] = iterations;
}
