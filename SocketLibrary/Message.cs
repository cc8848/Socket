using System;
using System.Net.Sockets;

namespace SocketLibrary
{
    public class Message
    {
        /// <summary>
        /// ö����Ϣ����
        /// </summary>
        public enum CommandType : byte
        {
            /// <summary>
            /// ������Ϣ
            /// </summary>
            SendMessage = 1,
            /// <summary>
            /// ������
            /// </summary>
            Seartbeat = 2
        }

        /// <summary>
        /// ��Ϣ������
        /// </summary>
        public string ConnectionName;
        /// <summary>
        /// ��Ϣ����
        /// </summary>
        public int MessageLength;
        /// <summary>
        /// ��Ϣ����
        /// </summary>
        public CommandType Command;
        /// <summary>
        /// ���汾��
        /// </summary>
        public byte MainVersion;
       /// <summary>
        ///  �ΰ汾��
       /// </summary>
        public byte SecondVersion;
        /// <summary>
        /// ��Ϣ����
        /// </summary>
        public string MessageBody;
        /// <summary>
        /// ��Ϣ�Ƿ��ѷ���
        /// </summary>
        public bool Sent;


        public Message()
        {
            ConnectionName = null;
            Sent = false;
        }
        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="command">��Ϣ����</param>
        /// <param name="messageBody">��Ϣ����</param>
        public Message(CommandType command, string messageBody)
            : this(command, 0, messageBody)
        {
        }
        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="command">��Ϣ����</param>
        /// <param name="mainVersion">���汾��</param>
        /// <param name="messageBody">��Ϣ����</param>
        public Message(CommandType command, byte mainVersion, string messageBody)
            : this(command, mainVersion, 0, messageBody)
        {
        }
        /// <summary>
        /// ��ʼ��
        /// </summary>
        /// <param name="command">��Ϣ����</param>
        /// <param name="mainVersion">���汾��</param>
        /// <param name="secondVersion">�ΰ汾��</param>
        /// <param name="messageBody">��Ϣ����</param>
        public Message(CommandType command, byte mainVersion, byte secondVersion, string messageBody)
            : this()
        {
            this.Command = command;
            this.MainVersion = mainVersion;
            this.SecondVersion = secondVersion;
            this.MessageBody = messageBody;
        }
        /// <summary>
        /// ת�����ֽ�
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            this.MessageLength = 7 + SocketFactory.DefaultEncoding.GetByteCount(this.MessageBody);//������Ϣ�ܳ��ȡ���Ϣͷ����Ϊ7������Ϣ��ĳ��ȡ�
            byte[] buffer = new byte[this.MessageLength];
            //�Ƚ����ȵ�4���ֽ�д�뵽�����С�
            BitConverter.GetBytes(this.MessageLength).CopyTo(buffer, 0);
            //��CommandHeaderд�뵽������
            buffer[4] = (byte)this.Command;
            //�����汾��д�뵽������
            buffer[5] = (byte)this.MainVersion;
            //���ΰ汾��д�뵽������
            buffer[6] = (byte)this.SecondVersion;

            //��Ϣͷ��д�꣬����д��Ϣ�塣
            byte[] body = new byte[this.MessageLength - 7];
            SocketFactory.DefaultEncoding.GetBytes(this.MessageBody).CopyTo(buffer, 7);
            return buffer;
        }
    }
}
