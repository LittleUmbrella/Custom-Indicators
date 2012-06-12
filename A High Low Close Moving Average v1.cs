// A High Low Close MA v1
// Modified Steady Band
// Last changed on 2009-12-15
// Part of Forex Strategy Builder & Forex Strategy Trader
// Website http://forexsb.com/
// Copyright (c) 2006 - 2009 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.
 

 
using System;
using System.Drawing;
 
namespace Forex_Strategy_Builder
{
    /// <summary>
    /// A High Low Close MA v1
    /// </summary>
    public class A_High_Low_Close_MA_v1 : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public A_High_Low_Close_MA_v1(SlotTypes slotType)
        {
            // General properties
            IndicatorName = "A High Low Close MA";
            PossibleSlots = SlotTypes.Open | SlotTypes.OpenFilter | SlotTypes.Close | SlotTypes.CloseFilter;
 
            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;
            IndParam.IndicatorType = TypeOfIndicator.Additional;
            CustomIndicator = true;
 
            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            if (slotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Enter long at the Upper Band",
                    "Enter long at the Lower Band"
                };
            else if (slotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "The bar opens above the Upper Band after opening below it",
                    "The bar opens below the Upper Band after opening above it",
                    "The bar opens above the Lower Band after opening below it",
                    "The bar opens below the Lower Band after opening above it",
                    "The bar opens below the Upper Band",
                    "The bar opens above the Upper Band",
                    "The bar opens below the Lower Band",
                    "The bar opens above the Lower Band",
                    "The position opens below the Upper Band",
                    "The position opens above the Upper Band",
                    "The position opens below the Lower Band",
                    "The position opens above the Lower Band",
                   
                };
 
            else if (slotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Exit long at the Upper Band",
                    "Exit long at the Lower Band"
                };
 
            else if (slotType == SlotTypes.CloseFilter)
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "The bar closes below the Upper Band",
                    "The bar closes above the Upper Band",
                    "The bar closes below the Lower Band",
                    "The bar closes above the Lower Band"
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
 
            IndParam.ListParam[1].Caption  = "Smoothing method of the MA";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[1].Index    = 2;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "The method of smoothing of central MA.";

            IndParam.ListParam[2].Caption  = "Base price High MA";
            IndParam.ListParam[2].ItemList = new string[] { "High" };
            IndParam.ListParam[2].Index    = 0;
            IndParam.ListParam[2].Text     = "High";
            IndParam.ListParam[2].Enabled  = true;
            IndParam.ListParam[2].ToolTip  = "The price the High MA is based on.";

            
            IndParam.ListParam[3].Caption  = "Base price Low MA";
            IndParam.ListParam[3].ItemList = new string[] { "Low" };
            IndParam.ListParam[3].Index    = 0;
            IndParam.ListParam[3].Text     = "Low";
            IndParam.ListParam[3].Enabled  = true;
            IndParam.ListParam[3].ToolTip  = "The price the Low MA is based on.";

            IndParam.ListParam[4].Caption  = "Base price Middle MA";
            IndParam.ListParam[4].ItemList = new string[] { "Close" };
            IndParam.ListParam[4].Index    = 0;
            IndParam.ListParam[4].Text     = "Close";
            IndParam.ListParam[4].Enabled  = true;
            IndParam.ListParam[4].ToolTip  = "The price the Middle MA is based on.";
 
 
            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "MA period";
            IndParam.NumParam[0].Value   = 34;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 2000;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The MA period.";

            IndParam.NumParam[1].Caption = "Addition to High Price";
            IndParam.NumParam[1].Value   = 0;
            IndParam.NumParam[1].Min     = 0;
            IndParam.NumParam[1].Max     = 2000;
            IndParam.NumParam[1].Point   = 0;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "Determines the distance from the High price.";

            IndParam.NumParam[2].Caption = "Subtraction from Low Price";
            IndParam.NumParam[2].Value   = 0;
            IndParam.NumParam[2].Min     = 0;
            IndParam.NumParam[2].Max     = 2000;
            IndParam.NumParam[2].Point   = 0;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "Determines the distance from the Low price.";
 
 
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
            MAMethod  maMethodM = (MAMethod )IndParam.ListParam[1].Index;
            MAMethod maMethodH  = (MAMethod)IndParam.ListParam[1].Index;
            MAMethod maMethodL  = (MAMethod)IndParam.ListParam[1].Index;
            BasePrice priceH    = BasePrice.High;
            BasePrice priceL    = BasePrice.Low;
            BasePrice priceM    = BasePrice.Close;
            int nMAM            = (int)IndParam.NumParam[0].Value;
            int nMAH            = (int)IndParam.NumParam[0].Value;
            int nMAL            = (int)IndParam.NumParam[0].Value;
            double  dMarginH    = IndParam.NumParam[1].Value * Point;
            double  dMarginL    = IndParam.NumParam[2].Value * Point;
            int     iPrvs       = IndParam.CheckParam[0].Checked ? 1 : 0;
 
            // Calculation
            double[] adMAH     = MovingAverage(nMAH, 0, maMethodH, Price(priceH));
            double[] adMAL     = MovingAverage(nMAL, 0, maMethodL, Price(priceL));
            double[] adMAM     = MovingAverage(nMAM, 0, maMethodM, Price(priceM));
			double[] adUpBand  = new double[Bars];
			double[] adDnBand  = new double[Bars];
 
            int iFirstBar = nMAM + iPrvs + 2;
 
            for (int iBar = nMAM; iBar < Bars; iBar++)
            {
                adUpBand[iBar] = adMAH[iBar] + dMarginH;
                adDnBand[iBar] = adMAL[iBar] - dMarginL;
            }
 
            // Saving the components
            Component = new IndicatorComp[5];
 
            Component[0]            = new IndicatorComp();
            Component[0].CompName   = "Upper Band";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor = Color.Blue;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adUpBand;
 
            Component[1]            = new IndicatorComp();
            Component[1].CompName   = "Centre Band";
            Component[1].DataType   = IndComponentType.IndicatorValue;
            Component[1].ChartType  = IndChartType.Line;
            Component[1].ChartColor = Color.Yellow;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = adMAM;
 
            Component[2]            = new IndicatorComp();
            Component[2].CompName   = "Lower Band";
            Component[2].DataType   = IndComponentType.IndicatorValue;
            Component[2].ChartType  = IndChartType.Line;
            Component[2].ChartColor = Color.Blue;
            Component[2].FirstBar   = iFirstBar;
            Component[2].Value      = adDnBand;
 
            Component[3] = new IndicatorComp();
            Component[3].ChartType  = IndChartType.NoChart;
            Component[3].FirstBar   = iFirstBar;
            Component[3].Value      = new double[Bars];
 
            Component[4] = new IndicatorComp();
            Component[4].ChartType  = IndChartType.NoChart;
            Component[4].FirstBar   = iFirstBar;
            Component[4].Value      = new double[Bars];
 
            // Sets the Component's type
            if (slotType == SlotTypes.Open)
            {
                Component[3].DataType = IndComponentType.OpenLongPrice;
                Component[3].CompName = "Long position entry price";
                Component[4].DataType = IndComponentType.OpenShortPrice;
                Component[4].CompName = "Short position entry price";
            }
            else if (slotType == SlotTypes.OpenFilter)
            {
                Component[3].DataType = IndComponentType.AllowOpenLong;
                Component[3].CompName = "Is long entry allowed";
                Component[4].DataType = IndComponentType.AllowOpenShort;
                Component[4].CompName = "Is short entry allowed";
            }
            else if (slotType == SlotTypes.Close)
            {
                Component[3].DataType = IndComponentType.CloseLongPrice;
                Component[3].CompName = "Long position closing price";
                Component[4].DataType = IndComponentType.CloseShortPrice;
                Component[4].CompName = "Short position closing price";
            }
            else if (slotType == SlotTypes.CloseFilter)
            {
                Component[3].DataType = IndComponentType.ForceCloseLong;
                Component[3].CompName = "Close out long position";
                Component[4].DataType = IndComponentType.ForceCloseShort;
                Component[4].CompName = "Close out short position";
            }
 
            if (slotType == SlotTypes.Open || slotType == SlotTypes.Close)
            {
                if (nMAM > 1)
                {
                    for (int iBar = iFirstBar; iBar < Bars; iBar++)
                    {   // Covers the cases when the price can pass through the band without a signal.
                        double dOpen = Open[iBar]; // Current open price
 
                        // Upper band
                        double dValueUp   = adUpBand[iBar - iPrvs];     // Current value
                        double dValueUp1  = adUpBand[iBar - iPrvs - 1]; // Previous value
                        double dTempValUp = dValueUp;
 
                        if ((dValueUp1 > High[iBar - 1] && dValueUp < dOpen) || // The Open price jumps above the indicator
                            (dValueUp1 < Low[iBar -  1] && dValueUp > dOpen) || // The Open price jumps below the indicator
                            (Close[iBar - 1] < dValueUp && dValueUp < dOpen) || // The Open price is in a positive gap
                            (Close[iBar - 1] > dValueUp && dValueUp > dOpen))   // The Open price is in a negative gap
                            dTempValUp = dOpen; // The entry/exit level is moved to Open price
 
                        // Lower band
                        double dValueDown   = adDnBand[iBar - iPrvs];     // Current value
                        double dValueDown1  = adDnBand[iBar - iPrvs - 1]; // Previous value
                        double dTempValDown = dValueDown;
 
                        if ((dValueDown1 > High[iBar - 1] && dValueDown < dOpen) || // The Open price jumps above the indicator
                            (dValueDown1 < Low[iBar  - 1] && dValueDown > dOpen) || // The Open price jumps below the indicator
                            (Close[iBar - 1] < dValueDown && dValueDown < dOpen) || // The Open price is in a positive gap
                            (Close[iBar - 1] > dValueDown && dValueDown > dOpen))   // The Open price is in a negative gap
                            dTempValDown = dOpen; // The entry/exit level is moved to Open price
 
                        if (IndParam.ListParam[0].Text == "Enter long at the Upper Band" ||
                            IndParam.ListParam[0].Text == "Exit long at the Upper Band")
                        {
                            Component[3].Value[iBar] = dTempValUp;
                            Component[4].Value[iBar] = dTempValDown;
                        }
                        else
                        {
                            Component[3].Value[iBar] = dTempValDown;
                            Component[4].Value[iBar] = dTempValUp;
                        }
                    }
                }
                else
                {
                    for (int iBar = 2; iBar < Bars; iBar++)
                    {
                        if (IndParam.ListParam[0].Text == "Enter long at the Upper Band" ||
                            IndParam.ListParam[0].Text == "Exit long at the Upper Band")
                        {
                            Component[3].Value[iBar] = adUpBand[iBar - iPrvs];
                            Component[4].Value[iBar] = adDnBand[iBar - iPrvs];
                        }
                        else
                        {
                            Component[3].Value[iBar] = adDnBand[iBar - iPrvs];
                            Component[4].Value[iBar] = adUpBand[iBar - iPrvs];
                        }
                    }
                }
            }
            else
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "The bar opens below the Upper Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[3], ref Component[4], BandIndLogic.The_bar_opens_below_the_Upper_Band);
                        break;
 
                    case "The bar opens above the Upper Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[3], ref Component[4], BandIndLogic.The_bar_opens_above_the_Upper_Band);
                        break;
 
                    case "The bar opens below the Lower Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[3], ref Component[4], BandIndLogic.The_bar_opens_below_the_Lower_Band);
                        break;
 
                    case "The bar opens above the Lower Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[3], ref Component[4], BandIndLogic.The_bar_opens_above_the_Lower_Band);
                        break;
 
                    case "The bar opens below the Upper Band after opening above it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[3], ref Component[4], BandIndLogic.The_bar_opens_below_the_Upper_Band_after_opening_above_it);
                        break;
 
                    case "The bar opens above the Upper Band after opening below it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[3], ref Component[4], BandIndLogic.The_bar_opens_above_the_Upper_Band_after_opening_below_it);
                        break;
 
                    case "The bar opens below the Lower Band after opening above it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[3], ref Component[4], BandIndLogic.The_bar_opens_below_the_Lower_Band_after_opening_above_it);
                        break;
 
                    case "The bar opens above the Lower Band after opening below it":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[3], ref Component[4], BandIndLogic.The_bar_opens_above_the_Lower_Band_after_opening_below_it);
                        break;
 
                    case "The position opens above the Upper Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[2].UsePreviousBar = iPrvs;
                        Component[3].DataType = IndComponentType.Other;
                        Component[4].DataType = IndComponentType.Other;
                        Component[3].ShowInDynInfo = false;
                        Component[4].ShowInDynInfo = false;
                        break;
 
                    case "The position opens below the Upper Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[2].UsePreviousBar = iPrvs;
                        Component[3].DataType = IndComponentType.Other;
                        Component[4].DataType = IndComponentType.Other;
                        Component[3].ShowInDynInfo = false;
                        Component[4].ShowInDynInfo = false;
                        break;
 
                    case "The position opens above the Lower Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[2].UsePreviousBar = iPrvs;
                        Component[3].DataType = IndComponentType.Other;
                        Component[4].DataType = IndComponentType.Other;
                        Component[3].ShowInDynInfo = false;
                        Component[4].ShowInDynInfo = false;
                        break;
 
                    case "The position opens below the Lower Band":
                        Component[0].PosPriceDependence = PositionPriceDependence.PriceSellHigher;
                        Component[2].PosPriceDependence = PositionPriceDependence.PriceBuyLower;
                        Component[0].UsePreviousBar = iPrvs;
                        Component[2].UsePreviousBar = iPrvs;
                        Component[3].DataType = IndComponentType.Other;
                        Component[4].DataType = IndComponentType.Other;
                        Component[3].ShowInDynInfo = false;
                        Component[4].ShowInDynInfo = false;
                        break;
 
                    case "The bar closes below the Upper Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[3], ref Component[4], BandIndLogic.The_bar_closes_below_the_Upper_Band);
                        break;
 
                    case "The bar closes above the Upper Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[3], ref Component[4], BandIndLogic.The_bar_closes_above_the_Upper_Band);
                        break;
 
                    case "The bar closes below the Lower Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[3], ref Component[4], BandIndLogic.The_bar_closes_below_the_Lower_Band);
                        break;
 
                    case "The bar closes above the Lower Band":
                        BandIndicatorLogic(iFirstBar, iPrvs, adUpBand, adDnBand, ref Component[3], ref Component[4], BandIndLogic.The_bar_closes_above_the_Lower_Band);
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
                case "Enter long at the Upper Band":
                    EntryPointLongDescription  = "at the Upper Band of " + ToString();
                    EntryPointShortDescription = "at the Lower Band of " + ToString();
                    break;
 
                case "Enter long at the Lower Band":
                    EntryPointLongDescription  = "at the Lower Band of " + ToString();
                    EntryPointShortDescription = "at the Upper Band of " + ToString();
                    break;
 
                case "Exit long at the Upper Band":
                    ExitPointLongDescription  = "at the Upper Band of " + ToString();
                    ExitPointShortDescription = "at the Lower Band of " + ToString();
                    break;
 
                case "Exit long at the Lower Band":
                    ExitPointLongDescription  = "at the Lower Band of " + ToString();
                    ExitPointShortDescription = "at the Upper Band of " + ToString();
                    break;
 
                case "The bar opens below the Upper Band":
                    EntryFilterLongDescription  = "the bar opens below the Upper Band of " + ToString();
                    EntryFilterShortDescription = "the bar opens above the Lower Band of " + ToString();
                    break;
 
                case "The bar opens above the Upper Band":
                    EntryFilterLongDescription  = "the bar opens above the Upper Band of " + ToString();
                    EntryFilterShortDescription = "the bar opens below the Lower Band of " + ToString();
                    break;
 
                case "The bar opens below the Lower Band":
                    EntryFilterLongDescription  = "the bar opens below the Lower Band of " + ToString();
                    EntryFilterShortDescription = "the bar opens above the Upper Band of " + ToString();
                    break;
 
                case "The bar opens above the Lower Band":
                    EntryFilterLongDescription  = "the bar opens above the Lower Band of " + ToString();
                    EntryFilterShortDescription = "the bar opens below the Upper Band of " + ToString();
                    break;
 
                case "The position opens above the Upper Band":
                    EntryFilterLongDescription  = "the position opening price is higher than the Upper Band of " + ToString();
                    EntryFilterShortDescription = "the position opening price is lower than the Lower Band of "  + ToString();
                    break;
 
                case "The position opens below the Upper Band":
                    EntryFilterLongDescription  = "the position opening price is lower than the Upper Band of "  + ToString();
                    EntryFilterShortDescription = "the position opening price is higher than the Lower Band of " + ToString();
                    break;
 
                case "The position opens above the Lower Band":
                    EntryFilterLongDescription  = "the position opening price is higher than the Lower Band of " + ToString();
                    EntryFilterShortDescription = "the position opening price is lower than the Upper Band of "  + ToString();
                    break;
 
                case "The position opens below the Lower Band":
                    EntryFilterLongDescription  = "the position opening price is lower than the Lower Band of "  + ToString();
                    EntryFilterShortDescription = "the position opening price is higher than the Upper Band of " + ToString();
                    break;
 
                case "The bar opens below the Upper Band after opening above it":
                    EntryFilterLongDescription  = "the bar opens below the Upper Band of " + ToString() + " after the previous bar has opened above it";
                    EntryFilterShortDescription = "the bar opens above the Lower Band of " + ToString() + " after the previous bar has opened below it";
                    break;
 
                case "The bar opens above the Upper Band after opening below it":
                    EntryFilterLongDescription  = "the bar opens above the Upper Band of " + ToString() + " after the previous bar has opened below it";
                    EntryFilterShortDescription = "the bar opens below the Lower Band of " + ToString() + " after the previous bar has opened above it";
                    break;
 
                case "The bar opens below the Lower Band after opening above it":
                    EntryFilterLongDescription  = "the bar opens below the Lower Band of " + ToString() + " after the previous bar has opened above it";
                    EntryFilterShortDescription = "the bar opens above the Upper Band of " + ToString() + " after the previous bar has opened below it";
                    break;
 
                case "The bar opens above the Lower Band after opening below it":
                    EntryFilterLongDescription  = "the bar opens above the Lower Band of " + ToString() + " after the previous bar has opened below it";
                    EntryFilterShortDescription = "the bar opens below the Upper Band of " + ToString() + " after the previous bar has opened above it";
                    break;
 
                case "The bar closes below the Upper Band":
                    ExitFilterLongDescription  = "the bar closes below the Upper Band of " + ToString();
                    ExitFilterShortDescription = "the bar closes above the Lower Band of " + ToString();
                    break;
 
                case "The bar closes above the Upper Band":
                    ExitFilterLongDescription  = "the bar closes above the Upper Band of " + ToString();
                    ExitFilterShortDescription = "the bar closes below the Lower Band of " + ToString();
                    break;
 
                case "The bar closes below the Lower Band":
                    ExitFilterLongDescription  = "the bar closes below the Lower Band of " + ToString();
                    ExitFilterShortDescription = "the bar closes above the Upper Band of " + ToString();
                    break;
 
                case "The bar closes above the Lower Band":
                    ExitFilterLongDescription  = "the bar closes above the Lower Band of " + ToString();
                    ExitFilterShortDescription = "the bar closes below the Upper Band of " + ToString();
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
                IndParam.ListParam[1].Text         + ", " + // Method
                IndParam.ListParam[2].Text         + ", " + // Price
                IndParam.NumParam[0].ValueToString + ", " + // MA period
                IndParam.NumParam[1].ValueToString + ")";   // Margin in Pips
 
            return sString;
        }
    }
}