﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Matterhook.NET.Webhooks.Discourse
{
    public class Actions_Summary
    {
        public int id { get; set; }
        public int count { get; set; }
        public bool hidden { get; set; }
        public bool can_act { get; set; }
    }
}