using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.Entities
{
    public enum WorkspaceStatus
    {
        Initialized,
        Reconstructed
    }

    public class ColMapWorkspace
    {
        public int Id { get; set; }
        public string FolderPath { get; set; }
        public int VideoId { get; set; }
        public virtual Video Video { get; set; }
        public WorkspaceStatus Status { get; set; } = WorkspaceStatus.Initialized;
    }
}
