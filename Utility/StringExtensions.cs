using System;

namespace GameCore.Utility;

public static class StringExtensions
{
    /// <summary>
    /// Checks for string equality without allocation
    /// </summary>
    /// <param name="compare"></param>
    /// <param name="stringA"></param>
    /// <param name="stringB"></param>
    /// <returns></returns>
    public static bool Equals(this string compare, string stringA, string stringB)
    {
        int length = stringA.Length + stringB.Length;

        if (compare.Length != length)
            return false;

        Span<char> span = stackalloc char[length];
        int spanIndex = 0;

        stringA.CopyTo(span);
        spanIndex += stringA.Length;
        stringB.CopyTo(span.Slice(spanIndex, stringB.Length));

        return compare.AsSpan() == span;
    }

    /// <summary>
    /// Checks for string equality without allocation
    /// </summary>
    /// <param name="compare"></param>
    /// <param name="stringA"></param>
    /// <param name="stringB"></param>
    /// <param name="stringC"></param>
    /// <returns></returns>
    public static bool Equals(this string compare, string stringA, string stringB, string stringC)
    {
        int length = stringA.Length + stringB.Length + stringC.Length;

        if (compare.Length != length)
            return false;

        Span<char> span = stackalloc char[length];
        int spanIndex = 0;

        stringA.CopyTo(span);
        spanIndex += stringA.Length;
        stringB.CopyTo(span.Slice(spanIndex, stringB.Length));
        spanIndex += stringB.Length;
        stringC.CopyTo(span.Slice(spanIndex, stringC.Length));

        return compare.AsSpan() == span;
    }

    /// <summary>
    /// Checks for string equality without allocation
    /// </summary>
    /// <param name="compare"></param>
    /// <param name="stringA"></param>
    /// <param name="stringB"></param>
    /// <param name="stringC"></param>
    /// <param name="stringD"></param>
    /// <returns></returns>
    public static bool Equals(this string compare, string stringA, string stringB, string stringC, string stringD)
    {
        int length = stringA.Length + stringB.Length + stringC.Length + stringD.Length;

        if (compare.Length != length)
            return false;

        Span<char> span = stackalloc char[length];
        int spanIndex = 0;

        stringA.CopyTo(span);
        spanIndex += stringA.Length;
        stringB.CopyTo(span.Slice(spanIndex, stringB.Length));
        spanIndex += stringB.Length;
        stringC.CopyTo(span.Slice(spanIndex, stringC.Length));
        spanIndex += stringC.Length;
        stringD.CopyTo(span.Slice(spanIndex, stringD.Length));

        return compare.AsSpan() == span;
    }
}
