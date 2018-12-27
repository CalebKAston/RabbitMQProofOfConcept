using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomerService
{
	public partial class Form1 : Form
	{
		private CustomerServiceManager customerService;
		public Form1()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			customerService = new CustomerServiceManager();
			base.OnLoad(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			customerService.Close();
			customerService = null;
			base.OnClosing(e);
		}
	}
}
