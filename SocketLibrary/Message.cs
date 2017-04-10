using System;
using System.Net.Sockets;

namespace SocketLibrary
{
    public class Message
    {
        /// <summary>
        /// 枚举消息类型
        /// </summary>
        public enum CommandType : byte
        {
            /// <summary>
            /// 发送消息
            /// </summary>
            SendMessage = 1,
            /// <summary>
            /// 心跳包
            /// </summary>
            Seartbeat = 2
        }

        /// <summary>
        /// 消息连接名
        /// </summary>
        public string ConnectionName;
        /// <summary>
        /// 消息长度
        /// </summary>
        public int MessageLength;
        /// <summary>
        /// 消息类型
        /// </summary>
        public CommandType Command;
        /// <summary>
        /// 主版本号
        /// </summary>
        public byte MainVersion;
       /// <summary>
        ///  次版本号
       /// </summary>
        public byte SecondVersion;
        /// <summary>
        /// 消息内容
        /// </summary>
        public string MessageBody;
        /// <summary>
        /// 消息是否已发出
        /// </summary>
        public bool Sent;

        
        public Message()
        {
            ConnectionName = null;
            Sent = false;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="command">消息类型</param>
        /// <param name="messageBody">消息内容</param>
        public Message(CommandType command, string messageBody)
            : this(command, 0, messageBody)
        {
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="command">消息类型</param>
        /// <param name="mainVersion">主版本号</param>
        /// <param name="messageBody">消息内容</param>
        public Message(CommandType command, byte mainVersion, string messageBody)
            : this(command, mainVersion, 0, messageBody)
        {
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="command">消息类型</param>
        /// <param name="mainVersion">主版本号</param>
        /// <param name="secondVersion">次版本号</param>
        /// <param name="messageBody">消息内容</param>
        public Message(CommandType command, byte mainVersion, byte secondVersion, string messageBody)
            : this()
        {
            this.Command = command;
            this.MainVersion = mainVersion;
            this.SecondVersion = secondVersion;
            this.MessageBody = messageBody;
        }
        /// <summary>
        /// 转换成字节
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            this.MessageLength = 7 + SocketFactory.DefaultEncoding.GetByteCount(this.MessageBody);//计算消息总长度。消息头长度为7加上消息体的长度。
            byte[] buffer = new byte[this.MessageLength];
            //先将长度的4个字节写入到数组中。
            BitConverter.GetBytes(this.MessageLength).CopyTo(buffer, 0);
            //将CommandHeader写入到数组中
            buffer[4] = (byte)this.Command;
            //将主版本号写入到数组中
            buffer[5] = (byte)this.MainVersion;
            //将次版本号写入到数组中
            buffer[6] = (byte)this.SecondVersion;

            //消息头已写完，现在写消息体。
            byte[] body = new byte[this.MessageLength - 7];
            SocketFactory.DefaultEncoding.GetBytes(this.MessageBody).CopyTo(buffer, 7);
            return buffer;
        }
    }
}
