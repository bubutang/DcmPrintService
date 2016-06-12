// Copyright (c) 2012-2015 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

using System;
using System.Collections.Generic;
using System.Drawing.Printing;
//using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing;
using Dicom.Imaging;
using PrintSystem.Common;

namespace Dicom.Printing
{
    public class StatusUpdateEventArgs : EventArgs
    {
        public ushort EventTypeId { get; private set; }
        public string ExecutionStatusInfo { get; private set; }
        public string FilmSessionLabel { get; private set; }
        public string PrinterName { get; private set; }

        public StatusUpdateEventArgs(
            ushort eventTypeId,
            string executionStatusInfo,
            string filmSessionLabel,
            string printerName)
        {
            EventTypeId = eventTypeId;
            ExecutionStatusInfo = executionStatusInfo;
            FilmSessionLabel = filmSessionLabel;
            PrinterName = printerName;
        }
    }

    public enum PrintJobStatus : ushort
    {
        Pending = 1,

        Printing = 2,

        Done = 3,

        Failure = 4
    }


    public class PrintJob : DicomDataset
    {
        #region Properties and Attributes

        public bool SendNEventReport { get; set; }

        private readonly object _synchRoot = new object();

        public Guid PrintJobGuid { get; private set; }

        public IList<string> FilmBoxFolderList { get; private set; }

        public Printer Printer { get; private set; }

        public PrintJobStatus Status { get; private set; }

        public string PrintJobFolder { get; private set; }

        public string FullPrintJobFolder { get; private set; }

        public Exception Error { get; private set; }

        public string FilmSessionLabel { get; private set; }

        private int _currentPage;

        private FilmBox _currentFilmBox;

        private static AsynchronousClient _asynchronousClient;

        /// <summary>
        /// Print job SOP class UID
        /// </summary>
        public readonly DicomUID SOPClassUID = DicomUID.PrintJobSOPClass;

        /// <summary>
        /// Print job SOP instance UID
        /// </summary>
        public DicomUID SOPInstanceUID { get; private set; }

        /// <summary>
        /// Execution status of print job.
        /// </summary>
        /// <remarks>
        /// Enumerated Values:
        /// <list type="bullet">
        /// <item><description>PENDING</description></item>
        /// <item><description>PRINTING</description></item>
        /// <item><description>DONE</description></item>
        /// <item><description>FAILURE</description></item>
        /// </list>
        /// </remarks> 
        public string ExecutionStatus
        {
            get
            {
                return Get(DicomTag.ExecutionStatus, string.Empty);
            }
            set
            {
                Add(DicomTag.ExecutionStatus, value);
            }
        }

        /// <summary>
        /// Additional information about Execution Status (2100,0020).
        /// </summary>
        public string ExecutionStatusInfo
        {
            get
            {
                return Get(DicomTag.ExecutionStatusInfo, string.Empty);
            }
            set
            {
                Add(DicomTag.ExecutionStatusInfo, value);
            }
        }

        /// <summary>
        /// Specifies the priority of the print job.
        /// </summary>
        /// <remarks>
        /// Enumerated values:
        /// <list type="bullet">
        ///     <item><description>HIGH</description></item>
        ///     <item><description>MED</description></item>
        ///     <item><description>LOW</description></item>
        /// </list>
        /// </remarks>
        public string PrintPriority
        {
            get
            {
                return Get(DicomTag.PrintPriority, "MED");
            }
            set
            {
                Add(DicomTag.PrintPriority, value);
            }
        }

        /// <summary>
        /// Date/Time of print job creation.
        /// </summary>
        public DateTime CreationDateTime
        {
            get
            {
                return this.GetDateTime(DicomTag.CreationDate, DicomTag.CreationTime);
            }
            set
            {
                Add(DicomTag.CreationDate, value);
                Add(DicomTag.CreationTime, value);
            }
        }

        /// <summary>
        /// User defined name identifying the printer.
        /// </summary>
        public string PrinterName
        {
            get
            {
                return Get(DicomTag.PrinterName, string.Empty);
            }
            set
            {
                Add(DicomTag.PrinterName, value);
            }
        }

        /// <summary>
        /// DICOM Application Entity Title that issued the print operation.
        /// </summary>
        public string CallingAETitle
        {
            get
            {
                return Get(DicomTag.Originator, string.Empty);
            }
            set
            {
                Add(DicomTag.Originator, value);
            }
        }

        public string CalledAETitle { get;set;}
        public IPAddress CallingIPAddress { get; set; }

        public Dicom.Log.Logger Log { get; private set; }

        public PrintTag printTag { get; set; }

        //public List<string> ImageList { get; set; }

        public event EventHandler<StatusUpdateEventArgs> StatusUpdate;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct new print job using specified SOP instance UID. If passed SOP instance UID is missing, new UID will
        /// be generated
        /// </summary>
        /// <param name="sopInstance">New print job SOP instance uID</param>
        public PrintJob(DicomUID sopInstance, Printer printer, IPAddress callingIPAddress, string callingAETitle, string calledAETitle, Dicom.Log.Logger log)
            : base()
        {
            if (printer == null)
            {
                throw new ArgumentNullException("printer");
            }

            Log = log;

            if (sopInstance == null || sopInstance.UID == string.Empty)
            {
                SOPInstanceUID = DicomUID.Generate();
            }
            else
            {
                SOPInstanceUID = sopInstance;
            }

            this.Add(DicomTag.SOPClassUID, SOPClassUID);
            this.Add(DicomTag.SOPInstanceUID, SOPInstanceUID);

            Printer = printer;

            Status = PrintJobStatus.Pending;

            PrinterName = Printer.PrinterAet;

            CallingAETitle = callingAETitle;
            CalledAETitle = calledAETitle;
            CallingIPAddress = callingIPAddress;

            if (CreationDateTime == DateTime.MinValue)
            {
                CreationDateTime = DateTime.Now;
            }

            PrintJobFolder = SOPInstanceUID.UID;



            //imageFolder should read from ini file
            var iniPath = string.Format(@"{0}\{1}", Environment.CurrentDirectory, @"PrintSCPConfiguration.ini");
            string imageFolder = IniFile.ReadIniData("PrintSCP", "ImageFolder", "PrintImages", iniPath);
            if (imageFolder == String.Empty)
            {
                imageFolder = "PrintImages";
            }

            //Get Current Date
            string dateFolder = DateTime.Now.ToString("yyyyMMdd");

            var receivingFolder = string.Format(@"{0}\{1}", imageFolder, dateFolder);
            //FullPrintJobFolder shoud pad with AutoID, Which is get from DataBase
            // Can We use This SOPInstanceUID As AutoID??
            FullPrintJobFolder = string.Format(@"{0}\{1}", receivingFolder.TrimEnd('\\'), PrintJobFolder);

            FilmBoxFolderList = new List<string>();

            _asynchronousClient = new AsynchronousClient(log);
        }

        #endregion

        #region Printing Methods

        public void Print(IList<FilmBox> filmBoxList)
        {
            try
            {
                Status = PrintJobStatus.Pending;

                OnStatusUpdate("Preparing films for printing");

                var thread = new Thread(new ParameterizedThreadStart(DoPrint));
                thread.Name = string.Format("PrintJob {0}", SOPInstanceUID.UID);
                thread.IsBackground = true;
                thread.Start(filmBoxList);

                FilmSessionLabel = filmBoxList.First().FilmSession.FilmSessionLabel;
            }
            catch (Exception ex)
            {
                Error = ex;
                Status = PrintJobStatus.Failure;
                OnStatusUpdate("Print failed");
                DeletePrintFolder();
            }
        }

        private void DoPrint(object arg)
        {
            try
            {
                Status = PrintJobStatus.Printing;
                OnStatusUpdate("Printing Started");

                //PrintThreadParameter printThreadParametr = (PrintThreadParameter)arg;
                IList <FilmBox> filmBoxList   = (IList <FilmBox>)arg;

                var printJobDir = new System.IO.DirectoryInfo(FullPrintJobFolder);
                if (!printJobDir.Exists)
                {
                    printJobDir.Create();
                }

                DicomFile file;
                List<string> autoIDList = new List<string>();
                for (int i = 0; i < filmBoxList.Count; i++)
                {
                    DicomUID autoID = DicomUID.Generate();
                    var filmBox = filmBoxList[i];
                    //One Film Box, One Folder
                    //var filmBoxDir = printJobDir.CreateSubdirectory(string.Format("F{0:000000}", i + 1 ));
                    var filmBoxDir = printJobDir.CreateSubdirectory(autoID.UID);

                    //Film Session Relative DICOM Tag
                    file = new DicomFile(filmBox.FilmSession);
                    file.Save(string.Format(@"{0}\FilmSession.dcm", filmBoxDir.FullName));

                    FilmBoxFolderList.Add(filmBoxDir.FullName);
                    List<string> ImageList = filmBox.SaveImage(filmBoxDir.FullName);


                    #region  PrintTask Content
                    PrintTask printTask = new PrintTask();
                    printTask.FilmUID = autoID.UID;
                    printTask.CallingIPAddress = CallingIPAddress;
                    printTask.CallingAE = CallingAETitle;
                    printTask.CalledAE = CalledAETitle;

                    #region  Get Film Session Level DICOM Tag, Save in PrintTag Structure
                    printTask.NumberOfCopies = filmBox.FilmSession.NumberOfCopies;
                    printTask.MediumType = filmBox.FilmSession.MediumType;
                    #endregion


                    #region  Get Film Box Level DICOM Tag, Save in PrintTag Structure
                    printTask.printUID = SOPInstanceUID.UID;
                    printTask.BorderDensity = filmBox.BorderDensity;
                    printTask.ImageDisplayFormat = filmBox.ImageDisplayFormat;
                    printTask.EmptyImageDensity = filmBox.EmptyImageDensity;
                    printTask.FilmSizeID = filmBox.FilmSizeID;
                    printTask.FilmOrienation = filmBox.FilmOrienation;
                    printTask.MagnificationType = filmBox.MagnificationType;
                    printTask.MaxDensity = filmBox.MaxDensity;
                    printTask.MinDensity = filmBox.MinDensity;
                    printTask.SmoothingType = filmBox.SmoothingType;
                    printTask.Trim = filmBox.Trim;
                    printTask.PresentationLut = filmBox.PresentationLut == null ? string.Empty : filmBox.PresentationLut.ToString();
                    #endregion

                    #endregion
                    int imageCount = ImageList.Count;
                    for (int imageIndex = 0; imageIndex < imageCount; imageIndex++)
                    {
                        #region PrintImage Content
                        PrintImage printImage = new PrintImage();
                        printImage.FilmUID = autoID.UID;
                        printImage.ImageBoxPosition = imageIndex;
                        string imagePath = string.Format(@"{0}\I{1:000000}", filmBoxDir.FullName, imageIndex + 1);
                        printImage.DicomFilePath    = imagePath + ".dcm";
                        //Save into Table T_PrintTag
                        if (printImage.SaveToDB() == false)
                        {
                            Log.Error("Print Job {0} FAIL: {1}", SOPInstanceUID.UID.Split('.').Last(), "Save PrintImage to DB Failed");
                        }
                        #endregion
                    }

                    int index = 0;
                    foreach (var item in filmBox.BasicImageBoxes)
                    {
                        if (item != null && item.ImageSequence != null && item.ImageSequence.Contains(DicomTag.PixelData))
                        {
                            try
                            {
                                var image = new DicomImage(item.ImageSequence);
                                image.RenderImage().AsBitmap().Save(ImageList[index]);
                                index++;
                            }
                            finally
                            {

                            }
                        }
                    }

                    //Parse DICOM File and render into Memory As image
                    var combineJpgFile = string.Format(@"{0}\{1}" + ".jpg", FilmBoxFolderList[i], autoID.UID);
                    int row = 0, column = 0;
                    GetDisplayFormat(printTask.ImageDisplayFormat, ref row, ref column);
                    CombineImages(ImageList, combineJpgFile,row,column);

                    printTask.JpgFilmPath = combineJpgFile;
                    //Save into Table T_PrintTag
                    if (printTask.SaveToDB() == false)
                    {
                        Log.Error("Print Job {0} FAIL: {1}", SOPInstanceUID.UID.Split('.').Last(), "Save printTask to DB Failed");
                    }

                    autoIDList.Add(autoID.UID);
                }

                Status = PrintJobStatus.Done;
                OnStatusUpdate("Printing Done");

                foreach (var autoIDItem in autoIDList)
                {
                    ClientStart(autoIDItem,this.CallingIPAddress,this.CallingAETitle);
                }
            }
            catch (Exception ex)
            {
                Status = PrintJobStatus.Failure;
                OnStatusUpdate("Printing failed");
                Log.Error("Print Job {0} FAIL: {1}", SOPInstanceUID.UID.Split('.').Last(), ex.Message);
            }
            finally
            {

            }
        }

        static void ClientStart(string autoID,IPAddress callingIPAddress,string callingAETitle)
        {
            _asynchronousClient.StartClient(autoID, callingIPAddress,callingAETitle);
        }


        private void CombineImages(List<string> imgFiles,string finalImagePath, int row, int column)
        {
            try
            {
                int imageWidth = 0;
                int imageHeight = 0;
                foreach (string fileName in imgFiles)
                {
                    if (File.Exists(fileName))
                    {
                        Image img = Image.FromFile(fileName);
                        imageHeight =   img.Height;
                        imageWidth =   img.Width;
                        img.Dispose();
                        break;
                    }
                }

                int jpgWidth = column * imageWidth;
                int jpgHeight = row * imageHeight;

                Bitmap combinedImage = new Bitmap(jpgWidth, jpgHeight);
                Graphics g = Graphics.FromImage(combinedImage);
                g.Clear(SystemColors.AppWorkspace);
                int imageIndex = 0;
                for (int rowIndex = 0; rowIndex < row; rowIndex++)
                {
                    for (int columnIndex = 0; columnIndex < column; columnIndex++)
                    {
                        string fileName = imgFiles[imageIndex++];
                        if (File.Exists(fileName))
                        {
                            Image img = Image.FromFile(fileName);
                            g.DrawImage(img, new Point(columnIndex * imageWidth, rowIndex * imageHeight));
                            img.Dispose();
                        }
                        else
                        {
                            //Draw a black background Image
                            Bitmap blackBmp = new Bitmap(imageWidth, imageHeight);
                            Graphics blackGraphic = Graphics.FromImage(blackBmp);
                            blackGraphic.FillRectangle(Brushes.Black, new Rectangle(0, 0, imageWidth, imageHeight));   
                            g.DrawImage(blackBmp, new Point(columnIndex * imageWidth, rowIndex * imageHeight));
                            blackBmp.Dispose();
                        }
                    }
                }
                g.Dispose();
                combinedImage.Save(finalImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                combinedImage.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error("Exception When Combine JPEG Image: {@error}", ex);
            }
            finally
            {
                foreach (string fileName in imgFiles)
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                }
            }
        }
        private void OnPrintPage(object sender, PrintPageEventArgs e)
        {
            _currentFilmBox.Print(e.Graphics, e.MarginBounds, 100);

            _currentFilmBox = null;
            _currentPage++;

            e.HasMorePages = _currentPage < FilmBoxFolderList.Count;
        }

        private void OnQueryPageSettings(object sender, QueryPageSettingsEventArgs e)
        {
            OnStatusUpdate(string.Format("Printing film {0} of {1}", _currentPage + 1, FilmBoxFolderList.Count));
            var filmBoxFolder = string.Format("{0}\\{1}", FullPrintJobFolder, FilmBoxFolderList[_currentPage]);
            var filmSession = FilmSession.Load(string.Format("{0}\\FilmSession.dcm", filmBoxFolder));
            _currentFilmBox = FilmBox.Load(filmSession, filmBoxFolder);

            e.PageSettings.Margins.Left = 25;
            e.PageSettings.Margins.Right = 25;
            e.PageSettings.Margins.Top = 25;
            e.PageSettings.Margins.Bottom = 25;

            e.PageSettings.Landscape = _currentFilmBox.FilmOrienation == "LANDSCAPE";
        }

        private void DeletePrintFolder()
        {
            var folderInfo = new System.IO.DirectoryInfo(FullPrintJobFolder);
            if (folderInfo.Exists)
            {
                folderInfo.Delete(true);
            }
        }

        #endregion

        #region Notification Methods

        protected virtual void OnStatusUpdate(string info)
        {
            ExecutionStatus = Status.ToString();
            ExecutionStatusInfo = info;

            if (Status != PrintJobStatus.Failure)
            {
                Log.Info("Print Job {0} Status {1}: {2}", SOPInstanceUID.UID.Split('.').Last(), Status, info);
            }
            else
            {
                Log.Error("Print Job {0} Status {1}: {2}", SOPInstanceUID.UID.Split('.').Last(), Status, info);
            }
            if (StatusUpdate != null)
            {
                var args = new StatusUpdateEventArgs((ushort)Status, info, FilmSessionLabel, PrinterName);
                StatusUpdate(this, args);
            }
        }

        #endregion

        #region Helper Method
        private void GetDisplayFormat(string strDisplayformt, ref int row, ref int column)
        {
            try
            {
                string[] dispalyformatArray = Regex.Split(strDisplayformt, @"\\");
                if (dispalyformatArray[1] != null)
                {
                    string[] displayformatValue = Regex.Split(dispalyformatArray[1],@",");
                    if (displayformatValue[0] != null && displayformatValue[1] != null)
                    {
                        column = Convert.ToInt32(displayformatValue[0]);
                        row = Convert.ToInt32(displayformatValue[1]);
                    }
                }
                else
                {
                    Log.Error("Error When GetDisplayFormat with DiaplayFormat {0}", strDisplayformt);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception When GetDisplayFormat with DiaplayFormat {0}: {@error}", strDisplayformt,ex);
            }
        }
        #endregion

    }
}
