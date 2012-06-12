//	Bar Range Price Spread Indicator

//	Uses the price spread of a series of bars to determine an entry point

//  (No calculation or optimization will take place if the End Bar Setting is Lower than the Start Bar Setting)

//	Developed by M J Headford incorporating suggested changes from footon and krog

using System;


namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Bar Range Price Spread Indicator
    /// </summary>
    public class Bar_Range_Price_Spread : Indicator
	{
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Bar_Range_Price_Spread(SlotTypes slotType)
        {
            // General properties
            IndicatorName = "Bar Range Price Spread";
			CustomIndicator = true;
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;
            

            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
			IndParam.ListParam[0].ItemList = new string[] { "Enter the Market after the Set Bar Range and Price Spread" };
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of the indicator.";
			
			// The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "End Bar (> Start Bar)";
            IndParam.NumParam[0].Value   = 15;
            IndParam.NumParam[0].Min = 2;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The number of bars to calculate the Price Range.";
			
			IndParam.NumParam[1].Caption = "Price Spread";
            IndParam.NumParam[1].Value   = 200;
            IndParam.NumParam[1].Min     = 20;
            IndParam.NumParam[1].Max     = 2000;
			IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The settings of the price spread in decimal";
            
            IndParam.NumParam[2].Caption = "Start Bar (< End Bar)";
            IndParam.NumParam[2].Value = 1;
            IndParam.NumParam[2].Min = 1;
            IndParam.NumParam[2].Max = 199;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The start number of the bar to calculate the Price Range from.";
            			
 
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
			int    nBars  = (int)IndParam.NumParam[0].Value;
			double price = IndParam.NumParam[1].Value;
            int start = (int)IndParam.NumParam[2].Value;
            int iFirstBar = nBars + start + 1;

            double[] golong = new double[Bars];
			double[] goshort = new double[Bars];
			double	spread = price * Point;

            if (start >= nBars)
            {
                return;
            }
            
            

			for (int iBar = iFirstBar; iBar < Bars; iBar++)
			{
				// Long trade
				if ((Open[iBar - nBars] - Close[iBar - start]) > (spread)) //Calculates the bar range price spread and compares it with the set value
					
					golong[iBar] = 1;
					
													
				// Short trade
                if ((Close[iBar - start] - Open[iBar - nBars]) > (spread)) //Calculates the bar range price spread and compares it with the set value
										
					goshort[iBar] = 1;
					
							
				
			}

            // Saving the components
            Component = new IndicatorComp[2];

            Component[0] = new IndicatorComp();
            Component[0].CompName  = "Allow long entry";
            Component[0].DataType  = IndComponentType.AllowOpenLong;
            Component[0].ChartType = IndChartType.NoChart;
            Component[0].FirstBar  = iFirstBar;
            Component[0].Value     = golong;

            Component[1] = new IndicatorComp();
            Component[1].CompName  = "Allow short entry";
            Component[1].DataType  = IndComponentType.AllowOpenShort;
            Component[1].ChartType = IndChartType.NoChart;
            Component[1].FirstBar  = iFirstBar;
            Component[1].Value     = goshort;
			
			
			
			// Sets the Component's Type
			if (slotType == SlotTypes.OpenFilter)
			{
			
			Component[0].DataType = IndComponentType.AllowOpenLong;
			Component[0].CompName = "Is long entry allowed";
			Component[1].DataType = IndComponentType.AllowOpenShort;
			Component[1].CompName = "Is short entry allowed";
			
			}
			 
			 else if (slotType == SlotTypes.CloseFilter)
			{
			
			Component[0].DataType = IndComponentType.ForceCloseLong;
			Component[0].CompName = "Close out long position";
			Component[1].DataType = IndComponentType.ForceCloseShort;
			Component[1].CompName = "Close out short position";}

            return;
		}

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            EntryFilterLongDescription  = "Enter Long at a Bar Range Price Spread";
            EntryFilterShortDescription = "Enter Short at a Bar Range Price Spread";

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