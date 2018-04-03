using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Neural_Application.Neuron
{
    static class CFunctionsRelease
    {
        static public float Derivate(float x1)
        {
            float fResult = Convert.ToSingle(Math.Exp(-x1) / ((1 + Math.Exp(-x1)) * (1 + Math.Exp(-x1)))); 

            return fResult;
        }
        static public float Sigmoid(float x1)
        {
            float fResult = Convert.ToSingle( 1 / (1 + Math.Exp(-x1)));

            return fResult;
        }
        static public float[] Normalize(int[] inputData)
        {
            float[] fOutpudData = new float[inputData.Length];
            int uMax = inputData.Max();

            if (uMax == 0) uMax = 1;

            float fNormKoef = 1.0f / uMax;

            for (int i = 0; i < inputData.Length; i++)
            {
                fOutpudData[i] = Convert.ToSingle(inputData[i] * fNormKoef); ;
            }
            return fOutpudData;
        }
        static public  double SampleGaussian(Random random, double mean, double stddev)
        {
            // The method requires sampling from a uniform random of (0,1]
            // but Random.NextDouble() returns a sample of [0,1).
            double x1 = 1 - random.NextDouble();
            double x2 = 1 - random.NextDouble();

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return y1 * stddev + mean;
        }
    }

    class CNeuron
    {

        const int HIDDEN_SIZE = 3;

        public float fSignal;

        private float[] uSignalWeight;


        public CNeuron()
        {
            fSignal = 0;

            uSignalWeight = new float[HIDDEN_SIZE];
        }


        public void NeuralProcess(float fData)
        {
           
        }

        public float Derivate(float x1)
        {
            float fResult = 1 * Convert.ToSingle(Math.Exp(- 1 * x1) / ((1.0f + Math.Exp(-1 * x1)) * (1.0f + Math.Exp(-1 * x1)))); 

            return fResult;
        }
        public float Sigmoid(float x1)
        {
            float fResult = Convert.ToSingle( 1.0f / (1.0f + Math.Exp(- 1 * x1)));

            return fResult;
        }
    }

    public class CHidden
    {
        const int HIDDEN_INPUT_SIZE = 1024;

        public float[] uInputData;
        public float[] uInputWeight;

        private float uHiddenSum;
        private float uHiddenData;
        public float uOutputWeight;
        private float fHiddenResult;

        private Random wRnd;

        public CHidden()
        {
            wRnd = new Random();

            uInputData = new float[HIDDEN_INPUT_SIZE];
            uInputWeight = new float[HIDDEN_INPUT_SIZE];

            uHiddenData = 0;
            uHiddenSum = 0;
            fHiddenResult = 0;

            for (int i = 0; i < HIDDEN_INPUT_SIZE; i++)
            {
                uInputWeight[i] = 0.1f;// Convert.ToSingle(CFunctionsRelease.SampleGaussian(wRnd, 0.1, 0.1));
            }
            uOutputWeight = 0.2f;// Convert.ToSingle(CFunctionsRelease.SampleGaussian(wRnd, 0.1, 0.1));
        }

        public void UpdateInputData(float[] fInputData)
        {
            for(int i = 0;i<HIDDEN_INPUT_SIZE;i++)
            {
                uInputData[i] = fInputData[i];
            }
           
        }

        public void HiddenProcess()
        {
           
            uHiddenData = 0;
            uHiddenSum = 0;

            for(int i = 0;i<HIDDEN_INPUT_SIZE;i++){
                uHiddenSum += uInputData[i] * uInputWeight[i];
            }
            uHiddenData = CFunctionsRelease.Sigmoid(uHiddenSum);

          
        }

        public float HiddenResult()
        {
            fHiddenResult = uHiddenData * uOutputWeight;

            return fHiddenResult;
        }

        public float HiddenSigmoid()
        {
            return uHiddenData;
        }
        public float HiddenSum()
        {
            return uHiddenSum;
        }

    }
}
