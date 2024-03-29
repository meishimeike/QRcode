﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ZXing.QrCode;
using ZXing.Common;
using ZXing;
using System.IO;
using System.Drawing.Imaging;
using System.Data.OleDb;
using System.Threading;

namespace 二維碼
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();     
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1_TextChanged(null, null);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            textBox1_TextChanged(null, null);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                if (radioButton1.Checked)
                    pictureBox1.Image = Generate1(textBox1.Text);
                if (radioButton2.Checked)
                    pictureBox1.Image = Generate2(textBox1.Text);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog opd = new OpenFileDialog();
            opd.Filter = "Excel(*.xls;*.xlsx;*.csv)|*.xls;*.xlsx;*.csv|文本文档(*.txt)|*.txt";
            if (opd.ShowDialog() == DialogResult.OK) 
            {
                textBox2.Text = opd.FileName;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text)) 
            {
                MessageBox.Show("请先选择數據文件");
                return;
            }
            if (string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("请先選擇二維碼保存文件夹");
                return;
            }
            button1.Enabled = false;
            int mode = 0;
            if (radioButton3.Checked)
            {
                mode = 0;
            }
            if (radioButton4.Checked)
            {
                mode = 1;
            }
            Thread StartThread = new Thread(() => Start(textBox2.Text, textBox3.Text, mode));
            StartThread.Start();
        }

        private void Start(string file,string fold,int mode)
        {
            try
            {
                string es = Path.GetExtension(file).ToLower();
                string[] strs = null;
                List<string> ls = new List<string>();
                int n = 0;
                if (es == ".txt")
                {
                    strs = File.ReadAllLines(file);
                }
                else if (es == ".csv")
                {
                    string[] mline = File.ReadAllLines(file);
                    foreach (string line in mline) 
                    {
                        string[] lines = line.Split(',');
                        foreach (string str in lines) 
                        {
                            ls.Add(str);
                        }
                    }
                    strs = ls.ToArray();
                }
                else if (es == ".xlsx" || es == ".xls")
                {
                    DataSet oe = ToDataTable(file);
                    foreach (DataTable tbs in oe.Tables)
                    {
                        foreach (DataRow dc in tbs.Rows)
                        {
                            foreach (object value in dc.ItemArray)
                            {
                                string values = value.ToString();
                                if (!string.IsNullOrEmpty(values))
                                {
                                    ls.Add(values);
                                }
                            }
                        }
                    }
                    strs = ls.ToArray();
                }
                else
                {
                    MessageBox.Show("不支持數據格式！");
                    setButton(true);
                    return;
                }

                n = strs.Length;
                for (int i = 0; i < n; i++)
                {
                    string qr = strs[i];
                    if (!string.IsNullOrEmpty(qr))
                    {
                        Bitmap sbitmap = null;
                        if (mode == 0)
                            sbitmap = Generate1(qr);
                        if (mode == 1)
                            sbitmap = Generate2(qr);

                        sbitmap.Save(fold + "\\" + qr.Replace("*","") + ".png", ImageFormat.Png);
                    }
                    ShowPro((int)(i / (double)n * 100));
                }
                ShowPro(100);
            }
            catch (Exception ex)
            {
                ShowPro(100);
                MessageBox.Show("操作失敗," + ex.Message);
                setButton(true);
                return;
            }
            MessageBox.Show("操作完成！");
            setButton(true);
        }

        // <summary>  
        /// 读取Excel文件到DataSet中  
        /// </summary>  
        /// <param name="filePath">文件路径</param>  
        /// <returns></returns>  
        public static DataSet ToDataTable(string filePath)
        {
            string connStr = "";
            string fileType = System.IO.Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(fileType)) return null;

            if (fileType == ".xls")
                connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + filePath + ";" + ";Extended Properties=\"Excel 8.0;HDR=NO;IMEX=1\"";
            else
                connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + filePath + ";" + ";Extended Properties=\"Excel 12.0;HDR=NO;IMEX=1\"";
            string sql_F = "Select * FROM [{0}]";

            OleDbConnection conn = null;
            OleDbDataAdapter da = null;
            DataTable dtSheetName = null;

            DataSet ds = new DataSet();
            try
            {
                conn = new OleDbConnection(connStr);
                conn.Open();                     
                string SheetName = "";
                dtSheetName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                da = new OleDbDataAdapter();
                for (int i = 0; i < dtSheetName.Rows.Count; i++)
                {
                    SheetName = (string)dtSheetName.Rows[i]["TABLE_NAME"];

                    if (SheetName.Contains("$") && !SheetName.Replace("'", "").EndsWith("$"))
                    {
                        continue;
                    }

                    da.SelectCommand = new OleDbCommand(String.Format(sql_F, SheetName), conn);
                    DataSet dsItem = new DataSet();
                    da.Fill(dsItem, SheetName);

                    ds.Tables.Add(dsItem.Tables[0].Copy());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    da.Dispose();
                    conn.Dispose();
                }
            }
            return ds;
        } 

        private void ShowPro(int value)
        {
            setProgressBarValue(value);
            setProgressBarTextValue(value);
        }

        private void setProgressBarValue(int value)
        {
            Invoke(new Action(() =>
            {
                progressBar1.Value = value;
            }));
        }
        private void setButton(bool enable)
        {
            Invoke(new Action(() =>
            {
                button2.Enabled = enable;
            }));
        }
        private void setProgressBarTextValue(int value)
        {
            Invoke(new Action(() =>
            {
                label3.Text = "处理进度 " + value + "%";
            }));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK) 
            {
                textBox3.Text = fbd.SelectedPath;
            }
        }

        private void 保存图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG文件|*.png";
            if(sfd.ShowDialog()==DialogResult.OK)
            {
                try
                {
                    pictureBox1.Image.Save(sfd.FileName, ImageFormat.Png);
                    MessageBox.Show("保存成功！");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存失敗！" + ex.Message);
                }
               
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            ShowPro(0);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            ShowPro(0);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "图片文件(*.jpg;*.png;*.bmp)|*.jpg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK) 
            {
                textBox4.Text = ofd.FileName;
            }
        }

        /// <summary>
        /// 生成二维码,保存成图片
        /// </summary>
        static Bitmap Generate1(string text)
        {
            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            QrCodeEncodingOptions options = new QrCodeEncodingOptions();
            options.DisableECI = false;
            options.CharacterSet = "UTF-8";
            options.Width = 400;
            options.Height = 400;
            options.Margin = 1;
            writer.Options = options;
            Bitmap map = writer.Write(text);
            return map;
        }

        /// <summary>
        /// 生成條形碼,保存成图片
        /// </summary>
        static Bitmap Generate2(string text)
        {
            if (text.Length > 80)
            {
                Bitmap newBitmap = new Bitmap(400,100);
                Pen Npan = new Pen(Color.Red);          
                Graphics Ngra= Graphics.FromImage(newBitmap);
                Ngra.DrawString("条形码字符长度不能大于80", new Font("arial",16), Npan.Brush,40,50);
                return newBitmap; 
            }
            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.CODE_128;
            EncodingOptions options = new EncodingOptions()
            {
                Width = 400,
                Height = 100,
                Margin = 2
            };
            writer.Options = options;
            Bitmap map = writer.Write(text);
            return map;
        }

        /// <summary>
        /// 读取二维码
        /// </summary>
        /// <param name="filename">指定二维码图片位置</param>
        static string QrRead(string filename)
        {
            BarcodeReader reader = new BarcodeReader();
            reader.Options.CharacterSet = "UTF-8";
            Bitmap map = new Bitmap(filename);
            Result result = reader.Decode(map);
            return result == null ? "读取失败" : result.Text;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            try
            {
                pictureBox2.Image = Image.FromFile(textBox4.Text);
                textBox5.Text = QrRead(textBox4.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Hide();
            Thread.Sleep(200);
            int w, h;
            w=Screen.PrimaryScreen.Bounds.Width;
            h=Screen.PrimaryScreen.Bounds.Height;
            Bitmap backmap = new Bitmap(w, h);
            Graphics gpt = Graphics.FromImage(backmap);
            gpt.CopyFromScreen(0, 0, 0, 0, new Size(w, h));
            Cpt cutPic = new Cpt();
            cutPic.BackImage = backmap;
            cutPic.GetImage += CutPic_GetImage;
            cutPic.ShowDialog();

            BarcodeReader reader = new BarcodeReader();
            Result result = reader.Decode((Bitmap)pictureBox2.Image);
            if (result == null)
                textBox5.Text = "读取失败，请重新选取！";
            else
                textBox5.Text = result.Text;
            Show();
        }

        private void CutPic_GetImage(Image image)
        {
            pictureBox2.Image = image;
        }

        private void 另存为ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                SaveFileDialog simage = new SaveFileDialog();
                simage.Filter = "BMP|*.bmp|PNG|*.png|JPG|*.jpg";
                if (simage.ShowDialog() == DialogResult.OK)
                    pictureBox2.Image.Save(simage.FileName);
            }
        }
    }
}
