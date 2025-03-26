using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EnigmaMachine2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Constants for rotor wiring and reflector
        string _control = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Standard alphabet for reference
        string _ring1 = "DMTWSILRUYQNKFEJCAZBPGXOHV"; // Rotor 1 wiring (Hours)
        string _ring2 = "HQZGPJTMOBLNCIFDYAWVEUSRKX"; // Rotor 2 wiring (Minutes)
        string _ring3 = "UQNTLSZFMREHDPXKIBVYGJCWOA"; // Rotor 3 wiring (Seconds)
        string _reflector = "YRUHQSLDPXNGOKMIEBFZCWVJAT"; // Reflector wiring

        // Rotor offset tracking
        int[] _keyOffset = { 0, 0, 0 }; // Current rotor offsets (H, M, S)
        int[] _initOffset = { 0, 0, 0 }; // Initial rotor offsets (H, M, S)

        // Rotor state flag
        bool _rotor = false;

        // Plugboard setup
        Dictionary<char, char> _plugboard = new Dictionary<char, char>(); // Plugboard dictionary
        private bool _plugboardSet = false; // Flag to indicate if plugboard is set

        // Constructor

        private TextBox[] textBoxArray = new TextBox[26];
        private Rectangle[] lamps = new Rectangle[26];

        public MainWindow()
        {
            InitializeComponent();

            SetDefaults(); // Initialize default values

            _rotor = false; // Initially rotor is off
            btnRotor.Content = "Rotor On"; // Set button text
            btnRotor.IsEnabled = false; // Disable rotor button until plugboard is set

            textBoxArray = new TextBox[]
            {
                TB1, TB2, TB3, TB4, TB5, TB6, TB7, TB8, TB9, TB10, TB11, TB12, TB13, TB14, 
                TB15, TB16, TB17, TB18, TB19, TB20, TB21, TB22, TB23, TB24, TB25, TB26
            };

            lamps = new Rectangle[]
            {
                Lamp1, Lamp2, Lamp3, Lamp4, Lamp5, Lamp6, Lamp7, Lamp8, Lamp9, Lamp10, 
                Lamp11, Lamp12, Lamp13, Lamp14, Lamp15, Lamp16, Lamp17, Lamp18, 
                Lamp19, Lamp20, Lamp21, Lamp22, Lamp23, Lamp24, Lamp25, Lamp26
            };
        }

        // Display rotor wiring in UI labels
        private void DisplayRing(Label displayLabel, string ring)
        {
            string temp = "";
            foreach (char r in ring)
                temp += r + "\t"; // Add tab for spacing
            displayLabel.Content = temp;
        }

        // Find the index of a character in a string
        private int IndexSearch(string ring, char letter)
        {
            int index = 0;
            for (int x = 0; x < ring.Length; x++)
            {
                if (ring[x] == letter)
                {
                    index = x;
                    break;
                }
            }
            return index;
        }

        // Handle keyboard input
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Check for uppercase letters and message length
            if (_plugboardSet && _rotor)
            {
                if (e.Key.ToString().Length == 1)
                {
                    if ((int)e.Key.ToString()[0] >= 65 && (int)e.Key.ToString()[0] <= 90)
                    {
                        for (int x = 0; x < 26; x++)
                        {
                            lamps[x].Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFF2F2"));
                            if (e.Key.ToString() == textBoxArray[x].Text)
                            {
                                lamps[x].Fill = new SolidColorBrush(Colors.MediumVioletRed);
                            }
                            if (Encrypt(e.Key.ToString()[0]) == textBoxArray[x].Text[0])
                            {
                                lamps[x].Fill = new SolidColorBrush(Color.FromArgb(0xFF, 0x7C, 0x9C, 0xFF));
                            }
                        }

                        lblInput.Text += e.Key.ToString(); // Append input character
                        lblEncrpyt.Text += Encrypt(e.Key.ToString()[0]) + ""; // Encrypt and append
                        lblEncrpytMirror.Text += Mirror(e.Key.ToString()[0]) + ""; // Mirror and append

                        if (_rotor)
                        {
                            Rotate(true);
                            DisplayRing(lblRing1, _ring1); // Update rotor display
                            DisplayRing(lblRing2, _ring2);
                            DisplayRing(lblRing3, _ring3);
                        }
                    }
                }
                // Handle space key
                else if (e.Key == Key.Space)
                {
                    lblInput.Text += " ";
                    lblEncrpyt.Text += " ";
                    lblEncrpytMirror.Text += " ";
                }
                else if (e.Key == Key.Back)
                {
                    Rotate(false); // Rotate rotors backward
                    DisplayRing(lblRing1, _ring1);
                    DisplayRing(lblRing2, _ring2);
                    DisplayRing(lblRing3, _ring3);

                    lblInput.Text = RemoveLastLetter(lblInput.Text.ToString()); // Remove last character
                    lblEncrpyt.Text = RemoveLastLetter(lblEncrpyt.Text.ToString());
                    lblEncrpytMirror.Text = RemoveLastLetter(lblEncrpytMirror.Text.ToString());
                }
            }
        }
        // Encrypt a character
        private char Encrypt(char letter)
        {
            char newChar = letter;

            // Plugboard pass (before rotors)
            if (_plugboard.ContainsKey(newChar))
                newChar = _plugboard[newChar];
            else if (_plugboard.ContainsValue(newChar))
                newChar = _plugboard.FirstOrDefault(x => x.Value == newChar).Key;

            // Rotor pass forward
            newChar = _ring1[IndexSearch(_control, newChar)];
            newChar = _ring2[IndexSearch(_control, newChar)];
            newChar = _ring3[IndexSearch(_control, newChar)];

            // Reflector pass
            newChar = _reflector[IndexSearch(_control, newChar)];

            // Rotor pass backward
            newChar = _ring3[IndexSearch(_control, newChar)];
            newChar = _ring2[IndexSearch(_control, newChar)];
            newChar = _ring1[IndexSearch(_control, newChar)];

            // Plugboard pass (after rotors)
            if (_plugboard.ContainsKey(newChar))
                newChar = _plugboard[newChar];
            else if (_plugboard.ContainsValue(newChar))
                newChar = _plugboard.FirstOrDefault(x => x.Value == newChar).Key;


            
            return newChar;
        }

        // Mirror a character (encrypt and pass back through rotors)
        private char Mirror(char letter)
        {
            char newChar = Encrypt(letter);

            newChar = _ring3[IndexSearch(_control, newChar)];
            newChar = _ring2[IndexSearch(_control, newChar)];
            newChar = _ring1[IndexSearch(_control, newChar)];

            return newChar;
        }

        // Set default values
        private void SetDefaults()
        {
            _control = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            _ring1 = "DMTWSILRUYQNKFEJCAZBPGXOHV";
            _ring2 = "HQZGPJTMOBLNCIFDYAWVEUSRKX";
            _ring3 = "UQNTLSZFMREHDPXKIBVYGJCWOA";

            _keyOffset = new int[] { 0, 0, 0 };

            lblInput.Text = "";
            lblEncrpyt.Text = "";
            lblEncrpytMirror.Text = "";

            DisplayRing(lblRing1, _ring1);
            DisplayRing(lblRing2, _ring2);
            DisplayRing(lblRing3, _ring3);
            DisplayRing(lblReflector, _reflector); // Display Reflector Ring

        }

        // Rotate rotors
        private void Rotate(bool forward)
        {
            if (forward)
            {
                _keyOffset[2]++;
                _ring3 = MoveValues(forward, _ring3);

                if (_keyOffset[2] / _control.Length >= 1)
                {
                    _keyOffset[2] = 0;
                    _keyOffset[1]++;
                    _ring2 = MoveValues(forward, _ring2);
                    if (_keyOffset[1] / _control.Length >= 1)
                    {
                        _keyOffset[1] = 0;
                        _keyOffset[0]++;
                        _ring1 = MoveValues(forward, _ring1);
                    }
                    else
                    {
                        if (_keyOffset[2] > 0 || _keyOffset[1] > 0)
                        {
                            _keyOffset[2]--;
                            _ring3 = MoveValues(forward, _ring3);
                            if (_keyOffset[2] < 0)
                            {
                                _keyOffset[2] = 25;
                                _keyOffset[1]--;
                                _ring2 = MoveValues(forward, _ring2);
                                if (_keyOffset[1] < 0)
                                {
                                    _keyOffset[1] = 25;
                                    _keyOffset[0]--;
                                    _ring1 = MoveValues(forward, _ring1);
                                    if (_keyOffset[0] < 0)
                                        _keyOffset[0] = 25;
                                }
                            }
                        }
                    }
                }
            }
            DisplayOffset(); // Update offset display
        }

        // Move rotor values
        private string MoveValues(bool forward, string ring)
        {
            char movingValue = ' ';
            string newRing = "";

            if (forward)
            {
                movingValue = ring[0];
                for (int x = 1; x < ring.Length; x++)
                    newRing += ring[x];
                newRing += movingValue;
            }
            else
            {
                movingValue = ring[25];
                for (int x = 0; x < ring.Length - 1; x++)
                    newRing += ring[x];
                newRing = movingValue + newRing;
            }

            return newRing;
        }

        // Handle rotor button click
        private void btnRotor_Click(object sender, RoutedEventArgs e)
        {
            SetDefaults();

            if (IsValidNumericInput(txtBRing1Init.Text) &&
            IsValidNumericInput(txtBRing2Init.Text) &&
             IsValidNumericInput(txtBRing3Init.Text))
            {
                // Try parsing the input values as integers
                if (int.TryParse(txtBRing1Init.Text, out _initOffset[0]) &&
                    int.TryParse(txtBRing2Init.Text, out _initOffset[1]) &&
                    int.TryParse(txtBRing3Init.Text, out _initOffset[2]))
                {
                    // Check if the values are within the valid range (0 to 25)
                    if (_initOffset[0] >= 0 && _initOffset[0] <= 25 &&
                        _initOffset[1] >= 0 && _initOffset[1] <= 25 &&
                        _initOffset[2] >= 0 && _initOffset[2] <= 25)
                    {
                        txtBRing1Init.IsEnabled = false;
                        txtBRing2Init.IsEnabled = false;
                        txtBRing3Init.IsEnabled = false;

                        _rotor = true;
                        MessageBox.Show("Rotors have been set");
                        btnRotor.IsEnabled = false;
                        btnRotor.Content = "Settings Lock";

                        _ring1 = InitializeRotors(_initOffset[0], _ring1);
                        _ring2 = InitializeRotors(_initOffset[1], _ring2);
                        _ring3 = InitializeRotors(_initOffset[2], _ring3);

                        // Display the updated ring values
                        DisplayRing(lblRing1, _ring1);
                        DisplayRing(lblRing2, _ring2);
                        DisplayRing(lblRing3, _ring3);
                        DisplayOffset();
                    }
                    else
                    {
                        MessageBox.Show("Please enter numbers between 0 and 25 for the rotor offsets.");
                    }
                }
                
            }
            else
            {
                MessageBox.Show("Please only input numbers between 0 and 25 for the rotor offsets.");
            }
        }

        private bool IsValidNumericInput(string input)
        {
            foreach (char c in input)
            {
                if (!char.IsDigit(c)) 
                {
                    return false; 
                }
            }
            return true; 
        }

        // Initialize rotors with initial offset
        private string InitializeRotors(int initial, string ring)
        {
            string newRing = ring;
            for (int x = 0; x < initial; x++)
                newRing = MoveValues(true, newRing);
            return newRing;
        }

        // Remove last letter from a string
        private string RemoveLastLetter(string word)
        {
            string newWord = "";
            for (int x = 0; x < word.Length - 1; x++)
                newWord += word[x];
            return newWord;
        }


        // Display rotor offsets
        private void DisplayOffset()
        {
            txtBRing1Init.Text = _keyOffset[0] + "";
            txtBRing2Init.Text = _keyOffset[1] + "";
            txtBRing3Init.Text = _keyOffset[2] + "";
        }

        // Setup plugboard
        private void SetupPlugboard(string plugboardSettings)
        {
            _plugboard.Clear();
            string[] pairs = plugboardSettings.ToUpper().Split(' ');
            foreach (string pair in pairs)
            {
                if (pair.Length == 2)
                {
                    _plugboard[pair[0]] = pair[1];
                    _plugboard[pair[1]] = pair[0];
                }
            }
        }

        // Handle plugboard button click
        private void btnSetPlugboard_Click(object sender, RoutedEventArgs e)
        {
            string plugboardInput = txtPlugboard.Text;

            if (String.IsNullOrWhiteSpace(plugboardInput) || ValidPB(plugboardInput) == true)
            {
                SetupPlugboard(plugboardInput);
                _plugboardSet = true;
                MessageBox.Show("Plugboard has been set!");
                btnSetPlugboard.IsEnabled = false;
                btnRotor.IsEnabled = true;

                if (_plugboardSet)
                    txtPlugboard.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("Please enter valid plugboard pairs or leave the field empty.");
                return;
            }
          
        }

        private void txtPlugboard_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblInput.Text = "";
            lblEncrpyt.Text = "";
            lblEncrpytMirror.Text = "";
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void lblInput_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private bool ValidPB(string input)
        {
            string[] pairs = input.ToUpper().Split(' ');

            if (pairs.Length == 0)
                return true;
            
            if (pairs.Length > 10)
                return false;

            foreach (string pair in pairs)
            {
                if (pair.Length != 2)
                    return false;

                if (!pair.All(char.IsLetter))
                    return false;
            }

            List<char> usedLetters = new List<char>(); 
            foreach (string pair in pairs)
            {
                if (usedLetters.Contains(pair[0]) || usedLetters.Contains(pair[1]))
                {
                    MessageBox.Show("Each letter can only be used once in the plugboard setup.");
                    return false;
                }
                usedLetters.Add(pair[0]);
                usedLetters.Add(pair[1]);
            }
            return true;
        }
    }
}
