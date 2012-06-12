// FST Remote Monitoring v1.0
// Part of Forex Strategy Trader
// Website http://forexsb.com/
// Copyright (c) 2009 - 2011 Miroslav Popov - All rights reserved!
// This code or any part of it cannot be used in other applications without a permission.

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Win32;

namespace Forex_Strategy_Trader
{
    class Remote_Monitoring : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Remote_Monitoring(SlotTypes slotType)
        {
            // General properties
            IndicatorName   = "Remote Monitoring";
            PossibleSlots   = SlotTypes.OpenFilter;
            CustomIndicator = true;
            WarningMessage  = "The indicator synchronizes account data with \"http://forexsb.com/fst\". You can monitor the trade process from the webpage. You have to login there with the shown unique ID key.";

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;
            IndParam.IndicatorType = TypeOfIndicator.Additional;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "Synchronize account data with the server"
            };
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Indicator's logic.";

            IndParam.ListParam[1].Caption  = "Unique user ID";
            IndParam.ListParam[1].ItemList = new string[] { GetUserID() };
            IndParam.ListParam[1].Index    = 0;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "Use this ID to login at http://forexsb.com/fst/";

            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            // Collect and send data.
            SendDataToServer();

            return;
        }

        /// <summary>
        /// Sends data to server
        /// </summary>
        void SendDataToServer()
        {
            if (!IsTimeForUpload())
                return;

            string accountData = GetAccountData();
            string serverUrl = "http://forexsb.com/fst/setrc.php";
            Uri url = new Uri(serverUrl + accountData);
            WebClient webClient = new WebClient();
            try
            {
                webClient.DownloadStringAsync(url);
            }
            catch { }

            return;
        }

        string GetAccountData()
        {
            // Collects market data.
            string userID = GetUserID();
            int connID    = Data.ConnectionID;
            string symbol = Data.Symbol;
            int period    = (int)Data.Period;
            int digits    = Data.InstrProperties.Digits;
            double bid    = Data.Bid;
            double ask    = Data.Ask;

            // Collects account data
            double balance = Data.AccountBalance;
            double equity  = Data.AccountEquity;

            // Collects position data
            string posdir = "flat";
            if (Data.PositionDirection == PosDirection.Long)  posdir = "long";
            if (Data.PositionDirection == PosDirection.Short) posdir = "short";
            double poslots   = Data.PositionLots;
            double posprice  = Data.PositionOpenPrice;
            double posprofit = Data.PositionProfit;

            // Prepare data for the server.
            string data =
                "?userid="    + userID   +
                "&connid="    + connID   +
                "&symbol="    + symbol   +
                "&period="    + period   +
                "&digits="    + digits   +
                "&bid="       + bid      +
                "&ask="       + ask      +
                "&balance="   + balance  +
                "&equity="    + equity   +
                "&posdir="    + posdir   +
                "&poslots="   + poslots  +
                "&posprice="  + posprice +
                "&posprofit=" + posprofit;

            if (System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator == ",")
                data = data.Replace(',', '.');

            return data;
        }

        /// <summary>
        /// Checks refresh time.
        /// </summary>
        bool IsTimeForUpload()
        {
            int refreshDelay = 1000;
            RegistryKey regKey = Registry.CurrentUser;
            regKey = regKey.CreateSubKey("Software\\Forex Software\\Forex Strategy Trader\\Remote Monitoring");
            string keyName = "UploadSecID" + Data.ConnectionID;
            if (regKey.GetValue(keyName) == null)
            {
                regKey.SetValue(keyName, DateTime.Now.Second.ToString());
            }
            else
            {
                int nowsec = DateTime.Now.Second;
                int oldsec = int.Parse(regKey.GetValue(keyName).ToString());
                if (nowsec < oldsec)
                    oldsec -= 60;
                refreshDelay = nowsec - oldsec;
            }

            // Refresh not more frequently than 30 seconds. 
            // Please, do not reduce this value!
            if (refreshDelay < 30)
                return false;

            regKey.SetValue(keyName, DateTime.Now.Second.ToString());

            return true;
        }

        /// <summary>
        /// Gets an unique user ID.
        /// </summary>
        static string GetUserID()
        {
            string mac = "";
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    mac = nic.GetPhysicalAddress().ToString();
                    break;
                }
            }

            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = Encoding.UTF8.GetBytes(mac);
            bs = x.ComputeHash(bs);
            StringBuilder s = new StringBuilder();
            foreach (byte b in bs)
                s.Append(b.ToString("x2").ToLower());
            string userid = s.ToString();
            userid = userid.Substring(0, 8);

            return userid;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            EntryFilterLongDescription  = "trade is allowed";
            EntryFilterShortDescription = "trade is allowed";

            return;
        }

        /// <summary>
        /// Indicator to string
        /// </summary>
        public override string ToString()
        {
            return IndicatorName;
        }
    }
}
