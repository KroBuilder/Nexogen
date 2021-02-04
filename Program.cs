using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Text;

namespace Nexogen
{

    /// <summary>
    /// This is a base class for anyithing with IDs
    /// </summary>
    abstract class IDable
    {
        public int ID { get; }
        public IDable()
        {
            ID = 0;
        }
        public IDable(int id)
        {
            ID = id;
        }
        public override string ToString()
        {
            return ID.ToString();
        }
    }

    /// <summary>
    /// This class is about everything connected with the Vehicle
    /// </summary>
    class Vehicle : IDable , IComparable<Vehicle>, IEquatable<Vehicle>
    {
        public int JobTypeCounter { get; set; }
        public List<string> JobTypeList { get; }
        public Vehicle() : base()
        {
            JobTypeCounter = 0;
            JobTypeList = new List<string>();
        }
        public Vehicle(string Row) : base(int.Parse( Row.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]))
        {
            JobTypeCounter = 0;
            JobTypeList = new List<string>();
            var Elements = Row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
             
            for (int i = 1; i < Elements.Length; i++)
            {
                JobTypeList.Add(Elements[i]);
                JobTypeCounter++;
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in JobTypeList)
            {
                sb.Append(item);
                sb.Append(' ');
            }
            return base.ToString() + ' ' + sb.ToString();
        }

        public int CompareTo([AllowNull] Vehicle other)
        {
            if (other == null)
                return 1;
            if (this.JobTypeCounter == other.JobTypeCounter)
                return this.ID.CompareTo(other.ID);
            return this.JobTypeCounter.CompareTo(other.JobTypeCounter);
        }

        public bool Equals([AllowNull] Vehicle other)
        {
            if (other == null)
                return false;
            if (this.JobTypeCounter == other.JobTypeCounter)
                return this.ID.Equals(other.ID);
            return false;
        }
    }

    /// <summary>
    /// This class is about everything connected with the Jobs
    /// </summary>
    class Job : IDable
    {
        public string  JobType { get; set; }
        public Job() : base()
        {
            JobType = string.Empty;
        }
        public Job(string Row) : base(int.Parse(Row.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]))
        {
            JobType = (Row.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
        }
    }

    /// <summary>
    /// This class is about a vehicles without jobs
    /// </summary>
    class FreeVehicle
    {
        public List<Vehicle> Vehicles { get; set; }
        public int Count { get; set; }

        public FreeVehicle()
        {
            Vehicles = new List<Vehicle>();
            Count = 0;
        }
        public void Add (string row)
        {
            Vehicles.Add(new Vehicle(row));
            Count++;
        }
        private void Remove(Vehicle vehicle)
        {
            Vehicles.Remove(vehicle);
            Count--;
        }
        public void Sort()
        {
            Vehicles.Sort();
        }
        public JobsDone Work(Dictionary<string, List<int>> Jobs)
        {
            JobsDone jobsDone = new JobsDone();
            foreach (var Vehicle in Vehicles)
            {
                int maxCounter = 0;
                int index = 0;
                for (int i = 0; i < Vehicle.JobTypeCounter; i++)
                {
                    if(maxCounter < Jobs[Vehicle.JobTypeList[i]].Count)
                    {
                        maxCounter = Jobs[Vehicle.JobTypeList[i]].Count;
                        index = i;
                    }
                }
                if (maxCounter == 0)
                {
                    //todo nincs munka a kocsira
                }
                else
                {
                    //todo van munka a kocsira tegyük le, countereket manageljük
                    jobsDone.VehicleJobList.Add(Vehicle.ID, Jobs[Vehicle.JobTypeList[index]][0]);
                    Jobs[Vehicle.JobTypeList[index]].RemoveAt(0);
                }
            }
            return jobsDone;
        }
    }

    /// <summary>
    /// Output builder
    /// </summary>
    class JobsDone
    {
        public Dictionary<int,int> VehicleJobList { get; set; }

        public JobsDone()
        {
            VehicleJobList = new Dictionary<int, int>();
        }
        public void Add(int VehicleID, int JobID)
        {
            if (VehicleJobList.ContainsKey(VehicleID))
                throw (new Exception("Vehicle's already in use."));
            if (VehicleJobList.ContainsValue(JobID))
                throw (new Exception("Job's already done."));
            VehicleJobList.Add(VehicleID, JobID);
        }
    }


    class Program
    {
        static void Main()
        {
            FreeVehicle freeVehicle = new FreeVehicle();
            Dictionary<string, List<int>> Jobs = new Dictionary<string, List<int>>();
            using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + @"\Iinput.txt", FileMode.Open, FileAccess.Read))
            {
                try
                {
                    StreamReader sr = new StreamReader(fs);
                    int Vehicles = int.Parse(sr.ReadLine());
                    for (int i = 0; i < Vehicles; i++)
                    {
                        freeVehicle.Add(sr.ReadLine());
                    }
                    int JobsCounter = int.Parse(sr.ReadLine());
                    for (int i = 0; i < JobsCounter; i++)
                    {
                        Job job = new Job(sr.ReadLine());
                        if (Jobs.ContainsKey(job.JobType))
                            Jobs[job.JobType].Add(job.ID);
                        else
                            Jobs.Add(job.JobType, new List<int>() { job.ID });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("File error:");
                    Console.WriteLine(e.Message);
                    throw (e);
                }
            }
            freeVehicle.Sort();

            JobsDone Out = freeVehicle.Work(Jobs);

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in Out.VehicleJobList)
            {
                stringBuilder.AppendLine(item.Key.ToString() + ' ' + item.Value.ToString());
            }
            try
            {
                File.WriteAllText(Directory.GetCurrentDirectory() + @"\Output.txt", stringBuilder.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
