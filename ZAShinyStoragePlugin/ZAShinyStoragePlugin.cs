using PKHeX.Core;
using ZAShinyStoragePlugin.Properties;

namespace ZAShinyStoragePlugin
{
    public class ZAShinyStoragePlugin : IPlugin
    {
        public string Name => "ZA Shiny Storage Manager";
        public int Priority => 1;
        public static Form MainWindow { get; private set; } = null!;
        public ISaveFileProvider SaveFileEditor { get; private set; } = null!;
        public IPKMView PKMEditor { get; private set; } = null!;
        private bool IsCompatibleSave => SaveFileEditor.SAV is SAV9ZA && SaveFileEditor.SAV.State.Exportable;
        public virtual void NotifySaveLoaded() => MenuItem!.Visible = IsCompatibleSave;
        public bool TryLoadFile(string filePath) => false;
        private ToolStripMenuItem? MenuItem { get; set; }

        public void Initialize(params object[] args)
        {
            SaveFileEditor = (ISaveFileProvider)Array.Find(args, z => z is ISaveFileProvider)!;
            PKMEditor = (IPKMView)Array.Find(args, z => z is IPKMView)!;
            MainWindow = (Form)Array.Find(args, z => z is Form)!;
            var menu = (ToolStrip)Array.Find(args, z => z is ToolStrip)!;
            LoadMenuStrip(menu);
        }

        private void LoadMenuStrip(ToolStrip menuStrip)
        {
            var items = menuStrip.Items;
            if (items.Find("Menu_Tools", false)[0] is not ToolStripDropDownItem tools)
                throw new ArgumentException(null, nameof(menuStrip));
            AddPluginControl(tools);
        }

        private void AddPluginControl(ToolStripDropDownItem tools)
        {
            MenuItem = new ToolStripMenuItem(Name)
            {
                Image = Resources.shiny,
            };
            MenuItem.Click += (_, _) => OpenStoredShinyForm();
            tools.DropDownItems.Add(MenuItem);
        }

        private void OpenStoredShinyForm()
        {
            var form = new ShinyStorageForm((SAV9ZA)SaveFileEditor.SAV);
            form.LoadPokemon();
            form.ShowDialog();
        }
    }
}