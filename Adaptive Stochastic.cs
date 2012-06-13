// Adaptive Stochastic Indicator
// Last changed on 2009-11-17
// Part of Forex Strategy Builder
// Website http://forexsb.com/
// Copyright (c) 2006 - 2009 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;
using System.Drawing;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Stochastics Indicator
    /// </summary>
    public class Adaptive_Stochastics : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Adaptive_Stochastics(SlotTypes slotType)
        {
            // General properties
            IndicatorName  = "Adaptive Stochastics";
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
                "The %D rises",
                "The %D falls",
                "The %D is higher than the Level line",
                "The %D is lower than the Level line",
                "The %D crosses the Level line upward",
                "The %D crosses the Level line downward",
                "The %D changes its direction upward",
                "The %D changes its direction downward",
                "The %K is higher than the %D",
                "The %K is lower than the %D",
                "The %K crosses the %D upward",
                "The %K crosses the %D downward",
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

            IndParam.NumParam[0].Caption = "Min %K period";
            IndParam.NumParam[0].Value   = 3;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 50;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The minimum value for smoothing period of %K.";

			IndParam.NumParam[1].Caption = "Max %K period";
            IndParam.NumParam[1].Value   = 28;
            IndParam.NumParam[1].Min     = 10;
            IndParam.NumParam[1].Max     = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The maximum value for smoothing period of %K.";
			
            IndParam.NumParam[2].Caption = "%D period";
            IndParam.NumParam[2].Value   = 3;
            IndParam.NumParam[2].Min     = 1;
            IndParam.NumParam[2].Max     = 100;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The fixed value for smoothing period of Fast %D.";

            IndParam.NumParam[4].Caption = "Level";
            IndParam.NumParam[4].Value   = 20;
            IndParam.NumParam[4].Min     = 0;
            IndParam.NumParam[4].Max     = 100;
            IndParam.NumParam[4].Enabled = true;
            IndParam.NumParam[4].ToolTip = "A critical level (for the appropriate logic).";

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
            MAMethod maMethod = (MAMethod)IndParam.ListParam[1].Index;
            int iKMin  = (int)IndParam.NumParam[0].Value;
			int iKMax  = (int)IndParam.NumParam[0].Value;
            int iD	   = (int)IndParam.NumParam[2].Value;
            int iLevel = (int)IndParam.NumParam[4].Value;
            int iPrvs  = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int iFirstBar = iKMax + iD + 3;
			
			double[] adHighs 		= new double[Bars];
            double[] adLows 		= new double[Bars];
			double[] adPriceStdDev 	= new double[Bars];
            double[] adMaxStdDev 	= new double[Bars];
			double[] adMinStdDev 	= new double[Bars];
			double[] adK = new double[Bars];
			
			double dRatio;
			int iKAvg = (int) (iKMax+iKMin)/2;
			int iK;
			
			adPriceStdDev = StdDev(Close, iKAvg);
			adMaxStdDev   = Highest(adPriceStdDev, iKAvg);
			adMinStdDev	  = Lowest (adPriceStdDev, iKAvg);		
            
            for (int iBar = iKMax; iBar < Bars; iBar++)
            {
                // Calculate Effective Length for %K Period
				if (adMaxStdDev[iBar]-adMinStdDev[iBar]!=0)
				{
					dRatio = (adPriceStdDev[iBar]-adMinStdDev[iBar])/(adMaxStdDev[iBar]-adMinStdDev[iBar]);
					iK = (int) (iKMin + (iKMax-iKMin)*(1-dRatio));
				}
				else
					iK = (int) iKMax;
				
				// Calculate Highest High and Lowest Low
				double dMax = double.MinValue;
				double dMin = double.MaxValue;
                for (int iBack = 0; iBack < iK; iBack++)
                {
                    if (High[iBar-iBack] > dMax) dMax = High[iBar-iBack];
					if (Low[iBar-iBack] < dMin) dMin = Low[iBar-iBack];
                }
                adHighs[iBar] = dMax;
				adLows[iBar] = dMin;
				
				// Calculate %K Value
				if (adHighs[iBar] == adLows[iBar])
                    adK[iBar] = 50;
                else
                    adK[iBar] = 100 * (Close[iBar] - adLows[iBar]) / (adHighs[iBar] - adLows[iBar]);
            }

			// Calculate %D Value
            double[] adD = MovingAverage(iD, 0, maMethod, adK);

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp();
            Component[0].CompName   = "%K";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor = Color.Brown;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adK;

            Component[1] = new IndicatorComp();
            Component[1].CompName   = "%D";
            Component[1].DataType   = IndComponentType.IndicatorValue;
            Component[1].ChartType  = IndChartType.Line;
            Component[1].ChartColor = Color.Blue;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = adD;

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

            if (IndParam.ListParam[0].Text == "The %K crosses the %D upward")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorCrossesAnotherIndicatorUpwardLogic(iFirstBar, iPrvs,adK, adD, ref Component[2], ref Component[3]);
                return;
            }
            else if (IndParam.ListParam[0].Text == "The %K crosses the %D downward")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorCrossesAnotherIndicatorDownwardLogic(iFirstBar, iPrvs, adK, adD, ref Component[2], ref Component[3]);
                return;
            }
            else if (IndParam.ListParam[0].Text == "The %K is higher than the %D")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorIsHigherThanAnotherIndicatorLogic(iFirstBar, iPrvs, adK, adD, ref Component[2], ref Component[3]);
                return;
            }
            else if (IndParam.ListParam[0].Text == "The %K is lower than the %D")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorIsLowerThanAnotherIndicatorLogic(iFirstBar, iPrvs, adK, adD, ref Component[2], ref Component[3]);
                return;
            }
            else
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "The %D rises":
                        indLogic = IndicatorLogic.The_indicator_rises;
                        SpecialValues = new double[1] { 50 };
                        break;

                    case "The %D falls":
                        indLogic = IndicatorLogic.The_indicator_falls;
                        SpecialValues = new double[1] { 50 };
                        break;

                    case "The %D is higher than the Level line":
                        indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                        SpecialValues = new double[2] { iLevel, 100 - iLevel };
                        break;

                    case "The %D is lower than the Level line":
                        indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                        SpecialValues = new double[2] { iLevel, 100 - iLevel };
                        break;

                    case "The %D crosses the Level line upward":
                        indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                        SpecialValues = new double[2] { iLevel, 100 - iLevel };
                        break;

                    case "The %D crosses the Level line downward":
                        indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                        SpecialValues = new double[2] { iLevel, 100 - iLevel };
                        break;

                    case "The %D changes its direction upward":
                        indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                        SpecialValues = new double[1] { 50 };
                        break;

                    case "The %D changes its direction downward":
                        indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                        SpecialValues = new double[1] { 50 };
                        break;

                    default:
                        break;
                }

                OscillatorLogic(iFirstBar, iPrvs, adD, iLevel, 100 - iLevel, ref Component[2], ref Component[3], indLogic);
            }

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
                case "The %D rises":
                    EntryFilterLongDescription  += "the %D rises";
                    EntryFilterShortDescription += "the %D falls";
                    ExitFilterLongDescription   += "the %D rises";
                    ExitFilterShortDescription  += "the %D falls";
                    break;

                case "The %D falls":
                    EntryFilterLongDescription  += "the %D falls";
                    EntryFilterShortDescription += "the %D rises";
                    ExitFilterLongDescription   += "the %D falls";
                    ExitFilterShortDescription  += "the %D rises";
                    break;

                case "The %D is higher than the Level line":
                    EntryFilterLongDescription  += "the %D is higher than the Level " + sLevelLong;
                    EntryFilterShortDescription += "the %D is lower than the Level "  + sLevelShort;
                    ExitFilterLongDescription   += "the %D is higher than the Level " + sLevelLong;
                    ExitFilterShortDescription  += "the %D is lower than the Level "  + sLevelShort;
                    break;

                case "The %D is lower than the Level line":
                    EntryFilterLongDescription  += "the %D is lower than the Level "  + sLevelLong;
                    EntryFilterShortDescription += "the %D is higher than the Level " + sLevelShort;
                    ExitFilterLongDescription   += "the %D is lower than the Level "  + sLevelLong;
                    ExitFilterShortDescription  += "the %D is higher than the Level " + sLevelShort;
                    break;

                case "The %D crosses the Level line upward":
                    EntryFilterLongDescription  += "the %D crosses the Level " + sLevelLong  + " upward";
                    EntryFilterShortDescription += "the %D crosses the Level " + sLevelShort + " downward";
                    ExitFilterLongDescription   += "the %D crosses the Level " + sLevelLong  + " upward";
                    ExitFilterShortDescription  += "the %D crosses the Level " + sLevelShort + " downward";
                    break;

                case "The %D crosses the Level line downward":
                    EntryFilterLongDescription  += "the %D crosses the Level " + sLevelLong  + " downward";
                    EntryFilterShortDescription += "the %D crosses the Level " + sLevelShort + " upward";
                    ExitFilterLongDescription   += "the %D crosses the Level " + sLevelLong  + " downward";
                    ExitFilterShortDescription  += "the %D crosses the Level " + sLevelShort + " upward";
                    break;

                case "The %K crosses the %D upward":
                    EntryFilterLongDescription  += "the %K crosses the %D upward";
                    EntryFilterShortDescription += "the %K crosses the %D downward";
                    ExitFilterLongDescription   += "the %K crosses the %D upward";
                    ExitFilterShortDescription  += "the %K crosses the %D downward";
                    break;

                case "The %K crosses the %D downward":
                    EntryFilterLongDescription  += "the %K crosses the %D downward";
                    EntryFilterShortDescription += "the %K crosses the %D upward";
                    ExitFilterLongDescription   += "the %K crosses the %D downward";
                    ExitFilterShortDescription  += "the %K crosses the %D upward";
                    break;

                case "The %K is higher than the %D":
                    EntryFilterLongDescription  += "the %K is higher than the %D";
                    EntryFilterShortDescription += "the %K is lower than the %D";
                    ExitFilterLongDescription   += "the %K is higher than the %D";
                    ExitFilterShortDescription  += "the %K is lower than the %D";
                    break;

                case "The %K is lower than  the %D":
                    EntryFilterLongDescription  += "the %K is lower than the %D";
                    EntryFilterShortDescription += "the %K is higher than than the %D";
                    ExitFilterLongDescription   += "the %K is lower than the %D";
                    ExitFilterShortDescription  += "the %K is higher than than the %D";
                    break;

                case "The %D changes its direction upward":
                    EntryFilterLongDescription  += "the %D changes its direction upward";
                    EntryFilterShortDescription += "the %D changes its direction downward";
                    ExitFilterLongDescription   += "the %D changes its direction upward";
                    ExitFilterShortDescription  += "the %D changes its direction downward";
                    break;

                case "The %D changes its direction downward":
                    EntryFilterLongDescription  += "the %D changes its direction downward";
                    EntryFilterShortDescription += "the %D changes its direction upward";
                    ExitFilterLongDescription   += "the %D changes its direction downward";
                    ExitFilterShortDescription  += "the %D changes its direction upward";
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
                IndParam.NumParam[0].ValueToString + ", " + // %K Min period
                IndParam.NumParam[1].ValueToString + ", " + // %K Max period
                IndParam.NumParam[2].ValueToString + ")";   // %D period

            return sString;
        }
    
		protected static double[] Highest(double[] adHigh, int iLen)
		{
			double[] adHighest = new double[Bars];

            for (int iBar = iLen; iBar < Bars; iBar++)
            {
                double dMax = double.MinValue;
                for (int iBack = 0; iBack < iLen; iBack++)
                {
                    if (adHigh[iBar-iBack] > dMax) dMax = adHigh[iBar-iBack];
                }
                adHighest[iBar] = dMax;
            }			
			return adHighest;
		}
		
		protected static double[] Lowest(double[] adLow, int iLen)
		{
            double[] adLowest  = new double[Bars];

            for (int iBar = iLen; iBar < Bars; iBar++)
            {
                double dMin = double.MaxValue;
                for (int iBack = 0; iBack < iLen; iBack++)
                {
                    if (adLow[iBar-iBack] < dMin) dMin = adLow[iBar-iBack];
                }
                adLowest[iBar]  = dMin;
            }			
			return adLowest;
		}
		
		protected static double[] StdDev(double[] adPrice, int iLen)
		{
			double[] adStdDev = new double[Bars];
			double[] adVar = new double[Bars];
			
			adVar = Var(adPrice, iLen);
			
			for (int iBar = iLen; iBar<Bars; iBar++)
			{
				adStdDev[iBar] = Math.Sqrt(adVar[iBar]);
			}
			
			return adStdDev;
		}
		
		protected static double[] Var(double[] adPrice, int iLen)
		{
			double[] adVar = new double[Bars];
			double[] adMean = new double[Bars];
			double dSum;
			
			adMean = Avg(adPrice, iLen);
			
			for (int iBar = iLen; iBar<Bars; iBar++)
			{
				dSum = 0;
				for (int iBack = 0; iBack<iLen; iBack++)
				{
					dSum = dSum + Math.Pow(adPrice[iBar-iBack]-adMean[iBar-iBack],2);
				}
				adVar[iBar] = dSum/iLen;
			}			
			return adVar;	
		}
	
		protected static double[] Avg(double[] adPrice, int iLen)
		{
			double[] adAvg = new double[Bars];
			double dSum;
			
			for (int iBar = iLen; iBar<Bars; iBar++)
			{
				dSum = 0;
				for (int iBack = 0; iBack<iLen; iBack++)
				{
					dSum = dSum + adPrice[iBar-iBack];
				}
				adAvg[iBar] = dSum/iLen;
			}			
			return adAvg;
		}
	
	
	}
}
