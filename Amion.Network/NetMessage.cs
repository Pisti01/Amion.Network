﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Amion.Network
{
    /// <summary>
    /// Type of a NetMessage.
    /// </summary>
    public enum MessageType : byte
    {
        /// <summary>
        /// Should only be unknown when invalid
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Indicates a message containing data defined by the application.
        /// </summary>
        Data = 1,

        /// <summary>
        /// Indicates a message containing no data. Used only for testing if the connection still exist.
        /// </summary>
        IsAlive = 2,

        /// <summary>
        /// Indicates a message containing data for verify the connection.
        /// </summary>
        Verification = 3,

        /// <summary>
        /// Indicates a message containing data for Ping requests. Unimplemented.
        /// </summary>
        Ping = 4,
    }

    /// <summary>
    /// Class for creating an outgoing network message.
    /// </summary>
    public class NetOutMessage
    {
        /// <summary>
        /// The size of the header part of the message (in bytes).
        /// </summary>
        public const int HeaderSize = 5;

        /// <summary>
        /// A MemoryStream containing the message while its writeable.
        /// </summary>
        protected MemoryStream message;

        /// <summary>
        /// A byte array containing the message after it finalized.
        /// </summary>
        protected byte[] messageArray = null;

        /// <summary>
        /// Returns the message byte array. Null if the message hasn't been closed.
        /// </summary>
        public byte[] Array => messageArray;

        /// <summary></summary>
        /// <param name="msgType">The type of the message. Defaults to 'MessageType.Data'</param>
        public NetOutMessage(MessageType msgType = MessageType.Data)
        {
            message = new MemoryStream();
            message.WriteByte((byte)msgType);
            message.Write(BitConverter.GetBytes((int)0), 0, sizeof(int));
        }

        private NetOutMessage(byte[] message) { messageArray = message; }

        /// <summary>
        /// Generates the final message as byte array and disposes the memory stream.
        /// </summary>
        public NetOutMessage Finish()
        {
            if (messageArray != null) return this;

            byte[] msgLength = BitConverter.GetBytes((int)(message.Length - 5));
            long msgPosition = message.Position;

            message.Position = 1;
            message.Write(msgLength, 0, sizeof(int));

            messageArray = message.ToArray();

            message.Dispose();
            return this;
        }

        /// <summary>
        /// Gets the message header data from a byte array.
        /// </summary>
        /// <param name="header">The array containing the header</param>
        /// <param name="messageType">The type of the message</param>
        /// <param name="messageLength">The length of the message data</param>
        public static void DecodeHeader(byte[] header, out MessageType messageType, out int messageLength)
        {
            messageType = (MessageType)header[0];
            messageLength = BitConverter.ToInt32(header, 1);
        }

        /// <summary>
        /// Create a finished message from byte arrays without creating a memory stream and the ability to write to it.
        /// </summary>
        /// <param name="msgType">The type of the message</param>
        /// <param name="arrays">Arrays to copy into the message</param>
        public static NetOutMessage CreateFinished(MessageType msgType, params ICollection<byte>[] arrays)
        {
            int arraysLength = arrays.Sum(x => x.Count);
            int offset = HeaderSize;
            var message = new byte[arraysLength + HeaderSize];

            //Fill header
            message[0] = (byte)msgType;
            BitConverter.GetBytes(arraysLength).CopyTo(message, 1);

            //Fill data
            for (int i = 0; i < arrays.Length; i++)
            {
                arrays[i].CopyTo(message, offset);
                offset += arrays[i].Count;
            }

            return new NetOutMessage(message);
        }

        //---------------------------------------------------------------------
        // Writers
        //---------------------------------------------------------------------

        /// <summary>
        /// Writes a string at the end of the message using an int for the length and Unicode.
        /// </summary>
        public void Write(String data)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(data);
            message.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
            message.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes a short at the end of the message using 2 bytes.
        /// </summary>
        public void Write(Int16 data)
        {
            message.Write(BitConverter.GetBytes(data), 0, sizeof(Int16));
        }

        /// <summary>
        /// Writes an int at the end of the message using 4 bytes.
        /// </summary>
        public void Write(Int32 data)
        {
            message.Write(BitConverter.GetBytes(data), 0, sizeof(Int32));
        }

        /// <summary>
        /// Writes a long at the end of the message using 8 bytes.
        /// </summary>
        public void Write(Int64 data)
        {
            message.Write(BitConverter.GetBytes(data), 0, sizeof(Int64));
        }

        /// <summary>
        /// Writes an unsigned short at the end of the message using 2 bytes.
        /// </summary>
        public void Write(UInt16 data)
        {
            message.Write(BitConverter.GetBytes(data), 0, sizeof(UInt16));
        }

        /// <summary>
        /// Writes an unsigned int at the end of the message using 4 bytes.
        /// </summary>
        public void Write(UInt32 data)
        {
            message.Write(BitConverter.GetBytes(data), 0, sizeof(UInt32));
        }

        /// <summary>
        /// Writes an unsigned long at the end of the message using 8 bytes.
        /// </summary>
        public void Write(UInt64 data)
        {
            message.Write(BitConverter.GetBytes(data), 0, sizeof(UInt64));
        }

        /// <summary>
        /// Writes a System.DateTime (as UTC-Ticks) at the end of the message using 8 bytes.
        /// </summary>
        public void Write(DateTime data)
        {
            long dateAsLong = data.ToUniversalTime().Ticks;
            message.Write(BitConverter.GetBytes(dateAsLong), 0, sizeof(long));
        }

        /// <summary>
        /// Writes a bool at the end of the message using a byte.
        /// </summary>
        public void Write(bool data)
        {
            message.WriteByte(BitConverter.GetBytes(data)[0]);
        }

        /// <summary>
        /// Writes a byte at the end of the message using a byte.
        /// </summary>
        public void Write(byte data)
        {
            message.WriteByte(data);
        }

        /// <summary>
        /// Writes a byte array at the end of the message.
        /// </summary>
        public void Write(byte[] data)
        {
            message.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Writes a byte array at the end of the message.
        /// </summary>
        /// <param name="data">Array to write from</param>
        /// <param name="offset">The zero-based byte offset in data at which to begin copying bytes</param>
        /// <param name="count">Maximum number of bytes to write</param>
        public void Write(byte[] data, int offset, int count)
        {
            message.Write(data, offset, count);
        }

        /// <summary>
        /// Writes a series of bytes at the end of the message.
        /// </summary>
        public void Write(IEnumerable<byte> data)
        {
            var dataArray = data.ToArray();
            message.Write(dataArray, 0, dataArray.Length);
        }

        /// <summary>
        /// Writes a Guid at the end of the message using 16 bytes.
        /// </summary>
        public void Write(Guid data)
        {
            message.Write(data.ToByteArray(), 0, 16);
        }

        //---------------------------------------------------------------------
        // Dispose
        //---------------------------------------------------------------------

        /// <summary>
        /// Releases resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Helper for Dispose()
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                message?.Dispose();
            }
        }
    }

    /// <summary>
    /// Class for an incoming network message.
    /// </summary>
    public class NetInMessage
    {
        /// <summary>
        /// The type of the message.
        /// </summary>
        protected MessageType messageType;

        /// <summary>
        /// The data of the message.
        /// </summary>
        protected byte[] messageData;

        /// <summary>
        /// The current position for reads.
        /// </summary>
        protected int readCursor = 0;

        /// <summary></summary>
        /// <param name="messageType">The type of the message</param>
        /// <param name="messageData">The data of the message</param>
        public NetInMessage(MessageType messageType, byte[] messageData)
        {
            this.messageType = messageType;
            this.messageData = messageData;
        }

        /// <summary>
        /// Reads a string from the message and moves the readCursor.
        /// </summary>
        public string ReadString()
        {
            int length = BitConverter.ToInt32(messageData, readCursor);
            string data = Encoding.Unicode.GetString(messageData, readCursor + sizeof(int), length);

            readCursor += sizeof(int) + length;

            return data;
        }

        /// <summary>
        /// Reads a short from the message and moves the readCursor.
        /// </summary>
        public Int16 ReadInt16()
        {
            Int16 data = BitConverter.ToInt16(messageData, readCursor);
            readCursor += sizeof(Int16);
            return data;
        }

        /// <summary>
        /// Reads an int from the message and moves the readCursor.
        /// </summary>
        public Int32 ReadInt32()
        {
            Int32 data = BitConverter.ToInt32(messageData, readCursor);
            readCursor += sizeof(Int32);
            return data;
        }

        /// <summary>
        /// Reads a long from the message and moves the readCursor.
        /// </summary>
        public Int64 ReadInt64()
        {
            Int64 data = BitConverter.ToInt64(messageData, readCursor);
            readCursor += sizeof(Int64);
            return data;
        }

        /// <summary>
        /// Reads an unsigned short from the message and moves the readCursor.
        /// </summary>
        public UInt16 ReadUInt16()
        {
            UInt16 data = BitConverter.ToUInt16(messageData, readCursor);
            readCursor += sizeof(UInt16);
            return data;
        }

        /// <summary>
        /// Reads an unsigned int from the message and moves the readCursor.
        /// </summary>
        public UInt32 ReadUInt32()
        {
            UInt32 data = BitConverter.ToUInt32(messageData, readCursor);
            readCursor += sizeof(UInt32);
            return data;
        }

        /// <summary>
        /// Reads an unsigned long from the message and moves the readCursor.
        /// </summary>
        public UInt64 ReadUInt64()
        {
            UInt64 data = BitConverter.ToUInt64(messageData, readCursor);
            readCursor += sizeof(UInt64);
            return data;
        }

        /// <summary>
        /// Reads a System.DateTime (as Ticks.ToLocalTime()) from the message and moves the readCursor.
        /// </summary>
        public DateTime ReadDateTime()
        {
            long data = BitConverter.ToInt64(messageData, readCursor);
            readCursor += sizeof(long);
            return new DateTime(data).ToLocalTime();
        }

        /// <summary>
        /// Reads a bool from the message and moves the readCursor.
        /// </summary>
        public bool ReadBoolean()
        {
            bool data = BitConverter.ToBoolean(messageData, readCursor);
            readCursor++;
            return data;
        }

        /// <summary>
        /// Reads a byte from the message and moves the readCursor.
        /// </summary>
        public byte ReadByte()
        {
            byte data = messageData[readCursor];
            readCursor++;
            return data;
        }

        /// <summary>
        /// Reads bytes from the message and moves the readCursor.
        /// </summary>
        /// <param name="amount">The amount of bytes to read</param>
        public byte[] ReadBytes(int amount)
        {
            byte[] data = new byte[amount];
            Buffer.BlockCopy(messageData, readCursor, data, 0, amount);
            readCursor += amount;
            return data;
        }

        /// <summary>
        /// Returns an ArraySegment referencing bytes from the message data then moves the readCursor.
        /// </summary>
        /// <param name="amount">The amount of bytes to return as ArraySegment</param>
        public ArraySegment<byte> ReadBytesAsSegment(int amount)
        {
            var arraySegment =  new ArraySegment<byte>(messageData, readCursor, amount);
            readCursor += amount;
            return arraySegment;
        }

        /// <summary>
        /// Reads a GUID from the message and moves the readCursor.
        /// </summary>
        public Guid ReadGuid()
        {
            const int guidSize = 16;
            byte[] data = new byte[guidSize];
            Buffer.BlockCopy(messageData, readCursor, data, 0, guidSize);
            readCursor += guidSize;
            return new Guid(data);
        }

        /// <summary>
        /// Get the type of this message.
        /// </summary>
        public MessageType GetMessageType()
        {
            return messageType;
        }

        /// <summary>
        /// Get the byte array that contains the data of this message.
        /// </summary>
        public byte[] GetMessageData()
        {
            return messageData;
        }
    }
}