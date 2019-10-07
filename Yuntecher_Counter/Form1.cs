using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Yuntecher_Counter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public Label counter = null;
        public Label remain1 = null, remain2 = null; //提醒用的動畫
        public Label time_up1 = null, time_up2 = null; //時間到用的
        public TextBox time = null;
        public TextBox remain_time = null;  //想要甚麼時候 提醒
        public Button get_time = null, remain_btn = null;
        public Button pause_time = null;
        public int time_int = 0, counter_second = 0, remain_counter = 0;
        public int[] remain_sec = new int[10];
        private void Form1_Load(object sender, EventArgs e)
        {
            //這邊設置 畫面全螢幕 & 可以在畫面顯示同時用keydown功能
            //DoubleBuffer 用於同時有兩個圖層 顯示不會lag
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.KeyPreview = true;
            this.DoubleBuffered = true;

            //獲取畫面的最大長寬
            String Size = SystemInformation.PrimaryMonitorSize.ToString();
            int Width = SystemInformation.PrimaryMonitorSize.Width;
            int Height = SystemInformation.PrimaryMonitorSize.Height;

            //設置 倒數Timer
            timer1 = new Timer();
            timer1.Interval = 1000;
            timer1.Tick += new EventHandler(timer1_Tick);

            //設置 動畫timer
            animate_timer = new Timer();
            animate_timer.Interval = 100;
            animate_timer.Tick += new EventHandler(animate_timer_Tick);

            //設置兩個提醒用的Label 樣式
            remain1 = new Label();
            remain1.Size = new Size(Width, Height / 2);
            remain1.Location = new Point(0 - Width,0);
            remain1.BackColor = Color.White;
            remain1.Text = "時間剩下";
            remain1.TextAlign = ContentAlignment.MiddleCenter; //文字置中
            remain1.Font = new Font("Segoe UI", 130, FontStyle.Bold);
            this.Controls.Add(remain1);
            

            //設置兩個提醒用的Label 樣式
            remain2 = new Label();
            remain2.Size = new Size(Width, Height / 2);
            remain2.Location = new Point(Width, Height/2);
            remain2.BackColor = Color.Red;
            remain2.Text = "X分鐘";
            remain2.TextAlign = ContentAlignment.MiddleCenter; //文字置中
            remain2.Font = new Font("Segoe UI", 130, FontStyle.Bold);
            this.Controls.Add(remain2);

            //設置兩個時間到用的Label 樣式
            time_up1 = new Label();
            time_up1.Size = new Size(Width, Height / 2);
            time_up1.Location = new Point(0, 0);
            time_up1.BackColor = Color.White;
            time_up1.Text = "時間到囉！";
            time_up1.TextAlign = ContentAlignment.MiddleCenter; //文字置中
            time_up1.Font = new Font("Segoe UI", 96, FontStyle.Bold);
            time_up1.Click += new EventHandler(Reset);
            this.Controls.Add(time_up1);
            time_up1.Visible = false;


            //設置兩個時間到用的Label 樣式
            time_up2 = new Label();
            time_up2.Size = new Size(Width, Height / 2);
            time_up2.Location = new Point(0, Height / 2);
            time_up2.BackColor = Color.Red;
            time_up2.Text = "Time is up！";
            time_up2.TextAlign = ContentAlignment.MiddleCenter; //文字置中
            time_up2.Font = new Font("Segoe UI", 96, FontStyle.Bold);
            time_up2.Click += new EventHandler(Reset);
            this.Controls.Add(time_up2);
            time_up2.Visible = false;

            //設定顯示倒數秒數的Label
            counter = new Label();
            counter.Text = "時間倒數";
            counter.Location = new Point(Width/12, Height/8 * 3);
            counter.Size = new Size(Width / 6 * 5 , Height / 3);   //5/6  1/3
            counter.BackColor = Color.Bisque;
            counter.TextAlign = ContentAlignment.MiddleCenter;
            counter.Font = new Font("Segoe UI", 113, FontStyle.Bold);
            /*test.FlatAppearance.BorderSize = 0;
            test.FlatStyle = FlatStyle.Flat;*/
            this.Controls.Add(counter);

            //設定獲取時間的Textbox
            time = new TextBox();
            time.Location = new Point((int)(Width / 5 * 2.5), Height / 4 * 3); //讓他盡量靠中間
            time.KeyDown += new KeyEventHandler(fresh_time);
            time.KeyUp += new KeyEventHandler(fresh_time);
            this.Controls.Add(time);

            //設置開始 的按鈕
            get_time = new Button();
            get_time.Text = "開始倒數";
            get_time.Location = new Point(time.Location.X + time.Width, time.Location.Y);
            get_time.Click += new EventHandler(start_count);
            this.Controls.Add(get_time);

            //設置暫停的按鈕
            pause_time = new Button();
            pause_time.Text = "暫停倒數";
            pause_time.Location = new Point(get_time.Location.X + get_time.Width , get_time.Location.Y);
            pause_time.Click += new EventHandler(pause);
            this.Controls.Add(pause_time);

            //設置提醒時間的輸入
            remain_time = new TextBox();
            remain_time.Location = new Point(time.Location.X,time.Location.Y+time.Height);
            this.Controls.Add(remain_time);

            //設置獲取提醒時間
            remain_btn = new Button();
            remain_btn.Text = "提醒";
            remain_btn.Click += new EventHandler(get_remain);
            remain_btn.Location = new Point(get_time.Location.X , get_time.Location.Y + get_time.Height);
            this.Controls.Add(remain_btn);


           
        }

        private void Reset(object sender, EventArgs e)
        {
            Application.Restart();
        }
        private void get_remain(object sender, EventArgs e)
        {
            //把需要提醒的時間 存取進remain_sec陣列
            string reg = remain_time.Text.ToString();
            string[] _reg = reg.Split(',');
            remain_counter = 0;
            for (int i = 0; i < 10; i++)
            {
                remain_sec[i] = 36000; //先預設 提醒倒數是10個小時
            }
            foreach(string x in _reg)
            {
                int num = Int32.Parse(x) * 60;
                remain_sec[remain_counter] = num;
                remain_counter++;
            }
        }
        private void pause(object sender, EventArgs e)
        {
            //暫停時間
            if (timer1.Enabled == true)
            {
                //那就暫停timer1
                timer1.Enabled = false;
                pause_time.Text = "繼續倒數";
            }
            else if (timer1.Enabled == false)
            {
                //啟動timer接續倒數
                timer1.Enabled = true;
                pause_time.Text = "暫停倒數";
            }
        }
        private void start_count(object sender, EventArgs e)
        {

            //這邊也把暫停的功能reset
            //啟動timer接續倒數
            timer1.Enabled = true;
            pause_time.Text = "暫停倒數";

            string counter_min = time.Text; //這邊從textbox取得輸入的時間
            try {
                int all_second = Int32.Parse(counter_min) * 60;
                counter_second = all_second;
                timer1.Enabled = true;
                timer1.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show("請檢查一下時間");
            }
            
        }

        private void fresh_time(object sender, KeyEventArgs e)
        {
            //馬上更新到上面
            counter.Text = "演講時間：" + time.Text.ToString() + "分鐘";
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
                this.Close();
            }

            if(e.KeyCode == Keys.A)
            {
                animate_timer.Enabled = true;
            }

        }

        private void animate_timer_Tick(object sender, EventArgs e)
        {
            if(remain1.Location.X < -50)
            {
                remain1.Location = new Point(remain1.Location.X + 100, remain1.Location.Y);
                remain2.Location = new Point(remain2.Location.X - 100, remain2.Location.Y);
            }else if(remain1.Location.X > 50)
            {
                remain1.Location = new Point(remain1.Location.X + 100, remain1.Location.Y);
                remain2.Location = new Point(remain2.Location.X - 100, remain2.Location.Y);
            }
            else
            {
                remain1.Location = new Point(remain1.Location.X + 2, remain1.Location.Y);
                remain2.Location = new Point(remain2.Location.X - 2, remain2.Location.Y);
            }

            //判斷是不是提醒結束了
            if(remain1.Location.X >= Width)
            {
                //重置回去原本的地方 & animate_time stop
                animate_timer.Enabled = false;
                remain1.Location = new Point(0 - Width, 0);
                remain2.Location = new Point(Width, Height / 2);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int min = counter_second / 60 , sec = counter_second % 60;
            
            if(counter_second <= 0)
            {
                timer1.Enabled = false;
                //MessageBox.Show("時間到");
                time_up1.Visible = true;
                time_up2.Visible = true;
            }
            counter.Text = "" + min + "分： " + sec + "秒";
            counter_second--;

            //檢查是不是要提醒的時間
            for(int i = 0; i < remain_counter; i++)
            {
                if(counter_second == remain_sec[i])
                {
                    remain2.Text = "" + counter_second / 60 + "分鐘";
                    animate_timer.Enabled = true;
                }
            }
        }


        //聽說加快畫面加載
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
    }
}
