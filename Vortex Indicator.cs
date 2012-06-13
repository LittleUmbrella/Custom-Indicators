// Vortex Indicator V1.0.0
// Last changed on 20 Feb 2010
// Part of Forex Strategy Builder v2.8.3.8+
// Website http://forexsb.com/
// Copyright (c) 2010 Denny Imanuel - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;
using System.Drawing;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Vortex Indicators
    /// </summary>
    public class Vortex_Indicators : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Vortex_Indicators(SlotTypes slotType)
        {
            // General properties
            IndicatorName  = "Vortex Indicators";
            PossibleSlots  = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
            // SeparatedChartMinValue = 0;
			CustomIndicator = true;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption   = "Logic";
            IndParam.ListParam[0].ItemList  = new string[]
            {
                "The VI+ rises",
                "The VI+ falls",
                "The VI- rises",
                "The VI- falls",
                "The VI+ is higher than VI-",
                "The VI+ is lower than VI-",
                "The VI+ crosses the VI- line upward",
                "The VI+ crosses the VI- line downward",
                "The VI+ changes its direction upward",
                "The VI+ changes its direction downward",
                "The VI- changes its direction upward",
                "The VI- changes its direction downward"
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
            IndParam.ListParam[1].ToolTip  = "The Moving Average method used for VI smoothing.";

            IndParam.ListParam[2].Caption  = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[2].Index    = (int)BasePrice.Close;
            IndParam.ListParam[2].Text     = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "The price the RSI is based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Period";
            IndParam.NumParam[0].Value   = 14;
            IndParam.NumParam[0].Min     = 5;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of VI.";

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
            int iNVI = (int)IndParam.NumParam[0].Value;
            int iPrvs = (slotType == SlotTypes.OpenFilter) ? 1 : 0;

            // Calculation
            double[] adBasePrice = Price(basePrice);
            int iFirstBar = iNVI + 2;

            double[] adVIPos = new double[Bars];
            double[] adVINeg = new double[Bars];
            double[] adVIOsc = new double[Bars];
            double[] adVMPlus  = new double[Bars];
            double[] adVMMinus  = new double[Bars];
			double[] adVMPlusSum  = new double[Bars];
            double[] adVMMinusSum  = new double[Bars];
			double[] adTrueRange  = new double[Bars];
            double[] adTrueHigh  = new double[Bars];
			double[] adTrueLow  = new double[Bars];
			double[] adTRSum  = new double[Bars];
			
			for (int iBar=iFirstBar; iBar<Bars; iBar++)
			{
				adVMPlus[iBar] 	  = Math.Abs(High[iBar]-Low[iBar-1]);
				adVMMinus[iBar]   = Math.Abs(Low[iBar] -High[iBar-1]);
				if (Close[iBar-1]>High[iBar]) adTrueHigh[iBar]=Close[iBar-1]; else adTrueHigh[iBar]=High[iBar];
				if (Close[iBar-1]<Low[iBar])  adTrueLow[iBar]=Close[iBar-1];  else adTrueLow[iBar]=Low[iBar];
				adTrueRange[iBar] = adTrueHigh[iBar] - adTrueLow[iBar];
				adVMPlusSum[iBar]  = 0;
				adVMMinusSum[iBar] = 0;
				adTRSum[iBar] = 0;
				for (int iBack=0; iBack<iNVI; iBack++)
				{
					adVMPlusSum[iBar]  += adVMPlus[iBar-iBack];
					adVMMinusSum[iBar] += adVMMinus[iBar-iBack];
					adTRSum[iBar] 	   += adTrueRange[iBar-iBack];					
				}
				if (adTRSum[iBar]!=0)
				adVIPos[iBar] = adVMPlusSum[iBar]/adTRSum[iBar];
				adVINeg[iBar] = adVMMinusSum[iBar]/adTRSum[iBar];
			}
			
            for (int iBar = 0; iBar < Bars; iBar++)
                adVIOsc[iBar] = adVIPos[iBar] - adVINeg[iBar];

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp();
            Component[0].CompName   = "The VI+";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor = Color.Green;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adVIPos;

            Component[1] = new IndicatorComp();
            Component[1].CompName   = "The VI-";
            Component[1].DataType   = IndComponentType.IndicatorValue;
            Component[1].ChartType  = IndChartType.Line;
            Component[1].ChartColor = Color.Red;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = adVINeg;

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

            switch (IndParam.ListParam[0].Text)
            {
                case "The VI+ rises":
                    OscillatorLogic(iFirstBar, iPrvs, adVIPos, 0, 0, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_rises);
                    break;

                case "The VI+ falls":
                    OscillatorLogic(iFirstBar, iPrvs, adVIPos, 0, 0, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_falls);
                    break;

                case "The VI- rises":
                    OscillatorLogic(iFirstBar, iPrvs, adVINeg, 0, 0, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_rises);
                    break;

                case "The VI- falls":
                    OscillatorLogic(iFirstBar, iPrvs, adVINeg, 0, 0, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_falls);
                    break;

                case "The VI+ is higher than VI-":
                    OscillatorLogic(iFirstBar, iPrvs, adVIOsc, 0, 0, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_is_higher_than_the_level_line);
                    break;

                case "The VI+ is lower than VI-":
                    OscillatorLogic(iFirstBar, iPrvs, adVIOsc, 0, 0, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_is_lower_than_the_level_line);
                    break;

                case "The VI+ crosses the VI- line upward":
                    OscillatorLogic(iFirstBar, iPrvs, adVIOsc, 0, 0, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_crosses_the_level_line_upward);
                    break;

                case "The VI+ crosses the VI- line downward":
                    OscillatorLogic(iFirstBar, iPrvs, adVIOsc, 0, 0, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_crosses_the_level_line_downward);
                    break;

                case "The VI+ changes its direction upward":
                    OscillatorLogic(iFirstBar, iPrvs, adVIPos, 0, 0, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_changes_its_direction_upward);
                    break;

                case "The VI+ changes its direction downward":
                    OscillatorLogic(iFirstBar, iPrvs, adVIPos, 0, 0, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_changes_its_direction_downward);
                    break;

                case "The VI- changes its direction upward":
                    OscillatorLogic(iFirstBar, iPrvs, adVINeg, 0, 0, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_changes_its_direction_upward);
                    break;

                case "The VI- changes its direction downward":
                    OscillatorLogic(iFirstBar, iPrvs, adVINeg, 0, 0, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_changes_its_direction_downward);
                    break;

                default:
                    break;
            }

            return;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            EntryFilterLongDescription  = ToString() + "; ";
            EntryFilterShortDescription = ToString() + "; ";
            ExitFilterLongDescription   = ToString() + "; ";
            ExitFilterShortDescription  = ToString() + "; ";

            switch (IndParam.ListParam[0].Text)
            {
                case "The VI+ rises":
                    EntryFilterLongDescription  += "the VI+ rises";
                    EntryFilterShortDescription += "the VI+ falls";
                    ExitFilterLongDescription   += "the VI+ rises";
                    ExitFilterShortDescription  += "the VI+ falls";
                    break;

                case "The VI+ falls":
                    EntryFilterLongDescription  += "the VI+ falls";
                    EntryFilterShortDescription += "the VI+ rises";
                    ExitFilterLongDescription   += "the VI+ falls";
                    ExitFilterShortDescription  += "the VI+ rises";
                    break;

                case "The VI- rises":
                    EntryFilterLongDescription  += "the VI- rises";
                    EntryFilterShortDescription += "the VI- falls";
                    ExitFilterLongDescription   += "the VI- rises";
                    ExitFilterShortDescription  += "the VI- falls";
                    break;

                case "The VI- falls":
                    EntryFilterLongDescription  += "the VI- falls";
                    EntryFilterShortDescription += "the VI- rises";
                    ExitFilterLongDescription   += "the VI- falls";
                    ExitFilterShortDescription  += "the VI- rises";
                    break;

                case "The VI+ is higher than VI-":
                    EntryFilterLongDescription  += "the VI+ is higher than the VI-";
                    EntryFilterShortDescription += "the VI+ is lower than the VI-";
                    ExitFilterLongDescription   += "the VI+ is higher than the VI-";
                    ExitFilterShortDescription  += "the VI+ is lower than the VI-";
                    break;

                case "The VI+ is lower than VI-":
                    EntryFilterLongDescription  += "the VI+ is lower than the VI-";
                    EntryFilterShortDescription += "the VI+ is higher than the VI-";
                    ExitFilterLongDescription   += "the VI+ is lower than the VI-";
                    ExitFilterShortDescription  += "the VI+ is higher than the VI-";
                    break;

                case "The VI+ crosses the VI- line upward":
                    EntryFilterLongDescription  += "the VI+ crosses the VI- line upward";
                    EntryFilterShortDescription += "the VI+ crosses the VI- line downward";
                    ExitFilterLongDescription   += "the VI+ crosses the VI- line upward";
                    ExitFilterShortDescription  += "the VI+ crosses the VI- line downward";
                    break;

                case "The VI+ crosses the VI- line downward":
                    EntryFilterLongDescription  += "the VI+ crosses the VI- line downward";
                    EntryFilterShortDescription += "the VI+ crosses the VI- line upward";
                    ExitFilterLongDescription   += "the VI+ crosses the VI- line downward";
                    ExitFilterShortDescription  += "the VI+ crosses the VI- line upward";
                    break;

                case "The VI+ changes its direction upward":
                    EntryFilterLongDescription  += "the VI+ changes its direction upward";
                    EntryFilterShortDescription += "the VI+ changes its direction downward";
                    ExitFilterLongDescription   += "the VI+ changes its direction upward";
                    ExitFilterShortDescription  += "the VI+ changes its direction downward";
                    break;

                case "The VI+ changes its direction downward":
                    EntryFilterLongDescription  += "the VI+ changes its direction downward";
                    EntryFilterShortDescription += "the VI+ changes its direction upward";
                    ExitFilterLongDescription   += "the VI+ changes its direction downward";
                    ExitFilterShortDescription  += "the VI+ changes its direction upward";
                    break;

                case "The VI- changes its direction upward":
                    EntryFilterLongDescription  += "the VI- changes its direction upward";
                    EntryFilterShortDescription += "the VI- changes its direction downward";
                    ExitFilterLongDescription   += "the VI- changes its direction upward";
                    ExitFilterShortDescription  += "the VI- changes its direction downward";
                    break;

                case "The VI- changes its direction downward":
                    EntryFilterLongDescription  += "the VI- changes its direction downward";
                    EntryFilterShortDescription += "the VI- changes its direction upward";
                    ExitFilterLongDescription   += "the VI- changes its direction downward";
                    ExitFilterShortDescription  += "the VI- changes its direction upward";
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
                IndParam.ListParam[1].Text         + ", " + // Method
                IndParam.ListParam[2].Text         + ", " + // Base price
                IndParam.NumParam[0].ValueToString + ")";   // Period

            return sString;
        }
	}
}