using AForge.Video.DirectShow;
using CZIDCardReader;
using JSCardKiosks;
using MediaPlayerHelper;
using Microsoft.Win32;
using Mytest;
using QrCodeScan;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using WorkLog;

namespace Ycps10
{
    
    public partial class MainForm : Form
    {
        int iTime = 20;
        Sunisoft.IrisSkin.SkinEngine s;
        private DeviceControlHelper m_device;
        string strPath = AppDomain.CurrentDomain.BaseDirectory + "IDPhoto\\";
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoDevice;
        bool bRead = false;
        public MainForm()
        {
            InitializeComponent();
            s = new Sunisoft.IrisSkin.SkinEngine();
            m_device = new DeviceControlHelper();
            //可以获取串口号数组
            foreach (string portname in m_device.PortNameArr)//遍历串口号数组
            {
                comboBox1.Items.Add(portname);
            }
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            int xWidth1 = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;//获取显示器屏幕宽度
            int yHeight1 = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height;//高度
            s.SkinFile = Application.StartupPath + @"/"+DealAppConfig.GetAppSettingsValue("风格") + ".ssk";
            checkBoxIC.Checked = (DealAppConfig.GetAppSettingsValue("IC") == "Y");
            checkBoxPrint.Checked = (DealAppConfig.GetAppSettingsValue("打印机") == "Y");
            checkBoxIDRead.Checked = (DealAppConfig.GetAppSettingsValue("身份证读取") == "Y");
            checkBoxIDScan.Checked = (DealAppConfig.GetAppSettingsValue("身份证扫描") == "Y");
            checkBoxCamera.Checked = (DealAppConfig.GetAppSettingsValue("摄像头") == "Y");
            checkBoxQRRead.Checked = (DealAppConfig.GetAppSettingsValue("二维码") == "Y");
            txtUrl1.Text = (DealAppConfig.GetAppSettingsValue("市卡管接口地址"));
            txtUrl2.Text = (DealAppConfig.GetAppSettingsValue("省卡管接口地址"));

        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void checkBoxCamera_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxIC.Checked)
                DealAppConfig.UpdateAppSettings("IC", "Y");
            else
                DealAppConfig.UpdateAppSettings("IC", "N");

            if (checkBoxPrint.Checked)
                DealAppConfig.UpdateAppSettings("打印机", "Y");
            else
                DealAppConfig.UpdateAppSettings("打印机", "N");
            if (checkBoxIDRead.Checked)
                DealAppConfig.UpdateAppSettings("身份证读取", "Y");
            else
                DealAppConfig.UpdateAppSettings("身份证读取", "N");
            if (checkBoxIDScan.Checked)
                DealAppConfig.UpdateAppSettings("身份证扫描", "Y");
            if (checkBoxCamera.Checked)
                DealAppConfig.UpdateAppSettings("摄像头", "Y");
            else
                DealAppConfig.UpdateAppSettings("摄像头", "N");
            if (checkBoxQRRead.Checked)
                DealAppConfig.UpdateAppSettings("二维码", "Y");
            else
                DealAppConfig.UpdateAppSettings("二维码", "N");
        }

        private void btnOneKey_Click(object sender, EventArgs e)
        {
            #region 清空temp+身份证扫描图片
            picBack.Image = null;
            picFront.Image = null;
            DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "temp");
            if (di.Exists)
                PublicHelp.DelectDir(AppDomain.CurrentDomain.BaseDirectory + "temp");
            #endregion
            #region 清空 身份证读取信息
            txtIDNur.Text = "";
            txtName.Text = "";
            di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "IDPhoto");
            if (di.Exists)
                di.Delete(true);
            picPhoto.Image = null;
            #endregion
            timer1.Enabled = true;
            iTime = 20;
            labRetTime.Text = iTime.ToString();
            bRead = false;
            btnCancel.Visible = true;
            pictureGIf.Visible = true;
            labRetTime.Visible = true;
            int iRet = ReadIDBadge.IDReaderOpen(AppDomain.CurrentDomain.BaseDirectory + "temp");
            if (iRet != 0)
            {
                WriteWorkLog.WriteLogs("程序运行日志", "错误", "身份证阅读器打开失败");
                listBox1.Items.Add("身份证阅读器打开失败");
                return;
            }
            ReadIDBadge.CardInEnable();
            backgroundWorker1.RunWorkerAsync();

        }

        private void btnReadIDNur_Click(object sender, EventArgs e)
        {
            txtIDNur2.Text = "";
            txtName2.Text = "";
            strPath = AppDomain.CurrentDomain.BaseDirectory + "IDPhoto\\";

            DirectoryInfo di = new DirectoryInfo(strPath.Substring(0, strPath.Length - 1));
            if (di.Exists)
                di.Delete(true);
            picPhoto2.Image = null;
            string strPath2 = AppDomain.CurrentDomain.BaseDirectory + "temp";
            int iRet = ReadIDBadge.IDReaderOpen(strPath2);

            if (iRet != 0)
            {
                WriteWorkLog.WriteLogs("日志", "错误", "身份证阅读器打开失败");
                return;
            }
            ReadIDBadge.CardInEnable();

            _IDS_STATUS_RESULT pResult = new _IDS_STATUS_RESULT();
            iRet = ReadIDBadge.GetStatus(_IDS_DEV_STATUS_TYPE._IDS_DEV_STATUS_TYPE_DS, out pResult);
            if (pResult._Sensor.ulCOVER == 12)
            {
                CardInfo IDInfo = new CardInfo();
                iRet = ReadIDBadge.IDCardRead(strPath2, out IDInfo);
                if (iRet != 0)
                {
                    listBox3.Items.Add("身份证阅读器打开失败");
                    WriteWorkLog.WriteLogs("日志", "错误", "身份证阅读器打开失败");
                    ReadIDBadge.CardEject();
                    //  ReadIDBadge.CardInDisable();
                    iRet = ReadIDBadge.Uninit();

                    return;
                }
                txtIDNur2.Text = IDInfo.IDNumber;
                txtName2.Text = IDInfo.name;
                strPath += IDInfo.IDNumber.Trim() + "_r.jpg";
                if (File.Exists(strPath))
                {
                    using (FileStream file = new FileStream(strPath, FileMode.Open))
                    {
                        byte[] b = new byte[file.Length];
                        file.Read(b, 0, b.Length);
                        MemoryStream stream = new MemoryStream(b);
                        picPhoto2.Image = Image.FromStream(stream);
                    }
                }
                // ReadIDBadge.CardInDisable();
                //ReadIDBadge.CardEject();
                //ReadIDBadge.CardInDisable();
                //iRet = ReadIDBadge.Uninit();
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            picBack2.Image = null;
            picFront2.Image = null;
            DirectoryInfo di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "temp");
            if (di.Exists)
                PublicHelp.DelectDir(AppDomain.CurrentDomain.BaseDirectory + "temp");
            if (!File.Exists(strPath))
            {
                listBox1.Items.Add("请先点读取身份证");
                return;
            }
            int iRet = ReadIDBadge.IDCardScan();
            if (iRet != 0)
            {
                listBox1.Items.Add("扫描身份证失败");
                ReadIDBadge.CardEject();
                iRet = ReadIDBadge.Uninit();

                return;
            }
            string strFront = AppDomain.CurrentDomain.BaseDirectory + "temp" + "//front.bmp";
            string strBack = AppDomain.CurrentDomain.BaseDirectory + "temp" + "//back.bmp";
            if (File.Exists(strFront))
            {
                using (FileStream file = new FileStream(strFront, FileMode.Open))
                {
                    byte[] b = new byte[file.Length];
                    file.Read(b, 0, b.Length);
                    MemoryStream stream = new MemoryStream(b);
                    picFront2.Image = Image.FromStream(stream);
                }
                using (FileStream file = new FileStream(strBack, FileMode.Open))
                {
                    byte[] b = new byte[file.Length];
                    file.Read(b, 0, b.Length);
                    MemoryStream stream = new MemoryStream(b);
                    picBack2.Image = Image.FromStream(stream);
                }

            }
            ReadIDBadge.CardEject();
            iRet = ReadIDBadge.Uninit();
        }

        private void btnIdEject_Click(object sender, EventArgs e)
        {
            string strPath2 = AppDomain.CurrentDomain.BaseDirectory + "temp";
            int iRet = ReadIDBadge.IDReaderOpen(strPath2);
            ReadIDBadge.CardEject();
            iRet = ReadIDBadge.Uninit();
        }



        private void btnAutoSan_Click(object sender, EventArgs e)
        {
            if (m_device.PortState)
            {
                m_device.CloseDevice();
            }
            else
            {
                //打开串口 
                if (!m_device.OpenDevice(comboBox1.SelectedItem.ToString()))
                {
                    listBox4.Items.Add("打开二维码端口失败");
                }
            }
            timer4.Enabled = true;
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            if (m_device.PortState)
            {
                //下发扫描二维码命令
                if (m_device.ScanQrCode())
                {
                    Mp3Player player = new Mp3Player();
                    player.FileName = "二维码扫描成功.mp3";
                    player.play();
                    listBox4.Items.Add(m_device.strResult);
                    listBoxQR.Items.Add(m_device.strResult);
                    timer4.Enabled = false;
                }
                    
            }
        }

        private void btnCardIn_Click(object sender, EventArgs e)
        {
            int iRet = PrintControl.IDPCardIn();
            if (iRet > 0)
            {
                listBox2.Items.Add("进卡成功，剩余色带" + iRet.ToString());
                Mp3Player player = new Mp3Player();
                player.FileName = "进卡成功.mp3";
                player.play();
            }
            else
            {
                listBox2.Items.Add("进卡失败" );
                Mp3Player player = new Mp3Player();
                player.FileName = "进卡失败.mp3";
                player.play();
            }
               
        }

        private void btnCardOut_Click(object sender, EventArgs e)
        {
            int iRet = PrintControl.IDPCardOut();
            listBox2.Items.Add(iRet.ToString());
        }

        private void btnCardKeep_Click(object sender, EventArgs e)
        {
            int iRet = PrintControl.IDPCardReBack(700);
            listBox2.Items.Add(iRet.ToString());
        }

        private void btnStatus_Click(object sender, EventArgs e)
        {
            bool bEmpty = false;
            bool bCardIn = false;
            UInt64 status = 0;
            int iRet = PrintControl.GetPrinterStatus(ref status);
            listBox2.Items.Add(iRet.ToString());
            listBox2.Items.Add("打印机状态码：" + String.Format("{0,16:X16}", status));
        }

        private void btnReboot_Click(object sender, EventArgs e)
        {
            int iRet = PrintControl.IDP51Reboot();
            listBox2.Items.Add(iRet.ToString());
        }

        private void btnPrintTest_Click(object sender, EventArgs e)
        {
            int iRet = PrintControl.IDPCardPrint("张莉|320282199003071812|123456789|2020年11月|320282196803071812|sfgmtv*51PrintConfig3.ini");//姓名，身份证号，卡号，发卡日期
           if(iRet==0)
            {
                Mp3Player player = new Mp3Player();
                player.FileName = "打印成功.mp3";
                player.play();
                listBox2.Items.Add("打印成功 ");
            }
           else
            {
                Mp3Player player = new Mp3Player();
                player.FileName = "打印失败.mp3";
                player.play();
                listBox2.Items.Add("打印失败 ");
            }
            
        }

        private void btnICRead_Click(object sender, EventArgs e)
        {
            string resetCode = "";
            int iRet = CardReadT10.ReadCard(out resetCode);
            if (iRet == 0)
            {
                Mp3Player player = new Mp3Player();
                player.FileName = "复位成功.mp3";
                player.play();
                listBox2.Items.Add("IC复位成功 " );
                listBox2.Items.Add("复位信息：" + resetCode);
            }
            else
            {
                if(iRet == -1)
                {

                }
                Mp3Player player = new Mp3Player();
                player.FileName = "复位失败.mp3";
                player.play();
                listBox2.Items.Add("IC复位失败 ");
            }
           
        }

        private void btnOpenCamera_Click(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count != 0)
            {
                for (int i = 0; i < videoDevices.Count; i++)
                {
                    string cameraName = videoDevices[i].Name;
                    //if (cameraName == "YC-FI3105C")
                    //{
                    //    videoDevice = new VideoCaptureDevice(videoDevices[i].MonikerString);
                    //    break;
                    //}
                    videoDevice = new VideoCaptureDevice(videoDevices[i].MonikerString);
                }
            }
            else
            {
                MessageBox.Show("没有找到摄像头", "ABC");//
                return;
            }
            videoDevice.VideoResolution = videoDevice.VideoCapabilities[0];
            showVideo();
        }
        private int showVideo()
        {
            if (videoDevice != null)
            {
                vispShoot.VideoSource = videoDevice;
                vispShoot.Start();
            }
            return 0;
        }
        private int showVideo1()
        {
            if (videoDevice != null)
            {
                vispShoot1.VideoSource = videoDevice;
                vispShoot1.Start();
            }
            return 0;
        }
        private void btnCloseCamera_Click(object sender, EventArgs e)
        {
            vispShoot.Stop();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!bRead)
            {
                System.Threading.Thread.Sleep(200);
                _IDS_STATUS_RESULT pResult = new _IDS_STATUS_RESULT();
                int iRet = ReadIDBadge.GetStatus(_IDS_DEV_STATUS_TYPE._IDS_DEV_STATUS_TYPE_DS, out pResult);
                if (pResult._Sensor.ulCOVER == 12|| pResult._Sensor.ulCOVER == 16|| pResult._Sensor.ulCOVER == 2)
                {
                    CardInfo IDInfo = new CardInfo();
                    IDInfo.bScan = false;
                    iRet = ReadIDBadge.IDCardRead(AppDomain.CurrentDomain.BaseDirectory+"temp", out IDInfo);
                    if (iRet == 0 && (IDInfo.name != ""&& IDInfo.name != null))
                    {
                        backgroundWorker1.ReportProgress(0, IDInfo);
                        try
                        {
                            iRet = ReadIDBadge.IDCardScan();
                        }
                        catch (Exception ex)
                        {

                            WriteWorkLog.WriteLogs("程序运行日志", "异常", "扫描证件失败" + ex.ToString());
                            IDInfo.name = "";
                        }

                    }
                    else
                    {
                        IDInfo.bScan = false;
                        backgroundWorker1.ReportProgress(0, IDInfo);
                       
                    }
                    if (iRet != 0)
                    {
                        WriteWorkLog.WriteLogs("程序运行日志", "错误", "扫描证件失败" + iRet.ToString());
                        IDInfo.bScan = false;
                    }
                    else
                    {
                        IDInfo.bScan = true;
                    }
                    backgroundWorker1.ReportProgress(1, IDInfo);



                }
            }
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            CardInfo IDInfo = new CardInfo();
            if (e.ProgressPercentage == 0)
            {
                btnCancel.Visible = false;
                 IDInfo = (CardInfo)e.UserState;
                if (IDInfo.name != ""&& IDInfo.name != null)
                {
                    bRead = true;
                    timer1.Enabled = false;
                    btnCancel.Visible = false;
                    pictureGIf.Visible = false;
                    labRetTime.Visible = false;
                    txtName.Text = IDInfo.name.Trim();
                    txtIDNur.Text = IDInfo.IDNumber.Trim();
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "IDPhoto\\" + IDInfo.IDNumber.Trim() + "_r.jpg"))
                    {
                        using (FileStream file = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "IDPhoto\\" + IDInfo.IDNumber.Trim() + "_r.jpg", FileMode.Open))
                        {
                            byte[] b = new byte[file.Length];
                            file.Read(b, 0, b.Length);
                            MemoryStream stream = new MemoryStream(b);
                            picPhoto.Image = Image.FromStream(stream);
                        }
                        SoundPlayer player = new SoundPlayer();
                        player.SoundLocation = Application.StartupPath + "\\身份证读取成功.wav";
                        player.Load();
                        player.Play();
                    }
                    else
                    {
                        bRead = true;
                        btnCancel.Visible = false;
                        pictureGIf.Visible = false;
                        labRetTime.Visible = false;
                        ReadIDBadge.CardEject();
                        ReadIDBadge.Uninit();
                        timer1.Enabled = false;
                        backgroundWorker1.CancelAsync();
                        SoundPlayer player = new SoundPlayer();
                        player.SoundLocation = Application.StartupPath + "\\身份证读取失败.wav";
                        player.Load();
                        player.Play();
                    }
                    
                  
                    
                }
                else
                {
                    bRead = true;
                    btnCancel.Visible = false;
                    pictureGIf.Visible = false;
                    labRetTime.Visible = false;
                    ReadIDBadge.CardEject();
                    ReadIDBadge.Uninit();
                    timer1.Enabled = false;
                    backgroundWorker1.CancelAsync();
                    SoundPlayer player = new SoundPlayer();
                    player.SoundLocation = Application.StartupPath + "\\身份证读取失败.wav";
                    player.Load();
                    player.Play();
                }
                return;
            }
            if (e.ProgressPercentage == 1  )
            {
                
                IDInfo = (CardInfo)e.UserState;
                if (IDInfo.name == "" || IDInfo.name == null)
                    return;
                if(IDInfo.bScan)
                {
                    string strFront = AppDomain.CurrentDomain.BaseDirectory + "temp" + "//front.bmp";
                    string strBack = AppDomain.CurrentDomain.BaseDirectory + "temp" + "//back.bmp";
                    if (File.Exists(strFront))
                    {
                        using (FileStream file = new FileStream(strFront, FileMode.Open))
                        {
                            byte[] b = new byte[file.Length];
                            file.Read(b, 0, b.Length);
                            MemoryStream stream = new MemoryStream(b);
                            picFront.Image = Image.FromStream(stream);
                        }
                        using (FileStream file = new FileStream(strBack, FileMode.Open))
                        {
                            byte[] b = new byte[file.Length];
                            file.Read(b, 0, b.Length);
                            MemoryStream stream = new MemoryStream(b);
                            picBack.Image = Image.FromStream(stream);
                        }
                        bRead = true;
                        Mp3Player player = new Mp3Player();
                        player.FileName = "身份证扫描成功.mp3";
                        player.play();
                        btnCancel.Visible = false;
                        pictureGIf.Visible = false;
                        labRetTime.Visible = false;
                        ReadIDBadge.CardEject();
                        ReadIDBadge.Uninit();
                        timer1.Enabled = false;
                       
                        backgroundWorker1.CancelAsync();
                    }
                    else
                    {
                        btnCancel.Visible = false;
                        pictureGIf.Visible = false;
                        labRetTime.Visible = false;
                        ReadIDBadge.CardEject();
                        ReadIDBadge.Uninit();
                        timer1.Enabled = false;
                        bRead = true;
                        backgroundWorker1.CancelAsync();
                        Mp3Player player = new Mp3Player();
                        player.FileName = "身份证扫描失败.mp3";
                        player.play();
                    }
              
                }
                else
                {
                    btnCancel.Visible = false;
                    pictureGIf.Visible = false;
                    labRetTime.Visible = false;
                    ReadIDBadge.CardEject();
                    ReadIDBadge.Uninit();
                    timer1.Enabled = false;
                    bRead = true;
                    backgroundWorker1.CancelAsync();
                    Mp3Player player = new Mp3Player();
                    player.FileName = "身份证扫描失败.mp3";
                    player.play();
                }
               
            }
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Mp3Player player = new Mp3Player();
            int iRet = 0;
            if (checkBoxIC.Checked||checkBoxPrint.Checked)
            {
                iRet = PrintControl.IDPCardIn();
                if (iRet > 0)
                {
                    player.FileName = "进卡成功.mp3";
                    player.play();
                    listBox1.Items.Add("进卡成功，剩余色带" + iRet.ToString());
                }
                else
                {
                    player.FileName = "进卡失败.mp3";
                    player.play();
                    listBox1.Items.Add("进卡失败");
                }
            }
           
            string resetCode = "";
            if (checkBoxIC.Checked)
            {
                iRet = CardReadT10.ReadCard(out resetCode);
                if (iRet == 0)
                {
                    Mp3Player player1 = new Mp3Player();
                    player1.FileName = "复位成功.mp3";
                    player1.play();
                    listBox1.Items.Add("IC复位返回：" + iRet.ToString());
                    listBox1.Items.Add("复位信息：" + resetCode);
                }
                else
                {
                                         
                    player.FileName = "复位失败.mp3";
                    player.play();
                    listBox2.Items.Add("复位信息失败");
                }
            }
                
            if(checkBoxPrint.Checked)
            {
                iRet = PrintControl.IDPCardPrint("张莉|320282199003071812|123456789|2020年11月|320282196803071812|sfgmtv*51PrintConfig3.ini");//姓名，身份证号，卡号，发卡日期
               
                if (iRet == 0)
                {
                    listBox1.Items.Add(iRet.ToString());
                }
                else
                {
                    player.FileName = "打印失败.mp3";
                    player.play();
                    listBox2.Items.Add("复位信息失败");
                }
                
            }

            if (checkBoxIC.Checked || checkBoxPrint.Checked)
                PrintControl.IDPCardReBack(700);

            if (checkBoxQRRead.Checked  )
            {
                if (m_device.PortState)
                {
                    m_device.CloseDevice();
                }
                else
                {
                    //打开串口 
                    if(comboBox1.SelectedItem==null)
                    {
                        listBox4.Items.Add("打开二维码端口失败");
                        listBox1.Items.Add("打开二维码端口失败");
                        timer4.Enabled =false;
                    }
                    else if (!m_device.OpenDevice(comboBox1.SelectedItem.ToString()))
                    {
                        listBox4.Items.Add("打开二维码端口失败");
                        listBox1.Items.Add("打开二维码端口失败");
                        timer4.Enabled = false;
                    }
                    else
                    {
                        timer4.Enabled = true;
                    }
                }
               
            }
               
            if(checkBoxCamera.Checked)
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count != 0)
                {
                    for (int i = 0; i < videoDevices.Count; i++)
                    {
                        string cameraName = videoDevices[i].Name;
                        //if (cameraName == "YC-FI3105C")
                        //{
                        //    videoDevice = new VideoCaptureDevice(videoDevices[i].MonikerString);
                        //    break;
                        //}
                        videoDevice = new VideoCaptureDevice(videoDevices[i].MonikerString);
                    }
                }
                else
                {
                    MessageBox.Show("没有找到摄像头", "ABC");//
                    return;
                }
                videoDevice.VideoResolution = videoDevice.VideoCapabilities[0];
                showVideo1();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            iTime--;
            labRetTime.Text = iTime.ToString().PadLeft(2, '0');

            if (iTime <= 0)
            {
                btnCancel.Visible = false;
                pictureGIf.Visible = false;
                labRetTime.Visible = false;
                ReadIDBadge.Uninit();
                timer1.Enabled = false;
                bRead = true;
                backgroundWorker1.CancelAsync();
                //mainForm.ShowChildForm("JSSYCardKiosks.MainPal", 0, new string[] { "", "" });
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            btnCancel.Visible = false;
            pictureGIf.Visible = false;
            labRetTime.Visible = false;
            bRead = true;
            ReadIDBadge.Uninit();
            backgroundWorker1.CancelAsync();
            timer1.Enabled = false;
        }

        private void btnPrintTab1_Click(object sender, EventArgs e) 
        {
            //RegistryKey regkeySetKey = Registry.CurrentUser.OpenSubKey(@"Software", true).CreateSubKey(@"科颐\sbkPrinter\DeviceInfo");
            //regkeySetKey.SetValue("设备型号", "CW-YCPKS10");
            //regkeySetKey.SetValue("设备编号", "ZJCWKS10010");
            //RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"Software\科颐\sbkPrinter\DeviceInfo");
            //txtIDNur.Text = regkey.GetValue("设备编号").ToString();
            System.Diagnostics.Process.Start(" http://www.baidu.com");
        }

        private void labRegister_DoubleClick(object sender, EventArgs e)
        {
            RegiserDeviceID regiserDeviceID = new RegiserDeviceID(); ;
            regiserDeviceID.ShowDialog();
        }

        private void labRegister_Click(object sender, EventArgs e)
        {

        }

        private void btnOpenUrl1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(txtUrl1.Text);
        }

        private void btnOpenUrl2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(txtUrl2.Text+"?wsdl");
        }
    }
}
