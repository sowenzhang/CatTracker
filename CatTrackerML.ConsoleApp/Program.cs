using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ML;
using CatTrackerML.Model;
using Microsoft.Extensions.CommandLineUtils;

namespace CatTrackerML.ConsoleApp
{
    class Program
    {
        //Dataset to use for predictions 
        private static readonly string TEST_TSV_FILEPATH = $@"{AppDomain.CurrentDomain.BaseDirectory}..\..\..\..\resources\testImages\test.tsv";
        private static readonly string TRAIN_TSV_FILEPATH = $@"{AppDomain.CurrentDomain.BaseDirectory}..\..\..\..\resources\cats\cats.tsv";
        private static readonly string MODEL_FILEPATH = $@"{AppDomain.CurrentDomain.BaseDirectory}..\..\..\..\CatTrackerML.Model\MLModel.zip";

        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "Cat Breed Trainer/Classifier";
            app.Description = "An app that trains and classifies cat breed";
            app.ExtendedHelpText = "use 'app train' or 'app predict' to either (re)train model or classify an image";
            app.HelpOption("-?|-help|-h");

            app.OnExecute(() =>
            {
                app.ShowHint();

                return 0;
            });

            app.Command("train", TrainCommand);
            app.Command("predict", PredictCommand);

            try
            {
                // This begins the actual execution of the application
                Console.WriteLine("app executing...");
                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                // You'll always want to catch this exception, otherwise it will generate a messy and confusing error for the end user.
                // the message will usually be something like:
                // "Unrecognized command or argument '<invalid-command>'"
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to execute application: {0}", ex.Message);
            }
        }

        private static Action<CommandLineApplication> TrainCommand => (command) =>
        {
            command.Description = "Train cat model";
            command.HelpOption("-?|-h|--help");

            var tsvFileOption = command.Option("-t|-tsv-file <file-path>",
                "the full path of a TSV file",
                CommandOptionType.SingleValue);

            var modelFileOption = command.Option("-m|-model-file <model-file-path>",
                "where do you want to save the model file", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                Console.WriteLine("=============== Start training ===============");
                string tsvFile;

                if (tsvFileOption.HasValue())
                {
                    tsvFile = tsvFileOption.Value();
                    if (File.Exists(tsvFile) == false)
                    {
                        throw new FileNotFoundException($"{tsvFile} does not exist");
                    }
                }
                else
                {
                    tsvFile = TRAIN_TSV_FILEPATH;
                }

                var modelFile = modelFileOption.HasValue() ? modelFileOption.Value() : MODEL_FILEPATH;

                ModelBuilder.CreateModel(tsvFile, modelFile);
                Console.WriteLine("=============== Finish training, hit any key to continue ===============");
                Console.ReadKey();
                return 0;
            });
        };

        private static Action<CommandLineApplication> PredictCommand =>
         command =>
            {
                command.Description = "Predict a cat image or multiple cat images";
                command.ExtendedHelpText = "Either -f or -t must be specified";
                command.HelpOption("-?|-h|--help");

                var fileNameOption = command.Option("-f|-file <file-path>",
                    "it needs to be an image", CommandOptionType.SingleValue);

                var tsvFileOption = command.Option("-t|-tsv-file <file-path>",
                    "the full path of a TSV file",
                    CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    Console.WriteLine("=============== Start prediction ===============");
                    IEnumerable<ModelInput> samples;

                    if (fileNameOption.HasValue())
                    {
                        string fileNameValue = fileNameOption.Value();
                        FileInfo fi = new FileInfo(fileNameValue);
                        if (fi.Exists == false)
                        {
                            throw new FileNotFoundException($"{fi.FullName} does not exist");
                        }

                        string tempFile = Path.GetTempFileName();
                        List<string> content = new List<string>();
                        content.Add("Label\tImageSource");
                        content.Add($"{fi.Name}\t{fi.FullName}");
                        File.WriteAllLines(tempFile, content);
                        samples = CreateInputFromTsv(tempFile);
                    }
                    else
                    {
                        var tsvFileValue = tsvFileOption.HasValue() ? tsvFileOption.Value() : TEST_TSV_FILEPATH;
                        samples = CreateInputFromTsv(tsvFileValue);
                    }
                    
                    foreach (var sampleData in samples)
                    {
                        Predict(sampleData);
                    }

                    Console.WriteLine("=============== End of process, hit any key to finish ===============");
                    Console.ReadKey();

                    return 0;
                });
            };
        

        private static void Predict(ModelInput sampleData)
        {
            // Make a single prediction on the sample data and print results
            ModelOutput predictionResult = ConsumeModel.Predict(sampleData);

            Console.WriteLine("Using model to make single prediction -- Comparing actual Label with predicted Label from sample data...\n\n");
            Console.WriteLine($"ImageSource: {sampleData.ImageSource}");
            Console.WriteLine(
                $"\n\nActual Label: {sampleData.Label} \nPredicted Label value {predictionResult.Prediction} \nPredicted Label scores: [{String.Join(",", predictionResult.Score)}]\n\n");
        }


        // Method to load single row of dataset to try a single prediction
        private static IEnumerable<ModelInput> CreateInputFromTsv(string dataFilePath)
        {
            // Create MLContext
            MLContext mlContext = new MLContext();
            
            // Load dataset
            IDataView dataView = mlContext.Data.LoadFromTextFile<ModelInput>(
                                            path: dataFilePath,
                                            hasHeader: true,
                                            separatorChar: '\t',
                                            allowQuoting: true);

            // Use first line of dataset as model input
            // You can replace this with new test data (hardcoded or from end-user application)
            return mlContext.Data.CreateEnumerable<ModelInput>(dataView, false);
        }

    }
}
