// Day of Week Indicator
// Last changed on 2009-05-05
// Part of Forex Strategy Builder v2.8.3.7+
// Website http://forexsb.com/
// Copyright (c) 2006 - 2009 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Day of Week Exit Indicator
    /// </summary>
    public class Day_of_Week_Exit : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Day_of_Week_Exit(SlotTypes slotType)
        {
            // General properties
            PossibleSlots = SlotTypes.CloseFilter;
            IndicatorName = "Day of Week Exit";
			CustomIndicator = true;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;
            IndParam.IndicatorType = TypeOfIndicator.DateTime;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "Exit the market between the specified day at the specified time"
            };
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Indicators' logic.";

            IndParam.ListParam[1].Caption  = "From (incl.)";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(DayOfWeek));
            IndParam.ListParam[1].Index    = (int)DayOfWeek.Friday;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "Day of beginning for the exit period.";

            IndParam.ListParam[2].Caption  = "To (excl.)";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof(DayOfWeek));
            IndParam.ListParam[2].Index    = (int)DayOfWeek.Saturday;
            IndParam.ListParam[2].Text     = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "End day for the exit period.";


            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "From hour (incl.)";
            IndParam.NumParam[0].Value   = 0;
            IndParam.NumParam[0].Min     = 0;
            IndParam.NumParam[0].Max     = 23;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "Beginning of the exit period.";

            IndParam.NumParam[1].Caption = "From min (incl.)";
            IndParam.NumParam[1].Value   = 0;
            IndParam.NumParam[1].Min     = 0;
            IndParam.NumParam[1].Max     = 59;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Beginning of the exit period.";

            IndParam.NumParam[2].Caption = "Until hour (excl.)";
            IndParam.NumParam[2].Value   = 24;
            IndParam.NumParam[2].Min     = 0;
            IndParam.NumParam[2].Max     = 24;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "End of the exit period.";

            IndParam.NumParam[3].Caption = "Until min( excl.)";
            IndParam.NumParam[3].Value   = 0;
            IndParam.NumParam[3].Min     = 0;
            IndParam.NumParam[3].Max     = 59;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "End of the exit period.";

            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            // Reading the parameters
            int iFromDay  = (int)IndParam.ListParam[1].Index;
            int iFromHour  = (int)IndParam.NumParam[0].Value;
            int iFromMin   = (int)IndParam.NumParam[1].Value;
			int iUntilDay  = (int)IndParam.ListParam[2].Index;
			int iUntilHour = (int)IndParam.NumParam[2].Value;
            int iUntilMin  = (int)IndParam.NumParam[3].Value;
            TimeSpan tsFromTime  = new TimeSpan(iFromDay, iFromHour, iFromMin, 0);
            TimeSpan tsUntilTime = new TimeSpan(iUntilDay, iUntilHour, iUntilMin, 0);



            // Calculation
            int iFirstBar = 1;
            double[] adBars = new double[Bars];

            // Calculation of the logic
            for (int iBar = iFirstBar; iBar < Bars; iBar++)
            {
				TimeSpan tsBar = new TimeSpan((int)Date[iBar].DayOfWeek, Date[iBar].Hour, Date[iBar].Minute, 0);
                 adBars[iBar] = tsBar >= tsFromTime &&
								   tsBar < tsUntilTime      ? 1 : 0;
                
            }

            // Saving the components
            Component = new IndicatorComp[1];

            Component[0] = new IndicatorComp();
            Component[0].CompName      = "Force close position";
            Component[0].DataType      = IndComponentType.ForceClose;
            Component[0].ChartType     = IndChartType.NoChart;
            Component[0].ShowInDynInfo = false;
            Component[0].FirstBar      = iFirstBar;
            Component[0].Value         = adBars;


            return;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            DayOfWeek dowFromDay  = (DayOfWeek)IndParam.ListParam[1].Index;
            int iFromHour  = (int)IndParam.NumParam[0].Value;
            int iFromMin  = (int)IndParam.NumParam[1].Value;
			string sFromMin = (iFromMin < 10) ? "0" + iFromMin.ToString() : iFromMin.ToString();
			DayOfWeek dowUntilDay  = (DayOfWeek)IndParam.ListParam[2].Index;
            int iUntilHour = (int)IndParam.NumParam[2].Value;
            int iUntilMin  = (int)IndParam.NumParam[3].Value;
			string sUntilMin = (iUntilMin < 10) ? "0" + iUntilMin.ToString() : iUntilMin.ToString();

            ExitFilterLongDescription  = "between " + dowFromDay + " " + iFromHour.ToString() + ":" + sFromMin + " and " + dowUntilDay + " " + iUntilHour.ToString() + ":" + sUntilMin;
            ExitFilterShortDescription = "between " + dowFromDay + " " + iFromHour.ToString() + ":" + sFromMin + " and " + dowUntilDay + " " + iUntilHour.ToString() + ":" + sUntilMin;

            return;
        }

        /// <summary>
        /// Indicator to string
        /// </summary>
        public override string ToString()
        {
            DayOfWeek dowFromDay  = (DayOfWeek)IndParam.ListParam[1].Index;
            int iFromHour  = (int)IndParam.NumParam[0].Value;
            int iFromMin  = (int)IndParam.NumParam[1].Value;
			string sFromMin = (iFromMin < 10) ? "0" + iFromMin.ToString() : iFromMin.ToString();
			DayOfWeek dowUntilDay  = (DayOfWeek)IndParam.ListParam[2].Index;
            int iUntilHour = (int)IndParam.NumParam[2].Value;
            int iUntilMin  = (int)IndParam.NumParam[3].Value;
			string sUntilMin = (iUntilMin < 10) ? "0" + iUntilMin.ToString() : iUntilMin.ToString();

            string sString = IndicatorName + " (" +
                dowFromDay  + " " + // Day
                iFromHour.ToString() + ":" +   // Hour
				sFromMin + "-" +   // Minute
				dowUntilDay  + " " + // Day
				iUntilHour.ToString() + ":" +   // Hour
				sUntilMin + ")";  // Minute

            return sString;
        }
    }
}
