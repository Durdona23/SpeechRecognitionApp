using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Speech.Synthesis;

namespace SpeechRecognitionApp
{
    public partial class MainWindow : Window
    {
        private SpeechHandler speechHandler;

        public MainWindow()
        {
            InitializeComponent();
            ResizeWindow();

            speechHandler = new SpeechHandler();
            speechHandler.StatusUpdated += UpdateStatus;
            speechHandler.DispatcherQueueChanged += OnDispatcherQueueChanged;

            RecognizedPhrasesListBox.ItemsSource = speechHandler.RecognizedPhrases;

            InitializeVoiceSettings();
        }

        private void OnDispatcherQueueChanged(Action action)
        {
            DispatcherQueue.TryEnqueue(() => action.Invoke());
        }

        private void ResizeWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 800, Height = 900 });
        }

        private void InitializeVoiceSettings()
        {
            foreach (var voice in speechHandler.GetInstalledVoices())
            {
                VoiceComboBox.Items.Add(voice);
            }

            if (VoiceComboBox.Items.Count > 0)
                VoiceComboBox.SelectedIndex = 0;
        }

        private void StartRecognitionButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Listening...", isActive: true);
        }

        private void StopRecognitionButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Recognition stopped", isActive: false);
        }

        private void ClearTextButton_Click(object sender, RoutedEventArgs e)
        {
            RecognizedTextBox.Text = string.Empty;
            speechHandler.RecognizedPhrases.Clear();
            StatusTextBlock.Text = string.Empty;
        }

        private void UpdateStatus(string message, bool isActive)
        {
            RecognizedTextBox.Text += $"{message}...\n";
            RecognitionProgressRing.IsActive = isActive;
            StatusTextBlock.Text = message;
        }

        private void VoiceSettingsChanged(object sender, SelectionChangedEventArgs e)
        {
            SetVoiceSettings();
        }

        private void VoiceSettingsSpeedValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            SetVoiceSettings();
        }

        private void SetVoiceSettings()
        {
            if (VoiceComboBox.SelectedItem == null) return;

            var selectedVoice = VoiceComboBox.SelectedItem.ToString();
            var selectedGender = ((ComboBoxItem)GenderComboBox.SelectedItem).Content.ToString();
            var gender = selectedGender == "Male" ? VoiceGender.Male : VoiceGender.Female;
            var rate = (int)((VoiceSpeedSlider.Value - 1) * 10);

            speechHandler.SetVoice(selectedVoice, gender, rate);
        }
    }
}
