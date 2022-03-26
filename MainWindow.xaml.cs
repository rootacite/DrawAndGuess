using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;

namespace DrawAndGuess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Conversation Server;
        private readonly Bitmap Buffer;
        private readonly WriteableBitmap BackSide;
        private readonly Graphics Pen;

        private SpinLock @lock = new SpinLock();
        private bool LockFlag = false;

        private byte[] ?PublicData;
        void Flush(PaintEventHandle e)
        {
            BackSide.Lock();
            e?.Invoke(Pen);
            BackSide.AddDirtyRect(new Int32Rect(0, 0, 1600, 900));
            BackSide.Unlock();

        }

        void ImgMutil(bool io, PaintEventHandle e, ref byte[] Opt)
        {
            LockFlag = false;
      //      @lock.Enter(ref LockFlag);
            if (io)
            {
                Flush(e);
            }
            else
            {
                Opt = Buffer.CompressionImage(12);
            }
     //       @lock.Exit();
        }
        public MainWindow()
        {
            InitializeComponent();

            BackSide = new WriteableBitmap(1600, 900, 96, 96, PixelFormats.Bgr32, null);
            Buffer = new Bitmap(1600, 900, BackSide.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppRgb, BackSide.BackBuffer);
            Pen = Graphics.FromImage(Buffer);
            CCn.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 220, 220));

            Plant.Source = BackSide;

            Server = new Conversation();
            Loaded += (e, v) =>
            {
                ImgMutil(true, (g) => { g.Clear(Color.FromArgb(255, 250, 250, 250)); }, ref PublicData);

                Server.OnHosted += () =>
                  {
                      Dispatcher.Invoke(() => { 
                          ImgMutil(true, (g) => { g.Clear(Color.FromArgb(255, 250, 250, 250)); }, ref PublicData);
                      });
                  };
                Server.UnHosted += () => 
                {
                    Dispatcher.Invoke(() => {
                        ImgMutil(true, (g) => { g.Clear(Color.FromArgb(255, 250, 250, 250)); }, ref PublicData);
                    });
                };
                Server.OnBitmap += (b) =>
                {
                    Dispatcher.Invoke(() => {
                        //   b.Save(times++.ToString() + ".png");
                        ImgMutil(true, (g) =>
                        {
                            g.Clear(Color.FromArgb(255, 250, 250, 250));
                            g.DrawImage(b, 0, 0, 1600, 900);
                            b.Dispose();
                        }, ref PublicData);
                       
                    });
                };
                Task.Run(() =>
                {
                    while (true)
                    {
                        if (Server.IsMainHost)
                        {
                            byte[] Db = new byte[1];
                            Dispatcher.Invoke(() => {
                                BackSide.Lock();
                            });
                            try
                            {
                                ImgMutil(false, null, ref Db);
                            }catch (Exception ex)
                            { }
                            Dispatcher.Invoke(() => {
                                BackSide.Unlock();
                            });
                            Server.PublishImg(Db);
                            Task.Delay(250);
                        }
                    }
                });
            };

           
        }

        delegate void PaintEventHandle(Graphics g);

        private void Window_StylusDown(object sender, StylusDownEventArgs e)
        {
            
            var FirstPen = e.GetStylusPoints(Plant)[0];
            var Point = new System.Drawing.Point((int)FirstPen.X, (int)FirstPen.Y);
            Save_Last.Clear();
            Save_Last.AddRange(new Point[] { Point, Point });
        }
        List<System.Drawing.Point> Save_Last = new List<System.Drawing.Point>();
        private void Window_StylusMove(object sender, StylusEventArgs e)
        {
            var FirstPen = e.GetStylusPoints(Plant)[0];
            var Point = new System.Drawing.Point((int)FirstPen.X, (int)FirstPen.Y);
            Title = FirstPen.PressureFactor.ToString();
            if (!Server.IsMainHost) return;

            using (Pen pen = new Pen(Color.Black, 7f* FirstPen.PressureFactor))
                ImgMutil(true, (g) => { g.DrawLines(pen, Save_Last.ToArray()); }, ref PublicData);
            Save_Last.RemoveAt(0);

            Save_Last.Add(Point);
        }
        int times = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            m_Id = Id.Text;
            Id.IsEnabled = false;
            (sender as Button).IsEnabled = false;

            Server.SetName(m_Id);
            
        }

        string m_Id = "";

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
         //   var FirstPen = e.GetPosition(Plant);
         //   var Point = new System.Drawing.Point((int)FirstPen.X, (int)FirstPen.Y);
         //   Save_Last.Clear();
         //   Save_Last.AddRange(new Point[] { Point, Point, Point });
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
        /*    if (!Server.IsMainHost || Mouse.LeftButton != MouseButtonState.Pressed) return;
            var FirstPen = e.GetPosition(Plant);
            var Point = new System.Drawing.Point((int)FirstPen.X, (int)FirstPen.Y);

            using (Pen pen = new Pen(Color.Black, 3.5F))
                ImgMutil(true, (g) => { g.DrawLines(pen, Save_Last.ToArray()); }, ref PublicData);
            Save_Last.RemoveAt(0);

            Save_Last.Add(Point);
            Title = Point.ToString();*/
        }
    }

}
