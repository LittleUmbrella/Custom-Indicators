// Laguerre Indicator
// Last changed on 2009-12-29
// Part of Forex Strategy Builder
// Website http://forexsb.com/
// This code or any part of it cannot be used in other applications without a permission.
// Copyright (c) 2009 Miroslav Popov - All rights reserved.

using System;
using System.Drawing;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Laguerre
    /// </summary>
    public class Laguerre : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Laguerre(SlotTypes slotType)
        {
            // General properties
            IndicatorName  = "Laguerre";
            PossibleSlots  = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
            SeparatedChartMinValue = 0;
            SeparatedChartMaxValue = 1;

            CustomIndicator  = true; // This is a custom indicator
            IsDescreteValues = true; // This is becaus the indicator can be 0 or 1 for several consecutive bars.

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "The Laguerre rises",
                "The Laguerre falls",
                "The Laguerre is higher than the Level line",
                "The Laguerre is lower than the Level line",
                "The Laguerre crosses the Level line upward",
                "The Laguerre crosses the Level line downward",
                "The Laguerre changes its direction upward",
                "The Laguerre changes its direction downward"
            };
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of the indicator.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption   = "Gamma";
            IndParam.NumParam[0].Value     = 0.65;
            IndParam.NumParam[0].Min       = 0.50;
            IndParam.NumParam[0].Max       = 0.90;
            IndParam.NumParam[0].Point     = 2;
            IndParam.NumParam[0].Enabled   = true;
            IndParam.NumParam[0].ToolTip   = "A coefficient of the Laguerre line.";

            IndParam.NumParam[1].Caption   = "Level";
            IndParam.NumParam[1].Value     = 0.2;
            IndParam.NumParam[1].Min       = 0.0;
            IndParam.NumParam[1].Max       = 1.0;
            IndParam.NumParam[1].Point     = 2;
            IndParam.NumParam[1].Enabled   = true;
            IndParam.NumParam[1].ToolTip   = "A critical level (for the appropriate logic).";

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
            double dGamma = IndParam.NumParam[0].Value;
            double dLevel = IndParam.NumParam[1].Value;
            int    iPrvs  = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int      iFirstBar   = 2;
            double[] adLaguerre  = new double[Bars];

            double L0   = 0;
            double L1   = 0;
            double L2   = 0;
            double L3   = 0;
            double L0A  = 0;
            double L1A  = 0;
            double L2A  = 0;
            double L3A  = 0;
            double LRSI = 0;
            double CU   = 0;
            double CD   = 0;

            for (int iBar = iFirstBar; iBar < Bars; iBar++)
            {
                L0A = L0;
                L1A = L1;
                L2A = L2;
                L3A = L3;
                L0  = (1 - dGamma) * Close[iBar] + dGamma * L0A;
                L1  = -dGamma * L0 + L0A + dGamma * L1A;
                L2  = -dGamma * L1 + L1A + dGamma * L2A;
                L3  = -dGamma * L2 + L2A + dGamma * L3A;

                CU = 0;
                CD = 0;

                if (L0 >= L1)
                    CU = L0 - L1;
                else
                    CD = L1 - L0;

                if (L1 >= L2)
                    CU = CU + L1 - L2;
                else
                    CD = CD + L2 - L1;
                
                if (L2 >= L3)
                    CU = CU + L2 - L3;
                else
                    CD = CD + L3 - L2;

                if (CU + CD != 0)
                    LRSI = CU / (CU + CD);

                adLaguerre[iBar] = LRSI;
            }

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp();
            Component[0].CompName   = "Laguerre";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor = Color.RoyalBlue;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adLaguerre;

            Component[1] = new IndicatorComp();
            Component[1].ChartType  = IndChartType.NoChart;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = new double[Bars];

            Component[2] = new IndicatorComp();
            Component[2].ChartType  = IndChartType.NoChart;
            Component[2].FirstBar   = iFirstBar;
            Component[2].Value      = new double[Bars];

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
                case "The Laguerre rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
                    SpecialValues = new double[1] { 0.5 };
                    break;

                case "The Laguerre falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
                    SpecialValues = new double[1] { 0.5 };
                    break;

                case "The Laguerre is higher than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new double[2] { dLevel, 1 - dLevel };
                    break;

                case "The Laguerre is lower than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new double[2] { dLevel, 1 - dLevel };
                    break;

                case "The Laguerre crosses the Level line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new double[2] { dLevel, 1 - dLevel };
                    break;

                case "The Laguerre crosses the Level line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new double[2] { dLevel, 1 - dLevel };
                    break;

                case "The Laguerre changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    SpecialValues = new double[1] { 0.5 };
                    break;

                case "The Laguerre changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    SpecialValues = new double[1] { 0.5 };
                    break;

                default:
                    break;
            }

            OscillatorLogic(iFirstBar, iPrvs, adLaguerre, dLevel, 1 - dLevel, ref Component[1], ref Component[2], indLogic);

            return;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            string sLevelLong  = IndParam.NumParam[1].ValueToString;
            string sLevelShort = IndParam.NumParam[1].AnotherValueToString(1 - IndParam.NumParam[1].Value);

            EntryFilterLongDescription  = "the " + ToString() + " ";
            EntryFilterShortDescription = "the " + ToString() + " ";
            ExitFilterLongDescription   = "the " + ToString() + " ";
            ExitFilterShortDescription  = "the " + ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "The Laguerre rises":
                    EntryFilterLongDescription  += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription   += "rises";
                    ExitFilterShortDescription  += "falls";
                    break;

                case "The Laguerre falls":
                    EntryFilterLongDescription  += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription   += "falls";
                    ExitFilterShortDescription  += "rises";
                    break;

                case "The Laguerre is higher than the Level line":
                    EntryFilterLongDescription  += "is higher than the Level " + sLevelLong;
                    EntryFilterShortDescription += "is lower than the Level "  + sLevelShort;
                    ExitFilterLongDescription   += "is higher than the Level " + sLevelLong;
                    ExitFilterShortDescription  += "is lower than the Level "  + sLevelShort;
                    break;

                case "The Laguerre is lower than the Level line":
                    EntryFilterLongDescription  += "is lower than the Level "  + sLevelLong;
                    EntryFilterShortDescription += "is higher than the Level " + sLevelShort;
                    ExitFilterLongDescription   += "is lower than the Level "  + sLevelLong;
                    ExitFilterShortDescription  += "is higher than the Level " + sLevelShort;
                    break;

                case "The Laguerre crosses the Level line upward":
                    EntryFilterLongDescription  += "crosses the Level " + sLevelLong  + " upward";
                    EntryFilterShortDescription += "crosses the Level " + sLevelShort + " downward";
                    ExitFilterLongDescription   += "crosses the Level " + sLevelLong  + " upward";
                    ExitFilterShortDescription  += "crosses the Level " + sLevelShort + " downward";
                    break;

                case "The Laguerre crosses the Level line downward":
                    EntryFilterLongDescription  += "crosses the Level " + sLevelLong  + " downward";
                    EntryFilterShortDescription += "crosses the Level " + sLevelShort + " upward";
                    ExitFilterLongDescription   += "crosses the Level " + sLevelLong  + " downward";
                    ExitFilterShortDescription  += "crosses the Level " + sLevelShort + " upward";
                    break;

                case "The Laguerre changes its direction upward":
                    EntryFilterLongDescription  += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription   += "changes its direction upward";
                    ExitFilterShortDescription  += "changes its direction downward";
                    break;

                case "The Laguerre changes its direction downward":
                    EntryFilterLongDescription  += "changes its direction downward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription   += "changes its direction downward";
                    ExitFilterShortDescription  += "changes its direction upward";
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
                IndParam.NumParam[0].ValueToString + ")";   // Gamma

            return sString;
        }
    }
}
