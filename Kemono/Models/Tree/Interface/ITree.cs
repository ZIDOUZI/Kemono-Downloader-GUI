using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Kemono.Models.Tree.Interface;

public interface ITree
{
    public ICollection<ITree> Children
    {
        get;
    }

    public bool HaveChildren
    {
        get;
    }

    event Action FreshNodesEvent;

    void FreshNodes();

}

