using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cars
{
    class Program
    {
        static void Main(string[] args)
        {
            var cars = ProcessCars("fuel.csv");
            var manufacturers = ProcessManufacturers("manufacturers.csv");

            var query =
                from manufacturer in manufacturers
                join car in cars on manufacturer.Name equals car.Manufacturer
                    
                    into carGroup
                orderby manufacturer.Name
                select new
                {
                    Manufacturer = manufacturer,
                    Cars = carGroup
                };


            var query2 =
                cars.GroupBy(c => c.Manufacturer)
                    .Select(g =>
                    {
                        var results = g.Aggregate(new CarStatistics(),
                            (acc, c) => acc.Accumulate(c),
                            acc => acc.Compute());

                        return new
                        {
                            Name = g.Key,
                            Avg = results.Average,
                            Max = results.Max,
                            Min = results.Min
                        };
                    })
                    .OrderBy(r => r.Max);
                


            foreach (var result in query2)
            {
                Console.WriteLine($"{ result.Name }");
                Console.WriteLine($"\tMax: { result.Max }");
                Console.WriteLine($"\tMin: { result.Min }");
                Console.WriteLine($"\tAverage: { result.Avg }");
                
                
            }
        }
        
        

        private static List<Manufacturer> ProcessManufacturers(string path)
        {
            return
                File.ReadAllLines(path)
                    .Where(l => l.Length > 1)
                    .Select(l =>
                    {
                        var columns = l.Split(",");
                        return new Manufacturer
                        {
                            Name = columns[0],
                            Headquarters = columns[1],
                            Year = int.Parse(columns[2])
                        };
                    })
                    .ToList();
        }

        private static List<Car> ProcessCars(string path)
        {
            return
                File.ReadAllLines(path)
                    .Skip(1)
                    .Where(line => line.Length > 1)
                    .Select(Car.ParseFromCsv)
                    .ToList();
        }
    }

    public class CarStatistics
    {
        public int Max { get; set; }
        public int Total { get; set; }
        public int Count { get; set; }
        public int Min { get; set; }
        public double Average { get; set; }

        public CarStatistics()
        {
            Max = Int32.MinValue;
            Min = Int32.MaxValue;
            
        }
        public CarStatistics Accumulate(Car car)
        {
            Count += 1;
            Total += car.Combined;
            Max = Math.Max(Max, car.Combined);
            Min = Math.Min(Min, car.Combined);
            return this;
        }

        public CarStatistics Compute()
        {

            Average = Total / Count;
            return this;
        }
    }
}