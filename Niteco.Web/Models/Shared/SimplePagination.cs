using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Niteco.Web.Models.Shared
{
    public class SimplePagination
    {
        public int PageSession { get; set; }

        public int TotalPage
        {
            get
            {
                int totalPage = TotalRecord / PageSession;
                if (TotalRecord % PageSession > 0)
                {
                    return totalPage + 1;
                }

                return totalPage;
            }
        }

        public int TotalRecord { get; set; }
    }
}