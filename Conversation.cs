using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DrawAndGuess
{
    internal class Conversation
    {
        readonly byte[] OKReply = new byte[2] { 122, 122 };
        readonly string IP = "119.49.86.85";
        readonly short Port = 25535;
        readonly TcpClient Client;
        readonly NetworkStream StreamL;
        void PaassiveModole(object ?Param)
        {
            NetworkStream Stream = Client.GetStream();
            byte[] HeadData = new byte[60];
            while (true)
            {
                try
                {
                    Stream.Read(HeadData, 0, 60);
                    DataHead Head = (DataHead)BytesToStuct(HeadData, typeof(DataHead));
                    ConversationType ct = (ConversationType)Head.ct;
                    CommandType cm = (CommandType)Head.cmt;

                    if (ct == ConversationType.ServerCommand)
                    {
                        if (cm == CommandType.Test)
                        {
                            byte[] Buf = new byte[Head.Length];
                            Stream.Read(Buf, 0, Buf.Length);

                            MessageBox.Show(Encoding.ASCII.GetString(Buf));

                            TransHead(ConversationType.ClientData, CommandType.Test, 2);
                            Stream.Write(OKReply);
                        }
                    }
                    else if (ct == ConversationType.ServerData)
                    {
                        if(cm == CommandType.Test)
                        {
                            byte[] Buf = new byte[Head.Length];
                            Stream.Read(Buf, 0, Buf.Length);

                        }
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }

        }
        public enum ConversationType
        {
            ServerCommand=0,
            ServerData=1,
            ClientCommand=2,
            ClientData=3
        }

        public enum CommandType
        {
            SetName=0,
            BeginData=1,
            EndData=2,
            Test=3
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DataHead
        {
            public uint Rev;
            public byte ct;
            public byte cmt;
            public uint Length;
        }

        public Conversation()
        {
            Client = new TcpClient();
            Client.Connect(IP, Port);
            StreamL = Client.GetStream();
            new Thread(new ParameterizedThreadStart(PaassiveModole)) { IsBackground = true }.Start();
        }

        public void Test()
        {
            TransHead(ConversationType.ClientCommand, CommandType.Test, 12);
            string VV = "Hello World";
            StreamL.Write(Encoding.ASCII.GetBytes(VV));

        }
        #region Area
        public void TransHead(ConversationType ct, CommandType cm, uint Length)
        {
            DataHead Dt = new DataHead() { ct = (byte)ct, cmt = (byte)cm, Rev = 0xff, Length = Length };
            var Buffer = StructToBytes(Dt);

            StreamL.Write(Buffer, 0, Buffer.Length);
        }
        //// <summary>
        /// 结构体转byte数组
        /// </summary>
        /// <param name="structObj">要转换的结构体</param>
        /// <returns>转换后的byte数组</returns>
        public static byte[] StructToBytes(object structObj)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(structObj);
            //创建byte数组
            byte[] bytes = new byte[size];
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间
            Marshal.StructureToPtr(structObj, structPtr, false);
            //从内存空间拷到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回byte数组
            return bytes;
        }

        /// <summary>
        /// byte数组转结构体
        /// </summary>
        /// <param name="bytes">byte数组</param>
        /// <param name="type">结构体类型</param>
        /// <returns>转换后的结构体</returns>
        public static object BytesToStuct(byte[] bytes, Type type)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(type);
            //byte数组长度小于结构体的大小
            if (size > bytes.Length)
            {
                //返回空
                return null;
            }
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将byte数组拷到分配好的内存空间
            Marshal.Copy(bytes, 0, structPtr, size);
            //将内存空间转换为目标结构体
            object obj = Marshal.PtrToStructure(structPtr, type);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回结构体
            return obj;
        }
        #endregion
    }
}
