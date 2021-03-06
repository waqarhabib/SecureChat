﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Helpers.Specifications
{
    public class DefaultSpecification<T> : BaseSpecification<T>
    {
        public new void AddCriteria(IEnumerable<Criteria> criteria)
        {
            base.AddCriteria(criteria);
        }

        public new void AddOrderBy(IEnumerable<OrderByColumn> orderBy)
        {
            base.AddOrderBy(orderBy);
        }

        public new void AddPaging(int take, int skip)
        {
            base.AddPaging(take, skip);
        }
    }
}
