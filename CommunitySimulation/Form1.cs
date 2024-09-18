using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using OxyPlot.Legends; // Import legend support
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CommunitySimulation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Create and configure the plot model
            var plotModel = new PlotModel { Title = "PS vs Asset, Transaction Volume, and Consumption" };

            // Create and configure the legend manually
            var legend = new Legend
            {
                LegendPosition = LegendPosition.BottomCenter, // Set the position of the legend below the plot
                LegendPlacement = LegendPlacement.Outside, // Ensure the legend is outside the plot area
                LegendOrientation = LegendOrientation.Horizontal // Set the legend orientation to horizontal
            };
            plotModel.Legends.Add(legend);  // Add the legend to the plot model

            // Create the scatter series for assets
            var scatterSeriesAssets = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                Title = "Assets"  // This title will show up in the legend
            };

            // Create the scatter series for transaction volume
            var scatterSeriesTransactions = new ScatterSeries
            {
                MarkerType = MarkerType.Square,
                Title = "Transaction Volume"  // This title will show up in the legend
            };

            // Create the scatter series for total consumption
            var scatterSeriesConsumption = new ScatterSeries
            {
                MarkerType = MarkerType.Triangle,
                Title = "Total Consumption"  // This title will show up in the legend
            };

            // Create a community instance and run the simulation
            Community community1 = new Community(20, 10, 0.02); // Adjust population, price, and tax rate
            community1.RunModel(30); // Run the model 30 times
            community1.RemovePoorConsumers();

            // Collect PS, Asset, Consumption, and Transaction Volume values for plotting
            List<double> PSValues = new List<double>();
            List<double> AssetValues = new List<double>();
            List<double> ConsumptionValues = new List<double>();
            List<int> TransactionVolumes = new List<int>();

            foreach (var consumer in community1.Consumers)
            {
                PSValues.Add(consumer.PS); // Get PS value from each consumer
                AssetValues.Add(consumer.Asset);
                ConsumptionValues.Add(consumer.TotalConsumption); // Track total consumption
                TransactionVolumes.Add(consumer.TransactionVolume); // Track number of transactions

                // Print values to the console for debugging
                Console.WriteLine($"PS: {consumer.PS}, Asset: {consumer.Asset}, Total Consumption: {consumer.TotalConsumption}, Transactions: {consumer.TransactionVolume}");
            }

            // Add points to the scatter series for assets
            for (int i = 0; i < PSValues.Count; i++)
            {
                scatterSeriesAssets.Points.Add(new ScatterPoint(PSValues[i], AssetValues[i]));
            }

            // Add points to the scatter series for transaction volume
            for (int i = 0; i < PSValues.Count; i++)
            {
                scatterSeriesTransactions.Points.Add(new ScatterPoint(PSValues[i], TransactionVolumes[i]));
            }

            // Add points to the scatter series for total consumption
            for (int i = 0; i < PSValues.Count; i++)
            {
                scatterSeriesConsumption.Points.Add(new ScatterPoint(PSValues[i], ConsumptionValues[i]));
            }

            // Add the scatter series to the plot model
            plotModel.Series.Add(scatterSeriesAssets);
            plotModel.Series.Add(scatterSeriesTransactions);
            plotModel.Series.Add(scatterSeriesConsumption);

            // Create and configure the plot view
            var plotView = new PlotView
            {
                Model = plotModel,
                Dock = DockStyle.Fill // Fill the entire form with the plot
            };

            // Add the plot view to the form
            this.Controls.Add(plotView);
        }
    }

    // Define the Community and Consumer classes
    public class Community
    {
        public int Population { get; private set; }
        public double TransactionPrice { get; private set; }
        public double TaxRate { get; private set; }
        public double TotalTax { get; private set; }
        public List<Consumer> Consumers { get; private set; }

        public Community(int population, double transactionPrice, double taxRate)
        {
            Population = population;
            TransactionPrice = transactionPrice;
            TaxRate = taxRate;
            TotalTax = 0;
            Consumers = new List<Consumer>();

            // Initialize consumers
            for (int i = 0; i < population; i++)
            {
                Consumers.Add(new Consumer(this));
            }
        }

        // Run the model a set number of times
        public void RunModel(int times)
        {
            Random rand = new Random();
            for (int t = 0; t < times; t++)
            {
                Consumers = Consumers.OrderBy(x => rand.Next()).ToList(); // Shuffle consumers
                for (int i = 0; i < Consumers.Count - 1; i++)
                {
                    Consumers[i].Consume();
                    Consumers[i + 1].Serve();
                }
            }
        }

        // Remove consumers with assets <= 0
        public void RemovePoorConsumers()
        {
            Consumers.RemoveAll(consumer => consumer.Asset <= 0);
        }

        public class Consumer
        {
            private Community community;
            public int Id { get; private set; }
            public double Asset { get; set; }
            public int PS { get; private set; } // Each consumer now has its own PS value
            public int TransactionVolume { get; private set; } // Tracks number of transactions
            public double TotalConsumption { get; private set; } // Tracks total consumption

            public Consumer(Community community)
            {
                this.community = community;
                Id = SetId();
                PS = SetPS(); // Each consumer gets a unique PS value
                Asset = 100; // Each consumer starts with 100 units of assets
                TransactionVolume = 0; // Initialize transaction volume
                TotalConsumption = 0; // Initialize total consumption
            }

            // Assign a random ID to the consumer
            private int SetId()
            {
                Random rand = new Random();
                return rand.Next(100000, 1000000);
            }

            // Each consumer has a unique PS value dynamically set
            private int SetPS()
            {
                Random rand = new Random();
                return rand.Next(-5, 6); // Generates PS values between -5 and 5
            }

            // The consumer spends money on consumption
            public void Consume()
            {
                double consumption = community.TransactionPrice + PS; // Consumption depends on transaction price and PS
                Asset -= consumption; // Reduce the consumer's asset
                TotalConsumption += consumption; // Track the total consumption
                TransactionVolume++; // Track the number of transactions
            }

            // The consumer earns money by serving, paying taxes in the process
            public void Serve()
            {
                Asset += community.TransactionPrice * (1 - community.TaxRate);
                community.TotalTax += community.TransactionPrice * community.TaxRate;
                TransactionVolume++; // Increment transaction volume for serving
            }
        }
    }
}
