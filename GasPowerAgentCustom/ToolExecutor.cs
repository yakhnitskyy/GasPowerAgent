using System.Text.Json;
using GasPowerAgentCustom;
using OpenAI.Chat;

public sealed class ToolExecutor
{
    private readonly MarketService _marketService;

    public ToolExecutor(MarketService marketService)
    {
        _marketService = marketService;
    }

    public string Execute(ChatToolCall toolCall)
    {
        Console.WriteLine(
            $"  TOOL REQUEST: {toolCall.FunctionName}");

        Console.WriteLine(
            $"  ARGUMENTS: {toolCall.FunctionArguments}");

        try
        {
            return toolCall.FunctionName switch
            {
                "get_gas_price"
                    => GetGasPrice(toolCall.FunctionArguments),

                "get_power_price"
                    => GetPowerPrice(toolCall.FunctionArguments),

                "calculate_percentage_change"
                    => CalculatePercentageChange(
                        toolCall.FunctionArguments),

                _
                    => JsonSerializer.Serialize(new
                    {
                        success = false,
                        error = "UnknownTool",
                        message =
                            $"Unknown tool '{toolCall.FunctionName}'"
                    })
            };
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.GetType().Name,
                message = ex.Message
            });
        }
    }

    private string GetGasPrice(BinaryData arguments)
    {
        var args =
            JsonSerializer.Deserialize<DateArguments>(
                arguments.ToString())!;

        var date = DateOnly.Parse(args.date);

        var price = _marketService.GetGasPrice(date);

        return JsonSerializer.Serialize(new
        {
            success = true,
            commodity = "gas",
            date,
            price
        });
    }

    private string GetPowerPrice(BinaryData arguments)
    {
        var args =
            JsonSerializer.Deserialize<DateArguments>(
                arguments.ToString())!;

        var date = DateOnly.Parse(args.date);

        var price = _marketService.GetPowerPrice(date);

        return JsonSerializer.Serialize(new
        {
            success = true,
            commodity = "power",
            date,
            price
        });
    }

    private string CalculatePercentageChange(
        BinaryData arguments)
    {
        var args =
            JsonSerializer.Deserialize<PercentageArguments>(
                arguments.ToString())!;

        var result =
            _marketService.CalculatePercentageChange(
                args.OldValue,
                args.NewValue);

        return JsonSerializer.Serialize(new
        {
            success = true,
            percentageChange = result
        });
    }

    private sealed record DateArguments(string date);

    private sealed record PercentageArguments(
        decimal OldValue,
        decimal NewValue);
}
