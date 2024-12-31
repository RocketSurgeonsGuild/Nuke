using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

internal static class TargetAttributeCache
{
    public static IEnumerable<string> GetTargetsWithAttribute<TAttribute>() where TAttribute : Attribute
    {
        var result = _cache
                    .Where(z => z.Value.Contains(typeof(TAttribute).FullName!))
                    .Select(z => z.Key)
                    .ToArray();
        return result;
    }

    internal static FrozenDictionary<string, FrozenSet<string>> BuildCache()
    {
        var cache = PopulateCache();
        AttributeCache.WriteAllText(JsonSerializer.Serialize(cache, new JsonSerializerOptions { WriteIndented = true }));
        return cache.ToFrozenDictionary(z => z.Key, z => z.Value.ToFrozenSet());
    }

    private static readonly FrozenDictionary<string, FrozenSet<string>> _cache = EnsureAttributeCacheIsUptoDate();

    private static FrozenDictionary<string, FrozenSet<string>> PopulateCacheFromDisk() => JsonSerializer.Deserialize<Dictionary<string, HashSet<string>>>(AttributeCache.ReadAllText())!
                                                                                                        .ToFrozenDictionary(z => z.Key, z => z.Value.ToFrozenSet());

    private static ImmutableSortedDictionary<string, ImmutableArray<string>> PopulateCache()
    {
        var items = new List<CacheItem>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var lookups = assembly
                             .GetTypes()
                             .SelectMany(z => z.GetProperties().Where(static z => z.PropertyType == typeof(Target)))
                             .Select(
                                  property => new CacheItem(
                                      property.Name,
                                      property.GetCustomAttribute<ExcludeTargetAttribute>(),
                                      property.GetCustomAttribute<NonEntryTargetAttribute>()
                                  )
                              );
                items.AddRange(lookups);
            }
            catch
            {
                //?
            }
        }

        return items
              .GroupBy(z => z.Key)
              .Select(z => ( z.Key, Targets: z.SelectMany(x => x.GetTargets()).Distinct().Order().ToImmutableArray() ))
              .OrderBy(z => z.Key)
              .ToImmutableSortedDictionary(z => z.Key, z => z.Targets);
    }

    private static AbsolutePath AttributeCache => NukeBuild.TemporaryDirectory / "attributes.cache";

    private static FrozenDictionary<string, FrozenSet<string>> EnsureAttributeCacheIsUptoDate() => !AttributeCache.ShouldUpdate(createFile: false) ? PopulateCacheFromDisk() : BuildCache();

    private record CacheItem(string Key, ExcludeTargetAttribute? ExcludeTarget, NonEntryTargetAttribute? NonEntryTarget)
    {
        public IEnumerable<string> GetTargets()
        {
            if (ExcludeTarget is { }) yield return ExcludeTarget.GetType().FullName!;
            if (NonEntryTarget is { }) yield return NonEntryTarget.GetType().FullName!;
        }
    }
}
