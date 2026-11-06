using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Proyecto_Casino
{
    public partial class ControldeJugadores : Form
    {
        private readonly string _connectionString = @"Server=DESKTOP-GDAOJOT\SQLEXPRESS;Database=DB_AZAR_USAM;Trusted_Connection=True;Encrypt=False;";
        private int _idJugadorSeleccionado = 0;

        public ControldeJugadores()
        {
            InitializeComponent();
        }

        private void ControldeJugadores_Load(object sender, EventArgs e)
        {
            RefrescarCatalogo();
        }

        private void RefrescarCatalogo()
        {
            try
            {
                dgvJugadores.DataSource = ObtenerListadoJugadores();
                ConfigurarEstilosTabla();
            }
            catch (Exception ex)
            {
                MostrarError($"Error al actualizar el listado: {ex.Message}");
            }
        }

        private void ConfigurarEstilosTabla()
        {
            if (dgvJugadores.DataSource == null) return;

            dgvJugadores.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvJugadores.EnableHeadersVisualStyles = false;
            dgvJugadores.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 34, 64);
            dgvJugadores.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvJugadores.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvJugadores.DefaultCellStyle.BackColor = Color.FromArgb(23, 42, 69);
            dgvJugadores.DefaultCellStyle.ForeColor = Color.White;

            if (dgvJugadores.Columns["ID"] != null) dgvJugadores.Columns["ID"].Width = 60;
        }

        // ========================================================
        // AQUÍ ESTÁ EL MÉTODO QUE BUSCABA EL DISEÑADOR VISUAL
        // ========================================================
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // Método vacío para solucionar el error del diseñador residual
        }

        private void dgvJugadores_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                DataGridViewRow fila = dgvJugadores.Rows[e.RowIndex];
                _idJugadorSeleccionado = Convert.ToInt32(fila.Cells["ID"].Value);
                txtNombre.Text = fila.Cells["Nombre"].Value.ToString();
                txtCarnet.Text = fila.Cells["Carnet"].Value.ToString();
                txtSaldo.Text = fila.Cells["Saldo"].Value.ToString();
            }
            catch (Exception ex)
            {
                MostrarError($"Error al seleccionar el registro: {ex.Message}");
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!ValidarCamposFormulario(out string mensajeError))
            {
                MostrarAdvertencia(mensajeError);
                return;
            }

            var nuevoJugador = MapearCamposAEntidad();

            try
            {
                EjecutarComandoInsert(nuevoJugador);
                MostrarInformacion("Jugador registrado exitosamente.");
                LimpiarFormulario();
                RefrescarCatalogo();
            }
            catch (Exception ex)
            {
                MostrarError($"No se pudo completar el registro: {ex.Message}");
            }
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (_idJugadorSeleccionado == 0)
            {
                MostrarAdvertencia("Seleccione un jugador del listado para modificar sus datos.");
                return;
            }

            if (!ValidarCamposFormulario(out string mensajeError))
            {
                MostrarAdvertencia(mensajeError);
                return;
            }

            var jugadorActualizado = MapearCamposAEntidad();
            jugadorActualizado.IdJugador = _idJugadorSeleccionado;

            try
            {
                EjecutarComandoUpdate(jugadorActualizado);
                MostrarInformacion("Los datos del jugador han sido actualizados.");
                LimpiarFormulario();
                RefrescarCatalogo();
            }
            catch (Exception ex)
            {
                MostrarError($"No se pudieron guardar las modificaciones: {ex.Message}");
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_idJugadorSeleccionado == 0)
            {
                MostrarAdvertencia("Seleccione el jugador que desea dar de baja.");
                return;
            }

            var confirmacion = MessageBox.Show("¿Está completamente seguro de eliminar este registro?", "Confirmar Acción", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmacion != DialogResult.Yes) return;

            try
            {
                EjecutarComandoDelete(_idJugadorSeleccionado);
                MostrarInformacion("El registro ha sido removido del sistema.");
                LimpiarFormulario();
                RefrescarCatalogo();
            }
            catch (Exception ex)
            {
                MostrarError($"El jugador no puede ser eliminado porque posee un historial de partidas activo.\nDetalle técnico: {ex.Message}");
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private DataTable ObtenerListadoJugadores()
        {
            string query = "SELECT Id_Jugador AS ID, Nombre, Carnet, Saldo, FechaRegistro AS [Fecha Registro] FROM Jugador ORDER BY Id_Jugador DESC";
            using (var con = new SqlConnection(_connectionString))
            using (var da = new SqlDataAdapter(query, con))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private void EjecutarComandoInsert(Jugador jugador)
        {
            string query = "INSERT INTO Jugador (Nombre, Carnet, Saldo) VALUES (@Nombre, @Carnet, @Saldo)";
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Nombre", jugador.Nombre);
                cmd.Parameters.AddWithValue("@Carnet", jugador.Carnet);
                cmd.Parameters.AddWithValue("@Saldo", jugador.Saldo);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void EjecutarComandoUpdate(Jugador jugador)
        {
            string query = "UPDATE Jugador SET Nombre = @Nombre, Carnet = @Carnet, Saldo = @Saldo WHERE Id_Jugador = @Id";
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Nombre", jugador.Nombre);
                cmd.Parameters.AddWithValue("@Carnet", jugador.Carnet);
                cmd.Parameters.AddWithValue("@Saldo", jugador.Saldo);
                cmd.Parameters.AddWithValue("@Id", jugador.IdJugador);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void EjecutarComandoDelete(int id)
        {
            string query = "DELETE FROM Jugador WHERE Id_Jugador = @Id";
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private bool ValidarCamposFormulario(out string mensaje)
        {
            mensaje = string.Empty;

            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                mensaje = "El campo 'Nombre' no puede estar vacío.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCarnet.Text))
            {
                mensaje = "El campo 'Carnet' es requerido para el registro académico.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtSaldo.Text) && !decimal.TryParse(txtSaldo.Text, out _))
            {
                mensaje = "El saldo inicial debe ser un valor numérico válido (Ejemplo: 500.00).";
                return false;
            }

            return true;
        }

        private Jugador MapearCamposAEntidad()
        {
            return new Jugador
            {
                Nombre = txtNombre.Text.Trim(),
                Carnet = txtCarnet.Text.Trim(),
                Saldo = string.IsNullOrWhiteSpace(txtSaldo.Text) ? 1000.00m : Convert.ToDecimal(txtSaldo.Text)
            };
        }

        private void LimpiarFormulario()
        {
            txtNombre.Clear();
            txtCarnet.Clear();
            txtSaldo.Clear();
            _idJugadorSeleccionado = 0;
            txtNombre.Focus();
        }

        private void MostrarInformacion(string mensaje) => MessageBox.Show(mensaje, "Operación Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void MostrarAdvertencia(string mensaje) => MessageBox.Show(mensaje, "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        private void MostrarError(string mensaje) => MessageBox.Show(mensaje, "Error del Sistema", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public class Jugador
    {
        public int IdJugador { get; set; }
        public string Nombre { get; set; }
        public string Carnet { get; set; }
        public decimal Saldo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
