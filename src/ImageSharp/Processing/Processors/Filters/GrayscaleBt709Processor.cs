﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace SixLabors.ImageSharp.Processing.Processors.Filters
{
    /// <summary>
    /// Applies a grayscale filter matrix using the given amount and the formula as specified by ITU-R Recommendation BT.709
    /// </summary>
    public class GrayscaleBt709Processor : FilterProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrayscaleBt709Processor"/> class.
        /// </summary>
        /// <param name="amount">The proportion of the conversion. Must be between 0 and 1.</param>
        public GrayscaleBt709Processor(float amount)
            : base(KnownFilterMatrices.CreateGrayscaleBt709Filter(amount))
        {
            this.Amount = amount;
        }

        /// <summary>
        /// Gets the proportion of the conversion.
        /// </summary>
        public float Amount { get; }

        // TODO: Move this to an appropriate extension method if possible.
        internal void ApplyToFrame<TPixel>(ImageFrame<TPixel> frame, Rectangle sourceRectangle, Configuration configuration)
            where TPixel : struct, IPixel<TPixel>
        {
            var processorImpl = new FilterProcessor<TPixel>(new GrayscaleBt709Processor(1F));
            processorImpl.Apply(frame, sourceRectangle, configuration);
        }
    }
}