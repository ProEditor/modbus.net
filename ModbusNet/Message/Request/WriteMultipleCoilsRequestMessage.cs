﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ModbusNet.Enum;
using ModbusNet.Extension;

namespace ModbusNet.Message.Request
{
    public class WriteMultipleCoilsRequestMessage : BaseRequestMessage
    {
        public override byte FunctionCode => FunctionCodeDefinition.WRITE_MULTIPLE_COILS;

        private ushort _shouldSendNums;

        private ushort _multipleWriteCoilsRemainByteNum;


        public int Quantity { get; set; }

        public int Address { get; set; }

        public List<bool> Values { get; set; }

        public override Span<byte> ToBinary()
        {

            _shouldSendNums = 7 + 1 + 2 + 2 + 1;//1字节的功能码，2字节的开始地址，2字节的数量，1字节的字节数量
            _multipleWriteCoilsRemainByteNum = 1 + 1 + 2 + 2 + 1;//1字节的单元标识符，1字节的功能码，2字节的开始地址，2字节的数量，1字节的地址数量
            ushort dataByteNum;

            if (Quantity % 8 == 0)
            {
                dataByteNum = (ushort)(Quantity / 8);
                _shouldSendNums += dataByteNum;
                _multipleWriteCoilsRemainByteNum += dataByteNum;
            }
            else
            {
                dataByteNum = (ushort)(Quantity / 8 + 1);
                _shouldSendNums += dataByteNum;
                _multipleWriteCoilsRemainByteNum += dataByteNum;
            }


            NativePtr = Marshal.AllocHGlobal(_shouldSendNums);
            Span<byte> nativeSpan;
            unsafe
            {
                nativeSpan = new Span<byte>(NativePtr.ToPointer(), _shouldSendNums);
            }

            BuildMbap(nativeSpan);

            byte[] addressBytes = BitConverter.GetBytes(Address).ToPlatform();

            nativeSpan[8] = addressBytes[0];
            nativeSpan[9] = addressBytes[1];


            byte[] quantityBytes = BitConverter.GetBytes(Quantity).ToPlatform();

            nativeSpan[10] = quantityBytes[0];
            nativeSpan[11] = quantityBytes[1];



            //数据的字节数量
            BitArray bitArray = new BitArray(Values.ToArray());

            nativeSpan[12] = (byte)dataByteNum;

            byte[] dataBytes = new byte[dataByteNum];
            bitArray.CopyTo(dataBytes, 0);
            for (int i = 0; i < dataBytes.Length; i++)
            {
                nativeSpan[13 + i] = dataBytes[i];
            }
            return nativeSpan;
        }

        protected override ushort GetRemainByteCount()
        {
            return _multipleWriteCoilsRemainByteNum;
        }
    }
}
