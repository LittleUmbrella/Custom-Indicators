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
    public class big : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public big(SlotTypes slotType)
        {
            // General properties
            IndicatorName   = "big";
            PossibleSlots   = SlotTypes.OpenFilter;
			CustomIndicator = true;

       /*// Setting up the indicator parameters
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
            */

            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Checked = PrepareUsePrevBarValueCheckBox(slotType);
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
            
            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new string[] { "Shadow V body" };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";


            IndParam.NumParam[0].Caption = "Percent";
            IndParam.NumParam[0].Value = 70;
            IndParam.NumParam[0].Min = 50;
            IndParam.NumParam[0].Max = 100;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The Rate of shadow candle against the body";

            IndParam.NumParam[1].Caption = "Count";
            IndParam.NumParam[1].Value = 1;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 6;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "No. candle";

            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            // Calculation

          int iPeriod = (int)IndParam.NumParam[1].Value;
            double p1 = (double)IndParam.NumParam[0].Value;
            p1 = p1 / 100;

            int iFirstBar = 1;
            
            double[] up1 = new double[Bars];
            double[] down1 = new double[Bars];
			double point = (Digits == 5 || Digits == 3) ? 10 * Point : Point;
		
         

            for (int iBar = 6; iBar < Bars; iBar++)
            {
                if (Math.Abs(Close[iBar - iPeriod] - Open[iBar - iPeriod]) >= (High[iBar - iPeriod] - Low[iBar - iPeriod]) * p1)
                {
                    if (Close[iBar - iPeriod] > Open[iBar - iPeriod])
                    up1[iBar] = 1;
                
                    else  
                     down1[iBar] = 1;
                }
            }

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp();
            Component[0].CompName  = "Allow long entry";
            Component[0].DataType  = IndComponentType.AllowOpenLong;
            Component[0].ChartType = IndChartType.NoChart;
            Component[0].FirstBar  = iFirstBar;
            Component[0].Value     =up1 ;

            Component[1] = new IndicatorComp();
            Component[1].CompName  = "Allow short entry";
            Component[1].DataType  = IndComponentType.AllowOpenShort;
            Component[1].ChartType = IndChartType.NoChart;
            Component[1].FirstBar  = iFirstBar;
            Component[1].Value     = down1;

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
