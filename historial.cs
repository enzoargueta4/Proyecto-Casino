using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Proyecto_Casino
{
    public partial class historial : Form
    {
        // 🔗 --- Cadena de Conexión Real para tu Servidor ---
        private readonly string _connectionString = @"Data Source=DESKTOP-GDAOJOT\SQLEXPRESS;Initial Catalog=DB_AZAR_USAM;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        public historial()
        {
            InitializeComponent();
        }

        private void historial_Load(object sender, EventArgs e)
        {
            try
            {
                // Configurar los rangos por defecto en base a tus registros reales
                dtpDesde.Value = new DateTime(2026, 1, 1);
                dtpHasta.Value = DateTime.Now;
            }
            catch { }

            CargarComboJugadoresFiltro();
            CargarHistorialCompleto();
            CalcularTopGanador();
        }
        // ========================================================
        // 👥 CARGAR JUGADORES EN EL COMBOBOX
        // ========================================================
        private void CargarComboJugadoresFiltro()
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Id_Jugador, Nombre FROM Jugador ORDER BY Nombre ASC";
                    using (var da = new SqlDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        DataRow filaTodos = dt.NewRow();
                        filaTodos["Id_Jugador"] = DBNull.Value;
                        filaTodos["Nombre"] = "[Todos los Jugadores]";
                        dt.Rows.InsertAt(filaTodos, 0);

                        cmbJugadores.DataSource = dt;
                        cmbJugadores.DisplayMember = "Nombre";
                        cmbJugadores.ValueMember = "Id_Jugador";
                    }
                }
                cmbJugadores.SelectedIndex = 0;
            }
            catch { }
        }

        // ========================================================
        // 🔍 FILTRAR Y LLENAR TU TABLA CON AUTOGENERACIÓN DE FILAS
        // ========================================================
        public void CargarHistorialCompleto()
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT 
                            p.Fecha AS [Fecha Partida], 
                            j.Nombre AS [Nombre Jugador], 
                            p.Eleccion AS [Apuesta Seleccionada], 
                            p.Resultado AS [Nº Ganador], 
                            p.MontoApostado AS [Monto Apostado], 
                            p.Ganancia AS [Ganancia Neta]
                        FROM Partida p
                        INNER JOIN Jugador j ON p.Id_Jugador = j.Id_Jugador
                        WHERE p.Fecha BETWEEN @Desde AND @Hasta";

                    if (cmbJugadores.SelectedValue != null && cmbJugadores.SelectedValue != DBNull.Value)
                    {
                        query += " AND p.Id_Jugador = @IdJugador";
                    }

                    query += " ORDER BY p.Fecha DESC";

                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Desde", dtpDesde.Value.Date);
                        cmd.Parameters.AddWithValue("@Hasta", dtpHasta.Value.Date.AddDays(1).AddTicks(-1));

                        if (cmbJugadores.SelectedValue != null && cmbJugadores.SelectedValue != DBNull.Value)
                        {
                            cmd.Parameters.AddWithValue("@IdJugador", cmbJugadores.SelectedValue);
                        }

                        using (var da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            // Limpiar y forzar autogeneración limpia desde las columnas de SQL
                            dgvResultados.Columns.Clear();
                            dgvResultados.AutoGenerateColumns = true;
                            dgvResultados.DataSource = dt;

                            if (lblRegistrosEncontrados != null)
                            {
                                lblRegistrosEncontrados.Text = $"RESULTADOS — {dt.Rows.Count} registros encontrados";
                            }

                            CalcularTotalesVariables();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el historial: {ex.Message}", "Aviso Casino");
            }
        }
        // ========================================================
        // 📊 RECALCULAR TOTALES USANDO LOS ÍNDICES DE LAS CELDAS
        // ========================================================
        private void CalcularTotalesVariables()
        {
            int totalPartidas = dgvResultados.Rows.Count;
            decimal totalApostado = 0;
            decimal totalGanancia = 0;

            foreach (DataGridViewRow row in dgvResultados.Rows)
            {
                // Mapear por el número de columna para que coincida con el SELECT automático
                if (row.Cells[4].Value != null && row.Cells[4].Value != DBNull.Value)
                    totalApostado += Convert.ToDecimal(row.Cells[4].Value);

                if (row.Cells[5].Value != null && row.Cells[5].Value != DBNull.Value)
                    totalGanancia += Convert.ToDecimal(row.Cells[5].Value);
            }

            if (lblTotalPartidas != null) lblTotalPartidas.Text = "📊 Total Partidas: " + totalPartidas;
            if (lblTotalApostado != null) lblTotalApostado.Text = "💸 Total Apostado: $" + totalApostado.ToString("N2");
            if (lblTotalGanancia != null) lblTotalGanancia.Text = "📈 Total Ganancia: $" + totalGanancia.ToString("N2");
        }

        // ========================================================
        // 🏆 CALCULAR EL TOP GANADOR ESTADÍSTICO
        // ========================================================
        private void CalcularTopGanador()
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT TOP 1 j.Nombre, SUM(p.Ganancia) AS TotalGanancia
                        FROM Partida p
                        INNER JOIN Jugador j ON p.Id_Jugador = j.Id_Jugador
                        GROUP BY j.Nombre ORDER BY TotalGanancia DESC";

                    using (var cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read() && lblTopGanador != null)
                            {
                                lblTopGanador.Text = "🏆 TOP GANADOR: " + dr["Nombre"].ToString();
                            }
                        }
                    }
                }
            }
            catch { }
        }
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            CargarHistorialCompleto();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            // Validar de forma segura si el combobox tiene elementos antes de asignar el índice
            if (cmbJugadores.Items.Count > 0)
            {
                cmbJugadores.SelectedIndex = 0; // Si hay elementos, selecciona el primero
            }
            else
            {
                cmbJugadores.SelectedIndex = -1; // Si está vacío, lo deja limpio sin dar error
            }

            // Restablecer el rango de fechas al año 2026
            dtpDesde.Value = new DateTime(2026, 1, 1);
            dtpHasta.Value = DateTime.Now;

            CargarHistorialCompleto();
        }


        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ========================================================
        // 📥 BOTÓN EXPORTAR EXCEL CSV
        // ========================================================
        private void btnExportarCSV_Click(object sender, EventArgs e)
        {
            if (dgvResultados.Rows.Count == 0) return;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Archivo CSV (*.csv)|*.csv";
                sfd.FileName = "Historial_Partidas_Ruleta.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Fecha,Nombre,Eleccion,Resultado,Apuesta,Ganancia");

                        foreach (DataGridViewRow row in dgvResultados.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                string fecha = row.Cells[0].Value?.ToString() ?? "";
                                string nombre = row.Cells[1].Value?.ToString() ?? "";
                                string eleccion = row.Cells[2].Value?.ToString() ?? "";
                                string resultado = row.Cells[3].Value?.ToString() ?? "";
                                string apuesta = row.Cells[4].Value?.ToString() ?? "";
                                string ganancia = row.Cells[5].Value?.ToString() ?? "";

                                sb.AppendLine($"{fecha},{nombre},{eleccion},{resultado},{apuesta},{ganancia}");
                            }
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("¡Datos exportados correctamente! 📊", "Éxito");
                    }
                    catch { }
                }
            }
        }

        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
    }
}
