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
    public partial class frmNhanVien : Form
    {
        private EmployeeService _employeeService;
        private enum Mode { View, Add, Edit }
        private Mode currentMode = Mode.View;
        private thanhdatEntities db;
        public frmNhanVien()
        {
            InitializeComponent();
            _employeeService = new EmployeeService();
            db = new thanhdatEntities();
        }
        private void SetMode(Mode mode)
        {
            currentMode = mode;

            // Điều chỉnh trạng thái của các control
            txtMa.Enabled = mode == Mode.Add; // Chỉ cho phép nhập Mã KH khi thêm mới
            txtHoTen.Enabled = txtSDT.Enabled = txtDiaChi.Enabled = txtEmail.Enabled = mode != Mode.View;

            // Điều chỉnh trạng thái của các nút
            btnThem.Enabled = mode == Mode.View;
            btnSua.Enabled = mode == Mode.View && dgvEntity.CurrentRow != null;
            btnLuu.Enabled = mode != Mode.View;
            btnHuy.Enabled = mode != Mode.View; // Chỉ hiển thị nút Hủy khi ở chế độ Thêm hoặc Sửa
        }
        private void ClearTextBoxes()
        {
            txtMa.Text = txtHoTen.Text = txtSDT.Text = txtDiaChi.Text = string.Empty;
        }

        private void frmNhanVien_Load(object sender, EventArgs e)
        {
            LoadEmployees();
            SetMode(Mode.View);
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
            if (string.IsNullOrEmpty(txtMa.Text))
            {
                MessageBox.Show("Vui lòng chọn một dòng để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var employeeId = txtMa.Text;

            using (var db = new thanhdatEntities())
            {
                // Kiểm tra xem nhân viên có đơn hàng hay không
                var hasOrders = db.Orders.Any(o => o.EmployeeID == employeeId);

                if (hasOrders)
                {
                    var confirmResult = MessageBox.Show(
                        "Nhân viên này có đơn hàng liên kết. Nếu tiếp tục, tất cả đơn hàng của nhân viên sẽ bị xóa. Bạn có chắc chắn muốn xóa?",
                        "Xác nhận xóa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            //Xóa chi tiết trước

                            // Xóa các đơn hàng liên kết trước
                            var ordersToDelete = db.Orders.Where(o => o.EmployeeID == employeeId).ToList();
                            foreach (var o in ordersToDelete)
                            {
                                var orderdetailsToDelete = db.OrderDetails.Where(od => od.OrderID == o.OrderID).ToList();
                                db.OrderDetails.RemoveRange(orderdetailsToDelete);
                                db.Orders.Remove(o);
                            }


                            // Xóa nhân viên
                            var employeeToDelete = db.Employees.SingleOrDefault(c => c.EmployeeID == employeeId);
                            if (employeeToDelete != null)
                            {
                                db.Employees.Remove(employeeToDelete);
                            }

                            db.SaveChanges();

                            MessageBox.Show("Nhân viên và các đơn hàng liên kết đã được xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Làm mới DataGridView
                            LoadEmployees();
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
                        "Bạn có chắc chắn muốn xóa nhân viên này?",
                        "Xác nhận xóa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            // Xóa nhân viên trực tiếp
                            var employeeToDelete = db.Employees.SingleOrDefault(c => c.EmployeeID == employeeId);
                            if (employeeToDelete != null)
                            {
                                db.Employees.Remove(employeeToDelete);
                                db.SaveChanges();

                                MessageBox.Show("Nhân viên đã được xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Làm mới DataGridView
                                LoadEmployees();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Đã xảy ra lỗi khi xóa nhân viên.\nChi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    var newEmployee = new Employees
                    {
                        EmployeeID = txtMa.Text.Trim(),
                        FullName = txtHoTen.Text.Trim(),
                        Phone = txtSDT.Text.Trim(),
                        Address = txtDiaChi.Text.Trim(),
                        Email = txtEmail.Text.Trim(),
                        Sex = _Sex
                    };

                    db.Employees.Add(newEmployee);
                }
                else if (currentMode == Mode.Edit)
                {
                    var employeeID = txtMa.Text.Trim();
                    var employee = db.Employees.FirstOrDefault(c => c.EmployeeID == employeeID);

                    if (employee != null)
                    {
                        employee.FullName = txtHoTen.Text.Trim();
                        employee.Phone = txtSDT.Text.Trim();
                        employee.Address = txtDiaChi.Text.Trim();
                        employee.Email = txtEmail.Text.Trim();
                        employee.Sex = _Sex;
                    }
                }

                db.SaveChanges();
                LoadEmployees();
                SetMode(Mode.View);
                MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            LoadEmployees();
        }

        private void dgvEntity_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Kiểm tra dòng hợp lệ
            {
                var row = dgvEntity.Rows[e.RowIndex];
                txtMa.Text = row.Cells["EmployeeID"].Value?.ToString();
                txtHoTen.Text = row.Cells["FullName"].Value?.ToString();
                txtSDT.Text = row.Cells["Phone"].Value?.ToString();
                txtDiaChi.Text = row.Cells["Address"].Value?.ToString();
                txtEmail.Text = row.Cells["Email"].Value?.ToString();
                if (row.Cells["Sex"].Value?.ToString() == "Nam")
                {
                    rbtnNam.Checked = true;
                }
                else rbtnNu.Checked = true;
                SetMode(Mode.View);
            }
        }
        private void LoadEmployees()
        {

            using (var db = new thanhdatEntities())
            {
                var employees = db.Employees
                    .Select(c => new
                    {
                        c.EmployeeID,
                        c.FullName,
                        c.Phone,
                        c.Email,
                        c.Address,
                        c.Sex
                    })
                    .ToList();

                dgvEntity.DataSource = null; // Xóa nguồn dữ liệu cũ
                dgvEntity.DataSource = employees; // Gán lại dữ liệu mới
            }
        }
    }

}
