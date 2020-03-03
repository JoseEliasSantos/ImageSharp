// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Processing.Processors.Quantization
{
    /// <summary>
    /// Gets the closest color to the supplied color based upon the Euclidean distance.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    internal readonly struct EuclideanPixelMap<TPixel>
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private readonly Vector4[] vectorCache;
        private readonly ConcurrentDictionary<TPixel, int> distanceCache;
        private readonly ReadOnlyMemory<TPixel> palette;
        private readonly int length;

        /// <summary>
        /// Initializes a new instance of the <see cref="EuclideanPixelMap{TPixel}"/> struct.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="palette">The color palette to map from.</param>
        /// <param name="length">The length of the color palette.</param>
        [MethodImpl(InliningOptions.ShortMethod)]
        public EuclideanPixelMap(Configuration configuration, ReadOnlyMemory<TPixel> palette, int length)
        {
            this.palette = palette;
            this.length = length;
            ReadOnlySpan<TPixel> paletteSpan = this.palette.Span.Slice(0, this.length);
            this.vectorCache = new Vector4[length];

            // Use the same rules across all target frameworks.
            this.distanceCache = new ConcurrentDictionary<TPixel, int>(Environment.ProcessorCount, 31);
            PixelOperations<TPixel>.Instance.ToVector4(configuration, paletteSpan, this.vectorCache);
        }

        /// <summary>
        /// Returns the palette span.
        /// </summary>
        /// <returns>The <seealso cref="ReadOnlySpan{TPixel}"/>.</returns>
        [MethodImpl(InliningOptions.ShortMethod)]
        public ReadOnlySpan<TPixel> GetPaletteSpan() => this.palette.Span.Slice(0, this.length);

        /// <summary>
        /// Returns the closest color in the palette and the index of that pixel.
        /// The palette contents must match the one used in the constructor.
        /// </summary>
        /// <param name="color">The color to match.</param>
        /// <param name="match">The matched color.</param>
        /// <returns>The <see cref="int"/> index.</returns>
        [MethodImpl(InliningOptions.ShortMethod)]
        public int GetClosestColor(TPixel color, out TPixel match)
        {
            ref TPixel paletteRef = ref MemoryMarshal.GetReference(this.GetPaletteSpan());

            // Check if the color is in the lookup table
            if (!this.distanceCache.TryGetValue(color, out int index))
            {
                return this.GetClosestColorSlow(color, ref paletteRef, out match);
            }

            match = Unsafe.Add(ref paletteRef, index);
            return index;
        }

        [MethodImpl(InliningOptions.ShortMethod)]
        private int GetClosestColorSlow(TPixel color, ref TPixel paletteRef, out TPixel match)
        {
            // Loop through the palette and find the nearest match.
            int index = 0;
            float leastDistance = float.MaxValue;
            var vector = color.ToVector4();
            ref Vector4 vectorCacheRef = ref MemoryMarshal.GetReference<Vector4>(this.vectorCache);

            for (int i = 0; i < this.length; i++)
            {
                Vector4 candidate = Unsafe.Add(ref vectorCacheRef, i);
                float distance = Vector4.DistanceSquared(vector, candidate);

                // If it's an exact match, exit the loop
                if (distance == 0)
                {
                    index = i;
                    break;
                }

                if (distance < leastDistance)
                {
                    // Less than... assign.
                    index = i;
                    leastDistance = distance;
                }
            }

            // Now I have the index, pop it into the cache for next time
            this.distanceCache[color] = index;
            match = Unsafe.Add(ref paletteRef, index);
            return index;
        }
    }
}
