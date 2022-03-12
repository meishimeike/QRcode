using System;
using System.Drawing;
using System.Windows.Forms;

namespace 二維碼
{
    public partial class Cpt : Form
    {
        public delegate void getImage(Image image);
        public event getImage GetImage;
        public Bitmap BackImage { set; get; }

        int mX, mY;
        public Cpt()
        {
            InitializeComponent();
        }

        private void Cpt_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = BackImage;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                pictureBox1.Refresh();
                Graphics gh = pictureBox1.CreateGraphics();
                int gapX = e.X - mX;
                int gapY = e.Y - mY;
                if (gapX >= 0 && gapY >= 0)
                    gh.DrawRectangle(new Pen(Color.Red), mX, mY, gapX, gapY);
                if (gapX >= 0 && gapY < 0)
                    gh.DrawRectangle(new Pen(Color.Red), mX, e.Y, gapX, Math.Abs(gapY));
                if (gapX <= 0 && gapY >= 0)
                    gh.DrawRectangle(new Pen(Color.Red), e.X, mY, Math.Abs(gapX), gapY);
                if (gapX < 0 && gapY < 0)
                    gh.DrawRectangle(new Pen(Color.Red), e.X, e.Y, Math.Abs(gapX), Math.Abs(gapY));
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            mX = e.X;
            mY = e.Y;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            int dX = e.X - mX;
            int dY = e.Y - mY;
            if (dX == 0 || dY == 0) return;
            Bitmap map = new Bitmap(Math.Abs(dX) + 1, Math.Abs(dY) + 1);
            Graphics gbm = Graphics.FromImage(map);
            gbm.CopyFromScreen(mX, mY, 0, 0, new Size(map.Width, map.Height));
            if (dX >= 0 && dX >= 0)
                gbm.CopyFromScreen(mX, mY, 0, 0, new Size(map.Width, map.Height));
            if (dX >= 0 && dY < 0)
                gbm.CopyFromScreen(mX, e.Y, 0, 0, new Size(map.Width, map.Height));
            if (dX <= 0 && dY >= 0)
                gbm.CopyFromScreen(e.X, mY, 0, 0, new Size(map.Width, map.Height));
            if (dX < 0 && dY < 0)
                gbm.CopyFromScreen(e.X, e.Y, 0, 0, new Size(map.Width, map.Height));
            GetImage(map);
            Close();
        }

    }
}
