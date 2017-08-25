using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Niteco.ContentTypes.Blocks;
using Niteco.ContentTypes.Pages;

namespace Niteco.Web.Models.ViewModels
{
    public class ItemViewModel<T>
    {
        public int IndexNumber;
        public T CurrentBlock;
        public ItemViewModel(T currentBlock, int indexNumber)
        {
            IndexNumber = indexNumber;
            CurrentBlock = currentBlock;
        }
    }
}