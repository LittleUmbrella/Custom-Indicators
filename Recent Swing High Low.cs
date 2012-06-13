// Recent Swing High Low indicator v1.51
// Part of Forex Strategy Builder
// Website http://forexsb.com/
// Copyright (c) 2010 Mauricio Peña - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;
using System.Drawing;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Recent High Low Indicator Indicator
    /// </summary>
    public class Recent_Swing_High_Low : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Recent_Swing_High_Low(SlotTypes slotType)
        {
            // General properties
            IndicatorName = "Recent Swing High Low";
            PossibleSlots = SlotTypes.Open | SlotTypes.Close;
            CustomIndicator = true;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            if (slotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Enter long at the most recent swing high",
                    "Enter long at the most recent swing low"
                };
            else if (slotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Exit long at the most recent swing high",
                    "Exit long at the most recent swing low"
                };
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption  = "Base price";
            IndParam.ListParam[1].ItemList = new string[] { "High and Low" };
            IndParam.ListParam[1].Index    = 0;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "Used price from the indicator.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Vertical shift";
            IndParam.NumParam[0].Value   = 0;
            IndParam.NumParam[0].Min     = -200;
            IndParam.NumParam[0].Max     = +200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "A vertical shift above the swing high and below the swing low price.";

            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            double dShift = IndParam.NumParam[0].Value * Point;
            int iFirstBar = 7;

            // Calculation
            double[] adHighPrice = new double[Bars];
            double[] adLowPrice  = new double[Bars];

            for (int iBar = iFirstBar; iBar < Bars; iBar++)
            {
                // Check if current high is a swing high
                if (High[iBar - 3] >= High[iBar - 4] && High[iBar - 3] > High[iBar - 5] &&  // Check 2 candles to the left
                    High[iBar - 3] >= High[iBar - 2] && High[iBar - 3] > High[iBar - 1])    // Check 2 candles to the right
                {
                    adHighPrice[iBar] = High[iBar - 3];
                }
                else
                {
                    adHighPrice[iBar] = adHighPrice[iBar - 1];
                }
                // Check if current low is a swing low
                if (Low[iBar - 3] <= Low[iBar - 4] && Low[iBar - 3] < Low[iBar - 5] &&  // Check 2 candles to the left
                    Low[iBar - 3] <= Low[iBar - 2] && Low[iBar - 3] < Low[iBar - 1])    // Check 2 candles to the right
                {
                    adLowPrice[iBar] = Low[iBar - 3];
                }
                else
                {
                    adLowPrice[iBar] = adLowPrice[iBar - 1];
                }
            }
            // Shifting the price
            double[] adUpperBand = new double[Bars];
            double[] adLowerBand = new double[Bars];
            for (int iBar = 1; iBar < Bars; iBar++)
            {
                adUpperBand[iBar] = adHighPrice[iBar] + dShift;
                adLowerBand[iBar] = adLowPrice[iBar]  - dShift;
            }

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0]  = new IndicatorComp();
            Component[0].CompName   = "Swing High";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Level;
            Component[0].ChartColor = Color.DarkGreen;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adHighPrice;

            Component[1] = new IndicatorComp();
            Component[1].CompName   = "Swing Low";
            Component[1].DataType   = IndComponentType.IndicatorValue;
            Component[1].ChartType  = IndChartType.Level;
            Component[1].ChartColor = Color.DarkRed;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = adLowPrice;

            Component[2] = new IndicatorComp();
            Component[2].ChartType = IndChartType.NoChart;
            Component[2].FirstBar  = iFirstBar;
            Component[2].Value     = new double[Bars];

            Component[3] = new IndicatorComp();
            Component[3].ChartType = IndChartType.NoChart;
            Component[3].FirstBar  = iFirstBar;
            Component[3].Value     = new double[Bars];

            // Sets the Component's type
            if (slotType == SlotTypes.Open)
            {
                Component[2].CompName = "Long position entry price";
                Component[2].DataType = IndComponentType.OpenLongPrice;
                Component[3].CompName = "Short position entry price";
                Component[3].DataType = IndComponentType.OpenShortPrice;
            }
            else if (slotType == SlotTypes.Close)
            {
                Component[2].CompName = "Long position closing price";
                Component[2].DataType = IndComponentType.CloseLongPrice;
                Component[3].CompName = "Short position closing price";
                Component[3].DataType = IndComponentType.CloseShortPrice;
            }

            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at the most recent swing high":
                case "Exit long at the most recent swing high":
                    Component[2].Value = adUpperBand;
                    Component[3].Value = adLowerBand;
                    break;
                case "Enter long at the most recent swing low":
                case "Exit long at the most recent swing low":
                    Component[2].Value = adLowerBand;
                    Component[3].Value = adUpperBand;
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
            int iShift = (int)IndParam.NumParam[0].Value;

            string sUpperTrade = null;
            string sLowerTrade = null;

            if (iShift > 0)
            {
                sUpperTrade = iShift + " pips above the ";
                sLowerTrade = iShift + " pips below the ";
            }
            else if (iShift == 0)
            {
                if (IndParam.ListParam[0].Text == "Enter long at the most recent swing high" ||
                    IndParam.ListParam[0].Text == "Enter long at the most recent swing low"  ||
                    IndParam.ListParam[0].Text == "Exit long at the most recent swing high"  ||
                    IndParam.ListParam[0].Text == "Exit long at the most recent swing low")
                {
                    sUpperTrade = "at the ";
                    sLowerTrade = "at the ";
                }
            }
            else
            {
                sUpperTrade = -iShift + " pips below the ";
                sLowerTrade = -iShift + " pips above the ";
            }

            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at the most recent swing high":
                    EntryPointLongDescription  = sUpperTrade + "most recent swing high";
                    EntryPointShortDescription = sLowerTrade + "most recent swing low";
                    break;
                case "Enter long at the most recent swing low":
                    EntryPointLongDescription  = sLowerTrade + "most recent swing low";
                    EntryPointShortDescription = sUpperTrade + "most recent swing high";
                    break;
                case "Exit long at the most recent swing high":
                    ExitPointLongDescription  = sUpperTrade + "most recent swing high";
                    ExitPointShortDescription = sLowerTrade + "most recent swing low";
                    break;
                case "Exit long at the most recent swing low":
                    ExitPointLongDescription  = sLowerTrade + "most recent swing low";
                    ExitPointShortDescription = sUpperTrade + "most recent swing high";
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
            string sString = IndicatorName +" (" +
                IndParam.NumParam[0].Value.ToString() + ")";   // Shift

            return sString;
        }
    }
}
