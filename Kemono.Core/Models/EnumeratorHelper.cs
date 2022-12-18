using System.Runtime.CompilerServices;

namespace Kemono.Core.Models;

public static class EnumeratorHelper
{
    #region No Return

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DoWork<T>(this IEnumerator<T> iter, Action<T> run)
    {
        if (iter.Current != null)
        {
            run(iter.Current);
        }

        while (iter.MoveNext())
        {
            run(iter.Current);
        }
    }

    #endregion

    #region Single Return

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TResult> DoWork<TSource, TResult>(this IEnumerator<TSource> iter,
        Func<TSource, TResult> run)
    {
        if (iter.Current != null)
        {
            yield return run(iter.Current);
        }

        while (iter.MoveNext())
        {
            yield return run(iter.Current);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncEnumerable<TResult> DoWork<TSource, TResult>(this IEnumerator<TSource> iter,
        Func<TSource, Task<TResult>> run) => iter.DoWork(run, s =>
        Console.WriteLine($"I: Continue Work {run.Method.Name} with source {s}"));

    #endregion

    #region No Return

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DoWork<T>(this IEnumerator<T> iter, Action<T> run, Action<T> con)
    {
        if (iter.Current != null)
        {
            con(iter.Current);
            run(iter.Current);
        }

        while (iter.MoveNext())
        {
            run(iter.Current);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task DoWork<T>(this IEnumerator<T> iter, Func<T, Task> run, Action<T> con)
    {
        if (iter.Current != null)
        {
            con(iter.Current);
            await run(iter.Current);
        }

        while (iter.MoveNext())
        {
            await run(iter.Current);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task DoWork<T>(this IEnumerator<T> iter, Func<T, Task> run, Func<T, Task> con)
    {
        if (iter.Current != null)
        {
            await con(iter.Current);
            await run(iter.Current);
        }

        while (iter.MoveNext())
        {
            await run(iter.Current);
        }
    }

    #endregion

    #region Single Return

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TResult> DoWork<TSource, TResult>(this IEnumerator<TSource> iter,
        Func<TSource, TResult> run, Action<TSource> con)
    {
        if (iter.Current != null)
        {
            con(iter.Current);
            yield return run(iter.Current);
        }

        while (iter.MoveNext())
        {
            yield return run(iter.Current);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async IAsyncEnumerable<TResult> DoWork<TSource, TResult>(this IEnumerator<TSource> iter,
        Func<TSource, Task<TResult>> run, Action<TSource> con)
    {
        if (iter.Current != null)
        {
            con(iter.Current);
            yield return await run(iter.Current);
        }

        while (iter.MoveNext())
        {
            yield return await run(iter.Current);
        }
    }

    #endregion
}