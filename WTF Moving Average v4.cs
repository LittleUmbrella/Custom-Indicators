// Moving Average Indicator (WTF v4)
// Last changed on 9/10/2011
// Part of Forex Strategy Builder v2.8.3.7+
// Website http://forexsb.com/
// This code or any part of it cannot be used in other applications without a permission.
// Copyright (c) 2006 - 2009 Miroslav Popov - All rights reserved.

using System;
using System.Drawing;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Moving Average Indicator 
	/// 2 bugs -- if error returns early, no Component sent to Backtester; iPrvs could be greater than 1, causing error in Backtester.TransferFromPreviousBar
	/// beta 2 -- include fixes so errors return blank component to solve bug when used with Generator 
	/// set Automatic to be 1 hour, solves another bug when used with Generator 
	/// set Component.UsePreviousBar directly from IndParam, not use iPrvs
	/// v2 -- Draw Only for OpenFilters only, fix bug of disallowing all exits when CloseFilter
	/// v3 -- fix bug of Postion Price Dependence allowing use of current bar instead of previous bar for signal (fix by manual shift of MA values)
	/// v4 -- v4 fix not needed for Moving Average
    /// </summary>
    public class Moving_Average_WTF : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Moving_Average_WTF(SlotTypes slotType)
        {
            // General properties
            IndicatorName = "Moving Average (WTF v4)";
            PossibleSlots = SlotTypes.Open | SlotTypes.OpenFilter | SlotTypes.Close | SlotTypes.CloseFilter;
			CustomIndicator = true;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
			// WTF v2 fix bug "Draw Only" -- only available on OpenFilter Slots, on CloseFilter will disallow any exits
            if (slotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Enter the market at the Moving Average"
                };
            else if (slotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "The Moving Average rises",
                    "The Moving Average falls",
                    "The bar opens above the Moving Average",
                    "The bar opens below the Moving Average",
                    "The bar opens above the Moving Average after opening below it",
                    "The bar opens below the Moving Average after opening above it",
                    "The position opens above the Moving Average",
                    "The position opens below the Moving Average",
					"Draw only, no entry or exit"
                };
            else if (slotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Exit the market at the Moving Average"
                };
            else if (slotType == SlotTypes.CloseFilter)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "The Moving Average rises",
                    "The Moving Average falls",
                    "The bar closes below the Moving Average",
                    "The bar closes above the Moving Average"
                };
            else
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Not Defined"
                };
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of the Moving Average.";

            IndParam.ListParam[1].Caption  = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[1].Index    = (int)MAMethod.Simple;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "The smoothing method of Moving Average.";

            IndParam.ListParam[2].Caption  = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[2].Index    = (int)BasePrice.Close;
            IndParam.ListParam[2].Text     = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "The price the Moving Average is based on.";

            IndParam.ListParam[3].Caption  = "Line Color";
            IndParam.ListParam[3].ItemList = new string [] 
			{
				"Blue",
				"Red",
				"Green",
				"Orange"
			};
            IndParam.ListParam[3].Index    = 0;
            IndParam.ListParam[3].Text     = IndParam.ListParam[3].ItemList[IndParam.ListParam[3].Index];
            IndParam.ListParam[3].Enabled  = true;
            IndParam.ListParam[3].ToolTip  = "Color of line on chart for this MA WTF value.";


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
            IndParam.NumParam[0].Caption   = "Period";
            IndParam.NumParam[0].Value     = 14;
            IndParam.NumParam[0].Min       = 1;
            IndParam.NumParam[0].Max       = 200;
            IndParam.NumParam[0].Enabled   = true;
            IndParam.NumParam[0].ToolTip   = "The Moving Average period.";

            IndParam.NumParam[1].Caption   = "Shift";
            IndParam.NumParam[1].Value     = 0;
            IndParam.NumParam[1].Min       = 0;
            IndParam.NumParam[1].Max       = 200;
            IndParam.NumParam[1].Enabled   = true;
            IndParam.NumParam[1].ToolTip   = "How many bars to shift with.";

            // The CheckBox parameters
/// start WTF modfication 1 beta 1
			IndParam.CheckParam[0].Caption = "Force previous WTF value";
            IndParam.CheckParam[0].Checked = true;
            IndParam.CheckParam[0].Enabled = false;
            IndParam.CheckParam[0].ToolTip = "WTF are set to always use their previous value.";
/// end WTF modfication 1 beta 1
            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            // Reading the parameters
            MAMethod  maMethod = (MAMethod )IndParam.ListParam[1].Index;
            BasePrice basePrice    = (BasePrice)IndParam.ListParam[2].Index;
            int       iPeriod  = (int)IndParam.NumParam[0].Value;
            int       iShift   = (int)IndParam.NumParam[1].Value;
            int       iPrvs    = IndParam.CheckParam[0].Checked ? 1 : 0;

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
                case 0: htfPeriod = DataPeriods.hour1; break;
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

			//------------------------------------------------------------------------

            // TimeExecution
			
            if (basePrice == BasePrice.Open && iPeriod == 1 && iShift == 0)
                IndParam.ExecutionTime = ExecutionTime.AtBarOpening;

            // Calculation
			double[] adMA = MovingAverage(iPeriod, iShift, maMethod, hfPrice);

			// v3 -- manually shift so Position entry type logics use previous WTF value, fix bug of not using previous value
			if (IndParam.ListParam[0].Text == "The position opens above the Moving Average" ||
				IndParam.ListParam[0].Text == "The position opens below the Moving Average")
			{
				adMA = MovingAverage(iPeriod, iShift+1, maMethod, hfPrice);
			}
			else {
				adMA = MovingAverage(iPeriod, iShift, maMethod, hfPrice);
			}

            int iFirstBar = iPeriod + iShift + 1 + iPrvs;

			// Convert to Current Time Frame ----------------------------------------------
			iPrvs = iPrvs*iFrame;
			iFirstBar = iFirstBar*iFrame;
            
            int err3 = (iFirstBar > 0.25 * Bars) ? 1 : 0;
			
 			int err4 = CurrentTimeFrame(hIndex, hBars, ref adMA);
			// if any WTF conversion errors, return here to fail silently
			if (err1 == 1 || err2 == 1 || err3 == 1 || err4 == 1)
			{
				Component = new IndicatorComp[1];
				Component[0] = new IndicatorComp();
				Component[0].CompName   = "MA Value";
				Component[0].DataType   = IndComponentType.IndicatorValue;
				Component[0].ChartType  = IndChartType.Line;
				Component[0].FirstBar   = (int)Bars/2;
                Component[0].UsePreviousBar = 0;
				Component[0].Value      =  new double[Bars];
				return;
			}
			//-----------------------------------------------------------------------------	

            // Saving the components
            if (slotType == SlotTypes.Open || slotType == SlotTypes.Close)
            {
                Component = new IndicatorComp[2];

                Component[1] = new IndicatorComp();
				Component[1].FirstBar  = iFirstBar;
                Component[1].Value = new double[Bars];

				int hBar = 0;
                for (int iBar = 3*iFrame; iBar < Bars; iBar++)
                {   // Covers the cases when the price can pass through the MA without a signal
/// start WTF modfication 3 beta 1
					while (hIndex[hBar] <= iBar && hBar < hBars)
					{
						hBar++;
					}
					double dValue   = adMA[hIndex[hBar-2]];     // MA value from previous HTF bar
                    double dValue1  = adMA[hIndex[hBar-3]]; // MA value from HTF 2 bars previous
/// end WTF modfication 3 beta 1
					double dTempVal = dValue;
                    if ((dValue1 > High[iBar - 1] && dValue < Low[iBar]) || // It jumps below the current bar
                        (dValue1 < Low[iBar - 1]  && dValue > High[iBar])|| // It jumps above the current bar
                        (Close[iBar - 1] < dValue && dValue < Open[iBar])|| // Positive gap
                        (Close[iBar - 1] > dValue && dValue > Open[iBar]))  // Negative gap
                        dTempVal = Open[iBar];
                    Component[1].Value[iBar] = dTempVal;
                }
            }
            else
            {
                Component = new IndicatorComp[3];

                Component[1] = new IndicatorComp();
                Component[1].ChartType = IndChartType.NoChart;
                Component[1].FirstBar  = iFirstBar;
                Component[1].Value     = new double[Bars];

                Component[2] = new IndicatorComp();
                Component[2].ChartType = IndChartType.NoChart;
                Component[2].FirstBar  = iFirstBar;
                Component[2].Value     = new double[Bars];
            }

            Component[0] = new IndicatorComp();
            Component[0].CompName   = "MA Value";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor	= Color.FromName(IndParam.ListParam[3].ItemList[IndParam.ListParam[3].Index]);
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adMA;

            if (slotType == SlotTypes.Open)
            {
                Component[1].CompName = "Position opening price";
                Component[1].DataType = IndComponentType.OpenPrice;
            }
            else if (slotType == SlotTypes.OpenFilter)
            {
                Component[1].DataType = IndComponentType.AllowOpenLong;
                Component[1].CompName = "Is long entry allowed";
                Component[2].DataType = IndComponentType.AllowOpenShort;
                Component[2].CompName = "Is short entry allowed";
            }
            else if (slotType == SlotTypes.Close)
            {
                Component[1].CompName = "Position closing price";
                Component[1].DataType = IndComponentType.ClosePrice;
            }
            else if (slotType == SlotTypes.CloseFilter)
            {
                Component[1].DataType = IndComponentType.ForceCloseLong;
                Component[1].CompName = "Close out long position";
                Component[2].DataType = IndComponentType.ForceCloseShort;
                Component[2].CompName = "Close out short position";
            }

            if (slotType == SlotTypes.OpenFilter || slotType == SlotTypes.CloseFilter)
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "The Moving Average rises":
                        IndicatorRisesLogic(iFirstBar, iPrvs, adMA, ref Component[1], ref Component[2]);
                        break;

                    case "The Moving Average falls":
                        IndicatorFallsLogic(iFirstBar, iPrvs, adMA, ref Component[1], ref Component[2]);
                        break;

                    case "The bar opens above the Moving Average":
                        BarOpensAboveIndicatorLogic(iFirstBar, iPrvs, adMA, ref Component[1], ref Component[2]);
                        break;

                    case "The bar opens below the Moving Average":
                        BarOpensBelowIndicatorLogic(iFirstBar, iPrvs, adMA, ref Component[1], ref Component[2]);
                        break;

                    case "The bar opens above the Moving Average after opening below it":
                        BarOpensAboveIndicatorAfterOpeningBelowLogic(iFirstBar, iPrvs, adMA, ref Component[1], ref Component[2]);
                        break;

                    case "The bar opens below the Moving Average after opening above it":
                        BarOpensBelowIndicatorAfterOpeningAboveLogic(iFirstBar, iPrvs, adMA, ref Component[1], ref Component[2]);
                        break;

                    case "The position opens above the Moving Average":
                        Component[0].PosPriceDependence = PositionPriceDependence.BuyHigherSellLower;
                        Component[1].DataType           = IndComponentType.Other;
                        Component[1].ShowInDynInfo      = false;
                        Component[2].DataType           = IndComponentType.Other;
                        Component[2].ShowInDynInfo      = false;
                        break;

                    case "The position opens below the Moving Average":
                        Component[0].PosPriceDependence = PositionPriceDependence.BuyLowerSelHigher;
				        Component[1].DataType           = IndComponentType.Other;
                        Component[1].ShowInDynInfo      = false;
                        Component[2].DataType           = IndComponentType.Other;
                        Component[2].ShowInDynInfo      = false;
                        break;

                    case "The bar closes below the Moving Average":
                        BarClosesBelowIndicatorLogic(iFirstBar, iPrvs, adMA, ref Component[1], ref Component[2]);
                        break;

                    case "The bar closes above the Moving Average":
                        BarClosesAboveIndicatorLogic(iFirstBar, iPrvs, adMA, ref Component[1], ref Component[2]);
                        break;

					case "Draw only, no entry or exit":
						Component[1].CompName = "Visual Only";
						Component[1].DataType = IndComponentType.NotDefined;
						Component[2].CompName = "Visual Only";
						Component[2].DataType = IndComponentType.NotDefined;
						break;
                    
					default:
                        break;
                }
            }

            return;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            EntryPointLongDescription  = "at the " + ToString();
            EntryPointShortDescription = "at the " + ToString();
            ExitPointLongDescription   = "at the " + ToString();
            ExitPointShortDescription  = "at the " + ToString();

            switch (IndParam.ListParam[0].Text)
            {
                case "The Moving Average rises":
                    EntryFilterLongDescription  = "the " + ToString() + " rises";
                    EntryFilterShortDescription = "the " + ToString() + " falls";
                    ExitFilterLongDescription   = "the " + ToString() + " rises";
                    ExitFilterShortDescription  = "the " + ToString() + " falls";
                    break;

                case "The Moving Average falls":
                    EntryFilterLongDescription  = "the " + ToString() + " falls";
                    EntryFilterShortDescription = "the " + ToString() + " rises";
                    ExitFilterLongDescription   = "the " + ToString() + " falls";
                    ExitFilterShortDescription  = "the " + ToString() + " rises";
                    break;

                case "The bar opens above the Moving Average":
                    EntryFilterLongDescription  = "the bar opens above the " + ToString();
                    EntryFilterShortDescription = "the bar opens below the " + ToString();
                    break;

                case "The bar opens below the Moving Average":
                    EntryFilterLongDescription  = "the bar opens below the " + ToString();
                    EntryFilterShortDescription = "the bar opens above the " + ToString();
                    break;

                case "The position opens above the Moving Average":
                    EntryFilterLongDescription  = "the position opening price is higher than the " + ToString();
                    EntryFilterShortDescription = "the position opening price is lower than the "  + ToString();
                    break;

                case "The position opens below the Moving Average":
                    EntryFilterLongDescription  = "the position opening price is lower than the "  + ToString();
                    EntryFilterShortDescription = "the position opening price is higher than the " + ToString();
                    break;

                case "The bar opens above the Moving Average after opening below it":
                    EntryFilterLongDescription  = "the bar opens above the " + ToString() + " after opening below it";
                    EntryFilterShortDescription = "the bar opens below the " + ToString() + " after opening above it";
                    break;

                case "The bar opens below the Moving Average after opening above it":
                    EntryFilterLongDescription  = "the bar opens below the " + ToString() + " after opening above it";
                    EntryFilterShortDescription = "the bar opens above the " + ToString() + " after opening below it";
                    break;

                case "The bar closes above the Moving Average":
                    ExitFilterLongDescription  = "the bar closes above the " + ToString();
                    ExitFilterShortDescription = "the bar closes below the " + ToString();
                    break;

                case "The bar closes below the Moving Average":
                    ExitFilterLongDescription  = "the bar closes below the " + ToString();
                    ExitFilterShortDescription = "the bar closes above the " + ToString();
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
                IndParam.ListParam[1].Text         + ", " + // Method
                IndParam.ListParam[2].Text         + ", " + // Price
                IndParam.NumParam[0].ValueToString + ", " + // MA period
                IndParam.NumParam[1].ValueToString + ")";   // MA shift

            return sString;
        }
		
				/// <summary>
        /// Convert current time frame to higher time frame
        /// </summary>
/// start WTF modfication 2 beta 1
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
/// end WTF modfication 2 beta 1
				
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