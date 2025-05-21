using GraphQL.Types;
using GatewayService.Models; // Assuming ExampleDataDto is in Models namespace

namespace GatewayService.GraphQL.Types
{
    // Placeholder DTO, actual DTOs would come from shared libraries or be defined per service
    public class ExampleDataDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Value { get; set; }
    }

    public class ExampleServiceDataType : ObjectGraphType<ExampleDataDto>
    {
        public ExampleServiceDataType()
        {
            Name = "ExampleServiceData";
            Description = "Represents example data retrieved from a downstream service.";

            Field(x => x.Id, type: typeof(IdGraphType)).Description("The unique identifier of the data.");
            Field(x => x.Name).Description("The name of the data item.");
            Field(x => x.Description, nullable: true).Description("A description of the data item.");
            Field(x => x.CreatedDate, type: typeof(DateTimeGraphType)).Description("The date when the data item was created.");
            Field(x => x.IsActive, type: typeof(BooleanGraphType)).Description("Indicates if the data item is active.");
            Field(x => x.Value, type: typeof(IntGraphType)).Description("An example integer value associated with the data.");
        }
    }

    // Additional DTOs and Types based on graphql.schema.graphql example
    public class ManagementClientStatusDto
    {
        public string ClientId { get; set; }
        public string Status { get; set; }
        public DateTime? LastSeen { get; set; }
        public List<KpiDataDto> Kpis { get; set; } = new List<KpiDataDto>();
    }

    public class KpiDataDto
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ManagementClientStatusType : ObjectGraphType<ManagementClientStatusDto>
    {
        public ManagementClientStatusType()
        {
            Name = "ManagementClientStatus";
            Field(x => x.ClientId, type: typeof(IdGraphType)).Description("The client's unique identifier.");
            Field(x => x.Status).Description("The current status of the client.");
            Field(x => x.LastSeen, type: typeof(DateTimeGraphType), nullable: true).Description("Timestamp of when the client was last seen.");
            Field<ListGraphType<KpiDataType>>("kpis")
                .Description("Key Performance Indicators for the client.")
                .Resolve(context => context.Source.Kpis);
        }
    }

    public class KpiDataType : ObjectGraphType<KpiDataDto>
    {
        public KpiDataType()
        {
            Name = "KpiData";
            Field(x => x.Name).Description("Name of the KPI.");
            Field(x => x.Value, type: typeof(FloatGraphType)).Description("Value of the KPI.");
            Field(x => x.Timestamp, type: typeof(DateTimeGraphType)).Description("Timestamp of the KPI data point.");
        }
    }

    public class InputDataDto
    {
        public string Feature1 { get; set; }
        public double Feature2 { get; set; }
    }
    public class InputDataInputType : InputObjectGraphType<InputDataDto> // Note: InputObjectGraphType for input types
    {
        public InputDataInputType()
        {
            Name = "InputDataInput";
            Field(x => x.Feature1).Description("First feature for prediction.");
            Field(x => x.Feature2, type: typeof(FloatGraphType)).Description("Second feature for prediction.");
        }
    }

    public class PredictionResultDto
    {
        public string PredictionId { get; set; }
        public double Score { get; set; }
        public string Details { get; set; }
    }

    public class PredictionResultType : ObjectGraphType<PredictionResultDto>
    {
         public PredictionResultType()
         {
            Name = "PredictionResult";
            Field(x => x.PredictionId, type: typeof(IdGraphType)).Description("Unique ID for the prediction.");
            Field(x => x.Score, type: typeof(FloatGraphType)).Description("The prediction score.");
            Field(x => x.Details, nullable: true).Description("Additional details about the prediction.");
         }
    }
}