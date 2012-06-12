// Entry Time indicator
// Forex Strategy Builder v2.8.1.1
// Copyright (c) 2006 - 2009 Miroslav Popov - All rights reserved!
// Last changed on: 2009-04-15
// http://forexsb.com

using System;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Entry Interval indicator
	/// to exit at some interval of the hour (ex, every 15 minutes)
    /// </summary>
    public class Exit_Interval : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Exit_Interval(SlotTypes slotType)
        {
            // General properties
            IndicatorName = "Exit Interval";
            PossibleSlots = SlotTypes.CloseFilter;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;
            IndParam.IndicatorType = TypeOfIndicator.DateTime;
			IndParam.ExecutionTime = ExecutionTime.AtBarClosing;
			CustomIndicator = true;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "Exit the market at the specified interval."
            };
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Indicator's logic.";

            // The NumericUpDown parameters
 			IndParam.ListParam[1].Caption = "Interval Period";
			IndParam.ListParam[1].ItemList = new string[] { 
					"5 Minutes", 
					"10 Minutes",
					"15 Minutes",
					"30 Minutes", 
					"1 Hour",
					"2 Hours",
					"3 Hours",
					"4 Hours",
					"8 Hours",
					"12 Hours"
				};
			IndParam.ListParam[1].Index    = 9;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "Choose interval in bars at which to exit.\nIf lower than your data time frame it will do nothing.";		

            IndParam.NumParam[1].Caption = "Offset";
            IndParam.NumParam[1].Value   = 0;
            IndParam.NumParam[1].Min     = 0;
            IndParam.NumParam[1].Max     = 500;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Offset the interval by this many bars.\nTry to choose a number of bars below the interval.";

            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            // Reading the parameters
			TimeSpan ts = new TimeSpan();
			switch (IndParam.ListParam[1].Text)
			{
				case "5 Minutes":
					ts = TimeSpan.FromMinutes (5);
					break;
				case  "10 Minutes":
					ts = TimeSpan.FromMinutes (10);
					break;
				case  "15 Minutes":
					ts = TimeSpan.FromMinutes (15);
					break;
				case "30 Minutes":
					ts = TimeSpan.FromMinutes (30);
					break;
				case "1 Hour":
					ts = TimeSpan.FromHours (1);
					break;
				case "2 Hours":
					ts = TimeSpan.FromHours (2);
					break;
				case "3 Hours":
					ts = TimeSpan.FromHours (3);
					break;
				case "4 Hours":
					ts = TimeSpan.FromHours (4);
					break;
				case "8 Hours":
					ts = TimeSpan.FromHours (8);
					break;
				case "12 Hours":
					ts = TimeSpan.FromHours (12);
					break;
				default:
					// if nothing works, then return to fail silently, used for start up
					return;
			}

			// if time frame is lower than interval, then return and do nothing
			if (ts.TotalMinutes <= (int)Period)
			{
				return;
			}

			double dOffset = IndParam.NumParam[1].Value * (double)Period;


            // Calculation
            double[] adBars = new double[Bars];

			int iFirstBar = 10;

			DateTime dtStart = new DateTime (Date[0].Year, Date[0].Month, Date[0].Day, 0, 0, 0);
			// init so it's one bar back before target time, since indicator executes at Bar Closing
			dtStart = dtStart.AddMinutes(-(double)Period);
			// increment by number of bars to offset
			dtStart = dtStart.AddMinutes(dOffset);

            // Calculation of the logic
            for (int iBar = iFirstBar; iBar < Bars; iBar++)
            {
                if((Date[iBar]-dtStart).Ticks % ts.Ticks == 0)
					adBars[iBar] = 1;
				else
                    adBars[iBar] = 0;
            }

            // Saving the components
            Component = new IndicatorComp[1];

            Component[0] = new IndicatorComp();
            Component[0].CompName      = "Force Exit";
            Component[0].DataType      = IndComponentType.ForceClose;
            Component[0].ChartType     = IndChartType.NoChart;
            Component[0].ShowInDynInfo = true;
            Component[0].FirstBar      = iFirstBar;
            Component[0].Value         = adBars;

            return;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
			string sInterval = IndParam.ListParam[1].Text;
			string sOffset =  IndParam.NumParam[1].Value.ToString();

            ExitFilterLongDescription  = "at the interval of " + sInterval + " with offset of " + sOffset + " bars"; 
            ExitFilterShortDescription = "at the interval of " + sInterval + " with offset of " + sOffset + " bars";

            return;
        }

        /// <summary>
        /// Indicator to string
        /// </summary>
        public override string ToString()
        {
			string sInterval = IndParam.ListParam[1].Text;
			string sOffset =  IndParam.NumParam[1].Value.ToString();

			string sString = IndicatorName + " (" +
                sInterval + ", " +   // Interval
				"off=" + sOffset + ")";   // Offset

            return sString;
        }
    }
}
