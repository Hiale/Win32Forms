using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Hiale.Win32Forms.TestData
{
    public partial class MainForm : Form
    {
        private class ComboBoxItem
        {
            public Type Type { get; set; }

            public override string ToString()
            {
                return Type.FullName;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            foreach (var form in FindForms(Assembly.GetExecutingAssembly()))
            {
                cmbForms.Items.Add(new ComboBoxItem {Type = form});
            }
            cmbForms.SelectedIndex = 0;
        }


        public static IEnumerable<Type> FindForms(Assembly assembly)
        {
            return assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof (Form)));
        }

        private void btnShowForm_Click(object sender, EventArgs e)
        {
            var item = cmbForms.SelectedItem as ComboBoxItem;
            if (item == null)
                return;
            var form = (Form)Activator.CreateInstance(item.Type);
            form.ShowDialog();
        }
    }
}
