// Heiken_Ashi_T3_WTF Indicator  (WTF v4)
// Last changed on 10/19/2011
// Part of Forex Strategy Builder & Forex Strategy Trader
// Website http://forexsb.com/
// Copyright (c) 2006 - 2010 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Heiken Ashi T3 Indicator
    /// </summary>
    public class Heiken_Ashi_T3_WTF : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
		/// v2 - latest version
		/// v 3 -- no changes
		/// v 4 -- swap DataBars and hBars for significant performance improvement
		///     -- replace comment "modification version 4" with "modification version 4"		
		///     -- comment out returns for Indicator Logic Functions		
        /// </summary>
        public Heiken_Ashi_T3_WTF(SlotTypes slotType)
        {
            // General properties
            IndicatorName   = "Triple ema ha (WTF v4)";
            PossibleSlots   = SlotTypes.OpenFilter;
			CustomIndicator = true;
			SeparatedChart = true;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;
            IndParam.IndicatorType = TypeOfIndicator.Additional;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "There is a signal formation",
            };
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Indicator's logic.";

            IndParam.ListParam[1].Caption  = "T3Original";
            IndParam.ListParam[1].ItemList = new string[]
            {
                "True",
                "False"
            };
            IndParam.ListParam[1].Index   = 0;
            IndParam.ListParam[1].Text    = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Is .";
			
			IndParam.ListParam[2].Caption  = "T3Average";
            IndParam.ListParam[2].ItemList = new string[]
            {
                "True",
                "False"
            };
            IndParam.ListParam[2].Index   = 0;
            IndParam.ListParam[2].Text    = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "Is .";
			
			//This block is suitable for WTF ListParam 
			IndParam.ListParam[3].Caption = "Higher Time Frame Reference";
			IndParam.ListParam[3].ItemList = new string[] { "Automatic", "5 Minutes", "15 Minutes", "30 Minutes", "1 Hour", "4 Hours", "1 Day", "1 Week"};
            if (Period == DataPeriods.min1) 		IndParam.ListParam[3].Index    = 1;
            else if (Period == DataPeriods.min5) 	IndParam.ListParam[3].Index    = 2;
            else if (Period == DataPeriods.min15) 	IndParam.ListParam[3].Index    = 3;
            else if (Period == DataPeriods.min30)	IndParam.ListParam[3].Index    = 4;
            else if (Period == DataPeriods.hour1)	IndParam.ListParam[3].Index    = 5;
            else if (Period == DataPeriods.hour4)	IndParam.ListParam[3].Index    = 6;
            else if (Period == DataPeriods.day)		IndParam.ListParam[3].Index    = 7;
            IndParam.ListParam[3].Text     = IndParam.ListParam[3].ItemList[IndParam.ListParam[3].Index];
            IndParam.ListParam[3].Enabled  = true;
            IndParam.ListParam[3].ToolTip  = "Chose higher time frame as compared to current chart period for this indicator.";		
			
			IndParam.ListParam[4].Caption  = "Smoothing method";
            IndParam.ListParam[4].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[4].Index    = (int)MAMethod.Simple;
            IndParam.ListParam[4].Text     = IndParam.ListParam[4].ItemList[IndParam.ListParam[4].Index];
            IndParam.ListParam[4].Enabled  = true;
            IndParam.ListParam[4].ToolTip  = "The smoothing method";
			
			IndParam.NumParam[0].Caption = "MaPeriod";
            IndParam.NumParam[0].Value   = 10;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period .";
 
            IndParam.NumParam[1].Caption = "Step";
            IndParam.NumParam[1].Value   = 10;
            IndParam.NumParam[1].Min     = 1;
            IndParam.NumParam[1].Max     = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The period .";
			
			IndParam.NumParam[2].Caption = "T3Hot";
            IndParam.NumParam[2].Value   = 0.618;
            IndParam.NumParam[2].Min     = 0.001;
            IndParam.NumParam[2].Max     = 200;
			IndParam.NumParam[2].Point   = 3;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The .";
 
 			IndParam.NumParam[3].Caption = "BetterFormula";
            IndParam.NumParam[3].Value   = 1;
            IndParam.NumParam[3].Min     = 0;
            IndParam.NumParam[3].Max     = 1;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "0 for false \n1 for true.";


            // The CheckBox parameters
/// start WTF modfication 1 version 4
			IndParam.CheckParam[0].Caption = "Force previous WTF value";
            IndParam.CheckParam[0].Checked = true;
            IndParam.CheckParam[0].Enabled = false;
            IndParam.CheckParam[0].ToolTip = "WTF are set to always use their previous value.";
/// end WTF modfication 1 version 4
 
            return;
        }
 
        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
			// Reading the parameters
			bool     T3Original = IndParam.ListParam[1].Text == "True";			
			bool   T3Average = IndParam.ListParam[2].Text == "True";
			MAMethod  maMethod  = (MAMethod)IndParam.ListParam[4].Index;

			int    	  MaPeriod  = (int)IndParam.NumParam[0].Value;
			double    Step    	= IndParam.NumParam[1].Value;
			double    T3Hot    	= IndParam.NumParam[2].Value;
			bool   BetterFormula = IndParam.NumParam[3].Value == 1;
            int 	  iPrvs 	= IndParam.CheckParam[0].Checked ? 1 : 0;
 
			
			
			
			// Convert to Higher Time Frame ---------------------------------------------
			DataPeriods htfPeriod = DataPeriods.week;
			double[] hfOpen = new double[Bars];
			double[] hfClose = new double[Bars];
			double[] hfHigh = new double[Bars];
			double[] hfLow = new double[Bars];
			double[] hfVolume = new double[Bars];
			double[] hfPrice = new double[Bars];
			int[] 	 hIndex = new int[Bars];
			int      iFrame;
			int      hBars;
		
		
			switch (IndParam.ListParam[3].Index)
			{
				case 1: htfPeriod = DataPeriods.min5; break;
				case 2: htfPeriod = DataPeriods.min15; break;
				case 3: htfPeriod = DataPeriods.min30; break;
				case 4: htfPeriod = DataPeriods.hour1; break;
				case 5: htfPeriod = DataPeriods.hour4; break;
				case 6: htfPeriod = DataPeriods.day; break;
				case 7: htfPeriod = DataPeriods.week; break;
			}		
			int err1 = HigherTimeFrame(Period, htfPeriod, out hIndex, out hBars, out iFrame, out hfHigh, out hfLow, out hfOpen, out hfClose, out hfVolume);
			// int err2 = HigherBasePrice(basePrice, hBars, hfHigh, hfLow, hfOpen, hfClose, out hfPrice);	
			if (err1==1) return;	
			//-----------------------------------------------------------------------




			double[] ExtMapBuffer1 = new double[Bars];
			double[] ExtMapBuffer2 = new double[Bars];
			double[] ExtMapBuffer3 = new double[Bars];
			double[] ExtMapBuffer4 = new double[Bars];
			double[] allowlong = new double[Bars];
			double[] allowshort = new double[Bars];
			double[] adDisplay = new double[Bars];
	  
			double maOpen, maClose, maLow, maHigh;
			double[] maOpen1 = new double[Bars];
			double[] maClose1 = new double[Bars];
			double[] maLow1 = new double[Bars];
			double[] maHigh1 = new double[Bars];
			double[] trace = new double[Bars];
			
			double haOpen, haClose, haLow, haHigh;
			int    pointModifier;
			
			if (Digits==3 || Digits==5)
					pointModifier = 10;
			else  	pointModifier = 1; 
			

			
			double alpha;
			double c1;
			double c2;
			double c3;
			double c4;

			double a  = T3Hot;
             c1 = -a*a*a;
             c2 =  3*(a*a+a*a*a);
             c3 = -3*(2*a*a+a+a*a*a);
             c4 = 1+3*a+a*a*a+3*a*a;
			 
			if (T3Original)
				alpha = 2.0/(1.0 + MaPeriod);
			else alpha = 2.0/(2.0 + (MaPeriod-1.0)/2.0); 
			
			double[,] emas = new double[Bars,24];
			
			int iFirstBar = 1 + iPrvs + MaPeriod;
			
			// WTF -- change from Price data to hfPrice data
			maOpen1  = MovingAverage(MaPeriod,0,maMethod, hfOpen);
			maClose1 = MovingAverage(MaPeriod,0,maMethod, hfClose);
			maLow1   = MovingAverage(MaPeriod,0,maMethod, hfLow);
			maHigh1  = MovingAverage(MaPeriod,0,maMethod, hfHigh);
			
			int L=0;
			int S=0;
			
			// WTF -- iterate up to hBars instead of Bars
            for (int iBar = 2; iBar < hBars; iBar++)
			{
				if (T3Average)
				{
					// WTF -- change from Price data to hfPrice data
					maOpen  = iT3(hfOpen[iBar], iBar, 0, alpha, c1, c2, c3, c4, emas);
					maClose = iT3(hfClose[iBar], iBar, 6, alpha, c1, c2, c3, c4, emas);
					maLow   = iT3(hfLow[iBar], iBar,12, alpha, c1, c2, c3, c4, emas);
					maHigh  = iT3(hfHigh[iBar], iBar,18, alpha, c1, c2, c3, c4, emas);
				}	
				
				else
				{	
					maOpen  = maOpen1[iBar];
					maClose = maClose1[iBar];
					maLow   = maLow1[iBar];
					maHigh  = maHigh1[iBar];
				}
				trace[iBar] = maOpen;
			
				if (BetterFormula) 
				{
					if (maHigh != maLow)
						haClose = (maOpen+maClose)/2+(((maClose-maOpen)/(maHigh-maLow))*Math.Abs((maClose-maOpen)/2));
					else  haClose = (maOpen+maClose)/2; 
				}
				else 
				{		
						haClose = (maOpen+maHigh+maLow+maClose)/4;
				}		
				haOpen   = (ExtMapBuffer3[iBar-1]+ExtMapBuffer4[iBar-1])/2;
				haHigh   = Math.Max(maHigh, Math.Max(haOpen,haClose));
				haLow    = Math.Min(maLow,  Math.Min(haOpen,haClose));
				
				if (haOpen<haClose) 
				{ 
					ExtMapBuffer1[iBar]=haLow;  
					ExtMapBuffer2[iBar]=haHigh; 
				} 
				else                
				{ 
					ExtMapBuffer1[iBar]=haHigh; 
					ExtMapBuffer2[iBar]=haLow;  
				} 
                               
				ExtMapBuffer3[iBar]=haOpen;
                ExtMapBuffer4[iBar]=haClose;
							   
				if (Step>0)
				{
					if( Math.Abs(ExtMapBuffer1[iBar]-ExtMapBuffer1[iBar-1]) < Step*pointModifier*Point ) ExtMapBuffer1[iBar]=ExtMapBuffer1[iBar-1];
					if( Math.Abs(ExtMapBuffer2[iBar]-ExtMapBuffer2[iBar-1]) < Step*pointModifier*Point ) ExtMapBuffer2[iBar]=ExtMapBuffer2[iBar-1];
					if( Math.Abs(ExtMapBuffer3[iBar]-ExtMapBuffer3[iBar-1]) < Step*pointModifier*Point ) ExtMapBuffer3[iBar]=ExtMapBuffer3[iBar-1];
					if( Math.Abs(ExtMapBuffer4[iBar]-ExtMapBuffer4[iBar-1]) < Step*pointModifier*Point ) ExtMapBuffer4[iBar]=ExtMapBuffer4[iBar-1];
				}
				
				// WTF -- subtract iPrvs here to move WTF signals to match ending time of WTF bar
				if (ExtMapBuffer3[iBar-1-iPrvs]<ExtMapBuffer4[iBar-1-iPrvs] && ExtMapBuffer4[iBar-2-iPrvs]<ExtMapBuffer3[iBar-2-iPrvs])
				{
					if (L == 0)
					{
						allowlong[iBar] = 1;
						allowshort[iBar] = 0;
						adDisplay[iBar] = 1;
						L = 1;
						S = 0;
					}
				}
				if (ExtMapBuffer3[iBar-1-iPrvs]>ExtMapBuffer4[iBar-1-iPrvs] && ExtMapBuffer4[iBar-2-iPrvs]>ExtMapBuffer3[iBar-2-iPrvs])
				{
					if (S == 0)
					{
						allowlong[iBar] = 0;
						allowshort[iBar] = 1;
						adDisplay[iBar] = -1;
						L = 0;
						S = 1;
					}
				}
			}
			
			// Convert to Current Time Frame ----------------------------------------------
/// WTF modification 2 -- copy of wider time frame array of values -- not used in this indicator because no need for IndicatorLogic step due to allowlong/short being 1 and 0 already

			// only expand from WTF to current time frame
			int err3 = CurrentTimeFrame(hIndex, hBars, ref allowlong);
			int err4 = CurrentTimeFrame(hIndex, hBars, ref allowshort);
			int err5 = CurrentTimeFrame(hIndex, hBars, ref adDisplay);

			// if any error, return out of calculation and indicator fails silently
			if (err3 == 1 || err4 == 1 | err5 == 1)
			{
				return;
			}
/// end WTF modfication 2 version 4
			//-----------------------------------------------------------------------------	


            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp();
            Component[0].CompName  = "Allow long entry";
            Component[0].DataType  = IndComponentType.AllowOpenLong;
            Component[0].ChartType = IndChartType.NoChart;
            Component[0].FirstBar  = iFirstBar;
            Component[0].Value     = allowlong;

            Component[1] = new IndicatorComp();
            Component[1].CompName  = "Allow short entry";
            Component[1].DataType  = IndComponentType.AllowOpenShort;
            Component[1].ChartType = IndChartType.NoChart;
            Component[1].FirstBar  = iFirstBar;
            Component[1].Value     = allowshort;

            Component[2] = new IndicatorComp();
            Component[2].CompName  = "Signal Display";
            Component[2].DataType  = IndComponentType.IndicatorValue;
            Component[2].ChartType = IndChartType.Histogram;
            Component[2].FirstBar  = iFirstBar;
            Component[2].Value     = adDisplay;


/// start WTF modfication 3 version 4
			// not used in this indicator because the IndicatorLogic function is not used
/// end WTF modfication 3 version 4

            return;
        }
		
		internal double iT3(double price,int iBar,int buffer, double alpha, double c1, double c2, double c3, double c4, double[,] emas)
		{
			int i = iBar-1;
			
			if (i < 1)
			{
				emas[i,0+buffer] = price;
				emas[i,1+buffer] = price;
				emas[i,2+buffer] = price;
				emas[i,3+buffer] = price;
				emas[i,4+buffer] = price;
				emas[i,5+buffer] = price;
			}
			else
			{
				emas[i,0+buffer] = emas[i-1,0+buffer]+alpha*(price           -emas[i-1,0+buffer]);
				emas[i,1+buffer] = emas[i-1,1+buffer]+alpha*(emas[i,0+buffer]-emas[i-1,1+buffer]);
				emas[i,2+buffer] = emas[i-1,2+buffer]+alpha*(emas[i,1+buffer]-emas[i-1,2+buffer]);
				emas[i,3+buffer] = emas[i-1,3+buffer]+alpha*(emas[i,2+buffer]-emas[i-1,3+buffer]);
				emas[i,4+buffer] = emas[i-1,4+buffer]+alpha*(emas[i,3+buffer]-emas[i-1,4+buffer]);
				emas[i,5+buffer] = emas[i-1,5+buffer]+alpha*(emas[i,4+buffer]-emas[i-1,5+buffer]);
				 
			}
			
			return(c1*emas[i,5+buffer] + c2*emas[i,4+buffer] + c3*emas[i,3+buffer] + c4*emas[i,2+buffer]);
		}

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            EntryFilterLongDescription  = "there is a signal formation";
            EntryFilterShortDescription = "there is a signal formation";

            return;
        }

        /// <summary>
        /// Indicator to string
        /// </summary>
        public override string ToString()
        {
            string sString = IndicatorName;

            return sString;
        }
  



 		/// <summary>
        /// Convert current time frame to higher time frame
        /// </summary>
/// start WTF modfication 4 version 4
		protected static int HigherTimeFrame(DataPeriods currentTF, DataPeriods higherTF, out int[] hIndex, out int hBars, out int iFrame, out double[] hfHigh, out double[] hfLow, out double[] hfOpen, out double[] hfClose, out double[] hfVolume)
		{
			hfOpen = new double[Bars];
			hfClose = new double[Bars];
			hfHigh = new double[Bars];
			hfLow = new double[Bars];
			hfVolume = new double[Bars];
			hIndex = new int[Bars];
			hBars =0;
			iFrame = 1;

			// verify user input, if higher TF is not greater than current TF then return to exit function
			if ((int)higherTF <=  (int)currentTF) {
				return 1;
			}
			
			// Frame Calculation
			if (higherTF>currentTF)
				iFrame = (int)((int)higherTF/(int)currentTF);
			else
				iFrame = 1;
			if (higherTF==DataPeriods.week && currentTF==DataPeriods.day) iFrame=5;
			
			TimeSpan tsCurrent = new TimeSpan();
			switch (currentTF)
			{
				case DataPeriods.min1:
					tsCurrent = TimeSpan.FromMinutes (1);
					break;
				case DataPeriods.min5:
					tsCurrent = TimeSpan.FromMinutes (5);
					break;
				case DataPeriods.min15:
					tsCurrent = TimeSpan.FromMinutes (15);
					break;
				case DataPeriods.min30:
					tsCurrent = TimeSpan.FromMinutes (30);
					break;
				case DataPeriods.hour1:
					tsCurrent = TimeSpan.FromHours (1);
					break;
				case DataPeriods.hour4:
					tsCurrent = TimeSpan.FromHours (4);
					break;
			}
			TimeSpan tsHTF = new TimeSpan();
			switch (higherTF)
			{
				case DataPeriods.min5:
					tsHTF = TimeSpan.FromMinutes (5);
					break;
				case DataPeriods.min15:
					tsHTF = TimeSpan.FromMinutes (15);
					break;
				case DataPeriods.min30:
					tsHTF = TimeSpan.FromMinutes (30);
					break;
				case DataPeriods.hour1:
					tsHTF = TimeSpan.FromHours (1);
					break;
				case DataPeriods.hour4:
					tsHTF = TimeSpan.FromHours (4);
					break;
				case DataPeriods.day:
					tsHTF = TimeSpan.FromDays (1);
					break;
				case DataPeriods.week:
					tsHTF = TimeSpan.FromDays (7);
					break;
			}


			// set all HTFs to start from first modulo period in data series and cut off earlier data 
			// set iStartBar back one so htf close is written on last lower time frame bar of interval (eg, htf = 1 hour, write close on 1:55, 2:55, 3:55 bar instead of on 2:00, 3:00, 4:00 bar)
			// if htf is week, sync to close on Fridays
			int iStartBar = 0;
			DateTime dtStart = new DateTime (Date[0].Year, Date[0].Month, Date[0].Day, 0, 0, 0);
			if (higherTF == DataPeriods.week)
			{
				while (dtStart.DayOfWeek != DayOfWeek.Friday)
				{
					dtStart = dtStart.AddDays(-1);
				}
			}

			for (int iBar=1; iBar<Bars; iBar++) {
				if ((Date[iBar]-dtStart).Ticks % tsHTF.Ticks == 0)
				{
					iStartBar = iBar;
					iBar = Bars;
				}
			}

			// loop through bars, figure difference between this bar time and period starting bar time
			// if difference equals time span, means new htf bar open
			// if greater than time span, resync the cycle, in case of crossing weekend or holidays, or a few lost bars
			// hIndex[hBar] -- to keep track of where HTF values change when going to LTF, which has a lot more bars; should be first LTF bar of the LTF bars that are in the HTF bar
			int hBar = 0;
			int iStartHTFBar = iStartBar;
			for (int iBar=iStartBar; iBar<Bars; iBar++)
			{
				// new higher time frame bar, initialize with values
				if ((Date[iBar] - Date[iStartHTFBar]).Ticks  % tsHTF.Ticks == 0)
				{
					hBar++;
					iStartHTFBar = iBar;
					hfOpen[hBar]   = Open[iBar];
					hfClose[hBar]  = Close[iBar];
					hfHigh[hBar]   = High[iBar];
					hfLow[hBar]    = Low[iBar];
					hfVolume[hBar] = Volume[iBar];
					hIndex[hBar] = iBar;
				}
				// progressing through higher time frame bar or at end, update High, Low, Close and Volume
				else if (Date[iBar] - Date[iStartHTFBar] < tsHTF)
				{
					hfClose[hBar]  = Close[iBar];
					if (High[iBar] > hfHigh[hBar])	hfHigh[hBar] = High[iBar];
					if (Low[iBar]  < hfLow[hBar])	hfLow[hBar] = Low[iBar];
					hfVolume[hBar] += Volume[iBar];
				}
				// must have lost some bars, so get back in sync, add values for closing of partial but completed htf bar
				else if (Date[iBar] - Date[iStartHTFBar] > tsHTF)
				{
					// set this bar as opening HTF bar, initialize
					hBar++;
					hfOpen[hBar]   = Open[iBar];
					hfClose[hBar]  = Close[iBar];
					hfHigh[hBar]   = High[iBar];
					hfLow[hBar]    = Low[iBar];
					hfVolume[hBar] = Volume[iBar];
					hIndex[hBar] = iBar;
					
					for (int iSyncBar = iBar; iSyncBar<Bars; iSyncBar++) {
						// check if have found next HTF bar against last known HTF start bar
						if ((Date[iSyncBar] - Date[iStartHTFBar]).Ticks % tsHTF.Ticks == 0)
						{
							//have found next HTF bar, initialize
							hBar++;
							iStartHTFBar = iSyncBar;
							iBar = iSyncBar;
							hfOpen[hBar]   = Open[iSyncBar];
							hfClose[hBar]  = Close[iSyncBar];
							hfHigh[hBar]   = High[iSyncBar];
							hfLow[hBar]    = Low[iSyncBar];
							hfVolume[hBar] = Volume[iSyncBar];
							hIndex[hBar] = iSyncBar;
							iSyncBar = Bars;
						}
						else // not found yet, only update
						{
							hfClose[hBar]  = Close[iSyncBar];
							if (High[iSyncBar] > hfHigh[hBar])	hfHigh[hBar] = High[iSyncBar];
							if (Low[iSyncBar]  < hfLow[hBar])	hfLow[hBar] = Low[iSyncBar];
							hfVolume[hBar] += Volume[iSyncBar];
						}
					}
				}

			}	
			hBars = hBar + 1;
			return 0;
		}


		/// <summary>
        /// Convert higher time frame to current time frame
        /// </summary>
		protected static int CurrentTimeFrame(int[] hIndex, int hBars, ref double[] hIndicator)
		{
			double[] hBuffer = new double[Bars];
			int hBar = 0;

			for (int iBar=0; iBar<Bars; iBar++)
			{	
				if (hBar < hBars-1) // protect against going out of HTF indicator range
				{
					if (iBar < hIndex[hBar+1])
					{
						hBuffer[iBar] = hIndicator[hBar];
					}
					else {
						hBar++;
						hBuffer[iBar] = hIndicator[hBar];
					}
				}
				// else reached end of HTF close values, fill in with rest of incomplete HTF bar values
				else {
					hBuffer[iBar] = hIndicator[hBar];
				}
			}
			hIndicator = new double[Bars];
			hIndicator = hBuffer;
			return 0;
		}
/// end WTF modfication 4 version 4
				
		/// <summary>
        /// Calculate base price for higher time frame data
        /// </summary>
		protected static int HigherBasePrice(BasePrice basePrice, int hBars, double[] hfHigh, double[] hfLow, double[] hfOpen, double[] hfClose, out double[] hfPrice)
		{
			hfPrice = new double[Bars];
			
			switch (basePrice)
			{
				case BasePrice.Open:	hfPrice = hfOpen;	break;
				case BasePrice.Close:	hfPrice = hfClose;	break;
				case BasePrice.High:	hfPrice = hfHigh;	break;
				case BasePrice.Low:		hfPrice = hfLow;	break;
				case BasePrice.Median:
					for (int iBar=0; iBar<hBars; iBar++)
					{
						hfPrice[iBar] = (hfLow[iBar] + hfHigh[iBar]) / 2;
					}
					break;
				case BasePrice.Typical:
					for (int iBar=0; iBar<hBars; iBar++)
					{
						hfPrice[iBar] = (hfLow[iBar] + hfHigh[iBar] + hfClose[iBar]) / 3;
					}
					break;
				case BasePrice.Weighted:
					for (int iBar=0; iBar<hBars; iBar++)
					{
						hfPrice[iBar] = (hfOpen[iBar] + hfLow[iBar] + hfHigh[iBar] + hfClose[iBar]) / 4;
					}
					break;
				default:
					break;
			}
			return 0;
		}

    }
}

