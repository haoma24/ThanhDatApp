using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThanhDatApp.BUS;

namespace ThanhDatApp.GUI.View
{
    public partial class frmTaiKhoan : Form
    {
        AccountService _accountService;
        private enum Mode { View, Add, Edit }
        private Mode currentMode = Mode.View;
        private thanhdatEntities db;

        public frmTaiKhoan()
        {
            InitializeComponent();
            db = new thanhdatEntities();
            _accountService = new AccountService();

            // Danh sách quyền với giá trị tượng trưng
            var rolesWithValues = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Quản lý", "ad"),
                new KeyValuePair<string, string>("Nhân viên", "emp"),
                new KeyValuePair<string, string>("Khách hàng", "cus")
            };
            // Gán danh sách vào ComboBox
            cbbQuyen.DataSource = rolesWithValues;
            cbbQuyen.DisplayMember = "Key"; // Hiển thị tên quyền
            cbbQuyen.ValueMember = "Value"; // Lưu giá trị tượng trưng
        }
        private void SetMode(Mode mode)
        {
            currentMode = mode;

            // Điều chỉnh trạng thái của các control
            txtMK.Enabled = txtMa.Enabled = mode == Mode.Add; // Chỉ cho phép nhập Mã KH khi thêm mới
            txtTen.Enabled =  cbbQuyen.Enabled = txtEmail.Enabled = mode != Mode.View;

            // Điều chỉnh trạng thái của các nút
            btnThem.Enabled = mode == Mode.View;
            btnSua.Enabled = mode == Mode.View && dgvEntity.CurrentRow != null;
            btnLuu.Enabled = mode != Mode.View;
            btnHuy.Enabled = mode != Mode.View; // Chỉ hiển thị nút Hủy khi ở chế độ Thêm hoặc Sửa
        }
        private void ClearTextBoxes()
        {
            txtMa.Text = txtTen.Text = txtMK.Text = cbbQuyen.Text = txtEmail.Text = txtMa.Text = string.Empty;
        }
        private void frmTaiKhoan_Load(object sender, EventArgs e)
        {
            LoadAccounts();
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

            var accountId = txtMa.Text;

            using (var db = new thanhdatEntities())
            {
                // Kiểm tra xem tài khoản có liên kết nhân viên hay không
                var hasEmployees = db.Employees.Any(x => x.AccountID == accountId);

                if (hasEmployees)
                {
                    var confirmResult = MessageBox.Show(
                        "Tài khoản này đang được sử dụng bởi nhân viên. Bạn có chắc chắn muốn xóa?",
                        "Xác nhận xóa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            //Xóa chi tiết trước

                            // Xóa nhân viên liên kết
                            var employeeToDelete = db.Employees.Where(o => o.AccountID == accountId).FirstOrDefault();
                            employeeToDelete.AccountID = "";
                            db.SaveChanges();

                            // Xóa tài khoản
                            var accountToDelete = db.Accounts.SingleOrDefault(c => c.AccountID == accountId);
                            if (accountToDelete != null)
                            {
                                db.Accounts.Remove(accountToDelete);
                            }

                            db.SaveChanges();

                            MessageBox.Show("Xóa tài khoản thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Làm mới DataGridView
                            LoadAccounts();
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
                        "Bạn có chắc chắn muốn xóa tài khoản này?",
                        "Xác nhận xóa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            // Xóa tài khoản trực tiếp
                            var accountToDelete = db.Accounts.SingleOrDefault(c => c.AccountID == accountId);
                            if (accountToDelete != null)
                            {
                                db.Accounts.Remove(accountToDelete);
                                db.SaveChanges();

                                MessageBox.Show("Tài khoản đã được xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Làm mới DataGridView
                                LoadAccounts();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Đã xảy ra lỗi khi xóa tài khoản.\nChi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentMode == Mode.Add)
                {
                    var newAccount = new Accounts
                    {
                        AccountID = txtMa.Text.Trim(),
                        UserName = txtTen.Text.Trim(),
                        PasswordHash = txtMK.Text.Trim(),
                        Role = cbbQuyen.SelectedValue.ToString(),
                        Email = txtEmail.Text.Trim()
                    };

                    db.Accounts.Add(newAccount);
                }
                else if (currentMode == Mode.Edit)
                {
                    var accountID = txtMa.Text.Trim();
                    var account = db.Accounts.FirstOrDefault(c => c.AccountID == accountID);

                    if (account != null)
                    {
                        account.UserName = txtTen.Text.Trim();
                        //account.PasswordHash = txtMK.Text.Trim();
                        account.Role = cbbQuyen.SelectedValue.ToString();
                        account.Email = txtEmail.Text.Trim();
                    }
                }

                db.SaveChanges();
                LoadAccounts();
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
            LoadAccounts();
        }
        private void LoadAccounts()
        {

            using (var db = new thanhdatEntities())
            {
                // Tải dữ liệu từ cơ sở dữ liệu
                var accounts = db.Accounts
                    .ToList() // Tải dữ liệu về danh sách cục bộ
                    .Select(c => new
                    {
                        c.AccountID,
                        c.UserName,
                        PasswordHash = HashPassword(c.PasswordHash), // Hash mật khẩu sau khi dữ liệu đã tải về
                        c.Email,
                        c.Role
                    })
                    .ToList();

                // Gán dữ liệu vào DataGridView
                dgvEntity.DataSource = null; // Xóa dữ liệu cũ
                dgvEntity.DataSource = accounts; // Gán dữ liệu mới
            }
        }
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        private void dgvEntity_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Kiểm tra dòng hợp lệ
            {
                var row = dgvEntity.Rows[e.RowIndex];
                txtMa.Text = row.Cells["AccountID"].Value?.ToString();
                txtTen.Text = row.Cells["UserName"].Value?.ToString();
                txtMK.Text = row.Cells["PasswordHash"].Value?.ToString();
                cbbQuyen.SelectedValue = row.Cells["Role"].Value?.ToString();
                txtEmail.Text = row.Cells["Email"].Value?.ToString();
                SetMode(Mode.View);
            }
        }
    }
}
