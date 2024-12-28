using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThanhDatApp.BUS;

namespace ThanhDatApp.GUI.View
{
    public partial class frmSanPham : Form
    {
        CategoryService _categoryService = new CategoryService();
        private enum Mode { View, Add, Edit }
        private Mode currentMode = Mode.View;
        private thanhdatEntities db = new thanhdatEntities();
        public frmSanPham()
        {
            InitializeComponent();
        }
        private void SetMode(Mode mode)
        {
            currentMode = mode;

            // Điều chỉnh trạng thái của các control
            txtMa.Enabled = mode == Mode.Add; // Chỉ cho phép nhập Mã KH khi thêm mới
            txtTen.Enabled = btnTaiAnh.Enabled = txtGiaBan.Enabled = rbtnStill.Enabled = rbtnStop.Enabled = cbbDanhMuc.Enabled = txtMoTa.Enabled = mode != Mode.View;

            // Điều chỉnh trạng thái của các nút
            btnThem.Enabled = mode == Mode.View;
            btnSua.Enabled = mode == Mode.View && dgvEntity.CurrentRow != null;
            btnLuu.Enabled = mode != Mode.View;
            btnHuy.Enabled = mode != Mode.View; // Chỉ hiển thị nút Hủy khi ở chế độ Thêm hoặc Sửa
        }
        private void ClearTextBoxes()
        {
            txtMa.Text = txtTen.Text = txtMoTa.Text = txtGiaBan.Text = string.Empty;
        }
        private void frmSanPham_Load(object sender, EventArgs e)
        {
            cbbDanhMuc.DataSource = _categoryService.Get(null);
            cbbDanhMuc.DisplayMember = "CategoryName";
            cbbDanhMuc.ValueMember = "CategoryID";
            LoadProducts();
            SetMode(Mode.View);
        }
        private void LoadProducts()
        {

            using (var db = new thanhdatEntities())
            {
                var products = db.Products
                    .Select(c => new
                    {
                        c.ProductID,
                        c.ProductName,
                        c.Description,
                        c.Discontinued,
                        c.UnitPrice,
                        c.Image,
                        c.CategoryID
                    })
                    .ToList();

                dgvEntity.DataSource = null; // Xóa nguồn dữ liệu cũ
                dgvEntity.DataSource = products; // Gán lại dữ liệu mới
                dgvEntity.Columns["CategoryID"].Visible = false;
            }
        }
        private void dgvEntity_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Đảm bảo người dùng nhấn vào dòng dữ liệu
            {
                DataGridViewRow row = dgvEntity.Rows[e.RowIndex];

                // Gán giá trị vào TextBox
                txtMa.Text = row.Cells["ProductID"].Value?.ToString();
                txtTen.Text = row.Cells["ProductName"].Value?.ToString();
                txtGiaBan.Text = row.Cells["UnitPrice"].Value?.ToString();
                txtMoTa.Text = row.Cells["Description"].Value?.ToString();

                // Gán giá trị vào PictureBox (dùng đường dẫn)
                string imagePath = row.Cells["Image"].Value?.ToString();
                if (!string.IsNullOrEmpty(imagePath))
                {
                    if (imagePath.Contains("C:"))
                    {
                        pbAnh.Image = Image.FromFile(imagePath.Replace(@"\", @"/").Replace(@"\", ""));
                    }
                    else
                        pbAnh.Image = Image.FromFile(imagePath.Substring(1).Replace(@"\", @"/"));
                }
                else
                {
                    pbAnh.Image = null; // Nếu không có ảnh hoặc đường dẫn không tồn tại
                }

                // Gán giá trị vào RadioButton
                bool isDiscontinued = Convert.ToBoolean(row.Cells["Discontinued"].Value);
                rbtnStill.Checked = !isDiscontinued;
                rbtnStop.Checked = isDiscontinued;

                // Gán giá trị vào ComboBox
                cbbDanhMuc.SelectedValue = row.Cells["CategoryID"].Value;
            }
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

        private void btnHuy_Click(object sender, EventArgs e)
        {
            SetMode(Mode.View);
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void btnTaiAnh_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedImagePath = openFileDialog.FileName;

                    // Hiển thị ảnh trong PictureBox
                    pbAnh.Image = Image.FromFile(selectedImagePath);

                    // Lưu đường dẫn ảnh vào TextBox (hoặc gán thẳng vào cột "Image")
                    txtMa.Tag = selectedImagePath; // Dùng Tag để lưu tạm đường dẫn
                }
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            try
            {
                string productId = txtMa.Text.Trim();
                if (currentMode == Mode.Add)
                {
                    var newProduct = new Products
                    {
                        ProductID = productId,
                        ProductName = txtTen.Text,
                        Description = txtMoTa.Text,
                        UnitPrice = double.Parse(txtGiaBan.Text),
                        Discontinued = rbtnStop.Checked,
                        Image = txtMa.Tag?.ToString() ?? "", // Đường dẫn ảnh
                        CategoryID = (string)cbbDanhMuc.SelectedValue
                    };

                    db.Products.Add(newProduct);
                    db.SaveChanges();
                }
                else if (currentMode == Mode.Edit)
                {
                    using (var context = new thanhdatEntities())
                    {
                        // Lấy ProductID từ TextBox


                        // Tìm sản phẩm trong database
                        var product = context.Products.FirstOrDefault(p => p.ProductID == productId);

                        if (product != null) // Nếu sản phẩm đã tồn tại, cập nhật thông tin
                        {
                            product.ProductName = txtTen.Text;
                            product.Description = txtMoTa.Text;
                            product.UnitPrice = double.Parse(txtGiaBan.Text);
                            product.Discontinued = rbtnStop.Checked;
                            product.Image = txtMa.Tag?.ToString() ?? ""; // Đường dẫn ảnh
                            product.CategoryID = (string)cbbDanhMuc.SelectedValue;

                            // Lưu thay đổi
                            context.SaveChanges();
                        }
                    }
                }
                db.SaveChanges();
                SetMode(Mode.View);
                MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi");
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem có dòng nào được chọn hay không
            if (string.IsNullOrEmpty(txtMa.Text))
            {
                MessageBox.Show("Vui lòng chọn một dòng để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var productId = txtMa.Text;
            using (var db = new thanhdatEntities())
            {
                // Kiểm tra xem nhân viên có đơn hàng hay không
                var hasOrders = db.OrderDetails.Any(o => o.ProductID == productId);

                if (hasOrders)
                {
                    var confirmResult = MessageBox.Show(
                        "Sản phẩm này có đơn hàng liên kết. Nếu tiếp tục, tất cả đơn hàng của nhân viên sẽ bị xóa. Bạn có chắc chắn muốn xóa?",
                        "Xác nhận xóa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            //Xóa chi tiết trước

                            // Xóa các đơn hàng liên kết trước
                            var ordersToDelete = db.OrderDetails.Where(o => o.ProductID == productId).ToList();
                            db.OrderDetails.RemoveRange(ordersToDelete);


                            // Xóa nhân viên
                            var productToDelete = db.Products.SingleOrDefault(c => c.ProductID == productId);
                            if (productToDelete != null)
                            {
                                db.Products.Remove(productToDelete);
                            }

                            db.SaveChanges();

                            MessageBox.Show("Sản phẩm và các đơn hàng liên kết đã được xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Làm mới DataGridView
                            LoadProducts();
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
                        "Bạn có chắc chắn muốn xóa sản phẩm này?",
                        "Xác nhận xóa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            // Xóa nhân viên trực tiếp
                            var productToDelete = db.Products.SingleOrDefault(c => c.ProductID == productId);
                            if (productToDelete != null)
                            {
                                db.Products.Remove(productToDelete);
                                db.SaveChanges();

                                MessageBox.Show("Sản phẩm đã được xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Làm mới DataGridView
                                LoadProducts();
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
    }
}
