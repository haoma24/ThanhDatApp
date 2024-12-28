using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThanhDatApp.BUS;

namespace ThanhDatApp.GUI.View
{
    public partial class frmKhachHang : Form
    {
        private CustomerService _customerService;
        private enum Mode { View, Add, Edit }
        private Mode currentMode = Mode.View;
        private thanhdatEntities db;
        public frmKhachHang()
        {
            InitializeComponent();
            _customerService = new CustomerService();
            db= new thanhdatEntities();
        }
        private void frmKhachHang_Load(object sender, EventArgs e)
        {
            LoadCustomers();
            SetMode(Mode.View);
        }
        private void SetMode(Mode mode)
        {
            currentMode = mode;

            // Điều chỉnh trạng thái của các control
            txtMaKH.Enabled = mode == Mode.Add; // Chỉ cho phép nhập Mã KH khi thêm mới
            txtHoTen.Enabled = txtSDT.Enabled = txtDiaChi.Enabled = txtEmail.Enabled = mode != Mode.View;

            // Điều chỉnh trạng thái của các nút
            btnThem.Enabled = mode == Mode.View;
            btnSua.Enabled = mode == Mode.View && dgvKhachHang.CurrentRow != null;
            btnLuu.Enabled = mode != Mode.View;
            btnHuy.Enabled = mode != Mode.View; // Chỉ hiển thị nút Hủy khi ở chế độ Thêm hoặc Sửa
        }
        private void LoadCustomers()
        {

            using (var db = new thanhdatEntities())
            {
                var customers = db.Customers
                    .Select(c => new
                    {
                        c.CustomerID,
                        c.FullName,
                        c.Phone,
                        c.Email,
                        c.Address,
                        c.Sex
                    })
                    .ToList();

                dgvKhachHang.DataSource = null; // Xóa nguồn dữ liệu cũ
                dgvKhachHang.DataSource = customers; // Gán lại dữ liệu mới
            }
        }
        private void ClearTextBoxes()
        {
            txtMaKH.Text = txtHoTen.Text = txtSDT.Text = txtDiaChi.Text = string.Empty;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            ClearTextBoxes();
            SetMode(Mode.Add);
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            SetMode(Mode.Edit);
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem có dòng nào được chọn hay không
            if (string.IsNullOrEmpty(txtMaKH.Text))
            {
                MessageBox.Show("Vui lòng chọn một dòng để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var customerId = txtMaKH.Text;

            using (var db = new thanhdatEntities())
            {
                // Kiểm tra xem khách hàng có đơn hàng hay không
                var hasOrders = db.Orders.Any(o => o.CustomerID == customerId);

                if (hasOrders)
                {
                    var confirmResult = MessageBox.Show(
                        "Khách hàng này có đơn hàng liên kết. Nếu tiếp tục, tất cả đơn hàng của khách hàng sẽ bị xóa. Bạn có chắc chắn muốn xóa?",
                        "Xác nhận xóa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            //Xóa chi tiết trước

                            // Xóa các đơn hàng liên kết trước
                            var ordersToDelete = db.Orders.Where(o => o.CustomerID == customerId).ToList();
                            foreach (var o in ordersToDelete)
                            {
                                var orderdetailsToDelete = db.OrderDetails.Where(od => od.OrderID == o.OrderID).ToList();
                                db.OrderDetails.RemoveRange(orderdetailsToDelete);
                                db.Orders.Remove(o);
                            }
                            

                            // Xóa khách hàng
                            var customerToDelete = db.Customers.SingleOrDefault(c => c.CustomerID == customerId);
                            if (customerToDelete != null)
                            {
                                db.Customers.Remove(customerToDelete);
                            }

                            db.SaveChanges();

                            MessageBox.Show("Khách hàng và các đơn hàng liên kết đã được xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Làm mới DataGridView
                            LoadCustomers();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Đã xảy ra lỗi khi xóa dữ liệu.\nChi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    var confirmResult = MessageBox.Show(
                        "Bạn có chắc chắn muốn xóa khách hàng này?",
                        "Xác nhận xóa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            // Xóa khách hàng trực tiếp
                            var customerToDelete = db.Customers.SingleOrDefault(c => c.CustomerID == customerId);
                            if (customerToDelete != null)
                            {
                                db.Customers.Remove(customerToDelete);
                                db.SaveChanges();

                                MessageBox.Show("Khách hàng đã được xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Làm mới DataGridView
                                LoadCustomers();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Đã xảy ra lỗi khi xóa khách hàng.\nChi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            try
            {
                var _Sex = "Nữ";
                if (rbtnNam.Checked)
                {
                    _Sex = "Nam";
                }
                if (currentMode == Mode.Add)
                {
                    var newCustomer = new Customers
                    {
                        CustomerID = txtMaKH.Text.Trim(),
                        FullName = txtHoTen.Text.Trim(),
                        Phone = txtSDT.Text.Trim(),
                        Address = txtDiaChi.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        Sex = _Sex
                    };

                    db.Customers.Add(newCustomer);
                }
                else if (currentMode == Mode.Edit)
                {
                    var customerID = txtMaKH.Text.Trim();
                    var customer = db.Customers.FirstOrDefault(c => c.CustomerID == customerID);

                    if (customer != null)
                    {
                        customer.FullName = txtHoTen.Text.Trim();
                        customer.Phone = txtSDT.Text.Trim();
                        customer.Address = txtDiaChi.Text.Trim();
                        customer.Email = txtEmail.Text.Trim();
                        customer.Sex = _Sex;
                    }
                }

                db.SaveChanges();
                LoadCustomers();
                SetMode(Mode.View);
                MessageBox.Show("Lưu thành công!", "Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi");
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            SetMode(Mode.View);
        }

        private void dgvKhachHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Kiểm tra dòng hợp lệ
            {
                var row = dgvKhachHang.Rows[e.RowIndex];
                txtMaKH.Text = row.Cells["CustomerID"].Value?.ToString();
                txtHoTen.Text = row.Cells["FullName"].Value?.ToString();
                txtSDT.Text = row.Cells["Phone"].Value?.ToString();
                txtDiaChi.Text = row.Cells["Address"].Value?.ToString();
                txtEmail.Text = row.Cells["Email"].Value?.ToString();
                if (row.Cells["Sex"].Value?.ToString() == "Nam")
                {
                    rbtnNam.Checked = true;
                }else rbtnNu.Checked = true;
                SetMode(Mode.View);
            }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            LoadCustomers();
        }
    }

}
