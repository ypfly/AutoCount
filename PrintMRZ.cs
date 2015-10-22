using Stimulsoft.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AutoCountDemo
{
    /// <summary>
    /// 保存Ftp服务器信息
    /// </summary>
    class ftpinfo
    {
        public string ftpip = "";
        public string ftpUNM = "";
        public string ftpDIR = "";
        public string ftpPWD = "";
    }
    class PrintMRZ
    {
        /// <summary>
        /// FTP下载MRZ
        /// </summary>
        /// <param name="filePath">文件存放路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="ftpPath">文件存放在FTP的路径</param>
        /// <param name="ftpServerIP">IP</param>
        /// <param name="ftpUserID">用户名</param>
        /// <param name="ftpPassword">密码</param>
        /// <returns></returns>
        public static int DownloadFtp(string filePath, string fileName, string ftpPath, string ftpServerIP, string ftpUserID, string ftpPassword)
        {
            FtpWebRequest reqFTP;
            try
            {
                //filePath = < <The full path where the file is to be created.>>, 
                //fileName = < <Name of the file to be created(Need not be the name of the file on FTP server).>> 
                if (File.Exists(filePath + "\\" + fileName))
                    File.Delete(filePath + "\\" + fileName);
                FileStream outputStream = new FileStream(filePath + "\\" + fileName, FileMode.Create);

                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + ftpPath));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.KeepAlive = false;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];

                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }

                ftpStream.Close();
                outputStream.Close();
                response.Close();
                return 0;
            }
            catch (Exception ex)
            {
                // Logging.WriteError(ex.Message + ex.StackTrace);
                // System.Windows.Forms.MessageBox.Show(ex.Message);
                return -2;
            }
        }
        /// <summary>
        /// 打印MRZ
        /// </summary>
        /// <param name="rptFiles">ftp文件</param>  
        /// <param name="dsSource">数据源</param>
        /// <param name="isPreview">是否预览</param>
        /// <returns></returns>
        public static string printStimulsoftReports(string rptFiles, DataSet dsSource, bool isPreview)
        {
            try
            {
                //处理下载多个文件的情形
                string lableFiles = rptFiles.Trim() + ",";
                string lFile = "";
                int filesCount = lableFiles.Length - lableFiles.Replace(",", String.Empty).Length;

                //对数据集重命名

                for (int i = 0; i < dsSource.Tables.Count; i++)
                {
                    dsSource.Tables[i].TableName = "t" + (i + 1).ToString();
                }

                StiReport report = new StiReport();

                for (int i = 0; i < filesCount; i++)  //连续打印
                {
                    lFile = lableFiles.Substring(0, lableFiles.IndexOf(","));
                    if (lFile.Trim() != "")
                    {
                        string LocalPath = Environment.GetEnvironmentVariable("TEMP") + "\\" + rptFiles;

                        if (System.IO.File.Exists(LocalPath) == true)
                        {

                            string rptFile = LocalPath;

                            report.Load(rptFile);

                            //对数据集重命名
                            for (int ii = 0; ii < dsSource.Tables.Count; ii++)
                            {
                                dsSource.Tables[ii].TableName = "t" + (ii + 1).ToString();
                            }

                            report.RegData(dsSource);
                            if (isPreview == false) report.PrinterSettings.ShowDialog = false;
                            report.Render(false);
                            if (isPreview == false)
                            {
                                report.Print();
                                File.Delete(LocalPath);
                            }
                            else
                            {
                                report.Show(true);
                                File.Delete(LocalPath);
                            }

                        }
                        else
                        {
                            return " RptReportServerURL + lFile, PlugInCommand";

                        }
                    }

                    lableFiles = lableFiles.Substring(lableFiles.IndexOf(",") + 1);
                }


            }

            catch (Exception er)
            {
                return er.ToString();

            }
            return "打印成功";
        }
    }
}
