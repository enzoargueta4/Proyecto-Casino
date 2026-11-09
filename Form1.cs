using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto_Casino
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Ruleta formulario = new Ruleta();
            formulario.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ControldeJugadores pantallaJugadores = new ControldeJugadores();
            pantallaJugadores.Show();
            this.Hide();
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnHistorial_Click(object sender, EventArgs e)
        {
            // Crear una instancia del formulario historial
            historial ventanaHistorial = new historial();

            // Mostrar el formulario de manera independiente
            ventanaHistorial.Show();

            // Opcional: Si quieres cerrar o abrirlo como diálogo modal, usa ventanaHistorial.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form2 autores= new Form2();
            autores.Show();

        }
    }

}