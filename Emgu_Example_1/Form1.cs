//Final Code
//Jacob Enrico
//Started: 4-18-18
//Finished: 4-27-18

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using System.Threading;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.IO.Ports;
using System.Diagnostics;

namespace Emgu_Example_1
{
    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private Thread _captureThread;
        int X, Y;
        int d = 20; // Divider

        SerialPort _serialPort = new SerialPort("COM4", 2400); // Connection to L2 Bot

        // COMMANDS //
        const byte STOP = 0x7F;
        const byte FLOAT = 0x0F;
        const byte FORWARD = 0x6F;
        const byte BACKWARD = 0x5F;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _capture = new VideoCapture();
            _captureThread = new Thread(DisplayWebcam);
            _captureThread.Start();

            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.Two;
            _serialPort.Open();
        }

        private void DisplayWebcam()
        {
            while (_capture.IsOpened)
            {
                Mat frame = _capture.QueryFrame();
                CvInvoke.Resize(frame, frame, pictureBox1.Size);
                Image<Gray, Byte> grayImage = frame.ToImage<Gray, Byte>();
                Image<Hsv, Byte> red = frame.ToImage<Bgr, Byte>().Convert<Hsv, Byte>();
                Image<Gray, Byte>[] redCheck = red.Split();

                redCheck[2] = redCheck[1].Clone().ThresholdBinary(new Gray(65), new Gray(255));

                grayImage = grayImage.ThresholdBinary(new Gray(X), new Gray(Y));

                int leftWhiteCount = 0; // Counting left side white
                for (int xx = 0; xx < (grayImage.Width * 1 / 3); xx++)
                {
                    for (int yy = 0; yy < grayImage.Height * 1 / 2; yy++)
                    {
                        if (grayImage.Data[yy, xx, 0] == Y)
                            leftWhiteCount++;
                    }
                }

                int rightWhiteCount = 0; // Counting right side white
                for (int xxx = (grayImage.Width * 2 / 3); xxx < grayImage.Width; xxx++)
                {
                    for (int yyy = 0; yyy < grayImage.Height * 1 / 2; yyy++)
                    {
                        if (grayImage.Data[yyy, xxx, 0] == Y)
                            rightWhiteCount++;
                    }
                }

                int redCount = 0; // Counting red in middle
                for (int xxxx = (grayImage.Width * 1 / 3); xxxx < (grayImage.Width * 2 / 3); xxxx++)
                {
                    for (int yyyy = 0; yyyy < grayImage.Height * 1 / 2; yyyy++)
                    {
                        if (redCheck[2].Data[yyyy, xxxx, 0] == 255)
                            redCount++;
                    }
                }

                // PIXEL COUNT DISPLAYS //
                String LWC = leftWhiteCount.ToString();
                label2.Invoke(new Action(() => label2.Text = LWC));

                String RWC = rightWhiteCount.ToString();
                label3.Invoke(new Action(() => label3.Text = RWC));

                String RC = redCount.ToString();
                label5.Invoke(new Action(() => label5.Text = RC));

                if (redCount > (((grayImage.Width * 1 / 3) * (grayImage.Height * 1 / 2)) * 6.5  / d)) // Looking for red
                {
                    label4.Invoke(new Action(() => label4.Text = "Turn Around"));

                    byte[] buffer = { 0x01, BACKWARD, FORWARD };
                    _serialPort.Write(buffer, 0, 3);

                    Thread.Sleep(1400);
                }

                else // Running commands
                {
                    if ((leftWhiteCount & rightWhiteCount) >= (((grayImage.Width * 1 / 3) * (grayImage.Height * 1 / 2)) * 1 / d))
                    {
                        label4.Invoke(new Action(() => label4.Text = "Move Forward"));

                        byte[] buffer = { 0x01, FORWARD, FORWARD };
                        _serialPort.Write(buffer, 0, 3);
                    }

                    else if (rightWhiteCount < (((grayImage.Width * 1 / 3) * (grayImage.Height * 1 / 2)) * 1 / (1.5 * d)))
                    {
                        label4.Invoke(new Action(() => label4.Text = "Move Left"));

                        byte[] buffer = { 0x01, STOP, FORWARD };
                        _serialPort.Write(buffer, 0, 3);
                    }

                    else if (leftWhiteCount < (((grayImage.Width * 1 / 3) * (grayImage.Height * 1 / 2)) * 1 / (1.5 * d)))
                    {
                        label4.Invoke(new Action(() => label4.Text = "Move Right"));

                        byte[] buffer = { 0x01, FORWARD, STOP };
                        _serialPort.Write(buffer, 0, 3);
                    }
                }
                //pictureBox1.Image = redCheck[2].ToBitmap(); // For my testing
                pictureBox1.Image = grayImage.Bitmap; // Shows the black and white image
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _captureThread.Abort();
        }

        private void button1_Click(object sender, EventArgs e) // Inputs for "new Gray"
        {
            String x = textBox1.Text;
            String y = textBox2.Text;

            X = int.Parse(x);
            Y = int.Parse(y);

            button1.BackColor = Color.CornflowerBlue;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
