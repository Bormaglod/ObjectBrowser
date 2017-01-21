//-----------------------------------------------------------------------
// <copyright file="${FILENAME}" company="Sergey Teplyashin">
//     Copyright (c) 2010-2012 Sergey Teplyashin. All rights reserved.
// </copyright>
// <author>Тепляшин Сергей Васильевич</author>
// <email>sergey-teplyashin@yandex.ru</email>
// <license>
//     This program is free software; you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation; either version 3 of the License, or
//     (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </license>
// <date>16.04.2012</date>
// <time>8:17</time>
// <summary>Defines the ? class.</summary>
//-----------------------------------------------------------------------

namespace ObjectBrowser
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Db4objects.Db4o;
    using Db4objects.Db4o.Config;
    using Db4objects.Db4o.Linq;

    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : Form
    {
        private string fileName;
        private IObjectContainer data;
        
        public MainForm()
        {
            InitializeComponent();
            
            this.fileName = string.Empty;
        }
        
        private void Clear()
        {
            if (this.data != null)
            {
                this.data.Close();
                this.data = null;
                this.treeObjects.Nodes[0].Nodes.Clear();
                this.propertyGrid1.SelectedObject = null;
            }
        }
        
        private void OpenDatabase()
        {
            this.Clear();
            IEmbeddedConfiguration config = Db4oEmbedded.NewConfiguration();
            this.data = Db4oEmbedded.OpenFile(config, this.fileName);
            IEnumerable<object> objs = from object o in this.data select o;
            List<object> lll = objs.ToList();
            Dictionary<string, List<object>> d = new Dictionary<string, List<object>>();
            foreach (object obj in objs)
            {
                string key = obj.GetType().Name;
                if (!d.ContainsKey(key))
                {
                    d.Add(key, new List<object>());
                }
                
                List<object> l = d[key];
                l.Add(obj);
            }
            
            foreach (string key in d.Keys)
            {
                TreeNode node = this.treeObjects.Nodes[0].Nodes.Add(key);
                foreach (object v in d[key])
                {
                    TreeNode n = node.Nodes.Add(v.ToString());
                    n.Tag = v;
                    
                    Type tv = v.GetType();
                    IList vlist = v as IList;
                    if (vlist != null)
                    {
                        foreach (object lo in vlist)
                        {
                            TreeNode nlo;
                            if (lo != null)
                            {
                                nlo = n.Nodes.Add(lo.ToString());
                            }
                            else
                            {
                                nlo = n.Nodes.Add("null");
                            }
                            
                            nlo.Tag = lo;
                        }
                    }
                }
            }
        }
        
        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.fileName = this.openFileDialog1.FileName;
                this.OpenDatabase();
            }
        }
        
        private void TreeObjectsAfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                this.propertyGrid1.SelectedObject = e.Node.Tag;
            }
            else
            {
                this.propertyGrid1.SelectedObject = null;
            }
        }
        
        private void CloseToolStripMenuItemClick(object sender, EventArgs e)
        {
            Clear();
        }
        
        private void CreateToolStripMenuItemClick(object sender, EventArgs e)
        {
            IEmbeddedConfiguration config = Db4oEmbedded.NewConfiguration();
            this.fileName = "test.test";
            if (File.Exists(this.fileName))
            {
                File.Delete(this.fileName);
            }
            
            this.data = Db4oEmbedded.OpenFile(config, this.fileName);
            
            Test t = new Test(10);
            data.Store(t);
            
            /*TestRef r = new TestRef();
            data.Store(r);*/
            
            TestCollection c = new TestCollection();
            data.Store(c);
            
            this.data.Close();
            
            
            IEmbeddedConfiguration config2 = Db4oEmbedded.NewConfiguration();
            //config2.Common.ObjectClass(typeof(TestRef)).CascadeOnDelete(true);
            config2.Common.ObjectClass(typeof(TestCollection)).CascadeOnDelete(true);
            this.data = Db4oEmbedded.OpenFile(config2, this.fileName);
            
            IEnumerable<TestCollection> rc = from TestCollection tr in this.data select tr;
            /*TestRef ttt = rc.First();
            ttt.ValueTest = new Test(101);*/
            data.Delete(rc.First());
            /*data.Store(ttt);
            this.data.Close();*/
            
            OpenDatabase();
        }
    }
    
    public class Test
    {
        public int valueInt;
        
        public int ValueInt
        {
            get { return valueInt; }
            set { valueInt = value; }
        }
        
        public Test(int v)
        {
            valueInt = v;
        }
    }
    
    public class TestRef
    {
        public Test valueTest;
        
        public Test ValueTest
        {
            get { return valueTest; }
            set { valueTest = value; }
        }
        
        public TestRef()
        {
            valueTest = new Test(100);
        }
        
        public TestRef(int v)
        {
            valueTest = new Test(v);
        }
    }
    
    public class TestCollection
    {
        public List<TestRef> valueList;
        
        public List<TestRef> ValueList
        {
            get { return valueList; }
            set { valueList = value; }
        }
        
        public TestCollection()
        {
            valueList = new List<TestRef>();
            valueList.Add(new TestRef(1001));
            valueList.Add(new TestRef(1002));
        }
    }
}
