using System;
using System.Drawing;
using System.Windows.Forms;

namespace 二維碼
{
    public partial class Cpt : Form
    {
        public Bitmap PBT { set; get; }

        int mX, mY;
        public Cpt()
        {
            InitializeComponent();
        }

        private void Cpt_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = PBT;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                pictureBox1.Refresh();
                Graphics gh = pictureBox1.CreateGraphics();
                gh.DrawRectangle(new Pen(Color.Red), mX, mY, Math.Abs(e.X - mX), Math.Abs(e.Y - mY));
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            mX = e.X;
            mY = e.Y;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            pictureBox1.Refresh();
            Bitmap newBm = new Bitmap(Math.Abs(e.X - mX), Math.Abs(e.Y - mY));
            Graphics gbm = Graphics.FromImage(newBm);
            gbm.CopyFromScreen(mX, mY, 0, 0, new Size(newBm.Width, newBm.Height));
            MainForm MF = (MainForm)this.Owner;
            MF.Sbt = newBm;
            Close();
        }

    }
}
