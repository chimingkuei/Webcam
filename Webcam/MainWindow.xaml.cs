using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Webcam
{
    public class Parameter
    {
        public string Save_File_Path_val { get; set; }
        public string Sreen_Grid_val { get; set; }

        public string Camera_Index_val { get; set; }

    }

    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Function
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (HandyControl.Controls.MessageBox.Show("請問是否要關閉？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void Mytimer_tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new SetControlValue(ShowTime), TimeCount);
            TimeCount++;
        }

        private void ShowTime(long t)
        {
            TimeSpan temp = new TimeSpan(0, 0, (int)t);
            Show_Time.Text = string.Format("{0:00}:{1:00}:{2:00}", temp.Hours, temp.Minutes, temp.Seconds);
        }

        private void Open_camera(object camera_index)
        {
            //string Sreen_Grid_val = "";
            //this.Dispatcher.Invoke(() =>
            //{
            //    Logger.WriteLog("打開相機!", 1, richTextBoxGeneral);
            //    Sreen_Grid_val = Sreen_Grid.Text;
            //});
            camera.Open_camera(Convert.ToInt32(camera_index), Display_Windows);
        }
        #endregion

        #region Parameter and Init
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Display_Windows.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            //設定時間間隔ms
            int interval = 1000;
            Mytimer = new System.Timers.Timer(interval);
            //設定重複計時
            Mytimer.AutoReset = true;
            //設定執行System.Timers.Timer.Elapsed事件
            Mytimer.Elapsed += new System.Timers.ElapsedEventHandler(Mytimer_tick);
            //Load Config
            Config.Path = @"./Config.json";
            List<Parameter> Parameter_info = Config.Load();
            Save_File_Path.Text = Parameter_info[0].Save_File_Path_val;
            Sreen_Grid.Text = Parameter_info[0].Sreen_Grid_val;
            Camera_Index.Text = Parameter_info[0].Camera_Index_val;

        }
        BaseLogRecord Logger = new BaseLogRecord();
        BaseConfig<Parameter> Config = new BaseConfig<Parameter>();
        Camera camera = new Camera();
        System.Timers.Timer Mytimer;
        long TimeCount;
        public delegate void SetControlValue(long value);
        Thread Open_camera_thread;
        #endregion

        #region Main Screen
        private void Main_Btn_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(Open_Camera):
                    {
                        if (Open_Camera.Content.ToString() == "開啟相機")
                        {

                            Open_camera_thread = new Thread(new ParameterizedThreadStart(Open_camera));
                            Open_camera_thread.Start(Camera_Index.Text);
                            Logger.WriteLog("開啟相機!", 1, richTextBoxGeneral);
                            Open_Camera.Content = "關閉相機";
                        }
                        else if (Open_Camera.Content.ToString() == "關閉相機")
                        {

                            Open_camera_thread.Abort();
                            while (Open_camera_thread.ThreadState != ThreadState.Aborted)
                            {
                                Thread.Sleep(100);
                            }
                            camera.capture.Dispose();
                            camera.frame.Dispose();
                            Display_Windows.Image = null;
                            Logger.WriteLog("關閉相機!", 1, richTextBoxGeneral);
                            Open_Camera.Content = "開啟相機";
                        }
                        break;
                    }
                case nameof(Take_Picture):
                    {
                        if (!string.IsNullOrEmpty(Save_File_Path.Text))
                        {
                            bool state = camera.Take_picture(Save_File_Path.Text + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bmp");
                            if (state)
                            {
                                Logger.WriteLog("拍攝影像!", 1, richTextBoxGeneral);
                            }
                        }
                        else
                        {
                            HandyControl.Controls.MessageBox.Show("請設定儲存路徑!", "警告");
                        }
                        break;
                    }
                case nameof(Record_video):
                    {
                        if (Record_video.Content.ToString() == "錄影")
                        {
                            if (!string.IsNullOrEmpty(Save_File_Path.Text))
                            {
                                Open_Camera.IsEnabled = false;
                                bool state=camera.Reord_video("on", Save_File_Path.Text + DateTime.Now.ToString("yyyyMMddHHmmss") + ".mp4");
                                if (state)
                                {
                                    Mytimer.Start();
                                    TimeCount = 0;
                                    Record_video.Content = "停止錄影";
                                    Logger.WriteLog("開始錄影!", 1, richTextBoxGeneral);
                                }
                            }
                            else
                            {
                                HandyControl.Controls.MessageBox.Show("請設定儲存路徑!", "警告");
                            }
                        }
                        else if (Record_video.Content.ToString() == "停止錄影")
                        {
                            Open_Camera.IsEnabled = true;
                            camera.Reord_video("off", null);
                            Mytimer.Stop();
                            Show_Time.Text = "";
                            Record_video.Content = "錄影";
                            Logger.WriteLog("停止錄影!", 1, richTextBoxGeneral);
                        }
                        break;
                    }
            }
        }
        #endregion

        #region Parameter Screen
        private void Sreen_Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0)
            {
                string text = (e.AddedItems[0] as ComboBoxItem).Content as string;
                camera.Change_grid_type(text);
            }
        }

        private void Parameter_Btn_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(Open_Dir):
                    {
                        System.Windows.Forms.FolderBrowserDialog path = new System.Windows.Forms.FolderBrowserDialog();
                        path.ShowDialog();
                        Save_File_Path.Text = path.SelectedPath;
                        break;
                    }
                case nameof(Save_Config):
                    {
                        List<Parameter> Parameter_config = new List<Parameter>()
                        {
                            new Parameter() {Save_File_Path_val = Save_File_Path.Text,
                                             Sreen_Grid_val = Sreen_Grid.Text,
                                             Camera_Index_val=Camera_Index.Text
                                             }
                        };
                        Config.Save(Parameter_config);
                        Logger.WriteLog("儲存參數!", 1, richTextBoxGeneral);
                        break;
                    }
            }
        }
        #endregion


    }
}
