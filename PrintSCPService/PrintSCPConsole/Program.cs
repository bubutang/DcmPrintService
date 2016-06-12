// Copyright (c) 2012-2015 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

namespace Print_SCP
{
    using System;
    using System.Collections.Generic;

    using Dicom;
    using Dicom.Imaging;
    using Dicom.Log;
    using Dicom.Printing;
    using PrintSystem.Common;

    internal class Program
    {
        private static void Main(string[] args)
        {
            // Initialize log manager.
            LogManager.SetImplementation(Log4NetManager.Instance);

            List<Listener> listenerList   =    ListenHelper.GetListeners();
            foreach (var listenerItem in listenerList)
            {
                PrintService.Start(listenerItem.ListenPort, listenerItem.AETitle);

                Console.WriteLine("Press any key to stop the service");

                Console.Read();

                Console.WriteLine("Stopping print service");

                PrintService.Stop();
            }
        }
    }
}
