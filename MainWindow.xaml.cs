using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using UcanDotNET;
using Neural_Application.Interfaces;
using Neural_Application.Neuron;

namespace Neural_Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UCanWrapper canWrapper;
        private CNeuron[] cNeuron;
        private CHidden[] hiddenNeurons;

        private int[] inputData = new int[1024];

        private const int HIDDEN_COUNT = 64;
        private const int INPUT_COUNT = 1024;


        public MainWindow()
        {
            InitializeComponent();

            Log.onAdditional += UpdateLogControl;

            canWrapper = new UCanWrapper(0, UCanWrapper.eUcanBaudrate.USBCAN_BAUD_500kBit);
            canWrapper.onBDGPDataRcv += DrawGraph;

            hiddenNeurons = new CHidden[HIDDEN_COUNT];
            for (int i = 0; i < HIDDEN_COUNT; i++)
                hiddenNeurons[i] = new CHidden();

            /*hiddenNeurons[0].uInputWeight[0] = 0.8f;
            hiddenNeurons[0].uInputWeight[1] = 0.2f;

            
            hiddenNeurons[1].uInputWeight[0] = 0.4f;
            hiddenNeurons[1].uInputWeight[1] = 0.9f;
            
            hiddenNeurons[2].uInputWeight[0] = 0.3f;
            hiddenNeurons[2].uInputWeight[1] = 0.5f;


            hiddenNeurons[0].uOutputWeight = 0.3f;
            hiddenNeurons[1].uOutputWeight = 0.5f;
            hiddenNeurons[2].uOutputWeight = 0.9f;*/

        }
        public void UpdateLogControl()
        {
            logListBox.Items.Clear();

            foreach (var Item in Log.GetLog())
            {
                logListBox.Items.Add("[" + Item.category + "] " + Item.message);
            }
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            canWrapper.Stop();
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            canWrapper.Start();
        }
        public void DrawGraph(int[] dataArr)
        {
            

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {

            int Max = dataArr.Max<int>();
            int WndY = Convert.ToInt16(myCanvas.ActualHeight);

            float koefY = 0;

            if (Max > 0)
                koefY = WndY / (float)Max;
            else
                koefY = WndY;


            myCanvas.Children.Clear();

            int oldX = 0, oldY = WndY;
            int Index = 0;

            for (int i = 0; i < dataArr.Length - 1; i+=2)
            {

                Line myLine = new Line();
                myLine.Stroke = System.Windows.Media.Brushes.Red;

                myLine.X1 = oldX;
                myLine.X2 = i ;
                myLine.Y1 = oldY;

                int currentY = (int)(WndY - ((dataArr[i] + dataArr[i + 1]) * 0.5 * koefY));

                myLine.Y2 = currentY;
                // myLine.Y2 = WndY - (dataArr[i] * koefY);

                oldX = (int)myLine.X2;
                oldY = (int)myLine.Y2;

                myCanvas.Children.Add(myLine);

                Index++;

            }
            inputData = dataArr;
            
            //AnalyzeProcess(dataArr);
            
            
            }));
        }

        public void DrawGraph2(float[] dataArr)
        {
            

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {

                float Max = dataArr.Max();
                int WndY = Convert.ToInt16(myCanvas2.ActualHeight);

                float koefY = 0;

                if (Max > 0)
                    koefY = WndY / (float)Max;
                else
                    koefY = WndY;


                myCanvas2.Children.Clear();

                int oldX = 0, oldY = WndY;
                int Index = 0;

                for (int i = 0; i < dataArr.Length - 1; i += 2)
                {

                    Line myLine = new Line();
                    myLine.Stroke = System.Windows.Media.Brushes.Red;

                    myLine.X1 = oldX;
                    myLine.X2 = i;
                    myLine.Y1 = oldY;

                    int currentY = (int)(WndY - ((dataArr[i] + dataArr[i + 1]) * 0.5 * koefY));

                    myLine.Y2 = currentY;
                    // myLine.Y2 = WndY - (dataArr[i] * koefY);

                    oldX = (int)myLine.X2;
                    oldY = (int)myLine.Y2;

                    myCanvas2.Children.Add(myLine);

                    Index++;

                }


            }));
        }

        public void AnalyzeProcess(int[] dataArr)
        {
            float fResult = 0;
            float fHiddenSum = 0;

            for (int i = 0; i < HIDDEN_COUNT; i++)
            {
                hiddenNeurons[i].UpdateInputData(CFunctionsRelease.Normalize(dataArr));
                hiddenNeurons[i].HiddenProcess();

                fHiddenSum += hiddenNeurons[i].HiddenResult();
            }

            fResult = CFunctionsRelease.Sigmoid(fHiddenSum);//calculated value

            resLabel.Content = "Result " + fResult.ToString();

            if (backpropChkBx.IsChecked == true)
            {
                 BackPropagation(0.0f, fHiddenSum, fResult);
            }
        }

        public void BackPropagation(float fTarget,float fHiddenSum,float fCalc)
        {
            float[] fDeltaHiddenSum = new float[HIDDEN_COUNT];
            float[] fNewOutputWeight = new float[HIDDEN_COUNT];
            float[] fNewInputWeight = new float[INPUT_COUNT];

            float fDeltaOutSum = CFunctionsRelease.Derivate(fHiddenSum) * (fTarget - fCalc);

            debugLabel.Content = "";
            debugLabel.Content += fDeltaOutSum.ToString() + Environment.NewLine;
            debugLabel.Content += (fHiddenSum).ToString();

            for(int i = 0;i<HIDDEN_COUNT;i++)
            {
                float fNewOutWeight = fDeltaOutSum / hiddenNeurons[i].HiddenSigmoid();

                fNewOutputWeight[i] = hiddenNeurons[i].uOutputWeight + fNewOutWeight;
            }

            for(int i = 0;i<HIDDEN_COUNT;i++)
            {
                fDeltaHiddenSum[i] = fDeltaOutSum / hiddenNeurons[i].uOutputWeight * CFunctionsRelease.Derivate(hiddenNeurons[i].HiddenSum());
            }

            int iBackInd = HIDDEN_COUNT - 1;

            for (int i = 0; i < HIDDEN_COUNT; i++)
            {
                for (int j = 0; j < INPUT_COUNT; j++)
                {
                    fNewInputWeight[j] = 0;
                    if (hiddenNeurons[i].uInputData[j] != 0)

                    fNewInputWeight[j] = hiddenNeurons[i].uInputWeight[j] + fDeltaHiddenSum[i] / hiddenNeurons[i].uInputData[j];
                    hiddenNeurons[i].uInputWeight[j] = fNewInputWeight[j];
                }
               // hiddenNeurons[i].uInputWeight = fNewInputWeight;
                hiddenNeurons[i].uOutputWeight = fNewOutputWeight[i];

                iBackInd--;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cyclChkBox.IsChecked == true)
            {
                int uCycles = 10000;
                while (uCycles-- > 0)
                    AnalyzeProcess(inputData);
            }
            else
            {
                AnalyzeProcess(inputData);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < INPUT_COUNT; i++)
                inputData[i] = 0;
            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            DrawGraph2(hiddenNeurons[0].uInputWeight);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < INPUT_COUNT; i++)
                inputData[i] = 1;
        }

    }
}
