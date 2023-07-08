using DevExpress.Utils.Serializing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQLDependency
{
    public partial class fmNhanVien : Form
    {
        private SqlConnection connection = new SqlConnection("Data Source=HIDO99;Initial Catalog = DATA_DEPENDENCY; User ID = sa; Password=123");
        private int position = 0;
        public fmNhanVien()
        {
            InitializeComponent();
        }

        private void fmNhanVien_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dS.NHANVIEN' table. You can move, or remove it, as needed.
            this.nHANVIENTableAdapter.Fill(this.dS.NHANVIEN);
            position = bdsNhanVien.Position;
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private int getId()
        {
            int id = -1;
            String cmd = "EXEC SP_GETID";
            SqlDataReader reader = ExecSqlDataReader(cmd);
            reader.Read();
            // id = Int32.Parse(reader.GetDecimal(0).ToString());
            id = reader.GetInt32(0);
            reader.Close();

            return id;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            bdsNhanVien.AddNew();
            //int id = getId();

            //tbMaNV.Text = id.ToString();
            tbMaNV.Enabled = false;
            position = bdsNhanVien.Position;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                this.nHANVIENTableAdapter.Fill(this.dS.NHANVIEN);
                bdsNhanVien.Position = position;
            }
            catch(TimeoutException to)
            {
                MessageBox.Show("Vui lòng chờ trong  giây lát\n", "Thông báo");
                btnRefresh.PerformClick();
            }
            catch(SqlException se)
            {
                MessageBox.Show("Vui lòng chờ trong  giây lát\n", "Thông báo");
                btnRefresh.PerformClick();
            }
        }

        private void nHANVIENGridControl_Click(object sender, EventArgs e)
        {
            position = bdsNhanVien.Position;
        }

        public SqlDataReader ExecSqlDataReader(String strLenh) // exec sp, select, view, truy vấn nhanh, chỉ dc xem, có nhiều dòng chỉ cho phép di xuống
        {
            SqlDataReader myreader;
            SqlCommand sqlcmd = new SqlCommand(strLenh, connection);
            sqlcmd.CommandType = CommandType.Text;
            sqlcmd.CommandTimeout = 0;
            if (connection.State == ConnectionState.Closed) connection.Open();
            try
            {
                myreader = sqlcmd.ExecuteReader(); return myreader;

            }
            catch (SqlException ex)
            {
                connection.Close();
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            try
            {
                String id = tbMaNV.Text.ToString();
                String ho = textEditHo.Text.ToString();
                String ten = textEditTen.Text.ToString();
                String phai = cmbPhai.Text.ToString();
                String diaChi = textEditDiaChi.Text.ToString();
                String ngaySinh = nGAYSINHDateEdit.Text.ToString();
                String luong = textEditLuong.Text.ToString();

                if(ho.Equals(String.Empty) || ten.Equals(String.Empty) || phai.Equals(String.Empty) ||
                    diaChi.Equals(String.Empty) || ngaySinh.Equals(String.Empty) || luong.Equals(String.Empty)){
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin", "Lưu ý");
                    return;
                }

                String cmd = String.Format("EXEC SP_MERGE @ID='{0}', @HO=N'{1}', @TEN=N'{2}', @PHAI=N'{3}', @DIACHI=N'{4}'," +
                    " @NGAYSINH='{5}', @LUONG={6}",
                    id, ho, ten, phai, diaChi, ngaySinh, luong);
                if (connection != null && connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                SqlCommand command = new SqlCommand(cmd, connection);
                command.CommandTimeout = 0;
                int numRows = command.ExecuteNonQuery();
                if (numRows > 0)
                {
                    MessageBox.Show("Ghi thành công!");
                    String newId = tbMaNV.Text.ToString();
                    if (!newId.Equals(id))
                    {
                        MessageBox.Show("Có sự thay đổi trên Id do một client khác!");
                    }
                    btnRefresh.PerformClick();
                }
                else MessageBox.Show("Ghi thất bại!");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Thông tin lỗi");
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if(bdsNhanVien.Count == 0)
            {
                btnXoa.Enabled = false;
                btnGhi.Enabled = true;
            }

            DialogResult userChoice = MessageBox.Show("Bạn có chắc chắc muốn xóa?", "Xác nhận", MessageBoxButtons.YesNo);
            if (userChoice == DialogResult.Yes)
            {
                String id = tbMaNV.Text.ToString();
                String ho = textEditHo.Text.ToString();
                String ten = textEditTen.Text.ToString();
                String phai = cmbPhai.Text.ToString();
                String diaChi = textEditDiaChi.Text.ToString();
                String ngaySinh = nGAYSINHDateEdit.Text.ToString();
                String luong = textEditLuong.Text.ToString();

                if (ho.Equals(String.Empty) || ten.Equals(String.Empty) || phai.Equals(String.Empty) ||
                    diaChi.Equals(String.Empty) || ngaySinh.Equals(String.Empty) || luong.Equals(String.Empty))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin", "Lưu ý");
                    return;
                }

                String cmd = String.Format("EXEC SP_DEL @ID={0}", id);
                if (connection != null && connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                SqlCommand command = new SqlCommand(cmd, connection);
                // command.CommandTimeout = 120;
                int numRows = command.ExecuteNonQuery();
                if (numRows > 0)
                {
                    MessageBox.Show("Xóa thành công!");
                    btnRefresh.PerformClick();
                }
                else MessageBox.Show("Xóa thất bại!");
            }
            else return;
        }

        private void btn_DelayRead_Click(object sender, EventArgs e)
        {
            try
            {
                String cmd = "EXEC SP_SELECT_DELAY";

                if (connection != null && connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                SqlCommand command = new SqlCommand(cmd, connection);
                command.CommandTimeout = 0;
                int numRows = command.ExecuteNonQuery();
                if (numRows <= 0)
                {
                    MessageBox.Show("SELECTED COMPLETED");
                    btnRefresh.PerformClick();
                }
                
            }
            catch (Exception) { }
        }
    }
}
