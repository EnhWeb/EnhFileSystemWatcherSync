/*------------------------------------------------
// YooKee
// Copyright (C) 2006-2008 
//
// �ļ���:Form1.cs
// �ļ���������������������ļ�ʵʱͬ������
//
// �����ˣ�Sunshine
// ��������: 2007.12.11
// 
// �޸ı�ʶ:2007.12.13
// �޸�����:����ͨ��XML��������
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
        /// ��¼�ļ���������
        /// </summary>
        private int sum = 0;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// �����ʼ���Ӱ�ť
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBegin_Click(object sender, EventArgs e)
        {
            //Ҫ���ӵ�Ŀ¼
            string sFolder = txtFolder.Text.Trim();
            myWatcher.EnableRaisingEvents = true;
            myWatcher.Path = sFolder;
            myWatcher.IncludeSubdirectories = true;
            myWatcher.NotifyFilter = NotifyFilters.Size;
            lblMessage.Text = "���ڼ�����...";

        }

        /// <summary>
        /// ���ļ��ı�ʱ���в���
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

            //���䶯��Ϣд��XML.
            WriterFileXML(e.Name, e.FullPath);

            //��XML�ж�ȡ�ļ���Ϣ���ϴ�.
            ReaderFileXML();

            lblMessage.Text = sum.ToString() + " �ļ��ѳɹ��ַ�!";
        }


        /// <summary>
        /// ���Ķ������ļ���¼��XML��.
        /// </summary>
        /// <param name="fileName">�ļ���</param>
        /// <param name="fullPath">����·��</param>
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

            //�ļ���
            XmlElement fn = doc.CreateElement("FileName");
            fn.InnerText = fileName;
            fileinfo.AppendChild(fn);

            //����·��
            XmlElement fp = doc.CreateElement("FullPath");
            fp.InnerText = fullPath;
            fileinfo.AppendChild(fp);

            //����ʱ��
            XmlElement st = doc.CreateElement("SaveTime");
            st.InnerText = DateTime.Now.ToString();
            fileinfo.AppendChild(st);

            //0�Ƿ���ftp����ͨ0��,1�ǣ�2����
            XmlElement isWT = doc.CreateElement("IsFTPWT");
            isWT.InnerText = "0";
            fileinfo.AppendChild(isWT);

            //FTP����ͨ��ʼʱ��
            XmlElement fsWT = doc.CreateElement("FtpWTStartTime");
            fsWT.InnerText = "0";
            fileinfo.AppendChild(fsWT);

            //FTP����ͨ���ʱ��
            XmlElement feWT = doc.CreateElement("FtpWTEndTime");
            feWT.InnerText = "0";
            fileinfo.AppendChild(feWT);


            //0�Ƿ���ftp������0��,1�ǣ�2����
            XmlElement isDX = doc.CreateElement("IsFTPDX");
            isDX.InnerText = "0";
            fileinfo.AppendChild(isDX);

            //FTP�����ſ�ʼʱ��
            XmlElement fsDX = doc.CreateElement("FtpDXStartTime");
            fsDX.InnerText = "0";
            fileinfo.AppendChild(fsDX);

            //FTP���������ʱ��
            XmlElement feDX = doc.CreateElement("FtpDXEndTime");
            feDX.InnerText = "0";
            fileinfo.AppendChild(feDX);

            root.AppendChild(fileinfo);
            doc.Save("E:\\WatcherFiles.xml");

        }

        /// <summary>
        /// ʹ��FTP�ϴ��ļ�
        /// </summary>
        private bool UseFTP(string fileName,string fullName)
        {
            string smallName = fileName.Replace(@"\", @"/");

            WebClient client = new WebClient();
            
            //FTP�û���������.
            client.Credentials = new NetworkCredential("YooYoCN", "200712101048YooYoCN");

            string strAddress = txtAddress.Text.Trim();
            Uri url = new Uri(strAddress +smallName);

            try
            {
                //FTPִ��
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
        /// ��ȡXML
        /// </summary>
        private void ReaderFileXML()
        {
            XmlDocument xd = new XmlDocument();
            xd.Load("E:\\WatcherFiles.xml");

            XmlNodeList xnl = xd.SelectSingleNode("WatcherFiles").ChildNodes;

            foreach (XmlNode xn in xnl)
            {
                //ȡ��FileInfo�ڵ�
                XmlElement fie = (XmlElement)xn;

                if (fie.SelectSingleNode("IsFTPWT").InnerText == "0")
                {
                    string isWT = fie.SelectSingleNode("IsFTPWT").InnerText;

                    string fileName = fie.SelectSingleNode("FileName").InnerText;

                    string fullPath = fie.SelectSingleNode("FullPath").InnerText;

                    //�ж��ļ��Ƿ����.
                    if (System.IO.File.Exists(fullPath) == true)
                    {
                        string startWTime = DateTime.Now.ToString();

                        //�ϴ�����ͨ������
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
                            WriteLog(fullPath + "û�гɹ��ϴ�.ʱ����" + startWTime);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// �쳣��Ϣд����־
        /// </summary>
        private void WriteLog(string errorMessage)
        {
            string path = @"C:\WINDOWS\FileWatcherLog.txt";

            using (StreamWriter sw = File.AppendText(path))
            {
                string now = "�쳣������" + DateTime.Now.ToString() +
                    ".��ϸ��ϢΪ:" + errorMessage ;
                sw.WriteLine(now);
            }
        }

        private void myWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            string s = e.FullPath;

            //���䶯��Ϣд��XML.
            WriterFileXML(e.Name, e.FullPath);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}