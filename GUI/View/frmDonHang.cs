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
    public partial class frmDonHang : Form
    {
        OrderService _orderService;
        PaymentService _paymentService;
        DeliveryMethodService _deliveryMethodService;
        CustomerService _customerService;
        private enum Mode { View, Edit }
        private Mode currentMode = Mode.View;
        private thanhdatEntities _db;
        public frmDonHang()
        {
            InitializeComponent();
            _orderService = new OrderService();
            _paymentService = new PaymentService();
            _deliveryMethodService = new DeliveryMethodService();
            _customerService = new CustomerService();
            _db = new thanhdatEntities();

            //Thêm dữ liệu vào cbb
            cbbHTTT.DataSource = _paymentService.Get(null);
            cbbHTTT.DisplayMember = "Payment1";
            cbbHTTT.ValueMember = "PaymentID";

            cbbHTVC.DataSource = _deliveryMethodService.Get(null);
            cbbHTVC.DisplayMember = "DeliveryName";
            cbbHTVC.ValueMember = "DeliveryMethodID";

            cbbKhachHang.DataSource = _customerService.Get(null);
            cbbKhachHang.DisplayMember = "FullName";  // Hiển thị tên khách hàng
            cbbKhachHang.ValueMember = "CustomerId";
        }
        private void frmDonHang_Load(object sender, EventArgs e)
        {
            LoadOrders();
            SetMode(Mode.View);

        }
        private void SetMode(Mode mode)
        {
            currentMode = mode;

            txtTinhTrang.Enabled
                =txtNgayDat.Enabled  
                = txtTongTien.Enabled 
                = cbbHTTT.Enabled=cbbHTVC.Enabled
                = cbbKhachHang.Enabled 
                = mode != Mode.View;

            btnSua.Enabled = mode == Mode.View && dgvEntity.CurrentRow != null;
            btnLuu.Enabled = mode != Mode.View;
            btnHuy.Enabled = mode != Mode.View; // Chỉ hiển thị nút Hủy khi ở chế độ Thêm hoặc Sửa
        }
        private void LoadOrders()
        {

            using (var db = new thanhdatEntities())
            {
                var orders = db.Orders
                    .Select(c => new
                    {
                        c.OrderID,
                        c.OrderDate,
                        c.TotalAmount,
                        c.Status,
                        c.DeliveryMethod.DeliveryName,
                        c.Payment.Payment1,
                        c.CustomerID,
                        c.PaymentID,
                        c.DeliveryMethodID
                    })
                    .ToList();

                dgvEntity.DataSource = null; // Xóa nguồn dữ liệu cũ
                dgvEntity.DataSource = orders; // Gán lại dữ liệu mới
                dgvEntity.Columns["CustomerID"].Visible = false;
                dgvEntity.Columns["PaymentID"].Visible = false;
                dgvEntity.Columns["DeliveryMethodID"].Visible = false;
            }
        }
        private void ClearTextBoxes()
        {
            txtMa.Text = 
                txtNgayDat.Text = 
                txtSDT.Text = 
                txtDiaChi.Text=
                txtEmail.Text=
                txtMa.Text=
                txtTinhTrang.Text=
                txtTongTien.Text
                = string.Empty;
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

            var orderId = txtMa.Text;

            using (var db = new thanhdatEntities())
            {   
                    var confirmResult = MessageBox.Show(
                        "Sau khi xóa, dữ liệu không thể khôi phục. Bạn có chắc chắn muốn xóa?",
                        "Xác nhận xóa",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            //Xóa chi tiết trước

                            // Xóa các đơn hàng liên kết trước
                            var ordersToDelete = db.Orders.Where(o => o.OrderID == orderId).ToList();
                            foreach (var o in ordersToDelete)
                            {
                                var orderdetailsToDelete = db.OrderDetails.Where(od => od.OrderID == o.OrderID).ToList();
                                db.OrderDetails.RemoveRange(orderdetailsToDelete);
                                db.Orders.Remove(o);
                            }


                            // Xóa đơn hàng
                            var orderToDelete = db.Orders.SingleOrDefault(c => c.OrderID == orderId);
                            if (orderToDelete != null)
                            {
                                db.Orders.Remove(orderToDelete);
                            }

                            db.SaveChanges();

                            MessageBox.Show("Đơn hàng đã được xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Làm mới DataGridView
                            LoadOrders();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Đã xảy ra lỗi khi xóa dữ liệu.\nChi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentMode == Mode.Edit)
                {
                    var orderID = txtMa.Text.Trim();
                    var order = _db.Orders.FirstOrDefault(c => c.OrderID == orderID);

                    if (order != null)
                    {
                        order.OrderDate = DateTime.Parse(txtNgayDat.Text.Trim());
                        order.Status = txtTinhTrang.Text.Trim();
                        order.Customers = cbbKhachHang.SelectedItem as Customers;
                    }
                }

                _db.SaveChanges();
                LoadOrders();
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
            LoadOrders();
        }

        private void dgvEntity_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Kiểm tra dòng hợp lệ
            {
                var row = dgvEntity.Rows[e.RowIndex];
                txtMa.Text = row.Cells["OrderID"].Value?.ToString();
                txtTinhTrang.Text = row.Cells["Status"].Value?.ToString();
                txtTongTien.Text = row.Cells["TotalAmount"].Value?.ToString();
                txtNgayDat.Text = row.Cells["OrderDate"].Value?.ToString();
                cbbKhachHang.SelectedValue = row.Cells["CustomerID"].Value.ToString();
                cbbHTTT.SelectedValue = row.Cells["PaymentID"].Value?.ToString() ?? string.Empty;
                cbbHTVC.SelectedValue = row.Cells["DeliveryMethodID"].Value?.ToString() ?? string.Empty;
                SetMode(Mode.View);
            }
        }

        private void cbbKhachHang_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbKhachHang.SelectedItem is Customers kh)
            {
                txtSDT.Text = kh.Phone;
                txtEmail.Text = kh.Email;
                txtDiaChi.Text = kh.Address;
                if (kh.Sex == "Nam")
                {
                    rbtnNam.Checked = true;
                }
                else rbtnNu.Checked = true;
            }
        }
    }
}
