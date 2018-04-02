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
        private CNeuron cNeuron;

        public MainWindow()
        {
            InitializeComponent();

            Log.onAdditional += UpdateLogControl;

            canWrapper = new UCanWrapper(0, UCanWrapper.eUcanBaudrate.USBCAN_BAUD_500kBit);
            canWrapper.onBDGPDataRcv += DrawGraph;

            cNeuron = new CNeuron();
            double temp1 = cNeuron.Prov(1.235);

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
            }));
        }


    }
}
