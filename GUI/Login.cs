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

namespace ThanhDatApp.GUI
{
    public partial class Login : Form
    {
        string _TK;
        string _MK;
        AccountService _accountService;
        BranchService _branchService;
        public Login()
        {
            InitializeComponent();
            _accountService = new AccountService();
            _branchService = new BranchService();
            cbbChiNhanh.DataSource = _branchService.Get(null);
            cbbChiNhanh.DisplayMember = "BranchName";
            cbbChiNhanh.ValueMember = "BranchID";
        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            _TK = txtTK.Text;
            _MK = txtMK.Text;
            if (_accountService.isExist(_TK, _MK))
            {
                Main main = new Main(_accountService.GetId(_TK, _MK),cbbChiNhanh.SelectedValue.ToString());
                main.Show();

            }
            else
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
