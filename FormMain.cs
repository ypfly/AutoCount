using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using elf;
using System.Timers;
using OrBitADCService;
using System.IO.Ports;
using Infragistics.Win.UltraWinGrid;
using System.Net;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Diagnostics;

namespace AutoCountDemo
{

    public partial class Form1 : System.Windows.Forms.Form
    {
        private SocketClient client_Receive;
        private SocketClient client_Send;
        string mM281A_IP = "";
        private bool mDeviceHeartBeatEnable;
        private bool mConnected;

        /// <summary>
        /// 计数
        /// </summary>
        int mCount = 0;

        /// <summary>
        /// 链接sql的对象
        /// </summary>
        ADCService adc = new ADCService();
        FormSetPieceWeight fspw = null;

        /// <summary>
        ///  跨越工作线程和UI线程的委托
        /// </summary>
        private delegate void Connected(string MSG);

        /// <summary>
        /// 保存FTP信息对象
        /// </summary>
        ftpinfo FTP = new ftpinfo();

        /// <summary>
        /// 修改UI的委托对象
        /// </summary>
        MyDelegate md = new MyDelegate();

        /// <summary>
        /// 自动包装程序配置参数对象
        /// </summary>
        packConfig _packConfig = new packConfig();

        /// <summary>
        /// 未修改前的单重
        /// </summary>
        decimal olddz = 0;

        string temp = Environment.GetEnvironmentVariable("TEMP") + "\\";
        /// <summary>
        /// WCF地址
        /// </summary>
        public string WCFADD = @"http://{0}/browserWCFService/DataService.svc";


        bool Closing = false,//窗口是否关闭
            Listening = false;//是否在监听串口
        private StringBuilder builder = new StringBuilder();
        string oldLotsn = ""; //记录上一个随工单
        int isok = 0;
        /// <summary>
        /// 获取FTP信息
        /// </summary>
        private void GetFtpInfo()
        {
            string strmsg = "";
            try
            {
                strmsg = adc.GetDataSetWithSQLString(WCFADD, sqlHelp.FtpInfo).Tables[0].Rows[0][0].ToString().Replace("[", "").Replace("]", "");
            }
            catch
            {
                MessageBox.Show("FTP服务器地址出错，请联系管理员", "系统提示");
                return;
            }
            //[FTP:33.0.1.4,USER:Mesuser,PASSWORD:Mesorbit123,FOLDER:MRZ]
            string[] strq = strmsg.Split(',');
            try
            {
                foreach (var item in strq)
                {
                    if (item.Contains("FTP"))
                    {
                        FTP.ftpip = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1));
                        continue;
                    }
                    if (item.Contains("USER"))
                    {
                        FTP.ftpUNM = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1));
                        continue;
                    }
                    if (item.Contains("PASSWORD"))
                    {
                        FTP.ftpPWD = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1));
                        continue;
                    }
                    if (item.Contains("FOLDER"))
                    {
                        FTP.ftpDIR = item.Substring(item.IndexOf(":") + 1, item.Length - (item.IndexOf(":") + 1));
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }



        /// <summary>
        /// 串口对象
        /// </summary>
        SerialPort sport = null;
        public Form1()
        {
            InitializeComponent();
            //   CheckForIllegalCrossThreadCalls = false;
        }

        /// <summary>
        /// 每20秒检测M281A是否发送了心跳信号
        /// </summary>
        private System.Timers.Timer DeviceHeatBeatEnableTimer = new System.Timers.Timer(20000);

        /// <summary>
        /// 每10秒请求M281A发送心跳检测
        /// </summary>
        private System.Timers.Timer DeviceHeatBeatTimer = new System.Timers.Timer(10000);



        /// <summary>
        /// 处理接收到的计数信号，收到一次表示有一个包装通过了自动扫描装置
        /// </summary>
        private void ClientOnReceive(byte[] ReceiveData)
        {
            try
            {
                if ((ReceiveData != null) && (ReceiveData.Length != 0))
                {
                    if ((ReceiveData[14] == 1))//收到康耐德M281A的DI0的信号
                    {
                        //在此添加计数器累加
                        mCount = mCount + 1;
                        Connected Data = new Connected(PostMessage);
                        this.BeginInvoke(Data, mCount.ToString());
                    }
                }
            }
            catch
            {

            }
        }


        /// <summary>
        ///  收到与康耐德M281A Sock 通讯连接状态事件
        /// </summary> 
        private void clientConnected(bool Connected)
        {
            this.mConnected = Connected;
            if (!Connected)
            {
                md.SetRichTextBoxText(richTextBox1, "与计数模块链接异常", true);
                isok = 0;
            }
            else
            {
                if (isok == 0)
                {
                    md.SetRichTextBoxText(richTextBox1, "与计数模块链接成功", false);
                    isok = 1;
                }
            }
        }

        //收到与康耐德M281A Sock 通讯过程中发生的异常 ErrCode错误代码，ErrorDiscription错误描述
        private void ClientOnErr(string ErrCode, string ErrorDiscription)
        {
            //在此添加与康耐德M281A Sock 通讯过程中发生的异常 的处理代码
            string str = "计数功能异常！错误代码:" + ErrCode + ". 错误描述:" + ErrorDiscription;
            md.SetRichTextBoxText(richTextBox1, str, true);
        }

        //收到心跳信号
        private void HeartBeatOnReceive(byte[] ReceiveData)
        {
            if (ReceiveData.Length > 0)
            {
                this.mDeviceHeartBeatEnable = true;
            }
        }

        private void HeartBeatEnableTimerOut(object sender, ElapsedEventArgs e)
        {
            try
            {
                this.DeviceHeatBeatEnableTimer.Stop();
                if (!this.mDeviceHeartBeatEnable)
                {
                    //在此添加康耐德M281A 掉线 的提示 代码
                    md.SetRichTextBoxText(richTextBox1, "计数模块掉线，请联系管理员", true);
                }
                this.mDeviceHeartBeatEnable = false;
                this.DeviceHeatBeatEnableTimer.Start();
            }
            catch
            {
            }
        }

        //请求M281A发送心跳信号
        private void ReqHeartBeatTimerOut(object sender, ElapsedEventArgs e)
        {
            if (this.mConnected)
            {
                try
                {
                    this.DeviceHeatBeatTimer.Stop();
                    byte[] msg = CommonUtil.strToHexByte("00 01 00 00 00 06 01 03 00 1B 00 01");
                    this.client_Send.SendMessage(msg);
                    this.DeviceHeatBeatTimer.Start();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 初始化计数模块
        /// </summary>
        /// <param name="IP">技术模块的IP</param>
        public void Reset(string IP)
        {

            this.mM281A_IP = IP;
            this.client_Receive = new SocketClient();
            this.client_Receive.HeartBeatTimerEnabled(false);
            this.client_Receive.RequestConnectServer(IP, 0x139c);
            this.client_Receive.OnSocketConnected += new SocketClient.socketConnectedEventHandler(this.clientConnected);
            this.client_Receive.OnReiceive += new SocketClient.ClientReceiveHandler(this.ClientOnReceive);
            this.client_Receive.OnError += new SocketClient.ClientErrorEventHandler(this.ClientOnErr);

            this.client_Send = new SocketClient();
            this.client_Send.HeartBeatTimerEnabled(false);
            this.client_Send.RequestConnectServer(this.mM281A_IP, 0x1f6);
            this.client_Send.OnSocketConnected += new SocketClient.socketConnectedEventHandler(this.clientConnected);
            this.client_Send.OnReiceive += new SocketClient.ClientReceiveHandler(this.HeartBeatOnReceive);
            this.DeviceHeatBeatTimer.Elapsed += new ElapsedEventHandler(this.ReqHeartBeatTimerOut);
            this.DeviceHeatBeatTimer.AutoReset = true;
            this.DeviceHeatBeatTimer.Enabled = true;
            this.DeviceHeatBeatEnableTimer.Elapsed += new ElapsedEventHandler(this.HeartBeatEnableTimerOut);
            this.DeviceHeatBeatEnableTimer.AutoReset = true;
            this.DeviceHeatBeatEnableTimer.Enabled = true;
        }

        /// <summary>
        /// 释放技术模块的资源
        /// </summary>
        public void _Dispose()
        {
            if (client_Receive == null && client_Send == null)
                return;
            this.client_Receive.OnSocketConnected -= new SocketClient.socketConnectedEventHandler(this.clientConnected);
            this.client_Receive.OnReiceive -= new SocketClient.ClientReceiveHandler(this.ClientOnReceive);
            this.client_Receive.OnError -= new SocketClient.ClientErrorEventHandler(this.ClientOnErr);

            this.client_Send.OnSocketConnected -= new SocketClient.socketConnectedEventHandler(this.clientConnected);
            this.client_Send.OnReiceive -= new SocketClient.ClientReceiveHandler(this.HeartBeatOnReceive);
            this.DeviceHeatBeatTimer.Elapsed -= new ElapsedEventHandler(this.ReqHeartBeatTimerOut);
            this.DeviceHeatBeatTimer.Stop();
            this.DeviceHeatBeatTimer.Enabled = false;
            this.DeviceHeatBeatTimer.Close();
            this.DeviceHeatBeatTimer.Dispose();
            this.DeviceHeatBeatEnableTimer.Elapsed -= new ElapsedEventHandler(this.HeartBeatEnableTimerOut);
            this.DeviceHeatBeatEnableTimer.Stop();
            this.DeviceHeatBeatEnableTimer.Enabled = false;
            this.DeviceHeatBeatEnableTimer.Close();
            this.DeviceHeatBeatEnableTimer.Dispose();
        }

        /// <summary>
        /// 系统关闭时发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            //根据当前串口对象，来判断操作   
            if (sport != null && sport.IsOpen)
            {
                Closing = true;
                while (Listening) Application.DoEvents();
                //打开时点击，则关闭串口   
                sport.Close();
            }
            _Dispose();
        }

        /// <summary>
        /// 修改UI的计数信息
        /// </summary>
        /// <param name="MSG"></param>
        private void PostMessage(string MSG)
        {
            this.label1.Text = MSG;
        }



        /// <summary>
        /// 输入随工单时获取的信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textLotSN_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyValue == 13)
            {
                if (oldLotsn != "")
                {
                    int lt = 0;
                    DataSet ltds = adc.GetDataSetWithSQLString(WCFADD, string.Format(sqlHelp.GetLTPCS, oldLotsn));
                    if (ltds.Tables.Count <= 0)
                        return;


                    if (ltds.Tables[0].Rows[0][0].ToString() != "")
                        lt = int.Parse(ltds.Tables[0].Rows[0][0].ToString());
                    md.SetRichTextBoxText(richTextBox1, "即将打印[" + oldLotsn + "]的[" + mCount + "]张客户条码", false);
                    printBQ(oldLotsn, lt, int.Parse(label1.Text));
                    mCount = 0;
                    PostMessage("0");
                    olddz = 0;

                }
                comMOid.Text = "";
                string sql = string.Format(sqlHelp.GetLotsnINfo, textLotSN.Text);
                DataSet ds = adc.GetDataSetWithSQLString(WCFADD, sql);
                if (ds.Tables[ds.Tables.Count - 1].Rows[0]["Return Value"].ToString() == "-1")
                {
                    MessageBox.Show(ds.Tables[ds.Tables.Count - 1].Rows[0]["@I_ReturnMessage"].ToString());
                    md.SetRichTextBoxText(richTextBox1, ds.Tables[ds.Tables.Count - 1].Rows[0]["@I_ReturnMessage"].ToString(), true);
                }
                if (ds.Tables[0].Rows.Count <= 0)
                    return;
                DGVLotSN.DataSource = ds.Tables[0];
                DGVLotSN.Rows[0].Activation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                {
                    switch (ds.Tables[0].Columns[i].ColumnName)
                    {
                        case "FlowWorkNO":
                            SetUltragridCaption(DGVLotSN.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "随工单号");
                            break;
                        case "ProductNO":
                            SetUltragridCaption(DGVLotSN.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "成品料号");
                            break;
                        case "ProductNM":
                            SetUltragridCaption(DGVLotSN.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "产品名称");
                            break;
                        case "OpreatorNM":
                            SetUltragridCaption(DGVLotSN.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "操作工");
                            break;
                        case "PCS":
                            SetUltragridCaption(DGVLotSN.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "数量");
                            break;
                        case "EquipmentNO":
                            SetUltragridCaption(DGVLotSN.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "电镀线");
                            break;
                    }
                }

                sql = string.Format(sqlHelp.GetMO, textLotSN.Text);
                DataSet mods = adc.GetDataSetWithSQLString(WCFADD, sql);
                if (mods.Tables[mods.Tables.Count - 1].Rows[0]["Return Value"].ToString() == "-1")
                {
                    MessageBox.Show(mods.Tables[mods.Tables.Count - 1].Rows[0]["@I_ReturnMessage"].ToString());
                    md.SetRichTextBoxText(richTextBox1, mods.Tables[mods.Tables.Count - 1].Rows[0]["@I_ReturnMessage"].ToString(), true);
                }

                comMOid.DataSource = mods.Tables[0];
                comMOid.Rows[0].Activation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                for (int i = 0; i < mods.Tables[0].Columns.Count; i++)
                {
                    switch (mods.Tables[0].Columns[i].ColumnName)
                    {
                        case "DDMoItem":
                            SetUltragridCaption(comMOid.DisplayLayout, mods.Tables[0].Columns[i].ColumnName, "工单ID");
                            break;
                        case "ScheduleNo":
                            SetUltragridCaption(comMOid.DisplayLayout, mods.Tables[0].Columns[i].ColumnName, "排产序号");
                            break;
                        case "MOName":
                            SetUltragridCaption(comMOid.DisplayLayout, mods.Tables[0].Columns[i].ColumnName, "工单名称");
                            break;
                        case "ProductName":
                            SetUltragridCaption(comMOid.DisplayLayout, mods.Tables[0].Columns[i].ColumnName, "成品料号");
                            break;
                        case "JGSingleWeightid":
                            SetUltragridCaption(comMOid.DisplayLayout, mods.Tables[0].Columns[i].ColumnName, "单重");
                            break;
                    }
                }
                textBZpcs.Text = "0";
                comMOid.Text = "请选择工单";
            }
        }

        /// <summary>
        /// 设置中文文本
        /// </summary>
        /// <param name="gridlayout"></param>
        /// <param name="ColumnsName"></param>
        /// <param name="CaptionName"></param>
        void SetUltragridCaption(UltraGridLayout gridlayout, string ColumnsName, string CaptionName)
        {
            gridlayout.Bands[0].Columns[ColumnsName].Header.Caption = CaptionName;

        }

        /// <summary>
        /// 选择工单时显示工的信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comMOid_RowSelected(object sender, Infragistics.Win.UltraWinGrid.RowSelectedEventArgs e)
        {
            if (e.Row == null)
                return;
            if (e.Row.Cells.Count < 3)
                return;
            string SingleWeight = e.Row.Cells["JGSingleWeightid"].Value.ToString();

            string sql = string.Format(sqlHelp.GeMOinfo, e.Row.Cells["DDMoItem"].Value.ToString());
            DataSet ds = adc.GetDataSetWithSQLString(WCFADD, sql);
            DGVMoInfo.DataSource = ds;
            DGVMoInfo.Rows[0].Activation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
            for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
            {
                switch (ds.Tables[0].Columns[i].ColumnName)
                {
                    case "ProductDescription":
                        SetUltragridCaption(DGVMoInfo.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "产品描述");
                        break;
                    case "PlannedDateFrom":
                        SetUltragridCaption(DGVMoInfo.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "计划交货时间");
                        break;
                    case "MOName":
                        SetUltragridCaption(DGVMoInfo.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "工单名称");
                        break;
                    case "ProductName":
                        SetUltragridCaption(DGVMoInfo.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "产品名称");
                        break;
                    case "MOQtyRequired":
                        SetUltragridCaption(DGVMoInfo.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "计划交货数量");
                        break;
                    case "Realmaterialpcs":
                        SetUltragridCaption(DGVMoInfo.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "实际领料数");
                        break;
                    case "DDmaterialQty":
                        SetUltragridCaption(DGVMoInfo.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "电镀领料数");
                        break;
                    case "BZWeight":
                        SetUltragridCaption(DGVMoInfo.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "标准重量");
                        break;

                    case "JGSingleWeight":
                        SetUltragridCaption(DGVMoInfo.DisplayLayout, ds.Tables[0].Columns[i].ColumnName, "单重");
                        break;

                }

            }
            numMOQtyDonePcs.Text = ds.Tables[0].Rows[0]["Realmaterialpcs"].ToString();//完成数量
            numMOQtyRequired.Text = ds.Tables[0].Rows[0]["MOQtyRequired"].ToString(); //计划数量
            textBZpcs.Text = ds.Tables[0].Rows[0]["BZWeight"].ToString();//标箱数量
            string jgsingleweight = ds.Tables[0].Rows[0]["jgsingleweight"].ToString();//单重
            if (jgsingleweight != "")
            {
                NumpieceWeight.Value = decimal.Parse(jgsingleweight);
            }
            else
            {
                NumpieceWeight.Value = 0;
                MessageBox.Show("请维护电镀产品的单品重量", "系统提示");
                md.SetRichTextBoxText(richTextBox1, "请维护电镀产品的单品重量", true);
                return;
            }
            oldLotsn = textLotSN.Text;

        }

        private void comMOid_ValueChanged(object sender, EventArgs e)
        {
            if (comMOid.Text == "")
            {
                DGVMoInfo.DataSource = null;
            }
        }


        /// <summary>
        /// 获取称的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RSDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //--------------------------------------------------------
            // //  Thread.Sleep(20);
            // byte[] readBuffer = new byte[4096];
            // //int i= readBuffer.Length;
            // // Scripter.RS232Object.Read(readBuffer, 0,i);

            // //			string x= Encoding.ASCII.GetString(readBuffer);
            // if (sport == null)
            //     return;
            // string x = sport.ReadLine();
            // sport.DiscardOutBuffer();
            // x = x.Replace("\r\n", "");
            //// textWeight.Text = x;          
            //md.SetTextBoxText(textWeight,x);
            //----------------------------------------------------------

            if (Closing) return;
            //如果正在关闭，忽略操作，直接返回，      尽快的完成串口监听线程的一次循环
            try
            {
                Listening = true;
                //设置标记，说明我已经开始处理数据，                一会儿要使用系统UI的。   
                //int n = sport.BytesToRead;
                //先记录下来，避免某种原因，人为的原因，             操作几次之间时间长，缓存不一致
                //byte[] buf = new byte[n];
                ////声明一个临时数组存储当前来的串口数据                
                //sport.Read(buf, 0, n);//读取缓冲数据   
                //builder.Clear();//清除字符串构造器的内容   
                string x = sport.ReadLine();
                //因为要访问ui资源，所以需要使用invoke方式同步ui
                this.Invoke((EventHandler)(delegate
                {
                    textWeight.Text = x.Replace("\r\n", "");
                    // md.SetRichTextBoxText(richTextBox1,builder.ToString()+"\n",false);bu                  

                }));
            }
            finally
            {
                Listening = false;//我用完了，ui可以关闭串口了。   
            }


        }




        /// <summary>
        /// 获取包装线的配置信息
        /// </summary>
        /// <param name="packNO"></param>
        /// <returns></returns>
        bool GetConfigInfo(string packNO)
        {
            DataSet ds = adc.GetDataSetWithSQLString(WCFADD, string.Format(sqlHelp.GetConfig, packNO));
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[ds.Tables.Count - 1].Rows.Count > 0)
            {
                _packConfig.ip = ds.Tables[ds.Tables.Count - 1].Rows[0]["CountModuleIP"].ToString();
                _packConfig.PortName = ds.Tables[ds.Tables.Count - 1].Rows[0]["PortName"].ToString();
                _packConfig.StandardPCSUpperLimit = 0;
                _packConfig.LotSNAmount = float.Parse(ds.Tables[ds.Tables.Count - 1].Rows[0]["LotSNAmount"].ToString());
                try
                {
                    if (ds.Tables[ds.Tables.Count - 1].Rows[0]["StandardPCSUpperLimit"].ToString() != "")
                        _packConfig.StandardPCSUpperLimit = int.Parse(ds.Tables[ds.Tables.Count - 1].Rows[0]["StandardPCSUpperLimit"].ToString());
                }
                catch (Exception)
                {
                }

            }
            if (_packConfig.ip == "" || _packConfig.PortName == "" || _packConfig.StandardPCSUpperLimit == 0)
            {
                md.SetRichTextBoxText(richTextBox1, "配置信息不全，请联系管理员", false);
                return false;
            }
            md.SetRichTextBoxText(richTextBox1, "计数模块IP:" + _packConfig.ip + ";串口：" + _packConfig.PortName, false);
            return true;
        }

        string getw()
        {
            string str = "";
            Process[] processes;
            //Get the list of current active processes.
            processes = System.Diagnostics.Process.GetProcesses();
            //Grab some basic information for each process.
            Process process;
            for (int i = 0; i < processes.Length - 1; i++)
            {
                process = processes[i];
                if (process.ProcessName == "BrowserPro")
                {
                    str = process.MainModule.FileName.ToString();
                    return str;
                }
                if (process.ProcessName == "Browser")
                {
                    str = process.MainModule.FileName.ToString();
                    return str;
                }

            }
            return str;
        }

        /// <summary>
        /// 获取WCF的IP地址
        /// </summary>
        /// <returns></returns>
        public string GetCurrentPath()
        {
            string path1 = getw();//@"{0}Program Files (x86)\OrBit Systems Inc\OrBit-Browser Pro\BrowserPro.exe.config";

            path1 += ".config";
            if (File.Exists(path1))
            {
                return GetWCFIP(path1);
            }
            else
            {
                md.SetRichTextBoxText(richTextBox1, "没有找到WCF配置文件", true);
            }

            return "";

        }

        /// <summary>
        /// 解析出IP地址
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetWCFIP(string path)
        {
            string str = "";
            XmlDocument doc = new XmlDocument();
            doc.Load(path);    //加载Xml文件  
            XmlElement rootElem = doc.DocumentElement;   //获取根节点  
            XmlNodeList personNodes = rootElem.GetElementsByTagName("endpoint"); //获取person子节点集合  
            foreach (XmlNode node in personNodes)
            {
                str = ((XmlElement)node).GetAttribute("address");   //获取name属性值  
                if (str.Contains("http"))
                {
                    break;
                }
            }

            str = str.Replace("http://", "");
            str = str.Substring(0, str.IndexOf("/"));
            return str;
        }

        /// <summary>
        /// 窗体加载时发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            string wcfip = GetCurrentPath();
            if (wcfip == "")
            {
                wcfip = "33.0.1.4";
                md.SetRichTextBoxText(richTextBox1, "WCF地址默认为“33.0.1.4”", false);
            }
            WCFADD = string.Format(WCFADD, wcfip);

            _packConfig.PackNO = "Pack1";//Dns.GetHostName();
            md.SetRichTextBoxText(richTextBox1, "WCFIP:" + wcfip, false);

            GetFtpInfo();
            if (!GetConfigInfo(_packConfig.PackNO))
            {
                return;
            }
            Reset(_packConfig.ip);//康耐德网络开关的IP地址，需根据实际IP赋值
            sport = new SerialPort("COM1",//_packConfig.PortName, 
                9600, Parity.None, 8, StopBits.One);
            try
            {
                sport.DataReceived += new SerialDataReceivedEventHandler(RSDataReceived);
                if (sport.IsOpen == true)
                {
                    sport.Close();
                }
                sport.Open();
            }
            catch (Exception ex)
            {

                if (sport != null && sport.IsOpen)
                {
                    Closing = true;
                    while (Listening) Application.DoEvents();
                    //打开时点击，则关闭串口   
                    sport.Close();
                }
                MessageBox.Show(ex.Message);
                md.SetRichTextBoxText(richTextBox1, ex.Message, true);
            }



        }

        /// <summary>
        /// 打印标签
        /// </summary>
        /// <param name="lt">零头数量</param>
        /// <param name="gts">条码数量</param>
        bool printBQ(string lotsn, int lt, int gts)
        {
            decimal pieceWeight = NumpieceWeight.Value;//单重
            int LabelNum = int.Parse(label1.Text),//条码数量
            Realmaterialpcs = int.Parse(numMOQtyDonePcs.Text),//完成量
            MOQtyRequired = int.Parse(numMOQtyRequired.Text),//
            BZpcs = 0,// 标准数量           
            ltpcs = lt,//零头箱数量
            gttmNum = gts,//条码数量		
            amount = 0, //称重时的总数
            LotsnSUM = 0, //随工单的总数
            sxSUM = 0
            ;

            //textBZpcs.Text = "4000";
            if (textBZpcs.Text != "")
            {
                BZpcs = int.Parse(textBZpcs.Text);
            }
         
            amount = ltpcs > 0 ? ltpcs +  BZpcs *(gttmNum-1)  : gttmNum * BZpcs;
            LotsnSUM = int.Parse(DGVLotSN.Rows[0].Cells["pcs"].Value.ToString());
            sxSUM = Convert.ToInt32((LotsnSUM * _packConfig.LotSNAmount + LotsnSUM));
            if (amount > sxSUM)
            {
                MessageBox.Show("当前随工单称重数量" + amount + "以超过了随工单原数量的"+(_packConfig.LotSNAmount*100).ToString()+"%(" + sxSUM + ")");
                md.SetRichTextBoxText(richTextBox1, "当前随工单称重数量" + amount + "以超过了随工单原数量的" + (_packConfig.LotSNAmount * 100).ToString() + "%(" + sxSUM + ")", true);
                return false;
            }

            string sql = string.Format(sqlHelp.PrintBSD, lotsn, BZpcs, int.Parse(DGVLotSN.Rows[0].Cells["pcs"].Value.ToString())
                                                        , NumpieceWeight.Value.ToString(), comMOid.Text, ltpcs, gts, amount, "admin");
            //md.SetRichTextBoxText(richTextBox1, sql, true);
            //return false;
            DataSet ds = adc.GetDataSetWithSQLString(WCFADD, sql);
            if (ds.Tables[ds.Tables.Count - 1].Rows[0]["Return Value"].ToString() == "-1")
            {
                MessageBox.Show(ds.Tables[ds.Tables.Count - 1].Rows[0]["@I_ReturnMessage"].ToString());
                md.SetRichTextBoxText(richTextBox1, ds.Tables[ds.Tables.Count - 1].Rows[0]["@I_ReturnMessage"].ToString(), true);
                return false;
            }
            ds.Tables.Remove(ds.Tables[ds.Tables.Count - 1]);
            ds.Tables.Remove(ds.Tables[ds.Tables.Count - 1]);
            DataTable ftpdt = adc.GetDataSetWithSQLString(WCFADD, sqlHelp.GetFtpFile).Tables[0];

            string FTPid = ftpdt.Rows[0][0].ToString();

            string FtpDirectory = ftpdt.Rows[0][1].ToString();


            if (PrintMRZ.DownloadFtp(temp, FTPid, FTP.ftpDIR + "/" + FtpDirectory + "/" + FTPid, FTP.ftpip, FTP.ftpUNM, FTP.ftpPWD) == -2)
            {
                MessageBox.Show("标签文件下载失败，请联系管理员");
                md.SetRichTextBoxText(richTextBox1, "标签文件下载失败，请联系管理员", true);
                return false;
            }
            md.SetRichTextBoxText(richTextBox1, PrintMRZ.printStimulsoftReports(FTPid, ds, true), false);
            return true;
        }

        /// <summary>
        /// 打印随工单的客户条码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param
        private void btnPrint_Click_1(object sender, EventArgs e)
        {
            if (mCount == 0)
                return;
            int lt = 0;
            DataSet ds = adc.GetDataSetWithSQLString(WCFADD, string.Format(sqlHelp.GetLTPCS, textLotSN.Text));
            if (ds.Tables.Count <= 0)
                return;
            if (ds.Tables[ds.Tables.Count - 1].Rows.Count > 0 && ds.Tables[0].Rows[0][0].ToString() != "")
                lt = int.Parse(ds.Tables[0].Rows[0][0].ToString());
            md.SetRichTextBoxText(richTextBox1, "即将打印[" + textLotSN.Text + "]的[" + mCount + "]张客户条码", false);
            if (printBQ(textLotSN.Text, lt, int.Parse(label1.Text)))
            {
                mCount = 0;
                PostMessage("0");
                oldLotsn = "";
                olddz = 0;
            }

        }


        /// <summary>
        /// 保存单重信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void butSetdz_Click(object sender, EventArgs e)
        {
            if (butSetdz.Text == "设置")
            {
                butSetdz.Text = "保存";
                butClearDZ.Visible = true;
                NumpieceWeight.Enabled = true;
                olddz = decimal.Parse(NumpieceWeight.Value.ToString());
                return;
            }

            if (butSetdz.Text == "保存")
            {
                if (NumpieceWeight.Value.ToString() == "0")
                {
                    MessageBox.Show("单重不能为0", "系统提示");
                    md.SetRichTextBoxText(richTextBox1, "单重不能为0", true);
                    return;
                }
                if (DGVLotSN.Rows.Count <= 0 || DGVLotSN.Rows[0].Cells["ProductNO"].ToString() == "")
                {
                    MessageBox.Show("没有料号");
                    md.SetRichTextBoxText(richTextBox1, "没有料号", true);
                    return;
                }
                string sql = string.Format(sqlHelp.SetpieceWeight, decimal.Parse(NumpieceWeight.Value.ToString()), DGVLotSN.Rows[0].Cells["ProductNO"].Value.ToString());
                adc.GetDataSetWithSQLString(WCFADD, sql);
                butSetdz.Text = "设置";
                butClearDZ.Visible = false;
                NumpieceWeight.Enabled = false;

            }
        }
        /// <summary>
        /// 取消修改单重
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void butClearDZ_Click(object sender, EventArgs e)
        {
            butSetdz.Text = "设置";
            butClearDZ.Visible = false;
            NumpieceWeight.Enabled = false;
            NumpieceWeight.Value = olddz;
        }

        double D_weight = 0;
        /// <summary>
        /// 获取电子称数据后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textWeight_TextChanged(object sender, EventArgs e)
        {
            string weight = textWeight.Text.ToLower();
            if (weight.Length < 2)
                return;
            bool iskg = false;

            if (weight.Contains("g") && !weight.Contains("k"))
            {
                D_weight = WeightSubstring(weight, "g");
                iskg = false;
            }
            if (weight.Contains("kg"))
            {
                D_weight = WeightSubstring(weight, "k");
                iskg = true;

            }

            if (fspw != null)
            {
                fspw.Do_SetD_wieght(D_weight, textWeight.Text);
            }

            if (decimal.Parse(NumpieceWeight.Value.ToString()) == 0)
            {
                md.SetRichTextBoxText(richTextBox1, "没有单重", true);
                return;
            }

            if (iskg)
            {
                textPCS.Text = Convert.ToInt32(D_weight / (Convert.ToDouble(NumpieceWeight.Value.ToString()) / 1000)).ToString();
            }
            else
            {
                textPCS.Text = Convert.ToInt32((D_weight / (Convert.ToDouble(NumpieceWeight.Value.ToString())))).ToString();
            }




        }

        /// <summary>
        /// 获取重量
        /// </summary>
        /// <param name="str_weight">带字符的重量</param>
        /// <param name="str">需要去掉的字符</param>
        /// <returns></returns>
        double WeightSubstring(string str_weight, string str)
        {
            string weight_str = str_weight;
            double d = 0;
            weight_str = weight_str.Substring(0, weight_str.LastIndexOf(str));
            if (weight_str.Contains("-"))
            {
                weight_str = "0";
            }
            try
            {
                d = double.Parse(weight_str);
            }
            catch (Exception)
            {
                d = 0;
            }
            return d;
        }

        /// <summary>
        /// 根据数量判断是否在正常范围内
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textPCS_TextChanged(object sender, EventArgs e)
        {
            int int_pcs = 0, int_bzpcs = 0;
            if (textPCS.Text == "")
                int_pcs = 0;
            else
            {
                int_pcs = int.Parse(textPCS.Text);
            }
            if (textBZpcs.Text == "")
            {
                md.SetRichTextBoxText(richTextBox1, "请维护标准包装数量", true);
                return;
            }
            else
            {
                int_bzpcs = int.Parse(textBZpcs.Text);
            }
            if (int_pcs < int_bzpcs)
            {
                md.SetTextBoxColor(textPCS, Color.Blue);
                md.SetTextBoxColor(textWeight, Color.Blue);
            }
            if (int_pcs > int_bzpcs + _packConfig.StandardPCSUpperLimit)
            {
                md.SetTextBoxColor(textPCS, Color.Red);
                md.SetTextBoxColor(textWeight, Color.Red);
            }

            if (int_pcs <= int_bzpcs + 5 && int_pcs >= int_bzpcs)
            {
                md.SetTextBoxColor(textPCS, Color.Lime);
                md.SetTextBoxColor(textWeight, Color.Lime);
            }
        }

        /// <summary>
        /// 修改计数器的计数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            FormSetmCount f = new FormSetmCount(mCount);
            if (f.ShowDialog() == DialogResult.OK)
            {
                mCount = f.SetValue;
                Connected Data = new Connected(PostMessage);
                this.BeginInvoke(Data, mCount.ToString());
                f.Close();
            }

        }

        /// <summary>
        /// 在平台里面取消工具栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Shown(object sender, EventArgs e)
        {
            //SendKeys.SendWait("{^t}");
            SendKeys.Send("^t");
        }

        private void butShowSetPWF_Click(object sender, EventArgs e)
        {

            fspw = new FormSetPieceWeight(D_weight, textWeight.Text);
            if (fspw.ShowDialog() == DialogResult.OK)
            {
                double PieceWeight = fspw.D_PieceWeight;
                if (DGVLotSN.Rows.Count <= 0 || DGVLotSN.Rows[0].Cells["ProductNO"].ToString() == "")
                {
                    MessageBox.Show("没有料号");
                    md.SetRichTextBoxText(richTextBox1, "没有料号", true);
                    return;
                }
                NumpieceWeight.Value = decimal.Parse(PieceWeight.ToString());
                string sql = string.Format(sqlHelp.SetpieceWeight, PieceWeight, DGVLotSN.Rows[0].Cells["ProductNO"].Value.ToString());
                adc.GetDataSetWithSQLString(WCFADD, sql);
                fspw.Close();
            }
        }

        /// <summary>
        /// 保存零头重量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void butSaveLtWeight_Click(object sender, EventArgs e)
        {

            string lotsn = textLotSN.Text;
            int pcs = 0;
            if (textPCS.Text != "")
                pcs = int.Parse(textPCS.Text);
            if (D_weight <= 0 || pcs <= 0)
            {
                md.SetRichTextBoxText(richTextBox1, "重量或者数量不能为0", true);
                return;
            }

            if (lotsn == "")
            {
                md.SetRichTextBoxText(richTextBox1, "随工单不能为空", true);
                return;

            }

            string sql = string.Format(sqlHelp.InsertLotSnWeigh, lotsn, pcs, 0, D_weight);
            DataSet ds = adc.GetDataSetWithSQLString(WCFADD, sql);
            if (ds.Tables[ds.Tables.Count - 1].Rows[0]["Return Value"].ToString() == "-1")
            {
                MessageBox.Show(ds.Tables[ds.Tables.Count - 1].Rows[0]["@I_ReturnMessage"].ToString());
                md.SetRichTextBoxText(richTextBox1, ds.Tables[ds.Tables.Count - 1].Rows[0]["@I_ReturnMessage"].ToString(), true);
                return;
            }
            if (ds.Tables[ds.Tables.Count - 1].Rows[0]["Return Value"].ToString() == "1")
            {
                md.SetRichTextBoxText(richTextBox1, ds.Tables[ds.Tables.Count - 1].Rows[0]["@I_ReturnMessage"].ToString(), false);
                if (MessageBox.Show(ds.Tables[ds.Tables.Count - 1].Rows[0]["@I_ReturnMessage"].ToString(), "系统提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    md.SetRichTextBoxText(richTextBox1, "你选择覆盖原先的零头重量", false);
                    sql = string.Format(sqlHelp.InsertLotSnWeigh, lotsn, pcs, 1, D_weight);
                    ds = adc.GetDataSetWithSQLString(WCFADD, sql);
                    if (ds.Tables[ds.Tables.Count - 1].Rows[0]["Return Value"].ToString() == "-1")
                    {
                        MessageBox.Show(ds.Tables[ds.Tables.Count - 1].Rows[0]["@I_ReturnMessage"].ToString());
                        md.SetRichTextBoxText(richTextBox1, ds.Tables[ds.Tables.Count - 1].Rows[0]["@I_ReturnMessage"].ToString(), true);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            md.SetRichTextBoxText(richTextBox1, "写入成功", false);

        }


    }
}
