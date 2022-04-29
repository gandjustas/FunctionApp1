using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Messages;

namespace Company.Function
{
    public static class Function1
    {
        private const string FieldStart = "msdyn_start";
        private const string FieldEnd = "msdyn_end";
        private const string EntityName = "msdyn_timeentry";

        public static IEnumerable<DateTime> GenerateDays(DateTime start, DateTime end, (DateTime, DateTime)[] values)
        {
            if (end < start) throw new ArgumentOutOfRangeException(nameof(start), start, $"{nameof(start)} should be less or equal than {nameof(end)}");
            return Enumerable.Range(0, (end - start).Days + 1)
                             .Select(x => start.AddDays(x))
                             .Where(d => !values.Any(tuple => tuple.Item1 <= d && d <= tuple.Item2));
        }

        private static IConfigurationRoot Config = new ConfigurationBuilder()
                                                        .AddEnvironmentVariables()
                                                        .AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly(),optional: true)
                                                        .Build();

        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {

                string requestBody = await req.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Request>(requestBody);
                if (data.EndOn < data.StartOn) return new BadRequestResult();

                var svc = new ServiceClient(Config.GetConnectionStringOrSetting("Dynamics"));

                var resp = await svc.RetrieveMultipleAsync(new QueryExpression
                {
                    EntityName = EntityName,
                    ColumnSet = new ColumnSet(FieldStart, FieldEnd),
                    Criteria = new FilterExpression(LogicalOperator.And)
                    {
                        Conditions = {
                        new ConditionExpression(FieldStart, ConditionOperator.GreaterEqual, data.StartOn),
                        new ConditionExpression(FieldEnd, ConditionOperator.LessEqual, data.EndOn)
                    }
                    }
                });

                var exitsingDates = resp.Entities.Select(e => (e.GetAttributeValue<DateTime>(FieldStart), e.GetAttributeValue<DateTime>(FieldEnd))).ToArray();

                var datesToCreate = GenerateDays(data.StartOn, data.EndOn, exitsingDates);
                if (datesToCreate.Any())
                {
                    var createRequests = datesToCreate.Select(d => new CreateRequest
                    {
                        Target = new Entity(EntityName)
                        {
                            Attributes = {
                        { FieldStart, d },
                        { FieldEnd, d }
                    }
                        }
                    });

                    var collection = new OrganizationRequestCollection();
                    collection.AddRange(createRequests);
                    var transactionResponse = (ExecuteTransactionResponse)await svc.ExecuteAsync(new ExecuteTransactionRequest()
                    {
                        Requests = collection,
                        ReturnResponses = true
                    });
                }

                return new OkResult();
            }
            catch (Exception e)
            {
                log.LogCritical(e, e.ToString());
                throw;
            }
        }
    }
}
