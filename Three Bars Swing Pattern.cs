// Three Bars Swing indicator
// Last changed on 2012-04-12
// Part of Forex Strategy Builder
// Website http://forexsb.com/
// Copyright (c) 2012 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System.Drawing;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Three Bars Swing Indicator
    /// </summary>
    public class Three_Bars_Swing : Indicator
	{
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public  Three_Bars_Swing(SlotTypes slotType)
        {
            // General properties
            IndicatorName = "Three Bars Swing";
			CustomIndicator = true;
            PossibleSlots = SlotTypes.OpenFilter;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
			IndParam.ListParam[0].ItemList = new string[] { "Three bars swing pattern" };
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of the indicator.";

            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            // Reading the parameters
            int firstBar = 4;

            double[] longSignals = new double[Bars];
			double[] shortSignals = new double[Bars];

			for (int bar = firstBar; bar < Bars; bar++)
			{
				// Long trade
				if (Close[bar - 3] < Open[bar - 3] && // Candle 1 is black
					Low[bar - 2]   < Low[bar - 3]  && // Candle 2 has lower low than candle 1
					Close[bar - 1] > Open[bar - 1] && // Candle 3 is white
					Low[bar - 1]   > Low[bar - 2]  && // Candle 3 has higher low than candle 2
					Close[bar - 1] > Open[bar - 3])   // Candle 3 closes above candle 1 open 
					longSignals[bar] = 1;
					
				// Short trade
				if (Close[bar - 3] > Open[bar - 3] && // Candle 1 is white
					High[bar - 2]  > High[bar - 3] && // Candle 2 has higher high than candle 1
					Close[bar - 1] < Open[bar - 1] && // Candle 3 is black
					High[bar - 1]  < High[bar - 2] && // Candle 3 has lower high than candle 2
					Close[bar - 1] < Open[bar - 3])   // Candle 3 closes below candle 1 open
					shortSignals[bar] = 1;
			}

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp();
            Component[0].CompName  = "Allow long entry";
            Component[0].DataType  = IndComponentType.AllowOpenLong;
            Component[0].ChartType = IndChartType.NoChart;
            Component[0].FirstBar  = firstBar;
            Component[0].Value     = longSignals;

            Component[1] = new IndicatorComp();
            Component[1].CompName  = "Allow short entry";
            Component[1].DataType  = IndComponentType.AllowOpenShort;
            Component[1].ChartType = IndChartType.NoChart;
            Component[1].FirstBar  = firstBar;
            Component[1].Value     = shortSignals;

            return;
		}

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            EntryFilterLongDescription  = "there is a Three Bars Swing pattern";
            EntryFilterShortDescription = "there is a Three Bars Swing pattern";

            return;
        }

        /// <summary>
        /// Indicator to string
        /// </summary>
        public override string ToString()
        {
            return IndicatorName;
        }
    }
}