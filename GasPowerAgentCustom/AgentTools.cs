using OpenAI.Chat;

public static class AgentTools
{
    public static ChatTool GetGasPriceTool =>
        ChatTool.CreateFunctionTool(
            functionName: "get_gas_price",
            functionDescription:
            "Gets the gas price for a particular date.",
            functionParameters: BinaryData.FromString("""
                                                      {
                                                        "type": "object",
                                                        "properties": {
                                                          "date": {
                                                            "type": "string",
                                                            "description": "Date in yyyy-MM-dd format."
                                                          }
                                                        },
                                                        "required": ["date"],
                                                        "additionalProperties": false
                                                      }
                                                      """));

    public static ChatTool GetPowerPriceTool =>
        ChatTool.CreateFunctionTool(
            functionName: "get_power_price",
            functionDescription:
            "Gets the power price for a particular date.",
            functionParameters: BinaryData.FromString("""
                                                      {
                                                        "type": "object",
                                                        "properties": {
                                                          "date": {
                                                            "type": "string",
                                                            "description": "Date in yyyy-MM-dd format."
                                                          }
                                                        },
                                                        "required": ["date"],
                                                        "additionalProperties": false
                                                      }
                                                      """));

    public static ChatTool CalculatePercentageChangeTool =>
        ChatTool.CreateFunctionTool(
            functionName: "calculate_percentage_change",
            functionDescription:
            """
            Calculates the percentage change between an old
            value and a new value.
            """,
            functionParameters: BinaryData.FromString("""
                                                      {
                                                        "type": "object",
                                                        "properties": {
                                                          "oldValue": {
                                                            "type": "number"
                                                          },
                                                          "newValue": {
                                                            "type": "number"
                                                          }
                                                        },
                                                        "required": [
                                                          "oldValue",
                                                          "newValue"
                                                        ],
                                                        "additionalProperties": false
                                                      }
                                                      """));
}