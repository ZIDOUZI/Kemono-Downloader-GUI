using System.Collections.Specialized;
using System.ComponentModel;

namespace Kemono.Models.Tree.Interface;

public interface ITreeCollection : ICollection<ITree>, INotifyCollectionChanged, INotifyPropertyChanged
{

}