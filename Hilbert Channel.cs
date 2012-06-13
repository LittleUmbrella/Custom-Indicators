// Hilbert Channel Indicator
// Last changed on 2 Mar 2010
// Part of Forex Strategy Builder v2.8.3.5+
// Website http://forexsb.com/
// Copyright (c) 2010 Denny Imanuel - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;
using System.Drawing;

namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Hilbert Channel Indicator
    /// </summary>
    public class Hilbert_Channel : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Hilbert_Channel(SlotTypes slotType)
        {
            // General properties
            IndicatorName = "Hilbert Channel";
            PossibleSlots = SlotTypes.Open | SlotTypes.OpenFilter | SlotTypes.Close | SlotTypes.CloseFilter;
			//SeparatedChart = true;
			CustomIndicator = true;
			
            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            if (slotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Enter long at the Entry Channel",
                    "Enter long at the Exit Channel"
                };
            else if (slotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "The bar opens below the Entry Channel",
                    "The bar opens above the Entry Channel",
                    "The bar opens below the Exit Channel",
                    "The bar opens above the Exit Channel",
                    "The position opens below the Entry Channel",
                    "The position opens above the Entry Channel",
                    "The position opens below the Exit Channel",
                    "The position opens above the Exit Channel",
                    "The bar opens below the Entry Channel after opening above it",
                    "The bar opens above the Entry Channel after opening below it",
                    "The bar opens below the Exit Channel after opening above it",
                    "The bar opens above the Exit Channel after opening below it"
                };
            else if (slotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Exit long at the Entry Channel",
                    "Exit long at the Exit Channel"
                };
            else if (slotType == SlotTypes.CloseFilter)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "The bar closes below the Entry Channel",
                    "The bar closes above the Entry Channel",
                    "The bar closes below the Exit Channel",
                    "The bar closes above the Exit Channel"
                };
            else
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Not Defined"
                };
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of the indicator.";

            IndParam.ListParam[2].Caption  = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[2].Index    = (int)BasePrice.Median;
            IndParam.ListParam[2].Text     = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "The price the central Moving Average is based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "Look Back Value";
            IndParam.NumParam[0].Value   = 15;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 100;
			IndParam.NumParam[0].Point	 = 0;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The Hilbert Channel look back value.";

            IndParam.NumParam[1].Caption = "K Coefficient";
            IndParam.NumParam[1].Value   = 0;
            IndParam.NumParam[1].Min     = 0;
            IndParam.NumParam[1].Max     = 5;
            IndParam.NumParam[1].Point   = 2;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Determines the width of Hilbert Channel.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Checked = PrepareUsePrevBarValueCheckBox(slotType);
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";

            return;
        }

        /// <summary>
        /// Calculates the indicator's components.
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            // Reading the parameters
            BasePrice price    = (BasePrice)IndParam.ListParam[2].Index;
            int    	  iVal     = (int)IndParam.NumParam[0].Value;
            double    dMpl	   = IndParam.NumParam[1].Value;
            int       iPrvs    = IndParam.CheckParam[0].Checked ? 1 : 0;
	
            // Calculation
			double[] adPrice   = Price(price);
			double[] adPeriod  = HilbertPeriod(adPrice);
			double[] adEntryChannel  = new double[Bars];
			double[] adExitChannel  = new double[Bars];
			int iLookBack;
            int iFirstBar = iVal + iPrvs + 2;
			
            for (int iBar = iFirstBar; iBar < Bars; iBar++)
            {
				if (iVal!=0) 		iLookBack = iVal; 
				else 				iLookBack = (int)(dMpl*adPeriod[iBar]);
				if (iLookBack<1) 	iLookBack = 1;
			
				adEntryChannel[iBar] = double.MinValue;
				adExitChannel[iBar] = double.MaxValue;
				for (int iBack=1; iBack<=iLookBack; iBack++)
				{
					if (adEntryChannel[iBar]<High[iBar-iBack]) 	adEntryChannel[iBar] = High[iBar-iBack];
					if (adExitChannel[iBar]>Low[iBar-iBack]) 	adExitChannel[iBar] = Low[iBar-iBack];
				}
            }

            // Saving the components
            Component = new IndicatorComp[4];

            Component[0]            = new IndicatorComp();
            Component[0].CompName   = "Entry Channel";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor = Color.DarkGreen;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adEntryChannel;

            Component[1]            = new IndicatorComp();
            Component[1].CompName   = "Exit Channel";
            Component[1].DataType   = IndComponentType.IndicatorValue;
            Component[1].ChartType  = IndChartType.Line;
            Component[1].ChartColor = Color.DarkRed;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = adExitChannel;

            Component[2] = new IndicatorComp();
            Component[2].ChartType  = IndChartType.NoChart;
            Component[2].FirstBar   = iFirstBar;
            Component[2].Value      = new double[Bars];

            Component[3] = new IndicatorComp();
            Component[3].ChartType  = IndChartType.NoChart;
            Component[3].FirstBar   = iFirstBar;
            Component[3].Value      = new double[Bars];

            // Sets the Component's type.
            if (slotType == SlotTypes.Open)
            {
                Component[2].DataType = IndComponentType.OpenLongPrice;
                Component[2].CompName = "Long position entry price";
                Component[3].DataType = IndComponentType.OpenShortPrice;
                Component[3].CompName = "Short position entry price";
            }
            else if (slotType == SlotTypes.OpenFilter)
            {
                Component[2].DataType = IndComponentType.AllowOpenLong;
                Component[2].CompName = "Is long entry allowed";
                Component[3].DataType = IndComponentType.AllowOpenShort;
                Component[3].CompName = "Is short entry allowed";
            }
            else if (slotType == SlotTypes.Close)
            {
                Component[2].DataType = IndComponentType.CloseLongPrice;
                Component[2].CompName = "Long position closing price";
                Component[3].DataType = IndComponentType.CloseShortPrice;
                Component[3].CompName = "Short position closing price";
            }
            else if (slotType == SlotTypes.CloseFilter)
            {
                Component[2].DataType = IndComponentType.ForceCloseLong;
                Component[2].CompName = "Close out long position";
                Component[3].DataType = IndComponentType.ForceCloseShort;
                Component[3].CompName = "Close out short position";
            }

            if (slotType == SlotTypes.Open || slotType == SlotTypes.Close)
            {
                if (iFirstBar > 1)
                {
                    for (int iBar = 2; iBar < Bars; iBar++)
                    {
                        // Covers the cases when the price can pass through the band without a signal
                        double dValueUp   = adEntryChannel[iBar - iPrvs];     // Current value
                        double dValueUp1  = adEntryChannel[iBar - iPrvs - 1]; // Previous value
                        double dTempValUp = dValueUp;

                        if ((dValueUp1 > High[iBar - 1] && dValueUp < Low[iBar])  || // It jumps below the current bar
                            (dValueUp1 < Low[iBar - 1]  && dValueUp > High[iBar]) || // It jumps above the current bar
                            (Close[iBar - 1] < dValueUp && dValueUp < Open[iBar]) || // Positive gap
                            (Close[iBar - 1] > dValueUp && dValueUp > Open[iBar]))   // Negative gap
                            dTempValUp = Open[iBar];

                        double dValueDown   = adExitChannel[iBar - iPrvs];     // Current value
                        double dValueDown1  = adExitChannel[iBar - iPrvs - 1]; // Previous value
                        double dTempValDown = dValueDown;

                        if ((dValueDown1 > High[iBar - 1] && dValueDown < Low[iBar])  || // It jumps below the current bar
                            (dValueDown1 < Low[iBar - 1]  && dValueDown > High[iBar]) || // It jumps above the current bar
                            (Close[iBar - 1] < dValueDown && dValueDown < Open[iBar]) || // Positive gap
                            (Close[iBar - 1] > dValueDown && dValueDown > Open[iBar]))   // Negative gap
                            dTempValDown = Open[iBar];

                        if (IndParam.ListParam[0].Text == "Enter long at the Entry Channel" ||
                            IndParam.ListParam[0].Text == "Exit long at the Entry Channel")
                        {
                            Component[2].Value[iBar] = dTempValUp;
                            Component[3].Value[iBar] = dTempValDown;
                        }
                        else
                        {
                            Component[2].Value[iBar] = dTempValDown;
                            Component[3].Value[iBar] = dTempValUp;
                        }
                    }
                }
                else
                {
                    for (int iBar = 2; iBar < Bars; iBar++)
                    {
                        if (IndParam.ListParam[0].Text == "Enter long at the Entry Channel" ||
                            IndParam.ListParam[0].Text == "Exit long at the Entry Channel")
                        {
                            Component[2].Value[iBar] = adEntryChannel[iBar - iPrvs];
                            Component[3].Value[iBar] = adExitChannel[iBar - iPrvs];
                        }
                        else
                        {
                            Component[2].Value[iBar] = adExitChannel[iBar - iPrvs];
                            Component[3].Value[iBar] = adEntryChannel[iBar - iPrvs];
                        }
                    }
                }
            }
            else
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "The bar opens below the Entry Channel":
                        BandIndicatorLogic(iFirstBar, iPrvs, adEntryChannel, adExitChannel, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_below_the_Upper_Band);
                        break;

                    case "The bar opens above the Entry Channel":
                        BandIndicatorLogic(iFirstBar, iPrvs, adEntryChannel, adExitChannel, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_above_the_Upper_Band);
                        break;

                    case "The bar opens below the Exit Channel":
                        BandIndicatorLogic(iFirstBar, iPrvs, adEntryChannel, adExitChannel, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_below_the_Lower_Band);
                        break;

                    case "The bar opens above the Exit Channel":
                        BandIndicatorLogic(iFirstBar, iPrvs, adEntryChannel, adExitChannel, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_above_the_Lower_Band);
                        break;

                    case "The bar opens below the Entry Channel after opening above it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adEntryChannel, adExitChannel, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_below_the_Upper_Band_after_opening_above_it);
                        break;

                    case "The bar opens above the Entry Channel after opening below it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adEntryChannel, adExitChannel, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_above_the_Upper_Band_after_opening_below_it);
                        break;

                    case "The bar opens below the Exit Channel after opening above it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adEntryChannel, adExitChannel, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_below_the_Lower_Band_after_opening_above_it);
                        break;

                    case "The bar opens above the Exit Channel after opening below it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adEntryChannel, adExitChannel, ref Component[2], ref Component[3], BandIndLogic.The_bar_opens_above_the_Lower_Band_after_opening_below_it);
                        break;

                    case "The position opens above the Entry Channel":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[2].UsePreviousBar = iPrvs;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The position opens below the Entry Channel":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[2].UsePreviousBar = iPrvs;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The position opens above the Exit Channel":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[2].UsePreviousBar = iPrvs;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The position opens below the Exit Channel":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[2].UsePreviousBar = iPrvs;
                        Component[2].DataType = IndComponentType.Other;
                        Component[3].DataType = IndComponentType.Other;
                        Component[2].ShowInDynInfo = false;
                        Component[3].ShowInDynInfo = false;
                        break;

                    case "The bar closes below the Entry Channel":
                        BandIndicatorLogic(iFirstBar, iPrvs, adEntryChannel, adExitChannel, ref Component[2], ref Component[3], BandIndLogic.The_bar_closes_below_the_Upper_Band);
                        break;

                    case "The bar closes above the Entry Channel":
                        BandIndicatorLogic(iFirstBar, iPrvs, adEntryChannel, adExitChannel, ref Component[2], ref Component[3], BandIndLogic.The_bar_closes_above_the_Upper_Band);
                        break;

                    case "The bar closes below the Exit Channel":
                        BandIndicatorLogic(iFirstBar, iPrvs, adEntryChannel, adExitChannel, ref Component[2], ref Component[3], BandIndLogic.The_bar_closes_below_the_Lower_Band);
                        break;

                    case "The bar closes above the Exit Channel":
                        BandIndicatorLogic(iFirstBar, iPrvs, adEntryChannel, adExitChannel, ref Component[2], ref Component[3], BandIndLogic.The_bar_closes_above_the_Lower_Band);
                        break;

                    default:
                        break;
                }
            }

            return;
		}

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            switch (IndParam.ListParam[0].Text)
            {
                case "Enter long at the Entry Channel":
                    EntryPointLongDescription  = "at the Entry Channel of " + ToString();
                    EntryPointShortDescription = "at the Exit Channel of " + ToString();
                    break;

                case "Enter long at the Exit Channel":
                    EntryPointLongDescription  = "at the Exit Channel of " + ToString();
                    EntryPointShortDescription = "at the Entry Channel of " + ToString();
                    break;

                case "Exit long at the Entry Channel":
                    ExitPointLongDescription  = "at the Entry Channel of " + ToString();
                    ExitPointShortDescription = "at the Exit Channel of " + ToString();
                    break;

                case "Exit long at the Exit Channel":
                    ExitPointLongDescription  = "at the Exit Channel of " + ToString();
                    ExitPointShortDescription = "at the Entry Channel of " + ToString();
                    break;

                case "The bar opens below the Entry Channel":
                    EntryFilterLongDescription  = "the bar opens below the Entry Channel of " + ToString();
                    EntryFilterShortDescription = "the bar opens above the Exit Channel of " + ToString();
                    break;

                case "The bar opens above the Entry Channel":
                    EntryFilterLongDescription  = "the bar opens above the Entry Channel of " + ToString();
                    EntryFilterShortDescription = "the bar opens below the Exit Channel of " + ToString();
                    break;

                case "The bar opens below the Exit Channel":
                    EntryFilterLongDescription  = "the bar opens below the Exit Channel of " + ToString();
                    EntryFilterShortDescription = "the bar opens above the Entry Channel of " + ToString(); 
                    break;

                case "The bar opens above the Exit Channel":
                    EntryFilterLongDescription  = "the bar opens above the Exit Channel of " + ToString(); 
                    EntryFilterShortDescription = "the bar opens below the Entry Channel of " + ToString(); 
                    break;

                case "The bar opens below the Entry Channel after opening above it":
                    EntryFilterLongDescription  = "the bar opens below the Entry Channel of " + ToString() + " after the previous bar has opened above it";
                    EntryFilterShortDescription = "the bar opens above the Exit Channel of " + ToString() + " after the previous bar has opened below it";
                    break;

                case "The bar opens above the Entry Channel after opening below it":
                    EntryFilterLongDescription  = "the bar opens above the Entry Channel of " + ToString() + " after the previous bar has opened below it";
                    EntryFilterShortDescription = "the bar opens below the Exit Channel of " + ToString() + " after the previous bar has opened above it";
                    break;

                case "The bar opens below the Exit Channel after opening above it":
                    EntryFilterLongDescription  = "the bar opens below the Exit Channel of " + ToString() + " after the previous bar has opened above it";
                    EntryFilterShortDescription = "the bar opens above the Entry Channel of " + ToString() + " after the previous bar has opened below it";
                    break;

                case "The bar opens above the Exit Channel after opening below it":
                    EntryFilterLongDescription  = "the bar opens above the Exit Channel of " + ToString() + " after the previous bar has opened below it";
                    EntryFilterShortDescription = "the bar opens below the Entry Channel of " + ToString() + " after the previous bar has opened above it";
                    break;

                case "The bar closes below the Entry Channel":
                    ExitFilterLongDescription  = "the bar closes below the Entry Channel of " + ToString();
                    ExitFilterShortDescription = "the bar closes above the Exit Channel of " + ToString();
                    break;

                case "The bar closes above the Entry Channel":
                    ExitFilterLongDescription  = "the bar closes above the Entry Channel of " + ToString();
                    ExitFilterShortDescription = "the bar closes below the Exit Channel of " + ToString();
                    break;

                case "The bar closes below the Exit Channel":
                    ExitFilterLongDescription  = "the bar closes below the Exit Channel of " + ToString();
                    ExitFilterShortDescription = "the bar closes above the Entry Channel of " + ToString();
                    break;

                case "The bar closes above the Exit Channel":
                    ExitFilterLongDescription  = "the bar closes above the Exit Channel of " + ToString();
                    ExitFilterShortDescription = "the bar closes below the Entry Channel of " + ToString();
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
                IndParam.ListParam[2].Text         + ", " + // Price
                IndParam.NumParam[0].ValueToString + ", " + // Look Back
                IndParam.NumParam[1].ValueToString + ")";   // K Coeff

            return sString;
        }
		
		
		protected static double[] HilbertPeriod(double[] adPrice)
		{
			double[]	adPeriod	= new double[Bars];
			double[]	adSmoother 	= new double[Bars];
			double[]	adDetrender = new double[Bars];
			double[]	Q1 = new double[Bars];
			double[]	I1 = new double[Bars];
			double[]	jI = new double[Bars];
			double[]	jQ = new double[Bars];
			double[]	I2 = new double[Bars];
			double[]	Q2 = new double[Bars];
			double[]	Re = new double[Bars];
			double[]	Im = new double[Bars];
			double X1; double X2; double Y1; double Y2;			

			for(int iBar=6; iBar<Bars; iBar++)
			{				
				adSmoother[iBar]  = (4*adPrice[iBar] + 3*adPrice[iBar-1] + 2*adPrice[iBar-2] + adPrice[iBar-3])/10;
				adDetrender[iBar] = (.25*adSmoother[iBar] + .75*adSmoother[iBar-2] - .75*adSmoother[iBar-4] - .25*adSmoother[iBar-6])*(.046*adPeriod[iBar-1] + .332);
				Q1[iBar] = (.25*adDetrender[iBar] + .75*adDetrender[iBar-2] - .75*adDetrender[iBar-4] - .25*adDetrender[iBar-6])*(.046*adPeriod[iBar-1] + .332);
				I1[iBar] = adDetrender[iBar-3];
				jI[iBar] = .25*I1[iBar] + .75*I1[iBar-2] - .75*I1[iBar-4] - .25*I1[iBar-6];
				jQ[iBar] = .25*Q1[iBar] + .75*Q1[iBar-2] - .75*Q1[iBar-4] - .25*Q1[iBar-6];
				I2[iBar] = I1[iBar] - jQ[iBar];
				Q2[iBar] = Q1[iBar] + jI[iBar];
				I2[iBar] = .15*I2[iBar] + .85*I2[iBar-1];
				Q2[iBar] = .15*Q2[iBar] + .85*Q2[iBar-1];
				X1 = I2[iBar]*I2[iBar-1];
				X2 = I2[iBar]*Q2[iBar-1];
				Y1 = Q2[iBar]*Q2[iBar-1];
				Y2 = Q2[iBar]*I2[iBar-1];
				Re[iBar] = X1 + Y1;
				Im[iBar] = X2 - Y2;
				Re[iBar] = .2*Re[iBar] + .8*Re[iBar-1];
				Im[iBar] = .2*Im[iBar] + .8*Im[iBar-1];
				if (Im[iBar]!=0 && Re[iBar]!=0) 			adPeriod[iBar] = (2*Math.PI)/(Math.Atan(Im[iBar]/Re[iBar]));
				if (adPeriod[iBar]>1.5*adPeriod[iBar-1]) 	adPeriod[iBar] = 1.5*adPeriod[iBar-1];
				if (adPeriod[iBar]<.67*adPeriod[iBar-1]) 	adPeriod[iBar] = .67*adPeriod[iBar-1];
				if (adPeriod[iBar]<6)				 		adPeriod[iBar] = 6;
				if (adPeriod[iBar]>50) 						adPeriod[iBar] = 50;
				adPeriod[iBar] = .2*adPeriod[iBar] + .8*adPeriod[iBar-1];
			}
			return adPeriod;
		}
    }
}
