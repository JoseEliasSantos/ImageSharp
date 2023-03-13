// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static SixLabors.ImageSharp.SimdUtils;

namespace SixLabors.ImageSharp;

/// <inheritdoc/>
internal interface IShuffle3 : IComponentShuffle
{
}

internal readonly struct DefaultShuffle3 : IShuffle3
{
    public DefaultShuffle3(byte control)
        => this.Control = control;

    public byte Control { get; }

    [MethodImpl(InliningOptions.ShortMethod)]
    public void ShuffleReduce(ref ReadOnlySpan<byte> source, ref Span<byte> dest)
        => HwIntrinsics.Shuffle3Reduce(ref source, ref dest, this.Control);

    [MethodImpl(InliningOptions.ShortMethod)]
    public void RunFallbackShuffle(ReadOnlySpan<byte> source, Span<byte> dest)
    {
        ref byte sBase = ref MemoryMarshal.GetReference(source);
        ref byte dBase = ref MemoryMarshal.GetReference(dest);

        Shuffle.InverseMMShuffle(this.Control, out _, out int p2, out int p1, out int p0);

        for (nint i = 0; i < (uint)source.Length; i += 3)
        {
            Unsafe.Add(ref dBase, i + 0) = Unsafe.Add(ref sBase, (nint)(uint)p0 + i);
            Unsafe.Add(ref dBase, i + 1) = Unsafe.Add(ref sBase, (nint)(uint)p1 + i);
            Unsafe.Add(ref dBase, i + 2) = Unsafe.Add(ref sBase, (nint)(uint)p2 + i);
        }
    }
}
