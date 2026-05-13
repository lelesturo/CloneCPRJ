using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace CreaCprjMontante;

internal sealed record TextReplacement(string Find, string Replace);

internal sealed record GenericReplaceJob(
    string ReplaceValue,
    string? OutputName = null,
    string? ExtraFind = null,
    string? ExtraReplace = null,
    IReadOnlyList<TextReplacement>? AdditionalReplacements = null);

internal sealed record TemplateClientLink(string DeviceType, string Href, string CurrentIp);

internal sealed record TemplateInspection(
    string TemplateName,
    string TemplateDeviceType,
    string ServerIp,
    IReadOnlyList<TemplateClientLink> Clients);

internal sealed record DeviceCsvProjectInspection(
    string Montante,
    IReadOnlyList<DeviceCsvDeviceInspection> Devices);

internal sealed record DeviceCsvDeviceInspection(
    string DeviceType,
    string Ip);

internal sealed record DeviceCsvInspection(
    string CsvPath,
    int ProjectCount,
    int EntryCount,
    IReadOnlyList<string> DeviceTypes,
    IReadOnlyList<DeviceCsvProjectInspection> Projects);

internal static class CprjGenerator
{
    private static readonly Encoding StrictUtf8Encoding = new UTF8Encoding(true, true);
    private static readonly Encoding StrictUnicodeEncoding = new UnicodeEncoding(false, true, true);
    private static readonly Encoding StrictBigEndianUnicodeEncoding = new UnicodeEncoding(true, true, true);

    private static readonly Dictionary<string, string> DeviceAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["OP"] = "OP",
        ["BMUY"] = "OP",
        ["AS"] = "AS1",
        ["AS1"] = "AS1",
        ["BMSY"] = "AS1",
        ["EV"] = "EV",
        ["BMVY"] = "EV"
    };

    public static string DefaultOutputDirectory => Path.Combine(AppContext.BaseDirectory, "GENERATI");

    public static string BuildSingle(
        string sourcePath,
        string? outputDirectory,
        string templateName,
        string templateIp,
        string? templateCode,
        string? targetCode,
        string targetName,
        string targetIp,
        string? extraFind,
        string? extraReplace,
        Action<string>? log = null)
    {
        var additional = new List<TextReplacement> { new(templateIp, targetIp) };
        if (!string.IsNullOrWhiteSpace(templateCode) && !string.IsNullOrWhiteSpace(targetCode))
        {
            additional.Add(new TextReplacement(templateCode.Trim(), targetCode.Trim()));
        }

        var jobs = new[]
        {
            new GenericReplaceJob(targetName, targetName, extraFind, extraReplace, additional)
        };

        return BuildGenericBatch(sourcePath, outputDirectory, templateName, jobs, log).Single();
    }

    public static IReadOnlyList<string> BuildBatch(
        string sourcePath,
        string? outputDirectory,
        string csvPath,
        string templateName,
        string templateIp,
        string? templateCode,
        string? targetCode,
        Action<string>? log = null)
    {
        var requests = LoadLegacyBatchRequests(csvPath, templateName, templateIp, templateCode, targetCode);
        var jobs = requests
            .Select(request => new GenericReplaceJob(
                request.TargetName,
                request.OutputName,
                request.ExtraFind,
                request.ExtraReplace,
                request.AdditionalReplacements))
            .ToList();

        return BuildGenericBatch(sourcePath, outputDirectory, templateName, jobs, log);
    }

    public static IReadOnlyList<string> BuildGenericBatch(
        string sourcePath,
        string? outputDirectory,
        string baseFind,
        IReadOnlyList<GenericReplaceJob> jobs,
        Action<string>? log = null)
    {
        if (string.IsNullOrWhiteSpace(baseFind))
        {
            throw new InvalidOperationException("Inserisci il testo principale da cercare.");
        }

        var normalizedSourcePath = ResolveSourcePath(sourcePath);
        var normalizedOutputDirectory = ResolveOutputDirectory(outputDirectory);
        var normalizedBaseFind = baseFind.Trim();
        var effectiveJobs = NormalizeGenericJobs(jobs);

        using var templateContext = PrepareTemplateContext(normalizedSourcePath);
        var templateFiles = GetTemplateItems(templateContext.TemplateRootPath);
        EnsureTemplateFiles(templateFiles, templateContext.TemplateRootPath);

        log?.Invoke($"Template: {normalizedSourcePath}");
        log?.Invoke($"Output: {normalizedOutputDirectory}");
        log?.Invoke($"Find principale: {normalizedBaseFind}");
        log?.Invoke($"Righe da generare: {effectiveJobs.Count}");

        var plans = effectiveJobs.Select(job =>
        {
            var replacements = new List<TextReplacement> { new(normalizedBaseFind, job.ReplaceValue) };
            replacements.AddRange(job.AdditionalReplacements ?? []);

            if (!string.IsNullOrWhiteSpace(job.ExtraFind))
            {
                replacements.Add(new TextReplacement(job.ExtraFind!, job.ExtraReplace!));
            }

            return new BuildPlan(
                job.OutputName!,
                [new TextReplacement(normalizedBaseFind, job.ReplaceValue)],
                replacements,
                new Dictionary<string, List<TextReplacement>>(StringComparer.OrdinalIgnoreCase));
        }).ToList();

        return ExecutePlans(normalizedOutputDirectory, templateFiles, plans, log);
    }

    public static TemplateInspection InspectTemplate(string sourcePath, string templateName)
    {
        var normalizedSourcePath = ResolveSourcePath(sourcePath);
        using var templateContext = PrepareTemplateContext(normalizedSourcePath);
        return InspectTemplateRoot(templateContext.TemplateRootPath, templateName);
    }

    public static DeviceCsvInspection InspectDeviceCsv(string sourcePath, string csvPath)
    {
        var normalizedSourcePath = ResolveSourcePath(sourcePath);
        var normalizedCsvPath = ResolveOptionalPath(csvPath, normalizedSourcePath)
            ?? throw new InvalidOperationException("CSV dispositivi non specificato.");
        var projects = LoadDeviceProjects(normalizedCsvPath);

        var deviceTypes = projects
            .SelectMany(project => project.DeviceIps.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(deviceType => deviceType, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var inspectedProjects = projects
            .Select(project => new DeviceCsvProjectInspection(
                project.Montante,
                project.DeviceIps
                    .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(entry => new DeviceCsvDeviceInspection(entry.Key, entry.Value))
                    .ToList()))
            .ToList();

        return new DeviceCsvInspection(
            normalizedCsvPath,
            projects.Count,
            projects.Sum(project => project.DeviceIps.Count),
            deviceTypes,
            inspectedProjects);
    }

    public static IReadOnlyList<string> BuildDeviceProjectsFromCsv(
        string sourcePath,
        string? outputDirectory,
        string templateName,
        string csvPath,
        string? quadFind,
        string? quadReplace,
        Action<string>? log = null)
    {
        if (string.IsNullOrWhiteSpace(templateName))
        {
            throw new InvalidOperationException("Inserisci il montante template.");
        }

        var normalizedSourcePath = ResolveSourcePath(sourcePath);
        var normalizedOutputDirectory = ResolveOutputDirectory(outputDirectory);
        var normalizedTemplateName = templateName.Trim();
        var normalizedCsvPath = ResolveOptionalPath(csvPath, normalizedSourcePath)
            ?? throw new InvalidOperationException("CSV dispositivi non specificato.");

        using var templateContext = PrepareTemplateContext(normalizedSourcePath);
        var templateFiles = GetTemplateItems(templateContext.TemplateRootPath);
        EnsureTemplateFiles(templateFiles, templateContext.TemplateRootPath);

        var inspection = InspectTemplateRoot(templateContext.TemplateRootPath, normalizedTemplateName);
        var projects = LoadDeviceProjects(normalizedCsvPath);

        log?.Invoke($"Template: {normalizedSourcePath}");
        log?.Invoke($"Output: {normalizedOutputDirectory}");
        log?.Invoke($"Montante template: {normalizedTemplateName}");
        log?.Invoke($"Tipo progetto rilevato: {inspection.TemplateDeviceType}");
        log?.Invoke($"IP server template: {inspection.ServerIp}");
        log?.Invoke($"CSV dispositivi: {normalizedCsvPath}");
        log?.Invoke($"Montanti da generare: {projects.Count}");
        if (!string.IsNullOrWhiteSpace(quadFind) && !string.IsNullOrWhiteSpace(quadReplace))
        {
            log?.Invoke($"Quadriletterale: {quadFind.Trim()} -> {quadReplace.Trim()}");
        }

        var plans = new List<BuildPlan>();
        foreach (var project in projects)
        {
            if (!project.DeviceIps.TryGetValue(inspection.TemplateDeviceType, out var targetServerIp))
            {
                throw new InvalidOperationException(
                    $"Nel CSV manca il dispositivo '{inspection.TemplateDeviceType}' per il montante '{project.Montante}'.");
            }

            var fileSpecific = new Dictionary<string, List<TextReplacement>>(StringComparer.OrdinalIgnoreCase)
            {
                ["rtu.ccx"] = [new TextReplacement(inspection.ServerIp, targetServerIp)]
            };

            foreach (var client in inspection.Clients)
            {
                if (!project.DeviceIps.TryGetValue(client.DeviceType, out var targetClientIp))
                {
                    throw new InvalidOperationException(
                        $"Nel CSV manca il dispositivo client '{client.DeviceType}' per il montante '{project.Montante}'.");
                }

                fileSpecific[client.Href] = [new TextReplacement(client.CurrentIp, targetClientIp)];
            }

            var globalReplacements = new List<TextReplacement>
            {
                new TextReplacement(normalizedTemplateName, project.Montante)
            };

            if (!string.IsNullOrWhiteSpace(quadFind) && !string.IsNullOrWhiteSpace(quadReplace))
            {
                globalReplacements.Add(new TextReplacement(quadFind.Trim(), quadReplace.Trim()));
            }

            plans.Add(new BuildPlan(
                project.Montante,
                [new TextReplacement(normalizedTemplateName, project.Montante)],
                globalReplacements,
                fileSpecific));
        }

        return ExecutePlans(normalizedOutputDirectory, templateFiles, plans, log);
    }

    private static IReadOnlyList<string> ExecutePlans(
        string outputDirectory,
        IReadOnlyList<TemplateItem> templateFiles,
        IReadOnlyList<BuildPlan> plans,
        Action<string>? log)
    {
        var createdFiles = new List<string>();
        var failures = new List<string>();

        foreach (var plan in plans)
        {
            try
            {
                var archivePath = BuildTarget(outputDirectory, templateFiles, plan, log);
                createdFiles.Add(archivePath);
                log?.Invoke($"Creato: {archivePath}");
            }
            catch (Exception ex)
            {
                failures.Add($"{plan.OutputName}: {ex.Message}");
                log?.Invoke($"ERRORE {plan.OutputName}: {ex.Message}");
            }
        }

        if (failures.Count > 0)
        {
            throw new InvalidOperationException("Elaborazione completata con errori: " + string.Join(" | ", failures));
        }

        return createdFiles;
    }

    private static string BuildTarget(string outputDirectory, IReadOnlyList<TemplateItem> templateFiles, BuildPlan plan, Action<string>? log)
    {
        var tempOutputDirectory = CreateTempDirectory(outputDirectory);

        try
        {
            foreach (var sourceFile in templateFiles)
            {
                var destinationRelativePath = sourceFile.IsDirectory
                    ? sourceFile.RelativePath
                    : BuildDestinationRelativePath(sourceFile.RelativePath, plan.FileNameReplacements);
                var destinationPath = Path.Combine(tempOutputDirectory, destinationRelativePath);

                if (sourceFile.IsDirectory)
                {
                    Directory.CreateDirectory(destinationPath);
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? tempOutputDirectory);

                var bytes = File.ReadAllBytes(sourceFile.FullPath);
                if (TryDecodeText(bytes, out var content, out var encoding))
                {
                    content = ApplyReplacements(content, plan.GlobalReplacements, out var replacementCount);

                    if (TryGetFileSpecificReplacements(plan.FileSpecificReplacements, sourceFile.RelativePath, out var specific))
                    {
                        content = ApplyReplacements(content, specific, out var specificCount);
                        replacementCount += specificCount;
                    }

                    if (Path.GetExtension(sourceFile.RelativePath).Equals(".ccx", StringComparison.OrdinalIgnoreCase))
                    {
                        content = EnsureNoRestrictionSignature(content);
                    }

                    File.WriteAllText(destinationPath, content, encoding);
                    if (replacementCount > 0)
                    {
                        log?.Invoke($"Modificato testo: {sourceFile.RelativePath} ({replacementCount} sostituzioni)");
                    }
                }
                else
                {
                    File.Copy(sourceFile.FullPath, destinationPath, true);
                }
            }

            return CreateProjectArchive(outputDirectory, tempOutputDirectory, plan.OutputName);
        }
        finally
        {
            DeleteDirectoryIfExists(tempOutputDirectory);
        }
    }

    private static string BuildDestinationRelativePath(string relativePath, IReadOnlyList<TextReplacement> fileNameReplacements)
    {
        var directory = Path.GetDirectoryName(relativePath);
        var fileName = Path.GetFileName(relativePath);
        var destinationFileName = ApplyReplacements(fileName, fileNameReplacements);
        return string.IsNullOrWhiteSpace(directory) ? destinationFileName : Path.Combine(directory, destinationFileName);
    }

    private static bool TryGetFileSpecificReplacements(
        IReadOnlyDictionary<string, List<TextReplacement>> replacements,
        string relativePath,
        out List<TextReplacement> specific)
    {
        if (replacements.TryGetValue(relativePath, out specific!))
        {
            return true;
        }

        return replacements.TryGetValue(Path.GetFileName(relativePath), out specific!);
    }

    private static bool TryDecodeText(byte[] bytes, out string content, out Encoding encoding)
    {
        content = string.Empty;
        encoding = Encoding.Default;

        if (bytes.Length == 0)
        {
            return true;
        }

        if (bytes.Contains((byte)0))
        {
            return false;
        }

        if (!LooksLikeTextBytes(bytes))
        {
            return false;
        }

        var offset = 0;
        if (HasPrefix(bytes, [0xEF, 0xBB, 0xBF]))
        {
            encoding = StrictUtf8Encoding;
            offset = 3;
        }
        else if (HasPrefix(bytes, [0xFF, 0xFE]))
        {
            encoding = StrictUnicodeEncoding;
            offset = 2;
        }
        else if (HasPrefix(bytes, [0xFE, 0xFF]))
        {
            encoding = StrictBigEndianUnicodeEncoding;
            offset = 2;
        }

        try
        {
            content = encoding.GetString(bytes, offset, bytes.Length - offset);
        }
        catch (DecoderFallbackException)
        {
            return false;
        }

        return LooksLikeText(content);
    }

    private static bool HasPrefix(byte[] bytes, byte[] prefix)
    {
        if (bytes.Length < prefix.Length)
        {
            return false;
        }

        for (var i = 0; i < prefix.Length; i++)
        {
            if (bytes[i] != prefix[i])
            {
                return false;
            }
        }

        return true;
    }

    private static bool LooksLikeText(string content)
    {
        if (content.Length == 0)
        {
            return true;
        }

        var controlCount = content.Count(ch =>
            char.IsControl(ch) &&
            ch is not '\r' and not '\n' and not '\t' and not '\f');

        return controlCount == 0 || controlCount <= Math.Max(2, content.Length / 100);
    }

    private static bool LooksLikeTextBytes(byte[] bytes)
    {
        var controlCount = bytes.Count(value =>
            value < 32 &&
            value is not (byte)'\r' and not (byte)'\n' and not (byte)'\t' and not (byte)'\f');

        return controlCount == 0 || controlCount <= Math.Max(2, bytes.Length / 100);
    }

    private static TemplateInspection InspectTemplateRoot(string templateRootPath, string templateName)
    {
        var rtuPath = Path.Combine(templateRootPath, "rtu.ccx");
        if (!File.Exists(rtuPath))
        {
            throw new InvalidOperationException("Nel template manca il file rtu.ccx.");
        }

        var rtuDocument = XDocument.Load(rtuPath, LoadOptions.PreserveWhitespace);
        var label = ReadElementValue(rtuDocument, "general", "label")
            ?? ReadElementValue(rtuDocument, "CCX_device", "name")
            ?? throw new InvalidOperationException("Impossibile leggere il label del progetto.");

        var templateDeviceType = DetectTemplateDeviceType(label)
            ?? throw new InvalidOperationException("Impossibile capire il tipo progetto dal label del template.");

        var serverIp = DetectServerIp(rtuDocument)
            ?? throw new InvalidOperationException("Impossibile leggere l'IP server da rtu.ccx.");

        var clients = new List<TemplateClientLink>();
        var seenClientFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var module in rtuDocument.Descendants().Where(node => node.Name.LocalName.Equals("m61850", StringComparison.OrdinalIgnoreCase)))
        {
            var ccxLabel = module.Attribute("ccx_label")?.Value;
            var href = module.Attribute("href")?.Value;
            var deviceType = DetectClientTypeFromLabel(ccxLabel);

            if (deviceType is null || string.IsNullOrWhiteSpace(href))
            {
                continue;
            }

            var clientPath = Path.Combine(templateRootPath, href);
            if (!File.Exists(clientPath))
            {
                throw new InvalidOperationException($"Il client '{ccxLabel}' punta a un file mancante: {href}");
            }

            var clientDocument = XDocument.Load(clientPath, LoadOptions.PreserveWhitespace);
            var clientIp = clientDocument
                .Descendants()
                .FirstOrDefault(node => node.Name.LocalName.Equals("IP_addr", StringComparison.OrdinalIgnoreCase))
                ?.Value
                ?.Trim();

            if (string.IsNullOrWhiteSpace(clientIp))
            {
                throw new InvalidOperationException($"Nel file '{href}' manca il nodo <IP_addr>.");
            }

            if (seenClientFiles.Add(href))
            {
                clients.Add(new TemplateClientLink(deviceType, href, clientIp));
            }
        }

        foreach (var client in DiscoverIedClients(templateRootPath))
        {
            if (seenClientFiles.Add(client.Href))
            {
                clients.Add(client);
            }
        }

        return new TemplateInspection(templateName.Trim(), templateDeviceType, serverIp.Trim(), clients);
    }

    private static IReadOnlyList<DeviceProject> LoadDeviceProjects(string csvPath)
    {
        if (!File.Exists(csvPath))
        {
            throw new InvalidOperationException($"CSV non trovato: {csvPath}");
        }

        var rawLines = File.ReadAllLines(csvPath, Encoding.UTF8)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Where(line => !line.StartsWith('#'))
            .ToList();

        if (rawLines.Count == 0)
        {
            return [];
        }

        var delimiter = rawLines[0].Contains(';') ? ';' : ',';
        var firstRow = SplitRow(rawLines[0], delimiter);
        var hasHeader = IsDeviceCsvHeader(firstRow);
        var entries = new List<DeviceCsvEntry>();

        if (hasHeader)
        {
            var headerMap = BuildHeaderMap(firstRow);
            foreach (var line in rawLines.Skip(1))
            {
                var row = SplitRow(line, delimiter);
                if (row.Count > 0)
                {
                    entries.Add(ParseNamedDeviceEntry(row, headerMap));
                }
            }
        }
        else
        {
            foreach (var line in rawLines)
            {
                var row = SplitRow(line, delimiter);
                if (row.Count > 0)
                {
                    entries.Add(ParsePositionalDeviceEntry(row));
                }
            }
        }

        return entries
            .GroupBy(entry => entry.Montante, StringComparer.OrdinalIgnoreCase)
            .Select(group => new DeviceProject(
                group.First().Montante,
                group.ToDictionary(entry => entry.DeviceType, entry => entry.Ip, StringComparer.OrdinalIgnoreCase)))
            .OrderBy(project => project.Montante, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsDeviceCsvHeader(IReadOnlyList<string> row)
    {
        var knownHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "montante", "name", "targetname", "dispositivo", "device", "tipo", "ip", "targetip"
        };

        return row.Select(NormalizeHeader).Any(knownHeaders.Contains);
    }

    private static DeviceCsvEntry ParseNamedDeviceEntry(IReadOnlyList<string> row, IReadOnlyDictionary<string, int> headerMap)
    {
        string? ReadColumn(params string[] names)
        {
            foreach (var name in names)
            {
                if (headerMap.TryGetValue(name, out var index) && index < row.Count)
                {
                    return NormalizeOptionalValue(row[index]);
                }
            }

            return null;
        }

        return CreateDeviceEntry(
            ReadColumn("montante", "name", "targetname"),
            ReadColumn("dispositivo", "device", "tipo"),
            ReadColumn("ip", "targetip"));
    }

    private static DeviceCsvEntry ParsePositionalDeviceEntry(IReadOnlyList<string> row)
    {
        if (row.Count < 3)
        {
            throw new InvalidOperationException("Il CSV dispositivi richiede almeno 3 colonne: Montante;Dispositivo;IP");
        }

        return CreateDeviceEntry(row[0], row[1], row[2]);
    }

    private static DeviceCsvEntry CreateDeviceEntry(string? montante, string? device, string? ip)
    {
        if (string.IsNullOrWhiteSpace(montante))
        {
            throw new InvalidOperationException("Nel CSV dispositivi manca il montante.");
        }

        var normalizedDevice = NormalizeDeviceType(device);
        if (normalizedDevice is null)
        {
            throw new InvalidOperationException($"Tipo dispositivo non riconosciuto: '{device}'.");
        }

        if (string.IsNullOrWhiteSpace(ip))
        {
            throw new InvalidOperationException($"Nel CSV dispositivi manca l'IP per '{montante}'.");
        }

        return new DeviceCsvEntry(montante.Trim(), normalizedDevice, ip.Trim());
    }

    private static string? DetectTemplateDeviceType(string label)
    {
        foreach (var token in ExtractProjectDeviceTypes(label))
        {
            return token;
        }

        return null;
    }

    private static string? DetectClientTypeFromLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return null;
        }

        return ExtractProjectDeviceTypes(label).LastOrDefault()
            ?? DetectProjectDeviceTypeBySuffix(label);
    }

    private static IEnumerable<string> ExtractProjectDeviceTypes(string value)
    {
        return value
            .Split(['_', '-', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeDeviceType)
            .Where(IsProjectDeviceType)
            .Cast<string>();
    }

    private static bool IsProjectDeviceType(string? deviceType)
    {
        return deviceType is "OP" or "AS1" or "EV";
    }

    private static IEnumerable<TemplateClientLink> DiscoverIedClients(string templateRootPath)
    {
        foreach (var clientPath in Directory.EnumerateFiles(templateRootPath, "IED*.ccx", SearchOption.TopDirectoryOnly))
        {
            var clientDocument = XDocument.Load(clientPath, LoadOptions.PreserveWhitespace);
            var clientLabel = ReadElementValue(clientDocument, "general", "label")
                ?? ReadElementValue(clientDocument, "CCX_device", "name")
                ?? Path.GetFileNameWithoutExtension(clientPath);
            var deviceType = DetectClientTypeFromLabel(clientLabel);
            if (deviceType is null)
            {
                continue;
            }

            var clientIp = clientDocument
                .Descendants()
                .FirstOrDefault(node => node.Name.LocalName.Equals("IP_addr", StringComparison.OrdinalIgnoreCase))
                ?.Value
                ?.Trim();

            if (string.IsNullOrWhiteSpace(clientIp))
            {
                throw new InvalidOperationException($"Nel file '{Path.GetFileName(clientPath)}' manca il nodo <IP_addr>.");
            }

            yield return new TemplateClientLink(deviceType, Path.GetFileName(clientPath), clientIp);
        }
    }

    private static string? DetectProjectDeviceTypeBySuffix(string value)
    {
        var normalized = new string(value.Trim().Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        foreach (var suffix in DeviceAliases.Keys.OrderByDescending(key => key.Length))
        {
            if (normalized.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) &&
                DeviceAliases.TryGetValue(suffix, out var deviceType) &&
                IsProjectDeviceType(deviceType))
            {
                return deviceType;
            }
        }

        return null;
    }

    private static string? DetectServerIp(XDocument rtuDocument)
    {
        var main = rtuDocument
            .Descendants()
            .FirstOrDefault(node =>
                node.Name.LocalName.Equals("cnx", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(node.Attribute("main")?.Value, "true", StringComparison.OrdinalIgnoreCase));

        var fallback = rtuDocument
            .Descendants()
            .FirstOrDefault(node =>
                node.Name.LocalName.Equals("cnx", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(node.Attribute("name")?.Value, "eth1", StringComparison.OrdinalIgnoreCase));

        return (main ?? fallback)?.Attribute("IP_addr")?.Value;
    }

    private static string? ReadElementValue(XDocument document, string parentName, string childName)
    {
        return document
            .Descendants()
            .FirstOrDefault(node => node.Name.LocalName.Equals(parentName, StringComparison.OrdinalIgnoreCase))
            ?.Elements()
            .FirstOrDefault(node => node.Name.LocalName.Equals(childName, StringComparison.OrdinalIgnoreCase))
            ?.Value
            ?.Trim();
    }

    private static string? NormalizeDeviceType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = new string(value.Trim().Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        return DeviceAliases.TryGetValue(normalized, out var alias) ? alias : null;
    }

    private static IReadOnlyList<LegacyBatchRequest> LoadLegacyBatchRequests(
        string csvPath,
        string defaultTemplateName,
        string defaultTemplateIp,
        string? defaultTemplateCode,
        string? defaultTargetCode)
    {
        if (!File.Exists(csvPath))
        {
            throw new InvalidOperationException($"CSV non trovato: {csvPath}");
        }

        var lines = File.ReadAllLines(csvPath, Encoding.UTF8)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Where(line => !line.StartsWith('#'))
            .ToList();

        if (lines.Count == 0)
        {
            return [];
        }

        var delimiter = lines[0].Contains(';') ? ';' : ',';
        var firstRow = SplitRow(lines[0], delimiter);
        var hasHeader = IsLegacyCsvHeader(firstRow);
        var result = new List<LegacyBatchRequest>();

        if (hasHeader)
        {
            var headerMap = BuildHeaderMap(firstRow);
            foreach (var line in lines.Skip(1))
            {
                var row = SplitRow(line, delimiter);
                if (row.Count > 0)
                {
                    result.Add(ParseNamedLegacyRequest(row, headerMap, defaultTemplateName, defaultTemplateIp, defaultTemplateCode, defaultTargetCode));
                }
            }
        }
        else
        {
            foreach (var line in lines)
            {
                var row = SplitRow(line, delimiter);
                if (row.Count > 0)
                {
                    result.Add(ParsePositionalLegacyRequest(row, defaultTemplateName, defaultTemplateIp, defaultTemplateCode, defaultTargetCode));
                }
            }
        }

        return result;
    }

    private static bool IsLegacyCsvHeader(IReadOnlyList<string> row)
    {
        var knownHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "templatename", "template", "source", "from",
            "templateip", "sourceip", "fromip",
            "templatecode", "sourcecode", "fromcode", "inputcode", "codein", "quadriletteraletemplate",
            "targetname", "target", "name", "montante",
            "targetip", "ip", "targetcode", "tocode", "outputcode", "codeout", "quadriletteraleoutput",
            "find", "extrafind", "search", "trova", "replace", "extrareplace", "replacement", "sostituzione",
            "output", "outputname", "file", "filename"
        };

        return row.Select(NormalizeHeader).Any(knownHeaders.Contains);
    }

    private static LegacyBatchRequest ParseNamedLegacyRequest(
        IReadOnlyList<string> row,
        IReadOnlyDictionary<string, int> headerMap,
        string defaultTemplateName,
        string defaultTemplateIp,
        string? defaultTemplateCode,
        string? defaultTargetCode)
    {
        string? ReadColumn(params string[] names)
        {
            foreach (var name in names)
            {
                if (headerMap.TryGetValue(name, out var index) && index < row.Count)
                {
                    return NormalizeOptionalValue(row[index]);
                }
            }

            return null;
        }

        return CreateLegacyRequest(
            ReadColumn("templatename", "template", "source", "from") ?? defaultTemplateName,
            ReadColumn("templateip", "sourceip", "fromip") ?? defaultTemplateIp,
            ReadColumn("targetname", "target", "name", "montante"),
            ReadColumn("targetip", "ip"),
            ReadColumn("output", "outputname", "file", "filename"),
            ReadColumn("find", "extrafind", "search", "trova"),
            ReadColumn("replace", "extrareplace", "replacement", "sostituzione"),
            ReadColumn("templatecode", "sourcecode", "fromcode", "inputcode", "codein", "quadriletteraletemplate") ?? defaultTemplateCode,
            ReadColumn("targetcode", "tocode", "outputcode", "codeout", "quadriletteraleoutput") ?? defaultTargetCode);
    }

    private static LegacyBatchRequest ParsePositionalLegacyRequest(
        IReadOnlyList<string> row,
        string defaultTemplateName,
        string defaultTemplateIp,
        string? defaultTemplateCode,
        string? defaultTargetCode)
    {
        return row.Count switch
        {
            2 => CreateLegacyRequest(defaultTemplateName, defaultTemplateIp, row[0], row[1], row[0], null, null, defaultTemplateCode, defaultTargetCode),
            3 => CreateLegacyRequest(defaultTemplateName, defaultTemplateIp, row[0], row[1], row[2], null, null, defaultTemplateCode, defaultTargetCode),
            _ => throw new InvalidOperationException("Formato CSV legacy non supportato in questa versione.")
        };
    }

    private static LegacyBatchRequest CreateLegacyRequest(
        string? templateName,
        string? templateIp,
        string? targetName,
        string? targetIp,
        string? outputName,
        string? extraFind,
        string? extraReplace,
        string? templateCode,
        string? targetCode)
    {
        if (string.IsNullOrWhiteSpace(templateName) || string.IsNullOrWhiteSpace(templateIp))
        {
            throw new InvalidOperationException("Nel CSV legacy mancano i dati del template.");
        }

        if (string.IsNullOrWhiteSpace(targetName) || string.IsNullOrWhiteSpace(targetIp))
        {
            throw new InvalidOperationException("Nel CSV legacy mancano montante o IP target.");
        }

        if (!string.IsNullOrWhiteSpace(extraFind) && string.IsNullOrWhiteSpace(extraReplace))
        {
            throw new InvalidOperationException($"Manca il replace extra per '{targetName}'.");
        }

        var additional = new List<TextReplacement> { new(templateIp.Trim(), targetIp.Trim()) };
        if (!string.IsNullOrWhiteSpace(templateCode) && !string.IsNullOrWhiteSpace(targetCode))
        {
            additional.Add(new TextReplacement(templateCode.Trim(), targetCode.Trim()));
        }

        return new LegacyBatchRequest(
            targetName.Trim(),
            string.IsNullOrWhiteSpace(outputName) ? targetName.Trim() : outputName.Trim(),
            NormalizeOptionalValue(extraFind),
            NormalizeOptionalValue(extraReplace),
            additional);
    }

    private static IReadOnlyList<GenericReplaceJob> NormalizeGenericJobs(IReadOnlyList<GenericReplaceJob> jobs)
    {
        var normalized = jobs
            .Where(job => !string.IsNullOrWhiteSpace(job.ReplaceValue))
            .Select(job => job with
            {
                ReplaceValue = job.ReplaceValue.Trim(),
                OutputName = string.IsNullOrWhiteSpace(job.OutputName) ? job.ReplaceValue.Trim() : job.OutputName.Trim(),
                ExtraFind = NormalizeOptionalValue(job.ExtraFind),
                ExtraReplace = NormalizeOptionalValue(job.ExtraReplace)
            })
            .ToList();

        if (normalized.Count == 0)
        {
            throw new InvalidOperationException("Aggiungi almeno una riga di replace.");
        }

        foreach (var job in normalized)
        {
            if (!string.IsNullOrWhiteSpace(job.ExtraFind) && string.IsNullOrWhiteSpace(job.ExtraReplace))
            {
                throw new InvalidOperationException($"Manca il replace extra per '{job.OutputName}'.");
            }
        }

        return normalized;
    }

    private static string ApplyReplacements(string content, IReadOnlyList<TextReplacement> replacements)
    {
        return ApplyReplacements(content, replacements, out _);
    }

    private static string ApplyReplacements(string content, IReadOnlyList<TextReplacement> replacements, out int replacementCount)
    {
        var updated = content;
        replacementCount = 0;
        foreach (var replacement in replacements)
        {
            if (!string.IsNullOrWhiteSpace(replacement.Find))
            {
                replacementCount += CountOccurrences(updated, replacement.Find);
                updated = updated.Replace(replacement.Find, replacement.Replace, StringComparison.OrdinalIgnoreCase);
            }
        }

        return updated;
    }

    private static int CountOccurrences(string content, string find)
    {
        var count = 0;
        var startIndex = 0;
        while (startIndex < content.Length)
        {
            var index = content.IndexOf(find, startIndex, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                break;
            }

            count++;
            startIndex = index + find.Length;
        }

        return count;
    }

    private static void EnsureTemplateFiles(IReadOnlyList<TemplateItem> templateFiles, string templateRootPath)
    {
        if (!templateFiles.Any(file => !file.IsDirectory))
        {
            throw new InvalidOperationException($"Nessun file template trovato in '{templateRootPath}'.");
        }
    }

    private static string EnsureNoRestrictionSignature(string content)
    {
        if (content.Contains("<norestrictionsignature>", StringComparison.OrdinalIgnoreCase))
        {
            return content;
        }

        var openTagIndex = content.IndexOf("<general>", StringComparison.OrdinalIgnoreCase);
        if (openTagIndex < 0)
        {
            return content;
        }

        var closeTagIndex = content.IndexOf("</general>", openTagIndex, StringComparison.OrdinalIgnoreCase);
        if (closeTagIndex < 0)
        {
            return content;
        }

        var newline = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var generalStart = openTagIndex + "<general>".Length;
        var generalBody = content.Substring(generalStart, closeTagIndex - generalStart);
        var indent = DetectGeneralIndentation(generalBody);
        var insertion = generalBody.EndsWith(newline, StringComparison.Ordinal)
            ? $"{indent}<norestrictionsignature>true</norestrictionsignature>{newline}"
            : $"{newline}{indent}<norestrictionsignature>true</norestrictionsignature>{newline}";

        return content.Insert(closeTagIndex, insertion);
    }

    private static string DetectGeneralIndentation(string generalBody)
    {
        using var reader = new StringReader(generalBody);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var trimmed = line.TrimStart();
            var indentLength = line.Length - trimmed.Length;
            if (indentLength > 0)
            {
                return line[..indentLength];
            }

            break;
        }

        return "    ";
    }

    private static string CreateProjectArchive(string outputDirectory, string tempOutputDirectory, string outputName)
    {
        var archivePath = Path.Combine(outputDirectory, $"{outputName}.cprj");

        if (File.Exists(archivePath))
        {
            try
            {
                File.Delete(archivePath);
            }
            catch (IOException)
            {
                throw new IOException($"impossibile sovrascrivere '{outputName}.cprj' perche' il file e' aperto o bloccato.");
            }
        }

        using var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create);
        foreach (var directory in Directory.GetDirectories(tempOutputDirectory, "*", SearchOption.AllDirectories))
        {
            var relativeDirectory = Path.GetRelativePath(tempOutputDirectory, directory).Replace('\\', '/').TrimEnd('/') + "/";
            archive.CreateEntry(relativeDirectory);
        }

        foreach (var file in Directory.GetFiles(tempOutputDirectory, "*", SearchOption.AllDirectories))
        {
            var relativeFile = Path.GetRelativePath(tempOutputDirectory, file).Replace('\\', '/');
            archive.CreateEntryFromFile(file, relativeFile, CompressionLevel.Optimal);
        }

        return archivePath;
    }

    private static TemplateContext PrepareTemplateContext(string sourcePath)
    {
        if (Directory.Exists(sourcePath))
        {
            var cprjFiles = Directory.GetFiles(sourcePath, "*.cprj", SearchOption.TopDirectoryOnly);
            if (cprjFiles.Length == 1)
            {
                return PrepareTemplateContext(cprjFiles[0]);
            }

            if (cprjFiles.Length > 1)
            {
                throw new InvalidOperationException($"La cartella sorgente contiene piu' .cprj. Specifica direttamente il file template: {sourcePath}");
            }

            return new TemplateContext(sourcePath, null);
        }

        if (!File.Exists(sourcePath))
        {
            throw new InvalidOperationException($"Sorgente non trovata: {sourcePath}");
        }

        var extension = Path.GetExtension(sourcePath);
        if (!extension.Equals(".cprj", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Il file sorgente deve essere .cprj oppure .zip.");
        }

        var extractionRoot = Path.Combine(Path.GetTempPath(), $"cprj_template_{Guid.NewGuid():N}");
        Directory.CreateDirectory(extractionRoot);
        ZipFile.ExtractToDirectory(sourcePath, extractionRoot);

        return new TemplateContext(NormalizeExtractedRoot(extractionRoot), extractionRoot);
    }

    private static string NormalizeExtractedRoot(string extractionRoot)
    {
        if (Directory.GetFiles(extractionRoot).Length > 0)
        {
            return extractionRoot;
        }

        var topDirectories = Directory.GetDirectories(extractionRoot);
        return topDirectories.Length == 1 ? topDirectories[0] : extractionRoot;
    }

    private static IReadOnlyList<TemplateItem> GetTemplateItems(string templateRootPath)
    {
        var root = new DirectoryInfo(templateRootPath);
        var directories = root
            .GetDirectories("*", SearchOption.AllDirectories)
            .Select(directory => TemplateItem.ForDirectory(directory.FullName, Path.GetRelativePath(templateRootPath, directory.FullName)));

        var files = root
            .GetFiles("*", SearchOption.AllDirectories)
            .Select(file => TemplateItem.ForFile(file.FullName, Path.GetRelativePath(templateRootPath, file.FullName)));

        return directories
            .Concat(files)
            .OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string CreateTempDirectory(string rootPath)
    {
        var tempDirectory = Path.Combine(rootPath, $".tmp_cprj_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        return tempDirectory;
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    private static string ResolveSourcePath(string sourcePath)
    {
        var normalized = NormalizeOptionalValue(sourcePath);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Percorso sorgente non specificato.");
        }

        return Path.GetFullPath(normalized.Trim('"'));
    }

    private static string ResolveOutputDirectory(string? requestedOutputDirectory)
    {
        var resolved = string.IsNullOrWhiteSpace(requestedOutputDirectory)
            ? DefaultOutputDirectory
            : Path.GetFullPath(requestedOutputDirectory.Trim().Trim('"'));

        Directory.CreateDirectory(resolved);
        return resolved;
    }

    private static string? ResolveOptionalPath(string? requestedPath, string sourcePath)
    {
        var normalized = NormalizeOptionalValue(requestedPath);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (Path.IsPathRooted(normalized))
        {
            return Path.GetFullPath(normalized);
        }

        var baseDirectory = Directory.Exists(sourcePath)
            ? sourcePath
            : Path.GetDirectoryName(sourcePath) ?? Environment.CurrentDirectory;

        return Path.GetFullPath(Path.Combine(baseDirectory, normalized));
    }

    private static Dictionary<string, int> BuildHeaderMap(IReadOnlyList<string> header)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < header.Count; i++)
        {
            var normalized = NormalizeHeader(header[i]);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                map[normalized] = i;
            }
        }

        return map;
    }

    private static List<string> SplitRow(string line, char delimiter) =>
        line.Split(delimiter).Select(part => part.Trim().Trim('"')).ToList();

    private static string NormalizeHeader(string header)
    {
        var buffer = new StringBuilder();
        foreach (var ch in header)
        {
            if (char.IsLetterOrDigit(ch))
            {
                buffer.Append(char.ToLowerInvariant(ch));
            }
        }

        return buffer.ToString();
    }

    private static string? NormalizeOptionalValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record BuildPlan(
        string OutputName,
        IReadOnlyList<TextReplacement> FileNameReplacements,
        IReadOnlyList<TextReplacement> GlobalReplacements,
        IReadOnlyDictionary<string, List<TextReplacement>> FileSpecificReplacements);

    private sealed record TemplateItem(string FullPath, string RelativePath, bool IsDirectory)
    {
        public static TemplateItem ForDirectory(string fullPath, string relativePath) =>
            new(fullPath, NormalizeRelativePath(relativePath), true);

        public static TemplateItem ForFile(string fullPath, string relativePath) =>
            new(fullPath, NormalizeRelativePath(relativePath), false);

        private static string NormalizeRelativePath(string relativePath) =>
            relativePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
    }

    private sealed record DeviceCsvEntry(string Montante, string DeviceType, string Ip);

    private sealed record DeviceProject(string Montante, IReadOnlyDictionary<string, string> DeviceIps);

    private sealed record LegacyBatchRequest(
        string TargetName,
        string OutputName,
        string? ExtraFind,
        string? ExtraReplace,
        IReadOnlyList<TextReplacement> AdditionalReplacements);

    private sealed class TemplateContext(string templateRootPath, string? cleanupRootPath) : IDisposable
    {
        public string TemplateRootPath { get; } = templateRootPath;

        public void Dispose()
        {
            if (!string.IsNullOrWhiteSpace(cleanupRootPath) && Directory.Exists(cleanupRootPath))
            {
                Directory.Delete(cleanupRootPath, true);
            }
        }
    }
}
