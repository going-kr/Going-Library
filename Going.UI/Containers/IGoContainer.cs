using Going.UI.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    public interface IGoContainer 
    {
        GoControlCollection Childrens { get; }
    }
}
