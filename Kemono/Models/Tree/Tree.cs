using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Kemono.Core.Models;
using Kemono.Models.Tree.Interface;

namespace Kemono.Models.Tree;

public abstract class Tree : ITree
{
    public virtual ICollection<ITree> Children
    {
        get;
    }

    public bool HaveChildren
    {
        get;
    }

    public event Action FreshNodesEvent;

    public void FreshNodes() => throw new NotImplementedException();

    public Tree()
    {
        FreshNodesEvent += FreshChild;
    }

    private void FreshChild()
    {
        if (HaveChildren)
        {
            Children.ForEach(tree => tree.FreshNodes());
        }
    }
}