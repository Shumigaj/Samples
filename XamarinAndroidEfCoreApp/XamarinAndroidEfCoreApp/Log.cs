﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace XamarinAndroidEfCoreApp
{
    public class Log : CommonLog
    {
        public string Level { get; set; }
        public string Timestamp { get; set; }
        public string RenderedMessage { get; set; }
        public string Properties { get; set; }
    }

    public abstract class CommonLog
    {
        [Key]
        public int EntityId { get; set; }
        public string Exception { get; set; }
    }
}
