﻿using CA.Core.Domain.Persistence.Common;
using System.Collections.Generic;

namespace CA.Core.Domain.Persistence.Entities
{
    public class Category : BaseEntity
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<Category> ChildCategories { get; set; }
        public virtual ICollection<PostCategory> Posts { get; set; }
    }
}
