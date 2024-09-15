using System;
using Amazon.MTurk;
using Amazon.MTurk.Model;

namespace MTurkAPIHelpers
{
    class Program
    {
        static void Main(string[] args)
        {
            // Set appropriate Config 
            // Get the client. Here, SandBox client has been fetched for testing In production, run AwsMturkHelper.GetAmazonMTurkClient()
            AmazonMTurkClient mturkClient = AwsMturkHelper.GetAmazonMTurkClient_Sandbox();

            // Example usage: List All HITs
            ListHITsResponse hitResponse = AwsMturkHelper.ListAllHITs(mturkClient);
            Console.WriteLine("Total HITs:" + hitResponse.HITs.Count);
            Console.WriteLine(hitResponse.HITs.Count > 0 ? "HIT Description:" + hitResponse.HITs[0].Description : "Please create a HIT to see its description");

            // Example usage: Get QualificationType with the name. Assuming a qualType with name "TEST1" is avaiable.
            string qualTypeName = "TEST1";
            QualificationType qualType = AwsMturkHelper.GetQualificationType(mturkClient, qualTypeName);
            Console.WriteLine(qualType != null ? qualType.Description : $"No QualificationType with name: '{qualTypeName}' avaiable");

            // Wait till a key is pressed before exiting the console
            Console.WriteLine("\n\nPress Any key To Exit...");
            Console.ReadKey();
        }
    }
}
