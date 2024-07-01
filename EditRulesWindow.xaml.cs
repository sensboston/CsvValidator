using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace CsvValidator
{
    public partial class EditRulesWindow : Window
    {
        public Dictionary<string, (string Pattern, bool IsUnique, bool AllowEmpty)> Rules { get; private set; }
        private string originalHeader;

        public EditRulesWindow(Dictionary<string, (string Pattern, bool IsUnique, bool AllowEmpty)> rules)
        {
            InitializeComponent();
            Rules = new Dictionary<string, (string Pattern, bool IsUnique, bool AllowEmpty)>(rules);
            UpdateRulesListBox();
        }

        private void UpdateRulesListBox()
        {
            RulesListBox.Items.Clear();
            foreach (var rule in Rules)
            {
                RulesListBox.Items.Add(rule.Key);
            }
        }

        private void RulesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RulesListBox.SelectedItem != null)
            {
                var selectedRuleName = RulesListBox.SelectedItem.ToString();
                var rule = Rules[selectedRuleName];
                originalHeader = selectedRuleName;
                HeaderTextBox.Text = selectedRuleName;
                IsUniqueCheckBox.IsChecked = rule.IsUnique;
                AllowEmptyCheckBox.IsChecked = rule.AllowEmpty;
                RegExTextBox.Text = rule.Pattern;
                DeleteButton.IsEnabled = true;
                EditorGrid.IsEnabled = true;
            }
            else
            {
                DeleteButton.IsEnabled = false;
                EditorGrid.IsEnabled = false; 
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newRuleName = "Empty";
            var newRule = (Pattern: @"^\d+$", IsUnique: false, AllowEmpty: true);
            Rules[newRuleName] = newRule;
            UpdateRulesListBox();
            RulesListBox.SelectedItem = newRuleName;
            RulesListBox.ScrollIntoView(newRuleName);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (RulesListBox.SelectedItem != null)
            {
                var selectedRuleName = RulesListBox.SelectedItem.ToString();
                Rules.Remove(selectedRuleName);
                UpdateRulesListBox();
                ClearBottomArea();
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var header = HeaderTextBox.Text;
            var pattern = RegExTextBox.Text;
            var isUnique = IsUniqueCheckBox.IsChecked ?? false;
            var allowEmpty = AllowEmptyCheckBox.IsChecked ?? false;

            if (string.IsNullOrWhiteSpace(header) || string.IsNullOrWhiteSpace(pattern))
            {
                MessageBox.Show("Header and RegEx cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Rules.Remove(originalHeader);
            Rules[header] = (pattern, isUnique, allowEmpty);
            UpdateRulesListBox();
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            ClearBottomArea();
            RulesListBox.SelectedItem = null;
        }

        private void ClearBottomArea()
        {
            HeaderTextBox.Text = string.Empty;
            IsUniqueCheckBox.IsChecked = false;
            AllowEmptyCheckBox.IsChecked = false;
            RegExTextBox.Text = string.Empty;
            HighlightTextBlock.Text = string.Empty;
        }

        private void RegExTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HighlightMatches();
        }

        private void TestValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HighlightMatches();
        }

        private void HighlightMatches()
        {
            var pattern = RegExTextBox.Text;
            var testValue = TestValueTextBox.Text;

            HighlightTextBlock.Inlines.Clear();

            try
            {
                var matches = Regex.Matches(testValue, pattern);
                int lastIndex = 0;

                foreach (Match match in matches)
                {
                    if (match.Index > lastIndex)
                    {
                        HighlightTextBlock.Inlines.Add(new Run(testValue.Substring(lastIndex, match.Index - lastIndex)));
                    }

                    var highlightedRun = new Run(testValue.Substring(match.Index, match.Length))
                    {
                        Background = Brushes.LightGreen
                    };

                    HighlightTextBlock.Inlines.Add(highlightedRun);
                    lastIndex = match.Index + match.Length;
                }

                if (testValue.Length == 0 || lastIndex < testValue.Length)
                {
                    HighlightTextBlock.Inlines.Add(new Run(testValue.Substring(lastIndex)));
                    ResultText.Text = "\uE0C7";
                    ResultText.Foreground = Brushes.Red;
                }
                else
                {
                    ResultText.Text = "\uE0A2";
                    ResultText.Foreground = Brushes.Green;
                }
            }
            catch (Exception)
            {
                HighlightTextBlock.Inlines.Add(new Run(testValue));
                ResultText.Text = "\uE0C7";
                ResultText.Foreground = Brushes.Red;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
