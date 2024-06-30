using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using Microsoft.Win32;

namespace CsvValidator
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, (string Pattern, bool IsUnique, bool AllowEmpty)> rules;
        private string currentRulesFileName = "default.xml";
        private string currentCsvFileName = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            LoadDefaultRules();
            UpdateWindowTitle();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                MessageBox.Show($"Unhandled Exception:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace.Take(5)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Menu action processing

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void UpdateWindowTitle()
        {
            Title = $"CSV file: {(string.IsNullOrEmpty(currentCsvFileName)?"none":currentCsvFileName)}    Rules {currentRulesFileName}";
        }

        private void OpenCsv_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                currentCsvFileName = Path.GetFileNameWithoutExtension(filePath);
                UpdateWindowTitle();
                ValidateCsv(filePath);
            }
        }

        private void EditCurrentRules_Click(object sender, RoutedEventArgs e)
        {
            // Implement the logic to edit current rules here
        }
        #endregion

        #region CSV processing

        private void ValidateCsv(string filePath)
        {
            var errors = new List<string>();
            var uniqueValueSets = new Dictionary<string, HashSet<string>>();

            foreach (var rule in rules)
            {
                if (rule.Value.IsUnique)
                {
                    uniqueValueSets[rule.Key] = new HashSet<string>();
                }
            }

            var lines = new List<string>();
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs))
            {
                while (!sr.EndOfStream)
                {
                    lines.Add(sr.ReadLine());
                }
            }

            if (lines.Count == 0)
            {
                DisplayError("CSV file is empty.");
                return;
            }

            var headers = SplitCsvLine(lines[0]);
            var headerIndexMap = new Dictionary<string, int>();

            for (int i = 0; i < headers.Length; i++)
            {
                foreach (var ruleKey in rules.Keys)
                {
                    if (headers[i].StartsWith(ruleKey))
                    {
                        headerIndexMap[ruleKey] = i;
                        break;
                    }
                }
            }

            for (int i = 1; i < lines.Count; i++)
            {
                var fields = SplitCsvLine(lines[i]);

                if (fields.Length != headers.Length)
                {
                    errors.Add($"Line {i + 1}: Incorrect number of fields.");
                    continue;
                }

                foreach (var ruleKey in rules.Keys)
                {
                    if (headerIndexMap.ContainsKey(ruleKey))
                    {
                        var fieldIndex = headerIndexMap[ruleKey];
                        var fieldValue = fields[fieldIndex].Trim();
                        var (regexPattern, isUnique, allowEmpty) = rules[ruleKey];

                        if (!Regex.IsMatch(fieldValue, regexPattern, RegexOptions.IgnoreCase))
                        {
                            var columnNumber = lines[i].IndexOf(fieldValue) + 1;
                            errors.Add($"Line {i + 1}: Invalid {ruleKey} format at column {columnNumber}. Value: '{fieldValue}'");
                        }

                        if (isUnique && !(allowEmpty && string.IsNullOrEmpty(fieldValue)) && !uniqueValueSets[ruleKey].Add(fieldValue))
                        {
                            var columnNumber = lines[i].IndexOf(fieldValue) + 1;
                            errors.Add($"Line {i + 1}: Duplicate {ruleKey} value at column {columnNumber}. Value: '{fieldValue}'");
                        }
                    }
                }
            }

            if (errors.Count == 0)
            {
                var message = $"{Path.GetFileName(filePath)} is VALID{Environment.NewLine}";
                message += $"{lines.Count-1} correct records found.";
                DisplaySuccess(message);
            }
            else
            {
                DisplayError(string.Join(Environment.NewLine, errors));
            }
        }

        private string[] SplitCsvLine(string line)
        {
            var pattern = @"(?:^|,)(?:""(?<val>[^""]*)""|(?<val>[^,]*))";
            var matches = Regex.Matches(line, pattern);
            return matches.Cast<Match>().Select(m => m.Groups["val"].Value).ToArray();
        }

        private void DisplaySuccess(string message)
        {
            ResultTextBox.Foreground = Brushes.Green;
            ResultTextBox.Text = message;
        }

        private void DisplayError(string message)
        {
            ResultTextBox.Foreground = Brushes.Red;
            ResultTextBox.Text = message;
        }

        #endregion

        #region Rules operations
        private Dictionary<string, (string Pattern, bool IsUnique, bool AllowEmpty)> LoadRulesFromXmlString(string xmlString)
        {
            var doc = XDocument.Parse(xmlString);
            return doc.Root.Elements("Rule")
                .ToDictionary(
                    el => el.Element("Name").Value,
                    el => (
                        el.Element("RegEx").Value,
                        bool.Parse(el.Element("IsUnique").Value),
                        bool.Parse(el.Element("AllowEmpty").Value)
                    )
                );
        }

        private void LoadDefaultRules()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, currentRulesFileName);
            if (File.Exists(filePath))
            {
                LoadRulesFromXmlFile(filePath);
            }
            else
            {
                rules = LoadRulesFromXmlString(DefaultRules.Xml);
            }
        }

        private void LoadValidationRules_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                LoadRulesFromXmlFile(filePath);
            }
        }

        private void SaveRulesAs_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                FileName = currentRulesFileName,
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                var filePath = saveFileDialog.FileName;
                SaveRulesToXmlFile(filePath);
            }
        }

        private void LoadRulesFromXmlFile(string filePath)
        {
            try
            {
                var doc = XDocument.Load(filePath);
                rules = doc.Root.Elements("Rule")
                    .ToDictionary(
                        el => el.Element("Name").Value,
                        el => (
                            el.Element("RegEx").Value,
                            bool.Parse(el.Element("IsUnique").Value),
                            bool.Parse(el.Element("AllowEmpty").Value)
                        )
                    );
                currentRulesFileName = Path.GetFileName(filePath);
                UpdateWindowTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading validation rules: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadDefaultRules();
            }
        }

        private void SaveRulesToXmlFile(string filePath)
        {
            try
            {
                var doc = new XDocument(
                    new XElement("Rules",
                        rules.Select(rule =>
                            new XElement("Rule",
                                new XElement("Name", rule.Key),
                                new XElement("RegEx", rule.Value.Pattern),
                                new XElement("IsUnique", rule.Value.IsUnique),
                                new XElement("AllowEmpty", rule.Value.AllowEmpty)
                            )
                        )
                    )
                );
                doc.Save(filePath);
                currentRulesFileName = Path.GetFileName(filePath);
                UpdateWindowTitle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving validation rules: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
