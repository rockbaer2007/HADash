using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HADashboardBackupExporteurUGSo;

public sealed class MainForm : Form
{
    private readonly Button _openButton = new() { Text = "Datei öffnen" };
    private readonly Button _reloadButton = new()
    {
        Text = string.Empty,
        Enabled = false,
        AccessibleName = "Dashboard neu laden",
        AccessibleDescription = "Lädt die aktuell geöffnete Dashboard-Datei erneut."
    };
    private readonly Button _saveDashboardButton = new() { Text = string.Empty, Enabled = false };
    private readonly Button _copyButton = new() { Text = string.Empty, Enabled = false };
    private readonly Button _exportTxtButton = new() { Text = string.Empty, Enabled = false };
    private readonly Button _exportYamlButton = new() { Text = string.Empty, Enabled = false };
    private readonly Button _exportDashboardButton = new() { Text = string.Empty, Enabled = false };
    private readonly TextBox _searchTextBox = new() { PlaceholderText = "Ansichten suchen …" };
    private readonly Label _fileLabel = new() { Text = "Keine Datei geladen", AutoEllipsis = true };
    private readonly Label _viewsLabel = new() { Text = "Ansichten" };
    private readonly ListBox _viewsListBox = new();
    private readonly RichTextBox _yamlTextBox = new();
    private readonly StatusStrip _statusStrip = new();
    private readonly ToolStripStatusLabel _statusLabel = new() { Text = "Bereit" };
    private readonly ToolStripStatusLabel _copyrightLabel = new() { Text = "© 2026 UGSo", Spring = true, TextAlign = ContentAlignment.MiddleRight };
    private readonly SplitContainer _splitContainer = new();
    private readonly ToolTip _toolTip = new();
    private readonly MenuStrip _menuStrip = new();
    private readonly ToolStripMenuItem _recentFilesMenuItem = new("Zuletzt geöffnet");
    private readonly List<DashboardViewItem> _allViews = [];
    private readonly AppSettings _settings;

    private string? _currentFilePath;
    private YamlMappingNode? _dashboardRoot;
    private string? _currentDashboardYaml;
    private bool _currentSourceWasJson;
    private bool _isApplyingHighlighting;
    private readonly List<YamlColorSpan> _yamlColorSpans = [];

    private enum YamlTokenKind
    {
        Key,
        Comment,
        Keyword,
        Number
    }

    private readonly record struct YamlColorSpan(int Start, int Length, YamlTokenKind Kind);

    public MainForm()
    {
        _settings = AppSettings.Load();
        InitializeWindow();
        InitializeControls();
        WireEvents();
        ApplyUiPreferences();
        RefreshRecentFilesMenu();

        if (_settings.OpenLastFileOnStartup && !string.IsNullOrWhiteSpace(_settings.LastFilePath) && File.Exists(_settings.LastFilePath))
        {
            TryLoadDashboard(_settings.LastFilePath, showSuccessMessage: false);
        }
    }

    private void InitializeWindow()
    {
        Text = "HADash by UGSo v0.9.4-preview";
        StartPosition = FormStartPosition.Manual;
        MinimumSize = new Size(980, 640);
        RestoreWindowSettings();
        Font = new Font("Segoe UI", _settings.FontSize);
        AllowDrop = true;
    }


    private void RestoreWindowSettings()
    {
        var proposedBounds = new Rectangle(
            _settings.WindowLeft,
            _settings.WindowTop,
            Math.Max(MinimumSize.Width, _settings.WindowWidth),
            Math.Max(MinimumSize.Height, _settings.WindowHeight));

        var visibleOnAnyScreen = Screen.AllScreens.Any(screen =>
            screen.WorkingArea.IntersectsWith(proposedBounds));

        if (visibleOnAnyScreen)
        {
            Bounds = proposedBounds;
        }
        else
        {
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1280, 800);
        }

        if (_settings.WindowState == FormWindowState.Maximized)
            WindowState = FormWindowState.Maximized;
    }

    private void SaveWindowAndUserSettings()
    {
        var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        _settings.WindowLeft = bounds.Left;
        _settings.WindowTop = bounds.Top;
        _settings.WindowWidth = Math.Max(MinimumSize.Width, bounds.Width);
        _settings.WindowHeight = Math.Max(MinimumSize.Height, bounds.Height);
        _settings.WindowState = WindowState == FormWindowState.Minimized
            ? FormWindowState.Normal
            : WindowState;
        _settings.Save();
    }

    private void InitializeControls()
    {
        BuildMenu();

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 8,
            Padding = new Padding(10),
            GrowStyle = TableLayoutPanelGrowStyle.AddColumns
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        _openButton.AutoSize = true;
        _openButton.Padding = new Padding(8, 3, 8, 3);

        ConfigureToolbarIconButton(
            _reloadButton,
            "HADashboardBackupExporteurUGSo.Resources.Refresh_icon.png",
            "Dashboard neu laden");
        ConfigureToolbarIconButton(
            _saveDashboardButton,
            "HADashboardBackupExporteurUGSo.Resources.visualisierung.png",
            "Komplettes Dashboard speichern");
        ConfigureToolbarIconButton(
            _copyButton,
            "HADashboardBackupExporteurUGSo.Resources.kopieren.png",
            "Code der ausgewählten Ansicht kopieren");
        ConfigureToolbarIconButton(
            _exportTxtButton,
            "HADashboardBackupExporteurUGSo.Resources.export.png",
            "Ausgewählte Ansicht als TXT exportieren");
        ConfigureToolbarIconButton(
            _exportYamlButton,
            "HADashboardBackupExporteurUGSo.Resources.document-file.png",
            "Ausgewählte Ansicht als YAML speichern");
        ConfigureToolbarIconButton(
            _exportDashboardButton,
            "HADashboardBackupExporteurUGSo.Resources.dashboards.png",
            "Ausgewählte Ansicht als neues Dashboard speichern");

        _fileLabel.Dock = DockStyle.Fill;
        _fileLabel.TextAlign = ContentAlignment.MiddleLeft;
        _fileLabel.Padding = new Padding(12, 0, 12, 0);

        header.Controls.Add(_openButton, 0, 0);
        header.Controls.Add(_reloadButton, 1, 0);
        header.Controls.Add(_fileLabel, 2, 0);
        header.Controls.Add(_saveDashboardButton, 3, 0);
        header.Controls.Add(_copyButton, 4, 0);
        header.Controls.Add(_exportTxtButton, 5, 0);
        header.Controls.Add(_exportYamlButton, 6, 0);
        header.Controls.Add(_exportDashboardButton, 7, 0);


        _splitContainer.Dock = DockStyle.Fill;
        _splitContainer.FixedPanel = FixedPanel.Panel1;

        // Die endgültige Breite steht während InitializeControls noch nicht fest.
        // Deshalb wird die Trennposition erst nach dem ersten vollständigen Layout gesetzt.
        Shown += (_, _) => BeginInvoke(SetInitialSplitterDistance);

        var leftPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10, 0, 5, 10)
        };
        leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _viewsLabel.AutoSize = true;
        _viewsLabel.Font = new Font(Font, FontStyle.Bold);
        _viewsLabel.Padding = new Padding(0, 4, 0, 6);
        _searchTextBox.Dock = DockStyle.Top;
        _searchTextBox.Margin = new Padding(0, 0, 0, 8);
        _viewsListBox.Dock = DockStyle.Fill;
        _viewsListBox.IntegralHeight = false;
        _viewsListBox.HorizontalScrollbar = true;
        leftPanel.Controls.Add(_viewsLabel, 0, 0);
        leftPanel.Controls.Add(_searchTextBox, 0, 1);
        leftPanel.Controls.Add(_viewsListBox, 0, 2);

        var rightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(5, 0, 10, 10)
        };
        rightPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var codeLabel = new Label
        {
            Text = "YAML-Code der ausgewählten Ansicht",
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Padding = new Padding(0, 4, 0, 6)
        };

        _yamlTextBox.Dock = DockStyle.Fill;
        _yamlTextBox.Font = new Font("Consolas", 10F);
        _yamlTextBox.WordWrap = false;
        _yamlTextBox.AcceptsTab = true;
        _yamlTextBox.DetectUrls = false;
        _yamlTextBox.ScrollBars = RichTextBoxScrollBars.Both;
        _yamlTextBox.HideSelection = false;

        rightPanel.Controls.Add(codeLabel, 0, 0);
        rightPanel.Controls.Add(_yamlTextBox, 0, 1);
        _splitContainer.Panel1.Controls.Add(leftPanel);
        _splitContainer.Panel2.Controls.Add(rightPanel);

        _statusStrip.Items.Add(_statusLabel);
        _statusStrip.Items.Add(_copyrightLabel);

        Controls.Add(_splitContainer);
        Controls.Add(header);
        Controls.Add(_menuStrip);
        Controls.Add(_statusStrip);
        MainMenuStrip = _menuStrip;
    }


    private void ConfigureToolbarIconButton(Button button, string resourceName, string toolTipText)
    {
        button.AutoSize = false;
        button.Size = new Size(42, 34);
        button.MinimumSize = new Size(42, 34);
        button.MaximumSize = new Size(42, 34);
        button.Padding = Padding.Empty;
        button.Margin = new Padding(3);
        button.Tag = resourceName;
        button.Image = LoadEmbeddedToolbarImage(resourceName, new Size(_settings.IconSize, _settings.IconSize));
        button.ImageAlign = ContentAlignment.MiddleCenter;
        button.Text = string.Empty;
        button.TextImageRelation = TextImageRelation.Overlay;
        button.FlatStyle = FlatStyle.Standard;
        button.UseVisualStyleBackColor = true;
        button.Cursor = Cursors.Hand;
        button.AccessibleName = toolTipText;
        button.AccessibleDescription = toolTipText;
        _toolTip.SetToolTip(button, toolTipText);
    }

    private static Image? LoadEmbeddedToolbarImage(string resourceName, Size targetSize)
    {
        try
        {
            var assembly = typeof(MainForm).Assembly;
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                return null;
            }

            using var sourceImage = Image.FromStream(stream);
            var result = new Bitmap(targetSize.Width, targetSize.Height);

            using var graphics = Graphics.FromImage(result);
            graphics.Clear(Color.Transparent);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.DrawImage(sourceImage, new Rectangle(Point.Empty, targetSize));

            return result;
        }
        catch
        {
            // Das Programm bleibt auch dann nutzbar, wenn eine Ressource beschädigt ist.
            return null;
        }
    }

    private void BuildMenu()
    {
        var fileMenu = new ToolStripMenuItem("Datei");
        var openItem = new ToolStripMenuItem("Datei öffnen …");
        var reloadItem = new ToolStripMenuItem("Neu laden");
        var saveFullDashboardItem = new ToolStripMenuItem("Komplettes Dashboard speichern unter …");
        var exportDashboardItem = new ToolStripMenuItem("Ausgewählte Ansicht als Dashboard erzeugen …");
        var exitItem = new ToolStripMenuItem("Beenden");
        openItem.Click += (_, _) => OpenDashboardFile();
        reloadItem.Click += (_, _) => ReloadDashboard();
        saveFullDashboardItem.Click += (_, _) => SaveCompleteDashboard();
        exportDashboardItem.Click += (_, _) => ExportSelectedViewAsDashboard();
        exitItem.Click += (_, _) => Close();
        fileMenu.DropDownItems.Add(openItem);
        fileMenu.DropDownItems.Add(reloadItem);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(_recentFilesMenuItem);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(saveFullDashboardItem);
        fileMenu.DropDownItems.Add(exportDashboardItem);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(exitItem);

        var settingsMenu = new ToolStripMenuItem("Einstellungen");
        var programSettingsItem = new ToolStripMenuItem("Programmeinstellungen …");
        programSettingsItem.Click += (_, _) => ShowSettings();
        settingsMenu.DropDownItems.Add(programSettingsItem);

        var helpMenu = new ToolStripMenuItem("Hilfe");
        var instructionsItem = new ToolStripMenuItem("Anleitung");
        var homeAssistantItem = new ToolStripMenuItem("Home Assistant");
        var notepadItem = new ToolStripMenuItem("Notepad++");
        var aboutItem = new ToolStripMenuItem("Über");
        instructionsItem.Click += (_, _) => ShowInstructions();
        homeAssistantItem.Click += (_, _) => OpenWebPage("https://www.home-assistant.io/");
        notepadItem.Click += (_, _) => OpenWebPage("https://notepad-plus-plus.org/");
        aboutItem.Click += (_, _) => ShowAbout();
        helpMenu.DropDownItems.Add(instructionsItem);
        helpMenu.DropDownItems.Add(new ToolStripSeparator());
        helpMenu.DropDownItems.Add(homeAssistantItem);
        helpMenu.DropDownItems.Add(notepadItem);
        helpMenu.DropDownItems.Add(new ToolStripSeparator());
        helpMenu.DropDownItems.Add(aboutItem);

        _menuStrip.Items.Add(fileMenu);
        _menuStrip.Items.Add(settingsMenu);
        _menuStrip.Items.Add(helpMenu);
    }

    private void WireEvents()
    {
        _openButton.Click += (_, _) => OpenDashboardFile();
        _reloadButton.Click += (_, _) => ReloadDashboard();
        _saveDashboardButton.Click += (_, _) => SaveCompleteDashboard();
        _copyButton.Click += (_, _) => CopySelectedView();
        _exportTxtButton.Click += (_, _) => ExportSelectedView("txt");
        _exportYamlButton.Click += (_, _) => ExportSelectedView("yaml");
        _exportDashboardButton.Click += (_, _) => ExportSelectedViewAsDashboard();
        _viewsListBox.SelectedIndexChanged += (_, _) => ShowSelectedView();
        _searchTextBox.TextChanged += (_, _) => ApplyViewFilter();
        _yamlTextBox.TextChanged += (_, _) => ApplyYamlHighlighting();
        DragEnter += MainForm_DragEnter;
        DragDrop += MainForm_DragDrop;
        FormClosing += (_, _) => SaveWindowAndUserSettings();
    }

    private void OpenDashboardFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Home-Assistant-Dashboard oder Backup-Datei auswählen",
            Filter = "Unterstützte Home-Assistant-Dateien (*.dash;*.yaml;*.yml;*.json;*.txt;*.lovelace)|*.dash;*.yaml;*.yml;*.json;*.txt;*.lovelace|Dashboard-Dateien (*.dash)|*.dash|YAML-Dateien (*.yaml;*.yml)|*.yaml;*.yml|JSON- und Backup-Dateien (*.json;*.txt;*.lovelace)|*.json;*.txt;*.lovelace|Alle Dateien (*.*)|*.*",
            DefaultExt = "dash",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
            TryLoadDashboard(dialog.FileName, showSuccessMessage: true);
    }

    private static string ConvertInputToDashboardYaml(string sourceText, out bool sourceWasJson)
    {
        sourceWasJson = false;
        try
        {
            var root = JsonNode.Parse(
                sourceText,
                documentOptions: new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip
                });

            if (root is null)
                return sourceText;

            sourceWasJson = true;
            JsonNode dashboardNode;
            if (root is JsonObject rootObject && rootObject["data"]?["config"] is JsonNode storageConfig)
                dashboardNode = storageConfig;
            else if (root is JsonObject directRoot && directRoot["config"] is JsonNode directConfig)
                dashboardNode = directConfig;
            else
                dashboardNode = root;

            var serializer = new SerializerBuilder()
                .WithNamingConvention(NullNamingConvention.Instance)
                .DisableAliases()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve)
                .Build();

            return NormalizeYaml(serializer.Serialize(ConvertJsonNode(dashboardNode)));
        }
        catch (JsonException)
        {
            return NormalizeYaml(sourceText);
        }
    }

    private static object? ConvertJsonNode(JsonNode? node) => node switch
    {
        null => null,
        JsonObject obj => obj.ToDictionary(
            property => property.Key,
            property => ConvertJsonNode(property.Value)),
        JsonArray array => array.Select(ConvertJsonNode).ToList(),
        JsonValue value => ConvertJsonValue(value),
        _ => node.ToJsonString()
    };

    private static object? ConvertJsonValue(JsonValue value)
    {
        if (value.TryGetValue<string>(out var stringValue)) return stringValue;
        if (value.TryGetValue<bool>(out var boolValue)) return boolValue;
        if (value.TryGetValue<int>(out var intValue)) return intValue;
        if (value.TryGetValue<long>(out var longValue)) return longValue;
        if (value.TryGetValue<decimal>(out var decimalValue)) return decimalValue;
        if (value.TryGetValue<double>(out var doubleValue)) return doubleValue;
        return value.ToJsonString();
    }

    private static string NormalizeYaml(string yaml) =>
        yaml.Replace("\r\n", "\n").Replace("\r", "\n");

    private void SaveCompleteDashboard()
    {
        if (string.IsNullOrWhiteSpace(_currentDashboardYaml))
        {
            ShowError("Es ist kein Dashboard geladen.");
            return;
        }

        var baseName = string.IsNullOrWhiteSpace(_currentFilePath)
            ? "HomeAssistant_Dashboard"
            : SanitizeFileName(Path.GetFileNameWithoutExtension(_currentFilePath));

        using var dialog = new SaveFileDialog
        {
            Title = "Komplettes Dashboard speichern",
            Filter = "Home Assistant Dashboard (*.dash)|*.dash|Home Assistant YAML (*.yaml)|*.yaml|YML-Datei (*.yml)|*.yml|Alle Dateien (*.*)|*.*",
            DefaultExt = _settings.DefaultDashboardFormat,
            FilterIndex = _settings.DefaultDashboardFormat == "yaml" ? 2 : 1,
            AddExtension = true,
            OverwritePrompt = true,
            FileName = $"{baseName}_export.{_settings.DefaultDashboardFormat}"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        var selectedExtension = dialog.FilterIndex switch
        {
            2 => ".yaml",
            3 => ".yml",
            _ => ".dash"
        };
        var outputPath = dialog.FilterIndex <= 3
            ? Path.ChangeExtension(dialog.FileName, selectedExtension)
            : dialog.FileName;

        CreateBackupIfRequired(outputPath);
        File.WriteAllText(outputPath, _currentDashboardYaml, new UTF8Encoding(false));
        WriteLog($"Dashboard gespeichert: {outputPath}");
        _statusLabel.Text = $"Komplettes Dashboard gespeichert: {outputPath}";
        MessageBox.Show(this,
            $"Das komplette Dashboard wurde gespeichert:\n\n{outputPath}",
            "Dashboard gespeichert",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void ReloadDashboard()
    {
        if (!string.IsNullOrWhiteSpace(_currentFilePath))
            TryLoadDashboard(_currentFilePath, showSuccessMessage: false);
    }

    private void TryLoadDashboard(string filePath, bool showSuccessMessage)
    {
        try
        {
            LoadDashboard(filePath);
            if (showSuccessMessage)
                _statusLabel.Text = $"Dashboard geladen: {Path.GetFileName(filePath)}";
        }
        catch (YamlException ex)
        {
            ClearDashboard();
            ShowError($"Die Datei enthält ungültiges YAML.\n\nZeile: {ex.Start.Line}\nSpalte: {ex.Start.Column}\n\n{ex.Message}");
        }
        catch (Exception ex)
        {
            ClearDashboard();
            ShowError($"Die Datei konnte nicht geladen werden.\n\n{ex.Message}");
        }
    }

    private void LoadDashboard(string filePath)
    {
        var sourceText = File.ReadAllText(filePath, Encoding.UTF8);
        var yamlText = ConvertInputToDashboardYaml(sourceText, out var sourceWasJson);
        using var reader = new StringReader(yamlText);
        var stream = new YamlStream();
        stream.Load(reader);

        if (stream.Documents.Count == 0 || stream.Documents[0].RootNode is not YamlMappingNode loadedRoot)
            throw new InvalidDataException("Die Datei besitzt kein gültiges YAML-Hauptobjekt.");

        var root = loadedRoot;
        var extractedYamlWrapper = false;
        if (!TryGetMappingValue(root, "views", out var viewsNode))
        {
            if (TryGetNestedDashboardRoot(root, out var nestedRoot))
            {
                root = nestedRoot;
                extractedYamlWrapper = true;
                TryGetMappingValue(root, "views", out viewsNode);
            }
        }

        if (viewsNode is null)
            throw new InvalidDataException("In der Datei wurde kein 'views:'-Block gefunden. Auch data.config und config enthielten kein Dashboard.");
        if (viewsNode is not YamlSequenceNode viewsSequence)
            throw new InvalidDataException("Der 'views:'-Block ist keine YAML-Liste.");

        _allViews.Clear();
        for (var index = 0; index < viewsSequence.Children.Count; index++)
        {
            var node = viewsSequence.Children[index];
            _allViews.Add(new DashboardViewItem { Index = index, DisplayName = BuildViewName(node, index), Node = node });
        }

        _dashboardRoot = root;
        _currentDashboardYaml = sourceWasJson || extractedYamlWrapper
            ? SerializeView(root)
            : NormalizeYaml(yamlText);
        _currentSourceWasJson = sourceWasJson;
        _currentFilePath = filePath;
        _settings.LastFilePath = filePath;
        AddRecentFile(filePath);
        _fileLabel.Text = sourceWasJson
            ? $"{filePath}  [JSON-Backup erkannt]"
            : extractedYamlWrapper
                ? $"{filePath}  [YAML-Backup erkannt]"
                : filePath;
        _toolTip.SetToolTip(_fileLabel, filePath);
        _reloadButton.Enabled = true;
        _saveDashboardButton.Enabled = true;
        _searchTextBox.Clear();
        ApplyViewFilter();
    }

    private static bool TryGetNestedDashboardRoot(YamlMappingNode root, out YamlMappingNode dashboardRoot)
    {
        if (TryGetMappingValue(root, "data", out var dataNode)
            && dataNode is YamlMappingNode dataMapping
            && TryGetMappingValue(dataMapping, "config", out var dataConfigNode)
            && dataConfigNode is YamlMappingNode dataConfigMapping
            && TryGetMappingValue(dataConfigMapping, "views", out _))
        {
            dashboardRoot = dataConfigMapping;
            return true;
        }

        if (TryGetMappingValue(root, "config", out var configNode)
            && configNode is YamlMappingNode configMapping
            && TryGetMappingValue(configMapping, "views", out _))
        {
            dashboardRoot = configMapping;
            return true;
        }

        dashboardRoot = null!;
        return false;
    }

    private void AddRecentFile(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);

        _settings.RecentFiles.RemoveAll(path =>
            string.Equals(path, fullPath, StringComparison.OrdinalIgnoreCase));
        _settings.RecentFiles.Insert(0, fullPath);

        var maximumRecentFiles = _settings.MaximumRecentFiles;
        if (_settings.RecentFiles.Count > maximumRecentFiles)
            _settings.RecentFiles.RemoveRange(maximumRecentFiles, _settings.RecentFiles.Count - maximumRecentFiles);

        _settings.Save();
        RefreshRecentFilesMenu();
    }

    private void RefreshRecentFilesMenu()
    {
        _recentFilesMenuItem.DropDownItems.Clear();

        // Nicht mehr vorhandene Dateien bleiben nicht dauerhaft im Menü stehen.
        _settings.RecentFiles.RemoveAll(path => string.IsNullOrWhiteSpace(path) || !File.Exists(path));

        if (_settings.RecentFiles.Count == 0)
        {
            _recentFilesMenuItem.DropDownItems.Add(new ToolStripMenuItem("Keine zuletzt geöffneten Dateien")
            {
                Enabled = false
            });
            _recentFilesMenuItem.Enabled = false;
            return;
        }

        _recentFilesMenuItem.Enabled = true;
        for (var index = 0; index < _settings.RecentFiles.Count; index++)
        {
            var filePath = _settings.RecentFiles[index];
            var displayName = $"{index + 1}. {Path.GetFileName(filePath)}";
            var item = new ToolStripMenuItem(displayName)
            {
                ToolTipText = filePath,
                Tag = filePath
            };

            item.Click += (_, _) => OpenRecentFile(filePath);
            _recentFilesMenuItem.DropDownItems.Add(item);
        }

        _recentFilesMenuItem.DropDownItems.Add(new ToolStripSeparator());
        var clearItem = new ToolStripMenuItem("Liste löschen");
        clearItem.Click += (_, _) =>
        {
            _settings.RecentFiles.Clear();
            _settings.Save();
            RefreshRecentFilesMenu();
        };
        _recentFilesMenuItem.DropDownItems.Add(clearItem);
        _settings.Save();
    }

    private void OpenRecentFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            TryLoadDashboard(filePath, showSuccessMessage: true);
            return;
        }

        _settings.RecentFiles.RemoveAll(path =>
            string.Equals(path, filePath, StringComparison.OrdinalIgnoreCase));
        _settings.Save();
        RefreshRecentFilesMenu();
        MessageBox.Show(this,
            "Die ausgewählte Datei ist nicht mehr vorhanden und wurde aus der Liste entfernt.",
            "Datei nicht gefunden",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void ApplyViewFilter()
    {
        var query = _searchTextBox.Text.Trim();
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allViews
            : _allViews.Where(view => view.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        _viewsListBox.BeginUpdate();
        _viewsListBox.Items.Clear();
        foreach (var item in filtered)
            _viewsListBox.Items.Add(item);
        _viewsListBox.EndUpdate();

        _viewsLabel.Text = $"Ansichten ({filtered.Count}/{_allViews.Count})";
        if (_viewsListBox.Items.Count > 0)
            _viewsListBox.SelectedIndex = 0;
        else
            SetSelectionActions(false);
    }

    private void ShowSelectedView()
    {
        if (_viewsListBox.SelectedItem is not DashboardViewItem selected)
        {
            _yamlTextBox.Clear();
            SetSelectionActions(false);
            return;
        }

        _yamlTextBox.Text = SerializeView(selected.Node);
        _yamlTextBox.SelectionStart = 0;
        _yamlTextBox.SelectionLength = 0;
        SetSelectionActions(true);
        _statusLabel.Text = $"Ausgewählt: {selected.DisplayName}";
    }

    private void SetSelectionActions(bool enabled)
    {
        _copyButton.Enabled = enabled;
        _exportTxtButton.Enabled = enabled;
        _exportYamlButton.Enabled = enabled;
        _exportDashboardButton.Enabled = enabled;
    }

    private void CopySelectedView()
    {
        if (string.IsNullOrWhiteSpace(_yamlTextBox.Text)) return;
        Clipboard.SetText(_yamlTextBox.Text);
        _statusLabel.Text = "Ansicht wurde in die Zwischenablage kopiert.";
    }

    private void ExportSelectedView(string extension)
    {
        if (_viewsListBox.SelectedItem is not DashboardViewItem selected)
        {
            MessageBox.Show(this, "Bitte zuerst eine Ansicht auswählen.", "Keine Ansicht ausgewählt", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var isYaml = extension.Equals("yaml", StringComparison.OrdinalIgnoreCase);
        using var dialog = new SaveFileDialog
        {
            Title = isYaml ? "Ansicht als YAML exportieren" : "Ansicht als TXT exportieren",
            Filter = isYaml ? "YAML-Datei (*.yaml)|*.yaml|YML-Datei (*.yml)|*.yml" : "Textdatei (*.txt)|*.txt",
            FileName = SanitizeFileName(selected.DisplayName) + "." + extension,
            DefaultExt = extension,
            AddExtension = true,
            OverwritePrompt = true,
            InitialDirectory = string.IsNullOrWhiteSpace(_currentFilePath) ? null : Path.GetDirectoryName(_currentFilePath)
        };

        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            CreateBackupIfRequired(dialog.FileName);
            File.WriteAllText(dialog.FileName, _yamlTextBox.Text, new UTF8Encoding(false));
            WriteLog($"Ansicht exportiert: {dialog.FileName}");
            _statusLabel.Text = $"Exportiert: {dialog.FileName}";
            MessageBox.Show(this, "Die Ansicht wurde erfolgreich exportiert.", "Export abgeschlossen", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowError($"Die Datei konnte nicht gespeichert werden.\n\n{ex.Message}");
        }
    }

    private void ExportSelectedViewAsDashboard()
    {
        if (_viewsListBox.SelectedItem is not DashboardViewItem selected || _dashboardRoot is null)
        {
            MessageBox.Show(this, "Bitte zuerst eine Ansicht auswählen.", "Keine Ansicht ausgewählt",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var suggestedName = GetScalarValue(_dashboardRoot, "title") ?? selected.DisplayName;
        using var nameDialog = new DashboardNameForm(suggestedName);
        if (nameDialog.ShowDialog(this) != DialogResult.OK)
            return;

        var dashboardName = nameDialog.DashboardName.Trim();
        if (string.IsNullOrWhiteSpace(dashboardName))
            return;

        using var saveDialog = new SaveFileDialog
        {
            Title = "Neues Home-Assistant-Dashboard speichern",
            Filter = "Home Assistant Dashboard (*.dash)|*.dash|Home Assistant YAML (*.yaml)|*.yaml",
            FilterIndex = _settings.DefaultDashboardFormat == "yaml" ? 2 : 1,
            FileName = SanitizeFileName(dashboardName),
            DefaultExt = _settings.DefaultDashboardFormat,
            AddExtension = true,
            OverwritePrompt = true,
            SupportMultiDottedExtensions = true,
            InitialDirectory = string.IsNullOrWhiteSpace(_currentFilePath) ? null : Path.GetDirectoryName(_currentFilePath)
        };

        // Die Dateiendung richtet sich nach dem im Speichern-Dialog gewählten Dateityp.
        // Dadurch wird beim Wechsel von *.dash zu *.yaml nicht versehentlich weiterhin
        // eine Datei mit der Endung *.dash gespeichert.
        saveDialog.FileOk += (_, _) =>
        {
            var selectedExtension = saveDialog.FilterIndex == 2 ? ".yaml" : ".dash";
            if (!saveDialog.FileName.EndsWith(selectedExtension, StringComparison.OrdinalIgnoreCase))
                saveDialog.FileName = Path.ChangeExtension(saveDialog.FileName, selectedExtension);
        };

        if (saveDialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            var dashboardCode = BuildSingleViewDashboard(_dashboardRoot, selected.Node, dashboardName);
            CreateBackupIfRequired(saveDialog.FileName);
            File.WriteAllText(saveDialog.FileName, dashboardCode, new UTF8Encoding(false));
            WriteLog($"Einzelansicht als Dashboard gespeichert: {saveDialog.FileName}");
            _statusLabel.Text = $"Dashboard erzeugt: {saveDialog.FileName}";
            MessageBox.Show(this,
                $"Das neue Dashboard wurde erfolgreich erzeugt.\n\n" +
                $"Name: {dashboardName}\n" +
                $"Ansicht: {selected.DisplayName}",
                "Dashboard erzeugt", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            ShowError($"Das Dashboard konnte nicht erzeugt werden.\n\n{ex.Message}");
        }
    }

    private static string BuildSingleViewDashboard(YamlMappingNode originalRoot, YamlNode selectedView, string dashboardName)
    {
        var newRoot = new YamlMappingNode();
        var titleFound = false;
        var viewsFound = false;

        foreach (var pair in originalRoot.Children)
        {
            if (pair.Key is YamlScalarNode key)
            {
                if (string.Equals(key.Value, "title", StringComparison.OrdinalIgnoreCase))
                {
                    newRoot.Add(CloneYamlNode(pair.Key), new YamlScalarNode(dashboardName));
                    titleFound = true;
                    continue;
                }

                if (string.Equals(key.Value, "views", StringComparison.OrdinalIgnoreCase))
                {
                    newRoot.Add(CloneYamlNode(pair.Key), new YamlSequenceNode(CloneYamlNode(selectedView)));
                    viewsFound = true;
                    continue;
                }
            }

            newRoot.Add(CloneYamlNode(pair.Key), CloneYamlNode(pair.Value));
        }

        if (!titleFound)
            newRoot.Add(new YamlScalarNode("title"), new YamlScalarNode(dashboardName));
        if (!viewsFound)
            newRoot.Add(new YamlScalarNode("views"), new YamlSequenceNode(CloneYamlNode(selectedView)));

        var stream = new YamlStream(new YamlDocument(newRoot));
        using var writer = new StringWriter();
        stream.Save(writer, assignAnchors: false);
        return writer.ToString().Replace("...\r\n", "").Replace("...\n", "").TrimEnd() + Environment.NewLine;
    }

    private static YamlNode CloneYamlNode(YamlNode node)
    {
        var yaml = SerializeView(node);
        using var reader = new StringReader(yaml);
        var stream = new YamlStream();
        stream.Load(reader);
        return stream.Documents[0].RootNode;
    }

    private void MainForm_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            e.Effect = DragDropEffects.Copy;
    }

    private void MainForm_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            TryLoadDashboard(files[0], showSuccessMessage: true);
    }

    private void ApplyYamlHighlighting()
    {
        if (_isApplyingHighlighting) return;

        // Die Treffer werden nur bei einer echten Textänderung neu ermittelt.
        _yamlColorSpans.Clear();
        if (_yamlTextBox.TextLength == 0) return;

        CacheMatches(@"(?m)^\s*[^#\r\n][^:\r\n]*?(?=:)", YamlTokenKind.Key);
        CacheMatches(@"(?m)#.*$", YamlTokenKind.Comment);
        CacheMatches(@"(?<![\w])(?:true|false|null|on|off)(?![\w])", YamlTokenKind.Keyword);
        CacheMatches(@"(?<![\w])-?\d+(?:\.\d+)?(?![\w])", YamlTokenKind.Number);

        ApplyCachedYamlColors();
    }

    private void CacheMatches(string pattern, YamlTokenKind kind)
    {
        foreach (Match match in Regex.Matches(_yamlTextBox.Text, pattern, RegexOptions.IgnoreCase))
            _yamlColorSpans.Add(new YamlColorSpan(match.Index, match.Length, kind));
    }

    private void ApplyCachedYamlColors()
    {
        if (_isApplyingHighlighting || _yamlTextBox.TextLength == 0) return;

        _isApplyingHighlighting = true;
        try
        {
            var selectionStart = _yamlTextBox.SelectionStart;
            var selectionLength = _yamlTextBox.SelectionLength;
            _yamlTextBox.SuspendLayout();
            _yamlTextBox.SelectAll();
            _yamlTextBox.SelectionColor = SystemColors.WindowText;

            foreach (var span in _yamlColorSpans)
            {
                if (span.Start < 0 || span.Length <= 0 || span.Start + span.Length > _yamlTextBox.TextLength)
                    continue;

                _yamlTextBox.Select(span.Start, span.Length);
                _yamlTextBox.SelectionColor = GetYamlTokenColor(span.Kind);
            }

            var safeStart = Math.Min(selectionStart, _yamlTextBox.TextLength);
            var safeLength = Math.Min(selectionLength, Math.Max(0, _yamlTextBox.TextLength - safeStart));
            _yamlTextBox.Select(safeStart, safeLength);
        }
        finally
        {
            _yamlTextBox.ResumeLayout();
            _isApplyingHighlighting = false;
        }
    }

    private static Color GetYamlTokenColor(YamlTokenKind kind) => kind switch
    {
        YamlTokenKind.Key => Color.DarkBlue,
        YamlTokenKind.Comment => Color.ForestGreen,
        YamlTokenKind.Keyword => Color.DarkMagenta,
        YamlTokenKind.Number => Color.DarkCyan,
        _ => SystemColors.WindowText
    };

    private void ShowSettings()
    {
        using var dialog = new SettingsForm(_settings);
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        _settings.CopyFrom(dialog.ResultSettings);
        _settings.Save();

        ApplyUiPreferences();
        RefreshRecentFilesMenu();
        _statusLabel.Text = "Programmeinstellungen wurden übernommen.";
    }

    private void ApplyUiPreferences()
    {
        Font = new Font("Segoe UI", _settings.FontSize);
        ApplyFontRecursively(this, Font);
        _yamlTextBox.Font = new Font("Consolas", _settings.FontSize);

        foreach (var button in new[] { _reloadButton, _saveDashboardButton, _copyButton, _exportTxtButton, _exportYamlButton, _exportDashboardButton })
        {
            if (button.Tag is string resourceName)
                button.Image = LoadEmbeddedToolbarImage(resourceName, new Size(_settings.IconSize, _settings.IconSize));
        }
    }

    private static void ApplyFontRecursively(Control parent, Font font)
    {
        foreach (Control control in parent.Controls)
        {
            if (control is not RichTextBox)
                control.Font = new Font(font.FontFamily, font.Size, control.Font.Style);
            if (control.HasChildren)
                ApplyFontRecursively(control, font);
        }
    }

    private void ShowAbout()
    {
        MessageBox.Show(this,
            "HADash\nVersion 0.9.4-preview\n\nEntwickelt von UGSo\nMit Hilfe von ChatGPT\n\nÖffnet Home-Assistant-Dashboards und Backup-Dateien in JSON oder YAML, listet Ansichten auf und exportiert komplette Dashboards oder einzelne Ansichten.",
            "Über HA – Dashboard Backup & Ansichten Exporteur by UGSo",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void OpenWebPage(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ShowError($"Die Webseite konnte nicht geöffnet werden.\n\n{ex.Message}");
        }
    }

    private void ShowInstructions()
    {
        using var dialog = new InstructionForm();
        dialog.ShowDialog(this);
    }

    private void SetInitialSplitterDistance()
    {
        // Keine Panel-MinSize-Werte setzen: WinForms prüft diese bereits beim
        // Zuweisen und kann während des Startlayouts eine Ausnahme auslösen.
        _splitContainer.PerformLayout();

        var availableWidth = _splitContainer.ClientSize.Width - _splitContainer.SplitterWidth;
        if (availableWidth <= 2)
            return;

        const int safeMargin = 25;
        var minimum = safeMargin;
        var maximum = availableWidth - safeMargin;
        if (maximum < minimum)
            return;

        var desired = Math.Clamp(320, minimum, maximum);

        try
        {
            _splitContainer.SplitterDistance = desired;
        }
        catch (InvalidOperationException)
        {
            // Das Betriebssystem kann während eines verspäteten DPI-/Layoutwechsels
            // nochmals Größen ändern. Die Standardposition bleibt dann gültig.
        }
    }

    private static string SerializeView(YamlNode node)
    {
        var stream = new YamlStream(new YamlDocument(node));
        using var writer = new StringWriter();
        stream.Save(writer, assignAnchors: false);
        return writer.ToString().Replace("...\r\n", "").Replace("...\n", "").TrimEnd() + Environment.NewLine;
    }

    private static string BuildViewName(YamlNode node, int index)
    {
        if (node is not YamlMappingNode mapping) return $"Ansicht {index + 1}";
        var title = GetScalarValue(mapping, "title");
        var path = GetScalarValue(mapping, "path");
        var icon = GetScalarValue(mapping, "icon");
        if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(path)) return $"{index + 1}. {title} ({path})";
        if (!string.IsNullOrWhiteSpace(title)) return $"{index + 1}. {title}";
        if (!string.IsNullOrWhiteSpace(path)) return $"{index + 1}. {path}";
        if (!string.IsNullOrWhiteSpace(icon)) return $"{index + 1}. {icon}";
        return $"Ansicht {index + 1}";
    }

    private static string? GetScalarValue(YamlMappingNode mapping, string key) =>
        TryGetMappingValue(mapping, key, out var value) && value is YamlScalarNode scalar ? scalar.Value : null;

    private static bool TryGetMappingValue(YamlMappingNode mapping, string key, out YamlNode value)
    {
        foreach (var pair in mapping.Children)
        {
            if (pair.Key is YamlScalarNode scalar && string.Equals(scalar.Value, key, StringComparison.OrdinalIgnoreCase))
            {
                value = pair.Value;
                return true;
            }
        }
        value = null!;
        return false;
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "HomeAssistant_Ansicht" : cleaned;
    }

    private void ClearDashboard()
    {
        _currentFilePath = null;
        _dashboardRoot = null;
        _currentDashboardYaml = null;
        _currentSourceWasJson = false;
        _allViews.Clear();
        _viewsListBox.Items.Clear();
        _yamlTextBox.Clear();
        _fileLabel.Text = "Keine Datei geladen";
        _viewsLabel.Text = "Ansichten";
        _reloadButton.Enabled = false;
        _saveDashboardButton.Enabled = false;
        SetSelectionActions(false);
        _statusLabel.Text = "Bereit";
    }

    private void CreateBackupIfRequired(string targetPath)
    {
        if (!_settings.CreateBackupCopies || !File.Exists(targetPath))
            return;
        try
        {
            Directory.CreateDirectory(AppSettings.BackupsDirectory);
            var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var backupName = $"{Path.GetFileNameWithoutExtension(targetPath)}_{stamp}{Path.GetExtension(targetPath)}.bak";
            File.Copy(targetPath, Path.Combine(AppSettings.BackupsDirectory, backupName), overwrite: false);
        }
        catch (Exception ex) { WriteLog($"Sicherungskopie fehlgeschlagen: {ex.Message}"); }
    }

    private void WriteLog(string message)
    {
        if (!_settings.LoggingEnabled) return;
        try
        {
            var directory = _settings.ResolvedLogDirectory;
            Directory.CreateDirectory(directory);
            File.AppendAllText(Path.Combine(directory, $"HADash-{DateTime.Now:yyyy-MM-dd}.log"), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}{Environment.NewLine}", new UTF8Encoding(false));
        }
        catch { }
    }

    private void ShowError(string message)
    {
        _statusLabel.Text = "Fehler";
        MessageBox.Show(this, message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
