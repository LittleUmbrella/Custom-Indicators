// Spread Limit Indicator
// Last changed on 2010-09-20
// Part of Forex Strategy Builder & Forex Strategy Trader
// Website http://forexsb.com/
// Copyright (c) 2006 - 2010 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;

namespace Forex_Strategy_Trader
{
    /// <summary>
    /// Spread Limit Indicator
    /// </summary>
    public class Spread_Limiter : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Spread_Limiter(SlotTypes slotType)
        {
            // General properties
            IndicatorName   = "Spread Limiter";
            PossibleSlots   = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
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
                "Enter if spread is <= than ...",
				"Enter if spread is >= than ...",
				"Close if spread is <= than ...",
				"Close if spread is >= than ..."
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
            IndParam.NumParam[0].Caption   = "Spread";
            IndParam.NumParam[0].Value     = 2;
            IndParam.NumParam[0].Min       = 0.1;
            IndParam.NumParam[0].Max       = 100;
			IndParam.NumParam[0].Point     = 1;
            IndParam.NumParam[0].Enabled   = true;
            IndParam.NumParam[0].ToolTip   = "Spread in pips";

            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
			// Reading the parameters
			int      iPip   = (int)IndParam.NumParam[0].Value;
			
				
            // Calculation
            int iFirstBar = 1;
            double[] spr = new double[Bars];
			double[] showspread = new double[Bars];
			double point = (Digits == 5 || Digits == 3) ? 10 * Point : Point;
			double bid = Data.Bid;
			double ask = Data.Ask;
			double correctpoint = iPip * point;
			double spread = ask - bid;
			


            for (int iBar = 1; iBar < Bars; iBar++)
            {
				
				showspread[iBar] = spread / point;
            
			
			if (IndParam.ListParam[0].Text == "Enter if spread is <= than ...")
			{
				if (spread <= correctpoint)
				spr[iBar] = 1;
			}
			if (IndParam.ListParam[0].Text == "Enter if spread is >= than ...")
			{
				if (spread >= correctpoint)
				spr[iBar] = 1;
			}
			if (IndParam.ListParam[0].Text == "Close if spread is <= than ...")
			{
				if (spread <= correctpoint)
				spr[iBar] = 1;
			}
			if (IndParam.ListParam[0].Text == "Close if spread is >= than ...")
			{
				if (spread <= correctpoint)
				spr[iBar] = 1;
			}
			}
			
			

            // Saving the components
            Component = new IndicatorComp[3];
			
			if (slotType == SlotTypes.OpenFilter)
            {
                Component[0] = new IndicatorComp();
				Component[0].CompName  = "Allow entry";
				Component[0].DataType  = IndComponentType.AllowOpenLong;
				Component[0].ChartType = IndChartType.NoChart;
				Component[0].FirstBar  = iFirstBar;
				Component[0].Value     = spr;
				
				Component[1] = new IndicatorComp();
				Component[1].CompName  = "Allow entry";
				Component[1].DataType  = IndComponentType.AllowOpenShort;
				Component[1].ChartType = IndChartType.NoChart;
				Component[1].FirstBar  = iFirstBar;
				Component[1].Value     = spr;
            }
            else if (slotType == SlotTypes.CloseFilter)
            {
                Component[1] = new IndicatorComp();
				Component[1].CompName  = "Force close";
				Component[1].DataType  = IndComponentType.ForceClose;
				Component[1].ChartType = IndChartType.NoChart;
				Component[1].FirstBar  = iFirstBar;
				Component[1].Value     = spr;
            }
			
			Component[2] = new IndicatorComp();
            Component[2].CompName   = "Spread";
            //Component[2].ChartColor = Color.Goldenrod;
            Component[2].DataType   = IndComponentType.IndicatorValue;
            Component[2].ChartType  = IndChartType.NoChart;
            Component[2].FirstBar   = iFirstBar;
            Component[2].Value      = showspread;

            return;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            EntryFilterLongDescription  = "Enter if spread is <= than";
            EntryFilterShortDescription = "Enter if spread is <= than";
			ExitFilterLongDescription   = "Close if spread is <= than ...";
            ExitFilterShortDescription  = "Close if spread is <= than ...";

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
