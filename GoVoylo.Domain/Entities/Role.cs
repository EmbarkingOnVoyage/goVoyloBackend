using System;
using System.Collections.Generic;
using System.Text;

namespace GoVoylo.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; } = null!;
    }
}
