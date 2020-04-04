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
using Fathym;
using LCU.Presentation.State.ReqRes;
using LCU.StateAPI.Utilities;
using LCU.StateAPI;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Collections.Generic;
using LCU.Graphs.Registry.Enterprises.IDE;

namespace LCU.State.API.NapkinIDE.NapkinIDE.IdeManagement
{
    [Serializable]
    [DataContract]
    public class IdeManagementState
    {
        #region Constants
        public const string HUB_NAME = "idemanagement";
        #endregion
        
		[DataMember]
		public virtual List<IDEActivity> Activities { get; set; }

		[DataMember]
		public virtual IDEActivity CurrentActivity { get; set; }

		[DataMember]
		public virtual IDEEditor CurrentEditor { get; set; }

		[DataMember]
		public virtual IDEPanel CurrentPanel { get; set; }

		[DataMember]
		public virtual List<IDEEditor> Editors { get; set; }

		[DataMember]
		public virtual bool InfrastructureConfigured { get; set; }

		[DataMember]
		public virtual bool IsActiveSubscriber { get; set; }

		[DataMember]
		public virtual bool Loading { get; set; }

		[DataMember]
		public virtual List<IDEPanel> Panels { get; set; }

		[DataMember]
		public virtual List<IDEActivity> RootActivities { get; set; }

		[DataMember]
		public virtual bool ShowPanels { get; set; }

		[DataMember]
		public virtual IDESideBar SideBar { get; set; }

		[DataMember]
		public virtual List<string> StatusChanges { get; set; }
    }
}
