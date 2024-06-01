using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace SpeechRecognitionApp
{
    public sealed partial class MainWindow : Window
    {
        private SpeechRecognitionEngine speechRecognizer;
        private ObservableCollection<string> RecognizedPhrases;
        private SpeechSynthesizer speechSynthesizer;
        private float ConfidenceThreshold = 0.8f;
        public MainWindow()
        {
            this.InitializeComponent();
            ResizeWindow();

            RecognizedPhrases = new ObservableCollection<string>();
            RecognizedPhrasesListBox.ItemsSource = RecognizedPhrases;
            InitializeSpeechRecognition();
            InitializeVoiceSettings();
        }

        private void ResizeWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 700, Height = 800 });
        }

        private void InitializeSpeechRecognition()
        {
            speechRecognizer = new SpeechRecognitionEngine();
            speechRecognizer.SetInputToDefaultAudioDevice();

            speechRecognizer.SpeechRecognized += SpeechRecognizer_SpeechRecognized;
            speechRecognizer.RecognizeCompleted += SpeechRecognizer_RecognizeCompleted;

            Choices commands = new Choices();
            commands.Add(new string[] { "start", "stop", "clear", "exit", "open", "close", "save", "load", "action one", "action two", "action three" });

            GrammarBuilder grammarBuilder = new GrammarBuilder();
            grammarBuilder.Append(commands);

            Grammar grammar = new Grammar(grammarBuilder);
            speechRecognizer.LoadGrammar(grammar);
        }

        private void InitializeVoiceSettings()
        {
            var speechSynthesizer = new SpeechSynthesizer();
            var installedVoices = speechSynthesizer.GetInstalledVoices();
            foreach (var voice in installedVoices)
            {
                VoiceComboBox.Items.Add(voice.VoiceInfo.Name);
            }
            if (VoiceComboBox.Items.Count > 0)
                VoiceComboBox.SelectedIndex = 0;
            GenderComboBox.SelectedIndex = 0; // Default to Male
        }

        private void ApplySettingsButton()
        {
            // Apply voice settings
            if (VoiceComboBox.SelectedItem != null)
            {
                var selectedVoice = VoiceComboBox.SelectedItem.ToString();
                speechSynthesizer.SelectVoice(selectedVoice);
                var selectedGender = ((ComboBoxItem)GenderComboBox.SelectedItem).Content.ToString();
                speechSynthesizer.SelectVoiceByHints(selectedGender == "Male" ? VoiceGender.Male : VoiceGender.Female);
            }

            speechSynthesizer.Rate = (int)((VoiceSpeedSlider.Value - 1) * 10); // Voice rate range is -10 to 10

            // Apply confidence level
            ConfidenceThreshold = (float)ConfidenceSlider.Value;
        }

        private void SpeechRecognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string recognizedPhrase = e.Result.Text;
            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    RecognizedTextBox.Text += $"Recognized: {recognizedPhrase}\n";
                    RecognizedPhrases.Add(recognizedPhrase);
                    PerformAction(recognizedPhrase);
                });
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    RecognizedTextBox.Text += $"Confidence level too low for: {recognizedPhrase}\n";
                });
            }
        }

        private void PerformAction(string recognizedPhrase)
        {
            switch (recognizedPhrase.ToLower())
            {
                case "action one":
                    // Execute action 1
                    break;
                case "action two":
                    // Execute action 2
                    break;
                case "action three":
                    // Execute action 3
                    break;
                default:
                    // Handle unrecognized commands or other actions
                    break;
            }
        }

        private void SpeechRecognizer_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    RecognizedTextBox.Text += $"Recognition error: {e.Error.Message}\n";
                });
            }
            else if (e.Cancelled)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    RecognizedTextBox.Text += "Recognition cancelled.\n";
                });
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    RecognizedTextBox.Text += "Recognition completed.\n";
                });
            }
        }

        private void SpeechRecognizer_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                RecognizedTextBox.Text += "Speech not recognized. Please try again.\n";
            });
        }


        private void StartRecognitionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
                RecognizedTextBox.Text += "Recognition started...\n";
                RecognitionProgressRing.IsActive = true;
                StatusTextBlock.Text = "Listening...";
            }
            catch (InvalidOperationException ex)
            {
                RecognizedTextBox.Text += "Error starting recognition: " + ex.Message + "\n";
                StatusTextBlock.Text = "Error starting recognition";
            }
        }

        private void StopRecognitionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                speechRecognizer.RecognizeAsyncStop();
                RecognizedTextBox.Text += "Recognition stopped...\n";
                RecognitionProgressRing.IsActive = false;
                StatusTextBlock.Text = "Recognition stopped";
            }
            catch (InvalidOperationException ex)
            {
                RecognizedTextBox.Text += "Error stopping recognition: " + ex.Message + "\n";
                StatusTextBlock.Text = "Error stopping recognition";
            }
        }

        private void ClearTextButton_Click(object sender, RoutedEventArgs e)
        {
            RecognizedTextBox.Text = string.Empty;
            RecognizedPhrases.Clear();
            StatusTextBlock.Text = string.Empty;
        }
    }
}
