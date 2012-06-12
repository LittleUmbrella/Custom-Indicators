// Moving Averages Crossover Indicator -- (Modified to WTF v4)
// Last changed on 8/23/2011
// Part of Forex Strategy Builder v2.8.3.7+
// Website http://forexsb.com/
// Copyright (c) 2006 - 2009 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;
using System.Drawing;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Moving Averages Crossover Indicator
    /// </summary>
    public class Moving_Averages_Crossover_WTF : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
		/// v2 -- Draw Only for OpenFilters only, fix bug of disallowing all exits when CloseFilter
		/// v 3 -- no changes
		/// v 4 -- swap DataBars and hBars for significant performance improvement
		///     -- replace comment "modification version 4" with "modification version 4"		
		///     -- comment out returns for Indicator Logic Functions	
        /// </summary>
        public Moving_Averages_Crossover_WTF(SlotTypes slotType)
        {
            // General properties
            IndicatorName = "Moving Averages Crossover (WTF v4)";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
			CustomIndicator = true;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
			// WTF v2 fix bug "Draw Only" -- only available on OpenFilter Slots, on CloseFilter will disallow any exits
			if (slotType == SlotTypes.OpenFilter)
			{ 
				IndParam.ListParam[0].ItemList = new string[]
				{
					"The Fast MA crosses the Slow MA upward",
					"The Fast MA crosses the Slow MA downward",
					"The Fast MA is higher than the Slow MA",
					"The Fast MA is lower than the Slow MA",
					"Draw only, no entry or exit signals"
				};
			}
			else {
				IndParam.ListParam[0].ItemList = new string[]
				{
					"The Fast MA crosses the Slow MA upward",
					"The Fast MA crosses the Slow MA downward",
					"The Fast MA is higher than the Slow MA",
					"The Fast MA is lower than the Slow MA"
				};
			}
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption  = "Base price";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[1].Index    = (int)BasePrice.Close;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "The price both Moving Averages are based on.";

			IndParam.ListParam[2].Caption = "Wider Time Frame Reference";
			IndParam.ListParam[2].ItemList = new string[] { "Automatic", "5 Minutes", "15 Minutes", "30 Minutes", "1 Hour", "4 Hours", "1 Day", "1 Week"};
            if (Period == DataPeriods.min1) 		IndParam.ListParam[2].Index    = 1;
            else if (Period == DataPeriods.min5) 	IndParam.ListParam[2].Index    = 2;
            else if (Period == DataPeriods.min15) 	IndParam.ListParam[2].Index    = 3;
            else if (Period == DataPeriods.min30)	IndParam.ListParam[2].Index    = 4;
            else if (Period == DataPeriods.hour1)	IndParam.ListParam[2].Index    = 5;
            else if (Period == DataPeriods.hour4)	IndParam.ListParam[2].Index    = 6;
            else if (Period == DataPeriods.day)		IndParam.ListParam[2].Index    = 7;
            IndParam.ListParam[2].Text     = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "Choose wider time frame as compared to current chart period for this indicator.";	
			
            IndParam.ListParam[3].Caption  = "Fast MA method";
            IndParam.ListParam[3].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[3].Index    = (int)MAMethod.Simple;
            IndParam.ListParam[3].Text     = IndParam.ListParam[3].ItemList[IndParam.ListParam[3].Index];
            IndParam.ListParam[3].Enabled  = true;
            IndParam.ListParam[3].ToolTip  = "The method used for smoothing the Fast Moving Averages.";

            IndParam.ListParam[4].Caption  = "Slow MA method";
            IndParam.ListParam[4].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[4].Index    = (int)MAMethod.Simple;
            IndParam.ListParam[4].Text     = IndParam.ListParam[4].ItemList[IndParam.ListParam[4].Index];
            IndParam.ListParam[4].Enabled  = true;
            IndParam.ListParam[4].ToolTip  = "The method used for smoothing the slow Moving Averages.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption   = "Fast MA period";
            IndParam.NumParam[0].Value     = 13;
            IndParam.NumParam[0].Min       = 1;
            IndParam.NumParam[0].Max       = 200;
            IndParam.NumParam[0].Enabled   = true;
            IndParam.NumParam[0].ToolTip   = "The period of Fast MA.";

            IndParam.NumParam[1].Caption   = "Slow MA period";
            IndParam.NumParam[1].Value     = 21;
            IndParam.NumParam[1].Min       = 1;
            IndParam.NumParam[1].Max       = 200;
            IndParam.NumParam[1].Enabled   = true;
            IndParam.NumParam[1].ToolTip   = "The period of Slow MA.";

            IndParam.NumParam[2].Caption   = "Fast MA shift";
            IndParam.NumParam[2].Value     = 0;
            IndParam.NumParam[2].Min       = 0;
            IndParam.NumParam[2].Max       = 100;
            IndParam.NumParam[2].Point     = 0;
            IndParam.NumParam[2].Enabled   = true;
            IndParam.NumParam[2].ToolTip   = "The shifting value of Fast MA.";

            IndParam.NumParam[3].Caption   = "Slow MA shift";
            IndParam.NumParam[3].Value     = 0;
            IndParam.NumParam[3].Min       = 0;
            IndParam.NumParam[3].Max       = 100;
            IndParam.NumParam[3].Point     = 0;
            IndParam.NumParam[3].Enabled   = true;
            IndParam.NumParam[3].ToolTip   = "The shifting value of Slow MA.";

            IndParam.NumParam[4].Caption   = "Fast MA color";
            IndParam.NumParam[4].Value     = 0;
            IndParam.NumParam[4].Min       = 0;
            IndParam.NumParam[4].Max       = 5;
            IndParam.NumParam[4].Point     = 0;
            IndParam.NumParam[4].Enabled   = true;
            IndParam.NumParam[4].ToolTip   = "0 = Blue\n1 = Black\n2 = Red\n3 = Green\n4 = Yellow\n5 = Orange";

            IndParam.NumParam[5].Caption   = "Slow MA color";
            IndParam.NumParam[5].Value     = 1;
            IndParam.NumParam[5].Min       = 0;
            IndParam.NumParam[5].Max       = 5;
            IndParam.NumParam[5].Point     = 0;
            IndParam.NumParam[5].Enabled   = true;
            IndParam.NumParam[5].ToolTip   = "0 = Blue\n1 = Black\n2 = Red\n3 = Green\n4 = Yellow\n5 = Orange";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Checked = PrepareUsePrevBarValueCheckBox(slotType);
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";

            // The CheckBox parameters
/// start WTF modfication 1 version 4
			IndParam.CheckParam[0].Caption = "Force previous WTF value";
            IndParam.CheckParam[0].Checked = true;
            IndParam.CheckParam[0].Enabled = false;
            IndParam.CheckParam[0].ToolTip = "WTF are set to always use their previous value.";
/// end WTF modfication 1version 4
			
            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            // Reading the parameters
            BasePrice basePrice    = (BasePrice)IndParam.ListParam[1].Index;
            MAMethod  fastMAMethod = (MAMethod )IndParam.ListParam[3].Index;
            MAMethod  slowMAMethod = (MAMethod )IndParam.ListParam[4].Index;
            int       iNFastMA  = (int)IndParam.NumParam[0].Value;
            int       iNSlowMA  = (int)IndParam.NumParam[1].Value;
            int       iSFastMA  = (int)IndParam.NumParam[2].Value;
            int       iSSlowMA  = (int)IndParam.NumParam[3].Value;
            int       iPrvs     = IndParam.CheckParam[0].Checked ? 1 : 0;
		
			string[] saColors = new string[] {"Blue", "Black", "Red", "Green", "Yellow", "Orange"};
			string sFastColor = saColors[(int)IndParam.NumParam[4].Value];
			string sSlowColor = saColors[(int)IndParam.NumParam[5].Value];

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
		
		
			switch (IndParam.ListParam[2].Index)
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
			int err2 = HigherBasePrice(basePrice, hBars, hfHigh, hfLow, hfOpen, hfClose, out hfPrice);	
			if (err1==1) return;	
			//-----------------------------------------------------------------------
			
			// Calculation
			
            int     iFirstBar = (int)Math.Max(iNFastMA + iSFastMA, iNSlowMA + iSSlowMA) + 2;
            double[] adMAFast = MovingAverage(iNFastMA, iSFastMA, fastMAMethod, hfPrice);
            double[] adMASlow = MovingAverage(iNSlowMA, iSSlowMA, slowMAMethod, hfPrice);
            double[] adMAOscillator = new double[Bars];

            for (int iBar = iFirstBar; iBar < Bars; iBar++)
                adMAOscillator[iBar] = adMAFast[iBar] - adMASlow[iBar];
				
			// Convert to Current Time Frame ----------------------------------------------
/// start WTF modfication 2 version 4
			// do in 3 blocks for adMAFast, adMASlow, to draw on chart, and adMAOscillator for signals
			// copy of wider time frame array of values
			double[] hadMAFast = new double[Bars];
			adMAFast.CopyTo (hadMAFast, 0);
			int err3 = CurrentTimeFrame(hIndex, hBars, ref adMAFast);		
			// if any error, return out of calculation and indicator fails silently
			if (err3 == 1)
			{
				return;
			}
			// copy of wider time frame array of values
			double[] hadMASlow = new double[Bars];
			adMASlow.CopyTo (hadMASlow, 0);
			err3 = CurrentTimeFrame(hIndex, hBars, ref adMASlow);		
			// if any error, return out of calculation and indicator fails silently
			if (err3 == 1)
			{
				return;
			}			
			// copy of wider time frame array of values
			double[] hadMAOscillator = new double[Bars];
			adMAOscillator.CopyTo (hadMAOscillator, 0);
			err3 = CurrentTimeFrame(hIndex, hBars, ref adMAOscillator);		
			// if any error, return out of calculation and indicator fails silently
			if (err3 == 1)
			{
				return;
			}
/// end WTF modfication 2 version 4
			//-----------------------------------------------------------------------------	
				

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp();
            Component[0].CompName   = "Fast Moving Average";
            Component[0].ChartColor = Color.FromName(sFastColor);
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adMAFast;

            Component[1] = new IndicatorComp();
            Component[1].CompName   = "Slow Moving Average";
            Component[1].ChartColor = Color.FromName(sSlowColor);
            Component[1].DataType   = IndComponentType.IndicatorValue;
            Component[1].ChartType  = IndChartType.Line;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = adMASlow;

            Component[2] = new IndicatorComp();
            Component[2].ChartType = IndChartType.NoChart;
            Component[2].FirstBar  = iFirstBar;
            Component[2].Value     = new double[Bars];

            Component[3] = new IndicatorComp();
            Component[3].ChartType = IndChartType.NoChart;
            Component[3].FirstBar  = iFirstBar;
            Component[3].Value     = new double[Bars];

            // Sets the Component's type
            if (slotType == SlotTypes.OpenFilter)
            {
                Component[2].DataType = IndComponentType.AllowOpenLong;
                Component[2].CompName = "Is long entry allowed";
                Component[3].DataType = IndComponentType.AllowOpenShort;
                Component[3].CompName = "Is short entry allowed";
            }
            else if (slotType == SlotTypes.CloseFilter)
            {
                Component[2].DataType = IndComponentType.ForceCloseLong;
                Component[2].CompName = "Close out long position";
                Component[3].DataType = IndComponentType.ForceCloseShort;
                Component[3].CompName = "Close out short position";
            }
			
            // Calculation of the logic
            IndicatorLogic indLogic = IndicatorLogic.It_does_not_act_as_a_filter;

            switch (IndParam.ListParam[0].Text)
            {
                case "The Fast MA crosses the Slow MA upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    break;
                case "The Fast MA crosses the Slow MA downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    break;
                case "The Fast MA is higher than the Slow MA":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    break;
                case "The Fast MA is lower than the Slow MA":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    break;
				case "Draw only, no entry or exit signals":
					Component[2].CompName = "Visual Only";
					Component[2].DataType = IndComponentType.NotDefined;
					Component[3].CompName = "Visual Only";
					Component[3].DataType = IndComponentType.NotDefined;
					break;
                default:
                    break;
            }

/// start WTF modfication 3 version 4		

			// back up Bars value, reset to hBars, for performance improvement in indicator logic function
			int mtfBars = Data.Bars;
			Data.Bars = hBars;

			// replace very small values with 0 for performance improvement; don't know why but works
			for (int ctr = 0; ctr < hadMAOscillator.Length; ctr++)
			{
				hadMAOscillator[ctr] = (hadMAOscillator[ctr] < .000000001 && hadMAOscillator[ctr] > -.000000001) ? 0 : hadMAOscillator[ctr];
			}
			
            OscillatorLogic(iFirstBar, iPrvs, hadMAOscillator, 0, 0, ref Component[2], ref Component[3], indLogic);

			// resest Bars to real value
			Data.Bars = mtfBars;

			// expand component array from wtf to current time frame
			double[] wtfCompValue = Component[2].Value;
			int err4 = CurrentTimeFrame(hIndex, hBars, ref wtfCompValue);
			if (err4 == 1) { return; }
            Component[2].Value = wtfCompValue;
			wtfCompValue = Component[3].Value;
			int err5 = CurrentTimeFrame(hIndex, hBars, ref wtfCompValue);
			if (err5 == 1) { return; }
            Component[3].Value = wtfCompValue;

/// end WTF modfication 3 version 4

            return;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            EntryFilterLongDescription  = ToString() + "; the Fast MA ";
            EntryFilterShortDescription = ToString() + "; the Fast MA ";
            ExitFilterLongDescription   = ToString() + "; the Fast MA ";
            ExitFilterShortDescription  = ToString() + "; the Fast MA ";

            switch (IndParam.ListParam[0].Text)
            {
                case "The Fast MA crosses the Slow MA upward":
                    EntryFilterLongDescription  += "crosses the Slow MA upward";
                    EntryFilterShortDescription += "crosses the Slow MA downward";
                    ExitFilterLongDescription   += "crosses the Slow MA upward";
                    ExitFilterShortDescription  += "crosses the Slow MA downward";
                    break;

                case "The Fast MA crosses the Slow MA downward":
                    EntryFilterLongDescription  += "crosses the Slow MA downward";
                    EntryFilterShortDescription += "crosses the Slow MA upward";
                    ExitFilterLongDescription   += "crosses the Slow MA downward";
                    ExitFilterShortDescription  += "crosses the Slow MA upward";
                    break;

                case "The Fast MA is higher than the Slow MA":
                    EntryFilterLongDescription  += "is higher than the Slow MA";
                    EntryFilterShortDescription += "is lower than the Slow MA";
                    ExitFilterLongDescription   += "is higher than the Slow MA";
                    ExitFilterShortDescription  += "is lower than the Slow MA";
                    break;

                case "The Fast MA is lower than the Slow MA":
                    EntryFilterLongDescription  += "is lower than the Slow MA";
                    EntryFilterShortDescription += "is higher than the Slow MA";
                    ExitFilterLongDescription   += "is lower than the Slow MA";
                    ExitFilterShortDescription  += "is higher than the Slow MA";
                    break;

                default:
                    break;
            }

            return;
        }

        /// <summary>
        /// Indicator to string
        /// </summary>
        public override string ToString()
        {
            string sString = IndicatorName +
                (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                IndParam.ListParam[1].Text         + ", " + // Price
                IndParam.ListParam[3].Text         + ", " + // Fast MA Method
                IndParam.ListParam[4].Text         + ", " + // Slow MA Method
                IndParam.NumParam[0].ValueToString + ", " + // Fast MA period
                IndParam.NumParam[1].ValueToString + ", " + // Slow MA period
                IndParam.NumParam[2].ValueToString + ", " + // Fast MA shift
                IndParam.NumParam[3].ValueToString + "," +   // Slow MA shift
				"WTF=" + IndParam.ListParam[2].Text + ")";  // WTF period

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

