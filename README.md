# Amazon MTurk Requester API Helpers - .NET Console App
The .NET app includes a curated list of helper functions with ability to perform advanced *Requester* tasks programmatically on **Amazon Mechanical Turk** crowdsourcing platform. The app leverages AWS MTurk SDKs toward performing the requester jobs that typically cannot be done from the MTurk Web Requester Portal.

## Features
1. Includes a curated list of helper functions that perform advanced *Requester* taks (e.g., emailing *Workers*) on MTurk.
2. Supports generating excel worksheets with Worker data based on *QualificationTypes*.
3. Supports *Logging mechanism* for sensitive tasks such as sending emails to workers along wth functionality to forward email logs to the *Requester*.

## List of Available MTurk API Helpers

* `ListAllHITs` - List all the HITs associated with the Requester Account
* `GetWorkerIdsForHit` - Get all the *Workers* who completed a specific HIT
* `GetQualificationType` - Get *QualificationType* object from the provided name
* `GetQualificationTypeId` - Get ID of the *QualificationType* object from the provided name
* `AssignQualificationTypeToWorkers` - Assigns *QualificationType* to *Workers* from a specific HIT or from a list of workers
* `RemoveQualificationTypeFromWorkers` - Removes QualificationType from all workers in the HIT
* `GenerateBatchDataForWorkersWithQualificationTypeExcel` - Generate a list of all *Workers* assigned to a *QualficationType* and save the result in a excel file in the current directory
* `SendMessageToWorker` - Send a email message to a certain *Worker* with a speific *Subject* and *Message*
* `SendMessageToWorkers` - Send email to a lis of *Workers* while generating logs of the email sending job and sending out log email to the *Requester*

## Dependencies
Here are a list of dependencies for the app:
* .NET SDK 8.0 (https://dotnet.microsoft.com/en-us/download)
* AWSSDK.MTurk v3.7.400.20 (https://www.nuget.org/packages/AWSSDK.MTurk)
* ClosedXML v0.102.3 (https://www.nuget.org/packages/ClosedXML)

## Usage (Visual Studio)

1. Clone the repo using the command: ``
2. Open the `MTurkAPIHelpers.sln` with **Visual Studio**
3. Set the `AWS_ACCESS_KEY_ID` and `AWS_ACCESS_KEY_SECRET` (AWS IAM/root user with permission to access Mechanical Turk) properties in the `MTurkAPIHelpers/Constants/Config` file.
4. Build and Run the Solution.

## Usage (VS Code)
Here are a list of steps to run the app locally:

1. Clone the repo using the command: ``
2. Open the repo with code and `cd` into *MTurkAPIHelpers*
3. Set the `AWS_ACCESS_KEY_ID` and `AWS_ACCESS_KEY_SECRET` (AWS IAM/root user with permission to access Mechanical Turk) properties in the `MTurkAPIHelpers/Constants/Config` file.
4. Build the project in the command line with the command: `dotnet build`
5. Run the project with the command: `dotnet run`

## References
1. Tutorial to setup/link AWS account with Mturk - https://docs.aws.amazon.com/AWSMechTurk/latest/AWSMechanicalTurkGettingStartedGuide/SetUp.html
1. Creating IAM user in AWS - https://docs.aws.amazon.com/IAM/latest/UserGuide/id_users_create.html
2. Adding permission to the IAM User - https://docs.aws.amazon.com/IAM/latest/UserGuide/id_users_change-permissions.html