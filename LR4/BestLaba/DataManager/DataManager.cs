using ServiceLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Laba4
{
    public partial class DataManager : ServiceBase
    {
        readonly DataIO appInsights;

        readonly DataOptions dataOptions;

        public DataManager(DataOptions dataOptions, DataIO appInsights)
        {
            InitializeComponent();

            this.dataOptions = dataOptions;

            this.appInsights = appInsights;
        }

        protected override void OnStart(string[] args)
        {
            DataIO reader = new DataIO(dataOptions.ConnectionString);

            FileTransfer fileTransfer = new FileTransfer(dataOptions.TargetFolder, dataOptions.SourcePath);

            string customersFileName = "person";

            reader.GetCustomers(dataOptions.TargetFolder, appInsights, customersFileName);

            fileTransfer.SendFileToFtp($"{customersFileName}.xml");
            fileTransfer.SendFileToFtp($"{customersFileName}.xsd");

            appInsights.InsertInsight("Files were sent to FTP successfully");
        }

        protected override void OnStop()
        {
            appInsights.InsertInsight("Service was successfully stopped");

            //appInsights.WriteInsightsToXml(dataOptions.TargetFolder);
        }
    }
}

