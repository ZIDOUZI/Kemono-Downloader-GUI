// using System.Data.SQLite;

// using YamlDotNet.RepresentationModel;

using System.Security.Cryptography;

namespace Kemono.Core.Models;

public static class Utils
{
    public static string GetMD5HashFromFile(this string file)
    {
        using var stream = new FileStream(file, FileMode.Open);
        return GetMD5HashFromFile(stream);
    }

    public static string GetMD5HashFromFile(this FileInfo file)
    {
        using var stream = file.OpenRead();
        return GetMD5HashFromFile(stream);
    }

    public static string GetMD5HashFromFile(this Stream file)
    {
        try
        {
            var retVal = MD5.Create().ComputeHash(file);
            return string.Join("", retVal.Select(it => it.ToString("x2")));
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
        }
    }


    public static bool DropWhere<TSource>(this ICollection<TSource> source, Func<TSource, bool> block) =>
        source.Where(block).Reverse().Aggregate(false, (current, s) => (block(s) && source.Remove(s)) || current);

    public static IEnumerable<TSource> OnEach<TSource>(this IEnumerable<TSource> source, Action<TSource> block) =>
        source.Apply(s => s.ForEach(block));

    public static DateTimeOffset FromUnixTimeMicroseconds(long microseconds) => DateTimeOffset
        .FromUnixTimeMilliseconds(microseconds / 1000).AddTicks(microseconds % 1000 * 10);

    public static long ToUnixTimeMicroseconds(this DateTimeOffset date) =>
        date.ToUnixTimeMilliseconds() * 1000 + date.Ticks / 10 % 1000;

    public static DateTimeOffset FromUnixTimeTicks(long ticks) =>
        DateTimeOffset.FromUnixTimeMilliseconds(ticks / 10000).AddTicks(ticks % 10000);

    public static long ToUnixTimeTicks(this DateTimeOffset date) =>
        date.ToUnixTimeMilliseconds() * 10000 + date.Ticks % 10000;

    public static DateTimeOffset FromUnixTimeSeconds(double ticks) =>
        DateTimeOffset.FromUnixTimeSeconds((long)ticks)
            .AddTicks((long)Math.Round(ticks % 1 * 1e7) % 10000000);

    public static double ToUnixTimeSecondsAccurateTick(this DateTimeOffset date) =>
        date.ToUnixTimeSeconds() + date.Ticks % 10000000 / 1e7;

    public static T WaitForResult<T>(this Task<T> task)
    {
        task.Wait();
        return task.ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static int IndexOf<TKey, TElement>(this Dictionary<TKey, TElement> source, TElement element) =>
        source.Values.ToList().IndexOf(element);

    public static int IndexOf<TKey, TElement>(this Dictionary<TKey, TElement> source, TKey key) =>
        source.Keys.ToList().IndexOf(key);

    /// <summary>
    ///     获取从 1970-01-01 到现在的毫秒数。
    /// </summary>
    /// <returns></returns>
    public static long GetTimeMillions() => new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

    /// <summary>
    ///     计算 1970-01-01 到指定 <see cref="DateTime" /> 的毫秒数。
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static long ToUnixTimeMilliseconds(this DateTime dateTime) =>
        new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();

    public static Dictionary<TKey, TElement> Zip<TKey, TElement>(
        this IEnumerable<TKey> source, IEnumerable<TElement> enumerator)
    {
        var dict = new Dictionary<TKey, TElement>();
        var keys = source as TKey[] ?? source.ToArray();
        var elements = enumerator as TElement[] ?? enumerator.ToArray();
        var max = Math.Max(keys.Length, elements.Length);
        for (var i = 0; i < max; i++)
        {
            dict[keys[i]] = elements[i];
        }

        return dict;
    }

    /// <summary>
    ///     将一个可迭代对象按照给定函数映射为一个字典. 原可迭代对象的值作为字典的值.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="mapping"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <returns></returns>
    public static Dictionary<TKey, TElement> AssociateTo<TKey, TElement>(
        this IEnumerable<TElement> source, Func<TElement, TKey> mapping) where TKey : notnull =>
        source.ToDictionary(mapping, x => x);

    public static KeyValuePair<TKey, TElement> First<TKey, TElement>(
        this Dictionary<TKey, TElement> source, Func<TKey, TElement, bool> selector) where TKey : notnull =>
        source.First(pair => selector(pair.Key, pair.Value));

    /// <summary>
    ///     将一个可迭代对象按照给定函数映射为一个字典
    /// </summary>
    /// <param name="source"></param>
    /// <param name="mapping"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <returns></returns>
    public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(
        this IEnumerable<TKey> source, Func<TKey, TElement> mapping) where TKey : notnull =>
        source.ToDictionary(x => x, mapping);

    /// <summary>
    ///     将一个可迭代对象按照给定函数映射为一个字典. 函数带有序数
    /// </summary>
    /// <param name="source"></param>
    /// <param name="mapping"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <returns></returns>
    public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(
        this IEnumerable<TKey> source, Func<TKey, int, TElement> mapping) where TKey : notnull =>
        source.Select((x, i) => new {Key = x, Index = i})
            .ToDictionary(arg => arg.Key, arg => mapping(arg.Key, arg.Index));

    /// <summary>
    ///     将一个键值对的可迭代对象转化为一个字典
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <returns></returns>
    public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(
        this IEnumerable<KeyValuePair<TKey, TElement>> source) where TKey : notnull =>
        source.ToDictionary(x => x.Key, x => x.Value);

    /// <summary>
    ///     将一个键值对的可迭代对象转化为一个字典
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <returns></returns>
    public static Dictionary<TKey, TElement> AsDictionary<TKey, TElement>(
        this KeyValuePair<TKey, TElement> source) where TKey : notnull =>
        new() {{source.Key, source.Value}};

    /// <summary>
    ///     同kotlin中的let
    /// </summary>
    /// <param name="source"></param>
    /// <param name="block"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static TResult Let<TSource, TResult>(this TSource source, Func<TSource, TResult> block) => block(source);

    public static TSource Apply<TSource>(this TSource source, Action<TSource> block)
    {
        block(source);
        return source;
    }

    /// <summary>
    ///     向字典中添加一个键值对
    /// </summary>
    /// <param name="source">原字典</param>
    /// <param name="pair">待添加的键值对</param>
    /// <typeparam name="TKey">字典键的类型</typeparam>
    /// <typeparam name="TElement">字典值的类型</typeparam>
    public static void Add<TKey, TElement>(
        this Dictionary<TKey, TElement> source, KeyValuePair<TKey, TElement> pair) where TKey : notnull =>
        source.Add(pair.Key, pair.Value);

    public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> block)
    {
        foreach (var item in source)
        {
            block(item);
        }
    }

    public static Task ForEach<TSource>(this IEnumerable<TSource> source, Func<TSource, Task> block) =>
        Task.Run(async () =>
        {
            foreach (var item in source)
            {
                await block(item);
            }
        });

    public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue> source, Action<TKey, TValue> block)
        where TKey : notnull
    {
        foreach (var (key, value) in source)
        {
            block(key, value);
        }
    }

    public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue> source, Action<TKey, TValue, int> block)
        where TKey : notnull
    {
        var i = 0;
        foreach (var (key, value) in source)
        {
            block(key, value, i++);
        }
    }

    public static bool[] Reverse(this uint value)
    {
        var array = new bool[(int)Math.Log2(value)];
        var i = 0;
        while (value != 0)
        {
            array[i++] = value % 2 == 1;
            value >>= 1;
        }

        return array;
    }

    public static string EscapePath(this string source) => Path.GetInvalidPathChars()
        .Aggregate(source, (current, c) => current.Replace(c, (char)(c == 32 ? 12288 : c + 65248)));

    public static string EscapeFile(this string source) => Path.GetInvalidFileNameChars()
        .Aggregate(source, (current, c) => current.Replace(c, (char)(c == 32 ? 12288 : c + 65248)));

    public static async Task<List<TResult>> Select<TSource, TResult>(this IEnumerable<TSource> source,
        Func<TSource, Task<TResult>> block)
    {
        var r = new List<TResult>();
        foreach (var s in source as TSource[] ?? source.ToArray())
        {
            r.Add(await block(s));
        }

        return r;
    }

    /*public static void OpenOrCreate(this SQLiteConnection sql)
    {
        try
        {
            sql.Open();
        }
        catch (Exception e)
        {
            var name = new Regex("Data Source ?= ?(.+?)[;$]").Match(sql.ConnectionString).Groups[1];
            var file = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\kemono\{name}";
            var directory = new FileInfo(file).Directory;
            if (!directory!.Exists) directory.Create();
            SQLiteConnection.CreateFile(file);
            sql.Open();
        }
    }

    public static IList<YamlDocument> Yaml(string path)
    {
        var file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Kemono{path}";
        var reader = new StreamReader(new FileStream(file, FileMode.OpenOrCreate));
        var yaml = new YamlStream();
        yaml.Load(reader);
        return yaml.Documents;
    }

    public static void test()
    {

    }*/
}