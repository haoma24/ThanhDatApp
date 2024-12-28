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
using ThanhDatApp.GUI;
using ThanhDatApp.GUI.View;

namespace ThanhDatApp
{
    public partial class Main : Form
    {
        private string _accountid;
        private string _empid;
        private string _branchid;
        AccountService _accountService;
        EmployeeService _employeeService;
        thanhdatEntities db = new thanhdatEntities();
        public Main(string accountid, string branchid)
        {
            InitializeComponent();
            _accountService = new AccountService();
            _employeeService = new EmployeeService();
            _accountid = accountid;

            _empid = db.Employees.Where(e=>e.AccountID==_accountid)
                .Select(e=>e.EmployeeID)
                .FirstOrDefault();
            _branchid = branchid;
        }

        private void btnBH_Click(object sender, EventArgs e)
        {
            frmBanHang frm = new frmBanHang(_empid,_branchid);
            frm.TopLevel = false;
            frm.Dock = DockStyle.Fill;

            pnlLayout.Controls.Clear();
            pnlLayout.Controls.Add(frm);
            frm.Show();
        }

        private void btnKH_Click(object sender, EventArgs e)
        {
            frmKhachHang frm = new frmKhachHang();
            frm.TopLevel = false;
            frm.Dock = DockStyle.Fill;
            frm.FormBorderStyle = FormBorderStyle.None;
            pnlLayout.Controls.Clear();
            pnlLayout.Controls.Add(frm);
            frm.Show();
        }

        private void btnNV_Click(object sender, EventArgs e)
        {
            frmNhanVien frm = new frmNhanVien();
            frm.TopLevel = false;
            frm.Dock = DockStyle.Fill;
            frm.FormBorderStyle = FormBorderStyle.None;
            pnlLayout.Controls.Clear();
            pnlLayout.Controls.Add(frm);
            frm.Show();
        }

        private void btnDH_Click(object sender, EventArgs e)
        {
            frmDonHang frm = new frmDonHang();
            frm.TopLevel = false;
            frm.Dock = DockStyle.Fill;
            frm.FormBorderStyle = FormBorderStyle.None;
            pnlLayout.Controls.Clear();
            pnlLayout.Controls.Add(frm);
            frm.Show();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            DialogResult rs = MessageBox.Show("Bạn có chắc chắn muốn thoát?", "Thoát chương trình",MessageBoxButtons.YesNo);
            if (rs == DialogResult.Yes)
            {
                this.Close();
                Login frm = new Login();
                frm.Show();
            }
            
        }

        private void btnSP_Click(object sender, EventArgs e)
        {
            frmSanPham frm = new frmSanPham();
            frm.TopLevel = false;
            frm.Dock = DockStyle.Fill;
            frm.FormBorderStyle = FormBorderStyle.None;
            pnlLayout.Controls.Clear();
            pnlLayout.Controls.Add(frm);
            frm.Show();
        }

        private void btnTK_Click(object sender, EventArgs e)
        {
            frmTaiKhoan frm = new frmTaiKhoan();
            frm.TopLevel = false;
            frm.Dock = DockStyle.Fill;
            frm.FormBorderStyle = FormBorderStyle.None;
            pnlLayout.Controls.Clear();
            pnlLayout.Controls.Add(frm);
            frm.Show();
        }
    }
}
