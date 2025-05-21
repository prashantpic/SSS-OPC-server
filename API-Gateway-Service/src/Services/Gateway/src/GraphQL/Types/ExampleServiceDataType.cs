using GraphQL.Types;
using GatewayService.Models; // Assuming ExampleServiceData is in Models

namespace GatewayService.GraphQL.Types
{
    /// <summary>
    /// Represents an example data DTO in the GraphQL schema.
    /// This maps to the ExampleServiceData class.
    /// </summary>
    public class ExampleServiceDataType : ObjectGraphType<ExampleServiceData>
    {
        public ExampleServiceDataType()
        {
            Name = "ExampleServiceData";
            Description = "Represents data retrieved from an example downstream service.";

            Field(x => x.Id, type: typeof(IdGraphType)).Description("The unique identifier of the data.");
            Field(x => x.Name).Description("The name associated with the data.");
            Field(x => x.Value, type: typeof(FloatGraphType)).Description("A numerical value associated with the data.");
            Field(x => x.Timestamp, type: typeof(DateTimeGraphType)).Description("The timestamp of when the data was recorded or generated.");
            Field<ListGraphType<StringGraphType>>(
                name: "tags",
                description: "A list of tags associated with the data.",
                resolve: context => context.Source.Tags
            );
        }
    }
}