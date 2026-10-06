using System.Diagnostics;

namespace CreaCprjMontante;

internal sealed class MainForm : Form
{
    private static readonly string LastPathsFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CreaCprjMontante",
        "last_paths.txt");

    private readonly TextBox _sourceTextBox = new() { Dock = DockStyle.Fill };
    private readonly TextBox _outputTextBox = new() { Dock = DockStyle.Fill };
    private readonly TextBox _genericFindTextBox = new() { Dock = DockStyle.Fill, Text = "3M11" };
    private readonly TextBox _deviceTemplateNameTextBox = new() { Dock = DockStyle.Fill, Text = "3M11" };
    private readonly TextBox _deviceCsvTextBox = new() { Dock = DockStyle.Fill };
    private readonly TextBox _deviceQuadFindTextBox = new() { Dock = DockStyle.Fill, Text = "PTOV" };
    private readonly TextBox _deviceQuadReplaceTextBox = new() { Dock = DockStyle.Fill, Text = "TNOT" };
    private readonly TextBox _genericCsvTextBox = new() { Dock = DockStyle.Fill };
    private readonly TextBox _moduleNameTextBox = new() { Dock = DockStyle.Fill, Text = "1M14_AS1_PING" };
    private readonly ComboBox _moduleTypeComboBox = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _moduleTargetTextBox = new() { Dock = DockStyle.Fill };
    private readonly TextBox _moduleCsvTextBox = new() { Dock = DockStyle.Fill };
    private readonly TextBox _moduleQuadFindTextBox = new() { Dock = DockStyle.Fill, Text = "PTOV" };
    private readonly TextBox _moduleQuadReplaceTextBox = new() { Dock = DockStyle.Fill, Text = "TNOT" };
    private readonly Label _genericHintLabel = new()
    {
        Dock = DockStyle.Fill,
        AutoSize = true,
        Text = "Find principale = testo da sostituire. Con un .cprj genera progetti, con un file testo genera file modificati."
    };
    private readonly Label _genericExampleLabel = new()
    {
        Dock = DockStyle.Fill,
        AutoSize = true,
        ForeColor = Color.DimGray,
        Text = "Esempio riga: Replace=ACEL | Output=file_ACEL.txt (opz.) | ExtraFind=PTOV | Sostituisci=TNOT"
    };
    private readonly TextBox _logTextBox = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        BackColor = Color.White,
        Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point)
    };

    private readonly DataGridView _genericGrid = new()
    {
        Dock = DockStyle.Fill,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
        RowHeadersVisible = false,
        BackgroundColor = Color.White
    };

    private readonly DataGridView _genericCsvMappingGrid = new()
    {
        Dock = DockStyle.Fill,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        RowHeadersVisible = false,
        BackgroundColor = Color.White
    };

    private readonly DataGridView _moduleReplacementGrid = new()
    {
        Dock = DockStyle.Fill,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        RowHeadersVisible = false,
        BackgroundColor = Color.White
    };

    private readonly Button _browseSourceFileButton = new() { Text = "File...", AutoSize = true };
    private readonly Button _browseSourceFolderButton = new() { Text = "Cartella...", AutoSize = true };
    private readonly Button _browseOutputButton = new() { Text = "Sfoglia...", AutoSize = true };
    private readonly Button _openOutputButton = new() { Text = "Apri Output", AutoSize = true };
    private readonly Button _addGenericRowButton = new() { Text = "Aggiungi Riga", AutoSize = true };
    private readonly Button _removeGenericRowButton = new() { Text = "Rimuovi Riga", AutoSize = true };
    private readonly Button _generateGenericButton = new() { Text = "Genera Batch", AutoSize = true };
    private readonly Button _browseDeviceCsvButton = new() { Text = "File...", AutoSize = true };
    private readonly Button _inspectTemplateButton = new() { Text = "Analizza Template", AutoSize = true };
    private readonly Button _inspectDeviceCsvButton = new() { Text = "Analizza File", AutoSize = true };
    private readonly Button _generateDeviceButton = new() { Text = "Genera da File", AutoSize = true };
    private readonly Button _insertPingersButton = new() { Text = "Inserisci PINGER da File", AutoSize = true };
    private readonly Button _browseGenericCsvButton = new() { Text = "File...", AutoSize = true };
    private readonly Button _inspectGenericCsvButton = new() { Text = "Analizza File", AutoSize = true };
    private readonly Button _addGenericCsvMappingButton = new() { Text = "Aggiungi Mapping", AutoSize = true };
    private readonly Button _removeGenericCsvMappingButton = new() { Text = "Rimuovi Mapping", AutoSize = true };
    private readonly Button _generateGenericCsvButton = new() { Text = "Genera File Generico", AutoSize = true };
    private readonly Button _inspectModuleButton = new() { Text = "Analizza Modulo", AutoSize = true };
    private readonly Button _browseModuleTargetFileButton = new() { Text = "File...", AutoSize = true };
    private readonly Button _browseModuleTargetFolderButton = new() { Text = "Cartella...", AutoSize = true };
    private readonly Button _browseModuleCsvButton = new() { Text = "File...", AutoSize = true };
    private readonly Button _insertModulePingersButton = new() { Text = "Inserisci PINGER da CSV", AutoSize = true };
    private readonly Button _addModuleReplacementButton = new() { Text = "Aggiungi Sostituzione", AutoSize = true };
    private readonly Button _removeModuleReplacementButton = new() { Text = "Rimuovi Sostituzione", AutoSize = true };
    private readonly Button _copyModuleButton = new() { Text = "Copia Modulo", AutoSize = true };
    private readonly Label _statusLabel = new() { Dock = DockStyle.Fill, AutoSize = true, Text = "Pronto", Padding = new Padding(0, 6, 0, 0) };
    private readonly Label _templateInfoLabel = new() { Dock = DockStyle.Fill, AutoSize = true, Text = "Tipo template: non analizzato" };
    private readonly Label _csvInfoLabel = new() { Dock = DockStyle.Fill, AutoSize = true, Text = "File dispositivi: non analizzato" };
    private readonly Label _genericCsvInfoLabel = new() { Dock = DockStyle.Fill, AutoSize = true, Text = "File generico: non analizzato" };
    private readonly Label _moduleInfoLabel = new() { Dock = DockStyle.Fill, AutoSize = true, Text = "Modulo: non analizzato" };

    public MainForm()
    {
        Text = "THYTOOLS | Crea CPRJ Montante | V7";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 820);
        Size = new Size(1180, 880);
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        _outputTextBox.Text = CprjGenerator.DefaultOutputDirectory;

        ConfigureGenericGrid();
        ConfigureGenericCsvMappingGrid();
        ConfigureModuleReplacementGrid();
        ConfigureModuleTypeComboBox();
        LoadSavedPaths();
        BuildLayout();
        WireEvents();
        Thytools.Ui.SuiteTheme.UseAccentButton(_generateGenericButton);
        Thytools.Ui.SuiteTheme.UseAccentButton(_generateDeviceButton);
        Thytools.Ui.SuiteTheme.UseAccentButton(_generateGenericCsvButton);
        Thytools.Ui.SuiteTheme.UseAccentButton(_copyModuleButton);
        AddGenericRow();
        AddDefaultGenericCsvMappingRows();
        AddModuleReplacementRow();

        AppendLog("Output predefinito: " + CprjGenerator.DefaultOutputDirectory);
        AppendLog("In File generico, una cartella viene copiata come cartella e non trasformata in .cprj.");
        AppendLog("V5: aggiunta analisi impatto modulo.");
    }

    private void ConfigureGenericGrid()
    {
        _genericGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ReplaceValue", HeaderText = "Nuovo valore (Replace)", FillWeight = 26 });
        _genericGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "OutputName", HeaderText = "Nome file output (opz.)", FillWeight = 22 });
        _genericGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ExtraFind", HeaderText = "Trova extra (opz.)", FillWeight = 26 });
        _genericGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ExtraReplace", HeaderText = "Sostituisci con (opz.)", FillWeight = 26 });
    }

    private void ConfigureGenericCsvMappingGrid()
    {
        _genericCsvMappingGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CsvColumn", HeaderText = "Col.", Width = 80 });
        _genericCsvMappingGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "FindText", HeaderText = "Testo template da sostituire", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _genericCsvMappingGrid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "UseAsOutputName", HeaderText = "Nome file", Width = 90 });
    }

    private void ConfigureModuleReplacementGrid()
    {
        _moduleReplacementGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "FindText", HeaderText = "Trova nel modulo" });
        _moduleReplacementGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ReplaceText", HeaderText = "Sostituisci con" });
    }

    private void ConfigureModuleTypeComboBox()
    {
        _moduleTypeComboBox.Items.AddRange(["IED 61850", "PINGER"]);
        _moduleTypeComboBox.SelectedIndex = 0;
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(BuildSourceGroup(), 0, 0);
        root.Controls.Add(Thytools.Ui.SuiteTheme.SplitPanels(BuildModeTabs(), BuildLogGroup(), Orientation.Horizontal, .72), 0, 1);
        root.Controls.Add(BuildStatusPanel(), 0, 2);

        Thytools.Ui.SuiteTheme.AttachWithHeader(this, root, "Crea CPRJ Montante | V7",
            "Template, anteprima e generazione di progetti CPRJ");
    }

    private Control BuildSourceGroup()
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Text = "Template e Output",
            Padding = new Padding(12)
        };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 3,
            AutoSize = true
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        grid.Controls.Add(CreateLabel("Sorgente (.cprj, cartella o file testo)"), 0, 0);
        grid.Controls.Add(_sourceTextBox, 1, 0);
        grid.Controls.Add(_browseSourceFileButton, 2, 0);
        grid.Controls.Add(_browseSourceFolderButton, 3, 0);

        grid.Controls.Add(CreateLabel("Cartella output"), 0, 1);
        grid.Controls.Add(_outputTextBox, 1, 1);
        grid.Controls.Add(_browseOutputButton, 2, 1);
        grid.Controls.Add(_openOutputButton, 3, 1);

        var hintLabel = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "La tab Generica usa un find unico e piu replace. DIGIS usa il formato dispositivi. File generico usa mapping colonna -> testo."
        };
        grid.Controls.Add(hintLabel, 1, 2);
        grid.SetColumnSpan(hintLabel, 3);

        group.Controls.Add(grid);
        return group;
    }

    private Control BuildModeTabs()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
        var genericTab = new TabPage("Sostituzioni Generiche");
        var deviceTab = new TabPage("Progetti DIGIS da file");
        var genericCsvTab = new TabPage("File generico");
        var moduleTab = new TabPage("Inserisci Modulo");

        genericTab.Controls.Add(BuildGenericTab());
        deviceTab.Controls.Add(BuildDeviceTab());
        genericCsvTab.Controls.Add(BuildGenericCsvTab());
        moduleTab.Controls.Add(BuildModuleAnalysisTab());

        tabs.TabPages.Add(genericTab);
        tabs.TabPages.Add(deviceTab);
        tabs.TabPages.Add(genericCsvTab);
        tabs.TabPages.Add(moduleTab);
        return tabs;
    }

    private Control BuildGenericTab()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var top = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        top.Controls.Add(CreateLabel("Testo template (Find)"), 0, 0);
        top.Controls.Add(_genericFindTextBox, 1, 0);
        top.Controls.Add(_genericHintLabel, 1, 1);
        top.Controls.Add(_genericExampleLabel, 1, 2);

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        footer.Controls.Add(_addGenericRowButton);
        footer.Controls.Add(_removeGenericRowButton);
        footer.Controls.Add(_generateGenericButton);

        var infoLabel = new Label
        {
            AutoSize = true,
            Padding = new Padding(12, 8, 0, 0),
            Text = "Suggerimento: con file singolo, Output vuoto usa il nome derivato dal Find; se non cambia, usa il Replace."
        };
        footer.Controls.Add(infoLabel);

        root.Controls.Add(top, 0, 0);
        root.Controls.Add(_genericGrid, 0, 1);
        root.Controls.Add(footer, 0, 2);
        return root;
    }

    private Control BuildDeviceTab()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var line1 = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 5, AutoSize = true };
        line1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        line1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        line1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        line1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        line1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        line1.Controls.Add(CreateLabel("Montante template (Find)"), 0, 0);
        line1.Controls.Add(_deviceTemplateNameTextBox, 1, 0);
        line1.Controls.Add(_inspectTemplateButton, 2, 0);
        line1.Controls.Add(_generateDeviceButton, 3, 0);
        line1.Controls.Add(_insertPingersButton, 4, 0);

        var line2 = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, AutoSize = true };
        line2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        line2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        line2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        line2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        line2.Controls.Add(CreateLabel("File dispositivi"), 0, 0);
        line2.Controls.Add(_deviceCsvTextBox, 1, 0);
        line2.Controls.Add(_browseDeviceCsvButton, 2, 0);
        line2.Controls.Add(_inspectDeviceCsvButton, 3, 0);

        var line3 = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, AutoSize = true };
        line3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        line3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        line3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        line3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        line3.Controls.Add(CreateLabel("Quadriletterale base (opz.)"), 0, 0);
        line3.Controls.Add(_deviceQuadFindTextBox, 1, 0);
        line3.Controls.Add(CreateLabel("Quadriletterale nuovo (opz.)"), 2, 0);
        line3.Controls.Add(_deviceQuadReplaceTextBox, 3, 0);

        var info = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "CSV/XLSX: Montante;Dispositivo;IP;Label opzionale. Label vale solo per AS/AS1 e aggiorna LABEL_BCU in states.ccx."
        };

        root.Controls.Add(line1, 0, 0);
        root.Controls.Add(line2, 0, 1);
        root.Controls.Add(line3, 0, 2);
        root.Controls.Add(_templateInfoLabel, 0, 3);
        root.Controls.Add(_csvInfoLabel, 0, 4);
        root.Controls.Add(info, 0, 5);
        return root;
    }

    private Control BuildGenericCsvTab()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var line1 = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, AutoSize = true };
        line1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        line1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        line1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        line1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        line1.Controls.Add(CreateLabel("File generico"), 0, 0);
        line1.Controls.Add(_genericCsvTextBox, 1, 0);
        line1.Controls.Add(_browseGenericCsvButton, 2, 0);
        line1.Controls.Add(_inspectGenericCsvButton, 3, 0);

        var info = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ForeColor = Color.DimGray,
            Text = "Esempio senza intestazioni: L465;172.26.135.60;L465_rev1. Mapping 1->3M11, 2->IP template. Spunta Nome file sulla colonna da usare."
        };

        var footer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        footer.Controls.Add(_addGenericCsvMappingButton);
        footer.Controls.Add(_removeGenericCsvMappingButton);
        footer.Controls.Add(_generateGenericCsvButton);

        root.Controls.Add(line1, 0, 0);
        root.Controls.Add(_genericCsvInfoLabel, 0, 1);
        root.Controls.Add(info, 0, 2);
        root.Controls.Add(_genericCsvMappingGrid, 0, 3);
        root.Controls.Add(footer, 0, 4);
        return root;
    }

    private Control BuildModuleAnalysisTab()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 9,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var line1 = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 3, AutoSize = true };
        line1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        line1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        line1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        line1.Controls.Add(CreateLabel("Modulo da cercare"), 0, 0);
        line1.Controls.Add(_moduleNameTextBox, 1, 0);
        line1.Controls.Add(_inspectModuleButton, 2, 0);

        var lineType = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, AutoSize = true };
        lineType.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        lineType.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        lineType.Controls.Add(CreateLabel("Tipo modulo"), 0, 0);
        lineType.Controls.Add(_moduleTypeComboBox, 1, 0);

        var line2 = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 5, AutoSize = true };
        line2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        line2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        line2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        line2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        line2.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        line2.Controls.Add(CreateLabel("Progetti destinazione"), 0, 0);
        line2.Controls.Add(_moduleTargetTextBox, 1, 0);
        line2.Controls.Add(_browseModuleTargetFileButton, 2, 0);
        line2.Controls.Add(_browseModuleTargetFolderButton, 3, 0);
        line2.Controls.Add(_copyModuleButton, 4, 0);

        var lineCsv = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, AutoSize = true };
        lineCsv.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        lineCsv.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        lineCsv.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        lineCsv.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        lineCsv.Controls.Add(CreateLabel("CSV matrice PINGER"), 0, 0);
        lineCsv.Controls.Add(_moduleCsvTextBox, 1, 0);
        lineCsv.Controls.Add(_browseModuleCsvButton, 2, 0);
        lineCsv.Controls.Add(_insertModulePingersButton, 3, 0);

        var linePlant = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 4, AutoSize = true };
        linePlant.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        linePlant.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        linePlant.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        linePlant.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        linePlant.Controls.Add(CreateLabel("Quadriletterale base (opz.)"), 0, 0);
        linePlant.Controls.Add(_moduleQuadFindTextBox, 1, 0);
        linePlant.Controls.Add(CreateLabel("Quadriletterale nuovo (opz.)"), 2, 0);
        linePlant.Controls.Add(_moduleQuadReplaceTextBox, 3, 0);

        var replacementsLabel = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Sostituzioni custom sul modulo copiato"
        };

        var replacementFooter = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        replacementFooter.Controls.Add(_addModuleReplacementButton);
        replacementFooter.Controls.Add(_removeModuleReplacementButton);

        var info = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ForeColor = Color.DimGray,
            Text = "Analisi: trova DBX/CCX, nodo rtu.ccx, ddbb.dbx e tag. Copia Modulo crea nuovi .cprj in output senza modificare gli originali."
        };

        root.Controls.Add(line1, 0, 0);
        root.Controls.Add(lineType, 0, 1);
        root.Controls.Add(line2, 0, 2);
        root.Controls.Add(lineCsv, 0, 3);
        root.Controls.Add(linePlant, 0, 4);
        root.Controls.Add(_moduleInfoLabel, 0, 5);
        root.Controls.Add(replacementsLabel, 0, 6);
        root.Controls.Add(_moduleReplacementGrid, 0, 7);
        root.Controls.Add(replacementFooter, 0, 8);
        root.Controls.Add(info, 0, 9);
        return root;
    }

    private Control BuildLogGroup()
    {
        var group = new GroupBox { Dock = DockStyle.Fill, Text = "Log", Padding = new Padding(12) };
        group.Controls.Add(_logTextBox);
        return group;
    }

    private Control BuildStatusPanel()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, AutoSize = true };
        panel.Controls.Add(_statusLabel, 0, 0);
        return panel;
    }

    private void WireEvents()
    {
        _browseSourceFileButton.Click += (_, _) => BrowseSourceFile();
        _browseSourceFolderButton.Click += (_, _) => BrowseSourceFolder();
        _browseOutputButton.Click += (_, _) => BrowseOutputFolder();
        _openOutputButton.Click += (_, _) => OpenOutputFolder();
        _addGenericRowButton.Click += (_, _) => AddGenericRow();
        _removeGenericRowButton.Click += (_, _) => RemoveGenericRow();
        _generateGenericButton.Click += async (_, _) => await GenerateGenericAsync();
        _browseDeviceCsvButton.Click += (_, _) => BrowseDeviceCsvFile();
        _inspectTemplateButton.Click += async (_, _) => await InspectTemplateAsync();
        _inspectDeviceCsvButton.Click += async (_, _) => await InspectDeviceCsvAsync();
        _generateDeviceButton.Click += async (_, _) => await GenerateDeviceAsync();
        _insertPingersButton.Click += async (_, _) => await InsertPingersAsync();
        _browseGenericCsvButton.Click += (_, _) => BrowseGenericCsvFile();
        _inspectGenericCsvButton.Click += async (_, _) => await InspectGenericCsvAsync();
        _addGenericCsvMappingButton.Click += (_, _) => AddGenericCsvMappingRow();
        _removeGenericCsvMappingButton.Click += (_, _) => RemoveGenericCsvMappingRow();
        _generateGenericCsvButton.Click += async (_, _) => await GenerateGenericCsvAsync();
        _inspectModuleButton.Click += async (_, _) => await InspectModuleAsync();
        _browseModuleTargetFileButton.Click += (_, _) => BrowseModuleTargetFile();
        _browseModuleTargetFolderButton.Click += (_, _) => BrowseModuleTargetFolder();
        _browseModuleCsvButton.Click += (_, _) => BrowseModuleCsvFile();
        _insertModulePingersButton.Click += async (_, _) => await InsertModulePingersAsync();
        _addModuleReplacementButton.Click += (_, _) => AddModuleReplacementRow();
        _removeModuleReplacementButton.Click += (_, _) => RemoveModuleReplacementRow();
        _copyModuleButton.Click += async (_, _) => await CopyModuleAsync();
        FormClosing += (_, _) => SaveSavedPaths();
        _genericCsvMappingGrid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_genericCsvMappingGrid.IsCurrentCellDirty)
            {
                _genericCsvMappingGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
        _genericCsvMappingGrid.CellValueChanged += (_, args) =>
        {
            if (args.RowIndex < 0 || _genericCsvMappingGrid.Columns[args.ColumnIndex].Name != "UseAsOutputName")
            {
                return;
            }

            if (_genericCsvMappingGrid.Rows[args.RowIndex].Cells["UseAsOutputName"].Value is not true)
            {
                return;
            }

            foreach (DataGridViewRow row in _genericCsvMappingGrid.Rows)
            {
                if (row.Index != args.RowIndex)
                {
                    row.Cells["UseAsOutputName"].Value = false;
                }
            }
        };
    }

    private void LoadSavedPaths()
    {
        if (!File.Exists(LastPathsFile))
        {
            return;
        }

        try
        {
            var values = File.ReadAllLines(LastPathsFile)
                .Select(line => line.Split('=', 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);

            if (values.TryGetValue("Source", out var source) && !string.IsNullOrWhiteSpace(source))
            {
                _sourceTextBox.Text = source;
            }

            if (values.TryGetValue("Output", out var output) && !string.IsNullOrWhiteSpace(output))
            {
                _outputTextBox.Text = output;
            }

            if (values.TryGetValue("GenericCsv", out var genericCsv) && !string.IsNullOrWhiteSpace(genericCsv))
            {
                _genericCsvTextBox.Text = genericCsv;
            }

            if (values.TryGetValue("ModuleTarget", out var moduleTarget) && !string.IsNullOrWhiteSpace(moduleTarget))
            {
                _moduleTargetTextBox.Text = moduleTarget;
            }

            if (values.TryGetValue("ModuleCsv", out var moduleCsv) && !string.IsNullOrWhiteSpace(moduleCsv))
            {
                _moduleCsvTextBox.Text = moduleCsv;
            }
        }
        catch
        {
        }
    }

    private void SaveSavedPaths()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LastPathsFile) ?? AppContext.BaseDirectory);
            File.WriteAllLines(LastPathsFile, new[]
            {
                $"Source={_sourceTextBox.Text}",
                $"Output={_outputTextBox.Text}",
                $"GenericCsv={_genericCsvTextBox.Text}",
                $"ModuleTarget={_moduleTargetTextBox.Text}",
                $"ModuleCsv={_moduleCsvTextBox.Text}"
            });
        }
        catch
        {
        }
    }

    private void AddGenericRow() => _genericGrid.Rows.Add("", "", "", "");

    private void AddGenericCsvMappingRow() => _genericCsvMappingGrid.Rows.Add("", "", false);

    private void AddModuleReplacementRow() => _moduleReplacementGrid.Rows.Add("", "");

    private void AddDefaultGenericCsvMappingRows()
    {
        for (var i = 1; i <= 6; i++)
        {
            _genericCsvMappingGrid.Rows.Add(i.ToString(), "", i == 1);
        }
    }

    private void RemoveGenericRow()
    {
        if (_genericGrid.SelectedRows.Count == 0 && _genericGrid.Rows.Count > 0)
        {
            _genericGrid.Rows.RemoveAt(_genericGrid.Rows.Count - 1);
            return;
        }

        foreach (DataGridViewRow row in _genericGrid.SelectedRows)
        {
            if (!row.IsNewRow)
            {
                _genericGrid.Rows.Remove(row);
            }
        }
    }

    private void RemoveGenericCsvMappingRow()
    {
        if (_genericCsvMappingGrid.SelectedRows.Count == 0 && _genericCsvMappingGrid.Rows.Count > 0)
        {
            _genericCsvMappingGrid.Rows.RemoveAt(_genericCsvMappingGrid.Rows.Count - 1);
            return;
        }

        foreach (DataGridViewRow row in _genericCsvMappingGrid.SelectedRows)
        {
            if (!row.IsNewRow)
            {
                _genericCsvMappingGrid.Rows.Remove(row);
            }
        }
    }

    private async Task GenerateGenericAsync()
    {
        try
        {
            ValidateSourceAndOutput();
            SetBusy(true, "Generazione batch generico in corso...");
            AppendLog("Avvio sostituzioni generiche...");

            var jobs = ReadGenericJobs();
            var createdFiles = await Task.Run(() =>
                CprjGenerator.BuildGenericBatch(
                    _sourceTextBox.Text,
                    _outputTextBox.Text,
                    _genericFindTextBox.Text,
                    jobs,
                    AppendLog));

            SetStatus("Completato");
            MessageBox.Show(this, $"Creati {createdFiles.Count} file .cprj.", "Batch completato", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            AppendLog("ERRORE: " + ex.Message);
            SetStatus("Errore");
            MessageBox.Show(this, ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, "Pronto");
        }
    }

    private async Task InspectTemplateAsync()
    {
        try
        {
            ValidateSourceAndOutput();
            SetBusy(true, "Analisi template in corso...");
            var inspection = await Task.Run(() =>
                CprjGenerator.InspectTemplate(_sourceTextBox.Text, _deviceTemplateNameTextBox.Text));

            var clientSummary = inspection.Clients.Count == 0
                ? "nessun client"
                : string.Join(", ", inspection.Clients.Select(client => $"{client.DeviceType}:{client.Href}"));

            _templateInfoLabel.Text = $"Tipo template: {inspection.TemplateDeviceType} | IP server: {inspection.ServerIp} | Client: {clientSummary}";
            AppendLog(_templateInfoLabel.Text);
            SetStatus("Template analizzato");
        }
        catch (Exception ex)
        {
            AppendLog("ERRORE: " + ex.Message);
            SetStatus("Errore");
            MessageBox.Show(this, ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, "Pronto");
        }
    }

    private async Task InspectDeviceCsvAsync()
    {
        try
        {
            ValidateSourceAndOutput();
            if (string.IsNullOrWhiteSpace(_deviceCsvTextBox.Text))
            {
                throw new InvalidOperationException("Seleziona il file dispositivi.");
            }

            SetBusy(true, "Analisi file dispositivi in corso...");
            var inspection = await Task.Run(() =>
                CprjGenerator.InspectDeviceCsv(_sourceTextBox.Text, _deviceCsvTextBox.Text, AppendLog));

            var deviceTypes = inspection.DeviceTypes.Count == 0
                ? "nessun dispositivo"
                : string.Join(", ", inspection.DeviceTypes);

            _csvInfoLabel.Text = $"File: {inspection.ProjectCount} montanti | {inspection.EntryCount} righe | Tipi: {deviceTypes}";
            AppendLog(_csvInfoLabel.Text);
            foreach (var project in inspection.Projects)
            {
                var deviceSummary = string.Join(", ", project.Devices.Select(device =>
                    string.IsNullOrWhiteSpace(device.Label)
                        ? $"{device.DeviceType}={device.Ip}"
                        : $"{device.DeviceType}={device.Ip} LABEL_BCU=\"{device.Label}\""));
                AppendLog($"File {project.Montante}: {deviceSummary}");
            }

            SetStatus("File analizzato");
        }
        catch (Exception ex)
        {
            AppendLog("ERRORE: " + ex.Message);
            SetStatus("Errore");
            MessageBox.Show(this, ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, "Pronto");
        }
    }

    private async Task GenerateDeviceAsync()
    {
        try
        {
            ValidateSourceAndOutput();
            if (string.IsNullOrWhiteSpace(_deviceTemplateNameTextBox.Text))
            {
                throw new InvalidOperationException("Inserisci il montante template.");
            }

            if (string.IsNullOrWhiteSpace(_deviceCsvTextBox.Text))
            {
                throw new InvalidOperationException("Seleziona il file dispositivi.");
            }

            var quadFind = _deviceQuadFindTextBox.Text?.Trim();
            var quadReplace = _deviceQuadReplaceTextBox.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(quadFind) && string.IsNullOrWhiteSpace(quadReplace))
            {
                throw new InvalidOperationException("Inserisci il quadriletterale nuovo.");
            }

            if (string.IsNullOrWhiteSpace(quadFind) && !string.IsNullOrWhiteSpace(quadReplace))
            {
                throw new InvalidOperationException("Inserisci il quadriletterale base.");
            }

            SetBusy(true, "Generazione da file dispositivi in corso...");
            AppendLog("Avvio progetti da file...");

            var createdFiles = await Task.Run(() =>
                CprjGenerator.BuildDeviceProjectsFromCsv(
                    _sourceTextBox.Text,
                    _outputTextBox.Text,
                    _deviceTemplateNameTextBox.Text,
                    _deviceCsvTextBox.Text,
                    quadFind,
                    quadReplace,
                    AppendLog));

            SetStatus("Completato");
            MessageBox.Show(this, $"Creati {createdFiles.Count} file .cprj.", "File completato", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            AppendLog("ERRORE: " + ex.Message);
            SetStatus("Errore");
            MessageBox.Show(this, ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, "Pronto");
        }
    }

    private async Task InsertPingersAsync()
    {
        try
        {
            ValidateSourceAndOutput();
            if (string.IsNullOrWhiteSpace(_deviceTemplateNameTextBox.Text))
            {
                throw new InvalidOperationException("Inserisci il montante template PINGER.");
            }

            if (string.IsNullOrWhiteSpace(_deviceCsvTextBox.Text))
            {
                throw new InvalidOperationException("Seleziona il file dispositivi.");
            }

            var quadFind = _deviceQuadFindTextBox.Text?.Trim();
            var quadReplace = _deviceQuadReplaceTextBox.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(quadFind) && string.IsNullOrWhiteSpace(quadReplace))
            {
                throw new InvalidOperationException("Inserisci il quadriletterale nuovo.");
            }

            if (string.IsNullOrWhiteSpace(quadFind) && !string.IsNullOrWhiteSpace(quadReplace))
            {
                throw new InvalidOperationException("Inserisci il quadriletterale base.");
            }

            SetBusy(true, "Inserimento PINGER in corso...");
            AppendLog("Avvio inserimento PINGER da file...");

            var result = await Task.Run(() =>
                CprjGenerator.InsertPingerModulesFromDeviceCsv(
                    _sourceTextBox.Text,
                    null,
                    _outputTextBox.Text,
                    _deviceTemplateNameTextBox.Text,
                    _deviceCsvTextBox.Text,
                    quadFind,
                    quadReplace,
                    AppendLog));

            SetStatus("PINGER inseriti");
            MessageBox.Show(
                this,
                $"Creato 1 progetto con {result.Modules} PINGER.",
                "Inserimento PINGER completato",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            AppendLog("ERRORE: " + ex.Message);
            SetStatus("Errore");
            MessageBox.Show(this, ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, "Pronto");
        }
    }

    private async Task InsertModulePingersAsync()
    {
        try
        {
            ValidateSourceAndOutput();
            if (string.IsNullOrWhiteSpace(_moduleNameTextBox.Text))
            {
                throw new InvalidOperationException("Inserisci il modulo PINGER modello.");
            }

            if (string.IsNullOrWhiteSpace(_moduleCsvTextBox.Text))
            {
                throw new InvalidOperationException("Seleziona il CSV matrice PINGER.");
            }

            var quadFind = _moduleQuadFindTextBox.Text?.Trim();
            var quadReplace = _moduleQuadReplaceTextBox.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(quadFind) && string.IsNullOrWhiteSpace(quadReplace))
            {
                throw new InvalidOperationException("Inserisci il quadriletterale nuovo.");
            }

            if (string.IsNullOrWhiteSpace(quadFind) && !string.IsNullOrWhiteSpace(quadReplace))
            {
                throw new InvalidOperationException("Inserisci il quadriletterale base.");
            }

            var templateModule = _moduleNameTextBox.Text.Trim();

            SetBusy(true, "Inserimento PINGER da CSV in corso...");
            AppendLog($"Avvio inserimento PINGER da CSV. Template: {templateModule}");

            var result = await Task.Run(() =>
                CprjGenerator.InsertPingerModulesFromDeviceCsv(
                    _sourceTextBox.Text,
                    _moduleTargetTextBox.Text,
                    _outputTextBox.Text,
                    templateModule,
                    _moduleCsvTextBox.Text,
                    quadFind,
                    quadReplace,
                    AppendLog));

            SetStatus("PINGER inseriti");
            MessageBox.Show(
                this,
                $"Creato 1 progetto con {result.Modules} PINGER.",
                "Inserimento PINGER completato",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            AppendLog("ERRORE: " + ex.Message);
            SetStatus("Errore");
            MessageBox.Show(this, ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, "Pronto");
        }
    }

    private async Task InspectGenericCsvAsync()
    {
        try
        {
            ValidateSourceAndOutput();
            if (string.IsNullOrWhiteSpace(_genericCsvTextBox.Text))
            {
                throw new InvalidOperationException("Seleziona il file generico.");
            }

            SetBusy(true, "Analisi file generico in corso...");
            var inspection = await Task.Run(() =>
                CprjGenerator.InspectGenericCsv(_sourceTextBox.Text, _genericCsvTextBox.Text));

            var columnSummary = inspection.Columns
                .Select((column, index) =>
                    index < inspection.ColumnPreviews.Count && !string.IsNullOrWhiteSpace(inspection.ColumnPreviews[index])
                        ? $"{column}: {inspection.ColumnPreviews[index]}"
                        : column);

            _genericCsvInfoLabel.Text = $"File generico: {inspection.RowCount} righe | Colonne: {string.Join(", ", columnSummary)}";
            AppendLog(_genericCsvInfoLabel.Text);
            for (var rowIndex = 0; rowIndex < inspection.RowPreviews.Count; rowIndex++)
            {
                var row = inspection.RowPreviews[rowIndex];
                var rowSummary = inspection.Columns.Select((column, columnIndex) =>
                    $"{column}={(columnIndex < row.Count ? row[columnIndex] : string.Empty)}");
                AppendLog($"Riga {rowIndex + 1}: {string.Join(", ", rowSummary)}");
            }

            if (_genericCsvMappingGrid.Rows.Count == 1 &&
                string.IsNullOrWhiteSpace(_genericCsvMappingGrid.Rows[0].Cells["CsvColumn"].Value?.ToString()) &&
                inspection.Columns.Count > 0)
            {
                _genericCsvMappingGrid.Rows[0].Cells["CsvColumn"].Value = "1";
                _genericCsvMappingGrid.Rows[0].Cells["UseAsOutputName"].Value = true;
            }

            SetStatus("File generico analizzato");
        }
        catch (Exception ex)
        {
            AppendLog("ERRORE: " + ex.Message);
            SetStatus("Errore");
            MessageBox.Show(this, ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, "Pronto");
        }
    }

    private async Task GenerateGenericCsvAsync()
    {
        try
        {
            ValidateSourceAndOutput();
            if (string.IsNullOrWhiteSpace(_genericCsvTextBox.Text))
            {
                throw new InvalidOperationException("Seleziona il file generico.");
            }

            SetBusy(true, "Generazione file generico in corso...");
            AppendLog("Avvio file generico...");
            SaveSavedPaths();

            var mappings = ReadGenericCsvMappings();
            var createdFiles = await Task.Run(() =>
                CprjGenerator.BuildGenericCsvBatch(
                    _sourceTextBox.Text,
                    _outputTextBox.Text,
                    _genericCsvTextBox.Text,
                    string.Empty,
                    mappings,
                    AppendLog));

            SetStatus("Completato");
            MessageBox.Show(this, $"Creati {createdFiles.Count} file.", "File generico completato", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            AppendLog("ERRORE: " + ex.Message);
            SetStatus("Errore");
            MessageBox.Show(this, ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, "Pronto");
        }
    }

    private void RemoveModuleReplacementRow()
    {
        if (_moduleReplacementGrid.SelectedRows.Count == 0 && _moduleReplacementGrid.Rows.Count > 0)
        {
            _moduleReplacementGrid.Rows.RemoveAt(_moduleReplacementGrid.Rows.Count - 1);
            return;
        }

        foreach (DataGridViewRow row in _moduleReplacementGrid.SelectedRows)
        {
            if (!row.IsNewRow)
            {
                _moduleReplacementGrid.Rows.Remove(row);
            }
        }
    }

    private async Task InspectModuleAsync()
    {
        try
        {
            ValidateSourceAndOutput();
            if (string.IsNullOrWhiteSpace(_moduleNameTextBox.Text))
            {
                throw new InvalidOperationException("Inserisci il nome modulo da analizzare.");
            }

            SetBusy(true, "Analisi modulo in corso...");
            AppendLog("Avvio analisi modulo: " + _moduleNameTextBox.Text.Trim());

            var inspection = await Task.Run(() =>
                CprjGenerator.InspectModuleImpact(
                    _sourceTextBox.Text,
                    _moduleNameTextBox.Text,
                    false));

            _moduleInfoLabel.Text =
                $"Modulo: {inspection.ModuleName} | Definizioni: {inspection.Definitions.Count} | RTU: {inspection.RtuReferences.Count} | Tag: {inspection.Tags.Count} | Richiami modulo: {inspection.TextImpacts.Count} | Presenze tag: {inspection.TagImpacts.Count}";
            AppendLog(_moduleInfoLabel.Text);

            AppendLog("Termini modulo cercati: " + string.Join(", ", inspection.DirectSearchTerms));

            if (inspection.Definitions.Count == 0)
            {
                AppendLog("Definizioni: nessuna definizione .dbx/.ccx trovata con il nome modulo.");
            }
            else
            {
                AppendLog("Definizioni trovate:");
                foreach (var definition in inspection.Definitions)
                {
                    var details = new[]
                    {
                        string.IsNullOrWhiteSpace(definition.Label) ? null : $"label={definition.Label}",
                        string.IsNullOrWhiteSpace(definition.DeviceType) ? null : $"tipo={definition.DeviceType}",
                        string.IsNullOrWhiteSpace(definition.Ip) ? null : $"IP={definition.Ip}",
                        definition.Tags.Count == 0 ? null : $"tag={definition.Tags.Count}"
                    }.Where(value => value is not null);
                    AppendLog($"  {definition.FileName} [{definition.Kind}] {string.Join(" | ", details)}");
                }
            }

            if (inspection.RtuReferences.Count == 0)
            {
                AppendLog("rtu.ccx: nessun nodo collegato trovato.");
            }
            else
            {
                AppendLog("Riferimenti rtu.ccx:");
                foreach (var reference in inspection.RtuReferences)
                {
                    AppendLog($"  {reference.NodeType} id={reference.Id ?? "-"} href={reference.Href ?? "-"} label={reference.Label ?? "-"} errorTag={reference.ErrorTag ?? "-"} disable={reference.Disabled ?? "-"}");
                }
            }

            if (inspection.Tags.Count > 0)
            {
                AppendLog("Tag diretti: " + string.Join(", ", inspection.Tags));
                AppendLog("Termini tag cercati: " + string.Join(", ", inspection.TagSearchTerms));
            }

            AppendLog("Richiami espliciti al modulo:");
            foreach (var impact in inspection.TextImpacts)
            {
                var termSummary = string.Join(", ", impact.Terms.Take(5).Select(term => $"{term.Term}={term.Occurrences}"));
                if (impact.Terms.Count > 5)
                {
                    termSummary += $", +{impact.Terms.Count - 5} altri";
                }

                AppendLog($"  {impact.FileName}: {impact.TotalOccurrences} occorrenze ({termSummary})");
            }

            AppendLog("Moduli/file in cui sono presenti le tag:");
            foreach (var impact in inspection.TagImpacts)
            {
                var termSummary = string.Join(", ", impact.Terms.Take(5).Select(term => $"{term.Term}={term.Occurrences}"));
                if (impact.Terms.Count > 5)
                {
                    termSummary += $", +{impact.Terms.Count - 5} altri";
                }

                AppendLog($"  {impact.FileName}: {impact.TotalOccurrences} occorrenze ({termSummary})");
            }

            SetStatus("Modulo analizzato");
        }
        catch (Exception ex)
        {
            AppendLog("ERRORE: " + ex.Message);
            SetStatus("Errore");
            MessageBox.Show(this, ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, "Pronto");
        }
    }

    private async Task CopyModuleAsync()
    {
        try
        {
            ValidateSourceAndOutput();
            if (string.IsNullOrWhiteSpace(_moduleNameTextBox.Text))
            {
                throw new InvalidOperationException("Inserisci il nome modulo da copiare.");
            }

            if (string.IsNullOrWhiteSpace(_moduleTargetTextBox.Text))
            {
                throw new InvalidOperationException("Seleziona i progetti destinazione.");
            }

            SetBusy(true, "Copia modulo in corso...");
            AppendLog("Avvio copia modulo: " + _moduleNameTextBox.Text.Trim());

            var results = await Task.Run(() =>
                CprjGenerator.CopyModuleToProjects(
                    _sourceTextBox.Text,
                    _moduleTargetTextBox.Text,
                    _outputTextBox.Text,
                    _moduleNameTextBox.Text,
                    ReadModuleTypeFilter(),
                    ReadModuleReplacements(),
                    AppendLog));

            foreach (var result in results)
            {
                AppendLog($"Modulo copiato in {Path.GetFileName(result.OutputProject)}: file={result.CopiedFiles}, rtu={result.RtuNodes}, ddbb={result.DatabaseNodes}");
            }

            SetStatus("Modulo copiato");
            MessageBox.Show(this, $"Creati {results.Count} progetti aggiornati.", "Copia modulo completata", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            AppendLog("ERRORE: " + ex.Message);
            SetStatus("Errore");
            MessageBox.Show(this, ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, "Pronto");
        }
    }

    private IReadOnlyList<GenericReplaceJob> ReadGenericJobs()
    {
        var jobs = new List<GenericReplaceJob>();

        foreach (DataGridViewRow row in _genericGrid.Rows)
        {
            var replaceValue = row.Cells["ReplaceValue"].Value?.ToString();
            var outputName = row.Cells["OutputName"].Value?.ToString();
            var extraFind = row.Cells["ExtraFind"].Value?.ToString();
            var extraReplace = row.Cells["ExtraReplace"].Value?.ToString();

            if (string.IsNullOrWhiteSpace(replaceValue) &&
                string.IsNullOrWhiteSpace(outputName) &&
                string.IsNullOrWhiteSpace(extraFind) &&
                string.IsNullOrWhiteSpace(extraReplace))
            {
                continue;
            }

            jobs.Add(new GenericReplaceJob(replaceValue ?? string.Empty, outputName, extraFind, extraReplace));
        }

        return jobs;
    }

    private IReadOnlyList<GenericCsvMapping> ReadGenericCsvMappings()
    {
        var mappings = new List<GenericCsvMapping>();

        foreach (DataGridViewRow row in _genericCsvMappingGrid.Rows)
        {
            var csvColumn = row.Cells["CsvColumn"].Value?.ToString();
            var findText = row.Cells["FindText"].Value?.ToString();
            var useAsOutputName = row.Cells["UseAsOutputName"].Value is bool checkedValue && checkedValue;

            if (string.IsNullOrWhiteSpace(csvColumn) && string.IsNullOrWhiteSpace(findText))
            {
                continue;
            }

            mappings.Add(new GenericCsvMapping(csvColumn ?? string.Empty, findText ?? string.Empty, useAsOutputName));
        }

        return mappings;
    }

    private string? ReadModuleTypeFilter()
    {
        return _moduleTypeComboBox.SelectedItem?.ToString() switch
        {
            "PINGER" => "mPinger",
            "IED 61850" => "m61850",
            _ => null
        };
    }

    private IReadOnlyList<TextReplacement> ReadModuleReplacements()
    {
        var replacements = new List<TextReplacement>();
        foreach (DataGridViewRow row in _moduleReplacementGrid.Rows)
        {
            var findText = row.Cells["FindText"].Value?.ToString();
            var replaceText = row.Cells["ReplaceText"].Value?.ToString();
            if (string.IsNullOrWhiteSpace(findText))
            {
                continue;
            }

            replacements.Add(new TextReplacement(findText, replaceText ?? string.Empty));
        }

        return replacements;
    }

    private static string ResolvePingerTemplateMontante(string moduleName)
    {
        var normalized = moduleName.Trim();
        const string suffix = "_PING";
        if (!normalized.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return normalized;
        }

        var withoutPing = normalized[..^suffix.Length];
        var separatorIndex = withoutPing.LastIndexOf('_');
        return separatorIndex > 0 ? withoutPing[..separatorIndex] : withoutPing;
    }

    private void ValidateSourceAndOutput()
    {
        if (string.IsNullOrWhiteSpace(_sourceTextBox.Text))
        {
            throw new InvalidOperationException("Seleziona il template sorgente.");
        }
    }

    private void BrowseSourceFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Seleziona il template .cprj o un file di testo",
            Filter = "CPRJ/ZIP/Testo (*.cprj;*.zip;*.txt;*.csv;*.xml;*.json;*.dbx;*.ccx;*.cid;*.scan)|*.cprj;*.zip;*.txt;*.csv;*.xml;*.json;*.dbx;*.ccx;*.cid;*.scan|Tutti i file (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _sourceTextBox.Text = dialog.FileName;
            AppendLog("Template selezionato: " + dialog.FileName);
            SaveSavedPaths();
        }
    }

    private void BrowseSourceFolder()
    {
        using var dialog = new FolderBrowserDialog { Description = "Seleziona la cartella che contiene il template o un solo .cprj" };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _sourceTextBox.Text = dialog.SelectedPath;
            AppendLog("Cartella template selezionata: " + dialog.SelectedPath);
            SaveSavedPaths();
        }
    }

    private void BrowseOutputFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Seleziona la cartella di output",
            SelectedPath = string.IsNullOrWhiteSpace(_outputTextBox.Text) ? CprjGenerator.DefaultOutputDirectory : _outputTextBox.Text
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _outputTextBox.Text = dialog.SelectedPath;
            AppendLog("Output impostato: " + dialog.SelectedPath);
            SaveSavedPaths();
        }
    }

    private void BrowseDeviceCsvFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Seleziona il file dispositivi",
            Filter = "Excel/CSV (*.xlsx;*.xlsm;*.xls;*.csv)|*.xlsx;*.xlsm;*.xls;*.csv|Excel (*.xlsx;*.xlsm;*.xls)|*.xlsx;*.xlsm;*.xls|CSV (*.csv)|*.csv|Tutti i file (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _deviceCsvTextBox.Text = dialog.FileName;
            AppendLog("File dispositivi selezionato: " + dialog.FileName);
            SaveSavedPaths();
        }
    }

    private void BrowseGenericCsvFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Seleziona il file generico",
            Filter = "Excel/CSV (*.xlsx;*.xlsm;*.xls;*.csv)|*.xlsx;*.xlsm;*.xls;*.csv|Excel (*.xlsx;*.xlsm;*.xls)|*.xlsx;*.xlsm;*.xls|CSV (*.csv)|*.csv|Tutti i file (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _genericCsvTextBox.Text = dialog.FileName;
            AppendLog("File generico selezionato: " + dialog.FileName);
            SaveSavedPaths();
        }
    }

    private void BrowseModuleTargetFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Seleziona il progetto destinazione",
            Filter = "CPRJ/ZIP (*.cprj;*.zip)|*.cprj;*.zip|Tutti i file (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _moduleTargetTextBox.Text = dialog.FileName;
            AppendLog("Progetto destinazione selezionato: " + dialog.FileName);
            SaveSavedPaths();
        }
    }

    private void BrowseModuleTargetFolder()
    {
        using var dialog = new FolderBrowserDialog { Description = "Seleziona la cartella con i progetti destinazione" };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _moduleTargetTextBox.Text = dialog.SelectedPath;
            AppendLog("Cartella progetti destinazione selezionata: " + dialog.SelectedPath);
            SaveSavedPaths();
        }
    }

    private void BrowseModuleCsvFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Seleziona il CSV matrice PINGER",
            Filter = "Excel/CSV (*.xlsx;*.xlsm;*.xls;*.csv)|*.xlsx;*.xlsm;*.xls;*.csv|Excel (*.xlsx;*.xlsm;*.xls)|*.xlsx;*.xlsm;*.xls|CSV (*.csv)|*.csv|Tutti i file (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _moduleCsvTextBox.Text = dialog.FileName;
            AppendLog("CSV matrice PINGER selezionato: " + dialog.FileName);
            SaveSavedPaths();
        }
    }

    private void OpenOutputFolder()
    {
        var outputDirectory = string.IsNullOrWhiteSpace(_outputTextBox.Text)
            ? CprjGenerator.DefaultOutputDirectory
            : _outputTextBox.Text.Trim();

        Directory.CreateDirectory(outputDirectory);
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{outputDirectory}\"",
            UseShellExecute = true
        });
    }

    private void SetBusy(bool busy, string statusText)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => SetBusy(busy, statusText));
            return;
        }

        UseWaitCursor = busy;
        _browseSourceFileButton.Enabled = !busy;
        _browseSourceFolderButton.Enabled = !busy;
        _browseOutputButton.Enabled = !busy;
        _openOutputButton.Enabled = !busy;
        _addGenericRowButton.Enabled = !busy;
        _removeGenericRowButton.Enabled = !busy;
        _generateGenericButton.Enabled = !busy;
        _browseDeviceCsvButton.Enabled = !busy;
        _inspectTemplateButton.Enabled = !busy;
        _inspectDeviceCsvButton.Enabled = !busy;
        _generateDeviceButton.Enabled = !busy;
        _insertPingersButton.Enabled = !busy;
        _browseGenericCsvButton.Enabled = !busy;
        _inspectGenericCsvButton.Enabled = !busy;
        _addGenericCsvMappingButton.Enabled = !busy;
        _removeGenericCsvMappingButton.Enabled = !busy;
        _generateGenericCsvButton.Enabled = !busy;
        _inspectModuleButton.Enabled = !busy;
        _browseModuleTargetFileButton.Enabled = !busy;
        _browseModuleTargetFolderButton.Enabled = !busy;
        _browseModuleCsvButton.Enabled = !busy;
        _insertModulePingersButton.Enabled = !busy;
        _addModuleReplacementButton.Enabled = !busy;
        _removeModuleReplacementButton.Enabled = !busy;
        _copyModuleButton.Enabled = !busy;
        _moduleReplacementGrid.Enabled = !busy;
        SetStatus(statusText);
    }

    private void SetStatus(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => SetStatus(text));
            return;
        }

        _statusLabel.Text = text;
    }

    private void AppendLog(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendLog(message));
            return;
        }

        _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        _logTextBox.SelectionStart = _logTextBox.TextLength;
        _logTextBox.ScrollToCaret();
    }

    private static Label CreateLabel(string text) =>
        new()
        {
            Text = text,
            AutoSize = true,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 6, 8, 0)
        };
}
