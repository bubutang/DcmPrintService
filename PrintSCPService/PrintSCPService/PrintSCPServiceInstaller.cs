using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace PrintSCPService
{
    [RunInstaller(true)]
    public partial class PrintSCPServiceInstaller : System.Configuration.Install.Installer
    {
        public PrintSCPServiceInstaller()
        {
            InitializeComponent();
            ServiceProcessInstaller spi = new ServiceProcessInstaller();
            spi.Account = ServiceAccount.LocalSystem;

            ServiceInstaller si = new ServiceInstaller();
            si.ServiceName = "PrintSCPService";
            si.DisplayName = "DICOM打印SCP服务";
            si.Description = "DICOM打印SCP服务";
            si.StartType = ServiceStartMode.Manual;
            this.Installers.Add(si);
            this.Installers.Add(spi);
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
        }
    }
}
