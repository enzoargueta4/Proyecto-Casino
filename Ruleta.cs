using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Proyecto_Casino
{
    public partial class Ruleta : Form
    {
        // --- Variables de Estado del Juego ---
        private readonly Random random = new Random();
        private string selectedBetType = ""; // Guarda "RED", "BLACK", "PAR", "IMPAR" o el número ("23", "0", etc.)
        private int currentBetAmount = 0;

        // --- Mapeo de la Ruleta (Números y Colores oficiales) ---
        private readonly Dictionary<string, string> numberColors = new Dictionary<string, string>()
        {
            { "0", "GREEN" }, { "00", "GREEN" },
            { "1", "RED" }, { "2", "BLACK" }, { "3", "RED" }, { "4", "BLACK" }, { "5", "RED" }, { "6", "BLACK" },
            { "7", "RED" }, { "8", "BLACK" }, { "9", "RED" }, { "10", "BLACK" }, { "11", "BLACK" }, { "12", "RED" },
            { "13", "BLACK" }, { "14", "RED" }, { "15", "BLACK" }, { "16", "RED" }, { "17", "BLACK" }, { "18", "RED" },
            { "19", "RED" }, { "20", "BLACK" }, { "21", "RED" }, { "22", "BLACK" }, { "23", "RED" }, { "24", "BLACK" },
            { "25", "RED" }, { "26", "BLACK" }, { "27", "RED" }, { "28", "BLACK" }, { "29", "BLACK" }, { "30", "RED" },
            { "31", "BLACK" }, { "32", "RED" }, { "33", "BLACK" }, { "34", "RED" }, { "35", "BLACK" }, { "36", "RED" }
        };

        private readonly List<string> rouletteNumbers = new List<string>
        {
            "0", "00", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10",
            "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
            "21", "22", "23", "24", "25", "26", "27", "28", "29", "30",
            "31", "32", "33", "34", "35", "36"
        };

        public Ruleta()
        {
            InitializeComponent();
            AsignarEventosTablero();
        }

        private void Ruleta_Load(object sender, EventArgs e)
        {
            textBox1.Text = currentBetAmount.ToString();
        }

        // --- ESCANEO RECURSIVO DEL TABLERO DE NÚMEROS ---
        private void AsignarEventosTablero()
        {
            AsignarEventosRecursivos(this);
        }

        private void AsignarEventosRecursivos(Control contenedor)
        {
            foreach (Control c in contenedor.Controls)
            {
                if (c is Button btn)
                {
                    if (btn.Text == "00" || int.TryParse(btn.Text, out _))
                    {
                        btn.Click -= BotonNumero_Click;
                        btn.Click += BotonNumero_Click;
                    }
                }
                if (c.HasChildren)
                {
                    AsignarEventosRecursivos(c);
                }
            }
        }

        // Evento compartido al hacer clic en cualquier número del tapete (0, 00, 1-36)
        private void BotonNumero_Click(object sender, EventArgs e)
        {
            Button botonPresionado = (Button)sender;
            selectedBetType = botonPresionado.Text;
            label4.Text = $"Apuesta: Nº {selectedBetType}";
        }

        // --- EVENTOS VINCULADOS A TUS BOTONES LATERALES ---

        // Botón RED (button39)
        private void button39_Click(object sender, EventArgs e) => selectedBetType = "RED";

        // Botón BLACK (button40)
        private void button40_Click(object sender, EventArgs e) => selectedBetType = "BLACK";

        // Botón PAR (button41)
        private void button41_Click(object sender, EventArgs e) => selectedBetType = "PAR";

        // Botón IMPAR (button42)
        private void button42_Click(object sender, EventArgs e) => selectedBetType = "IMPAR";

        // Botón Apuesta "-" (button43)
        private void button43_Click(object sender, EventArgs e)
        {
            if (currentBetAmount - 10 >= 0)
            {
                currentBetAmount -= 10;
                textBox1.Text = currentBetAmount.ToString();
            }
        }

        // Botón Apuesta "+" (button44)
        private void button44_Click(object sender, EventArgs e)
        {
            currentBetAmount += 10;
            textBox1.Text = currentBetAmount.ToString();
        }

        // Botón "Girar Ruleta" (button45)
        private void button45_Click(object sender, EventArgs e)
        {
            // Sincronizar por si el usuario escribió el número manualmente
            if (int.TryParse(textBox1.Text, out int montoManual))
            {
                currentBetAmount = montoManual;
            }

            if (string.IsNullOrEmpty(selectedBetType))
            {
                MessageBox.Show("Por favor, seleccione un número o condición (RED, BLACK, PAR, IMPAR) para apostar.", "Alerta");
                return;
            }

            if (currentBetAmount <= 0)
            {
                MessageBox.Show("Por favor, aumente el monto de su apuesta.", "Alerta");
                return;
            }

            int randomIndex = random.Next(rouletteNumbers.Count);
            string winningNumber = rouletteNumbers[randomIndex];
            string colorResult = numberColors[winningNumber];
            string parityResult = "Invalido";

            if (winningNumber != "0" && winningNumber != "00")
            {
                int numericValue = int.Parse(winningNumber);
                parityResult = (numericValue % 2 == 0) ? "PAR" : "IMPAR";
            }

            label4.Text = $"{winningNumber} * {colorResult} * {(parityResult != "Invalido" ? parityResult : "Green Side")}";

            bool playerWon = false;
            if (selectedBetType == "RED" && colorResult == "RED") playerWon = true;
            else if (selectedBetType == "BLACK" && colorResult == "BLACK") playerWon = true;
            else if (selectedBetType == "PAR" && parityResult == "PAR") playerWon = true;
            else if (selectedBetType == "IMPAR" && parityResult == "IMPAR") playerWon = true;
            else if (selectedBetType == winningNumber) playerWon = true;

            if (playerWon)
            {
                button47.Text = "¡GANASTE! +";
                button47.BackColor = Color.Lime;
            }
            else
            {
                button47.Text = "PERDISTE";
                button47.BackColor = Color.DarkRed;
            }
        }

        // Botón "Limpiar" (button46)
        private void button46_Click(object sender, EventArgs e)
        {
            selectedBetType = "";
            currentBetAmount = 0;
            textBox1.Text = "0";

            label4.Text = "- * - * -";
            button47.Text = "";
            button47.BackColor = Color.Transparent;
        }

        // --- MÉTODOS REQUERIDOS POR EL DISEÑADOR (NO BORRAR) ---
        // Estos métodos vacíos solucionan los errores del Diseñador.cs
        private void label1_Click(object sender, EventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e) { }
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void pictureBox2_Click_1(object sender, EventArgs e) { }
        private void tableLayoutPanel1_Paint_1(object sender, PaintEventArgs e) { }
    }
}
