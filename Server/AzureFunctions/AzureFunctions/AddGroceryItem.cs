using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureFunctions
{
    public static class AddGroceryItem
    {
        // TODO:
        // - Convert function to POST only and accept a particular JSON format.
        // - Update SQLDW to use a proper SQL date format
        [FunctionName("AddGroceryItem")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Blob("groceryitems/groceryitem-{rand-guid}.json", FileAccess.Write, Connection = "AzureWebJobsStorage")] CloudBlockBlob groceryData,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function for adding grocery item data to Azure blob storage.");

            string name = req.Query["name"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string jsonResponse = JsonConvert.SerializeObject(new { 
                epoch = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                name = name ?? data?.name
            });

            if((name ?? data?.name) != null)
            {
                await groceryData.UploadTextAsync(jsonResponse);
                return (ActionResult)new OkObjectResult(jsonResponse);
            }
            else
            {
                return new BadRequestObjectResult("Please pass a name on the query string or in the request body");
            }

        }
    }
}
