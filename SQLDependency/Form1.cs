using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Security.Permissions;

namespace SQLDependency
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        private int changeCount = 0;
        private const String statusMessage = "We have {0} changes";
        private const String tableName = "NHANVIEN";

        private DataSet dataToWatch = null;
        private SqlConnection connection = null;
        private SqlCommand command = null;
        
        //private DataTable dt = new DataTable();

        public Form1()
        {
            InitializeComponent();
            this.FormClosed += Form1_FormClosed;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            SqlDependency.Stop(getConnectionString());
            if (connection is object)
            {
                connection.Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dS.NHANVIEN' table. You can move, or remove it, as needed.
            // this.nHANVIENTableAdapter.Fill(this.dS.NHANVIEN);
            if (canRequestNotifications())
            {
                start();
            }
            else
            {
                MessageBox.Show("You have not activated Broker service");
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!CheckExists("fmNhanVien"))
            {
                Form fmNhanVien = new fmNhanVien();
                fmNhanVien.Show();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private Boolean CheckExists(String formName)
        {
            FormCollection fc = Application.OpenForms;
            foreach (Form f in fc)
                if (f.Name == formName)
                    return true;
            return false;
        }

        // Working with dependencies

        private Boolean canRequestNotifications()
        // In order to use the callback feature of the SqlDependency, the application must have
        // the SqlClientPermission permission.
        {
            try
            {
                SqlClientPermission pms = new SqlClientPermission(PermissionState.Unrestricted);
                pms.Demand();
                // Forces a SecurityException at run time if all callers higher in the call
                // stack have not been granted the permission specified by the current instance.
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private String getConnectionString()
        {
            return "Data Source=HIDO99;Initial Catalog = DATA_DEPENDENCY; User ID = sa; Password=123";
        }

        private String getCommand()
        {
            //return "SELECT [MANV],[HO],[TEN],[PHAI],[DIACHI],[NGAYSINH],[LUONG] " +
            //    "FROM[NV_CDCNPM].[dbo].[NHANVIEN]";

            return "select manv as [Mã NV],Ho as [Họ],Ten as [Tên],phai as [Phái],diachi as [Địa chỉ], ngaysinh as [Ngày sinh], luong as [Lương] from dbo.Nhanvien";
        }

        private void OnDependencyChange(object sender, SqlNotificationEventArgs e)
        {
           // MessageBox.Show("On change");
            // This event will occur on a thread pool thread.
            // It is illegal to update the UI from a worker thread
            // The following code checks to see if it is safe update the UI.
            ISynchronizeInvoke i = (ISynchronizeInvoke)this;

            //If InvokeRequired returns True, the code is executing on a worker thread.
            if (i.InvokeRequired)
            {
                // Create a delegate to perform the thread switch
                var tempDelegate = new OnChangeEventHandler(OnDependencyChange);
                var args = new object[] { sender, e };

                // Marshal the data from the worker thread
                // to the UI thread.
                i.BeginInvoke(tempDelegate, args);
                return;
            }

            // Remove the handler since it's only good
            // for a single notification
            SqlDependency dependency = (SqlDependency)sender;
            dependency.OnChange -= OnDependencyChange;

            // At this point, the code is executing on the
            // UI thread, so it is safe to update the UI.
            changeCount += 1;
            this.label1.Text = string.Format(statusMessage, changeCount);

            // Add information from the event arguments to the list box
            // for debugging purposes only.
            {
                var withBlock = this.listBox1.Items;
                withBlock.Clear();
                withBlock.Add("Info:   " + e.Info.ToString());
                withBlock.Add("Source: " + e.Source.ToString());
                withBlock.Add("Type:   " + e.Type.ToString());
            }

            // Reload the dataset that's bound to the grid.
            getData();
        }

        private void getData()
        {
            // Empty the dataset so that there is only
            // one batch worth of data displayed.
            dataToWatch.Clear();

            // Make sure the command object does not already have
            // a notification object associated with it.

            command.Notification = null;
            command.CommandTimeout = 0;
            // Create and bind the SqlDependency object
            // to the command object.

            var dependency = new SqlDependency(command);
            dependency.OnChange += OnDependencyChange;

            
            using (var adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(dataToWatch, tableName);
                this.dataGridView1.DataSource = dataToWatch;
                this.dataGridView1.DataMember = tableName;
            }

            
            //dt.Clear();
            //var dataReader = command.ExecuteReader();

            //dt.Load(dataReader);

            //this.dataGridView1.DataSource = dt;
        }

        private void start()
        {
            changeCount = 0;
            // Remove any existing dependency connection, then create a new one.
            SqlDependency.Stop(getConnectionString());
            try
            {
                SqlDependency.Start(getConnectionString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            if (connection is null)
            {
                connection = new SqlConnection(getConnectionString());
                connection.Open();
            }

            if (command is null)
            {
                // GetSQL is a local procedure that returns
                // a paramaterized SQL string. You might want
                // to use a stored procedure in your application.
                command = new SqlCommand(getCommand(), connection);
            }

            if (dataToWatch is null)
            {
                dataToWatch = new DataSet();
            }

            getData();
        }
    }
}