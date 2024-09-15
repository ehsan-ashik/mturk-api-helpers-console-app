using Amazon.MTurk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTurkAPIHelpers.Constants;
using Amazon.MTurk.Model;
using MTurkAPIHelpers.Models;
using ClosedXML.Excel;
using System.IO;
using System.Windows.Forms;

namespace MTurkAPIHelpers
{
    public static class AwsMturkHelper
    {
        /// <summary>
        /// private method to get the MTurk configurations
        /// </summary>
        /// <param name="serviceUrl">SANDBOX or PROD Mturk service URL</param>
        /// <returns>the Config object</returns>
        private static AmazonMTurkConfig GetAmazonMTurkConfig(string serviceUrl)
        {
            AmazonMTurkConfig config = new AmazonMTurkConfig();
            config.ServiceURL = serviceUrl;

            return config;
        }

        /// <summary>
        /// Get the Production client. Make sure that The Constants.Config file is updated with AWS_ACCESS_KEY and ACCESS_KEY_SECRET
        /// </summary>
        /// <returns>Initiates and returns the MTurk Client Object to run MTurk Quesries</returns>
        public static AmazonMTurkClient GetAmazonMTurkClient()
        {
            AmazonMTurkConfig config = GetAmazonMTurkConfig(MTurkAPIHelpers.Constants.Path.PROD_URL);

            AmazonMTurkClient mturkClient = new AmazonMTurkClient(
                Config.AWS_ACCESS_KEY_ID,
                Config.AWS_SECRET_ACCESS_KEY,
                config);

            return mturkClient;
        }

        /// <summary>
        /// Get the Sandbox client. Make sure that The Constants.Config file is updated with AWS_ACCESS_KEY and ACCESS_KEY_SECRET
        /// </summary>
        /// <returns>Initiates and returns the MTurk Client Object to run MTurk Quesries</returns>
        public static AmazonMTurkClient GetAmazonMTurkClient_Sandbox()
        {
            AmazonMTurkConfig config = GetAmazonMTurkConfig(MTurkAPIHelpers.Constants.Path.SANDBOX_URL);

            AmazonMTurkClient mturkClient = new AmazonMTurkClient(
                Config.AWS_ACCESS_KEY_ID,
                Config.AWS_SECRET_ACCESS_KEY,
                config);

            return mturkClient;
        }

        /// <summary>
        /// List all the HITS 
        /// </summary>
        /// <param name="mturkClient">mturk client associated with the environemnt</param>
        /// <returns>ListHITResponse contains the HITS and HIT details</returns>
        public static ListHITsResponse ListAllHITs(AmazonMTurkClient mturkClient)
        {
            ListHITsRequest listHITsRequest = new ListHITsRequest();

            return mturkClient.ListHITs(listHITsRequest);
        }

        /// <summary>
        /// Assigns qualification type to the Workers
        /// </summary>
        /// <param name="mturkClient">mturk client associated with the environemnt</param>
        /// <param name="workerIds">List of workers who will be assigned the QualificationType</param>
        /// <param name="qualificationTypeName">The name of the QualificationType to be assigned</param>
        /// <param name="qualificationScore">The Qualification Score to be assigned</param>
        /// <returns>True if the assignment operation went succesfull, False otherwise</returns>
        public static bool AssignQualificationTypeToWorkers(AmazonMTurkClient mturkClient, List<string> workerIds, string qualificationTypeName, int qualificationScore)
        {
            if (workerIds == null)
            {
                workerIds = new List<string>();
            }

            var qtype = GetQualificationType(mturkClient, qualificationTypeName);

            if (qtype == null)
            {
                return false;
            }

            var qtypeId = qtype?.QualificationTypeId;

            int total = 0;
            try
            {
                foreach (var item in workerIds)
                {
                    AssociateQualificationWithWorkerRequest associateQualificationWithWorkerRequest = new AssociateQualificationWithWorkerRequest()
                    {
                        IntegerValue = qualificationScore,
                        WorkerId = item,
                        QualificationTypeId = qtypeId,
                        SendNotification = false
                    };
                    var response = mturkClient.AssociateQualificationWithWorker(associateQualificationWithWorkerRequest);
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Console.WriteLine(qualificationTypeName + " assigned to workerId: " + item + " with value " + qualificationScore);
                        total++;
                    }
                    else
                    {
                        Console.WriteLine(qualificationTypeName + " cannot be assigned to workerId: " + item + " with value " + qualificationScore);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            Console.WriteLine("Total assigned: " + total);
            return true;
        }

        /// <summary>
        /// Removes QualificationType from all workers in the HIT
        /// </summary>
        /// <param name="mturkClient">mturk client associated with the environemnt</param>
        /// <param name="hitId">HitId for to pick all workers of the HIT to remove the QualificationType</param>
        /// <param name="qualificationTypeName">Name of the QualificationType</param>
        /// <returns>True if the removal is succesful, False otherwise</returns>
        public static bool RemoveQualificationTypeFromWorkers(AmazonMTurkClient mturkClient, string hitId, string qualificationTypeName)
        {
            var workerIds = GetWorkerIdsForHit(mturkClient, hitId);

            var qtype = GetQualificationType(mturkClient, qualificationTypeName);

            if (qtype == null)
            {
                return false;
            }

            var qtypeId = qtype?.QualificationTypeId;

            int total = 0;
            try
            {
                foreach (var item in workerIds)
                {
                    DisassociateQualificationFromWorkerRequest disassociateQualificationWithWorkerRequest = new DisassociateQualificationFromWorkerRequest()
                    {
                        WorkerId = item,
                        QualificationTypeId = qtypeId,
                        Reason = "Assigned by mistake"
                    };
                    var response = mturkClient.DisassociateQualificationFromWorker(disassociateQualificationWithWorkerRequest);
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Console.WriteLine(qualificationTypeName + " disassociated from workerId: " + item);
                        total++;
                    }
                    else
                    {
                        Console.WriteLine(qualificationTypeName + " cannot be disassociated from workerId: " + item);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            Console.WriteLine("Total Dissasociated: " + total);
            return true;
        }

        /// <summary>
        /// Assigns QualificationType to Workers from a specific HIT
        /// </summary>
        /// <param name="mturkClient">mturk client associated with the environemnt</param>
        /// <param name="hitId">HitId of the HIT to get the Hit workers</param>
        /// <param name="qualificationTypeName">The name of the QualificationType</param>
        /// <param name="qualificationScore">The Score to assign with the QualificationType</param>
        /// <returns>True if the assignment job is successful, False otherwise</returns>
        public static bool AssignQualificationTypeToWorkers(AmazonMTurkClient mturkClient, string hitId, string qualificationTypeName, int qualificationScore)
        {
            var workerIds = GetWorkerIdsForHit(mturkClient, hitId);

            var qtype = GetQualificationType(mturkClient, qualificationTypeName);

            if (qtype == null)
            {
                return false;
            }

            var qtypeId = qtype?.QualificationTypeId;

            int total = 0;
            try
            {
                foreach (var item in workerIds)
                {
                    AssociateQualificationWithWorkerRequest associateQualificationWithWorkerRequest = new AssociateQualificationWithWorkerRequest()
                    {
                        IntegerValue = qualificationScore,
                        WorkerId = item,
                        QualificationTypeId = qtypeId,
                        SendNotification = false
                    };
                    var response = mturkClient.AssociateQualificationWithWorker(associateQualificationWithWorkerRequest);
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Console.WriteLine(qualificationTypeName + " assigned to workerId: " + item + " with value " + qualificationScore);
                        total++;
                    }
                    else
                    {
                        Console.WriteLine(qualificationTypeName + " cannot be assigned to workerId: " + item + " with value " + qualificationScore);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            Console.WriteLine("Total assigned: " + total);
            return true;
        }

        /// <summary>
        /// Get QualificationType object from its Name
        /// </summary>
        /// <param name="mturkClient">mturk client associated with the environemnt</param>
        /// <param name="qualificationTypeName">The name of the QualificationType</param>
        /// <returns>The QualificationType object</returns>
        public static QualificationType GetQualificationType(AmazonMTurkClient mturkClient, string qualificationTypeName)
        {
            ListQualificationTypesRequest listQualificationTypesRequest = new ListQualificationTypesRequest()
            {
                MustBeOwnedByCaller = true,
                MustBeRequestable = true,
                Query = qualificationTypeName
            };

            var qtype = mturkClient.ListQualificationTypes(listQualificationTypesRequest).QualificationTypes.Where(x => x.Name.Equals(qualificationTypeName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            return qtype;
        }

        /// <summary>
        /// Get all the Workers from a HIT
        /// </summary>
        /// <param name="mturkClient">mturk client associated with the environemnt</param>
        /// <param name="hitId">HitId of the workers' completed HIT</param>
        /// <returns>List of the WorkerIds who completed the HIT</returns>
        public static List<string> GetWorkerIdsForHit(AmazonMTurkClient mturkClient, string hitId)
        {

            ListAssignmentsForHITRequest listAssignmentsForHITRequest = new ListAssignmentsForHITRequest()
            {
                HITId = hitId,
                MaxResults = 100
            };

            ListAssignmentsForHITResponse listAssignmentsForHITResponse = mturkClient.ListAssignmentsForHIT(listAssignmentsForHITRequest);

            var workerIDs = listAssignmentsForHITResponse.Assignments.Select(x => x.WorkerId)?.ToList();

            return workerIDs ?? new List<string>();
        }

        /// <summary>
        /// Get the QualificationTypeId from the Name
        /// </summary>
        /// <param name="mturkClient">mturk client associated with the environemnt</param>
        /// <param name="qualificationTypeName">The name of the QualificationType</param>
        /// <returns></returns>
        public static string GetQualificationTypeId(AmazonMTurkClient mturkClient, string qualificationTypeName)
        {
            var qtype = GetQualificationType(mturkClient, qualificationTypeName);

            return qtype?.QualificationTypeId;
        }

        /// <summary>
        /// Generate a list of all Workers assigned to a QualficationType and save the result in a excel file in the current directory
        /// </summary>
        /// <param name="mturkClient">mturk client associated with the environemnt</param>
        /// <param name="qualificationTypeName">The name of the QualificationType</param>
        public static void GenerateBatchDataForWorkersWithQualificationTypeExcel(AmazonMTurkClient mturkClient, string qualificationTypeName)
        {
            string qualificationTypeId = GetQualificationTypeId(mturkClient, qualificationTypeName);
            if (qualificationTypeId == null)
            {
                return;
            }

            ListWorkersWithQualificationTypeRequest request = new ListWorkersWithQualificationTypeRequest()
            {
                QualificationTypeId = qualificationTypeId,
                Status = QualificationStatus.Granted,
                MaxResults = 100
            };

            int total = 0;

            ListWorkersWithQualificationTypeResponse response = mturkClient.ListWorkersWithQualificationType(request);


            var qualData = new List<Qualification>();
            qualData.AddRange(response.Qualifications);
            total += response.NumResults;

            while (response.NextToken != null)
            {
                request = new ListWorkersWithQualificationTypeRequest()
                {
                    QualificationTypeId = qualificationTypeId,
                    Status = QualificationStatus.Granted,
                    MaxResults = 100,
                    NextToken = response.NextToken
                };

                response = mturkClient.ListWorkersWithQualificationType(request);

                qualData.AddRange(response.Qualifications);
                total += response.NumResults;
            }

            qualData = qualData.OrderBy(x => x.IntegerValue).ToList();

            List<BatchWorker> batchWorkerData = new List<BatchWorker>();

            foreach (var qitem in qualData)
            {
                var data = new BatchWorker
                {
                    BatchId = qitem.IntegerValue,
                    WorkerId = qitem.WorkerId,
                    AssignmentDate = qitem.GrantTime
                };

                batchWorkerData.Add(data);
            }

            var workbook = new XLWorkbook();
            workbook.AddWorksheet("data");
            var ws = workbook.Worksheet("data");

            int row = 1;
            foreach (var item in batchWorkerData)
            {
                ws.Cell("A" + row.ToString()).Value = item.BatchId.ToString();
                ws.Cell("B" + row.ToString()).Value = item.WorkerId.ToString();
                ws.Cell("C" + row.ToString()).Value = item.AssignmentDate.ToString();
                row++;
            }

            workbook.SaveAs("excelbatchdata.xlsx");

            Console.WriteLine(total);
        }

        /// <summary>
        /// Send an email to a Worker with a subjectline and message
        /// </summary>
        /// <param name="mturkClient">mturk client associated with the environemnt</param>
        /// <param name="workerid">WorkerId of the worker</param>
        /// <param name="subject">Subject line of the email</param>
        /// <param name="message">The body of the email</param>
        /// <returns>Notify Worker response from the job when complete</returns>
        /// <exception cref="InvalidDataException">Throws a exception if the NotifyWorker job fails</exception>
        private static NotifyWorkersResponse SendMessageToWorker(AmazonMTurkClient mturkClient, string workerid, string subject, string message)
        {
            if (subject.Length > 200 || message.Length > 4096)
            {
                throw new InvalidDataException("subject or message exceeds the acceptable length range; subject: " + subject.Length + ", messaage: " + message.Length);
            }

            NotifyWorkersRequest request = new NotifyWorkersRequest()
            {
                Subject = subject,
                MessageText = message,
                WorkerIds = new List<string>() { workerid }
            };

            try
            {
                var response = mturkClient.NotifyWorkers(request);

                return response;

            } catch (Exception ex)
            {
                NotifyWorkersResponse response = new NotifyWorkersResponse()
                {
                    NotifyWorkersFailureStatuses = new List<NotifyWorkersFailureStatus>()
                    {
                        new NotifyWorkersFailureStatus ()
                        {
                            NotifyWorkersFailureCode = "MTurkError",
                            NotifyWorkersFailureMessage = "NotifyWorker() threw error. Message:" + ex.InnerException.Message
                        }
                    }

                };

                return response;
            }
        }

        /// <summary>
        /// Send email to all the listed Workers alongside generate log data of the email sending job.
        /// </summary>
        /// <param name="mturkClient">mturk client associated with the environemnt</param>
        /// <param name="logWorkerID">The Sandbox Worker associated with the requester's account and email. Initialize the Worker Sandbox in the MTurk portal to generate this WorkerID</param>
        /// <param name="study">Study details to be included in the Log message</param>
        /// <param name="workerIds">Workers Ids who should receive the email</param>
        /// <param name="subject">Subject line of the email</param>
        /// <param name="message">The email body</param>
        public static void SendMessageToWorkers(AmazonMTurkClient mturkClient, string logWorkerID, string study, List<string> workerIds, string subject, string message)
        {
            //Initialize
            string emailLogSubject = DateTime.Now + " - EMAIL SENT FOR STUDY: " + study;

            string emailLogMessage = "DATE: " + DateTime.Now + "\n\n" +
                                        "SUBJECT: " + subject + "\n\n" +
                                        "MESSAGE:\n" + message + "\n\n" +
                                        "LOG: EMAIL SENDING INITIATED FOR " + workerIds.Count + " WORKERS. DETAILS BELOW.\n";


            //send message start
            int counter = 0;
            StringBuilder emailLogBuilder = new StringBuilder();

            List<string> builders = new List<string>();


            //send message 
            foreach (string workerid in workerIds)
            {
                NotifyWorkersResponse response = SendMessageToWorker(mturkClient, workerid, subject, message);

                counter++;
                
                if(emailLogBuilder.Length + emailLogMessage.Length >= 3000)
                {
                    builders.Add(emailLogBuilder.ToString());
                    emailLogBuilder.Clear();
                }

                if(response.NotifyWorkersFailureStatuses != null && response.NotifyWorkersFailureStatuses.Count > 0)
                {
                    emailLogBuilder.AppendLine(counter + ". " + workerid.ToUpper() + "\t" + "FAILURE");

                    foreach (var item in response.NotifyWorkersFailureStatuses)
                        emailLogBuilder.AppendLine("\tCODE: " + item.NotifyWorkersFailureCode + "\tREASON: " + item.NotifyWorkersFailureMessage);
                }
                else
                {
                    emailLogBuilder.AppendLine(counter + ". " + workerid.ToUpper() + "\t" + "SUCCESS");
                }
            }


            //Add the last chunk
            builders.Add(emailLogBuilder.ToString());
            emailLogBuilder.Clear();

            //send log email
            SendEmailLogs(logWorkerID, emailLogSubject, emailLogMessage, builders);

        }

        /// <summary>
        /// Private method that send Log email to the Sandbox Worker associated with the requester's account. Initialize the Worker Sandbox in the MTurk portal to generate this WorkerID
        /// </summary>
        /// <param name="logWorkerID">The Sandbox Worker associated with the requester's account. Initialize the Worker Sandbox in the MTurk portal to generate this WorkerID</param>
        /// <param name="emailLogSubject">Subject line of the Log email</param>
        /// <param name="emailLogMessage">Email body of the Log email</param>
        /// <param name="builders">List of Log message builders</param>
        private static void SendEmailLogs(string logWorkerID, string emailLogSubject, string emailLogMessage, List<string> builders)
        {
            AmazonMTurkClient mturkClient = GetAmazonMTurkClient_Sandbox();

            var aggregMessage = "";
            string path = System.IO.Directory.GetParent(System.IO.Path.GetDirectoryName(Application.StartupPath)).ToString() + "\\Logs\\logfile_" + string.Format("{0:yyyy-MM-dd_HH-mm-ss-fff}.log", DateTime.Now);

            foreach (var item in builders)
            {
                aggregMessage = emailLogMessage + item;


                Console.WriteLine(aggregMessage);
                WriteLogToFile(path, aggregMessage);
                try
                {
                    var res = SendMessageToWorker(mturkClient, logWorkerID, emailLogSubject, aggregMessage);
                    aggregMessage = "";
                }
                catch (Exception ex)
                {
                    var res = SendMessageToWorker(mturkClient, logWorkerID, emailLogSubject, aggregMessage.Substring(200));
                    Console.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// Private method: write the log to the file
        /// </summary>
        /// <param name="logFilePath">File path where Log data will be written</param>
        /// <param name="log">log messege to be written</param>
        private static void WriteLogToFile(string logFilePath, string log)
        {
            if (!File.Exists(logFilePath))
            {
                File.Create(logFilePath).Close();
            }
            using (var tw = new StreamWriter(logFilePath, true))
            {
                tw.WriteLine(log);
                tw.WriteLine("\n-----------------------------------------------");
            }
        }

        /// <summary>
        /// Compares Qualifications based on score
        /// </summary>
        /// <param name="a">Qualfication A</param>
        /// <param name="b">Qualification B</param>
        /// <returns>Value based on comparison</returns>
        private static int QualificationComparer (Qualification a, Qualification b)
        {
            if (a.IntegerValue <= b.IntegerValue)
            {
                return -1;
            }
            else if (a.IntegerValue > b.IntegerValue)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
