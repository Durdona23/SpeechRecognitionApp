using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace SpeechRecognitionApp
{
    public class SpeechHandler
    {
        private SpeechRecognitionEngine speechRecognizer;
        private SpeechSynthesizer speechSynthesizer;
        private Dictionary<string, string> commands;
        public event Action<string, bool> StatusUpdated;
        public ObservableCollection<string> RecognizedPhrases { get; private set; }
        public event Action<Action> DispatcherQueueChanged;

        public SpeechHandler()
        {
            RecognizedPhrases = new ObservableCollection<string>();
            speechSynthesizer = new SpeechSynthesizer();
            commands = new Dictionary<string, string>();
            LoadCommands();
            InitializeSpeechRecognition();
        }

        private void LoadCommands()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AssistantCommands.txt");
            if (File.Exists(filePath))
            {
                foreach (var line in File.ReadLines(filePath))
                {
                    var parts = line.Split(new[] { " : " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        commands[parts[0]] = parts[1];
                    }
                }
            }
        }

        private Grammar BuildGrammar()
        {
            var choices = new Choices(commands.Keys.ToArray());
            var grammarBuilder = new GrammarBuilder(choices);
            return new Grammar(grammarBuilder);
        }

        private void InitializeSpeechRecognition()
        {
            speechRecognizer = new SpeechRecognitionEngine();
            speechRecognizer.SetInputToDefaultAudioDevice();
            speechRecognizer.LoadGrammar(BuildGrammar());

            speechRecognizer.SpeechRecognized += SpeechRecognizer_SpeechRecognized;

            StartRecognition();
        }

        private async void SpeechRecognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            UpdateStatus("Listening...", true);
            await Task.Delay(100); // Simulate processing delay (adjust as needed)

            await Task.Run(() =>
            {
                var recognizedPhrase = e.Result.Text;
                string response = string.Empty;
                if (commands.TryGetValue(recognizedPhrase, out response))
                {
                    response = ReplacePlaceholders(response);
                    speechSynthesizer.Speak(response);
                }
                else
                {
                    speechSynthesizer.Speak("I did not understand that.");
                }

                UpdateStatus("Recognition stopped", false);

                RecognizedPhrases.Add($"{recognizedPhrase} : {response}");
            });
        }

        private string ReplacePlaceholders(string response)
        {
            if (response.Contains("-time-"))
            {
                response = response.Replace("-time-", DateTime.Now.ToString("h:mm tt"));
            }

            if (response.Contains("-datetoday-"))
            {
                response = response.Replace("-datetoday-", DateTime.Now.ToString("MMMM dd, yyyy"));
            }

            return response;
        }

        public void StartRecognition()
        {
            speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void SetVoice(string voiceName, VoiceGender gender, int rate)
        {
            speechSynthesizer.SelectVoice(voiceName);
            speechSynthesizer.SelectVoiceByHints(gender);
            speechSynthesizer.Rate = rate;
        }

        public IEnumerable<string> GetInstalledVoices()
        {
            return speechSynthesizer.GetInstalledVoices().Select(v => v.VoiceInfo.Name);
        }

        private void UpdateStatus(string message, bool isActive)
        {
            DispatcherQueueChanged.Invoke(() =>
            {
                StatusUpdated?.Invoke(message, isActive);
            });
        }
    }
}
