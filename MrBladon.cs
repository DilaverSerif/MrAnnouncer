using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterBot
{
    public class MrBladon
    {
        public class Work
        {
            public float toDOTime;
            public int toDOCount;
        }
        public class AutoWork
        {
            public SocailMediaUser user;
            public AutoWork(SocailMediaUser user)
            {
                this.user = user;
            }

            public List<Work> works = new List<Work>();
        }
    }
}