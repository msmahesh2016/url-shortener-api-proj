using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace UrlCleanupFunction
{
    public class DeleteUrlsFunction
    {
        [FunctionName("DeleteUrlsFunction")]
        public void Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"DeleteUrlsFunction executed at: {DateTime.Now}");

            string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM Urls", conn);
                int rowsAffected = cmd.ExecuteNonQuery();
                log.LogInformation($"Deleted {rowsAffected} URLs.");
            }
        }
    }
}
