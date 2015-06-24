# Overview

A tool to generated Buddhabrot images.

# Usage

The application is running using a command-line program (`Fractals.Console.exe`).

There are several operations that are performed through those same applications.
To control which operation is done, switches are passed to the application:

| Switch | Description                                                               |
| -t     | Controls the type of operation being performed                            |
| -c     | The path to the (XML) configuration file for the operation                |
| -u     | (Optional) The number of logical processes to use for parallel operations |

## Find Points

Operation Switch: `FindPoints`

Find Buddhabrot (or Mandlebrot) points using different point selection strategies.

Configuration File:

| Argument             | Description                                                                                                   |
| OutputDirectory      | The location where the found points will be stored                                                            |
| OutputFilenamePrefix | The prefix for the output file names (a unique suffix will be added)                                          |
| MinimumThreshold     | The minimum number of times a point must be valid to be included in the output                                |
| MaximumThreshold     | The maximum number of times to validate a point before giving up on it (i.e. bailout value)                   |
| SelectionStrategy    | The strategy to use when selecting the random points for evaluation                                           |
| InputDirectory       | (If using an "edge" strategy) The directory containing the input file                                         |
| InputEdgeFilename    | (If using an "edge" strategy) The filename containing the locations of the grids bordering the Mandelbrot set |

### Buddhabrot Selection Strategies

| Strategy               | Description                                                                                                            |
| Random                 | Generate a random complex number                                                                                       |
| BulbsExcluded          | Exclude the "bulbs" from the Mandelbrot set (these are known to not generate Buddhabrot points, computationally cheap) |
| EdgesWithBulbsExcluded | Only use points from the grids on the edge of the Mandelbrot (excluding the "bulbs")                                   |

### Mandelbrot Selection Strategies

| Strategy               | Description                                                                                                            |
| Random                 | Generate a random complex number                                                                                       |
| BulbsOnly              | Only uses points within the "bulbs" from the Mandelbrot set (computationally cheap)                                    |
| EdgesAndBulbsOnly      | Only use points from the grids on the edge of the Mandelbrot or within the "bulbs"                                     |

## Plot Points

Operation Switch: `PlotPoints`

Plot the Buddhabrot points for a specified number of iterations, tracking the hit count per location.

| Argument             | Description                                                                                                   |
| Resolution           | The resolution at which to plot the points (must match the rendering resolution)                              |
| Bailout              | The bailout value (how many iterations of the function to apply per point while tracking vectors)             |
| InputDirectory       | The directory containing the input files                                                                      |
| InputFilePattern     | The file pattern to use to find the input files (the list of points to plot)                                  |
| OutputDirectory      | The location where the output file will be stored                                                             |
| OutputFilename       | The name of the output file containing the plotted point values                                               |

## Render Plot

Operation Switch: `RenderPlot`

Render the plotted points into an image.

| Argument             | Description                                                                                                   |
| Resolution           | The resolution at which to render the image                                                                   |
| InputDirectory       | The directory containing the input file                                                                       |
| InputFilename        | The name of the file containing the plotting results                                                          |
| OutputDirectory      | The location where the output file will be stored                                                             |
| OutputFilename       | The name of the rendered output file                                                                          |

## Render Points

Operation Switch: `RenderPoints`

Render the Mandlebrot points into an image.

| Argument             | Description                                                                                                   |
| Resolution           | The resolution at which to render the image                                                                   |
| InputDirectory       | The directory containing the input file                                                                       |
| InputFilename        | The name of the file containing the plotting results                                                          |
| OutputDirectory      | The location where the output file will be stored                                                             |
| OutputFilename       | The name of the rendered output file                                                                          |

## Find Edge Areas

Operation Switch: `FindEdgeAreas`

Find regions near the Mandlebrot set (that are more likely to have Buddhabrot points).

| Argument             | Description                                                                                                   |
| Resolution           | The resolution at which to search for points (recommended to not exceed 8192x8192)                            |
| Grid Size            | The size of a single side of a grid (out of a total size of 3.0)                                              |
| OutputDirectory      | The location where the output file will be stored                                                             |
| OutputFilename       | The name of the output file                                                                                   |

## Render Nebula Plots

Operation Switch: `RenderNebulaPlots`

Render three separate plots of points (generally gathered with different bailout values) into a multi-color image.

| Argument             | Description                                                                                                   |
| Resolution           | The resolution at which to render the image                                                                   |
| InputDirectory       | The directory containing the input file                                                                       |
| RedInputFilename     | The name of the file containing the plotting results to be used for the *red* channel                         |
| GreenInputFilename   | The name of the file containing the plotting results to be used for the *green* channel                       |
| BlueInputFilename    | The name of the file containing the plotting results to be used for the *blue* channel                        |
| OutputDirectory      | The location where the output file will be stored                                                             |
| OutputFilename       | The name of the rendered output file                                                                          |

# References

* [Mandlebrot](https://en.wikipedia.org/wiki/Mandelbrot_set)
* [Buddhabrot](https://en.wikipedia.org/wiki/Buddhabrot)
