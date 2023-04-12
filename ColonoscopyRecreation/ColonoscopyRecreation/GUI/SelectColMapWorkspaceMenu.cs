using ColonoscopyRecreation.Database;
using ColonoscopyRecreation.Entities;
using Emgu.CV;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.GUI
{
    public class SelectColMapWorkspaceMenu : Menu
    {
        private string _sqlconnection = null!;
        private Func<int, bool> _condition = null!;
        public SelectColMapWorkspaceMenu(IConsoleContext parent, string title, Func<int, bool> condition, string sqlconnection = Program.SQLiteDatabasePath) 
            : base(parent, title, "Select workspace")
        {
            _sqlconnection = sqlconnection;
            _condition = condition;
        }

        public override T Display<T>()
        {
            using (var db = new DatabaseContext(_sqlconnection))
            {
                db.Database.EnsureCreated();
                this.Items.Clear();
                foreach (var w in db.ColMapWorkspaces.Select(w => new { w.Id, w.FolderPath }))
                {
                    bool condition_success = _condition == null ? true : _condition(w.Id);
                    if (condition_success)
                    {
                        this.AddItem($"{w.Id} {Path.GetFileName(w.FolderPath)}", () =>
                        {
                            base.ReturnValue = w.Id;
                            base.ExitEarly = true;
                        });
                    }
                }
            }
            return base.Display<T>();
        }
    }
}
