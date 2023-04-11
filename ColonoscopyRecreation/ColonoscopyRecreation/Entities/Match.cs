using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColonoscopyRecreation.Entities
{
    public class Match
    {
        public int Id { get; set; }
        public int KeyPoint1Id { get; set; }
        public virtual KeyPoint KeyPoint1 { get; set; }
        public int KeyPoint2Id { get; set; }
        public virtual KeyPoint KeyPoint2 { get; set; }
    }
}
