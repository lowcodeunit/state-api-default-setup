using System;
using System.IO;
using System.Linq;
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
using LCU.Personas.Client.Enterprises;
using LCU.Personas.Client.DevOps;
using LCU.Personas.Enterprises;
using LCU.Personas.Client.Applications;
using LCU.Personas.Client.Identity;
using Fathym.API;
using LCU.Graphs.Registry.Enterprises.IDE;

namespace LCU.State.API.NapkinIDE.NapkinIDE.IdeManagement
{
    public class IdeManagementStateHarness : LCUStateHarness<IdeManagementState>
    {
        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public IdeManagementStateHarness(IdeManagementState state)
            : base(state ?? new IdeManagementState())
        { }
        #endregion

        #region API Methods
        public virtual async Task Ensure(ApplicationManagerClient appMgr, IdentityManagerClient idMgr, string entApiKey, string username)
        {

            // check in to see if user has free trial/paid subscriber rights    
            var authResp = await idMgr.HasAccess(entApiKey, username, new List<string>() { "LCU.NapkinIDE.ActiveSubscriber" });

            State.IsActiveSubscriber = authResp.Status;

            if (State.IsActiveSubscriber)
            {

                var activitiesResp = await appMgr.LoadIDEActivities(entApiKey);

                State.Activities = activitiesResp.Model;

                var appsResp = await appMgr.ListApplications(entApiKey);

                State.InfrastructureConfigured = activitiesResp.Status && !activitiesResp.Model.IsNullOrEmpty() && appsResp.Status && !appsResp.Model.IsNullOrEmpty();

                State.RootActivities = new List<IDEActivity>();

                State.RootActivities.Add(new IDEActivity()
                {
                    Icon = "settings",
                    Lookup = Environment.GetEnvironmentVariable("FORGE-SETTINGS-PATH") ?? "/forge-settings",
                    Title = "Settings"
                });

            }
            else
            {

                State.RootActivities = new List<IDEActivity>();

                State.Activities = new List<IDEActivity>();

                State.Activities.Add(new IDEActivity()
                {
                    Icon = "redeem",
                    Lookup = "limited-trial",
                    Title = "Limited Trial"
                });
            }

            await LoadSideBar(appMgr, entApiKey);

        }

        public virtual async Task LoadSideBar(ApplicationManagerClient appMgr, string entApiKey)
        {
            if (State.SideBar == null)
                State.SideBar = new IDESideBar();

            if (State.CurrentActivity != null)
            {
                if (State.IsActiveSubscriber)
                {
                    var sectionsResp = await appMgr.LoadIDESideBarSections(entApiKey, State.CurrentActivity.Lookup);

                    State.SideBar.Actions = sectionsResp.Model.SelectMany(section =>
                    {
                        var actionsResp = appMgr.LoadIDESideBarActions(entApiKey, State.CurrentActivity.Lookup, section).Result;

                        return actionsResp.Model;
                    }).ToList();

                } else {
                    State.SideBar.Actions = new List<IDESideBarAction>();

                    State.SideBar.Actions.Add(new IDESideBarAction() 
                    {
                        Action = "welcome",
                        Group = "lcu-limited-trial",
                        Section = "Limited Low-Code Unit™ Trials",
                        Title = "Welcome"
                    });

                    State.SideBar.Actions.Add(new IDESideBarAction() 
                    {
                        Action = "data-flow",
                        Group = "lcu-limited-trial",
                        Section = "Limited Low-Code Unit™ Trials",
                        Title = "Data Flow"
                    });

                    State.SideBar.Actions.Add(new IDESideBarAction() 
                    {
                        Action = "data-apps",
                        Group = "lcu-limited-trial",
                        Section = "Limited Low-Code Unit™ Trials",
                        Title = "Data Applications"
                    });
                }
            }
            else
                State.SideBar = new IDESideBar();
        }

        public virtual async Task RemoveEditor(string editorLookup)
        {
            State.Editors = State.Editors.Where(e => e.Lookup != editorLookup).ToList();

            State.CurrentEditor = State.Editors.FirstOrDefault();

            State.SideBar.CurrentAction = State.SideBar.Actions.FirstOrDefault(a => $"{a.Group}|{a.Action}" == State.CurrentEditor?.Lookup);
        }

        public virtual async Task SelectEditor(string editorLookup)
        {
            State.SideBar.CurrentAction = State.SideBar.Actions.FirstOrDefault(a => $"{a.Group}|{a.Action}" == editorLookup);

            State.CurrentEditor = State.Editors.FirstOrDefault(a => a.Lookup == editorLookup);
        }

        public virtual async Task SelectSideBarAction(ApplicationManagerClient appMgr, string entApiKey, string host, string group, string action, string section)
        {
            State.SideBar.CurrentAction = State.SideBar.Actions.FirstOrDefault(a => a.Group == group && a.Action == action);

            if (State.Editors.IsNullOrEmpty())
                State.Editors = new List<IDEEditor>();

            var actionLookup = $"{group}|{action}";

            if (!State.Editors.Select(e => e.Lookup).Contains(actionLookup))
            {
                var ideEditorResp = await appMgr.LoadIDEEditor(entApiKey, group, action, section, host, State.CurrentActivity.Lookup);

                if (ideEditorResp.Status)
                    State.Editors.Add(ideEditorResp.Model);
            }

            State.CurrentEditor = State.Editors.FirstOrDefault(a => a.Lookup == actionLookup);
        }

        public virtual async Task SetActivity(ApplicationManagerClient appMgr, string entApiKey, string activityLookup)
        {
            State.CurrentActivity = State.Activities.FirstOrDefault(a => a.Lookup == activityLookup);

            await LoadSideBar(appMgr, entApiKey);

            State.SideBar.CurrentAction = State.SideBar.Actions.FirstOrDefault(a => $"{a.Group}|{a.Action}" == State.CurrentEditor?.Lookup);
        }

        public virtual async Task ToggleShowPanels(string group, string action)
        {
            State.ShowPanels = !State.ShowPanels;
        }
        #endregion
    }
}
