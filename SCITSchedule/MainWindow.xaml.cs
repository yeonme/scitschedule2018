using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using System.Windows.Threading;
using SCITSchedule.Properties;
using System.Windows.Forms;
using System.Drawing;

namespace SCITSchedule
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        WebClient wc = null;
        List<string> filters = new List<string>();
        NotifyIcon ni = new NotifyIcon();
        DispatcherTimer timer = null;
        const int MAX_TIME = 15;
        const string TITLE_DESC = " - 트레이아이콘 복원 문제 해결, 스플리터 해결 180515";
        string titledef = "Hi " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()+TITLE_DESC+": ";

        int refTimer = MAX_TIME;

        public MainWindow()
        {
            InitializeComponent();
            Title = titledef + "Ready";

            tboxFilter.Text = Settings.Default.filter;
            chkFilter.IsChecked = Settings.Default.invisible;
            filters = tboxFilter.Text.Split(',').ToList();

            DataForming.OnChanged += DataForming_OnChanged;

            String lastfile = null;
            try
            {
                lastfile = System.IO.File.ReadAllText(@"lastlist.txt");
            } catch
            {
                Debug.WriteLine("NO LAST FILE: First run?");
            }

            if (lastfile != null)
                DataForming.SelectAll(lastfile, true);

            ni.BalloonTipClicked += Ni_DoubleClick;
            ni.DoubleClick += Ni_DoubleClick;
            ni.MouseClick += Ni_MouseClick;
            System.Windows.Forms.ContextMenu ct = new System.Windows.Forms.ContextMenu();
            ct.MenuItems.Add("보기", (o, e) => {
                if (WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }
                Show();
                Activate();
            });            
            ct.MenuItems.Add("정보", (o, e) => { System.Windows.MessageBox.Show("Created by Jinbaek Lee","SCITSchedule Hi 2018"); });
            ct.MenuItems.Add("-");
            ct.MenuItems.Add("종료", (o, e) => { Close(); });
            ni.ContextMenu = ct;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("-h"))
            {
                Hide();
            }
            ni.Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                 System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name);
            ni.Visible = true;
        }

        private void Ni_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Right)
            {
                if (IsVisible)
                {
                    Hide();
                }
                else
                {
                    if (WindowState == WindowState.Minimized)
                    {
                        WindowState = WindowState.Normal;
                    }
                    Show();
                    Activate();
                }
            }
        }

        private void Ni_DoubleClick(object sender, EventArgs e) {
            if (IsVisible)
            {
                Activate();
            }
            else
            {
                if (WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }
                Show();
                Activate();
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if(WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
            base.OnStateChanged(e);
        }

        private void DataForming_OnChanged(object sender, EventArgs e)
        {
            if(sender != null)
            {
                List<Appointment> lst = (List<Appointment>)sender;
                StringBuilder sbLog = new StringBuilder();
                ni.BalloonTipTitle = "스케쥴 변경됨";
                sbLog.AppendLine(DateTime.Now.ToShortTimeString()+": 하기 내용으로 추가/변경됨");
                if(lst.Count == 0)
                {
                    ni.BalloonTipText = "새로 추가된 항목은 없으나 변경되었습니다.";
                    sbLog.AppendLine("일부 삭제되었거나 신규 추가 내용 없이 변경됨");
                }
                StringBuilder sb = new StringBuilder();
                foreach(Appointment a in lst)
                {
                    sb.AppendLine(string.Format("*{1}:{0}:{2}",a.date_start,a.schedule_title,a.schedule_content));
                    sbLog.AppendLine(string.Format("*{1}:{0}:{2}", a.date_start, a.schedule_title, a.schedule_content));
                }

                if(sb.Length > 0)
                {
                    ni.BalloonTipText = sb.ToString();
                    lvLog.Items.Add(sbLog.ToString());
                    lvLog.SelectedIndex = lvLog.Items.Count - 1;
                    ni.ShowBalloonTip(100000);
                }
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            Title = titledef + refTimer+"s remaining";
            if (refTimer <= 0)
            {
                refTimer = MAX_TIME;
                timer.Stop();
                DownloadData();
            } else
            {
                refTimer--;
            }
        }

        private void Wc_UploadDataCompleted(object sender, UploadDataCompletedEventArgs e) {

            //Debug.WriteLine(e.Error?.ToString() ?? "NOERROR");
            if (e.Error is WebException && ((WebException)e.Error).Status == WebExceptionStatus.RequestCanceled)
            {
                Title = titledef + "RETRY";
                Debug.WriteLine(e.Error?.ToString() ?? "NON ERROR ABORT");
                DownloadData();
                return;
            }
            timer.Start();
            if (e.Error != null)
            {
                /*lvLog.Items.Add(DateTime.Now.ToShortTimeString()+" "+(e.Cancelled?"사용자 취소":"통신 오류")+": "+e.Error.ToString());
                lvLog.SelectedIndex = lvLog.Items.Count - 1;*/
                Debug.WriteLine(e.Error.ToString());
                Title = titledef + "Comm Error!!";
                return;
            }
            Title = titledef + "Finished";
            Encoding en = new UTF8Encoding();
            string res = en.GetString(e.Result);
            DataForming.SelectAll(res);
            FillCalendar();

            if (wc != null)
            {
                wc.Dispose();
                wc = null;
            }
        }

        private void DownloadData()
        {
            Title = titledef+"Downloading...";
            wc = new WebClient();
            wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            wc.UploadDataCompleted += Wc_UploadDataCompleted;
            wc.UploadDataAsync(new Uri("http://sesoc.global/society_mypage/selectAllSchedule"), new byte[] { });
        }
        
        public void FillCalendar()
        {
            lvSchedule.Items.Clear();
            if(DataForming.List != null)
            {
                int rownum = 1;
                foreach(Appointment a in DataForming.List){
                    a.Highlight = false;
                    bool isfound = false;
                    if(filters != null && filters.Any(s => a.schedule_title.ToLower().Contains(s.ToLower())))
                    {
                        a.Highlight = filters.Count > 0 && (filters.FirstOrDefault(each=>each.Length == 0) != "");
                        isfound = true;
                    }
                    if (!chkFilter.IsChecked.GetValueOrDefault() ||(chkFilter.IsChecked.GetValueOrDefault() && isfound))
                    {
                        //필터 옵션이 꺼져있거나, 필터 옵션이 켜있으며 검색이 성공한 경우에 추가
                        a.RowNum = rownum++;
                        lvSchedule.Items.Add(a);
                    }
                }
            }
        }

        private void tboxFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataForming.List != null)
            {
                filters = tboxFilter.Text.Split(',').ToList();
                FillCalendar();

                Settings.Default.filter = tboxFilter.Text;
                Settings.Default.Save();
            }
        }

        private void chkFilter_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Default.invisible = chkFilter.IsChecked == true;
            Settings.Default.Save();

            if (DataForming.List != null)
            {
                FillCalendar();
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (DataForming.List == null)
            {
                DownloadData();

                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (System.Windows.MessageBox.Show("종료하면 다음 실행시까지 알림을 받을 수 없어요."+Environment.NewLine+"그래도 끄시겠어요?",
                "SCITSchedule 2018", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)==MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
