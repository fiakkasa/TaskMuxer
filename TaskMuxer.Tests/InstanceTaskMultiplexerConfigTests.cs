using System.ComponentModel.DataAnnotations;

namespace TaskMuxer.Tests;

public class InstanceTaskMultiplexerConfigTests
{
    [Fact]
    public void Valid_Config()
    {
        var obj = new InstanceTaskMultiplexerConfig();
        var results = new List<ValidationResult>();

        Assert.True(Validator.TryValidateObject(obj, new(obj), results, true));
        Assert.Empty(results);
    }

    [Fact]
    public void Invalid_Config_PreserveExecutionResultDuration()
    {
        var obj = new InstanceTaskMultiplexerConfig
        {
            PreserveExecutionResultDuration = TimeSpan.FromMinutes(-1)
        };
        var results = new List<ValidationResult>();

        Assert.False(Validator.TryValidateObject(obj, new(obj), results, true));
        Assert.Single(results, x => x.MemberNames.ToArray() is [nameof(InstanceTaskMultiplexerConfig.PreserveExecutionResultDuration)]);
    }

    [Fact]
    public void Invalid_Config_ExecutionTimeout()
    {
        var obj = new InstanceTaskMultiplexerConfig
        {
            ExecutionTimeout = TimeSpan.Zero
        };
        var results = new List<ValidationResult>();

        Assert.False(Validator.TryValidateObject(obj, new(obj), results, true));
        Assert.Single(results, x => x.MemberNames.ToArray() is [nameof(InstanceTaskMultiplexerConfig.ExecutionTimeout)]);
    }

    [Fact]
    public void Invalid_Config_LongRunningTaskExecutionTimeout()
    {
        var obj = new InstanceTaskMultiplexerConfig
        {
            ExecutionTimeout = TimeSpan.FromSeconds(30),
            LongRunningTaskExecutionTimeout = TimeSpan.FromSeconds(10)
        };
        var results = new List<ValidationResult>();

        Assert.False(Validator.TryValidateObject(obj, new(obj), results, true));
        Assert.Single(results, x => x.MemberNames.ToArray() is [nameof(InstanceTaskMultiplexerConfig.LongRunningTaskExecutionTimeout)]);
    }
}
