﻿using ChuongTrinhQuanLyQuanCafe.DAO;
using ChuongTrinhQuanLyQuanCafe.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChuongTrinhQuanLyQuanCafe
{
    public partial class fManagerTable : Form
    {
        private Account loginAccount;

        public Account LoginAccount
        {
            get { return loginAccount; }
            set { loginAccount = value; ChangeAccount(loginAccount.Type);}
        }
        public fManagerTable(Account acc)
        {
            InitializeComponent();
            this.LoginAccount = acc; 
            LoadTable();
            LoadCategory();
            LoadComboboxTable(cbSwitchTable);
            

        }
        #region Method
        void ChangeAccount(int type)
        {
            adminToolStripMenuItem.Enabled = type == 1;
            thôngTinCáNToolStripMenuItem.Text += " (" + LoginAccount.DisplayName + ")";
        }
        void LoadCategory()
        {
            List<Category> listCategory = CategoryDAO.Instance.GetListCategory();
            cbCategory.DataSource = listCategory;
            cbCategory.DisplayMember = "name";

        }
        void LoadFoodListByCategory(int id)
        {
            List<Food> listFood = FoodDAO.Instance.GetFoodByCategoryID(id);
            cbFood.DataSource = listFood;
            cbFood.DisplayMember = "name";
        }
        void LoadTable()
        {
            flpTable.Controls.Clear();
          List<Table> tableList =   TableDAO.Instance.LoadTableList();
          foreach (Table item in tableList)
          {
              Button btn = new Button() {Width = TableDAO.TableWidth, Height = TableDAO.TableHeight };
              btn.Click += btn_Click;
              btn.Tag = item;
              btn.Text = item.Name + Environment.NewLine + item.Status;
              switch (item.Status)
              {
                  case"Trống":
                      btn.BackColor = Color.Beige;
                      break;
                  default:
                      btn.BackColor = Color.Peru;
                      break;
              }
              flpTable.Controls.Add(btn);
          }
        }
        void ShowBill(int id)
        {
            lsvBill.Items.Clear();

            List<ChuongTrinhQuanLyQuanCafe.DTO.Menu> ListBillInfo = MenuDAO.Instance.GetListMenuByTable(id);
            float totalPrice = 0;
            foreach (ChuongTrinhQuanLyQuanCafe.DTO.Menu item in ListBillInfo)
            {
                ListViewItem lvItem = new ListViewItem(item.FoodName.ToString());
                lvItem.SubItems.Add(item.Count.ToString());
                lvItem.SubItems.Add(item.Price.ToString());
                lvItem.SubItems.Add(item.TotalPrice.ToString());
                totalPrice += item.TotalPrice;

                lsvBill.Items.Add(lvItem);
            }
            CultureInfo culture = new CultureInfo("vi-VN");
            //Thread.CurrentThread.CurrentCulture = cuture;
            tbTotalPrice.Text = totalPrice.ToString("c",culture);
        }
        void LoadComboboxTable(ComboBox cb)
        {
            cb.DataSource = TableDAO.Instance.LoadTableList();
            cb.DisplayMember = "Name";
        }
        #endregion

        #region Events
        private void btn_Click(object sender, EventArgs e)
        {
            int tableID = ((sender as Button ).Tag as Table).ID;
            lsvBill.Tag = (sender as Button).Tag;
            ShowBill(tableID);
        }
        private void đăngXuấtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void thôngTinCáNhânToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fAccountProfile f = new fAccountProfile(loginAccount);
            f.UpdateAccount +=f_UpdateAccount;
            f.ShowDialog();

        }

        private void f_UpdateAccount(object sender, AccountEvent e)
        {
            thôngTinCáNhânToolStripMenuItem.Text = "Thông Tin Tài Khoản(" + e.Acc.DisplayName + ")";
        }

        private void adminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fAdmin f = new fAdmin();
            f.loginAccount = LoginAccount;
            f.ShowDialog();
        }

        private void flpTable_Paint(object sender, PaintEventArgs e)
        {

        }
        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {

            int id = 0;
            ComboBox cb = sender as ComboBox;
            if (cb.SelectedItem == null)
                return;
            Category selected = cb.SelectedItem as Category;
            id = selected.ID;


            LoadFoodListByCategory(id);
        }

        private void btAddFood_Click(object sender, EventArgs e)
        {
            Table table = lsvBill.Tag as Table;
            int idBill = BillDAO.Instance.GetUncheckBillIDbyTableID(table.ID);
            int foodID = (cbFood.SelectedItem as Food).ID;
            int count = (int)nmFoodCount.Value;
            if (idBill == -1)
            {
                BillDAO.Instance.InsertBill(table.ID);
                BillInfoDAO.Instance.InsertBillInfo(BillDAO.Instance.GetMaxIDBill(), foodID, count);
            }
            else
            {
                BillInfoDAO.Instance.InsertBillInfo(idBill, foodID, count);
            }
            ShowBill(table.ID);
            LoadTable();
        }

        private void btCheckOut_Click(object sender, EventArgs e)
        {
            Table table = lsvBill.Tag as Table;
            int idBill = BillDAO.Instance.GetUncheckBillIDbyTableID(table.ID);
            int discount = (int)nmDisCount.Value;

            double totalPrice = Convert.ToDouble(tbTotalPrice.Text.Split(',')[0].Replace(".", ""));
            double finalTotalPrice = totalPrice - (totalPrice / 100) * discount;

            if (idBill != -1)
            {
                if (MessageBox.Show(string.Format("Bạn Có Muốn Thanh Toán Hóa Đơn Cho Bàn {0}\n Tổng Tiền - (Tổng Tiền / 100) x Giảm Giá\n => {1} - ({1}/100) x {2} = {3}", table.Name, totalPrice, discount, finalTotalPrice), "Thông Báo", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                {
                    BillDAO.Instance.CheckOut(idBill, discount, (float)finalTotalPrice);
                    ShowBill(table.ID);
                    LoadTable();
                }
            }

        }

        private void thôngTinCáNToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void fManagerTable_Load(object sender, EventArgs e)
        {

        }

        private void btSwitchTable_Click(object sender, EventArgs e)
        {
            int id1 = (lsvBill.Tag as Table).ID;
            int id2 = (cbSwitchTable.SelectedItem as Table).ID;
            if (MessageBox.Show(string.Format("Bạn có muốn chyển bàn {0} qua bàn {1}", (lsvBill.Tag as Table).Name, (cbSwitchTable.SelectedItem as Table).Name), "Thông Báo",MessageBoxButtons.OKCancel)==System.Windows.Forms.DialogResult.OK)
                TableDAO.Instance.SwitchTable(id1, id2);
            LoadTable();
        }
        #endregion


    }
}
