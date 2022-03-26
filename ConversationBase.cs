using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DrawAndGuess
{
    internal class ConversationBase
    {
        readonly protected TcpClient Client;
        readonly protected NetworkStream StreamL;
        public enum ConversationType
        {
            ServerCommand = 0,
            ServerData = 1,
            ClientCommand = 2,
            ClientData = 3
        }
        public enum CommandType
        {
            SetName = 0,
            BeginData = 1,
            EndData = 2,
            Test = 3
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DataHead
        {
            public uint Rev;
            public byte ct;
            public byte cmt;
            public uint Length;
        }

        protected ConversationBase(string IP, int Port)
        {
            Client = new TcpClient();
            Client.Connect(IP, Port);
            StreamL = Client.GetStream();
        }

        protected byte[] RecvInsertRange(NetworkStream Stream, int Size)
        {
            var RecvedData = new byte[Size];
            int RecvedSize = 0;

            do
            {
                RecvedSize += Stream.Read(RecvedData, RecvedSize, (int)(Size - RecvedSize));
            } while (RecvedSize < Size);
            return RecvedData;
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
