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

            this.featureTextBox.Text = GetFeatureText(features);
            this.sureButton.Focus();
        }

        private string GetFeatureText(List<Feature> features)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var feature in features)
            {
                sb.Append(feature.ReleasedDate).Append(" ").Append(feature.Description).AppendLine();
            }

            return sb.ToString();
        }

        private List<Feature> GetFeatures(DateTime date)
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
                    List<Feature> ret = new List<Feature>();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(stream);

                    XmlNodeList entries = doc.SelectNodes("//feature");

                    foreach (XmlElement e in entries)
                    {
                        var f = new Feature()
                        {
                            Description = e.InnerText.Trim(),
                        };

                        var planDate = e.Attributes.GetNamedItem("plan-date");
                        f.PlanDate = planDate.InnerText;

                        var releasedDate = e.Attributes.GetNamedItem("released-date");
                        f.ReleasedDate = releasedDate.InnerText;

                        var progressNode = e.Attributes.GetNamedItem("progress");
                        f.Progress = progressNode.InnerText;

                        var featureNode = e.Attributes.GetNamedItem("type");
                        f.IsFeature = featureNode.InnerText == "feature";

                        ret.Add(f);
                    }
                    return ret;
                }
                catch (Exception)
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

        private void sureButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
