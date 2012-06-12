// Parabolic SAR Indicator - WTF v 4
// Last changed on 2/11/2012
// Part of Forex Strategy Builder v2.8.3.7+
// Website http://forexsb.com/
// This code or any part of it cannot be used in other applications without a permission.
// Copyright (c) 2006 - 2009 Miroslav Popov - All rights reserved.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Parabolic SAR Indicator -- WTF version
    /// </summary>
    public class Parabolic_SAR_WTF : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
		/// v2 -- Draw Only for OpenFilters only, fix bug of disallowing all exits when CloseFilter
		/// v 3 -- no changes
		/// v 4 -- v4 changes not needed
        /// </summary>
        public Parabolic_SAR_WTF(SlotTypes slotType)
        {
            // General properties
            IndicatorName = "Parabolic SAR (WTF v4)";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.Close;
			CustomIndicator = true;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
			// WTF v2 fix bug "Draw Only" -- only available on OpenFilter Slots, on CloseFilter will disallow any exits
            if (slotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new string[] 
                {
                    "The price is higher than the PSAR value",
					"Draw only, no entry or exit"
                };
            else if (slotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new string[]
                { 
                    "Exit the market at PSAR"
                };
            else
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Not Defined"
                };
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of the indicator.";

 			IndParam.ListParam[1].Caption = "Wider Time Frame Reference";
			IndParam.ListParam[1].ItemList = new string[] { "Automatic", "5 Minutes", "15 Minutes", "30 Minutes", "1 Hour", "4 Hours", "1 Day", "1 Week"};
            if (Period == DataPeriods.min1) 		IndParam.ListParam[1].Index    = 1;
            else if (Period == DataPeriods.min5) 	IndParam.ListParam[1].Index    = 2;
            else if (Period == DataPeriods.min15) 	IndParam.ListParam[1].Index    = 3;
            else if (Period == DataPeriods.min30)	IndParam.ListParam[1].Index    = 4;
            else if (Period == DataPeriods.hour1)	IndParam.ListParam[1].Index    = 5;
            else if (Period == DataPeriods.hour4)	IndParam.ListParam[1].Index    = 6;
            else if (Period == DataPeriods.day)		IndParam.ListParam[1].Index    = 7;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "Choose wider time frame as compared to current chart period for this indicator.";		


            IndParam.ListParam[2].Caption  = "Indicator Color";
            IndParam.ListParam[2].ItemList = new string [] 
			{
				"Blue",
				"Red",
				"Green",
				"Orange"
			};
            IndParam.ListParam[2].Index    = 0;
            IndParam.ListParam[2].Text     = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "Color of line on chart for Parabolic SAR WTF value.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Starting AF";
            IndParam.NumParam[0].Value   = 0.02;
            IndParam.NumParam[0].Min     = 0.00;
            IndParam.NumParam[0].Max     = 5.00;
            IndParam.NumParam[0].Point   = 2;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The starting value of Acceleration Factor.";

            IndParam.NumParam[1].Caption = "Increment";
            IndParam.NumParam[1].Value   = 0.02;
            IndParam.NumParam[1].Min     = 0.01;
            IndParam.NumParam[1].Max     = 5.00;
            IndParam.NumParam[1].Point   = 2;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Increment value.";

            IndParam.NumParam[2].Caption = "Maximum AF";
            IndParam.NumParam[2].Value   = 2.00;
            IndParam.NumParam[2].Min     = 0.01;
            IndParam.NumParam[2].Max     = 9.00;
            IndParam.NumParam[2].Point   = 2;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The maximum value of the Acceleration Factor.";


            // The CheckBox parameters
/// start WTF modfication 1 beta 2
		/// for Parabolic SAR, regular version dsoes not have Use Previous Values checkbox, so skip adding control for WTF version
		/// adding modification comments so numbering remains in order
/// end WTF modfication 1 beta 2

            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            double dAFMin = IndParam.NumParam[0].Value;
            double dAFInc = IndParam.NumParam[1].Value;
            double dAFMax = IndParam.NumParam[2].Value;

            // Reading the parameters
            int intDirNew;
            double dAF;
            double dPExtr;
            double dPSARNew = 0;
            int[]    aiDir  = new int[Bars];
            double[] adPSAR = new double[Bars];

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
			
			switch (IndParam.ListParam[1].Index)
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
			if (err1 ==1) return;
			//------------------------------------------------------------------------


            //----	Calculating the initial values
			// ---- for WTF version: change Open-High-Low-Close to hfOpen-hfHigh-hfLow-hfClose
            adPSAR[0] = 0;
            dAF       = dAFMin;
            intDirNew = 0;

			int iFirstBar = 0;
			// get first non-zero value -- first bars could be 0's from WTF conversion
			while (hfOpen[iFirstBar] == 0)
			{
				iFirstBar++;
			}

            if (hfClose[iFirstBar+1] > hfOpen[iFirstBar])
            {
                aiDir[iFirstBar]  = 1;
                aiDir[iFirstBar+1]  = 1;
                dPExtr    = Math.Max(hfHigh[iFirstBar], hfHigh[iFirstBar+1]);
                adPSAR[iFirstBar+1] = Math.Min(hfLow[iFirstBar],  hfLow[iFirstBar+1]);
            }
            else
            {
                aiDir[iFirstBar]  = -1;
                aiDir[iFirstBar+1]  = -1;
                dPExtr    = Math.Min(hfLow[iFirstBar],  hfLow[iFirstBar+1]);
                adPSAR[iFirstBar+1] = Math.Max(hfHigh[iFirstBar], hfHigh[iFirstBar+1]);
            }
			
			iFirstBar += 2;

			// set max to hBars, no need to iterate through extra empty values
            for (int iBar = iFirstBar; iBar < hBars; iBar++)
            {
                //----	PSAR for the current period
                if (intDirNew != 0)
                {	
                    // The direction was changed during the last period
                    aiDir[iBar]  = intDirNew;
                    intDirNew    = 0;
                    adPSAR[iBar] = dPSARNew + dAF * (dPExtr - dPSARNew);
                }
                else
                {
                    aiDir[iBar]  = aiDir[iBar - 1];
                    adPSAR[iBar] = adPSAR[iBar - 1] + dAF * (dPExtr - adPSAR[iBar - 1]);
                }

                // PSAR has to be out of the previous two bars limits
                if (aiDir[iBar] > 0 && adPSAR[iBar] > Math.Min(hfLow[iBar - 1], hfLow[iBar - 2]))
                    adPSAR[iBar] = Math.Min(hfLow[iBar - 1], hfLow[iBar - 2]);
                else if (aiDir[iBar] < 0 && adPSAR[iBar] < Math.Max(hfHigh[iBar - 1], hfHigh[iBar - 2]))
                    adPSAR[iBar] = Math.Max(hfHigh[iBar - 1], hfHigh[iBar - 2]);

                //----	PSAR for the next period

                // Calculation of the new values of flPExtr and flAF
                // if there is a new extreme price in the PSAR direction
                if (aiDir[iBar] > 0 && hfHigh[iBar] > dPExtr)
                {
                    dPExtr = hfHigh[iBar];
                    dAF    = Math.Min(dAF + dAFInc, dAFMax);
                }

                if (aiDir[iBar] < 0 && hfLow[iBar] < dPExtr)
                {
                    dPExtr = hfLow[iBar];
                    dAF    = Math.Min(dAF + dAFInc, dAFMax);
                }

                // Wheather the price reaches PSAR
                if (hfLow[iBar] <= adPSAR[iBar] && adPSAR[iBar] <= hfHigh[iBar])
                {
                    intDirNew = -aiDir[iBar];
                    dPSARNew  = dPExtr;
                    dAF       = dAFMin;
                    if (intDirNew > 0)
                        dPExtr = hfHigh[iBar];
                    else
                        dPExtr = hfLow[iBar];
                }

            }



			// Convert to Current Time Frame ----------------------------------------------
/// start WTF modfication 2 beta 2

			iFirstBar = iFirstBar*iFrame;
			if (iFirstBar>0.25*Bars) return;
			
 			int err3 = CurrentTimeFrame(hIndex, hBars, ref adPSAR);	
/// end WTF modfication 2 beta 2


            // Saving the components
            Component = new IndicatorComp[1];

            Component[0] = new IndicatorComp();
            Component[0].CompName = "PSAR value (WTF)";
			if (IndParam.ListParam[0].Text == "Draw only, no entry or exit")
			{
				Component[0].DataType = IndComponentType.NotDefined;
			}
            else if (slotType == SlotTypes.Close) 
			{
                Component[0].DataType = IndComponentType.ClosePrice;
			}
            else if (slotType == SlotTypes.OpenFilter)
            {
				Component[0].DataType = IndComponentType.IndicatorValue;
				Component[0].PosPriceDependence = PositionPriceDependence.BuyHigherSellLower;
            }
                
            Component[0].ChartType  = IndChartType.Dot;
            Component[0].ChartColor =  Color.FromName(IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index]);
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adPSAR;


/// start WTF modfication 3 beta 2	
		// this wtf block not needed since not separated chart, does not have binary allow/disallow component value
		// including comment so numbering remains in order
/// end WTF modfication 3 beta 2	

            return;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            EntryFilterLongDescription  = "the price is higher than the " + ToString();
            EntryFilterShortDescription = "the price is lower than the " + ToString();
            ExitPointLongDescription    = "at the " + ToString() + ". It determines the position direction also";
            ExitPointShortDescription   = "at the " + ToString() + ". It determines the position direction also";

            return;
        }

        /// <summary>
        /// Indicator to string
        /// </summary>
        public override string ToString()
        {
            string sString = IndicatorName + " (" +
                IndParam.NumParam[0].ValueToString + ", " + // Starting AF
                IndParam.NumParam[1].ValueToString + ", " + // Increment
                IndParam.NumParam[2].ValueToString + "," +
				"WTF=" + IndParam.ListParam[1].Text + ")";   // Max AF

            return sString;
        }


		/// <summary>
        /// Convert current time frame to higher time frame
        /// </summary>
/// start WTF modfication 4 beta 2
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
/// end WTF modfication 4 beta 2
				
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