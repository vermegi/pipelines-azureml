using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Company.Function
{
    public static class HttpTriggerCSharp2
    {
        [FunctionName("HttpTriggerCSharp2")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string runId = req.Query["runId"];
            string operation = req.Query["operation"];

            var entityId = new EntityId(nameof(AMLRun), runId);

            if (operation == "get"){
                log.LogInformation("operation get");
                var theAMLRun = client.ReadEntityStateAsync<AMLRun>(entityId).Result;
                log.LogInformation("Status: {0}", theAMLRun.EntityState);
                return new OkObjectResult(theAMLRun.EntityState is null ? false : theAMLRun.EntityState.Status);
            }
            if (operation == "done"){
                log.LogInformation("operation done");
                var result = client.SignalEntityAsync(entityId, "Done");
                return new OkObjectResult(true);
            }

            return new OkObjectResult(false);
        }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class AMLRun
    {
        private bool? _status = false;
        
        [JsonProperty("Status")]
        public bool? Status { get{return _status;} set{_status = value;} }

        public void Done(){
            Status = true;
        }

        [FunctionName(nameof(AMLRun))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<AMLRun>();
        }
}
