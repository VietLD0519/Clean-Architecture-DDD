﻿using CA.Core.Domain.Persistence.Common;
using System.Collections.Generic;

namespace CA.Core.Domain.Persistence.Entities
{
    public class Tag : BaseEntity
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Content { get; set; }

        public virtual Tag ParentTag { get; set; }
        public virtual ICollection<Tag> ChildTags { get; set; }

        public virtual ICollection<PostTag> Posts { get; set; }
    }
}
