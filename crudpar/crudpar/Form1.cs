using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;

namespace crudpar
{
    public partial class Form1 : Form
    {

        string connectionString = @"Server=.\sqlexpress;Database=ParcialProgramacion1;Trusted_Connection=True;";

        public string ConnectionString { get => connectionString; set => connectionString = value; }

        private int idSeleccionado = 0;

        public Form1()
        {
            InitializeComponent();

            btnGuardar.Click -= btnGuardar_Click;
            btnGuardar.Click += btnGuardar_Click;

            btnActualizar.Click -= btnActualizar_Click;
            btnActualizar.Click += btnActualizar_Click;

            btnEliminar.Click -= btnEliminar_Click;
            btnEliminar.Click += btnEliminar_Click;

            btnConsultar.Click -= btnConsultar_Click;
            btnConsultar.Click += btnConsultar_Click;

            btnLimpiar.Click -= btnLimpiar_Click;
            btnLimpiar.Click += btnLimpiar_Click;

            dataGridView1.CellClick -= dataGridView1_CellClick;
            dataGridView1.CellClick += dataGridView1_CellClick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConfigurarGrid();
            CargarProductos();
        }

        private void ConfigurarGrid()
        {
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.MultiSelect = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.RowHeadersVisible = false;
        }


        private void CargarProductos(string nombre = null, string categoria = null)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT IdProducto, Nombre, Categoria, Precio, Existencias
                      FROM dbo.Productos
                      WHERE (@Nombre IS NULL OR Nombre LIKE '%' + @Nombre + '%')
                        AND (@Categoria IS NULL OR Categoria LIKE '%' + @Categoria + '%')
                      ORDER BY IdProducto", con))
                {
                    cmd.Parameters.AddWithValue("@Nombre", string.IsNullOrWhiteSpace(nombre) ? (object)DBNull.Value : nombre.Trim());
                    cmd.Parameters.AddWithValue("@Categoria", string.IsNullOrWhiteSpace(categoria) ? (object)DBNull.Value : categoria.Trim());

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los productos:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarCampos(out decimal precio, out int existencias)
        {
            precio = 0;
            existencias = 0;

            if (string.IsNullOrWhiteSpace(txtProducto.Text))
            {
                MessageBox.Show("Ingrese el nombre del producto.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProducto.Focus();
                return false;
            }
            if (txtProducto.Text.Trim().Length > 100)
            {
                MessageBox.Show("El nombre no puede superar 100 caracteres.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProducto.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtCategoria.Text))
            {
                MessageBox.Show("Ingrese la categoría.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCategoria.Focus();
                return false;
            }
            if (txtCategoria.Text.Trim().Length > 50)
            {
                MessageBox.Show("La categoría no puede superar 50 caracteres.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCategoria.Focus();
                return false;
            }
            if (!decimal.TryParse(txtPrecio.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out precio) || precio < 0)
            {
                MessageBox.Show("Ingrese un precio válido (número mayor o igual a 0).", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrecio.Focus();
                return false;
            }
            if (!int.TryParse(txtExistencias.Text.Trim(), out existencias) || existencias < 0)
            {
                MessageBox.Show("Ingrese unas existencias válidas (entero mayor o igual a 0).", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtExistencias.Focus();
                return false;
            }
            return true;
        }


        private void AgregarParametros(SqlCommand cmd, decimal precio, int existencias)
        {
            cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 100).Value = txtProducto.Text.Trim();
            cmd.Parameters.Add("@Categoria", SqlDbType.NVarChar, 50).Value = txtCategoria.Text.Trim();

            SqlParameter pPrecio = cmd.Parameters.Add("@Precio", SqlDbType.Decimal);
            pPrecio.Precision = 10;
            pPrecio.Scale = 2;
            pPrecio.Value = precio;

            cmd.Parameters.Add("@Existencias", SqlDbType.Int).Value = existencias;
        }

     
        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarCampos(out decimal precio, out int existencias))
                return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO dbo.Productos (Nombre, Categoria, Precio, Existencias)
                      VALUES (@Nombre, @Categoria, @Precio, @Existencias)", con))
                {
                    AgregarParametros(cmd, precio, existencias);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Producto guardado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarCampos();
                CargarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el producto:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnActualizar_Click(object sender, EventArgs e)
        {
            if (idSeleccionado == 0)
            {
                MessageBox.Show("Seleccione un producto de la tabla para actualizar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidarCampos(out decimal precio, out int existencias))
                return;

            try
            {
                int filas;
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE dbo.Productos
                      SET Nombre = @Nombre,
                          Categoria = @Categoria,
                          Precio = @Precio,
                          Existencias = @Existencias
                      WHERE IdProducto = @Id", con))
                {
                    AgregarParametros(cmd, precio, existencias);
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = idSeleccionado;
                    con.Open();
                    filas = cmd.ExecuteNonQuery();
                }

                if (filas > 0)
                    MessageBox.Show("Producto actualizado correctamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("No se encontró el producto (pudo haber sido eliminado).", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                LimpiarCampos();
                CargarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar el producto:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (idSeleccionado == 0)
            {
                MessageBox.Show("Seleccione un producto de la tabla para eliminar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult r = MessageBox.Show(
                "¿Está seguro de eliminar el producto \"" + txtProducto.Text + "\"?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (r != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM dbo.Productos WHERE IdProducto = @Id", con))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = idSeleccionado;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Producto eliminado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarCampos();
                CargarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el producto:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            CargarProductos(txtProducto.Text, txtCategoria.Text);

            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("No se encontraron productos con ese criterio.", "Consulta",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
            CargarProductos();
        }

        private void LimpiarCampos()
        {
            txtProducto.Clear();
            txtCategoria.Clear();
            txtPrecio.Clear();
            txtExistencias.Clear();
            idSeleccionado = 0;
            dataGridView1.ClearSelection();
            txtProducto.Focus();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow fila = dataGridView1.Rows[e.RowIndex];

            idSeleccionado = Convert.ToInt32(fila.Cells["IdProducto"].Value);
            txtProducto.Text = fila.Cells["Nombre"].Value.ToString();
            txtCategoria.Text = fila.Cells["Categoria"].Value.ToString();
            txtPrecio.Text = Convert.ToDecimal(fila.Cells["Precio"].Value).ToString("0.00", CultureInfo.CurrentCulture);
            txtExistencias.Text = fila.Cells["Existencias"].Value.ToString();
        }

        private void label1_Click(object sender, EventArgs e) { }
        private void label1_Click_1(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void titleProducto_Click(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
        private void textBox3_TextChanged(object sender, EventArgs e) { }
        private void txtPrecio_TextChanged(object sender, EventArgs e) { }
        private void button4_Click(object sender, EventArgs e) { }
        private void button5_Click(object sender, EventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
    }
}