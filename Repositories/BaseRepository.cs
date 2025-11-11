using Marten;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Tenup.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly IDocumentStore _store;
        protected readonly TelemetryClient _telemetryClient;

        protected BaseRepository(IDocumentStore store, TelemetryClient telemetryClient)
        {
            _store = store;
            _telemetryClient = telemetryClient;
        }

        protected void LogOperation(string operationName, Dictionary<string, string>? properties = null)
        {
            var telemetry = new EventTelemetry(operationName);
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    telemetry.Properties[prop.Key] = prop.Value;
                }
            }
            _telemetryClient.TrackEvent(telemetry);
        }
    }
}