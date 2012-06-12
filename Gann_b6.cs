// Gann Trend Indicator
// Last changed on 12/10/2011
// Part of Forex Strategy Builder v2.8.3.8+
// Website http://forexsb.com/
// Copyright (c) 2006 - 2009 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Gann Trend Indicator
	/// Cross between Donchian Channel and Donchian Counter
	/// if n consecutive highs, then draw upper Donchian Band
	/// if n consecutive lows, then draw lower Donchian Band
	/// if currently have less than required n consecutive highs or lows, don't draw indicator
	///
	///  b2 -- added Open and Close Filters
	///  b4 -- removed Open and Close Filters
	///   -- lock Bar Range Bands at period = 2, only need number of consecutive highs / lows
	///   -- if bar has both higher and lower, then call consolidation
	///   b5 -- direction preference -- if bar color is in direction of trend, then continues trend
	///   b6 -- 
    /// </summary>
    public class Gann_b6 : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Gann_b6(SlotTypes slotType)
        {
            // General properties
            IndicatorName  = "Gann (beta 6)";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
			CustomIndicator = true;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
			if (slotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new string[]
                {
					"The Trend is Up",
					"The Trend is Down",
					"The Trend changes from Consolidation to Up",
					"The Trend changes from Consolidation to Down",
					"The Trend changes from Up to Consolidation",
					"The Trend changes from Down to Consolidation"
                };
            else if (slotType == SlotTypes.CloseFilter)
                IndParam.ListParam[0].ItemList = new string[]
                {
					"The Trend is Up",
					"The Trend is Down",
					"The Trend changes from Consolidation to Up",
					"The Trend changes from Consolidation to Down",
					"The Trend changes from Up to Consolidation",
					"The Trend changes from Down to Consolidation"
                };
            else
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Not Defined"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the Gann Line.";

            IndParam.ListParam[1].Caption  = "Tall Bar Preference";
            IndParam.ListParam[1].ItemList = new string[] { 
				"Consolidation Preference",
				"Continuation Preference",
				"Directional Preference"
			};
            IndParam.ListParam[1].Index    = 0;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
			IndParam.ListParam[1].ToolTip = "Always uses highs and lows only";

			// The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Consecutive Highs/Lows";
            IndParam.NumParam[0].Value   = 2;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The number of consecutive highs or lows to switch between high or low band";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Checked = PrepareUsePrevBarValueCheckBox(slotType);
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";


            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            // Reading the parameters
            int iHits  = (int)IndParam.NumParam[0].Value;
            int iPrvs   = IndParam.CheckParam[0].Checked ? 1 : 0;
			string sPref = IndParam.ListParam[1].Text;
	
            // Calculation
			double[] adUpBand  = new double[Bars];
			double[] adDnBand  = new double[Bars];
			double[] adHighLowCtr = new double[Bars];
			double[] adGann = new double [Bars];
			double[] adTrendUp = new double [Bars];
			double[] adConsolidation = new double [Bars];
			double[] adTrendDown = new double [Bars];

			// period hard coded to 2 so always comparing 2 bars
            int iPeriod = 2;

            int iFirstBar = iPeriod + iHits + iPrvs + 2;

            for (int iBar = iFirstBar; iBar < Bars; iBar++)
            {
                double dMax = double.MinValue;
                double dMin = double.MaxValue;

                for (int i = 0; i < iPeriod; i++)
                {
                    if (High[iBar - i] > dMax) {
						dMax = High[iBar - i];
					}
                    if (Low[iBar - i]  < dMin) {
						dMin = Low[iBar - i];
					}
                }
                adUpBand[iBar] = dMax;
                adDnBand[iBar] = dMin;

				// count backwards how many band pushes there have been
				// separate counters for high and low band -- for breakout bars, give preference to continue as previous up / down line
				// if previous was consolidation, contnue as consolidation
				int iUpCtr = 0;
				int iDnCtr = 0;
				int iCtr = 0;
				
				// for highs, use positive values for ctr
				// for lows, use negative values for ctr
				// works out to have pos and neg values for histogram
				if (High[iBar] >= dMax)
				{
					while (iBar-iUpCtr > iFirstBar && (High[iBar-iUpCtr]) >= adUpBand[iBar-iUpCtr])
					{
						iUpCtr += 1;
					}
				}
				if (Low[iBar] <= dMin)
				{
					while (iBar+iDnCtr > iFirstBar && (Low[iBar+iDnCtr]) <= adDnBand[iBar+iDnCtr])
					{
						iDnCtr -= 1;
					}
				}

				// switch condition for preference
				switch (sPref) {
				case "Consolidation Preference":
					if (iUpCtr != 0 && iDnCtr != 0) {
						iCtr = 0;
					}
					else if (Math.Abs(iUpCtr) > Math.Abs(iDnCtr)) {
						iCtr = iUpCtr;
					}
					else if (Math.Abs(iUpCtr) < Math.Abs(iDnCtr))
					{
						iCtr = iDnCtr;
					}
					break;

				case "Continuation Preference":
					if (Math.Abs(iUpCtr) > Math.Abs(iDnCtr)) {
						iCtr = iUpCtr;
					}
					else if (Math.Abs(iUpCtr) < Math.Abs(iDnCtr))
					{
						iCtr = iDnCtr;
					}
					break;

				case "Directional Preference":
					// both high and low, let direction of bar be tie breaker
					if (iUpCtr != 0 && iDnCtr != 0) {
						if (Math.Abs(iUpCtr) > Math.Abs(iDnCtr) && Close[iBar] > Open[iBar]) {
							iCtr = iUpCtr;
						}
						else if (Math.Abs(iUpCtr) < Math.Abs(iDnCtr) && Close[iBar] < Open[iBar])
						{
							iCtr = iDnCtr;
						}
					}
					else if (Math.Abs(iUpCtr) > Math.Abs(iDnCtr)) {
						iCtr = iUpCtr;
					}
					else if (Math.Abs(iUpCtr) < Math.Abs(iDnCtr))
					{
						iCtr = iDnCtr;
					}
					break;

				default:
					break;

				}

				
				// keep for use with Trend Up - Trend Down Logic
				adHighLowCtr[iBar] = iCtr;

				// if over hits, then draw upper band; if lower then draw lower band
				// if counter is positive, then uptrend; negative, downtrend; 0, consolidation
				 if (iCtr >= iHits)
				{
					adGann[iBar] = adUpBand[iBar];
					adTrendUp[iBar] = 1.0;
				}
				else if (iCtr <= -iHits)
				{
					adGann[iBar] = adDnBand[iBar];
					adTrendDown[iBar] = 1.0;
				}
				else {
					adConsolidation[iBar] = 1.0;
				}

            }

            // Saving the components
            Component = new IndicatorComp[6];

            Component[0] = new IndicatorComp();
            Component[0].CompName   = "Gann Line";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor = Color.Green;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adGann;

            Component[1] = new IndicatorComp();
            Component[1].CompName   = "Trend Up";
            Component[1].DataType   = IndComponentType.IndicatorValue;
            Component[1].ChartType  = IndChartType.NoChart;
            Component[1].ChartColor = Color.Blue;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = adTrendUp;

            Component[2] = new IndicatorComp();
            Component[2].CompName   = "Consolidation";
            Component[2].DataType   = IndComponentType.IndicatorValue;
            Component[2].ChartType  = IndChartType.NoChart;
            Component[2].ChartColor = Color.Blue;
            Component[2].FirstBar   = iFirstBar;
            Component[2].Value      = adConsolidation;

            Component[3] = new IndicatorComp();
            Component[3].CompName   = "Trend Down";
            Component[3].DataType   = IndComponentType.IndicatorValue;
            Component[3].ChartType  = IndChartType.NoChart;
            Component[3].ChartColor = Color.Blue;
            Component[3].FirstBar   = iFirstBar;
            Component[3].Value      = adTrendDown;

            Component[4] = new IndicatorComp();
            Component[4].ChartType  = IndChartType.NoChart;
            Component[4].FirstBar   = iFirstBar;
            Component[4].Value      = new double[Bars];

            Component[5] = new IndicatorComp();
            Component[5].ChartType  = IndChartType.NoChart;
            Component[5].FirstBar   = iFirstBar;
            Component[5].Value      = new double[Bars];

            // Sets the Component's type.
			if (slotType == SlotTypes.OpenFilter)
            {
                Component[4].DataType = IndComponentType.AllowOpenLong;
                Component[4].CompName = "Is long entry allowed";
                Component[5].DataType = IndComponentType.AllowOpenShort;
                Component[5].CompName = "Is short entry allowed";
            }
            else if (slotType == SlotTypes.CloseFilter)
            {
                Component[4].DataType = IndComponentType.ForceCloseLong;
                Component[4].CompName = "Close out long position";
                Component[5].DataType = IndComponentType.ForceCloseShort;
                Component[5].CompName = "Close out short position";
            }

			switch (IndParam.ListParam[0].Text)
			{
				case "The Trend is Up":
					for (int iBar = iFirstBar; iBar < Bars; iBar++)
					{
						Component[4].Value[iBar] = adTrendUp[iBar-iPrvs];
						Component[5].Value[iBar] = adTrendDown[iBar-iPrvs];
					}
					break;

				case "The Trend is Down":
					for (int iBar = iFirstBar; iBar < Bars; iBar++)
					{
						Component[4].Value[iBar] = adTrendDown[iBar-iPrvs];
						Component[5].Value[iBar] = adTrendUp[iBar-iPrvs];
					}
					break;

				case "The Trend changes from Consolidation to Up":
					From0Logic(iFirstBar, iPrvs, adTrendUp, 1,ref Component[4]);
					From0Logic(iFirstBar, iPrvs, adTrendDown, 1,ref Component[5]);
					break;

			   case "The Trend changes from Consolidation to Down":
					From0Logic(iFirstBar, iPrvs, adTrendUp, 1,ref Component[5]);
					From0Logic(iFirstBar, iPrvs, adTrendDown, 1,ref Component[4]);
					break;


				case "The Trend changes from Up to Consolidation":
					To0Logic(iFirstBar, iPrvs, adTrendUp, 1, ref Component[4]);
					To0Logic(iFirstBar, iPrvs, adTrendDown, 1, ref Component[5]);
					break;

			   case "The Trend changes from Down to Consolidation":
					To0Logic(iFirstBar, iPrvs, adTrendUp, 1, ref Component[5]);
					To0Logic(iFirstBar, iPrvs, adTrendDown, 1, ref Component[4]);
					break;


			   case "The position opens above the Upper Band":
					Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
					Component[1].PosPriceDependence = PositionPriceDependence.PriceSellLower;
					Component[0].UsePreviousBar = iPrvs;
					Component[1].UsePreviousBar = iPrvs;
					Component[4].DataType = IndComponentType.Other;
					Component[5].DataType = IndComponentType.Other;
					Component[4].ShowInDynInfo = false;
					Component[5].ShowInDynInfo = false;
					break;

				case "The position opens below the Upper Band":
					Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
					Component[1].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
					Component[0].UsePreviousBar = iPrvs;
					Component[1].UsePreviousBar = iPrvs;
					Component[4].DataType = IndComponentType.Other;
					Component[5].DataType = IndComponentType.Other;
					Component[4].ShowInDynInfo = false;
					Component[5].ShowInDynInfo = false;
					break;

				case "The position opens above the Lower Band":
					Component[0].PosPriceDependence = PositionPriceDependence.PriceSellLower;
					Component[1].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
					Component[0].UsePreviousBar = iPrvs;
					Component[1].UsePreviousBar = iPrvs;
					Component[4].DataType = IndComponentType.Other;
					Component[5].DataType = IndComponentType.Other;
					Component[4].ShowInDynInfo = false;
					Component[5].ShowInDynInfo = false;
					break;

				case "The position opens below the Lower Band":
					Component[0].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
					Component[1].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
					Component[0].UsePreviousBar = iPrvs;
					Component[1].UsePreviousBar = iPrvs;
					Component[4].DataType = IndComponentType.Other;
					Component[5].DataType = IndComponentType.Other;
					Component[4].ShowInDynInfo = false;
					Component[5].ShowInDynInfo = false;
					break;

				  default:
					break;
			}


            return;
		}

		// simple logic if array goes from 1 to 0, won't work in general case
		// suitable for this indicator because trend up / trend down / consolidation are their own separate arrays
        protected void To0Logic (int iFirstBar, int iPrvs, double[] adIndValue, double dLevel, ref IndicatorComp indComp)
		{
			if (dLevel == 0)   // return if 0 for level, becomes meaningless comparison
			{
				return;
			}
			for (int iBar = iFirstBar; iBar < Bars; iBar++)
			{
				if (adIndValue[iBar-iPrvs] == 0)
				{
					if (adIndValue[iBar-iPrvs-1] >= dLevel)
					{
						indComp.Value[iBar] = 1;
					}
				}
			}
		}

		// simple logic if array goes from 0 to 1, won't work in general case
		// suitable for this indicator because trend up / trend down / consolidation are their own separate arrays
        protected void From0Logic (int iFirstBar, int iPrvs, double[] adIndValue, double dLevel, ref IndicatorComp indComp)
		{
			if (dLevel == 0)   // return if 0 for level, becomes meaningless comparison
			{
				return;
			}
			for (int iBar = iFirstBar; iBar < Bars; iBar++)
			{
				if (adIndValue[iBar-iPrvs] >= dLevel)
				{
					if (adIndValue[iBar-iPrvs-1] == 0)
					{
						indComp.Value[iBar] = 1;
					}
				}
			}
		}

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            switch (IndParam.ListParam[0].Text)
            {
				case "Enter long at Up Trend":
					EntryPointLongDescription  = "at the previous high if the Trend is Up";
                    EntryPointShortDescription = "at the previous low if the Trend is Down";
					break;

				case "Enter long at Down Trend":
					EntryPointLongDescription  = "at the previous low if the Trend is Down";
                    EntryPointShortDescription = "at the previous high if the Trend is Up";
					break;

				case "Exit long at Up Trend":
					ExitPointLongDescription  = "at the previous high if the Trend is Up";
                    ExitPointShortDescription = "at the previous low if the Trend is Down";
					break;

				case "Exit long at Down Trend":
					ExitPointLongDescription  = "at the previous low if the Trend is Down";
                    ExitPointShortDescription = "at the previous high if the Trend is Up";
					break;

				case "The Trend is Up":
					EntryPointLongDescription  = "if the Trend is Up";
                    EntryPointShortDescription = "if the Trend is Down";
					ExitPointLongDescription  = "if the Trend is Up";
                    ExitPointShortDescription = "if the Trend is Down";
					break;

				case "The Trend is Down":
					EntryPointLongDescription  = "if the Trend is Down";
                    EntryPointShortDescription = "if the Trend is Up";
					ExitPointLongDescription  = "if the Trend is Down";
                    ExitPointShortDescription = "if the Trend is Up";
					break;

				case "The Trend changes from Consolidation to Up":
					EntryPointLongDescription  = "if the Trend changes from Consolidation to Up";
                    EntryPointShortDescription = "if the Trend changes from Consolidation to Down";
					ExitPointLongDescription  = "if the Trend changes from Consolidation to Up";
                    ExitPointShortDescription = "if the Trend changes from Consolidation to Down";
					break;

				case "The Trend changes from Consolidation to Down":
					EntryPointLongDescription  = "if the Trend changes from Consolidation to Down";
                    EntryPointShortDescription = "if the Trend changes from Consolidation to Up";
					ExitPointLongDescription  = "if the Trend changes from Consolidation to Down";
                    ExitPointShortDescription = "if the Trend changes from Consolidation to Up";
					break;

				case "The Trend changes from Up to Consolidation":
					EntryPointLongDescription  = "if the Trend changes from Up to Consolidation";
                    EntryPointShortDescription = "if the Trend changes from Down to Consolidation";
					ExitPointLongDescription  = "if the Trend changes from Up to Consolidation";
                    ExitPointShortDescription = "if the Trend changes from Down to Consolidation";
					break;

				case "The Trend changes from Down to Consolidation":
					EntryPointLongDescription  = "if the Trend changes from Down to Consolidation";
                    EntryPointShortDescription = "if the Trend changes from Up to Consolidation";
					ExitPointLongDescription  = "if the Trend changes from Down to Consolidation";
                    ExitPointShortDescription = "if the Trend changes from Up to Consolidation";
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
                IndParam.NumParam[0].ValueToString + "," + // Hits
				IndParam.ListParam[1].Text + ")";   

            return sString;
        }
    }
}
