using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Data;
using System.Data.SqlClient;
using PrintSystem.Common.SQLServer;


namespace PrintSystem.Common
{
    public class IniFile
    {
        #region API函数声明
        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);

        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);
        #endregion

        #region 读Ini文件
        public static string ReadIniData(string Section, string Key, string NoText, string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                StringBuilder temp = new StringBuilder(1024);
                GetPrivateProfileString(Section, Key, NoText, temp, 1024, iniFilePath);
                return temp.ToString();
            }
            else
            {
                return String.Empty;
            }
        }
        #endregion

        #region 写Ini文件
        public static bool WriteIniData(string Section, string Key, string Value, string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                long OpStation = WritePrivateProfileString(Section, Key, Value, iniFilePath);
                if (OpStation == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion
    }

    public class PrintTag
    {

        #region PrintTag Property
        //AutoID, which is used in System to track Print Job
        public string AutoUID { get; set; }

        //The calling IP Address
        public IPAddress CallingIPAddress { get; set; }

        // Gets the calling application entity.
        public string CallingAE { get;  set; }

        /// Gets the called application entity.
        public string CalledAE { get;  set; }


        //(0008,0018) Print UID ,Film Box Level
        public string printUID { get; set; }

        //(2020,0010) Image Box Position, Image Box Level???
        public int ImageBoxPosition { get; set; }

        //(2010,0100)  Border Density,FilmBox Level
        public string BorderDensity { get; set; }

        //(2010,0100)  Image Display format,FilmBox Level
        public string ImageDisplayFormat { get; set; }

        //(2010,0110) Empty Image Density,FilmBox Level
        public string EmptyImageDensity { get; set; }

        //(2000,0010) Number of Copies,FilmSession Level
        public int NumberOfCopies { get; set; }

        //(2010,0050) Film Size,FilmBox Level
        public string FilmSizeID { get; set; }

        //(2010,0040) File Orientation, FilmBox Level
        public string FilmOrienation { get; set; }

        //(2010,0060) Magnification Type, FilmBox Level
        public string MagnificationType { get; set; }

        //(2010,0130)  Max Density, FilmBox Level
        public ushort MaxDensity { get; set; }

        //(2010,0120) Min Density, FilmBox Level
        public ushort MinDensity { get; set; }

        //(2010,0080) Smoothing Type, FilmBox Level
        public string SmoothingType { get; set; }

        //(2010,0140) Trim, FilmBox Level
        public string Trim { get; set; }

        //(0028,0002) ImageBit, Image Level ????
        public ushort SamplesPerPixel { get; set; }

        //(2000,0030) Medium Type,FilmSession Level
        public string MediumType { get; set; }

        //(2050,0020) Presentation LUT, FilmBox Level
        public string PresentationLut { get; set; }

        //DICOM Files Path
        public string DicomFilePath { get; set; }
        #endregion
        public bool SaveToDB()
        {
            try
            {
                List<SqlWrapper> lstSqlWrapper = new List<SqlWrapper>();
                SqlWrapper sqlWrapper = new SqlWrapper();
                string strSQL = String.Format("Insert into T_PrintTag" +
                        @"([AutoID],[PrintUID],[ImageUID],[CallingIP],[CallingAE],[CalledAE],[BorderDensity],[DisplayFormat],[EmptyImageDensity]
                        ,[Copies]
                        ,[FilmSize]
                        ,[FilmOrientation]
                        ,[MagnificationType]
                        ,[MaxDensity]
                        ,[MinDensity]
                        ,[SmoothingType]
                        ,[Trim]
                        ,[MediumType]
                        ,[PresentationLUT]
                        ,[FilePath]) Values" +
                        @"(@autoID,@printUID,@imageUID,@callingIP,@callingAE,@calledAE,@borderDensity,@displayFormat,@emptyImageDensity,@copies,
                        @filmSize,@filmOrientation,@magnificationType,@maxDensity,@minDensity,@smoothingType,@trim,@mediumType,@presentationLUT,@filePath)");


                List<SqlParameter> lstParas = new List<SqlParameter>();
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@autoID";
                param.Value = this.AutoUID;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@printUID";
                param.Value = this.printUID;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@imageUID";
                param.Value = this.ImageBoxPosition;
                param.SqlDbType = SqlDbType.TinyInt;
                param.Size = 8;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@callingIP";
                param.Value = this.CallingIPAddress.ToString();
                param.SqlDbType = SqlDbType.NVarChar;
                param.Size = 16;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@callingAE";
                param.Value = this.CallingAE;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@calledAE";
                param.Value = this.CalledAE;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@borderDensity";
                param.Value = this.BorderDensity;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@displayFormat";
                param.Value = this.ImageDisplayFormat;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@emptyImageDensity";
                param.Value = this.EmptyImageDensity;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@copies";
                param.Value = this.NumberOfCopies;
                param.SqlDbType = SqlDbType.TinyInt;
                param.Size = 8;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@filmSize";
                param.Value = this.FilmSizeID;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@filmOrientation";
                param.Value = this.FilmOrienation;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@magnificationType";
                param.Value = this.MagnificationType;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@maxDensity";
                param.Value = this.MaxDensity;
                param.SqlDbType = SqlDbType.Int;
                param.Size = 16;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@minDensity";
                param.Value = this.MinDensity;
                param.SqlDbType = SqlDbType.Int;
                param.Size = 16;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@smoothingType";
                param.Value = this.SmoothingType;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@trim";
                param.Value = this.Trim;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@mediumType";
                param.Value = this.MediumType;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@presentationLUT";
                param.Value = this.PresentationLut;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@filePath";
                param.Value = this.DicomFilePath;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);


                sqlWrapper.SqlString = strSQL;
                sqlWrapper.Parameter = lstParas.ToArray();

                lstSqlWrapper.Add(sqlWrapper);

                int iRet = SQLServerHelper.ExecuteNonQuery(lstSqlWrapper, QCConnectionType.PrintSystem);
                if (iRet > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {

            }

            return false;
        }
    }

    public class PrintTagHelper
    {
        public List<PrintTag> GetPrintTagFromDBByAutoID(string autoID)
        {
            try
            {
                List<PrintTag> printTagList = new List<PrintTag>();

                List<SqlWrapper> lstSqlWrapper = new List<SqlWrapper>();
                SqlWrapper sqlWrapper = new SqlWrapper();
                string strSQL = string.Format("Select * from T_PrintTag where AutoID = '{0}' order by ImageUID", autoID);
                List<SqlParameter> lstParas = new List<SqlParameter>();
                lstParas.Add(new SqlParameter("@AutoID", autoID));
                sqlWrapper.SqlString = strSQL;
                sqlWrapper.Parameter = lstParas.ToArray();

                DataSet ds = SQLServerHelper.ExecuteQuery(sqlWrapper, QCConnectionType.PrintSystem);

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    //DataRow row = ds.Tables[0].Rows[0];
                    PrintTag printTag = new PrintTag();
                    printTag.AutoUID = row["AutoID"].ToString().Trim();
                    printTag.BorderDensity = row["BorderDensity"].ToString().Trim();

                    printTag.CalledAE = row["CalledAE"].ToString().Trim();
                    printTag.CallingAE = row["CallingAE"].ToString().Trim();
                    printTag.CallingIPAddress = IPAddress.Parse(row["CallingIP"].ToString().Trim());
                    printTag.DicomFilePath = row["FilePath"].ToString().Trim();
                    printTag.EmptyImageDensity = row["EmptyImageDensity"].ToString().Trim();
                    printTag.FilmOrienation = row["FilmOrientation"].ToString().Trim();
                    printTag.FilmSizeID = row["FilmSize"].ToString().Trim();
                    printTag.ImageBoxPosition = Convert.ToInt32(row["ImageUID"].ToString().Trim());
                    printTag.ImageDisplayFormat = row["DisplayFormat"].ToString().Trim();
                    printTag.MagnificationType = row["MagnificationType"].ToString().Trim();
                    printTag.MaxDensity = Convert.ToUInt16(row["MaxDensity"].ToString().Trim());
                    printTag.MinDensity = Convert.ToUInt16(row["MinDensity"].ToString().Trim());
                    printTag.NumberOfCopies = Convert.ToInt32(row["Copies"].ToString().Trim());
                    printTag.PresentationLut = row["PresentationLUT"].ToString().Trim();
                    printTag.printUID = row["PrintUID"].ToString().Trim();
                    //printTag.SamplesPerPixel = row["BorderDensity"].ToString().Trim();
                    printTag.SmoothingType = row["SmoothingType"].ToString().Trim();
                    printTag.Trim = row["Trim"].ToString().Trim();
                    printTagList.Add(printTag);
                }

                return printTagList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public class PrintTask
    {
        #region PrintTask Property
        //FilmUID, which is used in System to track Print Job
        public string FilmUID { get; set; }

        //The calling IP Address
        public IPAddress CallingIPAddress { get; set; }

        // Gets the calling application entity.
        public string CallingAE { get; set; }

        /// Gets the called application entity.
        public string CalledAE { get; set; }


        //(0008,0018) Print UID ,Film Box Level
        public string printUID { get; set; }

        //(2010,0100)  Border Density,FilmBox Level
        public string BorderDensity { get; set; }

        //(2010,0100)  Image Display format,FilmBox Level
        public string ImageDisplayFormat { get; set; }

        //(2010,0110) Empty Image Density,FilmBox Level
        public string EmptyImageDensity { get; set; }

        //(2000,0010) Number of Copies,FilmSession Level
        public int NumberOfCopies { get; set; }

        //(2010,0050) Film Size,FilmBox Level
        public string FilmSizeID { get; set; }

        //(2010,0040) File Orientation, FilmBox Level
        public string FilmOrienation { get; set; }

        //(2010,0060) Magnification Type, FilmBox Level
        public string MagnificationType { get; set; }

        //(2010,0130)  Max Density, FilmBox Level
        public ushort MaxDensity { get; set; }

        //(2010,0120) Min Density, FilmBox Level
        public ushort MinDensity { get; set; }

        //(2010,0080) Smoothing Type, FilmBox Level
        public string SmoothingType { get; set; }

        //(2010,0140) Trim, FilmBox Level
        public string Trim { get; set; }

        //(0028,0002) ImageBit, Image Level ????
        public ushort SamplesPerPixel { get; set; }

        //(2000,0030) Medium Type,FilmSession Level
        public string MediumType { get; set; }

        //(2050,0020) Presentation LUT, FilmBox Level
        public string PresentationLut { get; set; }

        //DICOM Files Path
        public string JpgFilmPath { get; set; }
        #endregion


        public bool SaveToDB()
        {
            try
            {
                List<SqlWrapper> lstSqlWrapper = new List<SqlWrapper>();
                SqlWrapper sqlWrapper = new SqlWrapper();
                string strSQL = String.Format("Insert into T_PrintTask" +
                        @"([FilmUID],[PrintUID],[CallingIP],[CallingAE],[CalledAE],[BorderDensity],[DisplayFormat],[EmptyImageDensity]
                        ,[Copies]
                        ,[FilmSize]
                        ,[FilmOrientation]
                        ,[MagnificationType]
                        ,[MaxDensity]
                        ,[MinDensity]
                        ,[SmoothingType]
                        ,[Trim]
                        ,[MediumType]
                        ,[PresentationLUT]
                        ,[JpgFilmPath]) Values" +
                        @"(@autoID,@printUID,@callingIP,@callingAE,@calledAE,@borderDensity,@displayFormat,@emptyImageDensity,@copies,
                        @filmSize,@filmOrientation,@magnificationType,@maxDensity,@minDensity,@smoothingType,@trim,@mediumType,@presentationLUT,@filePath)");


                List<SqlParameter> lstParas = new List<SqlParameter>();
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@autoID";
                param.Value = this.FilmUID;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@printUID";
                param.Value = this.printUID;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                //param = new SqlParameter();
                //param.ParameterName = "@imageUID";
                //param.Value = this.ImageBoxPosition;
                //param.SqlDbType = SqlDbType.TinyInt;
                //param.Size = 8;
                //lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@callingIP";
                param.Value = this.CallingIPAddress.ToString();
                param.SqlDbType = SqlDbType.NVarChar;
                param.Size = 16;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@callingAE";
                param.Value = this.CallingAE;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@calledAE";
                param.Value = this.CalledAE;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@borderDensity";
                param.Value = this.BorderDensity;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@displayFormat";
                param.Value = this.ImageDisplayFormat;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@emptyImageDensity";
                param.Value = this.EmptyImageDensity;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@copies";
                param.Value = this.NumberOfCopies;
                param.SqlDbType = SqlDbType.TinyInt;
                param.Size = 8;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@filmSize";
                param.Value = this.FilmSizeID;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@filmOrientation";
                param.Value = this.FilmOrienation;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@magnificationType";
                param.Value = this.MagnificationType;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@maxDensity";
                param.Value = this.MaxDensity;
                param.SqlDbType = SqlDbType.Int;
                param.Size = 16;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@minDensity";
                param.Value = this.MinDensity;
                param.SqlDbType = SqlDbType.Int;
                param.Size = 16;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@smoothingType";
                param.Value = this.SmoothingType;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@trim";
                param.Value = this.Trim;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@mediumType";
                param.Value = this.MediumType;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@presentationLUT";
                param.Value = this.PresentationLut;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@filePath";
                param.Value = this.JpgFilmPath;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);


                sqlWrapper.SqlString = strSQL;
                sqlWrapper.Parameter = lstParas.ToArray();

                lstSqlWrapper.Add(sqlWrapper);

                int iRet = SQLServerHelper.ExecuteNonQuery(lstSqlWrapper, QCConnectionType.PrintSystem);
                if (iRet > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }
    }

    public class PrintImage
    {
        #region PrintImage Property
        #endregion
        //FilmUID, which is used in System to track Print Job
        public string FilmUID { get; set; }

        //(2020,0010) Image Box Position, Image Box Level???
        public int ImageBoxPosition { get; set; }

        //DICOM Files Path
        public string DicomFilePath { get; set; }

        public bool SaveToDB()
        {
            try
            {
                List<SqlWrapper> lstSqlWrapper = new List<SqlWrapper>();
                SqlWrapper sqlWrapper = new SqlWrapper();
                string strSQL = String.Format("Insert into T_PrintImage" +
                        @"([FilmUID],[ImageUID],[DcmFilePath]) Values" +
                        @"(@filmUID,@imageUID,@dcmfilePath)");


                List<SqlParameter> lstParas = new List<SqlParameter>();
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@filmUID";
                param.Value = this.FilmUID;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@imageUID";
                param.Value = this.ImageBoxPosition;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                param = new SqlParameter();
                param.ParameterName = "@dcmfilePath";
                param.Value = this.DicomFilePath;
                param.SqlDbType = SqlDbType.NVarChar;
                lstParas.Add(param);

                sqlWrapper.SqlString = strSQL;
                sqlWrapper.Parameter = lstParas.ToArray();

                lstSqlWrapper.Add(sqlWrapper);

                int iRet = SQLServerHelper.ExecuteNonQuery(lstSqlWrapper, QCConnectionType.PrintSystem);
                if (iRet > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }
    }


    public class MonitorClient
    {
        //Calling IP
        public string CallingIP { get; set; }

        //Calling AE Title
        public string CallingAETitle { get; set; }

        //Calling Device Type
        public string CallingDeviceType { get; set; }

        //Calling Device Name
        public string CallingDeviceName { get; set; }

        //Monitor IP
        public string MonitorIP { get; set; }

        //Monitor Port
        public int MonitorPort { get; set; }
    }
    public class MonitorClientHelper
    {
        public static List<MonitorClient> GetMonitors()
        {
            try
            {
                List<MonitorClient> monitorList = new List<MonitorClient>();

                List<SqlWrapper> lstSqlWrapper = new List<SqlWrapper>();
                SqlWrapper sqlWrapper = new SqlWrapper();
                string strSQL = @"Select * from T_Monitor";
                sqlWrapper.SqlString = strSQL;

                DataSet ds = SQLServerHelper.ExecuteQuery(sqlWrapper, QCConnectionType.PrintSystem);

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    MonitorClient monitor = new MonitorClient();
                    monitor.CallingIP = row["CallingIP"].ToString().Trim();
                    monitor.CallingAETitle = row["CallingAETitle"].ToString().Trim();
                    monitor.CallingDeviceType = row["CallingDeviceType"].ToString().Trim();
                    monitor.CallingDeviceName = row["CallingDeviceName"].ToString().Trim();
                    monitor.MonitorIP = row["MonitorIP"].ToString().Trim();
                    monitor.MonitorPort = Convert.ToInt32(row["MonitorPort"].ToString().Trim());
                    monitorList.Add(monitor);
                }

                return monitorList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<MonitorClient> GetMonitorsByCallingSide(IPAddress callingIPAddress, string callingAETitle)
        {
            try
            {
                List<MonitorClient> monitorList = new List<MonitorClient>();

                List<SqlWrapper> lstSqlWrapper = new List<SqlWrapper>();
                SqlWrapper sqlWrapper = new SqlWrapper();
                string strSQL = string.Format("Select * from T_Monitor where CallingIP = '{0}' and CallingAETitle = '{1}'", callingIPAddress.ToString(), callingAETitle);
                List<SqlParameter> lstParas = new List<SqlParameter>();
                lstParas.Add(new SqlParameter("@CallingIP", callingIPAddress.ToString()));
                lstParas.Add(new SqlParameter("@CallingAETitle", callingAETitle));
                sqlWrapper.SqlString = strSQL;
                sqlWrapper.Parameter = lstParas.ToArray();

                DataSet ds = SQLServerHelper.ExecuteQuery(sqlWrapper, QCConnectionType.PrintSystem);

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    MonitorClient monitor = new MonitorClient();
                    monitor.CallingIP = row["CallingIP"].ToString().Trim();
                    monitor.CallingAETitle = row["CallingAETitle"].ToString().Trim();
                    monitor.CallingDeviceType = row["CallingDeviceType"].ToString().Trim();
                    monitor.CallingDeviceName = row["CallingDeviceName"].ToString().Trim();
                    monitor.MonitorIP = row["MonitorIP"].ToString().Trim();
                    monitor.MonitorPort = Convert.ToInt32(row["MonitorPort"].ToString().Trim());
                    monitorList.Add(monitor);
                }

                return monitorList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }


    public class Listener
    {
        //Listen IP
        public string ListenIP { get; set; }

        //Listen Port
        public int ListenPort { get; set; }

        //Listen AE Title
        public string AETitle { get; set; }
    }

    public class ListenHelper
    {
        public static List<Listener> GetListeners()
        {
            try
            {
                List<Listener> ListenerList = new List<Listener>();

                List<SqlWrapper> lstSqlWrapper = new List<SqlWrapper>();
                SqlWrapper sqlWrapper = new SqlWrapper();
                string strSQL = @"Select * from T_Listen";
                sqlWrapper.SqlString = strSQL;

                DataSet ds = SQLServerHelper.ExecuteQuery(sqlWrapper, QCConnectionType.PrintSystem);

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    Listener listener = new Listener();
                    listener.ListenIP = row["IP"].ToString().Trim();
                    listener.ListenPort = Convert.ToInt32(row["Port"].ToString().Trim());
                    listener.AETitle = row["AETitle"].ToString().Trim();
                    ListenerList.Add(listener);
                }

                return ListenerList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    [Serializable]
    public class PrinterInfo
    {
        #region Property
        public string AutoID = string.Empty;

        public string PrinterIPAddress = string.Empty;

        public string PrinterAETitle = string.Empty;

        public int PrinterPort = 0;
        #endregion
    }

}
