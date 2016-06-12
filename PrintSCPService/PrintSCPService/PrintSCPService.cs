using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Dicom.Log;
using Dicom.Printing;
using PrintSystem.Common;

namespace PrintSCPService
{
    public partial class PrintSCPService : ServiceBase
    {
        public PrintSCPService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Initialize log manager.
            LogManager.SetImplementation(Log4NetManager.Instance);

            List<Listener> listenerList = ListenHelper.GetListeners();
            foreach (var listenerItem in listenerList)
            {
                
                PrintService.Start(listenerItem.ListenPort, listenerItem.AETitle);

                Console.WriteLine("Stopping print service");              
            }
        }

        protected override void OnStop()
        {
            PrintService.Stop();
        }
    }
}
