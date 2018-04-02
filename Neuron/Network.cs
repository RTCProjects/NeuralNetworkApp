using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Neural_Application.Neuron
{
    class CNeuron
    {
        const int neuralDataSize = 1024;

        private float[] uSignalData;
        private float[] uWeightData;

        public CNeuron()
        {
            uSignalData = new float[neuralDataSize];
            uWeightData = new float[neuralDataSize];


        }

        public float NeuronGetMux(int uIndex)
        {
            return uSignalData[uIndex] * uWeightData[uIndex];
        }

        public double Prov(double x1)
        {
            double fResult =  Math.Exp(-x1) / ((1 + Math.Exp(-x1)) * (1 + Math.Exp(-x1))); 

            return fResult;
        }
        
    }
}
