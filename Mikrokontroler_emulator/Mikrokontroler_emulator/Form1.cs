using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Reflection.Emit;

namespace Mikrokontroler_emulator
{
    public partial class Form1 : Form
    {
        private int currentLineIndex = 0;
        private string[] lines;

        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void label14_Click(object sender, EventArgs e)
        {
        }

        private void operacje(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateLinesFromTextBox();
            foreach (string line in lines)
            {
                ProcessLine(line);
            }
        }

        private void AddCommand(string[] args)
        {
            if (args.Length != 3) 
            {
                MessageBox.Show("Nieprawidłowy format rozkazu ADD. Oczekiwano: ADD <rejestr>, <rejestr/liczba>");
                return;
            }

            string destinationRegister = args[1].Trim().ToUpper().TrimEnd(','); 
            string source = args[2].Trim().ToUpper();

            int sourceValue;
            if (IsRegister(source))
            {
                sourceValue = GetRegisterValue(source);
            }
            else
            {
                bool isHex = source.EndsWith("H", StringComparison.OrdinalIgnoreCase);

                if (isHex)
                {
                    source = source.Substring(0, source.Length - 1);
                }

                try
                {
                    sourceValue = isHex ? int.Parse(source, NumberStyles.HexNumber) : int.Parse(source);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Nieprawidłowy format liczby.");
                    return;
                }
            }

            int destinationValue = GetRegisterValue(destinationRegister);

            int result = destinationValue + sourceValue;

            SetRegisterValue(destinationRegister, result);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.Title = "Open Text File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    string fileContent = File.ReadAllText(filePath);
                    richTextBox1.Text = fileContent;
                    UpdateLinesFromTextBox();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Problem z załadowaniem pliku: " + ex.Message);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (lines == null || currentLineIndex >= lines.Length)
            {
                UpdateLinesFromTextBox();
            }

            if (currentLineIndex < lines.Length)
            {
                HighlightCurrentLine(currentLineIndex);
                ProcessLine(lines[currentLineIndex]);

                currentLineIndex++;
            }
            else
            {
                MessageBox.Show("Brak dalszych linii kodu.");
            }
        }

        private void UpdateLinesFromTextBox()
        {
            lines = richTextBox1.Lines;
            currentLineIndex = 0;
        }

        private void ProcessLine(string line)
        {
            string[] words = line.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 0)
            {
                string command = words[0].ToUpper();
                switch (command)
                {
                    case "MOV":
                        MovCommand(words);
                        break;
                    case "SUB":
                        SubCommand(words);
                        break;
                    case "ADD":
                        AddCommand(words);
                        break;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string zero = "00000000";
            UpdateLinesFromTextBox();
            HighlightCurrentLine(currentLineIndex);
            label3.Text = zero; // AH
            label4.Text = zero;
            label5.Text = zero; // BH
            label6.Text = zero;
            label9.Text = zero; // CH
            label10.Text = zero;
            label13.Text = zero; // DH
            label14.Text = zero;
        }

        private void HighlightCurrentLine(int lineIndex)
        {
            richTextBox1.SelectAll();
            richTextBox1.SelectionBackColor = richTextBox1.BackColor;

            int start = richTextBox1.GetFirstCharIndexFromLine(lineIndex);
            int length = lines[lineIndex].Length;

            richTextBox1.Select(start, length);
            richTextBox1.SelectionBackColor = Color.Yellow;

            richTextBox1.ScrollToCaret();
        }

        private void MovCommand(string[] args)
        {
            if (args.Length != 3)
            {
                MessageBox.Show("Nieprawidłowy format rozkazu MOV. Oczekiwano: MOV <rejestr>, <liczba>");
                return;
            }

            string register = args[1].Trim().ToUpper().TrimEnd(',');
            string valueString = args[2].Trim();

            bool isHex = valueString.EndsWith("H", StringComparison.OrdinalIgnoreCase);

            if (isHex)
            {
                valueString = valueString.Substring(0, valueString.Length - 1);
            }

            int value;
            try
            {
                value = isHex ? int.Parse(valueString, NumberStyles.HexNumber) : int.Parse(valueString);
            }
            catch (FormatException)
            {
                MessageBox.Show("Nieprawidłowy format liczby.");
                return;
            }

            SetRegisterValue(register, value);
        }

        private void SetRegisterValue(string register, int value)
        {
            // konwersja na liczbę binarną rozszerzoną na 16 bitów
            string binaryValue = Convert.ToString(value, 2).PadLeft(16, '0');

            try
            {
                switch (register)
                {
                    case "AX":
                        string axHigh = binaryValue.Substring(0, 8);
                        string axLow = binaryValue.Substring(8);
                        label3.Text = axHigh; // AH
                        label4.Text = axLow; // AL
                        break;
                    case "AH":
                        label3.Text = binaryValue.Substring(binaryValue.Length - 8); 
                        break;
                    case "AL":
                        label4.Text = binaryValue.Substring(binaryValue.Length - 8); 
                        break;
                    case "BX":
                        string bxHigh = binaryValue.Substring(0, 8);
                        string bxLow = binaryValue.Substring(8);
                        label5.Text = bxHigh; // BH
                        label6.Text = bxLow; // BL
                        break;
                    case "BH":
                        label5.Text = binaryValue.Substring(binaryValue.Length - 8); 
                        break;
                    case "BL":
                        label6.Text = binaryValue.Substring(binaryValue.Length - 8); 
                        break;
                    case "CX":
                        string cxHigh = binaryValue.Substring(0, 8);
                        string cxLow = binaryValue.Substring(8);
                        label9.Text = cxHigh; // CH
                        label10.Text = cxLow; // CL
                        break;
                    case "CH":
                        label9.Text = binaryValue.Substring(binaryValue.Length - 8); 
                        break;
                    case "CL":
                        label10.Text = binaryValue.Substring(binaryValue.Length - 8); 
                        break;
                    case "DX":
                        string dxHigh = binaryValue.Substring(0, 8);
                        string dxLow = binaryValue.Substring(8);
                        label13.Text = dxHigh; // DH
                        label14.Text = dxLow; // DL
                        break;
                    case "DH":
                        label13.Text = binaryValue.Substring(binaryValue.Length - 8); 
                        break;
                    case "DL":
                        label14.Text = binaryValue.Substring(binaryValue.Length - 8);
                        break;
                    default:
                        MessageBox.Show($"Nieznany rejestr: {register}");
                        break;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Problem ze wpisaniem liczby do rejestru");
            }
        }

        private void SubCommand(string[] args)
        {
            if (args.Length != 3) 
            {
                MessageBox.Show("Nieprawidłowy format rozkazu SUB. Oczekiwano: SUB <rejestr>, <rejestr/liczba>");
                return;
            }

            string destinationRegister = args[1].Trim().ToUpper().TrimEnd(','); 
            string source = args[2].Trim().ToUpper();

            int sourceValue;
            if (IsRegister(source))
            {
                sourceValue = GetRegisterValue(source);
            }
            else
            {
                // Czy heksadecymalna liczba?
                bool isHex = source.EndsWith("H", StringComparison.OrdinalIgnoreCase);

                // 
                if (isHex)
                {
                    source = source.Substring(0, source.Length - 1);
                }

                // Konwersja na int
                try
                {
                    sourceValue = isHex ? int.Parse(source, NumberStyles.HexNumber) : int.Parse(source);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Nieprawidłowy format liczby.");
                    return;
                }
            }

            int destinationValue = GetRegisterValue(destinationRegister);

            
            int result = destinationValue - sourceValue;

           
            SetRegisterValue(destinationRegister, result);
        }

        private bool IsRegister(string value)
        {
            string[] registers = { "AX", "AH", "AL", "BX", "BH", "BL", "CX", "CH", "CL", "DX", "DH", "DL" };
            return registers.Contains(value);
        }

        private int GetRegisterValue(string register)
        {
            try
            {
                switch (register)
                {
                    case "AX":
                        string axHigh = label3.Text;
                        string axLow = label4.Text;
                        return Convert.ToInt32(axHigh + axLow, 2);
                    case "AH":
                        return Convert.ToInt32(label3.Text, 2);
                    case "AL":
                        return Convert.ToInt32(label4.Text, 2);
                    case "BX":
                        string bxHigh = label5.Text;
                        string bxLow = label6.Text;
                        return Convert.ToInt32(bxHigh + bxLow, 2);
                    case "BH":
                        return Convert.ToInt32(label5.Text, 2);
                    case "BL":
                        return Convert.ToInt32(label6.Text, 2);
                    case "CX":
                        string cxHigh = label9.Text;
                        string cxLow = label10.Text;
                        return Convert.ToInt32(cxHigh + cxLow, 2);
                    case "CH":
                        return Convert.ToInt32(label9.Text, 2);
                    case "CL":
                        return Convert.ToInt32(label10.Text, 2);
                    case "DX":
                        string dxHigh = label13.Text;
                        string dxLow = label14.Text;
                        return Convert.ToInt32(dxHigh + dxLow, 2);
                    case "DH":
                        return Convert.ToInt32(label13.Text, 2);
                    case "DL":
                        return Convert.ToInt32(label14.Text, 2);
                    default:
                        throw new ArgumentException($"Unknown register: {register}");
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Błąd odczytu rejestru: {register}");
                return 0;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.Title = "Save Text File";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                try
                {
                    File.WriteAllText(filePath, richTextBox1.Text);
                    MessageBox.Show("File saved successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Problem z zapisaniem pliku: " + ex.Message);
                }
            }
        }
    }
}
