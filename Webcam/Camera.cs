using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Webcam
{
    class Camera
    {
        public OpenCvSharp.VideoCapture capture { get; set; }
        public OpenCvSharp.Mat frame { get; set; }
        private VideoWriter videoWriter { get; set; }
        private System.Drawing.Bitmap image { get; set; }
        private bool Camera_state { get; set; }
        private bool Record_video_start = false;
        private bool Record_video_stop= false;
        private Grid_type type = Grid_type.None;
        public enum Grid_type
        {
            None, Three_point_line, Cross_line
        }


        public void Open_camera(int camera_index,PictureBox display_windows)
        {           
            frame = new OpenCvSharp.Mat();
            capture = new OpenCvSharp.VideoCapture(camera_index);
            capture.Set(VideoCaptureProperties.FrameWidth, 1280);
            capture.Set(VideoCaptureProperties.FrameHeight, 720);
            if (!capture.IsOpened())
            {
                HandyControl.Controls.MessageBox.Show("開啟相機失敗!", "警告");
                return;
            }
            Camera_state = true;
            while (true)
            {
                bool read_success = capture.Read(frame);
                if (Record_video_start)
                {
                    if (!Record_video_stop)
                    {
                        videoWriter.Write(frame);
                    }
                    else
                    {
                        videoWriter.Dispose();
                    }
                }
                //if (!read_success)
                //{
                //    HandyControl.Controls.MessageBox.Show("無法讀取相機的幀！", "警告");
                //}
                //else
                //{
                //防止狀態切換太快，讀到空值
                if (frame.Height == 0) continue;
                switch (type)
                {
                    case Grid_type.None:
                        {
                            image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);
                            break;
                        }
                    case Grid_type.Three_point_line:
                        {
                            Mat result;
                            Draw_three_point_line(frame, out result);
                            image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(result);
                            break;
                        }
                    case Grid_type.Cross_line:
                        {
                            Mat result;
                            Draw_cross_line(frame, out result);
                            image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(result);
                            break;
                        }

                }
                display_windows.Image = image;
                image = null;
                //}
            }
        }

        public bool Take_picture(string savepath)
        {
            if (Camera_state)
            {
                Cv2.ImWrite(savepath, frame);
                return true;
            }
            else
            {
                HandyControl.Controls.MessageBox.Show("請先開啟相機!", "警告");
                return false;
            }
            
        }

        public bool Reord_video(string onoff, string savepath)
        {
            if (Camera_state)
            {
                if (onoff == "on")
                {
                    videoWriter = new VideoWriter(savepath, VideoWriter.FourCC(@"XVID"), 30, new Size(640, 480), true);
                    Record_video_start = true;
                    Record_video_stop = false;
                }
                else if (onoff == "off")
                {
                    Record_video_stop = true;
                }
                return true;
            }
            else
            {
                HandyControl.Controls.MessageBox.Show("請先開啟相機!", "警告");
                return false;
            }

        }

        public void Draw_three_point_line(Mat frame, out Mat result)
        {
            Mat drawfram = frame.Clone();
            int gap_x = Convert.ToInt32(drawfram.Width / 3);
            int gap_y = Convert.ToInt32(drawfram.Height / 3);
            //畫直線
            Point point1 = new Point(gap_x, 0);
            Point point2 = new Point(gap_x, drawfram.Height);
            Cv2.Line(drawfram, point1, point2, Scalar.Blue, 1, LineTypes.AntiAlias);
            point1 = new Point(2*gap_x, 0);
            point2 = new Point(2*gap_x, drawfram.Height);
            Cv2.Line(drawfram, point1, point2, Scalar.Blue, 1, LineTypes.AntiAlias);
            //畫橫線
            point1 = new Point(0, gap_y);
            point2 = new Point(drawfram.Width, gap_y);
            Cv2.Line(drawfram, point1, point2, Scalar.Blue, 1, LineTypes.AntiAlias);
            point1 = new Point(0, 2*gap_y);
            point2 = new Point(drawfram.Width, 2*gap_y);
            Cv2.Line(drawfram, point1, point2, Scalar.Blue, 1, LineTypes.AntiAlias);
            result = drawfram;
        }

        public void Draw_cross_line(Mat frame, out Mat result)
        {
            Mat drawfram = frame.Clone();
            int gap_x = Convert.ToInt32(drawfram.Width / 2);
            int gap_y = Convert.ToInt32(drawfram.Height / 2);
            //畫直線
            Point point1 = new Point(gap_x, 0);
            Point point2 = new Point(gap_x, drawfram.Height);
            Cv2.Line(drawfram, point1, point2, Scalar.Blue, 1, LineTypes.AntiAlias);
            //畫橫線
            point1 = new Point(0, gap_y);
            point2 = new Point(drawfram.Width, gap_y);
            Cv2.Line(drawfram, point1, point2, Scalar.Blue, 1, LineTypes.AntiAlias);
            result = drawfram;

        }

        public void Change_grid_type(string grid_type)
        {
            type = (Camera.Grid_type)Enum.Parse(typeof(Camera.Grid_type), grid_type);
        }


    }
}
