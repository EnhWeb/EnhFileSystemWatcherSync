/*------------------------------------------------
// YooKee
// Copyright (C) 2006-2008 
//
// 文件名:Form1.cs
// 文件功能描述：镜像服务器文件实时同步程序
//
// 创建人：Sunshine
// 创建日期: 2007.12.11
// 
// 修改标识:2007.12.13
// 修改描述:增加通过XML操作功能
//
 ------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Xml;

namespace FileSystemWatcher
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 记录文件更新总数
        /// </summary>
        private int sum = 0;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 点击开始监视按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBegin_Click(object sender, EventArgs e)
        {
            //要监视的目录
            string sFolder = txtFolder.Text.Trim();
            myWatcher.EnableRaisingEvents = true;
            myWatcher.Path = sFolder;
            myWatcher.IncludeSubdirectories = true;
            myWatcher.NotifyFilter = NotifyFilters.Size;
            lblMessage.Text = "正在监视中...";

        }

        /// <summary>
        /// 当文件改变时进行操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void myWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                return;
            }

            if (e.Name.Length > 12)
            {
                string flvFile = e.Name.Substring(0, 11);

                if (flvFile == "UploadVideo")
                {
                    return;
                }
            }

            System.IO.FileInfo fileinfo = new FileInfo(e.FullPath);

            int i = 1;
        Finish:
            try
            {
                if (i < 10)
                {
                    fileinfo.OpenRead();
                }
                else
                {
                    return;
                }
            }
            catch
            {
                System.Threading.Thread.Sleep(6000);
                i = i + 1;
                goto Finish;
            }

            //将变动信息写入XML.
            WriterFileXML(e.Name, e.FullPath);

            //从XML中读取文件信息并上传.
            ReaderFileXML();

            lblMessage.Text = sum.ToString() + " 文件已成功分发!";
        }


        /// <summary>
        /// 将改动过的文件记录到XML中.
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="fullPath">完整路径</param>
        private void WriterFileXML(string fileName, string fullPath)
        {

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load("E:\\WatcherFiles.xml");
            }
            catch(Exception e)
            {
                WriteLog(e.ToString());
            }

            XmlNode root = doc.SelectSingleNode("WatcherFiles");
           
            XmlElement fileinfo = doc.CreateElement("FileInfo");

            sum += 1;

            XmlElement id = doc.CreateElement("ID");
            id.InnerText = sum.ToString();
            fileinfo.AppendChild(id);

            //文件名
            XmlElement fn = doc.CreateElement("FileName");
            fn.InnerText = fileName;
            fileinfo.AppendChild(fn);

            //完整路径
            XmlElement fp = doc.CreateElement("FullPath");
            fp.InnerText = fullPath;
            fileinfo.AppendChild(fp);

            //保存时间
            XmlElement st = doc.CreateElement("SaveTime");
            st.InnerText = DateTime.Now.ToString();
            fileinfo.AppendChild(st);

            //0是否已ftp到网通0否,1是，2出错
            XmlElement isWT = doc.CreateElement("IsFTPWT");
            isWT.InnerText = "0";
            fileinfo.AppendChild(isWT);

            //FTP到网通开始时间
            XmlElement fsWT = doc.CreateElement("FtpWTStartTime");
            fsWT.InnerText = "0";
            fileinfo.AppendChild(fsWT);

            //FTP到网通完成时间
            XmlElement feWT = doc.CreateElement("FtpWTEndTime");
            feWT.InnerText = "0";
            fileinfo.AppendChild(feWT);


            //0是否已ftp到电信0否,1是，2出错
            XmlElement isDX = doc.CreateElement("IsFTPDX");
            isDX.InnerText = "0";
            fileinfo.AppendChild(isDX);

            //FTP到电信开始时间
            XmlElement fsDX = doc.CreateElement("FtpDXStartTime");
            fsDX.InnerText = "0";
            fileinfo.AppendChild(fsDX);

            //FTP到电信完成时间
            XmlElement feDX = doc.CreateElement("FtpDXEndTime");
            feDX.InnerText = "0";
            fileinfo.AppendChild(feDX);

            root.AppendChild(fileinfo);
            doc.Save("E:\\WatcherFiles.xml");

        }

        /// <summary>
        /// 使用FTP上传文件
        /// </summary>
        private bool UseFTP(string fileName,string fullName)
        {
            string smallName = fileName.Replace(@"\", @"/");

            WebClient client = new WebClient();
            
            //FTP用户名，密码.
            client.Credentials = new NetworkCredential("YooYoCN", "200712101048YooYoCN");

            string strAddress = txtAddress.Text.Trim();
            Uri url = new Uri(strAddress +smallName);

            try
            {
                //FTP执行
                client.UploadFile(url,fullName);
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());

                return false;
            }

            return true;
        }

        /// <summary>
        /// 读取XML
        /// </summary>
        private void ReaderFileXML()
        {
            XmlDocument xd = new XmlDocument();
            xd.Load("E:\\WatcherFiles.xml");

            XmlNodeList xnl = xd.SelectSingleNode("WatcherFiles").ChildNodes;

            foreach (XmlNode xn in xnl)
            {
                //取得FileInfo节点
                XmlElement fie = (XmlElement)xn;

                if (fie.SelectSingleNode("IsFTPWT").InnerText == "0")
                {
                    string isWT = fie.SelectSingleNode("IsFTPWT").InnerText;

                    string fileName = fie.SelectSingleNode("FileName").InnerText;

                    string fullPath = fie.SelectSingleNode("FullPath").InnerText;

                    //判断文件是否存在.
                    if (System.IO.File.Exists(fullPath) == true)
                    {
                        string startWTime = DateTime.Now.ToString();

                        //上传到网通服务器
                        if (UseFTP(fileName, fullPath))
                        {
                            string endWTime = DateTime.Now.ToString();
                            fie.SelectSingleNode("IsFTPWT").InnerText = "1";
                            fie.SelectSingleNode("FtpWTStartTime").InnerText = startWTime;
                            fie.SelectSingleNode("FtpWTEndTime").InnerText = endWTime;
                            xd.Save("E:\\WatcherFiles.xml");
                        }
                        else
                        {
                            WriteLog(fullPath + "没有成功上传.时间在" + startWTime);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 异常信息写入日志
        /// </summary>
        private void WriteLog(string errorMessage)
        {
            string path = @"C:\WINDOWS\FileWatcherLog.txt";

            using (StreamWriter sw = File.AppendText(path))
            {
                string now = "异常发生在" + DateTime.Now.ToString() +
                    ".详细信息为:" + errorMessage ;
                sw.WriteLine(now);
            }
        }

        private void myWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            string s = e.FullPath;

            //将变动信息写入XML.
            WriterFileXML(e.Name, e.FullPath);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}