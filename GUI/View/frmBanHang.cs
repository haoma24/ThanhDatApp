using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThanhDatApp.BUS;
using ThanhDatApp.Helper;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ThanhDatApp.GUI.View
{
    public partial class frmBanHang : Form
    {
        private string url = "";
        private string _empid;
        private string _branchid;
        private ProductService _productService;
        private CustomerService _customerService;
        private PaymentService _paymentService;
        private int _tongtien;
        private thanhdatEntities _db;
        private string _orderid;
        public frmBanHang(string empid, string branchid)
        {
            InitializeComponent();
            _empid = empid;
            _orderid = "";
            _branchid = branchid;
            _productService = new ProductService();
            _customerService = new CustomerService();
            _paymentService = new PaymentService();
            _db = new thanhdatEntities();
            dgvSanPham.Columns.Add("ProductCode", "Mã sản phẩm");
            dgvSanPham.Columns.Add("ProductName", "Tên sản phẩm");
            dgvSanPham.Columns.Add("Quantity", "Số lượng");
            dgvSanPham.Columns.Add("Price", "Giá");

            //bind data to combobox
            cbbTenKH.DataSource = _customerService.Get(null);
            cbbTenKH.DisplayMember = "FullName";  // Hiển thị tên khách hàng
            cbbTenKH.ValueMember = "CustomerId";    // Giá trị ẩn là Id khách hàng
            cbbHTTT.DataSource = _paymentService.Get(null).OrderByDescending(p=>p.Payment1).ToList();
            cbbHTTT.DisplayMember = "Payment1";
            cbbHTTT.ValueMember = "PaymentID";
            
        }

        private void frmBanHang_Load(object sender, EventArgs e)
        {
            loadSanPham();
            LoadPrinters();
        }
        private void loadSanPham()
        {
            var sanpham = _productService.Get(null);

            foreach (var item in sanpham)
            {
                AddProductToPanel(item.ProductID,item.ProductName,1, string.Format("{0:C0}",item.UnitPrice), item.Image);
            }
        }
        public void LoadPrinters()
        {
            // Lấy danh sách máy in từ hệ thống
            foreach (string printerName in PrinterSettings.InstalledPrinters)
            {
                cbbPrinter.Items.Add(printerName);
            }

            // Chọn máy in mặc định
            var defaultPrinter = new PrinterSettings();
            cbbPrinter.SelectedItem = defaultPrinter.PrinterName;
        }
        private void AddProductToPanel(string id,string name,int quantity, string price, string imagePath)
        {
            // Tạo Panel cho mỗi sản phẩm
            Panel productPanel = new Panel();
            productPanel.Size = new Size(170, 220); // Kích thước mỗi thẻ
            productPanel.BorderStyle = BorderStyle.FixedSingle;

            // Thêm hình ảnh
            PictureBox pictureBox = new PictureBox();
            pictureBox.Image = Image.FromFile(imagePath.Substring(1).Replace(@"\",@"/"));
            pictureBox.Size = new Size(170, 120);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;

            // Thêm nhãn tên sản phẩm
            Label lblName = new Label();
            lblName.Text = name;
            lblName.TextAlign = ContentAlignment.MiddleCenter;
            lblName.Dock = DockStyle.Bottom;

            // Thêm nhãn giá
            Label lblPrice = new Label();
            lblPrice.Text = price;
            lblPrice.TextAlign = ContentAlignment.MiddleCenter;
            lblPrice.Dock = DockStyle.Bottom;

            // Thêm các control vào Panel
            productPanel.Controls.Add(pictureBox);
            productPanel.Controls.Add(lblPrice);
            productPanel.Controls.Add(lblName);
            productPanel.Click += (sender, e) => AddProductToGrid(id, name, quantity, price);
            // Thêm Panel vào FlowLayoutPanel
            flowLayoutPanel1.Controls.Add(productPanel);
        }
        // Giả sử bạn đã có DataGridView với tên 'dataGridViewProducts' đã được thêm vào Form của bạn.
        private void AddProductToGrid(string productCode, string productName, int quantity, string price)
        {
            // Kiểm tra nếu sản phẩm đã có trong DataGridView (dựa trên ProductCode)
            foreach (DataGridViewRow row in dgvSanPham.Rows)
            {
                // Kiểm tra null trước khi sử dụng .ToString()
                if (row.Cells["ProductCode"].Value != null && row.Cells["ProductCode"].Value.ToString() == productCode)
                {
                    // Nếu sản phẩm đã có, tăng số lượng
                    int currentQuantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                    row.Cells["Quantity"].Value = currentQuantity + 1;
                    setTongTien();
                    return;
                }
            }

            // Nếu sản phẩm chưa có, thêm dòng mới vào DataGridView
            dgvSanPham.Rows.Add(productCode, productName, 1, price);
            setTongTien();
        }
        public void setTongTien()
        {
            _tongtien = 0;
            if (dgvSanPham.Rows.Count==1)
            {
                _tongtien = 0;
            }
            foreach (DataGridViewRow row in dgvSanPham.Rows)
            {
                int SoLuong = Convert.ToInt32(row.Cells["Quantity"].Value);
                int DonGia = GetPriceFromCell(row.Cells["Price"]);
                _tongtien += SoLuong * DonGia;
            }
            lblTongTien.Text = "Tổng tiền: " + string.Format("{0:C0}", _tongtien);
        }
        private int GetPriceFromCell(DataGridViewCell cell)
        {
            if (cell.Value != null)
            {
                // Chuyển giá trị về dạng chuỗi
                string priceText = cell.Value.ToString();

                // Loại bỏ dấu chấm (.) và ký tự "đ"
                string cleanedPrice = priceText.Replace(".", "").Replace("₫", "").Trim();

                // Chuyển đổi thành số nguyên
                if (int.TryParse(cleanedPrice, out int price))
                {
                    return price; // Trả về giá trị số
                }
            }

            // Trả về 0 nếu không thể chuyển đổi
            return 0;
        }

        private void cbbTenKH_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbTenKH.SelectedItem is Customers kh)
            {
                txtSDT.Text = kh.Phone;
                txtEmail.Text = kh.Email;
                txtDiaChi.Text = kh.Address;
                if (kh.Sex == "Nam")
                {
                    rbtnNam.Checked = true;
                }
                else rbtnNu.Checked =true;
            }
        }


        private void dgvSanPham_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                foreach (DataGridViewRow row in dgvSanPham.SelectedRows)
                {
                    if (!row.IsNewRow) // Không cho phép xóa dòng trống mới
                    {
                        dgvSanPham.Rows.Remove(row);
                        setTongTien();
                    }
                }

                e.Handled = true; // Ngăn chặn hành vi mặc định của phím Delete
            }
        }

        private void btnThemKH_Click(object sender, EventArgs e)
        {
            frmKhachHang frm = new frmKhachHang();
            frm.ShowDialog();
        }

        private void btnThuTien_Click(object sender, EventArgs e)
        {
            DialogResult rs = MessageBox.Show("Xác nhận khách hàng đã thanh toán đủ số tiền", "Xác nhận thanh toán?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (rs == DialogResult.Yes)
            {
                if (_orderid == "")
                {
                    _orderid = IdIncrementer.Get("Orders");
                    var newOrder = new Orders
                    {
                        OrderID = _orderid,
                        CustomerID = cbbTenKH.SelectedValue.ToString(),
                        OrderDate = DateTime.Now,
                        EmployeeID = _empid,
                        DeliveryMethodID = "GHNTCH",
                        PaymentID = cbbHTTT.SelectedValue.ToString(),
                        Status = "Đã nhận",
                        BranchID = _branchid,
                        TotalAmount = _tongtien
                    };
                    _db.Orders.Add(newOrder);
                    _db.SaveChanges();
                    foreach (DataGridViewRow row in dgvSanPham.Rows)
                    {
                        if (!row.IsNewRow) // Bỏ qua dòng mới
                        {
                            // Lấy dữ liệu từ các cột
                            string productCode = row.Cells["ProductCode"].Value?.ToString();
                            int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                            double price = Convert.ToDouble(row.Cells["Price"].Value.ToString().Replace(".", "").Replace("₫", ""));
                            double total = quantity * price;

                            // Tạo một đối tượng OrderDetail
                            var detail = new OrderDetails
                            {
                                OrderID = _orderid,
                                ProductID = productCode,
                                Quantity = quantity,
                                UnitPrice = price,

                            };

                            // Thêm vào danh sách
                            _db.OrderDetails.Add(detail);
                        }
                    }
                }
                
                Clear();
                MessageBox.Show("Đặt hàng hoàn tất", "Thông báo!");
            }
            
                
        }
        public void Clear()
        {
            cbbHTTT.Text =
                cbbTenKH.Text =
                txtDiaChi.Text =
                txtEmail.Text =
                txtSDT.Text =
                cbbTenKH.Text = "";
            pbQRThanhToan.Image = null;
            foreach (DataGridViewRow row in dgvSanPham.Rows)
            {
                if (!row.IsNewRow) // Không cho phép xóa dòng trống mới
                {
                    dgvSanPham.Rows.Remove(row);
                    setTongTien();
                }
            }
        }
        private void btnInHoaDon_Click(object sender, EventArgs e)
        {
            _orderid = IdIncrementer.Get("Orders");
            var newOrder = new Orders
            {
                OrderID = _orderid,
                CustomerID = cbbTenKH.SelectedValue.ToString(),
                OrderDate = DateTime.Now,
                EmployeeID = _empid,
                DeliveryMethodID = "GHNTCH",
                PaymentID = cbbHTTT.SelectedValue.ToString(),
                Status = "Đã nhận",
                BranchID = _branchid,
                TotalAmount = _tongtien
            };
            _db.Orders.Add(newOrder);
            _db.SaveChanges();
            foreach (DataGridViewRow row in dgvSanPham.Rows)
            {
                if (!row.IsNewRow) // Bỏ qua dòng mới
                {
                    // Lấy dữ liệu từ các cột
                    string productCode = row.Cells["ProductCode"].Value?.ToString();
                    int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                    double price = Convert.ToDouble(row.Cells["Price"].Value.ToString().Replace(".", "").Replace("₫", ""));
                    double total = quantity * price;

                    // Tạo một đối tượng OrderDetail
                    var detail = new OrderDetails
                    {
                        OrderID = _orderid,
                        ProductID = productCode,
                        Quantity = quantity,
                        UnitPrice = price,
                        TotalPrice = total
                    };

                    // Thêm vào danh sách
                    _db.OrderDetails.Add(detail);
                    
                }
            }
            _db.SaveChanges();
            url = "https://localhost/BaoCao/Pages/ReportViewer.aspx?%2fDemoReport%2fPhieuTamTinh&OrderID="+_orderid+"&rs:Command=Render&rs:Format=PDF";
            string printerName = cbbPrinter.SelectedItem?.ToString(); // Tên máy in, ví dụ: "HP LaserJet 1100"
                                                                      // Tải file PDF
            string tempFilePath = Path.Combine(Path.GetTempPath(), "temp.pdf");
            using (var client = new WebClient())
            {
                client.UseDefaultCredentials = true;
                client.DownloadFile(url, tempFilePath);
            }

            // Gửi file PDF tới máy in
            PrintPDF(tempFilePath, printerName);

            // Xóa file tạm sau khi in xong
            File.Delete(tempFilePath);

        }
        private void PrintPDF(string filePath, string printerName)
        {
            ProcessStartInfo printProcessInfo = new ProcessStartInfo
            {
                Verb = "print",
                FileName = filePath,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $"/p /h \"{filePath}\" \"{printerName}\""
            };

            using (Process printProcess = new Process())
            {
                printProcess.StartInfo = printProcessInfo;
                printProcess.Start();
                printProcess.WaitForExit();
            }

            MessageBox.Show("In thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        


        static void PrintPdf(string filePath, string printerName)
        {
            try
            {
                // Sử dụng PdfiumViewer để in PDF
                using (var document = PdfDocument.Load(filePath))
                {
                    using (var printDocument = document.CreatePrintDocument())
                    {
                        printDocument.PrinterSettings.PrinterName = printerName;
                        printDocument.Print();
                    }
                }

                Console.WriteLine("In thành công.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi in: {ex.Message}");
            }
        }
        private void btnHuy_Click(object sender, EventArgs e)
        {

        }

        private async void cbbHTTT_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbHTTT.SelectedValue.ToString() == "TTCKVDT" && _tongtien>0)
            {
                pbQRThanhToan.Visible = true;
                string url = "https://img.vietqr.io/image/BIDV-3131488052-qr_only.png?amount=" + _tongtien;
                await LoadImageFromUrl(url, pbQRThanhToan);
            }
            else
            {
                pbQRThanhToan.Visible=false;
            }
            
        }
        private async Task LoadImageFromUrl(string imageUrl, PictureBox pictureBox)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Tải ảnh từ URL
                    byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);

                    // Chuyển byte[] thành Image
                    using (var ms = new MemoryStream(imageBytes))
                    {
                        pictureBox.Image = Image.FromStream(ms);
                        pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tải ảnh: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
