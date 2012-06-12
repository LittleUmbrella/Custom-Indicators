using System;
using System.Drawing;
 
namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Aroon Histogram Indicator
    /// </summary>
    public class Aroon_Histogram : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Aroon_Histogram(SlotTypes slotType)
        {
            // General properties
            IndicatorName  = "Aroon UpDown";
            PossibleSlots  = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
            CustomIndicator = true;
 
            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;
 
            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "Long condition if Aroon Up > 99, Short condition if Aroon Down > 99",
            };
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of the indicator.";
 
            IndParam.ListParam[1].Caption  = "Base price";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[1].Index    = (int)BasePrice.Close;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "The price the Aroon is based on.";
 
            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption   = "Period";
            IndParam.NumParam[0].Value     = 9;
            IndParam.NumParam[0].Min       = 1;
            IndParam.NumParam[0].Max       = 200;
            IndParam.NumParam[0].Enabled   = true;
            IndParam.NumParam[0].ToolTip   = "Period used to calculate the Aroon value.";
 
            /*IndParam.NumParam[1].Caption   = "Level";
            IndParam.NumParam[1].Value     = 0;
            IndParam.NumParam[1].Min       = 0;
            IndParam.NumParam[1].Max       = 100;
            IndParam.NumParam[1].Enabled   = true;
            IndParam.NumParam[1].ToolTip   = "A critical level (for the appropriate logic).";*/
 
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
            BasePrice basePrice = (BasePrice)IndParam.ListParam[1].Index;
            int       iPeriod   = (int)IndParam.NumParam[0].Value;
            //double    dLevel    = IndParam.NumParam[1].Value;
            int       iPrvs     = IndParam.CheckParam[0].Checked ? 1 : 0;
 
            // Calculation
            int      iFirstBar   = iPeriod + 2;
            double[] adBasePrice = Price(basePrice);
            double[] adUp        = new double[Bars];
            double[] adDown      = new double[Bars];
            double[] adAroon     = new double[Bars];
			double[] adAroon1     = new double[Bars];
 
            for (int iBar = iPeriod; iBar < Bars; iBar++)
            {
                double dHighestHigh = double.MinValue;
                double dLowestLow   = double.MaxValue;
                for (int i = 0; i < iPeriod; i++)
                {
                    int iBaseBar = iBar - iPeriod + 1 + i;
                    if (adBasePrice[iBaseBar] > dHighestHigh)
                    {
                        dHighestHigh = adBasePrice[iBaseBar];
                        adUp[iBar] = 100.0 * i / (iPeriod - 1);
                    }
                    if (adBasePrice[iBaseBar] < dLowestLow)
                    {
                        dLowestLow = adBasePrice[iBaseBar];
                        adDown[iBar] = 100.0 * i / (iPeriod - 1);
                    }
                }
				
				if (adUp[iBar-1] > 99)
					adAroon[iBar] = 1;
					
				if (adDown[iBar-1] > 99)
					adAroon1[iBar] = 1;
            }

			
			

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp();
            Component[0].CompName  = "Allow long entry";
            Component[0].DataType  = IndComponentType.AllowOpenLong;
            Component[0].ChartType = IndChartType.NoChart;
            Component[0].FirstBar  = iFirstBar;
            Component[0].Value     = adAroon;

            Component[1] = new IndicatorComp();
            Component[1].CompName  = "Allow short entry";
            Component[1].DataType  = IndComponentType.AllowOpenShort;
            Component[1].ChartType = IndChartType.NoChart;
            Component[1].FirstBar  = iFirstBar;
            Component[1].Value     = adAroon1;
			
			Component[2] = new IndicatorComp();
            Component[2].CompName   = "Aroon Up";
            Component[2].DataType   = IndComponentType.IndicatorValue;
            Component[2].ChartType  = IndChartType.Line;
            Component[2].ChartColor = Color.Blue;
            Component[2].FirstBar   = iFirstBar;
            Component[2].Value      = adUp;
			
			Component[3] = new IndicatorComp();
            Component[3].CompName   = "Aroon Down";
            Component[3].DataType   = IndComponentType.IndicatorValue;
            Component[3].ChartType  = IndChartType.Line;
            Component[3].ChartColor = Color.Red;
            Component[3].FirstBar   = iFirstBar;
            Component[3].Value      = adDown;

            return;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            EntryFilterLongDescription  = "Long condition if Aroon Up > 99, Short condition if Aroon Down > 99";
            EntryFilterShortDescription = "Long condition if Aroon Up > 99, Short condition if Aroon Down > 99";

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
