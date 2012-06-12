// Gap indicator
// Last changed on 2012-04-16
// Part of Forex Strategy Builder
// Website http://forexsb.com/
// Copyright (c) 2012 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System.Drawing;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Gap Indicator
    /// </summary>
    public class Gap : Indicator
	{
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public  Gap(SlotTypes slotType)
        {
            // General properties
            IndicatorName = "Gap";
			CustomIndicator = true;
            PossibleSlots = SlotTypes.OpenFilter;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
			IndParam.ListParam[0].ItemList = new string[] { "Positive gap", "Negative gap" };
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of the indicator.";

            IndParam.NumParam[0].Caption   = "Minimum Gap";
            IndParam.NumParam[0].Value     = 20;
            IndParam.NumParam[0].Min       = 1;
            IndParam.NumParam[0].Max       = 2000;
            IndParam.NumParam[0].Enabled   = true;
            IndParam.NumParam[0].ToolTip   = "Gap in points.";

            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            // Reading the parameters
			double minGap = Point * IndParam.NumParam[0].Value;
			
			// Calculating
            int firstBar = 1;

            double[] longSignals = new double[Bars];
			double[] shortSignals = new double[Bars];

			if (IndParam.ListParam[0].Text == "Positive gap")
				for (int bar = firstBar; bar < Bars; bar++)
				{
					if (Open[bar] > Close[bar - 1] + minGap) 
						longSignals[bar] = 1;
					if (Open[bar] < Close[bar - 1] - minGap) 
						shortSignals[bar] = 1;
				}
				
			if (IndParam.ListParam[0].Text == "Negative gap")
				for (int bar = firstBar; bar < Bars; bar++)
				{
					if (Open[bar] < Close[bar - 1] - minGap) 
						longSignals[bar] = 1;
					if (Open[bar] > Close[bar - 1] + minGap) 
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
			if (IndParam.ListParam[0].Text == "Positive gap")
            {
				EntryFilterLongDescription  = "there is a positive gap";
				EntryFilterShortDescription = "there is a negative gap";
			}
			if (IndParam.ListParam[0].Text == "Negative gap")
            {
				EntryFilterLongDescription  = "there is a negative gap";
				EntryFilterShortDescription = "there is a positive gap";
			}

            return;
        }

        /// <summary>
        /// Indicator to string
        /// </summary>
        public override string ToString()
        {
            return IndicatorName + 
				"( " + IndParam.NumParam[0].Value + // Min Gap
				")";
        }
    }
}