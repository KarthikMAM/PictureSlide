using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PictureSlide
{
    class GestureRecognition
    {
        //Basic properties
        char prevSample = '\0';
        public BackgroundWorker sampleCollector;
        Thread MainThread;

        /// <summary>
        /// This will initialize the attributes of the gesture recognition class
        /// </summary>
        /// <param name="mainThread">The thread that is using the gesture recognition feature</param>
        public GestureRecognition(Thread mainThread)
        {
            //The sample collector that samples the transitions 
            //in the background
            sampleCollector = new BackgroundWorker();
            sampleCollector.WorkerReportsProgress = true;
            sampleCollector.DoWork += sampler_DoWork;
            sampleCollector.RunWorkerAsync();

            //The main thread that is requesting the gesture feature
            MainThread = mainThread;
        }

        void sampler_DoWork(object sender, DoWorkEventArgs e)
        {
            //Create a new serial port on COM5
            SerialPort ports = new SerialPort("COM3");

            //Set the parameters for the serial data transfer
            ports.BaudRate = 9600;
            ports.Parity = Parity.None;
            ports.StopBits = StopBits.One;
            ports.DataBits = 8;
            ports.Handshake = Handshake.None;
            ports.RtsEnable = true;

            //Create the event handlers to receive the data and open the port
            ports.DataReceived += ports_DataReceived;
            ports.Open();

            //Wait till the main thread completes
            //So that this object is deallocated
            //only after the UI thread finishes its work
            MainThread.Join();
        }

        private void ports_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Get the port from which the data is being sent
            SerialPort sendingPort = (SerialPort)sender;
            char currentSample = sendingPort.ReadExisting()[0];

            //If the current sample and the previous sample are not the same 
            //Then update the progress to the main thread
            if (currentSample != prevSample)
            {
                switch (currentSample)
                {
                    case '0':
                        sampleCollector.ReportProgress(0);
                        break;
                    case '1':
                        sampleCollector.ReportProgress(1);
                        break;
                    case '2':
                        sampleCollector.ReportProgress(2);
                        break;
                }
            }

            //Update the current sample
            prevSample = currentSample;
        }
    }
}
