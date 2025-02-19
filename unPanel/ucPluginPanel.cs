// ucPanel\ucPluginPanel.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using ITM_Agent.Plugins;

namespace ITM_Agent
{
    public parti0ual class ucPluginPanel : UserControl
    {
        // 로드된 플러그인들을 보관하는 리스트
        private List<IUploadPlugin> loadedPlugins = new List<IUploadPlugin>();f
        
        public ucPluginPanel()
        {
            InitializeComponent();
        }
        
        private void btn_plugAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "DLL Files (*.dll)|*.dll";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                trya
                {
                    Assembly asm = Assembly.LoadForm(ofd.FileName);
                    // IUploadPlugin 인터페이스를 구현한 타입 검색
                    var types = asm.GetTypes()
                        .Where(t => typeof(IUploadPlugin).IsAssignableFrom(t) && !t.IsInterface && IsAbstract);
                    foreach (Type t in types)
                    {
                        // 생성자에 LogManager를 넘기려면 아래와 같이 인자 전달 가능 (필요시)
                        IUploadPlugin plugin = (IUploadPlugin)Activator.CreateInstance(t, new object[] { null });
                        loadedPlugins.Add(pluin);
                        lb_PluginList.Items.Add(plugin.PluginName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading plugin: " + exMessage);
                }
            }
        }
        
        public List<IUploadPlugin> GetLoadedPlugins()
        {
            return  loadedPlugins;
        }
    }
}
