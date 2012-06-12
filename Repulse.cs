// Repulse Indicator
// Last changed on 2009-05-05
// Copyright (c) 2011 - All rights reserved.
// Part of Forex Strategy Builder & Forex Strategy Trader
// Website http://forexsb.com/
// This code or any part of it cannot be used in other applications without a permission.
 
using System;
using System.Drawing;
 
namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Repulse Indicator
    /// </summary>
    public class Repulse : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Repulse(SlotTypes slotType)
        {
            // General properties
            IndicatorName  = "Repulse";
            PossibleSlots  = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
			CustomIndicator=true;
 
            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;
 
            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "The Repulse2 rises",
                "The Repulse2 falls",
                "The Repulse2 is higher than the Level line",
                "The Repulse2 is lower than the Level line",
                "The Repulse2 crosses the Level line upward",
                "The Repulse2 crosses the Level line downward",
                "The Repulse2 changes its direction upward",
                "The Repulse2 changes its direction downward",
                "The Repulse1 is higher than the Repulse2",
                "The Repulse1 is lower than the Repulse2",
                "The Repulse1 crosses the Repulse2 upward",
                "The Repulse1 crosses the Repulse2 downward",
            };
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of the indicator.";
 
    /*        IndParam.ListParam[1].Caption  = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[1].Index    = (int)MAMethod.Exponential;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "The smoothing method of Moving Averages.";
 
            IndParam.ListParam[2].Caption  = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[2].Index    = (int)BasePrice.Close;
            IndParam.ListParam[2].Text     = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "The price the Moving Averages are based on.";
 
            IndParam.ListParam[3].Caption  = "Repulse1 method";
            IndParam.ListParam[3].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[3].Index    = (int)MAMethod.Simple;
            IndParam.ListParam[3].Text     = IndParam.ListParam[3].ItemList[IndParam.ListParam[3].Index];
            IndParam.ListParam[3].Enabled  = true;
            IndParam.ListParam[3].ToolTip  = "The smoothing method of the Repulse1.";
	*/		
 
            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Repulse2";
            IndParam.NumParam[0].Value   = 5;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of Repulse2.";
 
            IndParam.NumParam[1].Caption = "Repulse3";
            IndParam.NumParam[1].Value   = 15;
            IndParam.NumParam[1].Min     = 1;
            IndParam.NumParam[1].Max     = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The period of Repulse3.";
 
            IndParam.NumParam[2].Caption = "Repulse1 period.";
            IndParam.NumParam[2].Value   = 1;
            IndParam.NumParam[2].Min     = 1;
            IndParam.NumParam[2].Max     = 200;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The period of Repulse1.";
			
			IndParam.NumParam[3].Caption = "Level";
            IndParam.NumParam[3].Value   = 0;
            IndParam.NumParam[3].Min     = -5;
            IndParam.NumParam[3].Max     = 5;
			IndParam.NumParam[3].Point   = 1;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "A critical level (for the appropriate logic).";
 
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
			int       iPrvs     = IndParam.CheckParam[0].Checked ? 1 : 0;
			int      RepulsePeriod2   = (int)IndParam.NumParam[0].Value;
            int      RepulsePeriod3   = (int)IndParam.NumParam[1].Value;
			int      RepulsePeriod1   = (int)IndParam.NumParam[2].Value;
			double    iLevel    = IndParam.NumParam[3].Value;
			int iFirstBar = 1 + RepulsePeriod2 + RepulsePeriod3 + RepulsePeriod1;
			
			double[] PosBuffer1 = new double[Bars];
			double[] NegBuffer1 = new double[Bars];
			double[] PosBuffer2 = new double[Bars];
			double[] NegBuffer2 = new double[Bars];
			double[] PosBuffer3 = new double[Bars];
			double[] NegBuffer3 = new double[Bars];
			
			double[] RepulseBuffer1 = new double[Bars];
			double[] RepulseBuffer2 = new double[Bars];
			double[] RepulseBuffer3 = new double[Bars];
			double[] Long = new double[Bars];
			double[] Short = new double[Bars];
 
            // Calculation
			// Repulse1
			for (int iBar = iFirstBar; iBar < Bars; iBar++)
			{
					double high1 = double.MinValue;
					double low1 = double.MaxValue;
					for (int i = 0; i <= RepulsePeriod1; i++)
					{
						if (high1 < High[iBar - i]) high1 = High[iBar - i];
						if (low1 > Low[iBar - i]) low1 = Low[iBar - i];
					}
			
				PosBuffer1[iBar] = ((((3*Close[iBar])-(2*low1)-Open[iBar])/Close[iBar])*100);
				NegBuffer1[iBar] = (((Open[iBar]+(2*high1)-(3*Close[iBar]))/Close[iBar])*100);
			}	
				double[] forceHaussiere1=MovingAverage(RepulsePeriod1*5, 0, MAMethod.Exponential, PosBuffer1);
				double[] forceBaissiere1=MovingAverage(RepulsePeriod1*5, 0, MAMethod.Exponential, NegBuffer1);
				
			for (int iBar = 1; iBar < Bars; iBar++)
			{	
				RepulseBuffer1[iBar]=forceHaussiere1[iBar]-forceBaissiere1[iBar];
			}
            // Repulse2
			for (int iBar = iFirstBar; iBar < Bars; iBar++)
			{
					double high2 = double.MinValue;
					double low2 = double.MaxValue;
					for (int i = 0; i <= RepulsePeriod2; i++)
					{
						if (high2 < High[iBar - i]) high2 = High[iBar - i];
						if (low2 > Low[iBar - i]) low2 = Low[iBar - i];
					}
				PosBuffer2[iBar] = ((((3*Close[iBar])-(2*low2)-Open[iBar-RepulsePeriod2])/Close[iBar])*100);
				NegBuffer2[iBar] = (((Open[iBar-RepulsePeriod2]+(2*high2)-(3*Close[iBar]))/Close[iBar])*100);
			}
				double[] forceHaussiere2=MovingAverage(RepulsePeriod2*5, 0, MAMethod.Exponential, PosBuffer2);
				double[] forceBaissiere2=MovingAverage(RepulsePeriod2*5, 0, MAMethod.Exponential, NegBuffer2);
			for (int iBar = 1; iBar < Bars; iBar++)
            {
				RepulseBuffer2[iBar]=forceHaussiere2[iBar]-forceBaissiere2[iBar];
			}
			// Repulse3
			for (int iBar = iFirstBar; iBar < Bars; iBar++)
			{
					double high3 = double.MinValue;
					double low3 = double.MaxValue;
					for (int i = 0; i <= RepulsePeriod3; i++)
					{
						if (high3 < High[iBar - i]) high3 = High[iBar - i];
						if (low3 > Low[iBar - i]) low3 = Low[iBar - i];
					}
				PosBuffer3[iBar] = ((((3*Close[iBar])-(2*low3)-Open[iBar-RepulsePeriod3])/Close[iBar])*100);
				NegBuffer3[iBar] = (((Open[iBar-RepulsePeriod3]+(2*high3)-(3*Close[iBar]))/Close[iBar])*100);
			}
				double[] forceHaussiere3=MovingAverage(RepulsePeriod3*5, 0, MAMethod.Exponential, PosBuffer3);
				double[] forceBaissiere3=MovingAverage(RepulsePeriod3*5, 0, MAMethod.Exponential, NegBuffer3);
			for (int iBar = 2; iBar < Bars; iBar++)
            {
				RepulseBuffer3[iBar]=forceHaussiere3[iBar]-forceBaissiere3[iBar];
			}
 
            // Saving the components
            Component = new IndicatorComp[5];
 
            Component[0] = new IndicatorComp();
            Component[0].CompName   = "Repulse1";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor = Color.Brown;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = RepulseBuffer1;
 
            Component[1] = new IndicatorComp();
            Component[1].CompName   = "Repulse3";
            Component[1].DataType   = IndComponentType.IndicatorValue;
            Component[1].ChartType  = IndChartType.Line;
            Component[1].ChartColor = Color.Yellow;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = RepulseBuffer3;
 
            Component[2] = new IndicatorComp();
            Component[2].CompName   = "Repulse2";
            Component[2].DataType   = IndComponentType.IndicatorValue;
            Component[2].ChartType  = IndChartType.Line;
            Component[2].ChartColor = Color.Blue;
            Component[2].FirstBar   = iFirstBar;
            Component[2].Value      = RepulseBuffer2;
 
            Component[3] = new IndicatorComp();
            Component[3].ChartType = IndChartType.NoChart;
            Component[3].FirstBar  = iFirstBar;
            Component[3].Value     = new double[Bars];
 
            Component[4] = new IndicatorComp();
            Component[4].ChartType = IndChartType.NoChart;
            Component[4].FirstBar  = iFirstBar;
            Component[4].Value     = new double[Bars];
 
            // Sets the Component's type
            if (slotType == SlotTypes.OpenFilter)
            {
                Component[3].DataType = IndComponentType.AllowOpenLong;
                Component[3].CompName = "Is long entry allowed";
                Component[4].DataType = IndComponentType.AllowOpenShort;
                Component[4].CompName = "Is short entry allowed";
            }
            else if (slotType == SlotTypes.CloseFilter)
            {
                Component[3].DataType = IndComponentType.ForceCloseLong;
                Component[3].CompName = "Close out long position";
                Component[4].DataType = IndComponentType.ForceCloseShort;
                Component[4].CompName = "Close out short position";
            }
 
            // Calculation of the logic
            IndicatorLogic indLogic = IndicatorLogic.It_does_not_act_as_a_filter;
 
            if (IndParam.ListParam[0].Text == "The Repulse1 crosses the Repulse2 upward")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorCrossesAnotherIndicatorUpwardLogic(iFirstBar, iPrvs,RepulseBuffer1, RepulseBuffer2, ref Component[3], ref Component[4]);
                return;
            }
            else if (IndParam.ListParam[0].Text == "The Repulse1 crosses the Repulse2 downward")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorCrossesAnotherIndicatorDownwardLogic(iFirstBar, iPrvs, RepulseBuffer1, RepulseBuffer2, ref Component[3], ref Component[4]);
                return;
            }
            else if (IndParam.ListParam[0].Text == "The Repulse1 is higher than the Repulse2")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorIsHigherThanAnotherIndicatorLogic(iFirstBar, iPrvs, RepulseBuffer1, RepulseBuffer2, ref Component[3], ref Component[4]);
                return;
            }
            else if (IndParam.ListParam[0].Text == "The Repulse1 is lower than the Repulse2")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorIsLowerThanAnotherIndicatorLogic(iFirstBar, iPrvs, RepulseBuffer1, RepulseBuffer2, ref Component[3], ref Component[4]);
                return;
            }
            else
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "The Repulse2 rises":
                        indLogic = IndicatorLogic.The_indicator_rises;
                        SpecialValues = new double[1] { 50 };
                        break;
 
                    case "The Repulse2 falls":
                        indLogic = IndicatorLogic.The_indicator_falls;
                        SpecialValues = new double[1] { 50 };
                        break;
 
                    case "The Repulse2 is higher than the Level line":
                        indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                        SpecialValues = new double[2] { iLevel, - iLevel };
                        break;
 
                    case "The Repulse2 is lower than the Level line":
                        indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                        SpecialValues = new double[2] { iLevel, - iLevel };
                        break;
 
                    case "The Repulse2 crosses the Level line upward":
                        indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                        SpecialValues = new double[2] { iLevel, - iLevel };
                        break;
 
                    case "The Repulse2 crosses the Level line downward":
                        indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                        SpecialValues = new double[2] { iLevel, - iLevel };
                        break;
 
                    case "The Repulse2 changes its direction upward":
                        indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                        SpecialValues = new double[1] { 50 };
                        break;
 
                    case "The Repulse2 changes its direction downward":
                        indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                        SpecialValues = new double[1] { 50 };
                        break;
 
                    default:
                        break;
                }
 
                OscillatorLogic(iFirstBar, iPrvs, RepulseBuffer2, iLevel, - iLevel, ref Component[3], ref Component[4], indLogic);
            }
 
            return;
        }
 
        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            string sLevelLong  = IndParam.NumParam[3].ValueToString;
            string sLevelShort = IndParam.NumParam[3].AnotherValueToString(100 - IndParam.NumParam[3].Value);
 
            EntryFilterLongDescription  = ToString() + " - ";
            EntryFilterShortDescription = ToString() + " - ";
            ExitFilterLongDescription   = ToString() + " - ";
            ExitFilterShortDescription  = ToString() + " - ";
 
            switch (IndParam.ListParam[0].Text)
            {
                case "The Repulse2 rises":
                    EntryFilterLongDescription  += "the Repulse2 rises";
                    EntryFilterShortDescription += "the Repulse2 falls";
                    ExitFilterLongDescription   += "the Repulse2 rises";
                    ExitFilterShortDescription  += "the Repulse2 falls";
                    break;
 
                case "The Repulse2 falls":
                    EntryFilterLongDescription  += "the Repulse2 falls";
                    EntryFilterShortDescription += "the Repulse2 rises";
                    ExitFilterLongDescription   += "the Repulse2 falls";
                    ExitFilterShortDescription  += "the Repulse2 rises";
                    break;
 
                case "The Repulse2 is higher than the Level line":
                    EntryFilterLongDescription  += "the Repulse2 is higher than the Level " + sLevelLong;
                    EntryFilterShortDescription += "the Repulse2 is lower than the Level "  + sLevelShort;
                    ExitFilterLongDescription   += "the Repulse2 is higher than the Level " + sLevelLong;
                    ExitFilterShortDescription  += "the Repulse2 is lower than the Level "  + sLevelShort;
                    break;
 
                case "The Repulse2 is lower than the Level line":
                    EntryFilterLongDescription  += "the Repulse2 is lower than the Level "  + sLevelLong;
                    EntryFilterShortDescription += "the Repulse2 is higher than the Level " + sLevelShort;
                    ExitFilterLongDescription   += "the Repulse2 is lower than the Level "  + sLevelLong;
                    ExitFilterShortDescription  += "the Repulse2 is higher than the Level " + sLevelShort;
                    break;
 
                case "The Repulse2 crosses the Level line upward":
                    EntryFilterLongDescription  += "the Repulse2 crosses the Level " + sLevelLong  + " upward";
                    EntryFilterShortDescription += "the Repulse2 crosses the Level " + sLevelShort + " downward";
                    ExitFilterLongDescription   += "the Repulse2 crosses the Level " + sLevelLong  + " upward";
                    ExitFilterShortDescription  += "the Repulse2 crosses the Level " + sLevelShort + " downward";
                    break;
 
                case "The Repulse2 crosses the Level line downward":
                    EntryFilterLongDescription  += "the Repulse2 crosses the Level " + sLevelLong  + " downward";
                    EntryFilterShortDescription += "the Repulse2 crosses the Level " + sLevelShort + " upward";
                    ExitFilterLongDescription   += "the Repulse2 crosses the Level " + sLevelLong  + " downward";
                    ExitFilterShortDescription  += "the Repulse2 crosses the Level " + sLevelShort + " upward";
                    break;
 
                case "The Repulse1 crosses the Repulse2 upward":
                    EntryFilterLongDescription  += "the Repulse1 crosses the Repulse2 upward";
                    EntryFilterShortDescription += "the Repulse1 crosses the Repulse2 downward";
                    ExitFilterLongDescription   += "the Repulse1 crosses the Repulse2 upward";
                    ExitFilterShortDescription  += "the Repulse1 crosses the Repulse2 downward";
                    break;
 
                case "The Repulse1 crosses the Repulse2 downward":
                    EntryFilterLongDescription  += "the Repulse1 crosses the Repulse2 downward";
                    EntryFilterShortDescription += "the Repulse1 crosses the Repulse2 upward";
                    ExitFilterLongDescription   += "the Repulse1 crosses the Repulse2 downward";
                    ExitFilterShortDescription  += "the Repulse1 crosses the Repulse2 upward";
                    break;
 
                case "The Repulse1 is higher than the Repulse2":
                    EntryFilterLongDescription  += "the Repulse1 is higher than the Repulse2";
                    EntryFilterShortDescription += "the Repulse1 is lower than the Repulse2";
                    ExitFilterLongDescription   += "the Repulse1 is higher than the Repulse2";
                    ExitFilterShortDescription  += "the Repulse1 is lower than the Repulse2";
                    break;
 
                case "The Repulse1 is lower than  the Repulse2":
                    EntryFilterLongDescription  += "the Repulse1 is lower than the Repulse2";
                    EntryFilterShortDescription += "the Repulse1 is higher than than the Repulse2";
                    ExitFilterLongDescription   += "the Repulse1 is lower than the Repulse2";
                    ExitFilterShortDescription  += "the Repulse1 is higher than than the Repulse2";
                    break;
 
                case "The Repulse2 changes its direction upward":
                    EntryFilterLongDescription  += "the Repulse2 changes its direction upward";
                    EntryFilterShortDescription += "the Repulse2 changes its direction downward";
                    ExitFilterLongDescription   += "the Repulse2 changes its direction upward";
                    ExitFilterShortDescription  += "the Repulse2 changes its direction downward";
                    break;
 
                case "The Repulse2 changes its direction downward":
                    EntryFilterLongDescription  += "the Repulse2 changes its direction downward";
                    EntryFilterShortDescription += "the Repulse2 changes its direction upward";
                    ExitFilterLongDescription   += "the Repulse2 changes its direction downward";
                    ExitFilterShortDescription  += "the Repulse2 changes its direction upward";
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
            string sString = IndicatorName + 
                (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                IndParam.ListParam[1].Text         + ", " + // Smoothing method
                IndParam.NumParam[0].ValueToString + ", " + // Repulse1 period
                IndParam.NumParam[1].ValueToString + ", " + // Repulse3 period
                IndParam.NumParam[2].ValueToString + ")";   // Repulse2 period
 
            return sString;
        }
    }
}
