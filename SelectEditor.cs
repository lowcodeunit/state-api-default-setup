using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Runtime.Serialization;
using Fathym;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.WindowsAzure.Storage.Blob;
using LCU.StateAPI.Utilities;
using LCU.Personas.Client.Applications;

namespace LCU.State.API.NapkinIDE.NapkinIDE.IdeManagement
{
	[Serializable]
	[DataContract]
	public class SelectEditorRequest
	{
		[DataMember]
		public virtual string EditorLookup { get; set; }
	}

	public class SelectEditor
    {
        protected ApplicationManagerClient appMgr;

        public SelectEditor(ApplicationManagerClient appMgr)
        {
            this.appMgr = appMgr;
        }

        [FunctionName("SelectEditor")]
        public virtual async Task<Status> Run([HttpTrigger] HttpRequest req, ILogger log,
            [SignalR(HubName = IdeManagementState.HUB_NAME)]IAsyncCollector<SignalRMessage> signalRMessages,
            [Blob("state-api/{headers.lcu-ent-api-key}/{headers.lcu-hub-name}/{headers.x-ms-client-principal-id}/{headers.lcu-state-key}", FileAccess.ReadWrite)] CloudBlockBlob stateBlob)
        {
            return await stateBlob.WithStateHarness<IdeManagementState, SelectEditorRequest, IdeManagementStateHarness>(req, signalRMessages, log,
                async (harness, reqData, actReq) =>
            {
				log.LogInformation($"Selecting Editor {reqData.EditorLookup}");

                var stateDetails = StateUtils.LoadStateDetails(req);

				await harness.SelectEditor(reqData.EditorLookup);

                return Status.Success;
            });
        }
    }
}
