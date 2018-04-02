using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using UcanDotNET;
using Neural_Application.Interfaces;

namespace Neural_Application.Interfaces
{
    public struct tDebugInfo
    {
        public UInt32 uSerialNumber;
        public UInt16[] uSensorsData;
        public UInt16[] uSortSensorData;
        public float fCurrentDose;
        public byte uFlags;
    }   

    class UCanWrapper : USBcanServer
    {
        private USBcanServer canServer;
        private tCanMsgStruct[] txMessage;
        private tCanMsgStruct[] rxMessage;
        private byte[] bufArray = new byte[5000];
        private int bufIndex = 0;
        private tDebugInfo curDebugInfo;

        private int uRcvCounter = 0;
        private int[] rcvBuf;

        public int pTxCount;
        public delegate void BDGPDebugDataRcv(tDebugInfo debugData);
        public event BDGPDebugDataRcv onBDGPDebugDataRange1Rcv;
        public event BDGPDebugDataRcv onBDGPDebugDataRange2Rcv;

        public delegate void BDGPDataRcv(int[] dataArr);
        public event BDGPDataRcv onBDGPDataRcv;

        public UCanWrapper(byte channel,eUcanBaudrate baud)
        {
           canServer = new USBcanServer();
           txMessage = new tCanMsgStruct[1];
           rxMessage = new tCanMsgStruct[1024];

           rcvBuf = new int[1024];

           curDebugInfo.uSensorsData = new UInt16[12];
           curDebugInfo.uSortSensorData = new UInt16[12];

              byte bReturn = canServer.InitHardware();

                if(bReturn != (byte)eUcanReturn.USBCAN_SUCCESSFUL)
                {
 
                    Log.Add(Log.LogCategories.HARDWARE,"USB-CAN модуль не обнаружен");
                    return;
                }
                else
                {
                    bReturn = canServer.InitCan(channel,
                                                (short)baud,
                                                (int)eUcanBaudrateEx.USBCAN_BAUDEX_USE_BTR01,
                                                (int)USBCAN_AMR_ALL,
                                                (int)USBCAN_ACR_ALL,
                                                (byte)tUcanMode.kUcanModeNormal,
                                                (byte)eUcanOutputControl.USBCAN_OCR_DEFAULT);

                    if(bReturn != (byte)eUcanReturn.USBCAN_SUCCESSFUL){
 
                        Log.Add(Log.LogCategories.HARDWARE,"USB-CAN модуль невозможно проинициализировать");
                        return;
                    }
                    else
                    {
                        canServer.CanMsgReceivedEvent += UsbCanReceiveEvent;
                        canServer.StatusEvent += onStatus;

                        Log.Add(Log.LogCategories.HARDWARE, "USB-CAN модуль проинициализирован успешно");
                    }
                }

             
           
        }
        public void SendData(int ID,byte[] Data)
        {
            txMessage[0].m_dwID = ID;
            txMessage[0].m_bDLC = (byte)Data.Length;
            txMessage[0].m_bData = Data;

            canServer.WriteCanMsg(0, ref txMessage, ref pTxCount);
        }

        private void onStatus(byte bDeviceNr_p, byte bChannel_p)
        {

        }

        public void Start()
        {
            SendData(0x101, new byte [] {0x07,0x01,0x00,0x00,0x00,0x00,0x00,0x00});
        }

        public void Stop()
        {
            SendData(0x101, new byte[] { 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
        }

        private void UsbCanReceiveEvent(byte bDeviceNr_p, byte bChannel_p)
        {


            int rxPwdCount = 0;

            byte rxChannel = (byte)USBcanServer.eUcanChannel.USBCAN_CHANNEL_CH0;
            byte bRet = canServer.ReadCanMsg(ref rxChannel, ref rxMessage, ref rxPwdCount);

            //onDEBUGCallbackMsgCounts(rxPwdCount);

            if (bRet == (byte)USBcanServer.eUcanReturn.USBCAN_SUCCESSFUL)
            {
                for (int i = 0; i < rxPwdCount; i++)
                {
                   /* if (rxMessage[i].m_dwID == 0x7E1 || rxMessage[i].m_dwID == 0x7E2)
                    {
                        for (int j = 0; j < rxMessage[i].m_bDLC; j++)
                        {
                            byte[] Package = GetPacketFromStream(rxMessage[i].m_bData[j]);

                            if (Package != null)
                            {
                                PackageAnalysis(rxMessage[i].m_dwID,Package);
                                bufIndex = 0;
                            }
                        }

                    }*/
                    if (rxMessage[i].m_bData[0] == 0x07 && rxMessage[i].m_bData[1] == 0x01 && rxMessage[i].m_bData[2] == 0x00)
                    {
                        UInt16 uTime   = Convert.ToUInt16((rxMessage[i].m_bData[4] << 8) | rxMessage[i].m_bData[3]);
                        UInt16 uLength = Convert.ToUInt16((rxMessage[i].m_bData[6] << 8) | rxMessage[i].m_bData[5]);


                        if (uLength == 1024 && uTime == 0x01)
                            uRcvCounter = 0;
                        
                        //if(rcvBuf!=null)onBDGPDataRcv(rcvBuf);
                    }
                    else if (uRcvCounter<1024)
                    {
                        rcvBuf[uRcvCounter] = (rxMessage[i].m_bData[1] << 8) | rxMessage[i].m_bData[0];
                        rcvBuf[uRcvCounter+1] = (rxMessage[i].m_bData[3] << 8) | rxMessage[i].m_bData[2];
                        rcvBuf[uRcvCounter+2] = (rxMessage[i].m_bData[5] << 8) | rxMessage[i].m_bData[4];
                        rcvBuf[uRcvCounter+3] = (rxMessage[i].m_bData[7] << 8) | rxMessage[i].m_bData[6];


                        uRcvCounter += 4;
                    }
                    if(uRcvCounter == 1024)
                    {
                        if (rcvBuf != null) onBDGPDataRcv(rcvBuf);
                        uRcvCounter = 0;
                    }
                }
            }

        }
        private void PackageAnalysis(int canID,byte[] Package)
        {
            int dataLength = Package.Length - 5;
            byte cmd = Package[2];

            if (cmd == 0x01)
            {

                curDebugInfo.uSerialNumber = (UInt32)((Package[6] << 24) | (Package[5] << 16) | (Package[4] << 8) | (Package[3]));

                curDebugInfo.uSensorsData[0] = (UInt16)((Package[8] << 8) | Package[7]);
                curDebugInfo.uSensorsData[1] = (UInt16)((Package[10] << 8) | Package[9]);
                curDebugInfo.uSensorsData[2] = (UInt16)((Package[12] << 8) | Package[11]);
                curDebugInfo.uSensorsData[3] = (UInt16)((Package[14] << 8) | Package[13]);
                curDebugInfo.uSensorsData[4] = (UInt16)((Package[16] << 8) | Package[15]);
                curDebugInfo.uSensorsData[5] = (UInt16)((Package[18] << 8) | Package[17]);
                curDebugInfo.uSensorsData[6] = (UInt16)((Package[20] << 8) | Package[19]);
                curDebugInfo.uSensorsData[7] = (UInt16)((Package[22] << 8) | Package[21]);
                curDebugInfo.uSensorsData[8] = (UInt16)((Package[24] << 8) | Package[23]);
                curDebugInfo.uSensorsData[9] = (UInt16)((Package[26] << 8) | Package[25]);
                curDebugInfo.uSensorsData[10] = (UInt16)((Package[28] << 8) | Package[27]);
                curDebugInfo.uSensorsData[11] = (UInt16)((Package[30] << 8) | Package[29]);

                curDebugInfo.uSortSensorData[0] = (UInt16)((Package[32] << 8) | Package[31]);
                curDebugInfo.uSortSensorData[1] = (UInt16)((Package[34] << 8) | Package[33]);
                curDebugInfo.uSortSensorData[2] = (UInt16)((Package[36] << 8) | Package[35]);
                curDebugInfo.uSortSensorData[3] = (UInt16)((Package[38] << 8) | Package[37]);
                curDebugInfo.uSortSensorData[4] = (UInt16)((Package[40] << 8) | Package[39]);
                curDebugInfo.uSortSensorData[5] = (UInt16)((Package[42] << 8) | Package[41]);
                curDebugInfo.uSortSensorData[6] = (UInt16)((Package[44] << 8) | Package[43]);
                curDebugInfo.uSortSensorData[7] = (UInt16)((Package[46] << 8) | Package[45]);
                curDebugInfo.uSortSensorData[8] = (UInt16)((Package[48] << 8) | Package[47]);
                curDebugInfo.uSortSensorData[9] = (UInt16)((Package[50] << 8) | Package[49]);
                curDebugInfo.uSortSensorData[10] = (UInt16)((Package[52] << 8) | Package[51]);
                curDebugInfo.uSortSensorData[11] = (UInt16)((Package[54] << 8) | Package[53]);

                curDebugInfo.fCurrentDose = BitConverter.ToSingle(new byte[] {     Package[55],
                                                                                   Package[56],
                                                                                   Package[57],
                                                                                   Package[58] }, 0);

                curDebugInfo.uFlags = Package[59];



                if (canID == 0x7E1)
                {
                    onBDGPDebugDataRange1Rcv(curDebugInfo);
                }
                if (canID == 0x7E2)
                {
                    onBDGPDebugDataRange2Rcv(curDebugInfo);
                }
            }
        }
        private byte[] GetPacketFromStream(byte InputByte)
        {

            /*if (bufIndex <= 0)
                return null;*/

            if ((bufIndex > 0) && (bufArray[0] != 0xA5))
            {
                int ii = 0;
                while ((ii < bufIndex) && (bufArray[ii] != 0xA5)) { ii++; }
                Array.Copy(bufArray, ii, bufArray, 0, bufIndex - ii);
                bufIndex = bufIndex - ii;
            }

            if (bufIndex >= bufArray.Length)
            {
                Array.Copy(bufArray, 1, bufArray, 0, bufArray.Length - 1);
                bufIndex = bufArray.Length - 1;

                if ((bufIndex > 0) && (bufArray[0] != 0xA5))
                {
                    int ii = 0;

                    while ((ii < bufIndex) && (bufArray[ii] != 0xA5)) ii++;
                    Array.Copy(bufArray, ii, bufArray, 0, bufIndex - ii);
                    bufIndex = bufIndex - ii;
                }
            }

            bufArray[bufIndex] = InputByte;
            bufIndex++;

            int iD = bufIndex;
            int pD = 0;

            while (true)
            {
                if (iD < 5) break;
                if (bufArray[pD] == 0xA5)
                {
                    int len = bufArray[1 + pD];
                    if (iD >= len)
                    {
                        if (len > 0)
                        {
                            int pKS = (bufArray[pD + (len - 1)] << 8) | bufArray[pD + (len - 2)];
                            int ks = 0;

                            for (int i = 0; i < len - 2; i++)
                                ks += bufArray[i + pD];
                            if (ks == pKS)
                            {
                                byte[] result = new byte[len];

                                Array.Copy(bufArray, pD, result, 0, len);

                                return result;
                            }

                        }
                    }
                }
                iD--;
                pD++;
            }

            return null;
        }
    }
}
