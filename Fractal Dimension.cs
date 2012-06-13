// Fractal Dimension Index
// Last changed on 18 Feb 2010
// Part of Forex Strategy Builder v2.8.3.5+
// Website http://forexsb.com/
// Copyright (c) 2010 Denny Imanuel - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;
using System.Drawing;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// FDI Indicator
    /// </summary>
    public class Fractal_Dimension_Index : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Fractal_Dimension_Index(SlotTypes slotType)
        {
            // General properties
            IndicatorName  = "Fractal Dimension Index";
            PossibleSlots  = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
            // SeparatedChartMinValue = 0;
            // SeparatedChartMaxValue = 100;
			CustomIndicator = true;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "The FDI rises",
                "The FDI falls",
                "The FDI is higher than the Level line",
                "The FDI is lower than the Level line",
                "The FDI crosses the Level line upward",
                "The FDI crosses the Level line downward",
                "The FDI changes its direction upward",
                "The FDI changes its direction downward"
            };
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption  = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[1].Index    = (int)MAMethod.Exponential;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "The Moving Average method used for smoothing the FDI value.";

            IndParam.ListParam[2].Caption  = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[2].Index    = (int)BasePrice.Close;
            IndParam.ListParam[2].Text     = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "The price the RSI is based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period";
            IndParam.NumParam[0].Value   = 30;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of FDI.";

            IndParam.NumParam[1].Caption = "Level";
            IndParam.NumParam[1].Value   = 1.5;
            IndParam.NumParam[1].Min     = 1;
            IndParam.NumParam[1].Max     = 2;
            IndParam.NumParam[1].Point   = 2;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "A critical level (for the appropriate logic).";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value.";
            IndParam.CheckParam[0].Checked = (slotType == SlotTypes.OpenFilter);
            IndParam.CheckParam[0].Enabled = false;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";

            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            // Reading the parameters
            MAMethod  maMethod  = (MAMethod)IndParam.ListParam[1].Index;
            BasePrice basePrice = (BasePrice)IndParam.ListParam[2].Index;
            int       iPeriod   = (int)IndParam.NumParam[0].Value;
            double    dLevel    = IndParam.NumParam[1].Value;
            int       iPrvs     = (slotType == SlotTypes.OpenFilter) ? 1 : 0;

            //Calculation
            int iFirstBar = iPeriod + 10;
            double[] adBasePrice = Price(basePrice);

            double[] adFractalDim  	= new double[Bars];
			double[] adUpperBand 	= new double[Bars];
			double[] adLowerBand 	= new double[Bars];
			double[] adLevel	 	= new double[Bars];
			double dPriceMax;
			double dPriceMin;
			double dDiff;
			double dPriorDiff;
			double dLength;
			double dVar;
			double dStdDev;
			double dMean;
			double dSum;
			double dDelta;

			for (int iBar=iFirstBar; iBar<Bars; iBar++)
			{
				dPriceMax = double.MinValue;
				dPriceMin = double.MaxValue;
				for (int iBack=0; iBack<iPeriod; iBack++)
				{				
					if (adBasePrice[iBar-iBack]>dPriceMax)
						dPriceMax = adBasePrice[iBar-iBack];
					if (adBasePrice[iBar-iBack]<dPriceMin)
						dPriceMin = adBasePrice[iBar-iBack];
				}
				dPriorDiff = 0;
				dLength = 0;
				dSum = 0;
				for (int iIteration=1; iIteration<iPeriod; iIteration++)
				{
					if (dPriceMax-dPriceMin>0)
					{
						dDiff = (adBasePrice[iIteration]-dPriceMin)/(dPriceMax-dPriceMin);
						if (iIteration>1)
						{
							dLength += Math.Sqrt( Math.Pow(dDiff-dPriorDiff,2) + (1/Math.Pow(iPeriod,2)) );
						}
						dPriorDiff = dDiff;
					}
				}
				if (dLength>0)
				{
					adFractalDim[iBar] = 1 + (Math.Log(dLength) + Math.Log(2))/Math.Log(2*iPeriod);
					dMean = dLength/(iPeriod-1);
					for (int iIteration=1; iIteration<iPeriod; iIteration++)
					{
						if (dPriceMax-dPriceMin>0)
						{
							dDiff = (adBasePrice[iIteration]-dPriceMin)/(dPriceMax-dPriceMin);
							if (iIteration>1)
							{
								dDelta = Math.Sqrt( Math.Pow(dDiff-dPriorDiff,2) + (1/Math.Pow(iPeriod,2)) );
								dSum += Math.Pow(dDelta-(dLength/(iPeriod-1)),2);
							}
							dPriorDiff = dDiff;
						}
					}
					dVar = dSum / (Math.Pow(dLength,2)*Math.Pow(Math.Log(2*(iPeriod-1)),2) );
				}
				else
				{
					adFractalDim[iBar] = 0;
					dVar = 0;
				}
				dStdDev = Math.Sqrt(dVar);
				adUpperBand[iBar] = adFractalDim[iBar] + dStdDev;
				adLowerBand[iBar] = adFractalDim[iBar] - dStdDev;
				adLevel[iBar] = dLevel;
			}	

            // Saving the components
            Component = new IndicatorComp[6];

            Component[0] = new IndicatorComp();
            Component[0].CompName   = "FDI";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor = Color.Blue;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adFractalDim;

            Component[1] = new IndicatorComp();
            Component[1].ChartType  = IndChartType.NoChart;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = new double[Bars];

            Component[2] = new IndicatorComp();
            Component[2].ChartType  = IndChartType.NoChart;
            Component[2].FirstBar   = iFirstBar;
            Component[2].Value      = new double[Bars];
			
			Component[3] = new IndicatorComp();
            Component[3].CompName   = "Upper Band";
            Component[3].DataType   = IndComponentType.IndicatorValue;
            Component[3].ChartType  = IndChartType.Line;
            Component[3].ChartColor = Color.Red;
            Component[3].FirstBar   = iFirstBar;
            Component[3].Value      = adUpperBand;
			
			Component[4] = new IndicatorComp();
            Component[4].CompName   = "Lower Band";
            Component[4].DataType   = IndComponentType.IndicatorValue;
            Component[4].ChartType  = IndChartType.Line;
            Component[4].ChartColor = Color.Green;
            Component[4].FirstBar   = iFirstBar;
            Component[4].Value      = adLowerBand;
			
			Component[5] = new IndicatorComp();
            Component[5].CompName   = "Level";
            Component[5].DataType   = IndComponentType.IndicatorValue;
            Component[5].ChartType  = IndChartType.Line;
            Component[5].ChartColor = Color.Orange;
            Component[5].FirstBar   = iFirstBar;
            Component[5].Value      = adLevel;

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
                case "The FDI rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
                    break;

                case "The FDI falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
                    break;

                case "The FDI is higher than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new double[1] { dLevel };
                    break;

                case "The FDI is lower than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new double[1] { dLevel };
                    break;

                case "The FDI crosses the Level line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new double[1] { dLevel };
                    break;

                case "The FDI crosses the Level line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new double[1] { dLevel };
                    break;

                case "The FDI changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    break;

                case "The FDI changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    break;

                default:
                    break;
            }

            NoDirectionOscillatorLogic(iFirstBar, iPrvs, adFractalDim, dLevel, ref Component[1], indLogic);
            Component[2].Value = Component[1].Value;

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
                case "The FDI rises":
                    EntryFilterLongDescription  += "rises";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription   += "rises";
                    ExitFilterShortDescription  += "rises";
                    break;

                case "The FDI falls":
                    EntryFilterLongDescription  += "falls";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription   += "falls";
                    ExitFilterShortDescription  += "falls";
                    break;

                case "The FDI is higher than the Level line":
                    EntryFilterLongDescription  += "is higher than the Level " + sLevelLong;
                    EntryFilterShortDescription += "is higher than the Level " + sLevelShort;
                    ExitFilterLongDescription   += "is higher than the Level " + sLevelLong;
                    ExitFilterShortDescription  += "is higher than the Level " + sLevelShort;
                    break;

                case "The FDI is lower than the Level line":
                    EntryFilterLongDescription  += "is lower than the Level " + sLevelLong;
                    EntryFilterShortDescription += "is lower than the Level " + sLevelShort;
                    ExitFilterLongDescription   += "is lower than the Level " + sLevelLong;
                    ExitFilterShortDescription  += "is lower than the Level " + sLevelShort;
                    break;

                case "The FDI crosses the Level line upward":
                    EntryFilterLongDescription  += "crosses the Level " + sLevelLong  + " upward";
                    EntryFilterShortDescription += "crosses the Level " + sLevelShort + " upward";
                    ExitFilterLongDescription   += "crosses the Level " + sLevelLong  + " upward";
                    ExitFilterShortDescription  += "crosses the Level " + sLevelShort + " upward";
                    break;

                case "The FDI crosses the Level line downward":
                    EntryFilterLongDescription  += "crosses the Level " + sLevelLong  + " downward";
                    EntryFilterShortDescription += "crosses the Level " + sLevelShort + " downward";
                    ExitFilterLongDescription   += "crosses the Level " + sLevelLong  + " downward";
                    ExitFilterShortDescription  += "crosses the Level " + sLevelShort + " downward";
                    break;

                case "The FDI changes its direction upward":
                    EntryFilterLongDescription  += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription   += "changes its direction upward";
                    ExitFilterShortDescription  += "changes its direction upward";
                    break;

                case "The FDI changes its direction downward":
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
            string sString = IndicatorName +
                (IndParam.SlotType == SlotTypes.OpenFilter ? "* (" : " (") +
                IndParam.ListParam[1].Text         + ", " + // Smoothing method
                IndParam.ListParam[2].Text         + ", " + // Base price
                IndParam.NumParam[0].ValueToString + ")";   // FDI Period

            return sString;
        }
    }
}