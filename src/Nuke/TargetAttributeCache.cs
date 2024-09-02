using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using Nuke.Common.IO;
using Rocket.Surgery.Nuke.GithubActions;

namespace Rocket.Surgery.Nuke;

internal static class TargetAttributeCache
{
    public static IEnumerable<string> GetTargetsWithAttribute<TAttribute>() where TAttribute : Attribute
    {
        return _cache
              .Where(z => z.Value.Contains(typeof(TAttribute).FullName!))
              .Select(z => z.Key);
    }

    private static readonly FrozenDictionary<string, FrozenSet<string>> _cache = EnsureAttributeCacheIsUptoDate();

    private static FrozenDictionary<string, FrozenSet<string>> PopulateCacheFromDisk(AbsolutePath path)
    {
        return JsonSerializer.Deserialize<Dictionary<string, HashSet<string>>>(path.ReadAllText())!
                             .ToFrozenDictionary(z => z.Key, z => z.Value.ToFrozenSet());
    }

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
              .Select(z => ( z.Key, Targets: z.SelectMany(x => x.GetTargets()).Distinct().OrderBy(z => z).ToImmutableArray() ))
              .OrderBy(z => z.Key)
              .ToImmutableSortedDictionary(z => z.Key, z => z.Targets);
    }

    private static FrozenDictionary<string, FrozenSet<string>> EnsureAttributeCacheIsUptoDate()
    {
        var attributeCache = NukeBuild.RootDirectory / ".nuke" / "attributes.cache";
        if (!attributeCache.ShouldUpdate(createFile: false)) return PopulateCacheFromDisk(attributeCache);

        var cache = PopulateCache();
        attributeCache.WriteAllText(JsonSerializer.Serialize(cache, new JsonSerializerOptions { WriteIndented = true, }));
        return cache.ToFrozenDictionary(z => z.Key, z => z.Value.ToFrozenSet());
    }

    private record CacheItem(string Key, ExcludeTargetAttribute? ExcludeTarget, NonEntryTargetAttribute? NonEntryTarget)
    {
        public IEnumerable<string> GetTargets()
        {
            if (ExcludeTarget is { }) yield return ExcludeTarget.GetType().FullName!;
            if (NonEntryTarget is { }) yield return NonEntryTarget.GetType().FullName!;
        }
    }
}