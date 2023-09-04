using System;
using System.ComponentModel.DataAnnotations;

namespace TaskMuxer.Tests;

public class InstanceTaskMultiplexerConfigTests
{
    [Fact]
    public void Valid_Config()
    {
        var obj = new InstanceTaskMultiplexerConfig();

        Assert.True(Validator.TryValidateObject(obj, new(obj), new List<ValidationResult>(), true));
    }

    [Fact]
    public void Invalid_Config_PreserveExecutionResultDuration()
    {
        var obj = new InstanceTaskMultiplexerConfig
        {
            PreserveExecutionResultDuration = TimeSpan.FromMinutes(-1)
        };

        Assert.False(Validator.TryValidateObject(obj, new(obj), new List<ValidationResult>(), true));
    }

    [Fact]
    public void Invalid_Config_ExecutionTimeout()
    {
        var obj = new InstanceTaskMultiplexerConfig
        {
            ExecutionTimeout = TimeSpan.Zero
        };

        Assert.False(Validator.TryValidateObject(obj, new(obj), new List<ValidationResult>(), true));
    }
}
