// Repulse Indicator -- WTF v4
// Last changed on 2/10/2012
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
	/// adapted for wider time frame (WTF)
	/// adapted as WTF version 4
    /// </summary>
    public class Repulse_WTF : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Repulse_WTF(SlotTypes slotType)
        {
            // General properties
            IndicatorName  = "Repulse (WTF v4)";
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
 
			// sticking with ListParam 4 -- easier for copying and pasting from existing WTF indicator
            IndParam.ListParam[4].Caption = "Wider Time Frame Reference";
			IndParam.ListParam[4].ItemList = new string[] { "1 Minute", "5 Minutes", "15 Minutes", "30 Minutes", "1 Hour", "4 Hours", "1 Day", "1 Week"};
            if (Period == DataPeriods.min1) 		IndParam.ListParam[4].Index    = 1;
            else if (Period == DataPeriods.min5) 	IndParam.ListParam[4].Index    = 2;
            else if (Period == DataPeriods.min15) 	IndParam.ListParam[4].Index    = 3;
            else if (Period == DataPeriods.min30)	IndParam.ListParam[4].Index    = 4;
            else if (Period == DataPeriods.hour1)	IndParam.ListParam[4].Index    = 5;
            else if (Period == DataPeriods.hour4)	IndParam.ListParam[4].Index    = 6;
            else if (Period == DataPeriods.day)		IndParam.ListParam[4].Index    = 7;
            IndParam.ListParam[4].Text     = IndParam.ListParam[4].ItemList[IndParam.ListParam[4].Index];
            IndParam.ListParam[4].Enabled  = true;
            IndParam.ListParam[4].ToolTip  = "Choose wider time frame as compared to current chart period for this indicator.";		
 
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
 
            IndParam.NumParam[2].Caption = "Repulse1";
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
/// start WTF modfication 1 version 4
			IndParam.CheckParam[0].Caption = "Force previous WTF value";
            IndParam.CheckParam[0].Checked = true;
            IndParam.CheckParam[0].Enabled = false;
            IndParam.CheckParam[0].ToolTip = "WTF are set to always use their previous value.";
/// end WTF modfication 1 version 4

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

			            // Convert to Higher Time Frame ---------------------------------------------
			DataPeriods htfPeriod = DataPeriods.week;
			double[] hfOpen = new double[Bars];
			double[] hfClose = new double[Bars];
			double[] hfHigh = new double[Bars];
			double[] hfLow = new double[Bars];
			double[] hfVolume = new double[Bars];
			double[] hfPrice = new double[Bars];
			int[] 	 hIndex = new int[Bars];
			int      iFrame;
			int      hBars;
			
			switch (IndParam.ListParam[4].Index)
			{
				case 1: htfPeriod = DataPeriods.min5; break;
				case 2: htfPeriod = DataPeriods.min15; break;
				case 3: htfPeriod = DataPeriods.min30; break;
				case 4: htfPeriod = DataPeriods.hour1; break;
				case 5: htfPeriod = DataPeriods.hour4; break;
				case 6: htfPeriod = DataPeriods.day; break;
				case 7: htfPeriod = DataPeriods.week; break;
			}
			
			int err1 = HigherTimeFrame(Period, htfPeriod, out hIndex, out hBars, out iFrame, out hfHigh, out hfLow, out hfOpen, out hfClose, out hfVolume);
			//int err2 = HigherBasePrice(basePrice, hBars, hfHigh, hfLow, hfOpen, hfClose, out hfPrice);	
			if (err1==1) return;
			//------------------------------------------------------------------------


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

			for (int iBar = iFirstBar; iBar < hBars; iBar++)
			{
				double high1 = double.MinValue;
				double low1 = double.MaxValue;
				for (int i = 0; i <= RepulsePeriod1; i++)
				{
					if (high1 < hfHigh[iBar - i]) high1 = hfHigh[iBar - i];
					if (low1 > hfLow[iBar - i]) low1 = hfLow[iBar - i];
				}
			
				PosBuffer1[iBar] = ((((3*hfClose[iBar])-(2*low1)-hfOpen[iBar])/hfClose[iBar])*100);
				NegBuffer1[iBar] = (((hfOpen[iBar]+(2*high1)-(3*hfClose[iBar]))/hfClose[iBar])*100);
			}	

			double[] forceHaussiere1=MovingAverage(RepulsePeriod1*5, 0, MAMethod.Exponential, PosBuffer1);
			double[] forceBaissiere1=MovingAverage(RepulsePeriod1*5, 0, MAMethod.Exponential, NegBuffer1);

			for (int iBar = 1; iBar < hBars; iBar++)
			{	
				RepulseBuffer1[iBar]=forceHaussiere1[iBar]-forceBaissiere1[iBar];
			}


			// Repulse2
			for (int iBar = iFirstBar; iBar < hBars; iBar++)
			{
				double high2 = double.MinValue;
				double low2 = double.MaxValue;
				for (int i = 0; i <= RepulsePeriod2; i++)
				{
					if (high2 < hfHigh[iBar - i]) high2 = hfHigh[iBar - i];
					if (low2 > hfLow[iBar - i]) low2 = hfLow[iBar - i];
				}
				PosBuffer2[iBar] = ((((3*hfClose[iBar])-(2*low2)-hfOpen[iBar-RepulsePeriod2])/hfClose[iBar])*100);
				NegBuffer2[iBar] = (((hfOpen[iBar-RepulsePeriod2]+(2*high2)-(3*hfClose[iBar]))/hfClose[iBar])*100);
			}

			double[] forceHaussiere2=MovingAverage(RepulsePeriod2*5, 0, MAMethod.Exponential, PosBuffer2);
			double[] forceBaissiere2=MovingAverage(RepulsePeriod2*5, 0, MAMethod.Exponential, NegBuffer2);

			for (int iBar = 1; iBar < hBars; iBar++)
            {
				RepulseBuffer2[iBar]=forceHaussiere2[iBar]-forceBaissiere2[iBar];
			}


			// Repulse3
			for (int iBar = iFirstBar; iBar < hBars; iBar++)
			{
				double high3 = double.MinValue;
				double low3 = double.MaxValue;
				for (int i = 0; i <= RepulsePeriod3; i++)
				{
					if (high3 < hfHigh[iBar - i]) high3 = hfHigh[iBar - i];
					if (low3 > hfLow[iBar - i]) low3 = hfLow[iBar - i];
				}
				PosBuffer3[iBar] = ((((3*hfClose[iBar])-(2*low3)-hfOpen[iBar-RepulsePeriod3])/hfClose[iBar])*100);
				NegBuffer3[iBar] = (((hfOpen[iBar-RepulsePeriod3]+(2*high3)-(3*hfClose[iBar]))/hfClose[iBar])*100);
			}

			double[] forceHaussiere3=MovingAverage(RepulsePeriod3*5, 0, MAMethod.Exponential, PosBuffer3);
			double[] forceBaissiere3=MovingAverage(RepulsePeriod3*5, 0, MAMethod.Exponential, NegBuffer3);

			for (int iBar = 2; iBar < hBars; iBar++)
            {
				RepulseBuffer3[iBar]=forceHaussiere3[iBar]-forceBaissiere3[iBar];
			}



            // Convert to Current Time Frame ----------------------------------------------

/// start WTF modfication 2 version 4
			// copy of wider time frame array of values
			double[] hRepulseBuffer1 = new double[Bars];
			double[] hRepulseBuffer2 = new double[Bars];
			double[] hRepulseBuffer3 = new double[Bars];
			RepulseBuffer1.CopyTo (hRepulseBuffer1, 0);
			RepulseBuffer2.CopyTo (hRepulseBuffer2, 0);
			RepulseBuffer3.CopyTo (hRepulseBuffer3, 0);

			int err3 = CurrentTimeFrame(hIndex, hBars, ref RepulseBuffer1);
			int err4 = CurrentTimeFrame(hIndex, hBars, ref RepulseBuffer2);			
			int err5 = CurrentTimeFrame(hIndex, hBars, ref RepulseBuffer3);
			// if any error, return out of calculation and indicator fails silently
			if (err3 == 1 || err4 == 1 || err5 == 1)
			{
				return;
			}
/// end WTF modfication 2 version 4

			//-----------------------------------------------------------------------------	



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
 

 /// start WTF modfication 3 version 4
// use wtf values here, then do expansion after this if clause to cover these IndicatorCrosses cases and the OscillatorLogic cases

			// back up Bars value, reset to hBars, for performance improvement in indicator logic function
			int mtfBars = Data.Bars;
			Data.Bars = hBars;

			// replace very small values with 0 for performance improvement; don't know why but works
			for (int ctr = 0; ctr < hRepulseBuffer1.Length; ctr++)
			{
				hRepulseBuffer1[ctr] = (hRepulseBuffer1[ctr] < .000000001 && hRepulseBuffer1[ctr] > -.000000001) ? 0 : hRepulseBuffer1[ctr];
				hRepulseBuffer2[ctr] = (hRepulseBuffer2[ctr] < .000000001 && hRepulseBuffer2[ctr] > -.000000001) ? 0 : hRepulseBuffer2[ctr];
				hRepulseBuffer3[ctr] = (hRepulseBuffer3[ctr] < .000000001 && hRepulseBuffer3[ctr] > -.000000001) ? 0 : hRepulseBuffer3[ctr];
			}

            if (IndParam.ListParam[0].Text == "The Repulse1 crosses the Repulse2 upward")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorCrossesAnotherIndicatorUpwardLogic(iFirstBar, iPrvs,hRepulseBuffer1, hRepulseBuffer2, ref Component[3], ref Component[4]);
            }
            else if (IndParam.ListParam[0].Text == "The Repulse1 crosses the Repulse2 downward")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorCrossesAnotherIndicatorDownwardLogic(iFirstBar, iPrvs, hRepulseBuffer1, hRepulseBuffer2, ref Component[3], ref Component[4]);
            }
            else if (IndParam.ListParam[0].Text == "The Repulse1 is higher than the Repulse2")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorIsHigherThanAnotherIndicatorLogic(iFirstBar, iPrvs, hRepulseBuffer1, hRepulseBuffer2, ref Component[3], ref Component[4]);
            }
            else if (IndParam.ListParam[0].Text == "The Repulse1 is lower than the Repulse2")
            {
                SpecialValues = new double[1] { 50 };
                IndicatorIsLowerThanAnotherIndicatorLogic(iFirstBar, iPrvs, hRepulseBuffer1, hRepulseBuffer2, ref Component[3], ref Component[4]);
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
 
				OscillatorLogic (iFirstBar, iPrvs, hRepulseBuffer2, iLevel, - iLevel, ref Component[3], ref Component[4], indLogic);

            }
 
			// resest Bars to real value
			Data.Bars = mtfBars;

 			// expand component array from wtf to current time frame
			double[] wtfCompValue = Component[3].Value;
			int err6 = CurrentTimeFrame(hIndex, hBars, ref wtfCompValue);
			if (err6 == 1) { return; }
			Component[3].Value = wtfCompValue;
			wtfCompValue = Component[4].Value;
			int err7 = CurrentTimeFrame(hIndex, hBars, ref wtfCompValue);
			if (err7 == 1) { return; }
			Component[4].Value = wtfCompValue;


/// end WTF modfication 3 version 4

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
                IndParam.NumParam[0].ValueToString + ", " + // Repulse1 period
                IndParam.NumParam[1].ValueToString + ", " + // Repulse3 period
                IndParam.NumParam[2].ValueToString + ", " +   // Repulse2 period
				"WTF=" + IndParam.ListParam[4].Text + ")";   // WTF period
 
            return sString;
        }


		/// <summary>

        /// Convert current time frame to higher time frame
        /// </summary>
/// start WTF modfication 4 version 4
		protected static int HigherTimeFrame(DataPeriods currentTF, DataPeriods higherTF, out int[] hIndex, out int hBars, out int iFrame, out double[] hfHigh, out double[] hfLow, out double[] hfOpen, out double[] hfClose, out double[] hfVolume)
		{
			hfOpen = new double[Bars];
			hfClose = new double[Bars];
			hfHigh = new double[Bars];
			hfLow = new double[Bars];
			hfVolume = new double[Bars];
			hIndex = new int[Bars];
			hBars =0;
			iFrame = 1;

			// verify user input, if higher TF is not greater than current TF then return to exit function
			if ((int)higherTF <=  (int)currentTF) {
				return 1;
			}
			
			// Frame Calculation
			if (higherTF>currentTF)
				iFrame = (int)((int)higherTF/(int)currentTF);
			else
				iFrame = 1;
			if (higherTF==DataPeriods.week && currentTF==DataPeriods.day) iFrame=5;
			
			TimeSpan tsCurrent = new TimeSpan();
			switch (currentTF)
			{
				case DataPeriods.min1:
					tsCurrent = TimeSpan.FromMinutes (1);
					break;
				case DataPeriods.min5:
					tsCurrent = TimeSpan.FromMinutes (5);
					break;
				case DataPeriods.min15:
					tsCurrent = TimeSpan.FromMinutes (15);
					break;
				case DataPeriods.min30:
					tsCurrent = TimeSpan.FromMinutes (30);
					break;
				case DataPeriods.hour1:
					tsCurrent = TimeSpan.FromHours (1);
					break;
				case DataPeriods.hour4:
					tsCurrent = TimeSpan.FromHours (4);
					break;
			}
			TimeSpan tsHTF = new TimeSpan();
			switch (higherTF)
			{
				case DataPeriods.min5:
					tsHTF = TimeSpan.FromMinutes (5);
					break;
				case DataPeriods.min15:
					tsHTF = TimeSpan.FromMinutes (15);
					break;
				case DataPeriods.min30:
					tsHTF = TimeSpan.FromMinutes (30);
					break;
				case DataPeriods.hour1:
					tsHTF = TimeSpan.FromHours (1);
					break;
				case DataPeriods.hour4:
					tsHTF = TimeSpan.FromHours (4);
					break;
				case DataPeriods.day:
					tsHTF = TimeSpan.FromDays (1);
					break;
				case DataPeriods.week:
					tsHTF = TimeSpan.FromDays (7);
					break;
			}


			// set all HTFs to start from first modulo period in data series and cut off earlier data 
			// set iStartBar back one so htf close is written on last lower time frame bar of interval (eg, htf = 1 hour, write close on 1:55, 2:55, 3:55 bar instead of on 2:00, 3:00, 4:00 bar)
			// if htf is week, sync to close on Fridays
			int iStartBar = 0;
			DateTime dtStart = new DateTime (Date[0].Year, Date[0].Month, Date[0].Day, 0, 0, 0);
			if (higherTF == DataPeriods.week)
			{
				while (dtStart.DayOfWeek != DayOfWeek.Friday)
				{
					dtStart = dtStart.AddDays(-1);
				}
			}

			for (int iBar=1; iBar<Bars; iBar++) {
				if ((Date[iBar]-dtStart).Ticks % tsHTF.Ticks == 0)
				{
					iStartBar = iBar;
					iBar = Bars;
				}
			}

			// loop through bars, figure difference between this bar time and period starting bar time
			// if difference equals time span, means new htf bar open
			// if greater than time span, resync the cycle, in case of crossing weekend or holidays, or a few lost bars
			// hIndex[hBar] -- to keep track of where HTF values change when going to LTF, which has a lot more bars; should be first LTF bar of the LTF bars that are in the HTF bar
			int hBar = 0;
			int iStartHTFBar = iStartBar;
			for (int iBar=iStartBar; iBar<Bars; iBar++)
			{
				// new higher time frame bar, initialize with values
				if ((Date[iBar] - Date[iStartHTFBar]).Ticks  % tsHTF.Ticks == 0)
				{
					hBar++;
					iStartHTFBar = iBar;
					hfOpen[hBar]   = Open[iBar];
					hfClose[hBar]  = Close[iBar];
					hfHigh[hBar]   = High[iBar];
					hfLow[hBar]    = Low[iBar];
					hfVolume[hBar] = Volume[iBar];
					hIndex[hBar] = iBar;
				}
				// progressing through higher time frame bar or at end, update High, Low, Close and Volume
				else if (Date[iBar] - Date[iStartHTFBar] < tsHTF)
				{
					hfClose[hBar]  = Close[iBar];
					if (High[iBar] > hfHigh[hBar])	hfHigh[hBar] = High[iBar];
					if (Low[iBar]  < hfLow[hBar])	hfLow[hBar] = Low[iBar];
					hfVolume[hBar] += Volume[iBar];
				}
				// must have lost some bars, so get back in sync, add values for closing of partial but completed htf bar
				else if (Date[iBar] - Date[iStartHTFBar] > tsHTF)
				{
					// set this bar as opening HTF bar, initialize
					hBar++;
					hfOpen[hBar]   = Open[iBar];
					hfClose[hBar]  = Close[iBar];
					hfHigh[hBar]   = High[iBar];
					hfLow[hBar]    = Low[iBar];
					hfVolume[hBar] = Volume[iBar];
					hIndex[hBar] = iBar;
					
					for (int iSyncBar = iBar; iSyncBar<Bars; iSyncBar++) {
						// check if have found next HTF bar against last known HTF start bar
						if ((Date[iSyncBar] - Date[iStartHTFBar]).Ticks % tsHTF.Ticks == 0)
						{
							//have found next HTF bar, initialize
							hBar++;
							iStartHTFBar = iSyncBar;
							iBar = iSyncBar;
							hfOpen[hBar]   = Open[iSyncBar];
							hfClose[hBar]  = Close[iSyncBar];
							hfHigh[hBar]   = High[iSyncBar];
							hfLow[hBar]    = Low[iSyncBar];
							hfVolume[hBar] = Volume[iSyncBar];
							hIndex[hBar] = iSyncBar;
							iSyncBar = Bars;
						}
						else // not found yet, only update
						{
							hfClose[hBar]  = Close[iSyncBar];
							if (High[iSyncBar] > hfHigh[hBar])	hfHigh[hBar] = High[iSyncBar];
							if (Low[iSyncBar]  < hfLow[hBar])	hfLow[hBar] = Low[iSyncBar];
							hfVolume[hBar] += Volume[iSyncBar];
						}
					}
				}

			}	
			hBars = hBar + 1;
			return 0;
		}


		/// <summary>
        /// Convert higher time frame to current time frame
        /// </summary>
		protected static int CurrentTimeFrame(int[] hIndex, int hBars, ref double[] hIndicator)
		{
			double[] hBuffer = new double[Bars];
			int hBar = 0;

			for (int iBar=0; iBar<Bars; iBar++)
			{	
				if (hBar < hBars-1) // protect against going out of HTF indicator range
				{
					if (iBar < hIndex[hBar+1])
					{
						hBuffer[iBar] = hIndicator[hBar];
					}
					else {
						hBar++;
						hBuffer[iBar] = hIndicator[hBar];
					}
				}
				// else reached end of HTF close values, fill in with rest of incomplete HTF bar values
				else {
					hBuffer[iBar] = hIndicator[hBar];
				}
			}
			hIndicator = new double[Bars];
			hIndicator = hBuffer;
			return 0;
		}
/// end WTF modfication 4 version 4
				
		/// <summary>
        /// Calculate base price for higher time frame data
        /// </summary>
		protected static int HigherBasePrice(BasePrice basePrice, int hBars, double[] hfHigh, double[] hfLow, double[] hfOpen, double[] hfClose, out double[] hfPrice)
		{
			hfPrice = new double[Bars];
			
			switch (basePrice)
			{
				case BasePrice.Open:	hfPrice = hfOpen;	break;
				case BasePrice.Close:	hfPrice = hfClose;	break;
				case BasePrice.High:	hfPrice = hfHigh;	break;
				case BasePrice.Low:		hfPrice = hfLow;	break;
				case BasePrice.Median:
					for (int iBar=0; iBar<hBars; iBar++)
					{
						hfPrice[iBar] = (hfLow[iBar] + hfHigh[iBar]) / 2;
					}
					break;
				case BasePrice.Typical:
					for (int iBar=0; iBar<hBars; iBar++)
					{
						hfPrice[iBar] = (hfLow[iBar] + hfHigh[iBar] + hfClose[iBar]) / 3;
					}
					break;
				case BasePrice.Weighted:
					for (int iBar=0; iBar<hBars; iBar++)
					{
						hfPrice[iBar] = (hfOpen[iBar] + hfLow[iBar] + hfHigh[iBar] + hfClose[iBar]) / 4;
					}
					break;
				default:
					break;
			}
			return 0;
		}


    }
}
