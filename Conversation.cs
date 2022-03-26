using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DrawAndGuess
{
    internal class Conversation : ConversationBase
    {
        readonly static string IP = "119.49.86.85";
        readonly static short Port = 25535;

        public delegate void OnBitmapHandler(Bitmap Data);

        public bool IsMainHost { get; private set; }
        public event Action? OnHosted;
        public event Action ?UnHosted;
        public event OnBitmapHandler ?OnBitmap;
        void PaassiveModole(object? Param)
        {
            NetworkStream Stream = Client.GetStream();
            while (true)
            {
                try
                {
                    byte[] HeadData = RecvInsertRange(Stream, Marshal.SizeOf(typeof(DataHead)));

                    DataHead Head = (DataHead)BytesToStuct(HeadData, typeof(DataHead));
                    if (Head.Rev != 255)
                        continue;
                    ConversationType ct = (ConversationType)Head.ct;
                    CommandType cm = (CommandType)Head.cmt;

                    switch(ct)
                    {
                        case ConversationType.ServerCommand:
                            switch(cm)
                            {
                                case CommandType.Test:
                                    {
                                        byte[] Buf = RecvInsertRange(Stream, (int)Head.Length);
                                        MessageBox.Show(Encoding.ASCII.GetString(Buf));
                                        break;
                                    }
                                case CommandType.SetName:
                                    {
                                        byte[] Buf = RecvInsertRange(Stream, (int)Head.Length);
                                        if (Buf[0] == 1)
                                        {
                                            IsMainHost = true;
                                            OnHosted?.Invoke();
                                        }
                                        else
                                        {
                                            IsMainHost = false;
                                            UnHosted?.Invoke();
                                        }
                                        break;
                                    }
                            }
                            break;
                        case ConversationType.ServerData:
                            switch(cm)
                            {
                                case CommandType.Test:
                                    byte[] Buf = RecvInsertRange(Stream, (int)Head.Length);
                                    break;
                                case CommandType.BeginData:
                                    {
                                        var RecvedData = RecvInsertRange(Stream, (int)Head.Length);

                                        if (RecvedData != null && OnBitmap != null)
                                        {
                                            try
                                            {
                                                var Bp = ImagingMedu.GetBitmap(RecvedData);
                                                OnBitmap?.Invoke(Bp);
                                            }
                                            catch (Exception) { }
                                        }
                                        break;
                                    }
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    break;
                }
            }

        }

        public Conversation() : base(IP, Port)
        {
            new Thread(new ParameterizedThreadStart(PaassiveModole)) { IsBackground = true }.Start();
        }
        public void Test()
        {
            TransHead(ConversationType.ClientCommand, CommandType.Test, 12);
            string VV = "Hello World";

            StreamL.Write(Encoding.ASCII.GetBytes(VV));
        }
        public void PublishImg(byte [] Data)
        {
            TransHead(ConversationType.ClientData, CommandType.EndData, (uint)Data.Length);
            StreamL.Write(Data);
        }
        public void SetName(string Name)
        {
            byte [] Data = Encoding.UTF8.GetBytes(Name);
            TransHead(ConversationType.ClientCommand, CommandType.SetName, (uint)Data.Length);

            StreamL.Write(Data);
        }
       
    }
}
