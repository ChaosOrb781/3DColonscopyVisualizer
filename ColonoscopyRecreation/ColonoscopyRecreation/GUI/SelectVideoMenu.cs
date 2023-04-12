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
    public class SelectVideoMenu : Menu
    {
        private string _sqlconnection = null!;
        private Func<int, bool> _condition = null!;
        public SelectVideoMenu(IConsoleContext parent, string title, Func<int, bool> condition, string sqlconnection = Program.SQLiteDatabasePath) 
            : base(parent, title, "Select video")
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
                foreach (var v in db.Videos.Select(v => new { v.Id, v.VideoFilepath }))
                {
                    int count = db.Frames.Count(f => f.VideoId == v.Id);
                    bool condition_success = _condition == null ? true : _condition(v.Id);
                    if (condition_success)
                    {
                        this.AddItem($"{v.Id} {Path.GetFileName(v.VideoFilepath)} ({count} frames)", () =>
                        {
                            base.ReturnValue = v.Id;
                            base.ExitEarly = true;
                        });
                    }
                }
            }
            return base.Display<T>();
        }
    }
}
