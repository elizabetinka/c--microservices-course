using Microsoft.Extensions.Configuration;

namespace Task2.Library;

public class CustomConfigurationProvider : ConfigurationProvider
{
    public override void Load()
    {
    }

    public void LoadDataFromConfiguration(IEnumerable<KeyValuePair<string, string?>> configurations, bool update = true)
    {
        var configurationValues = Data.ToList();
        var keyValuePairs = configurations.ToList();
        if (configurationValues.SequenceEqual(keyValuePairs)) return;
        if (update)
        {
            Data = keyValuePairs.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        }
        else
        {
            foreach (KeyValuePair<string, string?> keyValuePair in keyValuePairs.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase)) Data.Add(keyValuePair);
        }

        OnReload();
    }
}