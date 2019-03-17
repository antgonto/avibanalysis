using System;

public class CityContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

        // only serializes properties that start with the specified string
        properties = properties.Where(p => p.PropertyName.StartsWith("City")).ToList();

        return properties;
    }
}

