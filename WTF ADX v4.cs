// ADX Indicator -- Wider Time Frame v4
// Last changed on 12/1/2011
// Copyright (c) 2006 - 2010 Miroslav Popov - All rights reserved.
// Part of Forex Strategy Builder & Forex Strategy Trader
// Website http://forexsb.com
// This code or any part of it cannot be used in other applications without a permission.
 
using System;
using System.Drawing;
 
namespace Forex_Strategy_Builder
{
    /// <summary>
    /// ADX Indicator
    /// </summary>
    public class ADX_WTF : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
		/// v2 - latest version
		/// v 3 -- no changes
		/// v 4 -- swap DataBars and hBars for significant performance improvement
		///     -- replace comment "modification version 4" with "modification version 4"
		///     -- comment out returns for Indicator Logic Functions		
        /// </summary>
        public ADX_WTF(SlotTypes slotType)
        {
            // General properties
            IndicatorName  = "ADX (WTF v4)";
            PossibleSlots  = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
            SeparatedChartMinValue = 0;
			CustomIndicator = true;
 
            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType = slotType;
 
            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
			// WTF v2 fix bug "Draw Only" -- only available on OpenFilter Slots, on CloseFilter will disallow any exits
			if (slotType == SlotTypes.OpenFilter)
			{ 
				IndParam.ListParam[0].ItemList = new string[]
				{
					"The ADX rises",
					"The ADX falls",
					"The ADX is higher than the Level line",
					"The ADX is lower than the Level line",
					"The ADX crosses the Level line upward",
					"The ADX crosses the Level line downward",
					"The ADX changes its direction upward",
					"The ADX changes its direction downward",
					"Draw Only, no entry or exit"
				};
			}
			else {
				IndParam.ListParam[0].ItemList = new string[]
				{
					"The ADX rises",
					"The ADX falls",
					"The ADX is higher than the Level line",
					"The ADX is lower than the Level line",
					"The ADX crosses the Level line upward",
					"The ADX crosses the Level line downward",
					"The ADX changes its direction upward",
					"The ADX changes its direction downward"
				};
			}
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";
 
            IndParam.ListParam[1].Caption  = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[1].Index    = (int)MAMethod.Exponential;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "The Moving Average method used for smoothing the ADX value.";
 
            IndParam.ListParam[2].Caption  = "Base price";
            IndParam.ListParam[2].ItemList = new string[] { "Bar range" };
            IndParam.ListParam[2].Index    = 0;
            IndParam.ListParam[2].Text     = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "ADX uses current and previous bar ranges.";
 

 			IndParam.ListParam[4].Caption = "Wider Time Frame Reference";
			IndParam.ListParam[4].ItemList = new string[] { "Automatic", "5 Minutes", "15 Minutes", "30 Minutes", "1 Hour", "4 Hours", "1 Day", "1 Week"};
            if (Period == DataPeriods.min1) 		IndParam.ListParam[4].Index    = 1;
            else if (Period == DataPeriods.min5) 	IndParam.ListParam[4].Index    = 2;
            else if (Period == DataPeriods.min15) 	IndParam.ListParam[4].Index    = 3;
            else if (Period == DataPeriods.min30)	IndParam.ListParam[4].Index    = 4;
            else if (Period == DataPeriods.hour1)	IndParam.ListParam[4].Index    = 5;
            else if (Period == DataPeriods.hour4)	IndParam.ListParam[4].Index    = 6;
            else if (Period == DataPeriods.day)		IndParam.ListParam[4].Index    = 7;
            IndParam.ListParam[4].Text     = IndParam.ListParam[4].ItemList[IndParam.ListParam[4].Index];
            IndParam.ListParam[4].Enabled  = true;
            IndParam.ListParam[4].ToolTip  = "Choose wider time frame as compared to current chart period for this indicator.";		


            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period";
            IndParam.NumParam[0].Value   = 14;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of ADX.";
 
            IndParam.NumParam[1].Caption = "Level";
            IndParam.NumParam[1].Value   = 0;
            IndParam.NumParam[1].Min     = 0;
            IndParam.NumParam[1].Max     = 100;
            IndParam.NumParam[1].Point   = 0;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "A critical level (for the appropriate logic).";
 
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
            MAMethod maMethod = (MAMethod)IndParam.ListParam[1].Index;
            int period = (int)IndParam.NumParam[0].Value;
            double level = IndParam.NumParam[1].Value;
            int iPrvs = IndParam.CheckParam[0].Checked ? 1 : 0;
 
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
			
			switch (IndParam.ListParam[4].Index)
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
		//	int err2 = HigherBasePrice(basePrice, hBars, hfHigh, hfLow, hfOpen, hfClose, out hfPrice);	
			if (err1 ==1) return;


            // Calculation
 
            double[] DIPos = new double[Bars];
            double[] DINeg = new double[Bars];

			int iFirstBar = period + 2;
 
            for (int bar = iFirstBar; bar < Bars; bar++)
            {
                double trueRange = Math.Max(hfHigh[bar], hfClose[bar - 1]) - Math.Min(hfLow[bar], hfClose[bar - 1]);
 
                if (trueRange < Point)
                    trueRange = Point;
 
                double deltaHigh = hfHigh[bar] - hfHigh[bar - 1];
                double deltaLow  = hfLow[bar - 1] - hfLow[bar];
 
                if (deltaHigh > 0 && deltaHigh > deltaLow)
                    DIPos[bar] = 100 * deltaHigh / trueRange;
                else
                    DIPos[bar] = 0;
 
                if (deltaLow > 0 && deltaLow > deltaHigh)
                    DINeg[bar] = 100 * deltaLow / trueRange;
                else
                    DINeg[bar] = 0;
            }
			
 
            double[] ADIPos = MovingAverage(period, 0, maMethod, DIPos);
            double[] ADINeg = MovingAverage(period, 0, maMethod, DINeg);
 
            double[] DX = new double[Bars];

            for (int bar = 0; bar < Bars; bar++)
            {
                if (ADIPos[bar] + ADINeg[bar] == 0)
                    DX[bar] = 0;
                else
                    DX[bar] = 100 * Math.Abs((ADIPos[bar] - ADINeg[bar]) / (ADIPos[bar] + ADINeg[bar]));
            }

            double[] ADX = MovingAverage(period, 0, maMethod, DX);

			// Convert to Current Time Frame ----------------------------------------------
/// start WTF modfication 2 version 4
			// copy of wider time frame array of values
			double[] hADX = new double[Bars];
			ADX.CopyTo (hADX, 0);

			int err3 = CurrentTimeFrame(hIndex, hBars, ref ADX);	
			int err4 = CurrentTimeFrame(hIndex, hBars, ref ADIPos);
			int err5 = CurrentTimeFrame(hIndex, hBars, ref ADINeg);
			
			// if any error, return out of calculation and indicator fails silently
			if (err3 == 1 || err4 == 1 || err5 == 1)
			{
				return;
			}
/// end WTF modfication 2 version 4

			//-----------------------------------------------------------------------------	


            // Saving the components
            Component = new IndicatorComp[5];
 
            Component[0] = new IndicatorComp();
            Component[0].CompName   = "ADX";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor = Color.Blue;
            Component[0].FirstBar   =iFirstBar;
            Component[0].Value      = ADX;
 
            Component[1] = new IndicatorComp();
            Component[1].CompName   = "ADI+";
            Component[1].DataType   = IndComponentType.IndicatorValue;
            Component[1].ChartType  = IndChartType.Line;
            Component[1].ChartColor = Color.Green;
            Component[1].FirstBar   =iFirstBar;
            Component[1].Value      = ADIPos;
 
            Component[2] = new IndicatorComp();
            Component[2].CompName   = "ADI-";
            Component[2].DataType   = IndComponentType.IndicatorValue;
            Component[2].ChartType  = IndChartType.Line;
            Component[2].ChartColor = Color.Red;
            Component[2].FirstBar   =iFirstBar;
            Component[2].Value      = ADINeg;
 
            Component[3] = new IndicatorComp();
            Component[3].ChartType  = IndChartType.NoChart;
            Component[3].FirstBar   =iFirstBar;
            Component[3].Value      = new double[Bars];
 
            Component[4] = new IndicatorComp();
            Component[4].ChartType  = IndChartType.NoChart;
            Component[4].FirstBar   =iFirstBar;
            Component[4].Value      = new double[Bars];
 
            // Sets the Component's type
            if (slotType == SlotTypes.OpenFilter)
            {
                Component[3].DataType = IndComponentType.AllowOpenLong;
                Component[3].CompName = "Is long entry allowed";
                Component[4].DataType = IndComponentType.AllowOpenShort;
                Component[4].CompName = "Is short entry allowed";
            }
            else if (slotType == SlotTypes.CloseFilter)
            {
                Component[3].DataType = IndComponentType.ForceCloseLong;
                Component[3].CompName = "Close out long position";
                Component[4].DataType = IndComponentType.ForceCloseShort;
                Component[4].CompName = "Close out short position";
            }
 
            // Calculation of the logic
            IndicatorLogic logicRule;
 
            switch (IndParam.ListParam[0].Text)
            {
                case "The ADX rises":
                    logicRule = IndicatorLogic.The_indicator_rises;
                    break;
 
                case "The ADX falls":
                    logicRule = IndicatorLogic.The_indicator_falls;
                    break;
 
                case "The ADX is higher than the Level line":
                    logicRule = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new double[1] { level };
                    break;
 
                case "The ADX is lower than the Level line":
                    logicRule = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new double[1] { level };
                    break;
 
                case "The ADX crosses the Level line upward":
                    logicRule = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new double[1] { level };
                    break;
 
                case "The ADX crosses the Level line downward":
                    logicRule = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new double[1] { level };
                    break;
 
                case "The ADX changes its direction upward":
                    logicRule = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    break;
 
                case "The ADX changes its direction downward":
                    logicRule = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    break;
 
				case "Draw Only, no entry or exit":
					logicRule = IndicatorLogic.It_does_not_act_as_a_filter;
					Component[3].DataType = IndComponentType.NotDefined;
					Component[3].CompName = "Visual Only";
					Component[4].DataType = IndComponentType.NotDefined;
					Component[4].CompName = "Visual Only";
					break;

                default:
                    logicRule = IndicatorLogic.It_does_not_act_as_a_filter;
                    break;
            }
 
            // ADX rises equal signals in both directions!
/// start WTF modfication 3 version 4			
			
			// back up Bars value, reset to hBars, for performance improvement in indicator logic function
			int mtfBars = Data.Bars;
			Data.Bars = hBars;
			
			// replace very small values with 0 for performance improvement; don't know why but works
			for (int ctr = 0; ctr < hADX.Length; ctr++)
			{
				hADX[ctr] = (hADX[ctr] < .000000001 && hADX[ctr] > -.000000001) ? 0 : hADX[ctr];
			}
			NoDirectionOscillatorLogic(iFirstBar, iPrvs, hADX, level, ref Component[3], logicRule);

			// resest Bars to real value
			Data.Bars = mtfBars;
			
			// expand component array from wtf to current time frame
			double[] wtfCompValue = Component[3].Value;
			int err6 = CurrentTimeFrame(hIndex, hBars, ref wtfCompValue);
			if (err6 == 1) { return; }
            Component[4].Value = Component[3].Value = wtfCompValue;

/// end WTF modfication 3 version 4


            return;
        }
 
        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            string sLevelLong  = IndParam.NumParam[1].ValueToString;
            string sLevelShort = sLevelLong;
 
            EntryFilterLongDescription  = "the " + ToString() + " ";
            EntryFilterShortDescription = "the " + ToString() + " ";
            ExitFilterLongDescription   = "the " + ToString() + " ";
            ExitFilterShortDescription  = "the " + ToString() + " ";
 
            switch (IndParam.ListParam[0].Text)
            {
                case "The ADX rises":
                    EntryFilterLongDescription  += "rises";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription   += "rises";
                    ExitFilterShortDescription  += "rises";
                    break;
 
                case "The ADX falls":
                    EntryFilterLongDescription  += "falls";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription   += "falls";
                    ExitFilterShortDescription  += "falls";
                    break;
 
                case "The ADX is higher than the Level line":
                    EntryFilterLongDescription  += "is higher than the Level " + sLevelLong;
                    EntryFilterShortDescription += "is higher than the Level " + sLevelShort;
                    ExitFilterLongDescription   += "is higher than the Level " + sLevelLong;
                    ExitFilterShortDescription  += "is higher than the Level " + sLevelShort;
                    break;
 
                case "The ADX is lower than the Level line":
                    EntryFilterLongDescription  += "is lower than the Level " + sLevelLong;
                    EntryFilterShortDescription += "is lower than the Level " + sLevelShort;
                    ExitFilterLongDescription   += "is lower than the Level " + sLevelLong;
                    ExitFilterShortDescription  += "is lower than the Level " + sLevelShort;
                    break;
 
                case "The ADX crosses the Level line upward":
                    EntryFilterLongDescription  += "crosses the Level " + sLevelLong  + " upward";
                    EntryFilterShortDescription += "crosses the Level " + sLevelShort + " upward";
                    ExitFilterLongDescription   += "crosses the Level " + sLevelLong  + " upward";
                    ExitFilterShortDescription  += "crosses the Level " + sLevelShort + " upward";
                    break;
 
                case "The ADX crosses the Level line downward":
                    EntryFilterLongDescription  += "crosses the Level " + sLevelLong  + " downward";
                    EntryFilterShortDescription += "crosses the Level " + sLevelShort + " downward";
                    ExitFilterLongDescription   += "crosses the Level " + sLevelLong  + " downward";
                    ExitFilterShortDescription  += "crosses the Level " + sLevelShort + " downward";
                    break;
 
                case "The ADX changes its direction upward":
                    EntryFilterLongDescription  += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription   += "changes its direction upward";
                    ExitFilterShortDescription  += "changes its direction upward";
                    break;
 
                case "The ADX changes its direction downward":
                    EntryFilterLongDescription  += "changes its direction downward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription   += "changes its direction downward";
                    ExitFilterShortDescription  += "changes its direction downward";
                    break;

				case "Draw Only, no entry or exit":
					EntryFilterLongDescription  += "is drawn only, no signals given";
                    EntryFilterShortDescription += "is drawn only, no signals given";
                    ExitFilterLongDescription   += "is drawn only, no signals given";
                    ExitFilterShortDescription  += "is drawn only, no signals given";
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
            string indicator = IndicatorName +
                (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                IndParam.ListParam[1].Text         + ", " + // Smoothing method
                IndParam.ListParam[2].Text         + ", " + // Base price
                IndParam.NumParam[0].ValueToString + ", " +  // ADX Period
				IndParam.ListParam[4].Text + ")";   // WTF period
 
            return indicator;
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

