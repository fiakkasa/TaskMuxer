using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace TaskMuxer.Tests;

public static class TestingExtensions
{
    public static IConfigurationRoot GetConfigRoot(this Dictionary<string, object> config) =>
        new ConfigurationBuilder()
            .AddJsonStream(
                new MemoryStream(
                    JsonSerializer.SerializeToUtf8Bytes(config)
                )
            )
            .Build();

    public static T AddToConfigBuilder<T>(this T builder, Dictionary<string, object> config) where T : IConfigurationBuilder
    {
        builder.AddJsonStream(
            new MemoryStream(
                JsonSerializer.SerializeToUtf8Bytes(config)
            )
        );

        return builder;
    }
}