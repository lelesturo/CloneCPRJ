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

            if (!string.IsNullOrWhiteSpace(options.CopyModuleTo))
            {
                if (string.IsNullOrWhiteSpace(options.InspectModule))
                {
                    throw new InvalidOperationException("Per copiare un modulo usa anche --inspect-module con il nome modulo.");
                }

                var results = CprjGenerator.CopyModuleToProjects(
                    options.SourcePath,
                    options.CopyModuleTo,
                    options.OutputDirectory,
                    options.InspectModule,
                    options.ModuleType,
                    [],
                    Console.WriteLine);

                Console.WriteLine($"Progetti aggiornati: {results.Count}");
                foreach (var result in results)
                {
                    Console.WriteLine($"  COPY {result.OutputProject} file={result.CopiedFiles} rtu={result.RtuNodes} ddbb={result.DatabaseNodes}");
                }

                return 0;
            }

            if (!string.IsNullOrWhiteSpace(options.InspectModule))
            {
                var inspection = CprjGenerator.InspectModuleImpact(
                    options.SourcePath,
                    options.InspectModule,
                    options.IncludeModuleFamily);

                Console.WriteLine($"Modulo: {inspection.ModuleName}");
                Console.WriteLine($"Definizioni: {inspection.Definitions.Count}");
                foreach (var definition in inspection.Definitions)
                {
                    Console.WriteLine($"  DEF {definition.FileName} [{definition.Kind}] label={definition.Label ?? "-"} tipo={definition.DeviceType ?? "-"} ip={definition.Ip ?? "-"} tag={definition.Tags.Count}");
                }

                Console.WriteLine($"RTU: {inspection.RtuReferences.Count}");
                foreach (var reference in inspection.RtuReferences)
                {
                    Console.WriteLine($"  RTU {reference.NodeType} id={reference.Id ?? "-"} href={reference.Href ?? "-"} label={reference.Label ?? "-"} errorTag={reference.ErrorTag ?? "-"}");
                }

                Console.WriteLine($"Tag: {string.Join(", ", inspection.Tags)}");
                Console.WriteLine($"Termini modulo: {string.Join(", ", inspection.DirectSearchTerms)}");
                Console.WriteLine($"Richiami espliciti al modulo: {inspection.TextImpacts.Count}");
                foreach (var impact in inspection.TextImpacts)
                {
                    var terms = string.Join(", ", impact.Terms.Take(5).Select(term => $"{term.Term}={term.Occurrences}"));
                    Console.WriteLine($"  MOD {impact.FileName}: {impact.TotalOccurrences} ({terms})");
                }

                Console.WriteLine($"Termini tag: {string.Join(", ", inspection.TagSearchTerms)}");
                Console.WriteLine($"Moduli/file con tag: {inspection.TagImpacts.Count}");
                foreach (var impact in inspection.TagImpacts)
                {
                    var terms = string.Join(", ", impact.Terms.Take(5).Select(term => $"{term.Term}={term.Occurrences}"));
                    Console.WriteLine($"  TAG {impact.FileName}: {impact.TotalOccurrences} ({terms})");
                }

                return 0;
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

            if (IsStandaloneFileSource(options.SourcePath))
            {
                var additional = new List<TextReplacement>();
                if (!string.IsNullOrWhiteSpace(options.TargetIp))
                {
                    additional.Add(new TextReplacement(options.TemplateIp, options.TargetIp));
                }

                if (!string.IsNullOrWhiteSpace(options.TemplateCode) && !string.IsNullOrWhiteSpace(options.TargetCode))
                {
                    additional.Add(new TextReplacement(options.TemplateCode, options.TargetCode));
                }

                var createdFiles = CprjGenerator.BuildGenericBatch(
                    options.SourcePath,
                    options.OutputDirectory,
                    options.TemplateName,
                    [new GenericReplaceJob(options.TargetName, null, options.ExtraFind, options.ExtraReplace, additional)],
                    Console.WriteLine);

                Console.WriteLine($"Totale file creati: {createdFiles.Count}");
                return 0;
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
        Console.WriteLine("  --source|-s     Cartella template, file .cprj/.zip oppure singolo file testo");
        Console.WriteLine("  --out|-o        Cartella output (default: .\\GENERATI accanto all'exe)");
        Console.WriteLine("  --csv           File CSV/XLSX/XLS per modalita' batch");
        Console.WriteLine("  --template|-t   Nome template da sostituire (default: 3M11)");
        Console.WriteLine("  --template-ip   IP presente nel template (default: 10.16.26.41)");
        Console.WriteLine("  --template-code Quadriletterale presente nel template, es. PTOV");
        Console.WriteLine("  --name|-n       Nome montante output, modalita' singola");
        Console.WriteLine("  --ip|-i         IP montante output, richiesto per .cprj e opzionale per file testo");
        Console.WriteLine("  --target-code   Quadriletterale output, es. TNOT");
        Console.WriteLine("  --find          Testo extra da cercare");
        Console.WriteLine("  --replace       Testo extra sostitutivo");
        Console.WriteLine("  --inspect-module Nome modulo da analizzare senza modifiche");
        Console.WriteLine("  --module-type   Tipo modulo opzionale da copiare, es. IGCP, m61850, mPinger");
        Console.WriteLine("  --copy-module-to File .cprj/.zip o cartella di progetti in cui copiare il modulo");
        Console.WriteLine("  --module-exact  Non include la famiglia tag/prefisso nell'analisi modulo");
        Console.WriteLine();
        Console.WriteLine("File tabellare con header consigliato:");
        Console.WriteLine("  TargetName;TargetIp;TemplateCode;TargetCode;ExtraFind;ExtraReplace;OutputName");
    }

    private static bool IsStandaloneFileSource(string sourcePath)
    {
        if (!File.Exists(sourcePath))
        {
            return false;
        }

        var extension = Path.GetExtension(sourcePath);
        return !extension.Equals(".cprj", StringComparison.OrdinalIgnoreCase) &&
               !extension.Equals(".zip", StringComparison.OrdinalIgnoreCase);
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
        public string? InspectModule { get; private init; }
        public string? ModuleType { get; private init; }
        public string? CopyModuleTo { get; private init; }
        public bool IncludeModuleFamily { get; private init; } = true;
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
            string? inspectModule = null;
            string? moduleType = null;
            string? copyModuleTo = null;
            var includeModuleFamily = true;
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

                    case "--inspect-module":
                        inspectModule = ReadArgumentValue(args, ref i, arg);
                        break;

                    case "--module-type":
                        moduleType = ReadArgumentValue(args, ref i, arg);
                        break;

                    case "--copy-module-to":
                        copyModuleTo = ReadArgumentValue(args, ref i, arg);
                        break;

                    case "--module-exact":
                        includeModuleFamily = false;
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
                InspectModule = inspectModule,
                ModuleType = moduleType,
                CopyModuleTo = copyModuleTo,
                IncludeModuleFamily = includeModuleFamily,
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
