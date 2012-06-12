// Ichimoku_Kinko_Hyo (WTF v4)
// modified to WTF
// last modified 12/1/2011


using System;
using System.Drawing;
 
namespace Forex_Strategy_Builder
{
    /// <summary>
    /// Ichimoku Kinko Hyo indicator 
	///WTF  v 4 -- swap DataBars and hBars for significant performance improvement
		///     -- replace comment "modification beta 2" with "modification version 4"
		///     -- comment out returns for Indicator Logic Functions
    /// </summary>
    public class Ichimoku_Kinko_Hyo_WTF : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Ichimoku_Kinko_Hyo_WTF(SlotTypes slotType)
        {
            // General properties
            IndicatorName = "Alternative Ichimoku Kinko Hyo v07.1 (WTF v4)";
            PossibleSlots = SlotTypes.Open | SlotTypes.OpenFilter | SlotTypes.Close;
			CustomIndicator = true;
 
            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;
 
            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            if (slotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new string[] {
                    "Enter the market at the Middle line",
                    "Enter the market at the Stop-order line",
                };
            else if (slotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new string[] {
                    "The Middle line rises",
					"The Middle line falls",
                    "The Stop-order line rises",
					"The Stop-order line falls",
                    "The Middle line is higher than the Stop-order line",
					"The Middle line is lower than the Stop-order line",
                    "The Middle line crosses the Stop-order line upward",
					"The Middle line crosses the Stop-order line downward",
                    "The bar opens above the Middle line",
					"The bar opens below the Middle line",
                    "The bar opens above the Stop-order line",
					"The bar opens below the Stop-order line",
                    "The Middle line is above the closing price",
					"The Middle line is below the closing price",
                    "The position opens above the Priority/Overdue line",
                    "The position opens inside or above the Priority/Overdue line",
                    "The Middle line is above the Priority/Overdue line",
					"The Middle line is below the Priority/Overdue line",
                    "The Middle line is inside or above the Priority/Overdue line",
                    "The Stop-order line is above the Priority/Overdue line",
					"The Stop-order line is below the Priority/Overdue line",
                    "The Stop-order line is inside or above the Priority/Overdue line",
                    "The Priority line is higher than the Overdue line",
					"The Priority line is lower than the Overdue line",
                    "The Priority line crosses the Overdue line upward",
					"The Priority line crosses the Overdue line downward",
                };
            else if (slotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new string[] {
                    "Exit the market at the Middle line",
                    "Exit the market at the Stop-order line",
                };
            else
                IndParam.ListParam[0].ItemList = new string[]
                {
                    "Not Defined"
                };
            IndParam.ListParam[0].Index   = 0;
            IndParam.ListParam[0].Text    = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Application of Ichimoku Kinko Hyo.";
 
            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "SSP";
            IndParam.NumParam[0].Value   = 75;
            IndParam.NumParam[0].Min     = 1;
            IndParam.NumParam[0].Max     = 200;
            IndParam.NumParam[0].Point   = 0;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The SSP period.";
 
            IndParam.NumParam[2].Caption = "SSK";
            IndParam.NumParam[2].Value   = 75;
            IndParam.NumParam[2].Min     = 1;
            IndParam.NumParam[2].Max     = 200;
            IndParam.NumParam[2].Point   = 0;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The SSK period.";
 
			IndParam.ListParam[4].Caption = "Wider Time Frame Reference";
			IndParam.ListParam[4].ItemList = new string[] { "Automatic", "5 Minutes", "15 Minutes", "30 Minutes", "1 Hour", "4 Hours", "1 Day", "1 Week"};
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
            int SSP = (int)IndParam.NumParam[0].Value;
            int SSK  = (int)IndParam.NumParam[2].Value;
            //int iSenkou = (int)IndParam.NumParam[4].Value;
            int iPrvs   = IndParam.CheckParam[0].Checked ? 1 : 0;
 
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
                case 0: htfPeriod = DataPeriods.hour1; break;
				case 1: htfPeriod = DataPeriods.min5; break;
				case 2: htfPeriod = DataPeriods.min15; break;
				case 3: htfPeriod = DataPeriods.min30; break;
				case 4: htfPeriod = DataPeriods.hour1; break;
				case 5: htfPeriod = DataPeriods.hour4; break;
				case 6: htfPeriod = DataPeriods.day; break;
				case 7: htfPeriod = DataPeriods.week; break;
			}

			int err1 = HigherTimeFrame(Period, htfPeriod, out hIndex, out hBars, out iFrame, out hfHigh, out hfLow, out hfOpen, out hfClose, out hfVolume);
			// not used for this indicator, always uses Highs and Lows
			// int err2 = HigherBasePrice(basePrice, hBars, hfHigh, hfLow, hfOpen, hfClose, out hfPrice);

			//------------------------------------------------------------------------

            // this is never used
			// double[] adMedianPrice = Price(BasePrice.Median);
 
            int iFirstBar = 1 + SSP*2 + SSK;
 
            double[] adTenkanSen = new double[Bars];
            for (int iBar = iFirstBar; iBar < hBars; iBar++)
            {
                double dHighestHigh = double.MinValue;
                double dLowestLow   = double.MaxValue;
                for (int i = 0; i < SSP; i++)
                {
                    if (hfHigh[iBar - i] > dHighestHigh)
                        dHighestHigh = hfHigh[iBar - i];
                    if (hfLow[iBar - i] < dLowestLow)
                        dLowestLow = hfLow[iBar - i];
                }
				double dHighestHigh1 = double.MinValue;
                double dLowestLow1   = double.MaxValue;
                for (int i = SSK; i < SSK+SSP; i++)
                {
                    if (hfHigh[iBar - i] > dHighestHigh1)
                        dHighestHigh1 = hfHigh[iBar - i];
                    if (hfLow[iBar - i] < dLowestLow1)
                        dLowestLow1 = hfLow[iBar - i];
                }
                adTenkanSen[iBar] = ((dHighestHigh + dLowestLow) / 2 + (dHighestHigh1 + dLowestLow1) / 2) / 2;
            }
 
            double[] adKijunSen = new double[Bars];
	//		(gi_112 = Bars - SSP; gi_112 >= 0; gi_112--)
            for (int iBar = iFirstBar; iBar < hBars; iBar++)
			
            {
                double dHighestHigh = double.MinValue;
                double dLowestLow   = double.MaxValue;
				for (int i = 0; i < 1.62 * SSP; i++)
                {
				    if (hfHigh[iBar - i] > dHighestHigh)
                        dHighestHigh = hfHigh[iBar - i];
                    if (hfLow[iBar - i] < dLowestLow)
                        dLowestLow = hfLow[iBar - i];               
                }
				
                adKijunSen[iBar] = (dHighestHigh + dLowestLow) / 2;
            }
 
            double[] adChikouSpan  = new double[Bars];
            for (int iBar = iFirstBar; iBar < hBars; iBar++)
			{
				
				adChikouSpan[iBar] = adTenkanSen[iBar];
            }
            double[] adSenkouSpanA  = new double[Bars];
            for (int iBar = iFirstBar; iBar < hBars; iBar++)
            {
                double dHighestHigh = double.MinValue;
                double dLowestLow   = double.MaxValue;
                for (int i = 0; i < SSP; i++)
                {
                    if (hfHigh[iBar - i] > dHighestHigh)
                        dHighestHigh = hfHigh[iBar - i];
                    if (hfLow[iBar - i] < dLowestLow)
                        dLowestLow = hfLow[iBar - i];
                }
				adSenkouSpanA[iBar] = (dHighestHigh + dLowestLow) / 2;
            }
 
            double[] adSenkouSpanB  = new double[Bars];
            for (int iBar = iFirstBar; iBar < hBars; iBar++)
            {
                double dHighestHigh = double.MinValue;
                double dLowestLow   = double.MaxValue;
                for (int i = SSK; i < SSK+SSP; i++)
                {
                    if (hfHigh[iBar - i] > dHighestHigh)
                        dHighestHigh = hfHigh[iBar - i];
                    if (hfLow[iBar - i] < dLowestLow)
                        dLowestLow = hfLow[iBar - i];
                }
                
                adSenkouSpanB[iBar] = (dHighestHigh + dLowestLow) / 2;
            }
 
             // Convert to Current Time Frame ----------------------------------------------

/// start WTF modfication 2 version 4
			// copy of wider time frame array of values
			double[] hadTenkanSen = new double[Bars];
			double[] hadKijunSen = new double[Bars];
			double[] hadChikouSpan = new double[Bars];
			double[] hadSenkouSpanA = new double[Bars];
			double[] hadSenkouSpanB = new double[Bars];
			adTenkanSen.CopyTo (hadTenkanSen, 0);
			adKijunSen.CopyTo (hadKijunSen, 0);
			adChikouSpan.CopyTo (hadChikouSpan, 0);
			adSenkouSpanA.CopyTo (hadSenkouSpanA, 0);
			adSenkouSpanB.CopyTo (hadSenkouSpanB, 0);
			
			int err3 = CurrentTimeFrame(hIndex, hBars, ref adTenkanSen);
			int err4 = CurrentTimeFrame(hIndex, hBars, ref adKijunSen);			
			int err5 = CurrentTimeFrame(hIndex, hBars, ref adChikouSpan);
			int err6 = CurrentTimeFrame(hIndex, hBars, ref adSenkouSpanA);
			int err7 = CurrentTimeFrame(hIndex, hBars, ref adSenkouSpanB);
			// if any error, return out of calculation and indicator fails silently
			if (err3 == 1 || err4 == 1 || err5 == 1 ||  err6 == 1 || err7 == 1)
			{
				return;
			}
/// end WTF modfication 2 version 4

			//-----------------------------------------------------------------------------	

            // Saving the components
            if (slotType == SlotTypes.OpenFilter)
                Component = new IndicatorComp[7];
            else
                Component = new IndicatorComp[6];
 
            Component[0] = new IndicatorComp();
            Component[0].CompName   = "Middle line";
            Component[0].DataType   = IndComponentType.IndicatorValue;
            Component[0].ChartType  = IndChartType.Line;
            Component[0].ChartColor = Color.Red;
            Component[0].FirstBar   = iFirstBar;
            Component[0].Value      = adTenkanSen;
 
            Component[1] = new IndicatorComp();
            Component[1].CompName   = "Stop-order line";
            Component[1].DataType   = IndComponentType.IndicatorValue;
            Component[1].ChartType  = IndChartType.Line;
            Component[1].ChartColor = Color.Blue;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = adKijunSen;
 
            Component[2] = new IndicatorComp();
            Component[2].CompName   = "middle line";
            Component[2].DataType   = IndComponentType.IndicatorValue;
            Component[2].ChartType  = IndChartType.Line;
            Component[2].ChartColor = Color.Green;
            Component[2].FirstBar   = iFirstBar;
            Component[2].Value      = adChikouSpan;
 
            Component[3] = new IndicatorComp();
            Component[3].CompName   = "Priority line";
            Component[3].DataType   = IndComponentType.IndicatorValue;
            Component[3].ChartType  = IndChartType.CloudUp;
            Component[3].ChartColor = Color.SandyBrown;
            Component[3].FirstBar   = iFirstBar;
            Component[3].Value      = adSenkouSpanA;
 
            Component[4] = new IndicatorComp();
            Component[4].CompName   = "Overdue line";
            Component[4].DataType   = IndComponentType.IndicatorValue;
            Component[4].ChartType  = IndChartType.CloudDown;
            Component[4].ChartColor = Color.Thistle;
            Component[4].FirstBar   = iFirstBar;
            Component[4].Value      = adSenkouSpanB;
 
            Component[5] = new IndicatorComp();
            Component[5].FirstBar = iFirstBar;
            Component[5].Value    = new double[Bars];
            Component[5].DataType = IndComponentType.Other;
 
            if (slotType == SlotTypes.OpenFilter)
            {
                Component[5].CompName = "Is long entry allowed";
                Component[5].DataType = IndComponentType.AllowOpenLong;
 
                Component[6] = new IndicatorComp();
                Component[6].FirstBar = iFirstBar;
                Component[6].Value    = new double[Bars];
                Component[6].CompName = "Is short entry allowed";
                Component[6].DataType = IndComponentType.AllowOpenShort;
            }
 
 /// start WTF modfication 3 version 4
// use wtf values here, then do expansion after this if clause to cover these IndicatorCrosses cases and the OscillatorLogic cases

			// back up Bars value, reset to hBars, for performance improvement in indicator logic function
			int mtfBars = Data.Bars;
			Data.Bars = hBars;
			
/// end WTF modfication 3 version 4			
			
            switch (IndParam.ListParam[0].Text)
            {
                case "Enter the market at the Middle line":
                    Component[5].CompName  = "Middle line entry price";
                    Component[5].DataType  = IndComponentType.OpenPrice;
                    Component[5].ChartType = IndChartType.NoChart;
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadTenkanSen[iBar - iPrvs];
                    }
                    break;
                case "Enter the market at the Stop-order line":
                    Component[5].CompName  = "Stop-order line entry price";
                    Component[5].DataType  = IndComponentType.OpenPrice;
                    Component[5].ChartType = IndChartType.NoChart;
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadKijunSen[iBar - iPrvs];
                    }
                    break;
                case "Exit the market at the Middle line":
                    Component[5].CompName  = "Middle line exit price";
                    Component[5].DataType  = IndComponentType.ClosePrice;
                    Component[5].ChartType = IndChartType.NoChart;
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadTenkanSen[iBar - iPrvs];
                    }
                    break;
                case "Exit the market at the Stop-order line":
                    Component[5].CompName  = "Stop-order line exit price";
                    Component[5].DataType  = IndComponentType.ClosePrice;
                    Component[5].ChartType = IndChartType.NoChart;
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadKijunSen[iBar - iPrvs];
                    }
                    break;
                case "The Middle line rises":
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadTenkanSen[iBar - iPrvs] > hadTenkanSen[iBar - iPrvs - 1] + Sigma() ? 1 : 0;
                        Component[6].Value[iBar] = hadTenkanSen[iBar - iPrvs] < hadTenkanSen[iBar - iPrvs - 1] - Sigma() ? 1 : 0;
                    }
                    break;
				case "The Middle line falls":
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadTenkanSen[iBar - iPrvs] < hadTenkanSen[iBar - iPrvs - 1] + Sigma() ? 1 : 0;
                        Component[6].Value[iBar] = hadTenkanSen[iBar - iPrvs] > hadTenkanSen[iBar - iPrvs - 1] - Sigma() ? 1 : 0;
                    }
                    break;
                case "The Stop-order line rises":
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadKijunSen[iBar - iPrvs] > hadKijunSen[iBar - iPrvs - 1] + Sigma() ? 1 : 0;
                        Component[6].Value[iBar] = hadKijunSen[iBar - iPrvs] < hadKijunSen[iBar - iPrvs - 1] - Sigma() ? 1 : 0;
                    }
                    break;
				case "The Stop-order line falls":
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadKijunSen[iBar - iPrvs] < hadKijunSen[iBar - iPrvs - 1] + Sigma() ? 1 : 0;
                        Component[6].Value[iBar] = hadKijunSen[iBar - iPrvs] > hadKijunSen[iBar - iPrvs - 1] - Sigma() ? 1 : 0;
                    }
                    break;
                case "The Middle line is higher than the Stop-order line":
                    IndicatorIsHigherThanAnotherIndicatorLogic(iFirstBar, iPrvs, hadTenkanSen, hadKijunSen, ref Component[5], ref Component[6]);
                    break;
				case "The Middle line is lower than the Stop-order line":
                    IndicatorIsLowerThanAnotherIndicatorLogic(iFirstBar, iPrvs, hadTenkanSen, hadKijunSen, ref Component[5], ref Component[6]);
                    break;	
                case "The Middle line crosses the Stop-order line upward":
                    IndicatorCrossesAnotherIndicatorUpwardLogic(iFirstBar, iPrvs, hadTenkanSen, hadKijunSen, ref Component[5], ref Component[6]);
                    break;
				case "The Middle line crosses the Stop-order line downward":
                    IndicatorCrossesAnotherIndicatorDownwardLogic(iFirstBar, iPrvs, hadTenkanSen, hadKijunSen, ref Component[5], ref Component[6]);
                    break;
                case "The bar opens above the Middle line":
                    BarOpensAboveIndicatorLogic(iFirstBar, iPrvs, hadTenkanSen, ref Component[5], ref Component[6]);
                    break;
				case "The bar opens below the Middle line":
                    BarOpensBelowIndicatorLogic(iFirstBar, iPrvs, hadTenkanSen, ref Component[5], ref Component[6]);
                    break;	
                case "The bar opens above the Stop-order line":
                    BarOpensAboveIndicatorLogic(iFirstBar, iPrvs, hadKijunSen, ref Component[5], ref Component[6]);
                    break;
				case "The bar opens below the Stop-order line":
                    BarOpensBelowIndicatorLogic(iFirstBar, iPrvs, hadKijunSen, ref Component[5], ref Component[6]);
                    break;
                case "The Middle line is above the closing price":
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadChikouSpan[iBar - iPrvs] > hfClose[iBar - iPrvs] + Sigma() ? 1 : 0;
                        Component[6].Value[iBar] = hadChikouSpan[iBar- iPrvs] < hfClose[iBar - iPrvs] - Sigma() ? 1 : 0;
                    }
                    break;
				case "The Middle line is below the closing price":
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadChikouSpan[iBar - iPrvs] < hfClose[iBar - iPrvs] + Sigma() ? 1 : 0;
                        Component[6].Value[iBar] = hadChikouSpan[iBar- iPrvs] > hfClose[iBar - iPrvs] - Sigma() ? 1 : 0;
                    }
                    break;
 
                case "The position opens above the Priority/Overdue line":
                    for (int iBar = iFirstBar; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = Math.Max(hadSenkouSpanA[iBar], hadSenkouSpanB[iBar]);
                        Component[6].Value[iBar] = Math.Min(hadSenkouSpanA[iBar], hadSenkouSpanB[iBar]);
                    }
                    Component[5].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                    Component[5].DataType = IndComponentType.Other;
                    Component[5].UsePreviousBar = iPrvs;
                    Component[5].ShowInDynInfo  = false;
 
                    Component[6].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                    Component[6].DataType = IndComponentType.Other;
                    Component[6].UsePreviousBar = iPrvs;
                    Component[6].ShowInDynInfo  = false;
                    break;
 
                case "The position opens inside or above the Priority/Overdue line":
                    for (int iBar = iFirstBar; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = Math.Min(hadSenkouSpanA[iBar], hadSenkouSpanB[iBar]);
                        Component[6].Value[iBar] = Math.Max(hadSenkouSpanA[iBar], hadSenkouSpanB[iBar]);
                    }
                    Component[5].PosPriceDependence = PositionPriceDependence.PriceBuyHigher;
                    Component[5].DataType = IndComponentType.Other;
                    Component[5].UsePreviousBar = iPrvs;
                    Component[5].ShowInDynInfo  = false;
 
                    Component[6].PosPriceDependence = PositionPriceDependence.PriceSellLower;
                    Component[6].DataType = IndComponentType.Other;
                    Component[6].UsePreviousBar = iPrvs;
                    Component[6].ShowInDynInfo  = false;
                    break;
 
                case "The Middle line is above the Priority/Overdue line":
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadTenkanSen[iBar - iPrvs] > Math.Max(hadSenkouSpanA[iBar - iPrvs], hadSenkouSpanB[iBar - iPrvs]) + Sigma() ? 1 : 0;
                        Component[6].Value[iBar] = hadTenkanSen[iBar - iPrvs] < Math.Min(hadSenkouSpanA[iBar - iPrvs], hadSenkouSpanB[iBar - iPrvs]) - Sigma() ? 1 : 0;
                    }
                    break;
				case "The Middle line is below the Priority/Overdue line":
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadTenkanSen[iBar - iPrvs] < Math.Max(hadSenkouSpanA[iBar - iPrvs], hadSenkouSpanB[iBar - iPrvs]) + Sigma() ? 1 : 0;
                        Component[6].Value[iBar] = hadTenkanSen[iBar - iPrvs] > Math.Min(hadSenkouSpanA[iBar - iPrvs], hadSenkouSpanB[iBar - iPrvs]) - Sigma() ? 1 : 0;
                    }
                    break;
 
                case "The Middle line is inside or above the Priority/Overdue line":
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadTenkanSen[iBar - iPrvs] > Math.Min(hadSenkouSpanA[iBar - iPrvs], hadSenkouSpanB[iBar - iPrvs]) + Sigma() ? 1 : 0;
                        Component[6].Value[iBar] = hadTenkanSen[iBar - iPrvs] < Math.Max(hadSenkouSpanA[iBar - iPrvs], hadSenkouSpanB[iBar - iPrvs]) - Sigma() ? 1 : 0;
                    }
                    break;
 
                case "The Stop-order line is above the Priority/Overdue line":
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadKijunSen[iBar - iPrvs] > Math.Max(hadSenkouSpanA[iBar - iPrvs], hadSenkouSpanB[iBar - iPrvs]) + Sigma() ? 1 : 0;
                        Component[6].Value[iBar] = hadKijunSen[iBar - iPrvs] < Math.Min(hadSenkouSpanA[iBar - iPrvs], hadSenkouSpanB[iBar - iPrvs]) - Sigma() ? 1 : 0;
                    }
                    break;
				case "The Stop-order line is below the Priority/Overdue line":
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadKijunSen[iBar - iPrvs] < Math.Max(hadSenkouSpanA[iBar - iPrvs], hadSenkouSpanB[iBar - iPrvs]) + Sigma() ? 1 : 0;
                        Component[6].Value[iBar] = hadKijunSen[iBar - iPrvs] > Math.Min(hadSenkouSpanA[iBar - iPrvs], hadSenkouSpanB[iBar - iPrvs]) - Sigma() ? 1 : 0;
                    }
                    break;
 
                case "The Stop-order line is inside or above the Priority/Overdue line":
                    for (int iBar = iFirstBar + iPrvs; iBar < Bars; iBar++)
                    {
                        Component[5].Value[iBar] = hadKijunSen[iBar - iPrvs] > Math.Min(hadSenkouSpanA[iBar - iPrvs], hadSenkouSpanB[iBar - iPrvs]) + Sigma() ? 1 : 0;
                        Component[6].Value[iBar] = hadKijunSen[iBar - iPrvs] < Math.Max(hadSenkouSpanA[iBar - iPrvs], hadSenkouSpanB[iBar - iPrvs]) - Sigma() ? 1 : 0;
                    }
                    break;
 
                case "The Priority line is higher than the Overdue line":
                    IndicatorIsHigherThanAnotherIndicatorLogic(iFirstBar, iPrvs, hadSenkouSpanA, hadSenkouSpanB, ref Component[5], ref Component[6]);
                    break;
				case "The Priority line is lower than the Overdue line":
                    IndicatorIsLowerThanAnotherIndicatorLogic(iFirstBar, iPrvs, hadSenkouSpanA, hadSenkouSpanB, ref Component[5], ref Component[6]);
                    break;
 
                case "The Priority line crosses the Overdue line upward":
                    IndicatorCrossesAnotherIndicatorUpwardLogic(iFirstBar, iPrvs, hadSenkouSpanA, hadSenkouSpanB, ref Component[5], ref Component[6]);
                    break;
				case "The Priority line crosses the Overdue line downward":
                    IndicatorCrossesAnotherIndicatorDownwardLogic(iFirstBar, iPrvs, hadSenkouSpanA, hadSenkouSpanB, ref Component[5], ref Component[6]);
                    break;
 
                default:
                    break;
            }


/// start WTF modfication 4 version 4			

			// resest Bars to real value
			Data.Bars = mtfBars;
		
			// expand component array from wtf to current time frame
			double[] wtfCompValue = Component[5].Value;
			int err8 = CurrentTimeFrame(hIndex, hBars, ref wtfCompValue);
			if (err8 == 1) { return; }
            Component[5].Value = wtfCompValue;

			if (slotType == SlotTypes.OpenFilter)
			{
				wtfCompValue = Component[6].Value;
				int err9 = CurrentTimeFrame(hIndex, hBars, ref wtfCompValue);
				if (err9 == 1) { return; }
				Component[6].Value = wtfCompValue;
			}
/// end WTF modfication 4 version 4

 
            return;
        }
 
        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            switch (IndParam.ListParam[0].Text)
            {
                case "Enter the market at the Middle line":
                    EntryPointLongDescription  = "at the Middle line of " + ToString();
                    EntryPointShortDescription = "at the Middle line of " + ToString();
                    break;
                case "Enter the market at the Stop-order line":
                    EntryPointLongDescription  = "at the Stop-order line of " + ToString();
                    EntryPointShortDescription = "at the Stop-order line of " + ToString();
                    break;
                case "Exit the market at the Middle line":
                    ExitPointLongDescription  = "at the Middle line of " + ToString();
                    ExitPointShortDescription = "at the Middle line of " + ToString();
                    break;
                case "Exit the market at the Stop-order line":
                    ExitPointLongDescription  = "at the Stop-order line of " + ToString();
                    ExitPointShortDescription = "at the Stop-order line of " + ToString();
                    break;
                case "The Middle line rises":
                    EntryFilterLongDescription  = "the Middle line of " + ToString() + " rises";
                    EntryFilterShortDescription = "the Middle line of " + ToString() + " falls";
                    break;
                case "The Stop-order line rises":
                    EntryFilterLongDescription  = "the Stop-order line of " + ToString() + " rises";
                    EntryFilterShortDescription = "the Stop-order line of " + ToString() + " falls";
                    break;
                case "The Middle line is higher than the Stop-order line":
                    EntryFilterLongDescription  = ToString() + " - the Middle line is higher than the Stop-order line";
                    EntryFilterShortDescription = ToString() + " - the Middle line is lower than the Stop-order line";
                    break;
                case "The Middle line crosses the Stop-order line upward":
                    EntryFilterLongDescription  = ToString() + " - the Middle line crosses the Stop-order line upward";
                    EntryFilterShortDescription = ToString() + " - the Middle line crosses the Stop-order line downward";
                    break;
                case "The bar opens above the Middle line":
                    EntryFilterLongDescription  = "the bar opens above the Middle line of " + ToString();
                    EntryFilterShortDescription = "the bar opens below the Middle line of " + ToString();
                    break;
                case "The bar opens above the Stop-order line":
                    EntryFilterLongDescription  = "the bar opens above the Stop-order line of " + ToString();
                    EntryFilterShortDescription = "the bar opens below the Stop-order line of " + ToString();
                    break;
                case "The Middle line is above the closing price":
                    EntryFilterLongDescription  = "the Middle line of " + ToString() + " is above the closing price of the corresponding bar";
                    EntryFilterShortDescription = "the Middle line of " + ToString() + " is below the closing price of the corresponding bar";
                    break;
                case "The position opens above the Priority/Overdue line":
                    EntryFilterLongDescription  = "the position opens above the Priority/Overdue line of " + ToString();
                    EntryFilterShortDescription = "the position opens below the Priority/Overdue line of " + ToString();
                    break;
                case "The position opens inside or above the Priority/Overdue line":
                    EntryFilterLongDescription  = "the position opens inside or above the Priority/Overdue line of " + ToString();
                    EntryFilterShortDescription = "the position opens inside or below the Priority/Overdue line of " + ToString();
                    break;
                case "The Middle line is above the Priority/Overdue line":
                    EntryFilterLongDescription  = ToString() + " - the Middle line is above the Priority/Overdue line";
                    EntryFilterShortDescription = ToString() + " - the Middle line is below the Priority/Overdue line";
                    break;
                case "The Middle line is inside or above the Priority/Overdue line":
                    EntryFilterLongDescription  = ToString() + " - the Middle line is inside or above the Priority/Overdue line";
                    EntryFilterShortDescription = ToString() + " - the Middle line is inside or below the Priority/Overdue line";
                    break;
                case "The Stop-order line is above the Priority/Overdue line":
                    EntryFilterLongDescription  = ToString() + " - the Stop-order line is above the Priority/Overdue line";
                    EntryFilterShortDescription = ToString() + " - the Stop-order line is below the Priority/Overdue line";
                    break;
                case "The Stop-order line is inside or above the Priority/Overdue line":
                    EntryFilterLongDescription  = ToString() + " - the Stop-order line is inside or above the Priority/Overdue line";
                    EntryFilterShortDescription = ToString() + " - the Stop-order line is inside or below the Priority/Overdue line";
                    break;
                case "The Priority line is higher than the Overdue line":
                    EntryFilterLongDescription  = ToString() + " - Priority line is higher than the Overdue line";
                    EntryFilterShortDescription = ToString() + " - Priority line is lower than the Overdue line";
                    break;
                case "The Priority line crosses the Overdue line upward":
                    EntryFilterLongDescription  = ToString() + " - Priority line crosses the Overdue line upward";
                    EntryFilterShortDescription = ToString() + " - Priority line crosses the Overdue line downward";
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
                IndParam.NumParam[0].ValueToString + ", " + // Tenkan
                IndParam.NumParam[2].ValueToString + ", "  + // Kijun
                "WTF=" + IndParam.ListParam[4].Text + ")";  // WTF period
 
            return sString;
        }
    

		
				/// <summary>
        /// Convert current time frame to higher time frame
        /// </summary>
/// start WTF modfication 2 beta 1
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
/// end WTF modfication 2 beta 1
				
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