# Raytracing In One Weekend - C#

> Note: due to some SIMD usage, this will not currently work on x64. I will fix this at... some point

This is an implementation of [Raytracing In One Weekend](https://raytracing.github.io/books/RayTracingInOneWeekend.html#wherenext?/afinalrender) in C#.

By default, it took a long time (hour+) to render on my laptop so I built some acceleration structures to get it down to just under 30 seconds, without any sacrifice of quality.

When running, make sure to use release mode!

```
  dotnet run -c Release # Release must be capitalised!
```
