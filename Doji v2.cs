// Doji Indicator
// Last changed on 2010-09-20
// Part of Forex Strategy Builder & Forex Strategy Trader
// Website http://forexsb.com/
// Copyright (c) 2006 - 2010 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Doji Indicator
    /// </summary>
    public class Doji : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Doji(SlotTypes slotType)
        {
            // General properties
            IndicatorName   = "Doji";
            PossibleSlots   = SlotTypes.OpenFilter;
			CustomIndicator = true;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;
            IndParam.IndicatorType = TypeOfIndicator.Additional;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "There is a Doji formation",
            };
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Indicator's logic.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Checked = PrepareUsePrevBarValueCheckBox(slotType);
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
			
			// The NumericUpDown parameters
            IndParam.NumParam[0].Caption   = "PERCENT";
            IndParam.NumParam[0].Value     = 8;
            IndParam.NumParam[0].Min       = 1;
            IndParam.NumParam[0].Max       = 100;
            IndParam.NumParam[0].Enabled   = true;
            IndParam.NumParam[0].ToolTip   = "Percent body of the candle.";

            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
			// Reading the parameters
			int      iPERCENT   = (int)IndParam.NumParam[0].Value;
			
				
            // Calculation
            int iFirstBar = 1;
            double[] doji = new double[Bars];
			double point = (Digits == 5 || Digits == 3) ? 10 * Point : Point;


            for (int iBar = 1; iBar < Bars; iBar++)
            {
				if (Math.Abs(Close[iBar - 1] - Open[iBar - 1]) < iPERCENT * 0.01 * (High[iBar - 1] - Low[iBar - 1]))
				doji[iBar] = 1;
            }
			
			

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp();
            Component[0].CompName  = "Allow long entry";
            Component[0].DataType  = IndComponentType.AllowOpenLong;
            Component[0].ChartType = IndChartType.NoChart;
            Component[0].FirstBar  = iFirstBar;
            Component[0].Value     = doji;

            Component[1] = new IndicatorComp();
            Component[1].CompName  = "Allow short entry";
            Component[1].DataType  = IndComponentType.AllowOpenShort;
            Component[1].ChartType = IndChartType.NoChart;
            Component[1].FirstBar  = iFirstBar;
            Component[1].Value     = doji;

            return;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            EntryFilterLongDescription  = "there is a Doji formation";
            EntryFilterShortDescription = "there is a Doji formation";

            return;
        }

        /// <summary>
        /// Indicator to string
        /// </summary>
        public override string ToString()
        {
            string sString = IndicatorName;

            return sString;
        }
    }
}
