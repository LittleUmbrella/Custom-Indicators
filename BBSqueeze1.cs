// BB Squeeze indicator
// Last changed on 4/28/2011
// Part of Forex Strategy Builder v2.8.3.8+
// Website http://forexsb.com/
// Copyright (c) 2006 - 2009 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;
using System.Drawing;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// BB Squeeze Indicator
	/// from metastock indicator
	///  compare bollinger bands and keltner channels with linear regression
    /// </summary>
    public class BBSqueeze : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public BBSqueeze(SlotTypes slotType)
        {
            // General properties
            IndicatorName  = "BB Squeeze";
            PossibleSlots  =SlotTypes.OpenFilter | SlotTypes.CloseFilter;
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
				"Fire changes from on to off",
				"Fire changes from off to on",
				"The BB Squeeze is higher than the level line and Fire is on",
                "The BB Squeeze is higher than the level line and Fire is off",
				"The BB Squeeze is lower than the level line and Fire is on",
                "The BB Squeeze is lower than the level line and Fire is off",
				"The BB Squeeze is higher than the level line",
				"The BB Squeeze is lower than the level line",
				"The BB Squeeze rises and Fire is on",
				"The BB Squeeze rises and Fire is off",
				"The BB Squeeze falls and Fire is on",
				"The BB Squeeze falls and Fire is off",
				"The BB Squeeze rises",
				"The BB Squeeze falls",
				"Fire is on",
				"Fire is off",
				"Draw only, no Entry or Exit signals"
            };
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Bollinger Period";
            IndParam.NumParam[0].Value   = 10;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "Period of the Bollinger Band in indicator.";

            IndParam.NumParam[1].Caption = "Bollinger Multiplier";
            IndParam.NumParam[1].Value   = 2;
            IndParam.NumParam[1].Min     = 1;
            IndParam.NumParam[1].Max     = 5;
            IndParam.NumParam[1].Point   = 1;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Determines the width of Bollinger Bands.";

            IndParam.NumParam[2].Caption = "Keltner Period";
            IndParam.NumParam[2].Value   = 20;
            IndParam.NumParam[2].Min     = 1;
            IndParam.NumParam[2].Max     = 200;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "Period of the Keltner Channel in indicator.";

            IndParam.NumParam[3].Caption = "Keltner Multiplier";
            IndParam.NumParam[3].Value   = 1.5;
            IndParam.NumParam[3].Min     = 1;
            IndParam.NumParam[3].Max     = 10;
            IndParam.NumParam[3].Point   = 1;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "Average True Range Multiplier for the Keltner Channel.";

            IndParam.NumParam[4].Caption = "Level";
            IndParam.NumParam[4].Value   = 0;
            IndParam.NumParam[4].Min     = 0;
            IndParam.NumParam[4].Max     = 100;
            IndParam.NumParam[4].Point   = 0;
            IndParam.NumParam[4].Enabled = true;
            IndParam.NumParam[4].ToolTip = "Level Line for BB Squeeze Histogram.";

            IndParam.NumParam[5].Caption = "Fire Line";
            IndParam.NumParam[5].Value   = 15;
            IndParam.NumParam[5].Min     = 1;
            IndParam.NumParam[5].Max     = 30;
            IndParam.NumParam[5].Point   = 0;
            IndParam.NumParam[5].Enabled = true;
            IndParam.NumParam[5].ToolTip = "Adjust height of Fire Line in Separated Chart. \nTo help make visible only, does not affect signals.";

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
            int    iBollPrd  = (int)IndParam.NumParam[0].Value;
            double dBollMult = IndParam.NumParam[1].Value;
			int iKeltPrd = (int)IndParam.NumParam[2].Value;
			double dKeltMult = IndParam.NumParam[3].Value;
			double dLevel = IndParam.NumParam[4].Value * Point;
			double dFireLine = IndParam.NumParam[5].Value * Point;

            int    iPrvs  = IndParam.CheckParam[0].Checked ? 1 : 0;
			
            // Calculation
            int iFirstBar = Math.Max (iBollPrd*2, iKeltPrd) + 1;
	
			double[] adRegr = new double[Bars];
			double[] adAtr = new double[Bars];
			double[] adMA_Boll = new double[Bars];
			double[] adStd = new double[Bars];
			double[] adFire = new double[Bars];


			adMA_Boll = MovingAverage (iBollPrd, 0, MAMethod.Exponential, Close);

			// Get ATR and StandardDeviation values -- used in original MT4 indicator
			Average_True_Range Atr = new Average_True_Range (SlotTypes.OpenFilter);
			Atr.IndParam.NumParam[0].Value = iKeltPrd;
			Atr.Calculate (SlotTypes.OpenFilter);
			adAtr = Atr.Component[0].Value;

			Standard_Deviation StdDev = new Standard_Deviation (SlotTypes.OpenFilter);
			StdDev.IndParam.NumParam[0].Value = iBollPrd;
			StdDev.Calculate (SlotTypes.OpenFilter);
			adStd = StdDev.Component[0].Value;

			// fill Linear Regression values, not built in indicator
			// ported from MQ 4 code

			// these values remains constant, get outside for loop for performance
			double SumBars = 0;
			double SumSqrBars = 0;
			SumBars = iBollPrd * (iBollPrd-1) * 0.5;
			SumSqrBars = (iBollPrd - 1) * iBollPrd * (2 * iBollPrd - 1)/6;
			double Num2 = SumBars * SumBars-iBollPrd * SumSqrBars;
			
            for (int iBar = iFirstBar; iBar < Bars; iBar++)
            {

				double SumY = 0;
				double Sum1 = 0;
				double Sum2 = 0;
				double Slope = 0;

				for (int x = 0; x < iBollPrd; x++) {
					double HH = double.MinValue;
					double LL = double.MaxValue;
					for (int y = x; y < x+iBollPrd; y++)
					{
						HH = Math.Max(HH, High[iBar-y]);
						LL = Math.Min(LL, Low[iBar-y]);
					}

					double dSumsBegin = (Close[iBar-x]-((HH+LL)/2 + adMA_Boll[iBar-x])/2);
					Sum1 += x* dSumsBegin;
					SumY += dSumsBegin;
				}

				Sum2 = SumBars * SumY;
				double Num1 = iBollPrd * Sum1 - Sum2;

				if (Num2 != 0.0)  { 
					Slope = Num1/Num2; 
				} else { 
					Slope = 0; 
				}

				double Intercept = (SumY - Slope*SumBars) /iBollPrd;
				adRegr[iBar] = Intercept + Slope * (iBollPrd-1);

				double diff = adAtr[iBar] * dKeltMult;
				double std = adStd[iBar];
				double bbs = (dBollMult * std) / diff;
				if (bbs < 1)
				{
					adFire[iBar] = -dFireLine;
				}
				else {
					adFire[iBar] = dFireLine;
				}

            }
            
            // Saving the components
            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp();
            Component[0].CompName  = "BB Squeeze";
            Component[0].DataType  = IndComponentType.IndicatorValue;
            Component[0].ChartType = IndChartType.Histogram;
            Component[0].FirstBar  = iFirstBar;
            Component[0].Value     = adRegr;

            Component[1] = new IndicatorComp();
            Component[1].CompName  = "Fire";
            Component[1].DataType  = IndComponentType.IndicatorValue;
            Component[1].ChartType = IndChartType.Line;
			Component[1].ChartColor = Color.Blue;
            Component[1].FirstBar  = iFirstBar;
            Component[1].Value     = adFire;

			
			Component[2] = new IndicatorComp();
            Component[2].ChartType = IndChartType.NoChart;
            Component[2].FirstBar  = iFirstBar;
            Component[2].Value     = new double[Bars];

            Component[3] = new IndicatorComp();
            Component[3].ChartType = IndChartType.NoChart;
            Component[3].FirstBar  = iFirstBar;
            Component[3].Value     = new double[Bars];

            // Sets the Component's type
			if (slotType == SlotTypes.OpenFilter)
            {
                Component[2].DataType = IndComponentType.AllowOpenLong;
                Component[2].CompName = "Is long entry allowed";
                Component[3].DataType = IndComponentType.AllowOpenShort;
                Component[3].CompName = "Is short entry allowed";
            }
            else if (slotType == SlotTypes.CloseFilter)
            {
                Component[2].DataType = IndComponentType.ForceCloseLong;
                Component[2].CompName = "Close out long position";
                Component[3].DataType = IndComponentType.ForceCloseShort;
                Component[3].CompName = "Close out short position";
            }


			if (slotType == SlotTypes.OpenFilter || slotType == SlotTypes.CloseFilter) {
				// Calculation of the logic
				IndicatorLogic indLogic = IndicatorLogic.It_does_not_act_as_a_filter;

				// placeholder components for storing long / short values
				IndicatorComp icBBLong = new IndicatorComp();
				IndicatorComp icBBShort = new IndicatorComp();
				IndicatorComp icFire = new IndicatorComp();

				switch (IndParam.ListParam[0].Text)
				{
					case "The BB Squeeze is higher than the level line and Fire is on":
						icBBLong.Value = new double[Bars];
						icBBShort.Value = new double[Bars];
						icFire.Value = new double[Bars];
						OscillatorLogic(iFirstBar, iPrvs, adRegr, dLevel, -dLevel, ref icBBLong, ref icBBShort, IndicatorLogic.The_indicator_is_higher_than_the_level_line);
						NoDirectionOscillatorLogic(iFirstBar, iPrvs, adFire, 0, ref icFire, IndicatorLogic.The_indicator_is_higher_than_the_level_line);
						for (int iBar = iFirstBar; iBar < Bars; iBar++)
						{
							Component[2].Value[iBar] = (icBBLong.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
							Component[3].Value[iBar] = (icBBShort.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
						}
						if (dLevel != 0) {
							SpecialValues = new double[2] { dLevel, -dLevel };
						}
						break;

					case "The BB Squeeze is higher than the level line and Fire is off":
						icBBLong.Value = new double[Bars];
						icBBShort.Value = new double[Bars];
						icFire.Value = new double[Bars];
						OscillatorLogic(iFirstBar, iPrvs, adRegr, dLevel, -dLevel, ref icBBLong, ref icBBShort, IndicatorLogic.The_indicator_is_higher_than_the_level_line);
						NoDirectionOscillatorLogic(iFirstBar, iPrvs, adFire, 0, ref icFire, IndicatorLogic.The_indicator_is_lower_than_the_level_line);
						for (int iBar = iFirstBar; iBar < Bars; iBar++)
						{
							Component[2].Value[iBar] = (icBBLong.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
							Component[3].Value[iBar] = (icBBShort.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
						}
						if (dLevel != 0) {
							SpecialValues = new double[2] { dLevel, -dLevel };
						}
						break;

					case "The BB Squeeze is lower than the level line and Fire is on":
						icBBLong.Value = new double[Bars];
						icBBShort.Value = new double[Bars];
						icFire.Value = new double[Bars];
						OscillatorLogic(iFirstBar, iPrvs, adRegr, dLevel, -dLevel, ref icBBLong, ref icBBShort, IndicatorLogic.The_indicator_is_lower_than_the_level_line);
						NoDirectionOscillatorLogic(iFirstBar, iPrvs, adFire, 0, ref icFire, IndicatorLogic.The_indicator_is_higher_than_the_level_line);
						for (int iBar = iFirstBar; iBar < Bars; iBar++)
						{
							Component[2].Value[iBar] = (icBBLong.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
							Component[3].Value[iBar] = (icBBShort.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
						}
						if (dLevel != 0) {
							SpecialValues = new double[2] { dLevel, -dLevel };
						}
						break;

					case "The BB Squeeze is lower than the level line and Fire is off":
						icBBLong.Value = new double[Bars];
						icBBShort.Value = new double[Bars];
						icFire.Value = new double[Bars];
						OscillatorLogic(iFirstBar, iPrvs, adRegr, dLevel, -dLevel, ref icBBLong, ref icBBShort, IndicatorLogic.The_indicator_is_lower_than_the_level_line);
						NoDirectionOscillatorLogic(iFirstBar, iPrvs, adFire, 0, ref icFire, IndicatorLogic.The_indicator_is_lower_than_the_level_line);
						for (int iBar = iFirstBar; iBar < Bars; iBar++)
						{
							Component[2].Value[iBar] = (icBBLong.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
							Component[3].Value[iBar] = (icBBShort.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
						}
						if (dLevel != 0) {
							SpecialValues = new double[2] { dLevel, -dLevel };
						}
						break;

					case "The BB Squeeze is higher than the level line":						
						OscillatorLogic(iFirstBar, iPrvs, adRegr, dLevel, -dLevel, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_is_higher_than_the_level_line);
						if (dLevel != 0) {
							SpecialValues = new double[2] { dLevel, -dLevel };
						}
						break;

					case "The BB Squeeze is lower than the level line":
						OscillatorLogic(iFirstBar, iPrvs, adRegr, dLevel, -dLevel, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_is_lower_than_the_level_line);
						if (dLevel != 0) {
							SpecialValues = new double[2] { dLevel, -dLevel };
						}
						break;

					case "The BB Squeeze rises and Fire is on":
						icBBLong.Value = new double[Bars];
						icBBShort.Value = new double[Bars];
						icFire.Value = new double[Bars];
						OscillatorLogic(iFirstBar, iPrvs, adRegr, dLevel, -dLevel, ref icBBLong, ref icBBShort, IndicatorLogic.The_indicator_rises);
						NoDirectionOscillatorLogic(iFirstBar, iPrvs, adFire, 0, ref icFire, IndicatorLogic.The_indicator_is_higher_than_the_level_line);
						for (int iBar = iFirstBar; iBar < Bars; iBar++)
						{
							Component[2].Value[iBar] = (icBBLong.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
							Component[3].Value[iBar] = (icBBShort.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
						}
						if (dLevel != 0) {
							SpecialValues = new double[2] { dLevel, -dLevel };
						}
						break;

					case "The BB Squeeze rises and Fire is off":
						icBBLong.Value = new double[Bars];
						icBBShort.Value = new double[Bars];
						icFire.Value = new double[Bars];
						OscillatorLogic(iFirstBar, iPrvs, adRegr, dLevel, -dLevel, ref icBBLong, ref icBBShort, IndicatorLogic.The_indicator_rises);
						NoDirectionOscillatorLogic(iFirstBar, iPrvs, adFire, 0, ref icFire, IndicatorLogic.The_indicator_is_lower_than_the_level_line);
						for (int iBar = iFirstBar; iBar < Bars; iBar++)
						{
							Component[2].Value[iBar] = (icBBLong.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
							Component[3].Value[iBar] = (icBBShort.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
						}
						if (dLevel != 0) {
							SpecialValues = new double[2] { dLevel, -dLevel };
						}
						break;

					case "The BB Squeeze falls and Fire is on":
						icBBLong.Value = new double[Bars];
						icBBShort.Value = new double[Bars];
						icFire.Value = new double[Bars];
						OscillatorLogic(iFirstBar, iPrvs, adRegr, dLevel, -dLevel, ref icBBLong, ref icBBShort, IndicatorLogic.The_indicator_falls);
						NoDirectionOscillatorLogic(iFirstBar, iPrvs, adFire, 0, ref icFire, IndicatorLogic.The_indicator_is_higher_than_the_level_line);
						for (int iBar = iFirstBar; iBar < Bars; iBar++)
						{
							Component[2].Value[iBar] = (icBBLong.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
							Component[3].Value[iBar] = (icBBShort.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
						}
						if (dLevel != 0) {
							SpecialValues = new double[2] { dLevel, -dLevel };
						}
						break;

					case "The BB Squeeze falls and Fire is off":
						icBBLong.Value = new double[Bars];
						icBBShort.Value = new double[Bars];
						icFire.Value = new double[Bars];
						OscillatorLogic(iFirstBar, iPrvs, adRegr, dLevel, -dLevel, ref icBBLong, ref icBBShort, IndicatorLogic.The_indicator_falls);
						NoDirectionOscillatorLogic(iFirstBar, iPrvs, adFire, 0, ref icFire, IndicatorLogic.The_indicator_is_lower_than_the_level_line);
						for (int iBar = iFirstBar; iBar < Bars; iBar++)
						{
							Component[2].Value[iBar] = (icBBLong.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
							Component[3].Value[iBar] = (icBBShort.Value[iBar] == 1 && icFire.Value[iBar] == 1) ? 1.0 : 0.0;
						}
						if (dLevel != 0) {
							SpecialValues = new double[2] { dLevel, -dLevel };
						}
						break;


					case "The BB Squeeze rises":
						OscillatorLogic(iFirstBar, iPrvs, adRegr, 0, 0, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_rises);
						break;

					case "The BB Squeeze falls":
						OscillatorLogic(iFirstBar, iPrvs, adRegr, 0, 0, ref Component[2], ref Component[3], IndicatorLogic.The_indicator_falls);
						break;

					case "Fire is on":
						NoDirectionOscillatorLogic(iFirstBar, iPrvs, adFire, 0, ref Component[2], IndicatorLogic.The_indicator_is_higher_than_the_level_line);
						Component[3].Value = Component[2].Value;
						break;

					case "Fire is off":
						NoDirectionOscillatorLogic(iFirstBar, iPrvs, adFire, 0, ref Component[2], IndicatorLogic.The_indicator_is_lower_than_the_level_line);
						Component[3].Value = Component[2].Value;
						break;

					case "Fire changes from on to off":
						NoDirectionOscillatorLogic(iFirstBar, iPrvs, adFire, 0, ref Component[2], IndicatorLogic.The_indicator_crosses_the_level_line_downward);
						Component[3].Value = Component[2].Value;
						break;
					
					case "Fire changes from off to on":
						NoDirectionOscillatorLogic(iFirstBar, iPrvs, adFire, 0, ref Component[2], IndicatorLogic.The_indicator_crosses_the_level_line_upward);
						Component[3].Value = Component[2].Value;
						break;

					case "Draw only, no Entry or Exit signals":
						Component[2].DataType = IndComponentType.NotDefined;
						Component[2].CompName = "Visual Only";
						Component[3].DataType = IndComponentType.NotDefined;
						Component[3].CompName = "Visual Only";
						break;

					default:
						break;
				}

				OscillatorLogic(iFirstBar, iPrvs, adRegr, 0, 0, ref Component[2], ref Component[3], indLogic);
			}

			return;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            int    nBars       = (int) IndParam.NumParam[0].Value;
            string sLevelLong  = IndParam.NumParam[1].ValueToString;
            string sLevelShort = "-" + sLevelLong;

			EntryFilterLongDescription  = "the " + ToString() + " ";
			EntryFilterShortDescription = "the " + ToString() + " ";
			ExitFilterLongDescription   = "the " + ToString() + " ";
			ExitFilterShortDescription  = "the " + ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "The BB Squeeze is higher than the level line and Fire is on":
                    EntryFilterLongDescription  += "is higher than " + sLevelLong + " and Fire is on";
                    EntryFilterShortDescription += "is lower than " + sLevelShort + " and Fire is on";
                    ExitFilterLongDescription   += "is higher than " + sLevelLong + " and Fire is on";
                    ExitFilterShortDescription  += "is lower than " + sLevelShort + " and Fire is on";
					break;

				case "The BB Squeeze is higher than the level line and Fire is off":
                    EntryFilterLongDescription  += "is higher than " + sLevelLong + " and Fire is off";
                    EntryFilterShortDescription += "is lower than " + sLevelShort + " and Fire is off";
                    ExitFilterLongDescription   += "is higher than " + sLevelLong + " and Fire is off";
                    ExitFilterShortDescription  += "is lower than " + sLevelShort + " and Fire is off";
					break;

				case "The BB Squeeze is lower than the level line and Fire is on":
                    EntryFilterLongDescription  += "is lower than " + sLevelLong + " and Fire is on";
                    EntryFilterShortDescription += "is higher than " + sLevelShort + " and Fire is on";
                    ExitFilterLongDescription   += "is lower than " + sLevelLong + " and Fire is on";
                    ExitFilterShortDescription  += "is higher than " + sLevelShort + " and Fire is on";
					break;

                case "The BB Squeeze is lower than the level line and Fire is off":
                    EntryFilterLongDescription  += "is lower than " + sLevelLong + " and Fire is off";
                    EntryFilterShortDescription += "is higher than " + sLevelShort + " and Fire is off";
                    ExitFilterLongDescription   += "is lower than " + sLevelLong + " and Fire is off";
                    ExitFilterShortDescription  += "is higher than " + sLevelShort + " and Fire is off";
					break;

				case "The BB Squeeze is higher than the level line":
                    EntryFilterLongDescription  += "is higher than " + sLevelLong;
                    EntryFilterShortDescription += "is lower than " + sLevelShort;
                    ExitFilterLongDescription   += "is higher than " + sLevelLong;
                    ExitFilterShortDescription  += "is lower than " + sLevelShort;
					break;

				case "The BB Squeeze is lower than the level line":
					EntryFilterLongDescription  += "is lower than " + sLevelLong;
                    EntryFilterShortDescription += "is higher than " + sLevelShort;
                    ExitFilterLongDescription   += "is lower than " + sLevelLong;
                    ExitFilterShortDescription  += "is higher than " + sLevelShort;
					break;

				case "The BB Squeeze rises and Fire is on":
                    EntryFilterLongDescription  += "rises" + " and Fire is on";
                    EntryFilterShortDescription += "falls" + " and Fire is on";
                    ExitFilterLongDescription   += "rises" + " and Fire is on";
                    ExitFilterShortDescription  += "falls" + " and Fire is on";
					break;

				case "The BB Squeeze rises and Fire is off":
                    EntryFilterLongDescription  += "rises" + " and Fire is off";
                    EntryFilterShortDescription += "falls" + " and Fire is off";
                    ExitFilterLongDescription   += "rises" + " and Fire is off";
                    ExitFilterShortDescription  += "falls" + " and Fire is off";
					break;

				case "The BB Squeeze falls and Fire is on":
					EntryFilterLongDescription  += "falls"+ " and Fire is on";
                    EntryFilterShortDescription += "rises"+ " and Fire is on";
                    ExitFilterLongDescription   += "falls"+ " and Fire is on";
                    ExitFilterShortDescription  += "rises"+ " and Fire is on";
					break;

				case "The BB Squeeze falls and Fire is off":
					EntryFilterLongDescription  += "falls" + " and Fire is off";
                    EntryFilterShortDescription += "rises" + " and Fire is off";
                    ExitFilterLongDescription   += "falls" + " and Fire is off";
                    ExitFilterShortDescription  += "rises" + " and Fire is off";
					break;

				case "The BB Squeeze rises":
                    EntryFilterLongDescription  += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription   += "rises";
                    ExitFilterShortDescription  += "falls";
					break;

				case "The BB Squeeze falls":
					EntryFilterLongDescription  += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription   += "falls";
                    ExitFilterShortDescription  += "rises";
					break;

				case "Fire is on":
					EntryFilterLongDescription  += "Fire is on";
                    EntryFilterShortDescription += "Fire is on";
                    ExitFilterLongDescription   += "Fire is on";
                    ExitFilterShortDescription  += "Fire is on";
					break;

				case "Fire is off":
					EntryFilterLongDescription  += "Fire is off";
                    EntryFilterShortDescription += "Fire is off";
                    ExitFilterLongDescription   += "Fire is off";
                    ExitFilterShortDescription  += "Fire is off";
					break;

				case "Fire changes from on to off":
					EntryFilterLongDescription  += "Fire changes from on to off";
                    EntryFilterShortDescription += "Fire changes from on to off";
                    ExitFilterLongDescription   += "Fire changes from on to off";
                    ExitFilterShortDescription  += "Fire changes from on to off";
					break;

				case "Fire changes from off to on":
					EntryFilterLongDescription  += "Fire changes from off to on";
                    EntryFilterShortDescription += "Fire changes from off to on";
                    ExitFilterLongDescription   += "Fire changes from off to on";
                    ExitFilterShortDescription  += "Fire changes from off to on";
					break;

				case "Draw only, no Entry or Exit signals":
					EntryFilterLongDescription  = "Draw only, no Entry or Exit signals";
                    EntryFilterShortDescription = "Draw only, no Entry or Exit signals";
                    ExitFilterLongDescription   = "Draw only, no Entry or Exit signals";
                    ExitFilterShortDescription  = "Draw only, no Entry or Exit signals";
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
                "Boll n=" + IndParam.NumParam[0].ValueToString + ", " + // Bollinger period
				"mult=" + IndParam.NumParam[1].ValueToString + ", " + // Bollinger Multiplier
                "Kelt n="+ IndParam.NumParam[2].ValueToString + "," +   // Keltner Period
				"mult="+ IndParam.NumParam[3].ValueToString + "," +   // Keltner Multiplier
				"level="+ IndParam.NumParam[4].ValueToString + ")";   // level

            return sString;
        }
    }
}