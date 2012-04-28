using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoHistory.Models
{
    public class CategoryModel
    {

        public virtual int? Id { get; set; }
        public virtual string Name { get; set; }


        public static int GetCount(){

            return 0;
        }

    }
}