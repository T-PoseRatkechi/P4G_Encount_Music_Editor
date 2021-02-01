using System;
using System.Collections.Generic;
using System.Text;

namespace P4G_Encount_Music_Editor
{
    interface IMenuOption
    {
        string Name { get; }
        void Run(GameProps game);
    }
}
