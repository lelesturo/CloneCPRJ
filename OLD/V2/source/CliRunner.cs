namespace CreaCprjMontante;

internal static class CliRunner
{
    public static int Run(string[] args)
    {
        try
        {
            var options = Options.Parse(args);

            if (options.ShowHelp)
            {
                PrintHelp();
                return 0;
            }

            if (string.IsNullOrWhiteSpace(options.SourcePath))
            {
                throw new InvalidOperationException("Manca il percorso sorgente. Usa --source oppure --root.");
            }

            if (!string.IsNullOrWhiteSpace(options.CsvPath))
            {
                var createdFiles = CprjGenerator.BuildBatch(
                    options.SourcePath,
                    options.OutputDirectory,
                    options.CsvPath,
                    options.TemplateName,
                    options.TemplateIp,
                    options.TemplateCode,
                    options.TargetCode,
                    Console.WriteLine);

                Console.WriteLine($"Totale file creati: {createdFiles.Count}");
                return 0;
            }

            if (string.IsNullOrWhiteSpace(options.TargetName))
            {
                throw new InvalidOperationException("Manca il nome montante. Usa --name.");
            }

            if (string.IsNullOrWhiteSpace(options.TargetIp))
            {
                throw new InvalidOperationException("Manca l'IP montante. Usa --ip.");
            }

            var createdFile = CprjGenerator.BuildSingle(
                options.SourcePath,
                options.OutputDirectory,
                options.TemplateName,
                options.TemplateIp,
                options.TemplateCode,
                options.TargetCode,
                options.TargetName,
                options.TargetIp,
                options.ExtraFind,
                options.ExtraReplace,
                Console.WriteLine);

            Console.WriteLine($"Creato: {createdFile}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("ERRORE: " + ex.Message);
            return 1;
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("CreaCprjMontante");
        Console.WriteLine("  --source|-s     Cartella template oppure file .cprj");
        Console.WriteLine("  --out|-o        Cartella output dei .cprj (default: .\\GENERATI accanto all'exe)");
        Console.WriteLine("  --csv           File CSV per modalita' batch");
        Console.WriteLine("  --template|-t   Nome template da sostituire (default: 3M11)");
        Console.WriteLine("  --template-ip   IP presente nel template (default: 10.16.26.41)");
        Console.WriteLine("  --template-code Quadriletterale presente nel template, es. PTOV");
        Console.WriteLine("  --name|-n       Nome montante output, modalita' singola");
        Console.WriteLine("  --ip|-i         IP montante output, modalita' singola");
        Console.WriteLine("  --target-code   Quadriletterale output, es. TNOT");
        Console.WriteLine("  --find          Testo extra da cercare");
        Console.WriteLine("  --replace       Testo extra sostitutivo");
        Console.WriteLine();
        Console.WriteLine("CSV con header consigliato:");
        Console.WriteLine("  TargetName;TargetIp;TemplateCode;TargetCode;ExtraFind;ExtraReplace;OutputName");
    }

    private sealed class Options
    {
        public string? SourcePath { get; private init; }
        public string? OutputDirectory { get; private init; }
        public string? CsvPath { get; private init; }
        public string TemplateName { get; private init; } = "3M11";
        public string TemplateIp { get; private init; } = "10.16.26.41";
        public string? TemplateCode { get; private init; }
        public string? TargetName { get; private init; }
        public string? TargetIp { get; private init; }
        public string? TargetCode { get; private init; }
        public string? ExtraFind { get; private init; }
        public string? ExtraReplace { get; private init; }
        public bool ShowHelp { get; private init; }

        public static Options Parse(string[] args)
        {
            string? sourcePath = null;
            string? outputDirectory = null;
            string? csvPath = null;
            string templateName = "3M11";
            string templateIp = "10.16.26.41";
            string? templateCode = null;
            string? targetName = null;
            string? targetIp = null;
            string? targetCode = null;
            string? extraFind = null;
            string? extraReplace = null;
            var showHelp = false;

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg.ToLowerInvariant())
                {
                    case "--source":
                    case "--root":
                    case "-s":
                    case "-r":
                        sourcePath = ReadArgumentValue(args, ref i, arg);
                        break;

                    case "--out":
                    case "-o":
                        outputDirectory = ReadArgumentValue(args, ref i, arg);
                        break;

                    case "--csv":
                        csvPath = ReadArgumentValue(args, ref i, arg);
                        break;

                    case "--template":
                    case "-t":
                        templateName = ReadArgumentValue(args, ref i, arg);
                        break;

                    case "--template-ip":
                        templateIp = ReadArgumentValue(args, ref i, arg);
                        break;

                    case "--template-code":
                    case "--source-code":
                        templateCode = ReadArgumentValue(args, ref i, arg);
                        break;

                    case "--name":
                    case "-n":
                        targetName = ReadArgumentValue(args, ref i, arg);
                        break;

                    case "--ip":
                    case "-i":
                        targetIp = ReadArgumentValue(args, ref i, arg);
                        break;

                    case "--target-code":
                    case "--to-code":
                        targetCode = ReadArgumentValue(args, ref i, arg);
                        break;

                    case "--find":
                        extraFind = ReadArgumentValue(args, ref i, arg);
                        break;

                    case "--replace":
                        extraReplace = ReadArgumentValue(args, ref i, arg);
                        break;

                    case "--help":
                    case "-h":
                        showHelp = true;
                        break;

                    default:
                        throw new InvalidOperationException($"Argomento non riconosciuto: {arg}");
                }
            }

            return new Options
            {
                SourcePath = sourcePath,
                OutputDirectory = outputDirectory,
                CsvPath = csvPath,
                TemplateName = templateName,
                TemplateIp = templateIp,
                TemplateCode = templateCode,
                TargetName = targetName,
                TargetIp = targetIp,
                TargetCode = targetCode,
                ExtraFind = extraFind,
                ExtraReplace = extraReplace,
                ShowHelp = showHelp
            };
        }

        private static string ReadArgumentValue(string[] args, ref int index, string option)
        {
            index++;
            if (index >= args.Length)
            {
                throw new InvalidOperationException($"Manca il valore per {option}.");
            }

            return args[index];
        }
    }
}
