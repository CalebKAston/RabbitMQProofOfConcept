using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CacheService
{
	public partial class Form1 : Form
	{
		private CacheServiceManager cacheService;

		public Form1()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			cacheService = new CacheServiceManager();
			base.OnLoad(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			cacheService.Close();
			cacheService = null;
			base.OnClosing(e);
		}
	}
}
