// Oscar Indicator
// Last changed on 2009-05-05
// Part of Forex Strategy Builder v2.8.3.7+
// Website http://forexsb.com/
// Copyright (c) 2006 - 2009 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;
using System.Drawing;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Oscar Indicator
	/// for more info:  http://www.forexfactory.com/showthread.php?t=184856
    /// </summary>
    public class Oscar_3 : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Oscar_3(SlotTypes slotType)
        {
            // General properties
            IndicatorName  = "Oscar 3";
            PossibleSlots  = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
			CustomIndicator = true;
            SeparatedChart = true;
			SeparatedChartMinValue = 0;
			SeparatedChartMaxValue = 100;


            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;
            IndParam.IndicatorType = TypeOfIndicator.Additional;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "The Oscar rises",
                "The Oscar falls",
                "The Oscar is higher than the Level line",
                "The Oscar is lower than the Level line",
                "The Oscar crosses the Level line upward",
                "The Oscar crosses the Level line downward",
                "The Oscar changes its direction upward",
                "The Oscar changes its direction downward",
				"Draw Only, no Entry or Exit"
            };
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption  = "Chart Style";
            IndParam.ListParam[1].ItemList = new string[]
            {
                "Line",
				"Histogram"
            };
            IndParam.ListParam[1].Index   = 0;
            IndParam.ListParam[1].Text    = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Draw Indicator as Line or Histogram";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period";
            IndParam.NumParam[0].Value   = 10;
            IndParam.NumParam[0].Min     = 2;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of calculation.";

            IndParam.NumParam[1].Caption = "Level";
            IndParam.NumParam[1].Value   = 80;
            IndParam.NumParam[1].Min     = 1;
            IndParam.NumParam[1].Max     = 99;
            IndParam.NumParam[1].Point   = 0;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "A critical level (for the appropriate logic).";

            IndParam.NumParam[2].Caption = "Oscar Smoothing";
            IndParam.NumParam[2].Value   = 1;
            IndParam.NumParam[2].Min     = 1;
            IndParam.NumParam[2].Max     = 200;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "Smoothing period for Oscar Indicator.";

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
            int       iPeriod  = (int)IndParam.NumParam[0].Value;
            int iLevel   = (int)IndParam.NumParam[1].Value;
			int iSmooth = (int)IndParam.NumParam[2].Value;
			int       iPrvs    =      IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation

			double[] adOscar = new double[Bars];

			double[] adOscarSmoothed = new double[Bars];
            int iFirstBar = iPeriod + 1;
			double dY = 0;

            for (int iBar = iPeriod; iBar < Bars; iBar++)
            {
				double dX = dY;
				double dHighest = 0;
				double dLowest = 999999;

                for (int iIndex = 0; iIndex < iPeriod; iIndex++)
                {
					dHighest = Math.Max(High[iBar-iIndex], dHighest);
					dLowest = Math.Min(Low[iBar-iIndex], dLowest);
                }
				double dClose = Close[iBar];
				double dRough = (dClose - dLowest) / (dHighest - dLowest) * 100;
				dY = (((dX/3)*2) + (dRough/3));

				adOscar[iBar] = dY;
            }

			adOscarSmoothed = MovingAverage(iSmooth, 0, MAMethod.Simple, adOscar);

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp();
            Component[0].CompName   = "Oscar";
            Component[0].DataType   = IndComponentType.IndicatorValue;
			if(IndParam.ListParam[1].Text == "Line") {
	            Component[0].ChartType  = IndChartType.Line;
			}
			else if (IndParam.ListParam[1].Text == "Histogram")
			{
	            Component[0].ChartType  = IndChartType.Histogram;
			}

            Component[0].ChartColor = Color.Blue;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adOscarSmoothed;

            Component[1] = new IndicatorComp();
            Component[1].ChartType = IndChartType.NoChart;
            Component[1].FirstBar  = iFirstBar;
            Component[1].Value     = new double[Bars];

            Component[2] = new IndicatorComp();
            Component[2].ChartType = IndChartType.NoChart;
            Component[2].FirstBar  = iFirstBar;
            Component[2].Value     = new double[Bars];

            // Sets the Component's type
            if (slotType == SlotTypes.OpenFilter)
            {
                Component[1].DataType = IndComponentType.AllowOpenLong;
                Component[1].CompName = "Is long entry allowed";
                Component[2].DataType = IndComponentType.AllowOpenShort;
                Component[2].CompName = "Is short entry allowed";
            }
            else if (slotType == SlotTypes.CloseFilter)
            {
                Component[1].DataType = IndComponentType.ForceCloseLong;
                Component[1].CompName = "Close out long position";
                Component[2].DataType = IndComponentType.ForceCloseShort;
                Component[2].CompName = "Close out short position";
            }

            // Calculation of the logic
            IndicatorLogic indLogic = IndicatorLogic.It_does_not_act_as_a_filter;

            switch (IndParam.ListParam[0].Text)
            {
                case "The Oscar rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
					SpecialValues = new double[1] {50};
                    break;

                case "The Oscar falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
					SpecialValues = new double[1] {50};
                    break;

                case "The Oscar is higher than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new double[2] { iLevel, 100 - iLevel };
                    break;

                case "The Oscar is lower than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new double[2] { iLevel, 100 - iLevel };
                    break;

                case "The Oscar crosses the Level line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new double[2] { iLevel, 100 - iLevel };
                    break;

                case "The Oscar crosses the Level line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new double[2] { iLevel, 100 - iLevel };
                    break;

                case "The Oscar changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
					SpecialValues = new double[1] {50};
                    break;

                case "The Oscar changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
					SpecialValues = new double[1] {50};
                    break;

				case "Draw Only, no Entry or Exit":
					indLogic =  IndicatorLogic.It_does_not_act_as_a_filter;
					Component[1].DataType = IndComponentType.Other;
					Component[1].CompName = "Visual Only";
					Component[2].DataType = IndComponentType.Other;
					Component[2].CompName = "Visual Only";
					SpecialValues = new double[2] { iLevel, 100 - iLevel };
					break;

                default:
                    break;
            }

            OscillatorLogic(iFirstBar, iPrvs, adOscarSmoothed, iLevel, 100 - iLevel, ref Component[1], ref Component[2], indLogic);
            

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
                case "The Oscar rises":
                    EntryFilterLongDescription  += "rises";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription   += "rises";
                    ExitFilterShortDescription  += "rises";
                    break;

                case "The Oscar falls":
                    EntryFilterLongDescription  += "falls";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription   += "falls";
                    ExitFilterShortDescription  += "falls";
                    break;

                case "The Oscar is higher than the Level line":
                    EntryFilterLongDescription  += "is higher than the Level " + sLevelLong;
                    EntryFilterShortDescription += "is higher than the Level " + sLevelShort;
                    ExitFilterLongDescription   += "is higher than the Level " + sLevelLong;
                    ExitFilterShortDescription  += "is higher than the Level " + sLevelShort;
                    break;

                case "The Oscar is lower than the Level line":
                    EntryFilterLongDescription  += "is lower than the Level "  + sLevelLong;
                    EntryFilterShortDescription += "is lower than the Level "  + sLevelShort;
                    ExitFilterLongDescription   += "is lower than the Level "  + sLevelLong;
                    ExitFilterShortDescription  += "is lower than the Level "  + sLevelShort;
                    break;

                case "The Oscar crosses the Level line upward":
                    EntryFilterLongDescription  += "crosses the Level " + sLevelLong  + " upward";
                    EntryFilterShortDescription += "crosses the Level " + sLevelShort + " upward";
                    ExitFilterLongDescription   += "crosses the Level " + sLevelLong  + " upward";
                    ExitFilterShortDescription  += "crosses the Level " + sLevelShort + " upward";
                    break;

                case "The Oscar crosses the Level line downward":
                    EntryFilterLongDescription  += "crosses the Level " + sLevelLong  + " downward";
                    EntryFilterShortDescription += "crosses the Level " + sLevelShort + " downward";
                    ExitFilterLongDescription   += "crosses the Level " + sLevelLong  + " downward";
                    ExitFilterShortDescription  += "crosses the Level " + sLevelShort + " downward";
                    break;

                case "The Oscar changes its direction upward":
                    EntryFilterLongDescription  += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription   += "changes its direction upward";
                    ExitFilterShortDescription  += "changes its direction upward";
                    break;

                case "The Oscar changes its direction downward":
                    EntryFilterLongDescription  += "changes its direction downward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription   += "changes its direction downward";
                    ExitFilterShortDescription  += "changes its direction downward";
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
			int iLevel = (int)IndParam.NumParam[1].Value; 
			int iOppLevel = 100 - iLevel;
            string sString = IndicatorName + 
                (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                "period: " + IndParam.NumParam[0].ValueToString + ", " +  // period
				"levels: " + iLevel.ToString() + " - " + iOppLevel.ToString() + ", " +  // Level
				"smoothing: " + IndParam.NumParam[2].ValueToString +	")";   // smoothing

            return sString;
        }
    }
}
 