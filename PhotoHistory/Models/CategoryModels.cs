using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhotoHistory.Data;

namespace PhotoHistory.Models
{
	public class CategoryModel : AbstractDataModel<CategoryModel>
    {
        public virtual string Name { get; set; }

    
    }
}