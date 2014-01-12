using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Scada.About
{
    public partial class AboutForm : Form
    {
        private const int MaxFeatureCount = 100;

        public AboutForm()
        {
            InitializeComponent();
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            var features = GetFeatures(DateTime.Now);
        }

        private List<string> GetFeatures(DateTime date)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            String projectName = Assembly.GetExecutingAssembly().GetName().Name.ToString();

            Stream stream = null;
            foreach (var file in assembly.GetManifestResourceNames())
            {
                if (file.IndexOf("Features") > 0)
                {
                    stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(file);
                    break;
                }
            }

            if (stream != null)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(stream);

                    var entries = doc.SelectNodes("//feature");

                    foreach (var e in entries)
                    {

                    }
                       
                }
                catch (Exception e)
                {
                }
            }
            return null;
            
        }

        private string GetFeatureId(DateTime date, int index)
        {
            var y = date.Year - 2000;
            return string.Format("F{0}_{1:D2}_{2:d2}_{3:D2}", y, date.Month, date.Day, index);
        }

    }
}
