using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MultiQueueModels;
using MultiQueueTesting;
using System.IO;

namespace MultiQueueSimulation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            process();
        }
        public void process()
        {
            List<TimeDistribution> Customer = new List<TimeDistribution>();
            List<TimeDistribution> Able = new List<TimeDistribution>();
            List<TimeDistribution> Baker = new List<TimeDistribution>();
            //Probability(Customer, "Customer.txt");
            //Probability(Able, "Able.txt");
            //Probability(Baker, "Baker.txt");
            //StreamWriter sw = new StreamWriter("output");
            //sw.WriteLine(Customer);
            Server serv = new Server();
            List<SimulationCase> Scase = new List<SimulationCase>();
            PerformanceMeasures PM = new PerformanceMeasures();
            CustomerServ(Scase,"Server.txt");
            Serv(serv, Scase);
            PerMeas(PM, Scase);
        }
        public void Probability(List<TimeDistribution> Customer, String Path_file)
        {
            
            string[] lines = File.ReadAllLines(Path_file);
            string[] customer_time = lines[0].Split(' ');
            string[] customer_Probability = lines[1].Split(' ');
            int SIZE = customer_time.Length;
            for (int i = 0; i < SIZE; i++)
            {
                Customer[i].Time = Convert.ToInt16(customer_time[i]);
                Customer.ElementAt(i).Probability = Convert.ToDecimal(customer_Probability[i]);
                if (i == 0)
                {
                    Customer[i].CummProbability = Customer[i].Probability;
                    Customer[i].MinRange = 0;
                }
                else
                {
                    Customer[i].CummProbability = Customer[i].Probability + Customer[i - 1].CummProbability;
                    Customer[i].MinRange = Customer[i - 1].MaxRange + 1;
                }
                Customer[i].MaxRange = Convert.ToInt16(Customer[i].CummProbability) * 100;
            }
        }
        public void Serv(Server serv, List<SimulationCase> Scase)
        {
            //calc the idel time
            int idel_time = 0;
            for (int i = 1; i < Scase.Count; i++)
            {
                idel_time += Scase[i].StartTime - Scase[i - 1].EndTime; 
            }
            serv.IdleProbability = idel_time / Scase[Scase.Count - 1].EndTime;
            serv.AverageServiceTime = Scase[Scase.Count - 1].EndTime / Scase.Count;
            serv.Utilization = (Scase[Scase.Count - 1].EndTime - idel_time)
                / Scase[Scase.Count - 1].EndTime;
        }
        public void CustomerServ(List<SimulationCase> Scase,String Path_file)
        {
            string[] lines = File.ReadAllLines(Path_file);
            // reed from data file
            foreach (string data in lines)
            {
                Scase[Scase.Count].CustomerNumber = Convert.ToInt16(data[0]);
                Scase[Scase.Count].RandomInterArrival = Convert.ToInt16(data[1]);
                Scase[Scase.Count].RandomService = Convert.ToInt16(data[2]);
            }
            for(int i = 0; i < Scase.Count; i++)
            {
                if(Scase[i].CustomerNumber == 1)  // rea 1st customer 
                {
                    Scase[i].InterArrival = 0;
                    Scase[i].ArrivalTime = 0;   
                }
                else
                {
                    for (int j = 0; j < Scase[i].AssignedServer.TimeDistribution.Count; j++)
                    {   //read the rondom interval and get the time interval  
                        if (Scase[i].RandomInterArrival <
                            Scase[i].AssignedServer.TimeDistribution.ElementAt(j).MaxRange)
                        {   
                            Scase[i].InterArrival =               
                                Scase[i].AssignedServer.TimeDistribution.ElementAt(j).Time; break;
                        }
                    }
                    Scase[i].ArrivalTime = Scase[i - 1].ArrivalTime + Scase[i].InterArrival;
                }
                //read start serves time
                if (Scase[i].ArrivalTime > Scase[i - 1].EndTime) 
                    Scase[i].StartTime = Scase[i].ArrivalTime;          
                else
                    Scase[i].StartTime = Scase[i-1].EndTime;
                
                for (int j = 0; j < Scase[i].AssignedServer.TimeDistribution.Count; j++)// <- ده للسرفر طب فين للعاميل
                {   //read the rondom interval and get the time interval for server 
                    if (Scase[i].RandomService <
                        Scase[i].AssignedServer.TimeDistribution.ElementAt(j).MaxRange)
                    {
                        Scase[i].ServiceTime =
                            Scase[i].AssignedServer.TimeDistribution.ElementAt(j).Time; break;
                    }
                }
                Scase[i].EndTime = Scase[i].StartTime + Scase[i].ServiceTime;
                Scase[i].TimeInQueue = Scase[i].StartTime - Scase[i].ArrivalTime;
              }
        }
        public void PerMeas(PerformanceMeasures PM, List<SimulationCase> Scase)
        {
            int w_cust = 0;
            int delay_time = 0;
            for (int i = 1; i < Scase.Count; i++)
            {
                delay_time += Scase[i].TimeInQueue;
                if (Scase[i].TimeInQueue > 0)
                    w_cust += 1;
            }
            PM.AverageWaitingTime = delay_time / Scase.Count;
            PM.WaitingProbability = w_cust / Scase.Count;
            //PM.MaxQueueLength
        }
    }
}