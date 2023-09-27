using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.DataVisualization.Charting;
using System.Management;

namespace SP_lab2
{
    public partial class Form1 : Form
    {
        private double[] a;
        private double[] b;
        private int N_a;
        private int N_threads;
        private int K;
        private int Delta_threads;
        private int Delta_K;

        public Form1()
        {
            InitializeComponent();

            chart1.Series.Clear();
            chart1.Series.Add("Single");
            chart1.Series.Add("Multiple");
            chart1.Series["Single"].ChartType = SeriesChartType.Line;
            chart1.Series["Multiple"].ChartType = SeriesChartType.Line;
            chart1.Series["Single"].Color = Color.Red;
            chart1.Series["Multiple"].Color = Color.Blue;
            chart1.Series["Single"].BorderWidth = 3;
            chart1.Series["Multiple"].BorderWidth = 3;
            chart1.ChartAreas[0].AxisX.Title = "Количество потоков";
            chart1.ChartAreas[0].AxisY.Title = "Время, мс";

            chart2.Series.Clear();
            chart2.Series.Add("Single");
            chart2.Series.Add("Multiple");
            chart2.Series["Single"].ChartType = SeriesChartType.Line;
            chart2.Series["Multiple"].ChartType = SeriesChartType.Line;
            chart2.Series["Single"].Color = Color.Red;
            chart2.Series["Multiple"].Color = Color.Blue;
            chart2.Series["Single"].BorderWidth = 3;
            chart2.Series["Multiple"].BorderWidth = 3;
            chart2.ChartAreas[0].AxisX.Title = "Сложность";
            chart2.ChartAreas[0].AxisY.Title = "Время, мс";


        }

        private void InitializeArrays()
        {
            Random random = new Random();
            a = new double[N_a];
            b = new double[N_a];

            for (int i = 0; i < N_a; i++)
            {
                a[i] = random.NextDouble();
            }
        }

        private void ProcessArray(int start, int end, int K)
        {
            for (int i = start; i < end; i++)
            {
                for (int j = 0; j < K; j++)
                {
                    b[i] += Math.Pow(a[i], 1.789);
                }
            }
        }

        private void SingleThreadedProcessing()
        {
            Stopwatch stopwatch = new Stopwatch();
            for (int thr = 1; thr <= N_threads; thr += Delta_threads)
            {
                stopwatch.Reset();
                stopwatch.Start();

                ProcessArray(0, a.Length, K);

                stopwatch.Stop();
                chart1.Series["Single"].Points.AddXY(thr, stopwatch.ElapsedMilliseconds);
            }

            for (int k = 0; k < K; k += Delta_K)
            {
                stopwatch.Reset();
                stopwatch.Start();

                ProcessArray(0, a.Length, k);

                stopwatch.Stop();
                chart2.Series["Single"].Points.AddXY(k, stopwatch.ElapsedMilliseconds);
            }

        }

        private void MultiThreadedProcessing()
        {
            Stopwatch stopwatch = new Stopwatch();
            
            for (int thr = 1; thr <= N_threads; thr += Delta_threads)
            {
                stopwatch.Reset();
                stopwatch.Start();

                Thread[] threads = new Thread[thr];
                int delta = N_a / thr;
                for (int i = 0; i < thr; i++)
                {
                    int start = i * delta;
                    int end = (i + 1) * delta;
                    if (i == thr - 1)
                    {
                        end = N_a;
                    }
                    threads[i] = new Thread(() => ProcessArray(start, end, K));
                    threads[i].Start();
                }
                for (int i = 0; i < thr; i++)
                {
                    threads[i].Join();
                }

                stopwatch.Stop();
                chart1.Series["Multiple"].Points.AddXY(thr, stopwatch.ElapsedMilliseconds);
            }

            for (int k = 0; k < K; k += Delta_K)
            {
                stopwatch.Reset();
                stopwatch.Start();

                Thread[] threads = new Thread[N_threads];
                int delta = N_a / N_threads;
                for (int i = 0; i < N_threads; i++)
                {
                    int start = i * delta;
                    int end = (i + 1) * delta;
                    if (i == N_threads - 1)
                    {
                        end = N_a;
                    }
                    threads[i] = new Thread(() => ProcessArray(start, end, k));
                    threads[i].Start();
                }
                for (int i = 0; i < N_threads; i++)
                {
                    threads[i].Join();
                }

                stopwatch.Stop();
                chart2.Series["Multiple"].Points.AddXY(k, stopwatch.ElapsedMilliseconds);
            }
        }

        private void GetProcessorParameters()
        {
            
            string clockSpeed = "";
            string numberOfCores = "";
            
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                clockSpeed = obj["CurrentClockSpeed"].ToString();
                numberOfCores = obj["NumberOfCores"].ToString();
            }
            
            label6.Text = "Тактовая частота: " + clockSpeed + " МГц";
            label7.Text = "Количество ядер: " + numberOfCores;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            chart1.Series["Single"].Points.Clear();
            chart1.Series["Multiple"].Points.Clear();
            chart2.Series["Single"].Points.Clear();
            chart2.Series["Multiple"].Points.Clear();

            N_a = int.Parse(textBoxNa.Text);
            N_threads = int.Parse(textBoxNThreads.Text);
            K = int.Parse(textBoxK.Text);
            Delta_threads = int.Parse(textBoxDeltaThreads.Text);
            Delta_K = int.Parse(textBoxDeltaK.Text);
            
            InitializeArrays();
            SingleThreadedProcessing();
            MultiThreadedProcessing();
            GetProcessorParameters();
        }
    }
}
