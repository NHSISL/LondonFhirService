// ---
// skill: the-standard-csharp-classes
// type: template
// source-section: "4. Classes"
// ---

// File: {EntityName}.cs — Model
namespace {Namespace}
{
    public class {EntityName}
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        // Add properties in the order they are assigned
    }
}

// File: {EntityName}Service.cs — Service (singular)
namespace {Namespace}
{
    public partial class {EntityName}Service : I{EntityName}Service
    {
        private readonly I{EntityName}Broker {entityName}Broker;
        private readonly ILoggingBroker loggingBroker;

        public {EntityName}Service(
            I{EntityName}Broker {entityName}Broker,
            ILoggingBroker loggingBroker)
        {
            this.{entityName}Broker = {entityName}Broker;
            this.loggingBroker = loggingBroker;
        }
    }
}

// File: {EntityName}Broker.cs — Broker (singular)
namespace {Namespace}
{
    public partial class {EntityName}Broker : I{EntityName}Broker
    {
        // Broker constructor and DI
    }
}

// File: {EntityNames}Controller.cs — Controller (plural)
namespace {Namespace}
{
    [ApiController]
    [Route("api/{entityNames}")]
    public class {EntityNames}Controller : RESTFulController
    {
        private readonly I{EntityName}Service {entityName}Service;

        public {EntityNames}Controller(I{EntityName}Service {entityName}Service) =>
            this.{entityName}Service = {entityName}Service;
    }
}
