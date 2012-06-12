// Stochastics Indicator -- WTF v4
// Last changed on 2/10/2012
// Part of Forex Strategy Builder v2.8.3.7+
// Website http://forexsb.com/
// Copyright (c) 2006 - 2009 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;
using System.Drawing;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Stochastics Indicator
	/// adapted for wider time frame (WTF)
		/// v2 - latest version
		/// v 3 -- no changes
		/// v 4 -- swap DataBars and hBars for significant performance improvement
		///     -- replace comment "modification version 4" with "modification version 4"		
		///     -- comment out returns for Indicator Logic Functions		
    /// </summary>
    public class Stochastics_WTF : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Stochastics_WTF (SlotTypes slotType)
        {
            // General properties
            IndicatorName  = "Stochastics (WTF v4)";
            PossibleSlots  = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
            SeparatedChartMinValue = 0;
            SeparatedChartMaxValue = 100;
			CustomIndicator = true;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "The Slow %D rises",
                "The Slow %D falls",
                "The Slow %D is higher than the Level line",
                "The Slow %D is lower than the Level line",
                "The Slow %D crosses the Level line upward",
                "The Slow %D crosses the Level line downward",
                "The Slow %D changes its direction upward",
                "The Slow %D changes its direction downward",
                "The %K is higher than the Slow %D",
                "The %K is lower than the Slow %D",
                "The %K crosses the Slow %D upward",
                "The %K crosses the Slow %D downward",
            };
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption  = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[1].Index    = (int)MAMethod.Simple;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "The Moving Average method used for smoothing.";

            IndParam.ListParam[4].Caption = "Wider Time Frame Reference";
			IndParam.ListParam[4].ItemList = new string[] { "1 Minute", "5 Minutes", "15 Minutes", "30 Minutes", "1 Hour", "4 Hours", "1 Day", "1 Week"};
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



            IndParam.NumParam[0].Caption = "%K period";
            IndParam.NumParam[0].Value   = 5;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The smoothing period of %K.";

            IndParam.NumParam[1].Caption = "Fast %D period";
            IndParam.NumParam[1].Value   = 3;
            IndParam.NumParam[1].Min     = 1;
            IndParam.NumParam[1].Max     = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The smoothing period of Fast %D.";

            IndParam.NumParam[2].Caption = "Slow %D period";
            IndParam.NumParam[2].Value   = 3;
            IndParam.NumParam[2].Min     = 1;
            IndParam.NumParam[2].Max     = 200;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The smoothing period of Slow %D.";

            IndParam.NumParam[3].Caption = "Level";
            IndParam.NumParam[3].Value   = 20;
            IndParam.NumParam[3].Min     = 0;
            IndParam.NumParam[3].Max     = 100;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "A critical level (for the appropriate logic).";

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
            int iK     = (int)IndParam.NumParam[0].Value;
            int iDFast = (int)IndParam.NumParam[1].Value;
            int iDSlow = (int)IndParam.NumParam[2].Value;
            int iLevel = (int)IndParam.NumParam[3].Value;
            int iPrvs  = IndParam.CheckParam[0].Checked ? 1 : 0;


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
			//int err2 = HigherBasePrice(basePrice, hBars, hfHigh, hfLow, hfOpen, hfClose, out hfPrice);	
			if (err1==1) return;
			//------------------------------------------------------------------------


            // Calculation
            int iFirstBar = iK + iDFast + iDSlow + 3;

            double[] adHighs = new double[Bars];
            double[] adLows  = new double[Bars];
            for (int iBar = iK; iBar < hBars; iBar++)
            {
                double dMin = double.MaxValue;
                double dMax = double.MinValue;
                for (int i = 0; i < iK; i++)
                {
                    if (hfHigh[iBar - i] > dMax) dMax = hfHigh[iBar - i];
                    if (hfLow[iBar  - i] < dMin) dMin = hfLow[iBar  - i];
                }
                adHighs[iBar] = dMax;
                adLows[iBar]  = dMin;
            }

            double[] adK = new double[Bars];
            for (int iBar = iK; iBar < hBars; iBar++)
            {
                if (adHighs[iBar] == adLows[iBar])
                    adK[iBar] = 50;
                else
                    adK[iBar] = 100 * (hfClose[iBar] - adLows[iBar]) / (adHighs[iBar] - adLows[iBar]);
            }

            double[] adDFast = new double[Bars];
            for (int iBar = iDFast; iBar < hBars; iBar++)
            {
                double dSumHigh = 0;
                double dSumLow  = 0;
                for (int i = 0; i < iDFast; i++)
                {
                    dSumLow  += hfClose[iBar - i]   - adLows[iBar - i];
                    dSumHigh += adHighs[iBar - i] - adLows[iBar - i];
                }
                if (dSumHigh == 0)
                    adDFast[iBar] = 100;
                else
                    adDFast[iBar] = 100 * dSumLow / dSumHigh;
            }

            double[] adDSlow = MovingAverage(iDSlow, 0, maMethod, adDFast);


            // Convert to Current Time Frame ----------------------------------------------

/// start WTF modfication 2 version 4
			// copy of wider time frame array of values
			double[] hadK = new double[Bars];
			double[] hadDFast = new double[Bars];
			double[] hadDSlow = new double[Bars];
			adK.CopyTo (hadK, 0);
			adDFast.CopyTo (hadDFast, 0);
			adDSlow.CopyTo (hadDSlow, 0);
			
			int err3 = CurrentTimeFrame(hIndex, hBars, ref adK);
			int err4 = CurrentTimeFrame(hIndex, hBars, ref adDFast);			
			int err5 = CurrentTimeFrame(hIndex, hBars, ref adDSlow);
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
            Component[0].CompName   = "%K";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor = Color.Brown;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adK;

            Component[1] = new IndicatorComp();
            Component[1].CompName   = "Fast %D";
            Component[1].DataType   = IndComponentType.IndicatorValue;
            Component[1].ChartType  = IndChartType.Line;
            Component[1].ChartColor = Color.Yellow;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = adDFast;

            Component[2] = new IndicatorComp();
            Component[2].CompName   = "Slow %D";
            Component[2].DataType   = IndComponentType.IndicatorValue;
            Component[2].ChartType  = IndChartType.Line;
            Component[2].ChartColor = Color.Blue;
            Component[2].FirstBar   = iFirstBar;
            Component[2].Value      = adDSlow;

            Component[3] = new IndicatorComp();
            Component[3].ChartType = IndChartType.NoChart;
            Component[3].FirstBar  = iFirstBar;
            Component[3].Value     = new double[Bars];

            Component[4] = new IndicatorComp();
            Component[4].ChartType = IndChartType.NoChart;
            Component[4].FirstBar  = iFirstBar;
            Component[4].Value     = new double[Bars];

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
            IndicatorLogic indLogic = IndicatorLogic.It_does_not_act_as_a_filter;

/// start WTF modfication 3 version 4		

			// back up Bars value, reset to hBars, for performance improvement in indicator logic function
			int mtfBars = Data.Bars;
			Data.Bars = hBars;

			// use wtf values here, then do expansion after this if clause to cover these IndicatorCrosses cases and the OscillatorLogic cases
			// replace very small values with 0 for performance improvement; don't know why but works
			for (int ctr = 0; ctr < hadDSlow.Length; ctr++)
			{
				hadK[ctr] = (hadK[ctr] < .000000001 && hadK[ctr] > -.000000001) ? 0 : hadK[ctr];
				hadDSlow[ctr] = (hadDSlow[ctr] < .000000001 && hadDSlow[ctr] > -.000000001) ? 0 : hadDSlow[ctr];
			}

            if (IndParam.ListParam[0].Text == "The %K crosses the Slow %D upward")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorCrossesAnotherIndicatorUpwardLogic(iFirstBar, iPrvs, hadK, hadDSlow, ref Component[3], ref Component[4]);
            }
            else if (IndParam.ListParam[0].Text == "The %K crosses the Slow %D downward")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorCrossesAnotherIndicatorDownwardLogic(iFirstBar, iPrvs, hadK, hadDSlow, ref Component[3], ref Component[4]);
            }
            else if (IndParam.ListParam[0].Text == "The %K is higher than the Slow %D")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorIsHigherThanAnotherIndicatorLogic(iFirstBar, iPrvs, hadK, hadDSlow, ref Component[3], ref Component[4]);
            }
            else if (IndParam.ListParam[0].Text == "The %K is lower than the Slow %D")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorIsLowerThanAnotherIndicatorLogic(iFirstBar, iPrvs, hadK, hadDSlow, ref Component[3], ref Component[4]);
            }	
            else
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "The Slow %D rises":
                        indLogic = IndicatorLogic.The_indicator_rises;
                        SpecialValues = new double[1] { 50 };
                        break;

                    case "The Slow %D falls":
                        indLogic = IndicatorLogic.The_indicator_falls;
                        SpecialValues = new double[1] { 50 };
                        break;

                    case "The Slow %D is higher than the Level line":
                        indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                        SpecialValues = new double[2] { iLevel, 100 - iLevel };
                        break;

                    case "The Slow %D is lower than the Level line":
                        indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                        SpecialValues = new double[2] { iLevel, 100 - iLevel };
                        break;

                    case "The Slow %D crosses the Level line upward":
                        indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                        SpecialValues = new double[2] { iLevel, 100 - iLevel };
                        break;

                    case "The Slow %D crosses the Level line downward":
                        indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                        SpecialValues = new double[2] { iLevel, 100 - iLevel };
                        break;

                    case "The Slow %D changes its direction upward":
                        indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                        SpecialValues = new double[1] { 50 };
                        break;

                    case "The Slow %D changes its direction downward":
                        indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                        SpecialValues = new double[1] { 50 };
                        break;

                    default:
                        break;
                }


				OscillatorLogic(iFirstBar, iPrvs, hadDSlow, iLevel, 100 - iLevel, ref Component[3], ref Component[4], indLogic);

			}

			// resest Bars to real value
			Data.Bars = mtfBars;

			// expand component array from wtf to current time frame
			double[] wtfCompValue = Component[3].Value;
			int err6 = CurrentTimeFrame(hIndex, hBars, ref wtfCompValue);
			if (err6 == 1) { return; }
			Component[3].Value = wtfCompValue;
			wtfCompValue = Component[4].Value;
			int err7 = CurrentTimeFrame(hIndex, hBars, ref wtfCompValue);
			if (err7 == 1) { return; }
			Component[4].Value = wtfCompValue;

/// end WTF modfication 3 version 4

            return;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            string sLevelLong  = IndParam.NumParam[3].ValueToString;
            string sLevelShort = IndParam.NumParam[3].AnotherValueToString(100 - IndParam.NumParam[3].Value);

            EntryFilterLongDescription  = ToString() + " - ";
            EntryFilterShortDescription = ToString() + " - ";
            ExitFilterLongDescription   = ToString() + " - ";
            ExitFilterShortDescription  = ToString() + " - ";

            switch (IndParam.ListParam[0].Text)
            {
                case "The Slow %D rises":
                    EntryFilterLongDescription  += "the Slow %D rises";
                    EntryFilterShortDescription += "the Slow %D falls";
                    ExitFilterLongDescription   += "the Slow %D rises";
                    ExitFilterShortDescription  += "the Slow %D falls";
                    break;

                case "The Slow %D falls":
                    EntryFilterLongDescription  += "the Slow %D falls";
                    EntryFilterShortDescription += "the Slow %D rises";
                    ExitFilterLongDescription   += "the Slow %D falls";
                    ExitFilterShortDescription  += "the Slow %D rises";
                    break;

                case "The Slow %D is higher than the Level line":
                    EntryFilterLongDescription  += "the Slow %D is higher than the Level " + sLevelLong;
                    EntryFilterShortDescription += "the Slow %D is lower than the Level "  + sLevelShort;
                    ExitFilterLongDescription   += "the Slow %D is higher than the Level " + sLevelLong;
                    ExitFilterShortDescription  += "the Slow %D is lower than the Level "  + sLevelShort;
                    break;

                case "The Slow %D is lower than the Level line":
                    EntryFilterLongDescription  += "the Slow %D is lower than the Level "  + sLevelLong;
                    EntryFilterShortDescription += "the Slow %D is higher than the Level " + sLevelShort;
                    ExitFilterLongDescription   += "the Slow %D is lower than the Level "  + sLevelLong;
                    ExitFilterShortDescription  += "the Slow %D is higher than the Level " + sLevelShort;
                    break;

                case "The Slow %D crosses the Level line upward":
                    EntryFilterLongDescription  += "the Slow %D crosses the Level " + sLevelLong  + " upward";
                    EntryFilterShortDescription += "the Slow %D crosses the Level " + sLevelShort + " downward";
                    ExitFilterLongDescription   += "the Slow %D crosses the Level " + sLevelLong  + " upward";
                    ExitFilterShortDescription  += "the Slow %D crosses the Level " + sLevelShort + " downward";
                    break;

                case "The Slow %D crosses the Level line downward":
                    EntryFilterLongDescription  += "the Slow %D crosses the Level " + sLevelLong  + " downward";
                    EntryFilterShortDescription += "the Slow %D crosses the Level " + sLevelShort + " upward";
                    ExitFilterLongDescription   += "the Slow %D crosses the Level " + sLevelLong  + " downward";
                    ExitFilterShortDescription  += "the Slow %D crosses the Level " + sLevelShort + " upward";
                    break;

                case "The %K crosses the Slow %D upward":
                    EntryFilterLongDescription  += "the %K crosses the Slow %D upward";
                    EntryFilterShortDescription += "the %K crosses the Slow %D downward";
                    ExitFilterLongDescription   += "the %K crosses the Slow %D upward";
                    ExitFilterShortDescription  += "the %K crosses the Slow %D downward";
                    break;

                case "The %K crosses the Slow %D downward":
                    EntryFilterLongDescription  += "the %K crosses the Slow %D downward";
                    EntryFilterShortDescription += "the %K crosses the Slow %D upward";
                    ExitFilterLongDescription   += "the %K crosses the Slow %D downward";
                    ExitFilterShortDescription  += "the %K crosses the Slow %D upward";
                    break;

                case "The %K is higher than the Slow %D":
                    EntryFilterLongDescription  += "the %K is higher than the Slow %D";
                    EntryFilterShortDescription += "the %K is lower than the Slow %D";
                    ExitFilterLongDescription   += "the %K is higher than the Slow %D";
                    ExitFilterShortDescription  += "the %K is lower than the Slow %D";
                    break;

                case "The %K is lower than  the Slow %D":
                    EntryFilterLongDescription  += "the %K is lower than the Slow %D";
                    EntryFilterShortDescription += "the %K is higher than than the Slow %D";
                    ExitFilterLongDescription   += "the %K is lower than the Slow %D";
                    ExitFilterShortDescription  += "the %K is higher than than the Slow %D";
                    break;

                case "The Slow %D changes its direction upward":
                    EntryFilterLongDescription  += "the Slow %D changes its direction upward";
                    EntryFilterShortDescription += "the Slow %D changes its direction downward";
                    ExitFilterLongDescription   += "the Slow %D changes its direction upward";
                    ExitFilterShortDescription  += "the Slow %D changes its direction downward";
                    break;

                case "The Slow %D changes its direction downward":
                    EntryFilterLongDescription  += "the Slow %D changes its direction downward";
                    EntryFilterShortDescription += "the Slow %D changes its direction upward";
                    ExitFilterLongDescription   += "the Slow %D changes its direction downward";
                    ExitFilterShortDescription  += "the Slow %D changes its direction upward";
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
                IndParam.ListParam[1].Text         + ", " + // Smoothing method
                IndParam.NumParam[0].ValueToString + ", " + // %K period
                IndParam.NumParam[1].ValueToString + ", " + // Fast %D period
                IndParam.NumParam[2].ValueToString + "," +  // Slow %D period
				"WTF=" + IndParam.ListParam[4].Text + ")";   

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