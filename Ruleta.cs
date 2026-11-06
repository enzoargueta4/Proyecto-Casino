using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Proyecto_Casino
{
    public partial class Ruleta : Form
    {
        // --- Cadena de Conexión Universal (Compatible en GitHub para ambos) ---
        private readonly string _connectionString = @"Server=.\SQLEXPRESS;Database=DB_AZAR_USAM;Trusted_Connection=True;Encrypt=False;";

        // --- Variables de Estado del Juego ---
        private readonly Random random = new Random();
        private string selectedBetType = ""; // Guarda "RED", "BLACK", "PAR", "IMPAR" o el número ("23", "0", etc.)
        private int currentBetAmount = 0;

        // Variables dinámicas enlazadas a la base de datos
        private int _idJugadorActivo = 0;
        private decimal _saldoJugadorActivo = 0m;

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
            CargarComboJugadores(); // Llena el ComboBox al iniciar la pantalla
        }

        // ========================================================
        // CARGA DIRECTA DESDE SQL SERVER AL COMBOBOX
        // ========================================================
        private void CargarComboJugadores()
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Id_Jugador, Nombre + ' (' + Carnet + ')' AS InfoJugador FROM Jugador ORDER BY Nombre ASC";
                    using (var da = new SqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        cmbJugadores.DataSource = dt;
                        cmbJugadores.DisplayMember = "InfoJugador";
                        cmbJugadores.ValueMember = "Id_Jugador";
                    }
                }
                cmbJugadores.SelectedIndex = -1; // Inicia limpio sin selección obligatoria
                label4.Text = "Seleccione un Jugador para iniciar sesión";
            }
            catch (Exception ex)
            {
                label4.Text = "Modo Local (Sin BD SQL Server)";
                MessageBox.Show($"No se pudieron jalar los jugadores de SQL Server. Verifique su servicio local.\nDetalle: {ex.Message}", "Error Conectividad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cmbJugadores_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbJugadores.SelectedValue != null && int.TryParse(cmbJugadores.SelectedValue.ToString(), out int id))
            {
                _idJugadorActivo = id;
                ObtenerSaldoActualBD();
            }
        }

        private void ObtenerSaldoActualBD()
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Saldo FROM Jugador WHERE Id_Jugador = @Id";
                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Id", _idJugadorActivo);
                        con.Open();
                        object resultado = cmd.ExecuteScalar();
                        if (resultado != null)
                        {
                            _saldoJugadorActivo = Convert.ToDecimal(resultado);
                            label4.Text = $"Jugador Activo • Saldo Disponible: ${_saldoJugadorActivo:N2} USD";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al consultar el saldo del alumno: {ex.Message}", "Error SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void BotonNumero_Click(object sender, EventArgs e)
        {
            Button botonPresionado = (Button)sender;
            selectedBetType = botonPresionado.Text;
            label4.Text = $"Apuesta Seleccionada: Nº {selectedBetType}";
        }

        // --- EVENTOS VINCULADOS A TUS BOTONES LATERALES ---
        private void button39_Click(object sender, EventArgs e) => selectedBetType = "RED";
        private void button40_Click(object sender, EventArgs e) => selectedBetType = "BLACK";
        private void button41_Click(object sender, EventArgs e) => selectedBetType = "PAR";
        private void button42_Click(object sender, EventArgs e) => selectedBetType = "IMPAR";

        private void button43_Click(object sender, EventArgs e)
        {
            if (currentBetAmount - 10 >= 0) { currentBetAmount -= 10; textBox1.Text = currentBetAmount.ToString(); }
        }

        private void button44_Click(object sender, EventArgs e)
        {
            currentBetAmount += 10; textBox1.Text = currentBetAmount.ToString();
        }

        private void button45_Click(object sender, EventArgs e)
        {
            // 1. Sincroniza el jugador seleccionado antes de validar
            if (cmbJugadores.SelectedValue != null && int.TryParse(cmbJugadores.SelectedValue.ToString(), out int id))
            {
                _idJugadorActivo = id;
            }

            if (_idJugadorActivo == 0)
            {
                MessageBox.Show("Por favor, seleccione un jugador del menú desplegable antes de girar.", "Falta Jugador");
                return;
            }

            // 2. FORZAR LA CONSULTA DEL SALDO REAL EN LA BASE DE DATOS AQUÍ
            ObtenerSaldoActualBD();

            if (string.IsNullOrEmpty(selectedBetType) || !int.TryParse(textBox1.Text, out currentBetAmount) || currentBetAmount <= 0)
            {
                MessageBox.Show("Configure su apuesta correctamente seleccionando una casilla y un monto válido.", "Alerta");
                return;
            }

            if ((decimal)currentBetAmount > _saldoJugadorActivo)
            {
                MessageBox.Show("El jugador seleccionado no posee saldo suficiente para cubrir esta apuesta.", "Fondos Insuficientes");
                return;
            }



            if (string.IsNullOrEmpty(selectedBetType) || !int.TryParse(textBox1.Text, out currentBetAmount) || currentBetAmount <= 0)
            {
                MessageBox.Show("Configure su apuesta correctamente seleccionando una casilla y un monto válido.", "Alerta");
                return;
            }

            if ((decimal)currentBetAmount > _saldoJugadorActivo)
            {
                MessageBox.Show("El jugador seleccionado no posee saldo suficiente para cubrir esta apuesta.", "Fondos Insuficientes");
                return;
            }

            int randomIndex = random.Next(rouletteNumbers.Count);
            string winningNumber = rouletteNumbers[randomIndex];
            string colorResult = numberColors[winningNumber];
            string parityResult = (winningNumber != "0" && winningNumber != "00" && int.Parse(winningNumber) % 2 == 0) ? "PAR" : "IMPAR";

            bool playerWon = (selectedBetType == "RED" && colorResult == "RED") ||
                             (selectedBetType == "BLACK" && colorResult == "BLACK") ||
                             (selectedBetType == "PAR" && parityResult == "PAR") ||
                             (selectedBetType == "IMPAR" && parityResult == "IMPAR") ||
                             (selectedBetType == winningNumber);

            decimal gananciaCalculada = 0;
            if (playerWon)
            {
                gananciaCalculada = (selectedBetType == winningNumber) ? currentBetAmount * 35 : currentBetAmount;
            }
            else
            {
                gananciaCalculada = -currentBetAmount;
            }

            decimal saldoFinal = _saldoJugadorActivo + gananciaCalculada;
            string dbTipoApuesta = (selectedBetType == "RED" || selectedBetType == "BLACK") ? selectedBetType : "NUMERO";
            int intResultadoConvertido = (winningNumber == "00") ? -1 : int.Parse(winningNumber);

             ProcesarResultadoEnBD(dbTipoApuesta, selectedBetType, intResultadoConvertido, (decimal)currentBetAmount, playerWon ? gananciaCalculada : 0, saldoFinal);




            label4.Text = $"Salió: {winningNumber} ({colorResult})";

            if (playerWon)
            {
                button47.Text = $"¡GANASTE! +${gananciaCalculada:F2}";
                button47.BackColor = Color.Lime;
            }
            else
            {
                button47.Text = $"PERDISTE -${currentBetAmount}";
                button47.BackColor = Color.DarkRed;
            }
        }

        private void ProcesarResultadoEnBD(string tipoApuesta, string apuestaSeleccionada, int resultado, decimal montoApuesta, decimal ganancia, decimal saldoFinal)
        {
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(
                "UPDATE Jugador SET Saldo = @Saldo WHERE Id_Jugador = @IdJugador", con))
            {
                cmd.Parameters.AddWithValue("@Saldo", saldoFinal);
                cmd.Parameters.AddWithValue("@IdJugador", _idJugadorActivo);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            _saldoJugadorActivo = saldoFinal;
        }
    }
}